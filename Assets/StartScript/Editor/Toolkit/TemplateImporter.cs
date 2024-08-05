using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Callbacks;
using UnityEngine;

namespace StartScript
{
    [ScriptedImporter(0, FILE_EXT)]
    public class TemplateImporter : ScriptedImporter
    {
        const string FILE_EXT = ".sstp";
        const string DEFAULT_TEMPLATE = @"<Header
    Name = """",
    SDesc = """",
    LDesc = """",
    
    Langs = """",
    FEx = """",
    Flags = """",
>


";

        #region PARAMS
        [SerializeField]
        bool import;
        #endregion

        #region CreateAssetMenu

        static LazyMethod getActiveFolderPath =
            LazyMethod.From<ProjectWindowUtil>("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

        static string GetPathToProjectWindowFolder()
        {
            object obj = getActiveFolderPath.Invoke(null);
            string pathToCurrentFolder = obj.ToString();

            return pathToCurrentFolder;
        }

        [MenuItem("Assets/Create/StartScript Template", priority = 5000)]
        static void CreateSSTPFile()
        {
            var path = GetPathToProjectWindowFolder();
            path += "/new-template";
            if (File.Exists(path + FILE_EXT))
            {
                int num = 0;
                while (File.Exists(path + num.ToString() + FILE_EXT))
                    num++;
                path += num;
            }
            path += FILE_EXT;

            using (File.CreateText(path))
            {

            }
        }
        #endregion

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (import)
            {
                var parse = new Parser();
                parse.Parse(File.ReadAllText(ctx.assetPath));

                var build = new StringBuilder(64);

                parse.PrintErrors(Print);

                Debug.Log(build.ToString());

                void Print(LogType type, string msg)
                {
                    switch (type)
                    {
                        case LogType.Message:   build.Append("   [MSG] " + msg); break;
                        case LogType.Warning:   build.Append("###[WRN] " + msg); break;
                        case LogType.Error:     build.Append("!!![ERR] " + msg); break;
                        case LogType.Fatal:     build.Append("$$$[FTL] " + msg); break;
                        case LogType.Trace:  // build.Append("[MSG] " + msg); break;
                        default: break;
                    }
                }
            }
        }

        //[OnOpenAsset]
        //static bool OpenFileContext(int instanceID, int line)
        //{
        //    string assetPath = AssetDatabase.GetAssetPath(instanceID);
        //    var ext = Path.GetExtension(assetPath);
        //}
    }
}

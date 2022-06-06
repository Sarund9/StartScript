using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections.Concurrent;
using System.Text;

namespace StartScript
{
    public class DebugWindow : EditorWindow
    {

        static readonly string SOURCE = @"
using System;

<Toggle(): >

<!this is a comment>

<If()>

<end>

";


        string output = "";

        [MenuItem("Tools/StartScript/Debug Window")]
        public static void Open()
        {
            var win = GetWindow<DebugWindow>();
            win.titleContent = new GUIContent("Debug");
        }

        public void OnGUI()
        {
            GUILayout.TextArea(SOURCE, "Label",
                GUILayout.Height(200f),
                GUILayout.MaxHeight(200f));

            if (GUILayout.Button("Run"))
            {
                var tks = new ConcurrentQueue<Token>();
                Lexer.ParseSSTP(SOURCE, tks.Enqueue);

                // PRINT
                var build = new StringBuilder();
                build.AppendLine("============= TOKEN LOG =============");
                build.AppendLine("===================================");
                while (tks.Count > 0)
                {
                    if (tks.TryDequeue(out var token))
                    {
                        build.AppendLine(token.ToPrint());
                    }
                }
                Debug.Log(build);
            }
            if (GUILayout.Button("Run Parser"))
            {
                var parser = new Parser();
                parser.Parse(SOURCE);

                var build = new StringBuilder(64);

                parser.PrintErrors(Print);

                Debug.Log(build.ToString());

                void Print(LogType type, string msg)
                {
                    switch (type)
                    {
                        case LogType.Message: build.Append("   [MSG] " + msg); break;
                        case LogType.Warning: build.Append("###[WRN] " + msg); break;
                        case LogType.Error: build.Append("!!![ERR] " + msg); break;
                        case LogType.Fatal: build.Append("$$$[FTL] " + msg); break;
                        case LogType.Trace:  // build.Append("[MSG] " + msg); break;
                        default: break;
                    }
                }
            }

            GUILayout.Label(output,
                GUILayout.Height(200f),
                GUILayout.MaxHeight(200f));

            
        }
    }

}

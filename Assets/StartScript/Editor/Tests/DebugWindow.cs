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

        string toFormat = "";
        string result = "";
        string var1 = "";
        string var2 = "";
        string var3 = "";

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

            if (GUILayout.Button("Run Token Logger"))
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
                build.AppendLine("==== PARSER LOG ====");

                parser.PrintErrors(Print);

                Debug.Log(build.ToString());

                void Print(LogType type, string msg)
                {
                    switch (type)
                    {
                        case LogType.Message: build.AppendLine("   [MSG] " + msg); break;
                        case LogType.Warning: build.AppendLine("###[WRN] " + msg); break;
                        case LogType.Error: build.AppendLine("!!![ERR] " + msg); break;
                        case LogType.Fatal: build.AppendLine("$$$[FTL] " + msg); break;
                        case LogType.Trace:  // build.Append("[MSG] " + msg); break;
                        default: break;
                    }
                }
            }

            GUILayout.Label(output,
                GUILayout.Height(200f),
                GUILayout.MaxHeight(200f));

            DoText(ref toFormat, nameof(toFormat));
            DoText(ref result, nameof(result));
            DoText(ref var1, nameof(var1));
            DoText(ref var2, nameof(var2));
            DoText(ref var3, nameof(var3));

            if (GUILayout.Button("Test Format"))
            {
                var parser = new Parser();
                bool succes = parser.ParseTest(toFormat, out var newResult,
                    (nameof(var1), var1),
                    (nameof(var2), var2),
                    (nameof(var3), var3));
                result = succes ? newResult : "Error";
                if (!succes)
                    parser.PrintErrors((t, m) =>
                    {
                        Debug.Log($"[{t}] - {m}");
                    });
            }
        }

        static void DoText(ref string str, string label)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(120f), GUILayout.MinWidth(0));
            str = GUILayout.TextField(str, GUILayout.MinWidth(120f));
            GUILayout.EndHorizontal();
        }
    }

}

#region OLD
/*

VariableCollection vars = new VariableCollection();
string[] names = new string[] {
    "John", "Bob", "Michael", "Jonnatan", "Kaylee", "Rose", "Lea", "Ruth"
};
string GetRandomName() => names[Random.Range(0, names.Length)];

//GUILayout.Label($"GS TEST");

//if (GUILayout.Button("AddName"))
//{
//    if (names.Length > vars.Count)
//    {
//        string key;
//        do
//            key = GetRandomName();
//        while (vars.Exists(key));
//        vars[key] = Random.Range(19, 50);
//    }
//}
//if (GUILayout.Button("Enter Scope"))
//{
//    vars.BeginScope();
//}
//if (GUILayout.Button("Exit Scope"))
//{
//    vars.EndScope();
//}
//foreach (var item in vars)
//{
//    GUILayout.Label($"{item.Key.AssertLenght(10, ' ')}= {item.Value}");
//}
*/
#endregion

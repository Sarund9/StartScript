using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace StartScript
{
    public delegate void LogHandler(LogType msgType, string msg);

    public enum LogType
    {
        /// <summary> Standart Message </summary>
        Message,
        /// <summary> Warning, template may contain unintended behaviour </summary>
        Warning,
        /// <summary> Error, template did not compile </summary>
        Error,
        /// <summary> Internal Error, this should not happen </summary>
        Fatal,
        /// <summary> Only print this for debug purposes </summary>
        Trace,
    }

    public class Parser
    {
        // AST: Hierarchical list of Function calls / Values

        FunctionManager funcs = new FunctionManager();

        List<(LogType type, string msg)> log = new List<(LogType, string)>(32);
        List<Token> toks = new List<Token>(64);
        List<IValueAST> valueList = new List<IValueAST>(64);

        public bool IsFinished { get; private set; }
        public bool IsFaulted { get; private set; }

        // Get succesful result
        public IEnumerable<IValueAST> Iterate => valueList;

        // Print Errors
        public void PrintErrors(LogHandler handler)
        {
            foreach (var msg in log)
            {
                handler(msg.type, msg.msg);
            }
        }

        void Log(string msg) => log.Add((LogType.Message, msg));
        void Warn(string msg) => log.Add((LogType.Warning, msg));
        void Error(string msg) => log.Add((LogType.Error, msg));
        void Trace(string msg) => log.Add((LogType.Trace, msg));

        public void Parse(string source)
        {
            Lexer.ParseSSTP(source, toks.Add);
            
            int exprCount = 0;
            for (int i = 0; i < toks.Count; i++)
            {
                if (toks[i].Type != TokenType.Text)
                {
                    exprCount++;
                    continue;
                }
                
                if (exprCount > 0)
                {
                    HandleExpression(i - exprCount, i, toks, valueList);
                }

                valueList.Add(new ValueAST(toks[i].StringValue));
            }
        }

        void HandleExpression(int start, int end, List<Token> toks, List<IValueAST> list)
        {
            int i = start;
            while (i < end)
            {
                // Ignore Comments
                while (toks[i].Type == TokenType.Comment)
                    i++;

                // Function Calls / Variables
                if (toks[i].Type == TokenType.Name)
                {
                    if (funcs.TryFunction(toks[i].StringValue, out var closure))
                    {
                        
                    }
                }

                // Unhandled Token
                IsFaulted = true;
                Error($"Unhandled Token: {toks[i]}");
                i++;
            }
        }

        void HandleFunction(int start, int end, List<Token> toks, List<IValueAST> list)
        {

        }

    }

}

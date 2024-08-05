using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

        //ClosureManager funcs = new ClosureManager();
        readonly List<Token> toks = new List<Token>(64);
        readonly List<IExpr> valueList = new List<IExpr>(64);
        readonly VariableCollection vars = new VariableCollection();
        readonly List<(LogType type, string msg)> log = new List<(LogType, string)>(32);

        public bool IsFinished { get; private set; }
        public bool IsFaulted { get; private set; }

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
                    exprCount = 0;
                }

                valueList.Add(new ValueExpr(toks[i].StringValue));
            }
        }

        static void HandleExpression(Token[] toks)
        {
            static void Run(IExpr[] exprs, Token[] toks, Func<Token[], int, IExpr> action)
            {
                for (int i = 0; i < toks.Length; i++)
                {
                    if (exprs[i] == null)
                        exprs[i] = action(toks, i);
                }
            }

            var exprs = new IExpr[toks.Length];

            Run(exprs, toks, LiftValues);

            
        }

        static IExpr LiftValues(Token[] toks, int i) => toks[i].Type switch
        {
            TokenType.Number => new ValueExpr(toks[i].Value),
            TokenType.StringLit => new ValueExpr(toks[i].Value),

            _ => null,
        };



        void HandleFunction(int start, int end, List<Token> toks, List<IExpr> list)
        {

        }

        public bool ParseTest(
            string toFormat, out string result,
            params (string, object)[] vars)
        {
            for (int i = 0; i < vars.Length; i++)
            {
                this.vars[vars[i].Item1] = vars[i].Item2;
            }

            return ParseString(toFormat, out result);
        }

        // TODO: More Parsing

        public bool ParseString(string str, out string result)
        {
            var sb = new StringBuilder(str.Length + 16);

            for (int i = 0; i < str.Length; i++)
            {
                // % Format TODO: Constants
                if (str[i] == '%')
                {
                    i++;
                    if (i >= str.Length) { sb.Append('%'); break; }
                    if (str[i] == '{')
                    {
                        // CAPTURE VARIABLE
                        var isb = new StringBuilder();
                        do {
                            i++;
                            if (i >= str.Length) {
                                result = null;
                                Error("String Format: Format not closed");
                                return false;
                            }
                            isb.Append(str[i]);
                        } while (str[i] != '}');
                        if (isb.Length < 2)
                        {
                            result = null;
                            Error("String Format: Empty Format");
                            return false;
                        }

                        var name = isb.ToString()
                            .Substring(0, isb.Length - 1)
                            .Trim();
                        if (!vars.Exists(name))
                        {
                            result = null;
                            Error($"Unkown Variable: '{name}'");
                            return false;
                        }

                        sb.Append(vars[name].ToString());
                    }
                    continue;
                }
                
                // Escape Characters
                if (str[i] == '\\')
                {
                    i++;
                    if (i >= str.Length) {
                        result = null;
                        Error("String Format: Unescaped Final Character");
                        return false;
                    }
                    var ec = GetEscape(str[i]);
                    if (ec == '\0')
                    {
                        result = null;
                        Error($"String Format: Unrecognized escape: \\{str[i]}");
                        return false;
                    }
                    sb.Append(ec);
                    continue;
                }
                
                sb.Append(str[i]);
            }

            result = sb.ToString();
            return true;

            static char GetEscape(char c) => c switch
            {
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'v' => '\v',
                'f' => '\f',
                'a' => '\a',
                '\\' => '\\',

                _ => '\0',
            };
        }
    }

    // IntermediateAST
    
    class AST
    {

    }

    /*
    <Parse("Hello", 20): J = True>

    Name
    Group
    |- String
    |- Comma
    |- Number
    Colon
    Name
    EqualsSign
    Name
    
     */
}

#region OLD
/*
// Function Calls / Names
if (toks[i].Type == TokenType.Name)
{
    if (i + 1 >= end)
    {
        // The expression ended
        Error($"Name: '{toks[i].Value}' is unused. " +
            $"To access a variable use '%{toks[i].Value}'");
        IsFaulted = true;
        break;
    }
    if (toks[i + 1].Type == TokenType.LeftParen)
    {
        if (i + 2 >= end)
        {
            // The expression with 'name('
            Error($"Unclosed group (Expected ')')");
            IsFaulted = true;
            break;
        }
    }
}

                

// Handle Comment by Ignoring
if (toks[i].Type == TokenType.Comment)
{
    i++;
    continue;
}

// Unhandled Token
IsFaulted = true;
Error($"Unhandled Token: {toks[i]}");
//// Strings
//if (toks[i].Type == TokenType.StringLit)
//{
//    valueList.Add(new ValueExpr(toks[i].Value));
//    i++;
//    continue;
//}
//if (toks[i].Type == TokenType.Number)
//{
//    valueList.Add(new ValueExpr(toks[i].Value));
//    i++;
//    continue;
//}
*/
#endregion

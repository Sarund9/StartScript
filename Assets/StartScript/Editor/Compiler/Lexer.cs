using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StartScript
{
    public class Lexer
    {
        private static readonly HashSet<char> s_Operators
                = new HashSet<char>("{}()[].,:;=+-*%&|^~<>!?$#@\\/");

        //private static readonly HashSet<char> s_Operators
        //    = new HashSet<char>("{}()[].,:;=+-*/%&|^~<>!?$#@\\");

        private readonly static Dictionary<string, TokenType> s_CompoundOperators
            = new Dictionary<string, TokenType>()
            {
                //{ "<#", TokenType.OpTemplateStart },
                //{ "//>", TokenType.OpTemplateEnd },
                //{ "\\=/", TokenType.OpTemplateSeparate },
            };

        static readonly Dictionary<string, TokenType> s_ValueKeywords =
            new Dictionary<string, TokenType>()
            {
                { "null", TokenType.Error },
                { "true", TokenType.Error },
                { "false", TokenType.Error },
            };

        

        static bool IsLetter(char c) => char.IsLetter(c);
        static bool IsNumber(char c) => char.IsNumber(c);

        public static async Task ParseSSTPAsync(string source, Action<Token> addtok)
        {
            await Task.Run(() => ParseSSTP(source, addtok));
        }

        public static void ParseSSTP(string source, Action<Token> addtok)
        {
            int curr = 0;
            bool inside = false;

            bool Advance() {
                curr++;
                return curr < source.Length;
            }
            var sb = new StringBuilder();

            // if (!Advance()) { goto lEnd; }
            while (curr < source.Length)
            {
                // Ignore Whitespace
                while (char.IsWhiteSpace(source[curr])) {
                    if (!Advance()) goto lEnd;
                }

                // Diferent
                if (!inside)
                {
                    #region TEXT
                    // Text
                    sb.Clear();
                    while (true)
                    {
                        if (source[curr] == '<')
                        {
                            if (!Advance()) {
                                addtok(new Token(TokenType.Error, "Escape not closed"));
                                goto lEnd;
                            }
                            if (source[curr] == '<')
                            {
                                sb.Append('<');
                                continue;
                            }
                            break;
                        }
                        // TODO: handle '>>' as '>' for consistency reasons

                        sb.Append(source[curr]);
                        if (!Advance()) {
                            addtok(new Token(TokenType.Text, sb.ToString()));
                            goto lEnd;
                        }
                    }
                    addtok(new Token(TokenType.Text, sb.ToString()));
                    inside = true;
                    continue;
                    #endregion
                }
                else
                {
                    while (curr < source.Length)
                    {
                        #region Escape Comment
                        if (source[curr] == '!')
                        {
                            sb.Clear();
                            if (!Advance()) {
                                addtok(new Token(TokenType.Error, "Escape Comment not closed"));
                                goto lEnd;
                            }
                            do {
                                sb.Append(source[curr]);
                                if (!Advance()) {
                                    addtok(new Token(TokenType.Error, "Escape Comment not closed"));
                                    goto lEnd;
                                }
                            } while (source[curr] != '>');
                            
                            addtok(new Token(TokenType.Comment, sb.ToString()));
                            inside = false;
                            if (!Advance())
                                goto lEnd;
                            break;
                        }
                        #endregion

                        #region Ignore Whitespace
                        while (char.IsWhiteSpace(source[curr])) {
                            if (!Advance()) {
                                addtok(new Token(TokenType.Error, "Escape not closed"));
                                goto lEnd;
                            }
                        }
                        #endregion

                        #region Names
                        if (char.IsLetter(source[curr]) || source[curr] == '_')
                        {
                            sb.Clear();
                            do
                            {
                                sb.Append(source[curr]);
                                if (!Advance()) {
                                    addtok(new Token(TokenType.Name, sb.ToString()));
                                    goto lEnd;
                                }
                            } while (char.IsLetterOrDigit(source[curr]) || source[curr] == '_');
                            addtok(new Token(TokenType.Name, sb.ToString()));
                        }
                        #endregion

                        // TODO: Numbers
                        // TODO: Strings

                        #region New Lines
                        if (Environment.NewLine.Contains(source[curr]))
                        {
                            do {
                                if (!Advance()) {
                                    addtok(new Token(TokenType.NewLine, null));
                                    goto lEnd;
                                }
                            } while (Environment.NewLine.Contains(source[curr]));
                            addtok(new Token(TokenType.NewLine, null));
                        }
                        #endregion

                        #region Standart Escape
                        if (source[curr] == '>')
                        {
                            if (!Advance()) {
                                goto lEnd;
                            }
                            if (source[curr] == '>')
                            {
                                addtok(new Token(TokenType.RightArrow, null));
                                continue;
                            }
                            // Exit out of inside
                            inside = false;
                            break;
                        }
                        #endregion

                        // TODO: Complex Operators

                        // Handle operators //
                        var typ = GetCharOperator(source[curr]);

                        // Handle the known character
                        if (typ != TokenType.Error)
                            addtok(new Token(typ, null));
                        // Handle any other character as unknown
                        else
                            addtok(new Token(TokenType.Error, $"Unkown char: '{source[curr]}'"));
                        

                        if (!Advance()) {
                            addtok(new Token(TokenType.Error, "Escape not closed"));
                            goto lEnd;
                        }
                    }
                }
            }
        lEnd:;
            //addtok(new Token(TokenType.EoF, null));
            
        }

        static TokenType GetCharOperator(char C) => C switch
        {
            '{' => TokenType.LeftBracket,
            '}' => TokenType.RightBracket,
            '(' => TokenType.LeftParen,
            ')' => TokenType.RightParen,
            '[' => TokenType.LeftSqBracket,
            ']' => TokenType.RightSqBracket,

            '.' => TokenType.Period,
            ',' => TokenType.Comma,
            ':' => TokenType.Colon,
            ';' => TokenType.Semicolon,
            '=' => TokenType.EqualsSign,
            '+' => TokenType.PlusSign,
            '-' => TokenType.MinusSign,
            '*' => TokenType.Asterisk,
            '/' => TokenType.ForwardSlash,
            '&' => TokenType.Ampersand,
            '|' => TokenType.VerticalBar,
            '^' => TokenType.Caret,
            '~' => TokenType.Tilde,
            '<' => TokenType.LeftArrow,
            '>' => TokenType.RightArrow,
            '!' => TokenType.ExclamationSign,
            '?' => TokenType.QuestionSign,
            '$' => TokenType.DollarSign,
            '#' => TokenType.HashSign,
            '@' => TokenType.AtSign,

            _ => TokenType.Error,
        };

        //public void Run(string source, Action<Token> addtok)
        //{
        //    State state = Outside;
        //    for (int i = 0; i < source.Length;) {
        //        var nextState = state(source, ref i, addtok);
        //        if (nextState != null) {
        //            state = nextState;
        //        }
        //    }
        //    addtok(new Token(TokenType.EoF, null));
        //}

        // #define STATE_FUNC(name) State name (string source, ref int index, Action<Token> addtok)
        // mixin StateFunc(name) { State name (string source, ref int index, Action<Token> addtok) }

        //State Outside(string source, ref int i, Action<Token> addtok)
        //{
        //    sb.Clear();
        //    while (i < source.Length)
        //    {
        //        if (source[i] == '<')
        //        {

        //        }
        //        sb.Append(source[i]);
        //        i++;
        //    }

        //    return null;
        //}
        //State Inside(string source, ref int i, Action<Token> addtok)
        //{

        //    i++;
        //    return null;
        //}
    }

}

#region OLD_CODE
/*
struct TokAdder
{
    private readonly Action<Token> addtok;
    public TokAdder(Action<Token> addtok) : this()
    {
        this.addtok = addtok;
    }

    public bool LastWasNewLine { get; set; }
    public bool LastWas2Tick { get; set; }

    public void Add(in Token tok)
    {
        LastWasNewLine = false;
        LastWas2Tick = false;
        addtok(tok);
    }

}
var it = source.GetEnumerator();
            bool isIt = it.MoveNext();

            var tkAdd = new TokAdder(addtok);

            while (isIt)
            {
                while (isIt && (it.Current == ' ' || it.Current == '\t'))
                    isIt = it.MoveNext();

                // Names
                if (IsLetter(it.Current))
                {
                    var build = new StringBuilder();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();
                    } while (isIt && (IsLetter(it.Current) || IsNumber(it.Current)));

                    tkAdd.Add(new Token(TokenType.Name, build.ToString()));
                    continue;
                }

                // Numbers
                if (IsNumber(it.Current))
                {
                    var build = new StringBuilder();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();
                    } while (isIt && (IsLetter(it.Current) || IsNumber(it.Current)));

                    tkAdd.Add(new Token(TokenType.Number, build.ToString()));
                    continue;
                }

                // String Literal
                if (it.Current == '"')
                {
                    var build = new StringBuilder();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();

                        if (!tkAdd.LastWas2Tick && (!isIt || it.Current == '\n' || it.Current == '\r'))
                        {
                            tkAdd.Add(new Token(TokenType.Error, $"String End of Line Error at[{ build }]"));
                            return false;
                        }
                        //throw new Exception("String Error");

                    } while (it.Current != '"');

                    tkAdd.Add(new Token(TokenType.StringLit, build.ToString()));
                    isIt = it.MoveNext();
                    continue;
                }
                if (it.Current == '\'')
                {
                    var build = new StringBuilder();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();

                        if (!tkAdd.LastWas2Tick && (!isIt || it.Current == '\n' || it.Current == '\r'))
                        {
                            tkAdd.Add(new Token(TokenType.Error, $"String End of Line Error at[{build}]"));
                            return false;
                        }
                        //throw new Exception("String Error");

                    } while (it.Current != '\'');

                    tkAdd.Add(new Token(TokenType.StringLit, build.ToString()));
                    if (build.ToString() == "\'")
                    {
                        tkAdd.LastWas2Tick = true;
                    }
                    isIt = it.MoveNext();
                    continue;
                }

                // New Lines
                if (Environment.NewLine.Contains(it.Current))
                {
                    
                    if (!tkAdd.LastWasNewLine)
                    {
                        tkAdd.Add(new Token(TokenType.NewLine, null));
                    }
                    isIt = it.MoveNext();
                    continue;
                }

                // New Comment Syntax
                if (it.Current == '<')
                {
                    isIt = it.MoveNext();
                    if (!isIt)
                    {
                        tkAdd.Add(new Token(TokenType.LeftArrow, null));
                    }

                }

                // Operators
                if (s_Operators.Contains(it.Current))
                {
                    var build = new StringBuilder();
                    while (isIt && s_Operators.Contains(it.Current))
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();
                    }

                    // Compound Operator 
                    if (s_CompoundOperators.TryGetValue(build.ToString(), out var type))
                    {
                        tkAdd.Add(new Token(type, ""));
                    }
                    else
                    {
                        //Console.WriteLine($"== ADDING FULL: '{build}'");
                        foreach (var OPC in build.ToString())
                        {
                            tkAdd.Add(new Token(GetCharOperator(OPC), ""));
                        }
                    }

                    continue;
                }

                Debug.LogError($"Unknown Char: {it.Current}");
                return false;
            }
            tkAdd.Add(new Token(TokenType.EoF, null));

            return true;
int out_start = 0;
int in_start = 0;
int i = 0;
void AddText()
{
    // Add Text Tokens
    if (i - out_start > 0)
    {
        var substring = source.Substring(out_start, i - out_start);
        addtok(new Token(TokenType.Text, substring));
        out_start = i;
    }
}
bool Advance()
{
    i++;
    if (i == source.Length)
    {
        // Add Error Tokens
        if (i - in_start > 0)
        {
            var substring = source.Substring(in_start, i - in_start);
            addtok(new Token(TokenType.Error, substring));
            in_start = i;
        }
        return false;
    }
    return true;
}

while (i < source.Length)
{
    if (source[i] == '<')
    {
        AddText();
        if (!Advance())
            goto End;
        if (source[i] == '<')

        while (true)
        {
            if (!Advance())
                goto End;
        }
    }


    i++;
}
private static readonly HashSet<char> s_Operators
                = new HashSet<char>("{}()[].,:;=+-*% &| ^~<> !?$#@\\/");

        //private readonly static Dictionary<string, TokenType> s_CompoundOperators
        //    = new Dictionary<string, TokenType>()
        //    {
        //            { "<#", TokenType.OpTemplateStart },
        //            { "//>", TokenType.OpTemplateEnd },
        //            { "\\=/", TokenType.OpTemplateSeparate },
        //    };

        //static readonly Dictionary<string, TokenType> s_ReservedKeywords =
        //    new Dictionary<string, TokenType>()
        //    {
        //            { "null", TokenType.Err },
        //            { "true", TokenType.Err },
        //            { "false", TokenType.Err },
        //    };
        
        public static async Task ParseAsync(
            IEnumerable<char> source, ConcurrentQueue<Token> tokens)
{
    await Task.Run(() => ParseSSTP(source.GetEnumerator(), tokens));
}
public static void ParseSSTP(IEnumerator<char> it, ConcurrentQueue<Token> tokens)
{
    //var it = source;
    bool isIt = it.MoveNext();
    var charBuffer = new Queue<char>();
    bool isLastNewLine = false;

    // == Local Functions
    void Advance()
    {
        isIt = it.MoveNext();
    }
    void AddTok(Token tok)
    {
        if (isLastNewLine && tok.Type == TokenType.NewLine)
        {
            return;
        }

        isLastNewLine = tok.Type == TokenType.NewLine;
        tokens.Enqueue(tok);
    }

    // ===
    var textbuild = new StringBuilder();

    while (isIt)
    {
        while (isIt && IsNewLine(it.Current))
        {
            if (textbuild.Length > 0)
            {
                AddTok(new Token(TokenType.Text, textbuild.ToString()));
                textbuild.Clear();
            }
            if (IsNewLine(it.Current))
            {
                AddTok(new Token(TokenType.NewLine, null));
            }
            Advance();
        }

        if (!isIt)
            break;

        if (it.Current == '<')
        {
            Advance();

            if (!isIt)
            {
                // TODO: Handle Error
                Debug.LogError("Code Block not Closed!");
                return;
            }

            if (it.Current == '!')
            {
                // TODO: Handle Comments
                while (true)
                {
                    charBuffer.Enqueue(it.Current);
                    Advance();
                    if (!isIt)
                    {
                        // TODO: Handle Error
                        Debug.LogError("Comment Block not Closed!");
                        return;
                    }
                    if (it.Current == '>')
                    {
                        Advance();
                        break;
                    }
                }
                AddTok(
                    new Token(
                        TokenType.Comment,
                        charBuffer.ExaustContentsToString())
                    );
            }
            else if (it.Current == '<')
            {
                textbuild.Append('<');
                Advance();
            }
            else
            {
                while (true)
                {
                    charBuffer.Enqueue(it.Current);
                    Advance();
                    if (!isIt)
                    {
                        // TODO: Handle Error
                        Debug.LogError("Code Block not Closed!");
                        return;
                    }
                    if (it.Current == '>')
                    {
                        Advance();
                        if (!isIt || it.Current != '>')
                        {
                            break;
                        }
                        charBuffer.Enqueue(it.Current);
                    }
                }
                // TODO: Parse Buffer
                Debug.Log("Parse Buffer: "
                    + charBuffer.PeekContentsToString());
                ParseBuffer(charBuffer, AddTok);
                charBuffer.Clear(); // Clear buffer if an Error occurred
            }
        }


        textbuild.Append(it.Current);
        Advance();
        //Debug.LogError($"Unknown Char: {it.Current}");
        //return;
    }

}

static void ParseBuffer(Queue<char> charStack, Action<Token> addtok)
{
    char C;
    // == Local Functions
    void Advance() => C = charStack.Dequeue();
    bool IsIt() => charStack.Count > 0;
    // ===
    Advance();
    while (charStack.Count > 0)
    {
        while (char.IsWhiteSpace(C))
            Advance();

        // Names
        if (IsLetter(C))
        {
            var build = new StringBuilder();
            do
            {
                build.Append(C);
                Advance();
            } while (IsIt() && (IsLetter(C) || IsNumber(C)));

            addtok(new Token(TokenType.Name, build.ToString()));
            continue;
        }

        // Numbers
        if (IsNumber(C))
        {
            var build = new StringBuilder();
            do
            {
                build.Append(C);
                Advance();
            } while (IsIt() && (IsLetter(C) || IsNumber(C)));

            addtok(new Token(TokenType.Number, build.ToString()));
            continue;
        }

        // Operators
        if (s_Operators.Contains(C))
        {
            var build = new StringBuilder();
            do
            {
                build.Append(C);
                Advance();
            } while (s_Operators.Contains(C));

            Debug.Log($"PARSING '{build}'");
            // TODO: Complex Operators ?
            foreach (var opc in build.ToString())
            {
                addtok(new Token(
                    GetCharOperator(opc), null
                    ));
            }
            continue;
        }

        Debug.LogError($"Unknown Character {C}");
        return;
    }

}
static string ExaustContentsToString(this Queue<char> charStack)
{
    var build = new StringBuilder();
    while (charStack.Count > 0)
    {
        build.Append(charStack.Dequeue());
    }
    return build.ToString();
}
static string PeekContentsToString(this Queue<char> charStack)
{
    var build = new StringBuilder();
    foreach (var item in charStack)
    {
        build.Append(item);
    }
    return build.ToString();
}
static TokenType GetCharOperator(char C) => C switch
{
    '{' => TokenType.LeftBracket,
    '}' => TokenType.RightBracket,
    '(' => TokenType.LeftParen,
    ')' => TokenType.RightParen,
    '[' => TokenType.LeftSqBracket,
    ']' => TokenType.RightSqBracket,

    '.' => TokenType.Period,
    ',' => TokenType.Comma,
    ':' => TokenType.Colon,
    ';' => TokenType.Semicolon,
    '=' => TokenType.EqualsSign,
    '+' => TokenType.PlusSign,
    '-' => TokenType.MinusSign,
    '*' => TokenType.Asterisk,
    '/' => TokenType.ForwardSlash,
    '&' => TokenType.Ampersand,
    '|' => TokenType.VerticalBar,
    '^' => TokenType.Caret,
    '~' => TokenType.Tilde,
    '<' => TokenType.LeftArrow,
    '>' => TokenType.RightArrow,
    '!' => TokenType.ExclamationSign,
    '?' => TokenType.QuestionSign,
    '$' => TokenType.DollarSign,
    '#' => TokenType.HashSign,
    '@' => TokenType.AtSign,

    _ => throw new Exception("Unknown Char"),
};
static bool IsNewLine(char C)
{
    var str = Environment.NewLine;
    for (int i = 0; i < str.Length; i++)
        if (str[i] == C)
            return true;
    return false;
}

static bool IsLetter(char C)
{
    var cat = char.GetUnicodeCategory(C);
    return C == '_'
        || cat == UnicodeCategory.UppercaseLetter
        || cat == UnicodeCategory.LowercaseLetter;
}
static bool IsNumber(char C) => char.IsNumber(C);

*/
#endregion

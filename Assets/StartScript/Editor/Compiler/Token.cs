using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StartScript
{
    [Serializable]
    public struct Token
    {
        private readonly TokenType type;
        private readonly object m_Value;

        public Token(TokenType type, object value)
        {
            this.type = type;
            m_Value = new ByteBox();
            m_Value = value;
        }

        public TokenType Type => type;
        public object Value => m_Value;

        public string StringValue => m_Value as string;

        public string ToPrint()
        {
            var build = new StringBuilder();
            build.Append(Type);
            while (build.Length < 20)
                build.Append(' ');
            build.Append($"= {Value}");
            return build.ToString();
        }
        public override string ToString()
        {
            return $"[{Type}|{Value}]";
        }

        public override bool Equals(object obj) => obj is Token tok &&
                Type == tok.Type &&
                Value == tok.Value;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Token a, Token b) =>
            a.Type == b.Type &&
            a.Value == b.Value;

        public static bool operator !=(Token a, Token b) =>
            a.Type != b.Type ||
            a.Value != b.Value;
    }

    public enum TokenType
    {
        Error,

        EoF,

        Text,

        Name,

        Number,

        StringLit,

        Comment,

        NewLine,

        // Brackets
        LeftBracket,            // {
        RightBracket,           // }
        LeftParen,              // (
        RightParen,             // )
        LeftSqBracket,          // [
        RightSqBracket,         // ]
        LeftArrow,              // <
        RightArrow,             // >

        // Operator Characters
        Period,                 // .
        Comma,                  // ,
        Colon,                  // :
        Semicolon,              // ;
        EqualsSign,             // =
        PlusSign,               // +
        MinusSign,              // -
        Asterisk,               // *
        ForwardSlash,           // /
        PercentSign,            // %
        Ampersand,              // &
        VerticalBar,            // |
        Caret,                  // ^
        Tilde,                  // ~
        ExclamationSign,        // !
        QuestionSign,           // ?
        DollarSign,             // $
        HashSign,               // #
        AtSign,                 // @
        Backslash,              // \

        // Compound Operators

    }

    public static class TokenTypeMethods
    {

    }
}

/**/
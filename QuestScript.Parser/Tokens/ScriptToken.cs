// ReSharper disable UnusedMember.Global

using Superpower.Display;

namespace QuestScript.Parser.Tokens
{
    public enum ScriptToken
    {
        Undefined,

        [Token(Category = "comment")]
        LineComment,
        [Token(Category = "comment")]
        BlockComment,

        //literals
        [Token(Category = "literal")]
        IntegerLiteral,
        [Token(Category = "literal")]
        DoubleLiteral,
        [Token(Category = "literal")]
        StringLiteral,
        [Token(Category = "literal")]
        BooleanLiteral,
        [Token(Category = "literal")]
        NullLiteral,

        //misc tokens
        [Token(Example = ",")]
        Comma,

        [Token(Example = "(")]
        LeftParen,

        [Token(Example = ")")]
        RightParen,

        [Token(Example = ":")]
        Colon,

        [Token(Example = "{")]
        LeftBracket,

        [Token(Example = "}")]
        RightBracket,

        [Token(Example = "[")]
        LeftSquareBracket,

        [Token(Example = "]")]
        RightSquareBracket,

        [Token(Category = "identifier")]
        Identifier,

        //operators

        [Token(Example = "+",Category = "operator")]
        Plus,

        [Token(Example = "-",Category = "operator")]
        Minus,

        [Token(Example = "*",Category = "operator")]
        Multiply,

        [Token(Example = "/",Category = "operator")]
        Divide,


        [Token(Example = "%",Category = "operator")]
        Mod,

        [Token(Example = "And",Category = "operator")]
        And,

        [Token(Example = "Or",Category = "operator")]
        Or,

        [Token(Example = "Not",Category = "operator")]
        Not,

        //keyword tokens
        [Token(Category = "keyword")]
        If,
        [Token(Category = "keyword")]
        ElseIf,
        [Token(Category = "keyword")]
        Else,
        [Token(Category = "keyword")]
        While,
        [Token(Category = "keyword")]
        For,
        [Token(Category = "keyword")]
        ForEach
    }
}

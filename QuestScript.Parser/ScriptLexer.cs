using System;
using System.Collections.Generic;
using System.Text;
using QuestScript.Parser.Tokens;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace QuestScript.Parser
{
    public class ScriptLexer
    {
        //TODO: rewrite everything possible to use Span<T> - minimize allocations

        #region Custom Tokenizers

        public static TextParser<string> Identifier { get; } = (TextParser<string>) (input =>
        {
            var result = input.ConsumeChar();
            var sb = new StringBuilder();           

            if(!result.HasValue && !char.IsLetter(result.Value) && result.Value != '_')
                return Result.Empty<string>(input, "an identifier should start with a letter or a '_'");

            bool IsTerminator(char c) => c == '(' || c == '\n' || c == '{' || c == ' ';
            bool IsIdentifierChar(char c) => char.IsLetterOrDigit(c) || c == '_';

            return RecognizeIdentifier(result, input,IsIdentifierChar,IsTerminator);
        });

        public static TextParser<string> SpecialIdentifier { get; } = (TextParser<string>) (input =>
        {
            //TODO: rewrite this to use Span<string> to reduce allocations
            var result = input.ConsumeChar();

            if(!result.HasValue && !char.IsLetter(result.Value) && result.Value != '_')
                return Result.Empty<string>(input, "an identifier should start with a letter or a '_'");
            bool IsIdentifierChar(char c) => char.IsLetterOrDigit(c) || c == ' ';
            bool IsTerminator(char c) => c == '(' || c == '\n' || c == '{';

            return RecognizeIdentifier(result, input,IsIdentifierChar,IsTerminator);
        });

        private static Result<string> RecognizeIdentifier(Result<char> result, TextSpan input, Func<char,bool> isIdentifierChar, Func<char,bool> isTerminatorChar)
        {
            var sb = new StringBuilder();           
            var current = result.Value;
            TextSpan remainder;
            do
            {
                sb.Append(current);
                remainder = result.Remainder;
                result = result.Remainder.ConsumeChar();
                if (!result.HasValue || !isIdentifierChar(current))
                    break;
                current = result.Value;
            } while (!isTerminatorChar(current));

            return current == ' '
                ? Result.Empty<string>(input, "an identifier must not end with a whitespace")
                : Result.Value(sb.ToString(), input, remainder);
        }
       
        #endregion

        public static readonly Tokenizer<ScriptToken> Instance  = new TokenizerBuilder<ScriptToken>()
            .Ignore(Span.WhiteSpace)
            .Ignore(Span.EqualTo("\r\n"))      
            
            //comments
            .Match(Comment.CPlusPlusStyle,ScriptToken.LineComment)
            .Match(Comment.CStyle,ScriptToken.BlockComment)

            //literals
            .Match(Numerics.Integer, ScriptToken.IntegerLiteral)
            .Match(Numerics.Decimal, ScriptToken.DoubleLiteral)
            .Match(QuotedString.CStyle, ScriptToken.StringLiteral)
            .Match(Span.EqualToIgnoreCase("null"), ScriptToken.NullLiteral)
            .Match(Span.EqualToIgnoreCase(bool.TrueString).Try()
                .Or(Span.EqualToIgnoreCase(bool.FalseString)), ScriptToken.BooleanLiteral)

            //misc
            .Match(Character.EqualTo(','),ScriptToken.Comma)
            .Match(Character.EqualTo(':'),ScriptToken.Colon)
            .Match(Character.EqualTo('('),ScriptToken.LeftParen)
            .Match(Character.EqualTo(')'),ScriptToken.RightParen)
            .Match(Character.EqualTo('{'),ScriptToken.LeftBracket)
            .Match(Character.EqualTo('}'),ScriptToken.RightBracket)
            .Match(Character.EqualTo('['),ScriptToken.LeftSquareBracket)
            .Match(Character.EqualTo(']'),ScriptToken.RightSquareBracket)

            //identifiers
            //note : 'Try()' backtracks if unsuccessful and tries another parser
            .Match(Identifier.Try().Or(SpecialIdentifier),ScriptToken.Identifier)

            //keywords
            .Match(Span.EqualToIgnoreCase("if"),ScriptToken.If)
            .Match(Span.EqualToIgnoreCase("elseif"),ScriptToken.ElseIf)
            .Match(Span.EqualToIgnoreCase("else"),ScriptToken.Else)
            .Match(Span.EqualToIgnoreCase("while"),ScriptToken.While)
            .Match(Span.EqualToIgnoreCase("for"),ScriptToken.For)
            .Match(Span.EqualToIgnoreCase("foreach"),ScriptToken.ForEach)

            //operators
            .Match(Character.EqualTo('+'),ScriptToken.Plus)
            .Match(Character.EqualTo('-'),ScriptToken.Minus)
            .Match(Character.EqualTo('*'),ScriptToken.Multiply)
            .Match(Character.EqualTo('/'),ScriptToken.Divide)
            .Match(Character.EqualTo('%'),ScriptToken.Mod)

            .Build();

    }
}

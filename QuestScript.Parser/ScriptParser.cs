using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using QuestScript.Parser.Helpers;
using QuestScript.Parser.ScriptElements;
using QuestScript.Parser.Tokens;
using QuestScript.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace QuestScript.Parser
{
    public class ScriptParser
    {
        #region Subparsers for Literals

        private static TokenListParser<ScriptToken, LiteralExpression> GenerateLiteralsParser<TLiteral>(ScriptToken tokenType)
            where TLiteral : Types.Type
        {
            if(tokenType == ScriptToken.Undefined) //precaution, should never happen
                throw new InvalidEnumArgumentException("Invalid token type, cannot be undefined");

            var typeInstance = typeof(TLiteral).GetScriptTypeInstance();

            return Token.EqualTo(tokenType)
                        .Select(t => new LiteralExpression
                        {
                            Type = typeInstance,
                            Value = Convert.ChangeType(t.ToStringValue(),typeInstance.UnderlyingType)
                        });
        }

        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralBooleanExpression =
            GenerateLiteralsParser<BooleanType>(ScriptToken.BooleanLiteral);

        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralIntegerExpression =
            GenerateLiteralsParser<IntegerType>(ScriptToken.IntegerLiteral);

        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralDoubleExpression =
            GenerateLiteralsParser<DoubleType>(ScriptToken.DoubleLiteral);

        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralStringExpression =
            GenerateLiteralsParser<StringType>(ScriptToken.StringLiteral);
        
        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralNullExpression =
            Token.EqualTo(ScriptToken.NullLiteral)
                .Select(t => new LiteralExpression
                {
                    Type = ObjectType.Instance,
                    Value = null
                });

        #endregion

        #region Subparsers for Expressions

        #endregion

        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralExpression =
            LiteralIntegerExpression
                .Try().Or(LiteralDoubleExpression)
                .Try().Or(LiteralStringExpression)
                .Try().Or(LiteralBooleanExpression)
                .Try().Or(LiteralNullExpression);

        private static readonly TokenListParser<ScriptToken, ParenthesizedExpression> ParenthesizedExpression =
            from leftParen in Token.EqualTo(ScriptToken.LeftParen)
            from expr in Superpower.Parse.Ref(() => Expression)
            from rightParen in Token.EqualTo(ScriptToken.RightParen)
            select new ParenthesizedExpression(expr);
        
        //exception parser, it parses types recursively
        private static readonly TokenListParser<ScriptToken, Expression> Expression = 
                LiteralExpression.Select(expr => (Expression)expr)
                    .Try().Or(ParenthesizedExpression.Select(x => (Expression)x))
            ;

        public static ScriptContext Parse(TokenList<ScriptToken> scriptTokens)
        {            
            var root = new ScriptContext();
            
            return root;
        }
    }
}

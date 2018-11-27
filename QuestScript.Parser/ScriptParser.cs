using System;
using System.ComponentModel;
using QuestScript.Parser.Expressions;
using QuestScript.Parser.Helpers;
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

        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralBoolean =
            GenerateLiteralsParser<BooleanType>(ScriptToken.BooleanLiteral);

        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralInteger =
            GenerateLiteralsParser<IntegerType>(ScriptToken.IntegerLiteral);

        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralDouble =
            GenerateLiteralsParser<DoubleType>(ScriptToken.DoubleLiteral);

        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralString =
            GenerateLiteralsParser<StringType>(ScriptToken.StringLiteral);
        
        private static readonly TokenListParser<ScriptToken, LiteralExpression> LiteralNull =
            Token.EqualTo(ScriptToken.NullLiteral)
                .Select(t => new LiteralExpression
                {
                    Type = ObjectType.Instance,
                    Value = null
                });

        public static readonly TokenListParser<ScriptToken, LiteralExpression> Literal =
            LiteralInteger.Or(LiteralDouble).Or(LiteralString).Or(LiteralBoolean).Or(LiteralNull);

        #endregion

        public static ScriptRoot Parse(TokenList<ScriptToken> scriptTokens)
        {
            var root = new ScriptRoot();
            
            return root;
        }
    }
}

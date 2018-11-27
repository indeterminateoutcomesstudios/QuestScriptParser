using System;
using System.Collections.Generic;
using System.Text;
using QuestScript.Parser.Expressions;
using QuestScript.Parser.Tokens;
using Superpower;
using Superpower.Model;

namespace QuestScript.Parser
{
    public class ScriptParser
    {
        private readonly static TokenListParser<ScriptToken, LiteralExpression>

        public static Script Parse(TokenList<ScriptToken> scriptTokens)
        {
            var root = new Script();

            return root;
        }
    }
}

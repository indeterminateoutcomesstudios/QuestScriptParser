using Antlr4.Runtime;
using QuestScriptParser;

namespace QuestScript.Tests
{
    public class BaseParserTest
    {
        protected static QuestScriptParser.QuestScriptParser GenerateParserForScript(string script)
        {
            var lexer = new QuestScriptLexer(new AntlrInputStream(script));
            var tokens = new CommonTokenStream(lexer);
            var parser = new QuestScriptParser.QuestScriptParser(tokens);
            return parser;
        }
    }
}

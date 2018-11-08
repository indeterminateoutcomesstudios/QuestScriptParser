using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace QuestScriptParser.Tryouts
{
    class Program
    {
        protected static QuestScriptParser GenerateParserForScript(string script)
        {
            var lexer = new QuestScriptLexer(new AntlrInputStream(script));
            var tokens = new CommonTokenStream(lexer);
            var parser = new QuestScriptParser(tokens);
            return parser;
        }

        static void Main(string[] args)
        {
            var parser = GenerateParserForScript(@" x++ ");
            
            var scriptTree = parser.script();
            var treeAsString = scriptTree.ToStringTree(parser);
            Console.WriteLine(treeAsString);
            //var treeToStringVisitor = new StringQuestScriptVisitor();
            //treeToStringVisitor.Visit(scriptTree);
            //var formattedCode = treeToStringVisitor.Output;

            //Console.WriteLine(formattedCode);
        }
    }
}

using System;
using System.Linq;
using Antlr4.Runtime;
using QuestScript.Interpreter;
using QuestScript.Parser;

namespace QuestScript.Tryouts
{
     class Program
    {
        static void Main(string[] args)
        {

            var lexer = new QuestScriptLexer(new AntlrInputStream(@" 
                x = 43.33
                { x = 34 }
            "));

            //var collectorTokenSource = new CollectorTokenSource(lexer);
            var tokens = new CommonTokenStream(lexer);
            var parser = new QuestScriptParser(tokens);
            var environmentTreeBuilder = new EnvironmentTreeBuilder();            
            var scriptTree = parser.script();
            environmentTreeBuilder.Visit(scriptTree);

            foreach (var msg in environmentTreeBuilder.Errors.Select(e => e.Message))
                Console.WriteLine(msg);

            Console.ReadKey();
        }
    }
}

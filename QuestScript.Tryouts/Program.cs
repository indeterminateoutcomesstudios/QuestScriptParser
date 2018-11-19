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
                x = 4    
                y = 10.5 - (x + 2) / 2 
            "));
            var tokens = new CommonTokenStream(lexer);
            var parser = new QuestScriptParser(tokens);

            var environmentTreeBuilder = new EnvironmentTreeBuilder();            
            var scriptTree = parser.script();
            environmentTreeBuilder.Visit(scriptTree);

            var envTree = environmentTreeBuilder.Output;

            foreach (var variable in envTree.DebugGetAllVariables())
            {
                Console.WriteLine(variable);
            }

            foreach (var msg in environmentTreeBuilder.Errors.Select(e => e.Message))
                Console.WriteLine(msg);

            Console.ReadKey();
        }
    }
}

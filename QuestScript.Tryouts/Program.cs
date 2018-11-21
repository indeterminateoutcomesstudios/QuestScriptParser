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
            //z = 
            var lexer = new QuestScriptLexer(new AntlrInputStream(@" 
                z = [ [[1,2],[3,4]] , [[1,2],[""ABC"",4],[5,6]] ]
                x = not true
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

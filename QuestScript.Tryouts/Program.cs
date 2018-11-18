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
                    z = 1

                if(x > 0)
                {
                    x = z + 4
                }
                {
                    {
                        x = 4 + z
                    }
                }   
            "));
            //var collectorTokenSource = new CollectorTokenSource(lexer);
            var tokens = new CommonTokenStream(lexer);
            var parser = new QuestScriptParser(tokens);
            var environmentTreeBuilder = new EnvironmentTreeBuilder();            
            var scriptTree = parser.script();
            environmentTreeBuilder.Visit(scriptTree);
            //Console.WriteLine(scriptTree.ToStringTree(parser));

            foreach (var msg in environmentTreeBuilder.Errors.Select(e => e.Message))
                Console.WriteLine(msg);

            Console.ReadKey();
        }
    }
}

using System;
using Antlr4.Runtime;
using QuestScript.Interpreter;
using QuestScript.Parser;

namespace QuestScript.Tryouts
{
     class Program
    {
        static void Main(string[] args)
        {
            
            var questGameLexer = new QuestGameLexer(new AntlrFileStream(@"C:\Users\orev.HRHINOS\Documents\Quest Games\TestGame\TestGame.aslx"));
            var questGameParser = new QuestGameParser(new CommonTokenStream(questGameLexer));
            
            var questGameTree = questGameParser.document();
            var gameObjectResolver = new GameObjectResolverVisitor();
            gameObjectResolver.Visit(questGameTree);         

            Console.ReadKey();
        }
    }
}

//var lexer = new QuestScriptLexer(new AntlrInputStream(@" 
//    z = [ [[1,2],[3,4]] , [[1,2],[6,4],[5,6]], [[[3,4],[4,5]],[[6,7],[8.5]]] ]
//    x = not true
//"));
//var tokens = new CommonTokenStream(lexer);
//var parser = new QuestScriptParser(tokens);

//var environmentTreeBuilder = new EnvironmentTreeBuilder();            
//var scriptTree = parser.script();
//environmentTreeBuilder.Visit(scriptTree);

//var envTree = environmentTreeBuilder.Output;

//foreach (var variable in envTree.DebugGetAllVariables())
//{
//    Console.WriteLine(variable);
//}

//foreach (var msg in environmentTreeBuilder.Errors.Select(e => e.Message))
//    Console.WriteLine(msg);

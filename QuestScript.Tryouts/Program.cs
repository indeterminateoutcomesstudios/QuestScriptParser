using System;
using System.Linq;
using System.Management;
using Antlr4.Runtime;
using QuestScript.Interpreter;
using QuestScript.Interpreter.Helpers;
using QuestScript.Parser;

namespace QuestScript.Tryouts
{
     class Program
    {
        static void Main(string[] args)
        {         

            var questGameLexer = new QuestGameLexer(new AntlrFileStream(@"C:\Users\Admin\Documents\Quest Games\BasicNeedsLib\TestGame.aslx"));
            var questGameParser = new QuestGameParser(new CommonTokenStream(questGameLexer));

            var questGameTree = questGameParser.game();
            var gameObjectResolver = new GameObjectResolverVisitor();
            gameObjectResolver.Visit(questGameTree);
            ////Console.WriteLine(questGameTree.ToStringTree(questGameParser));
            Console.ReadKey();
        }
    }
}
using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using QuestScript.Interpreter;
using QuestScript.Parser;

namespace QuestScript.Tryouts
{
    internal class Program
    {               
        
        private static void Main(string[] args)
        {
            var parser = new QuestScriptParser(new CommonTokenStream(new QuestScriptLexer(new AntlrInputStream(@"
      switch (x) {
        case(""a"")  { }
        case(""b"")  { }
        default {}
      }
            "))));
            var tree = parser.script();
            //Console.WriteLine(tree.ToStringTree(parser));
            //var gameObjectResolver = new GameObjectResolver(@"C:\Users\Admin\Documents\Quest Games\BasicNeedsLib\TestGame.aslx");
            //var gameObjectResolver = new GameObjectResolver(@"C:\Program Files (x86)\Quest 5\Core\CoreDevMode.aslx");
            Console.ReadKey();
        }
    }
}
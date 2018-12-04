using System;
using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using QuestScript.Interpreter;
using QuestScript.Parser;

namespace QuestScript.Tryouts
{
    internal class Program
    {               
        
        private static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            var gameObjectResolver = new GameObjectResolver(@"C:\Users\Admin\Documents\Quest Games\BasicNeedsLib\TestGame.aslx");
            Console.WriteLine(sw.Elapsed);
            Console.ReadKey();
        }
    }
}
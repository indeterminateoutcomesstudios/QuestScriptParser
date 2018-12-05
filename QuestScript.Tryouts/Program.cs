using System;
using System.Diagnostics;
using QuestScript.Interpreter;

namespace QuestScript.Tryouts
{
    internal class Program
    {               
        
        private static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            var gameObjectResolver = new GameObjectResolver(@"C:\Users\Admin\Documents\Quest Games\BasicNeedsLib\BasicNeedsLib.aslx");
            Console.WriteLine(sw.Elapsed);
            Console.ReadKey();
        }
    }
}
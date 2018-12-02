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

            var gameObjectResolver = new GameObjectResolver(@"C:\Users\Admin\Documents\Quest Games\BasicNeedsLib\TestGame.aslx");
            ////Console.WriteLine(questGameTree.ToStringTree(questGameParser));
            Console.ReadKey();
        }
    }
}
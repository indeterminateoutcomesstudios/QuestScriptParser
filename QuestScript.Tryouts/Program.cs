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

            var gameObjectResolver = new GameObjectResolver(@"C:\Program Files (x86)\Quest 5\Core\CoreFunctions.aslx");
            ////Console.WriteLine(questGameTree.ToStringTree(questGameParser));
            Console.ReadKey();
        }
    }
}
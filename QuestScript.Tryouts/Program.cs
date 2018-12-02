using System;
using QuestScript.Interpreter;

namespace QuestScript.Tryouts
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var gameObjectResolver = new GameObjectResolver(@"C:\Program Files (x86)\Quest 5\Core\CoreFunctions.aslx");
            ////Console.WriteLine(questGameTree.ToStringTree(questGameParser));
            Console.ReadKey();
        }
    }
}
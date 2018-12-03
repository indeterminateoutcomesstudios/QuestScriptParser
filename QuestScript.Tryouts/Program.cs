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
            var parser = new QuestScriptParser(new CommonTokenStream(new QuestScriptLexer(new AntlrInputStream("x = default switch(a) { case(1) { x = default }  default { default = x }} x = default x = default switch(a) { case(1) { x = default }  default { default = x }} x = default x = default switch(a) { case(1) { x = default }  default { default = x }} x = default"))));

            Console.WriteLine(parser.script().ToStringTree(parser));
            //var gameObjectResolver = new GameObjectResolver(@"C:\Program Files (x86)\Quest 5\Core\CoreFunctions.aslx");
            Console.ReadKey();
        }
    }
}
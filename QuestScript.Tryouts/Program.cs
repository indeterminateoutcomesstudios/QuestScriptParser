using System;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using QuestScript.Parser.Helpers;
using QuestScript.Parser.Tokens;
using QuestScript.Parser.Types;

namespace QuestScript.Tryouts
{
     class Program
    {
        static void Main(string[] args)
        {

            var result = Convert.ChangeType("null", typeof(object));
            Console.ReadKey();
        }
    }
}
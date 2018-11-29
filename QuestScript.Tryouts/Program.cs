using System;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
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
            //            var script = @"
            //   <![CDATA[
            //    if ((GetBoolean(object, \""isopen\"") or GetBoolean(object, \""transparent\"")) and GetBoolean(object, \""listchildren\"")) {
            //      if (GetBoolean(object, \""hidechildren\"")) {
            //        object.hidechildren = false
            //      }
            //      if (HasString(object, \""listchildrenprefix\"")) {
            //        listprefix = object.listchildrenprefix
            //      }
            //      else {
            //        listprefix = DynamicTemplate(\""ObjectContains\"", object)
            //      }
            //      list = FormatObjectList(listprefix, object, Template(\""And\""), \"".\"")
            //      if (list <> \""\"") {
            //        msg (list)
            //      }
            //    }
            //    ]]>
            //";

            //            var cleanedScript = Regex.Unescape(script);
            //            var lexer = new QuestScriptLexer(new AntlrInputStream(cleanedScript));
            //            var tokens = new CommonTokenStream(lexer);
            //            var parser = new QuestScriptParser(tokens);

            //            var environmentTreeBuilder = new ScriptEnvironmentBuilder();
            //            var scriptTree = parser.script();
            //            environmentTreeBuilder.Visit(scriptTree);

            //            var envTree = environmentTreeBuilder.Output;

            //var questGameLexer = new GameScriptLexer(new AntlrFileStream(@"C:\Users\orev.HRHINOS\Documents\Quest Games\TestGame\TestGame.aslx"));
            //var test = questGameLexer.GetAllTokens().ToList();
            //var questGameParser = new QuestGameParser(new CommonTokenStream(questGameLexer));

            //var questGameTree = questGameParser.document();
            //var gameObjectResolver = new GameObjectResolverVisitor();
            //gameObjectResolver.Visit(questGameTree);
            ////Console.WriteLine(questGameTree.ToStringTree(questGameParser));
            //Console.ReadKey();
        }
    }
}
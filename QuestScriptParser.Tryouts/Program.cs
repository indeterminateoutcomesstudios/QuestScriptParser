using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace QuestScriptParser.Tryouts
{
    class Program
    {
        protected static QuestScriptParser GenerateParserForScript(string script)
        {
            var lexer = new QuestScriptLexer(new AntlrInputStream(script));
            //var collectorTokenSource = new CollectorTokenSource(lexer);
            var tokens = new CommonTokenStream(lexer);
            var parser = new QuestScriptParser(tokens);
            return parser;
        }

        static void Main(string[] args)
        {
            var parser = GenerateParserForScript(@" firsttime { x.a=>
{ someFunc(""Foo!!"", 3, 5.32, ""a"", [3  ,  4, 5    ]) obj.member = foo()
           
        if(foo.bar(3.14) > z)
           DoCoolStuff()
    if( x = foo.bar(12) and  


func1(func2(func3())) > x.foobar(""b""))
//mya mya
{
DoSomeOtherCoolStuff()
AndSomeMoreStuff()
}
MegaStuff()
}
}

");
            
            var scriptTree = parser.script();

            var treeToStringVisitor = new StringQuestScriptVisitor();
            treeToStringVisitor.Visit(scriptTree);
            var formattedCode = treeToStringVisitor.Output;

            Console.WriteLine(formattedCode);
        }
    }
}

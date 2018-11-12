using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using QuestScript.Interpreter;
using QuestScript.Parser;

namespace QuestScript.Tryouts
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
            var parser = GenerateParserForScript(@" 
foo()  bar() x = 3

firsttime { x.a=>
{ someFunc(""Foo!!"", 3, 5.32, ""a"", [3  ,  4, 5    ]) obj.member = foo()
           
        if(foo.bar(3.14) > z)
           DoCoolStuff()
elseif (x = 4)
    y = 5
else
aaa()
    if( x = foo.bar(12) and  


func1(func2(func3())) > x.foobar(""b""))
//mya mya
{
DoSomeOtherCoolStuff()  AndSomeMoreStuff()

}
elseif(x < 4){
        abc()
}
else{
    myaa()
}

MegaStuff()
}
}

while(not(x>     5)   and (                    x <= y     )){
    foo.bar = xyz(3,   4,5)
}

switch(x != 3 and y > foobar()){
    case(3){
    zz = 234.44
}
            case(234)
    foobar()
default{
    ask(""why?""){ xyz = 3 }
}
}
");
            
            var scriptTree = parser.script();

            var treeToStringVisitor = new StringQuestScriptVisitor();
            treeToStringVisitor.Visit(scriptTree);
            var formattedCode = treeToStringVisitor.Output;         

            Console.WriteLine(formattedCode);
            Console.WriteLine(CSharpScript.EvaluateAsync<bool>("4 == 5").Result);
        }
    }
}

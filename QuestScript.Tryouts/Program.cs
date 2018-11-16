using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using QuestScript.Interpreter;
using QuestScript.Interpreter.ScriptElements;
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
//            var parser = GenerateParserForScript(@" 
//foo()  bar() x = 3 y = 1231231 z = 5

//firsttime { x.a=>
//{ someFunc(""Foo!!"", 3, 5.32, ""a"", [3  ,  4, 5    ]) obj.member = foo()
           
//        if(foo.bar(3.14) > z)
//           DoCoolStuff()
//elseif (x = 4)
//    y = 5.34
//else
//aaa()
//    if( x = foo.bar(12) and  


//func1(func2(func3())) > x.foobar(""b""))
////mya mya
//{
//DoSomeOtherCoolStuff()  AndSomeMoreStuff()

//}
//elseif(x < 4){
//        abc()
//}
//else{
//    myaa()
//}

//MegaStuff()
//}
//}

//while(not(x>     5)   and (                    x <= y     )){
//    foo.bar = xyz(3,   4,5)
//}

//switch(x != 3 and y > foobar()){
//    case(3){
//    zz = 234.44
//    for(i,0,5){
//        x++
//    }
//}
//            case(234)
//    foobar()
//default{
//    ask(""why?""){ xyz = 3 }
//}
//}
//");
            var parser = GenerateParserForScript(@" 
                x = 4
        y = 3.13
    str = ""XYZ""
                if(obj.member = 4) {                    
                    x = foo.bar
                }  
            ");
            var scriptTree = parser.script();
            //var treeToStringVisitor = new CodeFormattingVisitor();
            //treeToStringVisitor.Visit(scriptTree);
            //var formattedCode = treeToStringVisitor.Output;

            //Console.WriteLine(formattedCode);

            var syntaxValidator = new SyntaxValidationVisitor
            {
                DeclaredObjects = new List<IObject>
                {
                    new ObjectInfo("obj",ObjectType.Object,"TestObjType", new List<ObjectInfo>{ new ObjectInfo("member",ObjectType.Double,"Double") }),
                    new ObjectInfo("foo",ObjectType.Object,"FooBarType", new List<ObjectInfo>{ new ObjectInfo("bar",ObjectType.String,"String") })
                }
            };
            syntaxValidator.Visit(scriptTree);
            foreach (var kvp in syntaxValidator.SymbolsPerContextScope)
            {
                if (kvp.Key is QuestScriptParser.ScriptContext)
                {
                    Console.WriteLine($"root context => {string.Join(",", kvp.Value)}");
                }
                else
                {
                    var text = kvp.Key.GetText();
                    Console.WriteLine($"{text.Substring(0,Math.Min(text.Length,20))}... => {string.Join(",", kvp.Value)}");
                }
            }

            foreach (var ex in syntaxValidator.ValidationExceptions)
            {
                Console.WriteLine(ex.Message);
            }

            //var testScript = CSharpScript.Create<bool>("4 == 5");
            //Console.WriteLine(testScript.RunAsync().Result.ReturnValue);
            Console.ReadKey();
        }
    }
}

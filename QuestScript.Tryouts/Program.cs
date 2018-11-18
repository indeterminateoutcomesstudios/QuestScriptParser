using System;
using System.Linq;
using Antlr4.Runtime;
using QuestScript.Interpreter;
using QuestScript.Parser;

namespace QuestScript.Tryouts
{
     class Program
    {
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
            var lexer = new QuestScriptLexer(new AntlrInputStream(@" 
                x = 4 
                z = 3
                if(x > 0 and z = 5)
                {
                    y = 4.44
                    x = 4 + (5 * y) - z
                }
            "));
            //var collectorTokenSource = new CollectorTokenSource(lexer);
            var tokens = new CommonTokenStream(lexer);
            var parser = new QuestScriptParser(tokens);
            var environmentTreeBuilder = new EnvironmentTreeBuilder();
            parser.AddParseListener(environmentTreeBuilder);
            var scriptTree = parser.script();
            //Console.WriteLine(scriptTree.ToStringTree(parser));

            foreach (var msg in environmentTreeBuilder.Errors.Select(e => e.Message))
                Console.WriteLine(msg);

            Console.ReadKey();
        }
    }
}

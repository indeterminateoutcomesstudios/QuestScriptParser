using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using QuestScriptParser;
using Xunit;

namespace QuestScript.Tests
{
    public class StatementTests : BaseParserTest
    {
        [Fact]
        public void Can_parse_if()
        {
            var parser = GenerateParserForScript(@"
                if(x = 5){
                    foobarFunc()
                }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (ifStatement if (ifConditionStatement ( (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 5)))))) )) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression foobarFunc) (arguments ( ))))))) }))))) <EOF>)";  
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_if_else()
        {
            var parser = GenerateParserForScript(@"
                if(x = 5)
                {
                    foobarFunc()
                }
                else{
                    x = 4
                }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (ifStatement if (ifConditionStatement ( (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 5)))))) )) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression foobarFunc) (arguments ( ))))))) })) (elseStatement else (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 4))))))))) })))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_if_elseif_else()
        {
            var parser = GenerateParserForScript(@"
                if(x = 5)
                {
                    foobarFunc()
                }
                elseif( x < 3){
                    otherFooBarFunc()
                }
                else{
                    x = 4
                }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (ifStatement if (ifConditionStatement ( (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 5)))))) )) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression foobarFunc) (arguments ( ))))))) })) (elseIfStatement elseif (ifConditionStatement ( (expressionSequence (singleExpression (singleExpression x) < (singleExpression (literal (numericLiteral (integerLiteral 3)))))) )) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression otherFooBarFunc) (arguments ( ))))))) }))) (elseStatement else (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 4))))))))) })))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_wait()
        {
            var parser = GenerateParserForScript(@"
                wait
                {
                    x = 4
                }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (waitStatement wait (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 4))))))))) }))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_switch_case()
        {
            var parser = GenerateParserForScript(@"
                switch(x++)
                {
                    case(5){
                        x = 4
                    }
                    case(3){
                        x = 2
                    }
                    case(2){
                        x = 6
                    }
                    default{
                        x--
                    }                    
                }
            ");
            parser.BuildParseTree = true;
            var scriptTree = parser.script();

            var treeToStringVisitor = new StringQuestScriptVisitor();
            treeToStringVisitor.Visit(scriptTree);
            var test = treeToStringVisitor.Output;

            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (switchCaseStatement (switchStatement switch ( (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) ++)))) )) { (caseStatement case ( (literal (numericLiteral (integerLiteral 5))) ) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 4))))))))) }))) (caseStatement case ( (literal (numericLiteral (integerLiteral 3))) ) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 2))))))))) }))) (caseStatement case ( (literal (numericLiteral (integerLiteral 2))) ) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 6))))))))) }))) (defaultStatement default (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) --))))) }))) }))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Switch_case_should_not_support_non_literals_in_case_statements()
        {
            var parser = GenerateParserForScript(@"
                switch(x++)
                {
                    case(3){
                        x = 4
                    }
                    case(z){
                        x = 2
                    }
                    case(2){
                        x = 6
                    }
                    default{
                        x--
                    }                    
                }
            ");
            
            var _ = parser.script();
            
            // because of 'case(z)' this should fall, since 'z' is an identifier
            Assert.Equal(1,parser.NumberOfSyntaxErrors);
        }

        [Fact]
        public void Switch_case_should_not_support_function_calls_in_case_statements()
        {
            var parser = GenerateParserForScript(@"
                switch(x++)
                {
                    case(3){
                        x = 4
                    }
                    case(foo()){
                        x = 2
                    }
                    case(2){
                        x = 6
                    }
                    default{
                        x--
                    }                    
                }
            ");
            
            var _ = parser.script();
            Assert.Equal(3,parser.NumberOfSyntaxErrors);
        }

        [Fact]
        public void Can_parse_firsttime()
        {
            var parser = GenerateParserForScript(@"
                firsttime
                {
                    foobarFunc()
                }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (firstTimeStatement firsttime (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression foobarFunc) (arguments ( ))))))) }))))) <EOF>)"; 
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_firsttime_otherwise()
        {
            var parser = GenerateParserForScript(@"
                firsttime
                {
                    foobarFunc()
                }
                otherwise{
                    someOtherFoobarFunc()
                }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (firstTimeStatement firsttime (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression foobarFunc) (arguments ( ))))))) })) otherwise (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression someOtherFoobarFunc) (arguments ( ))))))) }))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_for()
        {
            var parser = GenerateParserForScript(@"
                for(x,3,4){
                    y++
                }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (iterationStatement for ( x , 3 , 4 ) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression y) ++))))) }))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_do_loop()
        {
            var parser = GenerateParserForScript(@"
               y = 0
               do{
                    y++
               }while (y < 10)
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression y) = (singleExpression (literal (numericLiteral (integerLiteral 0)))))))) (statement (iterationStatement do (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression y) ++))))) })) while ( (expressionSequence (singleExpression (singleExpression y) < (singleExpression (literal (numericLiteral (integerLiteral 10)))))) )))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_while_loop()
        {
            var parser = GenerateParserForScript(@"
               y = 0
               while (y < 10){
                    y++
               }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression y) = (singleExpression (literal (numericLiteral (integerLiteral 0)))))))) (statement (iterationStatement while ( (expressionSequence (singleExpression (singleExpression y) < (singleExpression (literal (numericLiteral (integerLiteral 10)))))) ) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression y) ++))))) }))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_foreach()
        {
            var parser = GenerateParserForScript(@"
                x = """"
                foreach(item : collection){
                    x += item + "" ""
                }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (stringLiteral \"\"))))))) (statement (iterationStatement foreach ( item : collection ) (statement (block { (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) (assignmentOperator +=) (singleExpression (singleExpression item) + (singleExpression (literal (stringLiteral \" \"))))))))) }))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_break_and_continue()
        {
            var parser = GenerateParserForScript(@"
                x = """"
                foreach(item : collection){
                    if(item = ""foo"")
                        continue

                    if(item = ""bar"")
                        break

                    x += item + "" ""
                }
            ");
            var scriptTree = parser.script();
            
            Assert.Null(scriptTree.exception);
            var expectedTree =
                "(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (stringLiteral \"\"))))))) (statement (iterationStatement foreach ( item : collection ) (statement (block { (statementList (statement (ifStatement if (ifConditionStatement ( (expressionSequence (singleExpression (singleExpression item) = (singleExpression (literal (stringLiteral \"foo\"))))) )) (statement (continueStatement continue)))) (statement (ifStatement if (ifConditionStatement ( (expressionSequence (singleExpression (singleExpression item) = (singleExpression (literal (stringLiteral \"bar\"))))) )) (statement (breakStatement break)))) (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) (assignmentOperator +=) (singleExpression (singleExpression item) + (singleExpression (literal (stringLiteral \" \"))))))))) }))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }
    }
}

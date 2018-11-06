using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using QuestScriptParser;
using Antlr4.Runtime;

namespace QuestScript.Tests
{   
    public class BasicTests
    {        
        [Fact]
        public void Can_parse_int_assignment()
        {
            var parser = GenerateParserForScript("x = 12\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree =
                @"(script 
                    (statementList 
                        (statement 
                            (expressionStatement 
                                (expressionSequence 
                                    (singleExpression 
                                        (singleExpression x) = 
                                    (singleExpression 
                                        (literal 
                                            (numericLiteral 
                                                (integerLiteral 12)))))) 
                    (endOfLine <EOF>)))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_double_assignment()
        {
            var parser = GenerateParserForScript("x = 12.4\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree =
                @"(script 
                    (statementList 
                        (statement 
                            (expressionStatement 
                                (expressionSequence 
                                    (singleExpression 
                                        (singleExpression x) = 
                                    (singleExpression 
                                        (literal 
                                            (numericLiteral 
                                                (doubleLiteral 12.4)))))) 
                    (endOfLine <EOF>)))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_string_assignment()
        {
            var parser = GenerateParserForScript("x = \"FooBar!\"\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (stringLiteral \"FooBar!\"))))) (endOfLine <EOF>)))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_function_call_without_parameters()
        {
            var parser = GenerateParserForScript("foobarFunc()");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression foobarFunc) (arguments ( )))) (endOfLine <EOF>)))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_function_call_with_parameters()
        {
            var parser = GenerateParserForScript("foobarFunc(4,3,\"mega string!\")");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression foobarFunc) (arguments ( (singleExpression (literal (numericLiteral (integerLiteral 4)))) , (singleExpression (literal (numericLiteral (integerLiteral 3)))) , (singleExpression (literal (stringLiteral \"mega string!\"))) )))) (endOfLine <EOF>)))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        private static QuestScriptParser.QuestScriptParser GenerateParserForScript(string script)
        {
            var lexer = new QuestScriptLexer(new AntlrInputStream(script));
            var tokens = new CommonTokenStream(lexer);
            var parser = new QuestScriptParser.QuestScriptParser(tokens);
            return parser;
        }
    }
}

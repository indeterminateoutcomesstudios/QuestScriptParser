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
    public class BasicTests : BaseParserTest
    {        
        [Fact]
        public void Can_parse_int_assignment()
        {
            var parser = GenerateParserForScript("x = 12\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree = "(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (integerLiteral 12))))))))) <EOF>)";            
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_member_assignment()
        {
            var parser = GenerateParserForScript("x.foo = 12\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree = "(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression (singleExpression x) . (identifierName foo)) = (singleExpression (literal (numericLiteral (integerLiteral 12))))))))) <EOF>)";            
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_script_assignment()
        {
            var parser = GenerateParserForScript("scriptObj => { foobarFunc(1,2,3); }\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree = "(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression scriptObj) => (singleExpression { foobarFunc))))) (statement (expressionStatement (expressionSequence (singleExpression ( (expressionSequence (singleExpression (literal (numericLiteral (integerLiteral 1)))) , (singleExpression (literal (numericLiteral (integerLiteral 2)))) , (singleExpression (literal (numericLiteral (integerLiteral 3))))) )))))) } <EOF>)";            
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_double_assignment()
        {
            var parser = GenerateParserForScript("x = 12.4\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (numericLiteral (doubleLiteral 12.4))))))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_string_assignment()
        {
            var parser = GenerateParserForScript("x = \"FooBar!\"\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression x) = (singleExpression (literal (stringLiteral \"FooBar!\")))))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_array_element_assignment()
        {
            var parser = GenerateParserForScript("x[34] = \"FooBar!\"\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression (singleExpression x) [ (expressionSequence (singleExpression (literal (numericLiteral (integerLiteral 34))))) ]) = (singleExpression (literal (stringLiteral \"FooBar!\")))))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_array_element_assignment_with_calculated_indexer()
        {
            var parser = GenerateParserForScript("x[foo() + 4] = \"FooBar!\"\n");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression (singleExpression x) [ (expressionSequence (singleExpression (singleExpression (singleExpression foo) (arguments ( ))) + (singleExpression (literal (numericLiteral (integerLiteral 4)))))) ]) = (singleExpression (literal (stringLiteral \"FooBar!\")))))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_function_call_without_parameters()
        {
            var parser = GenerateParserForScript("foobarFunc()");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression foobarFunc) (arguments ( ))))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }


        [Fact]
        public void Can_parse_boolean_expression()
        {
            var parser = GenerateParserForScript("not ((foo = 4 and not x != 5) or foobar() > y) and z[5 * 6] <= 4");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression (singleExpression (singleExpression not (singleExpression ( (expressionSequence (singleExpression (singleExpression (singleExpression (singleExpression ( (expressionSequence (singleExpression (singleExpression foo) = (singleExpression (singleExpression (literal (numericLiteral (integerLiteral 4)))) and (singleExpression (singleExpression not (singleExpression x)) != (singleExpression (literal (numericLiteral (integerLiteral 5)))))))) )) or (singleExpression foobar)) (arguments ( ))) > (singleExpression y))) ))) and (singleExpression z)) [ (expressionSequence (singleExpression (singleExpression (literal (numericLiteral (integerLiteral 5)))) * (singleExpression (literal (numericLiteral (integerLiteral 6)))))) ]) <= (singleExpression (literal (numericLiteral (integerLiteral 4))))))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }

        [Fact]
        public void Can_parse_function_call_with_parameters()
        {
            var parser = GenerateParserForScript("foobarFunc(4,3,\"mega string!\")");
            var scriptTree = parser.script();

            Assert.Null(scriptTree.exception);
            var expectedTree ="(script (statementList (statement (expressionStatement (expressionSequence (singleExpression (singleExpression foobarFunc) (arguments ( (singleExpression (literal (numericLiteral (integerLiteral 4)))) , (singleExpression (literal (numericLiteral (integerLiteral 3)))) , (singleExpression (literal (stringLiteral \"mega string!\"))) ))))))) <EOF>)";
            var generatedTree = scriptTree.ToStringTree(parser);
            ParseTreeHelper.AssertTreesAreEqual(expectedTree,generatedTree);
        }           
    }
}

using System;
using System.Diagnostics;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace QuestScriptParser
{
    public class StringQuestScriptVisitor : QuestScriptBaseVisitor<bool>
    {
        private int _currentIndentation = 0;
        private string Whitespaces =>  new string(' ',_currentIndentation * 2);
        private readonly StringBuilder _output = new StringBuilder();
        public string Output => _output.ToString();

        public override bool VisitBlockStatement(QuestScriptParser.BlockStatementContext context)
        {
            _output.AppendLine($"{Environment.NewLine}{Whitespaces}{{");
            _currentIndentation++;
            VisitStatementList(context.blockStatements);
            _currentIndentation--;
            _output.Append($"{Whitespaces}}}{Environment.NewLine}");

            return true;
        }

        public override bool VisitCodeBlock(QuestScriptParser.CodeBlockContext context)
        {
            _currentIndentation++;
            _output.Append($"{Environment.NewLine}{Whitespaces}");
            base.VisitCodeBlock(context);
            _currentIndentation--;
            return true;
        }

        public override bool VisitScriptAssignmentExpression(QuestScriptParser.ScriptAssignmentExpressionContext context)
        {
            _output.AppendFormat("{0} {1} ",context.lvalue.GetText(), context.ScriptAssignToken().GetText());
            VisitStatement(context.rvalue); //this will continue "printing" the statement            
            return true;
        }

        public override bool VisitRelationalExpression(QuestScriptParser.RelationalExpressionContext context)
        {
            return PrintBooleanExpression(context.lvalue, context.op.Text, context.rvalue);
        }

        public override bool VisitLogicalAndExpression(QuestScriptParser.LogicalAndExpressionContext context)
        {
            return PrintBooleanExpression(context.lvalue, context.AndToken().Symbol.Text, context.rvalue);
        }

        public override bool VisitLogicalOrExpression(QuestScriptParser.LogicalOrExpressionContext context)
        {
            return PrintBooleanExpression(context.lvalue, context.OrToken().Symbol.Text, context.rvalue);
        }

        public override bool VisitMultiplicativeExpression(QuestScriptParser.MultiplicativeExpressionContext context)
        {
            return PrintBooleanExpression(context.lvalue, context.op.Text, context.rvalue);
        }

        public override bool VisitAdditiveExpression(QuestScriptParser.AdditiveExpressionContext context)
        {
            return PrintBooleanExpression(context.lvalue, context.op.Text, context.rvalue);
        }

        private bool PrintBooleanExpression(QuestScriptParser.SingleExpressionContext lvalue, string op,
            QuestScriptParser.SingleExpressionContext rvalue)
        {    
            base.VisitSingleExpression(lvalue);
            _output.Append($" {op} ");
            base.VisitSingleExpression(rvalue);

            var str = _output.ToString();
            return true;
        }

        public override bool VisitStatementList(QuestScriptParser.StatementListContext context)
        {
            bool isFirst = true;
            for (int i = 0; i < context._statements.Count; i++)
            {
                if (isFirst)
                {
                    _output.Append(Whitespaces);
                    isFirst = false;
                }
                else
                {
                    _output.AppendFormat("{0}{1}", Environment.NewLine, Whitespaces);
                }

                base.VisitStatement(context._statements[i]);
            }

            _output.AppendLine();
            return true;
        }

      
        public override bool VisitAssignmentOrEqualityExpression(QuestScriptParser.AssignmentOrEqualityExpressionContext context)
        {
            base.VisitSingleExpression(context.lvalue);
            _output.Append(" = ");
            base.VisitSingleExpression(context.rvalue);

            return true;
        }

        public override bool VisitChildren(IRuleNode node)
        {
            int childCount = node.ChildCount;
            bool _ = true;
            for (int i = 0; i < childCount && this.ShouldVisitNextChild(node, _); ++i)
            {
                var child = node.GetChild(i);
                if (child.ChildCount == 0)
                {
                    var value = child.GetText();
                    if(value != "<EOF>")
                        _output.Append(value);
                }
                else
                {
                    child.Accept(this);
                }
            }
            return true;
        }

        public override bool VisitFunctionCallExpression(QuestScriptParser.FunctionCallExpressionContext context)
        {
            base.VisitSingleExpression(context.functionExpression); //print whatever this resolves to
            _output.Append('(');

            var args = context.arguments()._argumentExpressions;
            for (var index = 0; index < args.Count - 1; index++)
            {
                base.VisitSingleExpression(args[index]);
                _output.Append(", ");
            }
            
            if(args.Count > 0)
                base.VisitSingleExpression(args[args.Count - 1]);

            _output.Append(')');
            return true;
        }

        public override bool VisitArrayLiteral(QuestScriptParser.ArrayLiteralContext context)
        {
            var elements = context.values._elements;
            _output.Append('[');
            for (int i = 0; i < elements.Count - 1; i++)
            {
                base.VisitSingleExpression(elements[i]);
                _output.Append(", ");
            }

            if(elements.Count > 0)
                base.VisitSingleExpression(elements[elements.Count - 1]);

            _output.Append(']');
            return true;
        }
    }
}
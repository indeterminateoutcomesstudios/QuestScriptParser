using System;
using System.Diagnostics;
using System.Linq;
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
            var hasCodeBlockAsParent = context.Parent is QuestScriptParser.CodeBlockContext;
            _output.AppendLine($"{(!hasCodeBlockAsParent ? Environment.NewLine : string.Empty)}{Whitespaces}{{");
            
            if(!hasCodeBlockAsParent)
                _currentIndentation++;
            VisitStatementList(context.blockStatements);
            if(!hasCodeBlockAsParent)
                _currentIndentation--;
            _output.Append($"{Whitespaces}}}");

            return true;
        }

        public override bool VisitCodeBlock(QuestScriptParser.CodeBlockContext context)
        {
            var hasBlockStatementAsChild = context.children.Any(x => x is QuestScriptParser.BlockStatementContext);
            if(!hasBlockStatementAsChild)
                _currentIndentation++;
            _output.Append($"{(hasBlockStatementAsChild ? Environment.NewLine : string.Empty)}{Whitespaces}");
            base.VisitCodeBlock(context);
            if(!hasBlockStatementAsChild)
                _currentIndentation--;
            return true;
        }

        public override bool VisitScriptAssignmentExpression(QuestScriptParser.ScriptAssignmentExpressionContext context)
        {
            context.lvalue.Accept(this);
            _output.AppendFormat(" {0} ",context.ScriptAssignToken().GetText());
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
            lvalue.Accept(this);
            _output.Append($" {op} ");
            rvalue.Accept(this);
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

                context._statements[i].Accept(this);
            }

            _output.AppendLine();
            return true;
        }

      
        public override bool VisitAssignmentOrEqualityExpression(QuestScriptParser.AssignmentOrEqualityExpressionContext context)
        {
            return PrintBooleanExpression(context.lvalue,"=",context.rvalue);
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
                args[index].Accept(this);
                _output.Append(", ");
            }
            
            if(args.Count > 0)
                args[args.Count - 1].Accept(this);

            _output.Append(')');
            return true;
        }

        public override bool VisitArrayLiteral(QuestScriptParser.ArrayLiteralContext context)
        {
            var elements = context.values._elements;
            _output.Append('[');
            for (int i = 0; i < elements.Count - 1; i++)
            {
                elements[i].Accept(this);
                _output.Append(", ");
            }

            if(elements.Count > 0)
                elements[elements.Count - 1].Accept(this);


            _output.Append(']');
            return true;
        }
    }
}
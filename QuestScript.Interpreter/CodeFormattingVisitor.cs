using System;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using QuestScript.Parser;

namespace QuestScript.Interpreter
{
    //TODO: figure out how NOT to delete comments (currently they are output to HIDDEN channel by the lexer)
    public class CodeFormattingVisitor : QuestScriptBaseVisitor<bool>
    {
        private int _currentIndentation;
        private string Whitespaces => new string(' ', _currentIndentation);
        private readonly StringBuilder _output = new StringBuilder();
        public string Output => _output.ToString();

        public override bool VisitStatement(QuestScriptParser.StatementContext context)
        {
            _output.Append(Whitespaces);
            VisitChildren(context);

            _output.AppendLine();
            return true;
        }

        public override bool VisitSwitchCaseStatement(QuestScriptParser.SwitchCaseStatementContext context)
        {
            _output.Append("switch (");
            context.switchConditionStatement.Accept(this);
            _output.AppendFormat("){0}{{{0}",Environment.NewLine);
            _currentIndentation++;

            foreach (var caseStatement in context._cases)
            {
                caseStatement.Accept(this);
            }

            context.defaultContext?.Accept(this);

            _currentIndentation--;

            _output.AppendFormat("}}{0}", Environment.NewLine);
            return true;
        }

        public override bool VisitCaseStatement(QuestScriptParser.CaseStatementContext context)
        {
            _output.AppendFormat("{1}case ({0}", context.caseValue.GetText(),Whitespaces);

            if (context.code.codeBlockStatement() == null)
            {
                _output.AppendFormat("){0}{1}", Environment.NewLine, Whitespaces);
            }
            else
            {
                _output.AppendFormat("){0}", Whitespaces);
            }

            context.code.Accept(this);

            return true;
        }

        public override bool VisitDefaultStatement(QuestScriptParser.DefaultStatementContext context)
        {
            _output.AppendFormat("{0}default", Whitespaces);
            if (context.code.codeBlockStatement() == null)
            {
                _output.AppendFormat("{0}{1}", Environment.NewLine, Whitespaces);
            }
            else
            {
                _output.Append(Whitespaces);
            }

            context.code.Accept(this);
            return true;
        }

        public override bool VisitIfStatement(QuestScriptParser.IfStatementContext context)
        {
            _output.Append("if(");
            context.condition.Accept(this);
            if (context.ifCode.codeBlockStatement() == null)
            {
                _output.AppendFormat("){0}{1}", Environment.NewLine, Whitespaces);
            }
            else
            {
                _output.AppendFormat("){0}", Whitespaces);
            }
            context.ifCode.Accept(this);

            if (context._elseIfCodes.Count > 0)
            {
                for (var i = 0; i < context._elseIfCodes.Count; i++)
                {
                    var ifElseCondition = context._elseifConditions[i];
                    var ifElseCode = context._elseIfCodes[i];

                    _output.AppendFormat("{0}elseif(", Whitespaces);
                    ifElseCondition.Accept(this);

                    if (ifElseCode.codeBlockStatement() == null)
                    {
                        _output.AppendFormat("){0}{1}", Environment.NewLine, Whitespaces);
                    }
                    else
                    {
                        _output.AppendFormat("){0}", Whitespaces);
                    }

                    ifElseCode.Accept(this);

                }
            }

            if (context.elseCode != null)
            {
                _output.AppendFormat("{0}else", Whitespaces);
                if (context.elseCode.codeBlockStatement() == null)
                {
                    _output.AppendFormat("{0}{1}", Environment.NewLine, Whitespaces);
                }
                else
                {
                    _output.AppendFormat("{0}", Whitespaces);
                }
                context.elseCode.Accept(this);
            }

            return true;
        }

        public override bool VisitAssignmentStatement(QuestScriptParser.AssignmentStatementContext context)
        {
            context.LVal.Accept(this);
            _output.Append(" = ");
            context.RVal.Accept(this);

            return true;
        }

        public override bool VisitScriptAssignmentStatement(QuestScriptParser.ScriptAssignmentStatementContext context)
        {
            context.LVal.Accept(this);
            _output.Append(" => ");
            context.RVal.Accept(this);

            return true;
        }

        private void PrintBoolean(RuleContext left, string op, RuleContext right)
        {
            left.Accept(this);
            _output.AppendFormat(" {0} ", op);
            right.Accept(this);
        }

        public override bool VisitRelationalExpression(QuestScriptParser.RelationalExpressionContext context)
        {
            PrintBoolean(context.left, context.op.GetText(), context.right);
            return true;
        }

        public override bool VisitLogicalExpression(QuestScriptParser.LogicalExpressionContext context)
        {
            PrintBoolean(context.left, context.op.GetText(), context.right);
            return true;
        }

        public override bool VisitNotExpression(QuestScriptParser.NotExpressionContext context)
        {
            _output.AppendFormat("not ");
            context.expr.Accept(this);
            return true;
        }

        public override bool VisitArithmeticExpression(QuestScriptParser.ArithmeticExpressionContext context)
        {
            PrintBoolean(context.left, context.op.GetText(), context.right);
            return true;
        }

        public override bool VisitCodeBlockStatement(QuestScriptParser.CodeBlockStatementContext context)
        {
            _output.AppendFormat("{1}{0}{{{1}", Whitespaces, Environment.NewLine);
            _currentIndentation++;
            foreach (var ctx in context._statements)
            {
                ctx.Accept(this);
            }

            _currentIndentation--;
            _output.AppendFormat("{0}}}", Whitespaces);
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
                    if (value != "<EOF>")
                        _output.Append(value);
                }
                else
                {
                    child.Accept(this);
                }
            }
            return true;
        }
    }
}
using System;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace QuestScriptParser
{
    public class StringQuestScriptVisitor : QuestScriptBaseVisitor<bool>
    {
        private int _currentIndentation;
        private string Whitespaces =>  new string(' ',_currentIndentation);
        private readonly StringBuilder _output = new StringBuilder();
        public string Output => _output.ToString();

        public override bool VisitStatement(QuestScriptParser.StatementContext context)
        {
            _output.AppendLine();
            VisitChildren(context);
            return true;
        }

        public override bool VisitCodeBlockStatement(QuestScriptParser.CodeBlockStatementContext context)
        {
            _output.AppendFormat("{0}{1}{{",Whitespaces,Environment.NewLine);
            _currentIndentation++;

            foreach (var ctx in context._statements)
            {
                ctx.Accept(this);
            }

            _currentIndentation--;
            _output.AppendFormat("{0}{1}}}{1}",Whitespaces,Environment.NewLine);
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

        private bool HasAnyChildOfType<TChild>(ParserRuleContext context)
            where TChild : ParserRuleContext
        {
            if (context == null)
                return false;
            foreach (var child in context.children)
            {
                if (child.GetType().Name == typeof(TChild).Name)
                    return true;
                if (child.GetType().Name == typeof(ParserRuleContext).Name)
                    return false;
                if (HasAnyChildOfType<TChild>(child as ParserRuleContext))
                    return true;
            }

            return false;
        }
    }
}
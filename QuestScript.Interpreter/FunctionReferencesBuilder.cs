using System.Collections.Generic;
using QuestScript.Parser;

namespace QuestScript.Interpreter
{
    public class FunctionReferencesBuilder : QuestScriptBaseVisitor<HashSet<string>>
    {
        private readonly HashSet<string> _references = new HashSet<string>();

        protected override HashSet<string> DefaultResult => _references;

        public override HashSet<string> VisitFunctionOperand(QuestScriptParser.FunctionOperandContext context)
        {
            _references.Add(context.expr.functionName.Text);
            base.VisitFunctionOperand(context);
            return _references;
        }

        public override HashSet<string> VisitFunctionStatement(QuestScriptParser.FunctionStatementContext context)
        {
            _references.Add(context.functionName.Text);
            base.VisitFunctionStatement(context);
            return _references;
        }

        public void Reset() => _references.Clear();
    }
}
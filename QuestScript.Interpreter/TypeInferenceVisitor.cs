using QuestScript.Interpreter.ScriptElements;
using QuestScript.Parser;

namespace QuestScript.Interpreter
{
    public class TypeInferenceVisitor : QuestScriptBaseVisitor<ObjectType>
    {
        private readonly EnvironmentTreeBuilder _environmentBuilder;

        public TypeInferenceVisitor(EnvironmentTreeBuilder environmentBuilder)
        {
            _environmentBuilder = environmentBuilder;
        }

        public override ObjectType VisitIdentifierOperand(QuestScriptParser.IdentifierOperandContext context)
        {
            //this is either an object or a variable identifier.
            var identifier = context.GetText();
            var variable = _environmentBuilder.GetVariableFromCurrentEnvironment(identifier);
            
            if (variable != null) //so this is variable...
            {
                return variable.Type;
            }
            //TODO: add here the resolving types of an object if identifier is an object (which are global in Quest)

            return base.VisitIdentifierOperand(context);
        }

        public override ObjectType VisitRelationalExpression(QuestScriptParser.RelationalExpressionContext context)
        {
            if (context.left.Accept(this) != ObjectType.Boolean)
                return ObjectType.Unknown;
            
            if (context.right.Accept(this) != ObjectType.Boolean)
                return ObjectType.Unknown;

            return ObjectType.Boolean;
        }

        public override ObjectType VisitLogicalExpression(QuestScriptParser.LogicalExpressionContext context)
        {
            if (context.left.Accept(this) != ObjectType.Boolean)
                return ObjectType.Unknown;
            
            if (context.right.Accept(this) != ObjectType.Boolean)
                return ObjectType.Unknown;

            return ObjectType.Boolean;
        }

        public override ObjectType VisitNotExpression(QuestScriptParser.NotExpressionContext context)
        {
            return context.expr.Accept(this) != ObjectType.Boolean ? 
                ObjectType.Unknown : ObjectType.Boolean;
        }

        public override ObjectType VisitIntegerLiteral(QuestScriptParser.IntegerLiteralContext context)
        {
            return ObjectType.Integer;
        }

        public override ObjectType VisitDoubleLiteral(QuestScriptParser.DoubleLiteralContext context)
        {
            return ObjectType.Double;
        }

        public override ObjectType VisitStringLiteral(QuestScriptParser.StringLiteralContext context)
        {
            return ObjectType.String;
        }

        public override ObjectType VisitNullLiteral(QuestScriptParser.NullLiteralContext context)
        {
            return ObjectType.Null;
        }

        public override ObjectType VisitBooleanLiteral(QuestScriptParser.BooleanLiteralContext context)
        {
            return ObjectType.Boolean;
        }

        public override ObjectType VisitArrayLiteral(QuestScriptParser.ArrayLiteralContext context)
        {
            return ObjectType.List;
        }
    }
}

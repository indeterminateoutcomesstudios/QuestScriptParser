using QuestScript.Interpreter.Exceptions;
using QuestScript.Interpreter.Helpers;
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

        public override ObjectType VisitParenthesizedExpression(QuestScriptParser.ParenthesizedExpressionContext context) => context.expr.Accept(this);

        

        public override ObjectType VisitArithmeticExpression(QuestScriptParser.ArithmeticExpressionContext context)
        {
            var leftType = context.left.Accept(this);
            var rightType = context.right.Accept(this);

            //if at least one is unknown, then we already have an error and can stop evaluating types
            if (leftType == ObjectType.Unknown || rightType == ObjectType.Unknown)
                return ObjectType.Unknown;

            if (leftType == rightType)
                return leftType;

            if (TypeUtil.CanConvert(rightType, leftType))
                return leftType;

            _environmentBuilder.Errors.Add(new InvalidOperandsException(context,context.op.GetText(), leftType, rightType));

            return ObjectType.Unknown;
        }

        public override ObjectType VisitRelationalExpression(QuestScriptParser.RelationalExpressionContext context)
        {
            var leftType = context.left.Accept(this);
            var rightType = context.right.Accept(this);

            if (TypeUtil.IsComparable(leftType) && TypeUtil.IsComparable(rightType)) 
                return ObjectType.Boolean; //comparison results are boolean of course...

            _environmentBuilder.Errors.Add(new InvalidOperandsException(context,context.op.GetText(),leftType,rightType));
            return ObjectType.Unknown;
        }

        public override ObjectType VisitLogicalExpression(QuestScriptParser.LogicalExpressionContext context)
        {
            var leftType = context.left.Accept(this);
            var rightType = context.right.Accept(this);

            if (leftType == ObjectType.Boolean &&
                rightType == ObjectType.Boolean)
            {
                return ObjectType.Boolean;
            }

            _environmentBuilder.Errors.Add(new InvalidOperandsException(context,context.op.GetText(),leftType,rightType));
            return ObjectType.Unknown;
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

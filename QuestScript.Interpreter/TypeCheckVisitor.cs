using System.Collections.Generic;
using System.Linq;
using QuestScript.Interpreter.ScriptElements;
using QuestScript.Interpreter.ValidationExceptions;
using QuestScript.Parser;

namespace QuestScript.Interpreter
{
    public class TypeCheckVisitor : QuestScriptBaseVisitor<ObjectType>
    {
        public List<InvalidTypeException> TypeCheckExceptions { get; } = new List<InvalidTypeException>();
        public List<IFunction> DeclaredFunctions;
        public List<IObject> DeclaredObjects;

        public override ObjectType VisitFunctionOperand(QuestScriptParser.FunctionOperandContext context)
        {
            var funcInfo =
                DeclaredFunctions.FirstOrDefault(f => f.Name.Equals(context.functionStatement().functionName.Text));

            return funcInfo?.ReturnType ?? ObjectType.Void;
        }

        public override ObjectType VisitVariableOperand(QuestScriptParser.VariableOperandContext context)
        {
            return base.VisitVariableOperand(context);
        }

        public override ObjectType VisitIntegerLiteral(QuestScriptParser.IntegerLiteralContext context)
        {
            return ObjectType.Integer;
        }

        public override ObjectType VisitDoubleLiteral(QuestScriptParser.DoubleLiteralContext context)
        {
            return ObjectType.Double;
        }

        public override ObjectType VisitArrayLiteralExpression(QuestScriptParser.ArrayLiteralExpressionContext context)
        {
            return ObjectType.List;
        }

        public override ObjectType VisitStringLiteral(QuestScriptParser.StringLiteralContext context)
        {
            return ObjectType.String;
        }

        public override ObjectType VisitNullLiteral(QuestScriptParser.NullLiteralContext context)
        {
            return ObjectType.Void;
        }

        public override ObjectType VisitBooleanLiteral(QuestScriptParser.BooleanLiteralContext context)
        {
            return ObjectType.Boolean;
        }

        public override ObjectType VisitPrefixUnaryExpression(QuestScriptParser.PrefixUnaryExpressionContext context)
        {
            var type = context.expr.Accept(this);
            if (type != ObjectType.Integer &&
                type != ObjectType.Double)
            {
                TypeCheckExceptions.Add(new InvalidTypeException(context,new []{ ObjectType.Integer, ObjectType.Double }, type));
            }

            return type;
        }

        public override ObjectType VisitPostfixUnaryExpression(QuestScriptParser.PostfixUnaryExpressionContext context)
        {
            var type = context.expr.Accept(this);
            if (type != ObjectType.Integer &&
                type != ObjectType.Double)
            {
                TypeCheckExceptions.Add(new InvalidTypeException(context,new []{ ObjectType.Integer, ObjectType.Double }, type));
            }

            return type;
        }

        public override ObjectType VisitLogicalExpression(QuestScriptParser.LogicalExpressionContext context)
        {
            var leftType = context.left.Accept(this);
            if (leftType != ObjectType.Boolean)
            {
                TypeCheckExceptions.Add(new InvalidTypeException(context,ObjectType.Boolean,leftType));
            }

            var rightType = context.right.Accept(this);
            if (rightType != ObjectType.Boolean)
            {
                TypeCheckExceptions.Add(new InvalidTypeException(context,ObjectType.Boolean,rightType));
            }

            return ObjectType.Boolean;
        }
    }
}

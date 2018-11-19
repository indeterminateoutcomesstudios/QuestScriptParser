using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using FastMember;
using QuestScript.Interpreter.Exceptions;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.ScriptElements;
using QuestScript.Parser;
// ReSharper disable UncatchableException

namespace QuestScript.Interpreter
{
    //resolver for values of object attributes and local variables
    public class ExpressionValueResolverVisitor : QuestScriptBaseVisitor<object>
    {
        private readonly EnvironmentTreeBuilder _environmentBuilder;
        private static readonly TypeAccessor LazyTypeAccessor;

        static ExpressionValueResolverVisitor()
        {
            LazyTypeAccessor = TypeAccessor.Create(typeof(Lazy<object>));
        }

        public ExpressionValueResolverVisitor(EnvironmentTreeBuilder environmentBuilder)
        {
            _environmentBuilder = environmentBuilder;
        }

        public List<BaseInterpreterException> Errors { get; } = new List<BaseInterpreterException>();

        public override object VisitIdentifierOperand(QuestScriptParser.IdentifierOperandContext context)
        {
            var variable = _environmentBuilder.GetVariableFromCurrentEnvironment(context.GetText());
            if (variable != null)
            {
                return variable.Value;
            }

            return null;
        }

        public override object VisitParenthesizedExpression(QuestScriptParser.ParenthesizedExpressionContext context) => context.expr.Accept(this);

        public override object VisitAdditiveExpression(QuestScriptParser.AdditiveExpressionContext context) =>
            VisitArithmeticExpression(context, context.op, context.left, context.right);

        public override object VisitMultiplicativeExpression(QuestScriptParser.MultiplicativeExpressionContext context) =>
            VisitArithmeticExpression(context, context.op, context.left, context.right);

        private object VisitArithmeticExpression(ParserRuleContext context, ParserRuleContext op,ParserRuleContext left, ParserRuleContext right)
        {
            var leftValue = GetValueOrLazyValue(left.Accept(this));
            var rightValue = GetValueOrLazyValue(right.Accept(this));

            if (op.GetText() == "+")
            {
                if (leftValue is string leftStr)
                    return leftStr + rightValue;
                if (rightValue is string rightStr)
                    return leftValue + rightStr;
            }
            
            if ((leftValue is int || leftValue is double) &&
                (rightValue is int || rightValue is double))
            {
                
                bool TryConvertToNumber(object value, out double result)
                {
                    result = 0;
                    try
                    {
                        result = Convert.ToDouble(value);
                    }
                    catch (InvalidCastException e)
                    {
                        Errors.Add(new FailedToInferTypeException(context, left, e));
                        return false;
                    }

                    return true;
                }

                if (!TryConvertToNumber(leftValue, out var leftValueAsNumber)) 
                    return null;

                if (!TryConvertToNumber(rightValue, out var rightValueAsNumber)) 
                    return null;

                switch (op.GetText())
                {
                    case "+":
                        return leftValueAsNumber + rightValueAsNumber;
                    case "-":
                        return leftValueAsNumber - rightValueAsNumber;
                    case "/":
                        return leftValueAsNumber / rightValueAsNumber;
                    case "%":
                        return leftValueAsNumber % rightValueAsNumber;
                    case "*":
                        return leftValueAsNumber * rightValueAsNumber;
                }
            }

            //if not numeric and not string concatenation, nothing to do...
            return null;
        }

        public override object VisitStringLiteral(QuestScriptParser.StringLiteralContext context)
        {
            return context.GetText();
        }

        public override object VisitBooleanLiteral(QuestScriptParser.BooleanLiteralContext context)
        {
            return TryResolveLiteralValue(context, ObjectType.Boolean);
        }

        private object TryResolveLiteralValue<TContext>(TContext context, ObjectType type)
            where TContext : QuestScriptParser.LiteralContext
        {
            if (!TypeUtil.TryConvertToType(type, out var dotnetType))
            {
                return null;
            }

            try
            {
                return Convert.ChangeType(context.GetText(), dotnetType);                  
            }
            catch (InvalidCastException e)
            {
                Errors.Add(new FailedValueInterpretation(context, type, context.GetText(),e));
                return null;
            }
        }

        public override object VisitDoubleLiteral(QuestScriptParser.DoubleLiteralContext context)
        {
            return TryResolveLiteralValue(context, ObjectType.Double);
        }

        public override object VisitNullLiteral(QuestScriptParser.NullLiteralContext context)
        {
            if (!context.GetText().Equals("null"))
            {
                Errors.Add(new FailedValueInterpretation(context, ObjectType.Null, context.GetText()));
            }

            return null;
        }

        public override object VisitIntegerLiteral(QuestScriptParser.IntegerLiteralContext context)
        {
            return TryResolveLiteralValue(context, ObjectType.Integer);
        }

        public override object VisitArrayLiteral(QuestScriptParser.ArrayLiteralContext context)
        {
            var list = new ArrayList();
            foreach (var el in context._elements)
            {
                var value = el.Accept(this);
                list.Add(value);
            }

            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object GetValueOrLazyValue(object valueOrLazy) => 
            valueOrLazy is Lazy<object> ? LazyTypeAccessor[valueOrLazy, "Value"] : valueOrLazy;
    }
}

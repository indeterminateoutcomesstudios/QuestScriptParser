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
    public class ValueResolverVisitor : QuestScriptBaseVisitor<Lazy<object>>
    {
        private readonly EnvironmentTreeBuilder _environmentBuilder;
        private static readonly TypeAccessor LazyTypeAccessor;

        static ValueResolverVisitor()
        {
            LazyTypeAccessor = TypeAccessor.Create(typeof(Lazy<object>));
        }

        public ValueResolverVisitor(EnvironmentTreeBuilder environmentBuilder)
        {
            _environmentBuilder = environmentBuilder;
        }

        private List<BaseInterpreterException> Errors => _environmentBuilder.Errors;

        public override Lazy<object> VisitIdentifierOperand(QuestScriptParser.IdentifierOperandContext context)
        {
            return new Lazy<object>(() =>
            {
                var variable = _environmentBuilder.GetVariableFromCurrentEnvironment(context.GetText());
                return variable != null ? variable.Value : null;
            });
        }

        public override Lazy<object> VisitParenthesizedExpression(QuestScriptParser.ParenthesizedExpressionContext context) => context.expr.Accept(this);

        public override Lazy<object> VisitAdditiveExpression(QuestScriptParser.AdditiveExpressionContext context) =>
            VisitArithmeticExpression(context, context.op, context.left, context.right);

        public override Lazy<object> VisitMultiplicativeExpression(QuestScriptParser.MultiplicativeExpressionContext context) =>
            VisitArithmeticExpression(context, context.op, context.left, context.right);

        private Lazy<object> VisitArithmeticExpression(ParserRuleContext context, ParserRuleContext op,ParserRuleContext left, ParserRuleContext right)
        {
            return new Lazy<object>(() =>
            {
                var expressionType = _environmentBuilder.TypeInferenceVisitor.Visit(context);

                var leftValue = GetValueOrLazyValue(left.Accept(this));
                var rightValue = GetValueOrLazyValue(right.Accept(this));

                if (op.GetText() == "+")
                {
                    if (leftValue is string leftStr)
                        return leftStr + rightValue;
                    if (rightValue is string rightStr)
                        return leftValue + rightStr;
                }

                if (!(leftValue is int) && !(leftValue is double) ||
                    !(rightValue is int) && !(rightValue is double)) 
                    return null;

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

                object Cast(object val, ObjectType type) => 
                    TypeUtil.TryConvert(val, type, out var castResult) ? 
                        castResult : null;

                switch (op.GetText())
                {
                    case "+":
                        return Cast(leftValueAsNumber + rightValueAsNumber, expressionType);
                    case "-":
                        return Cast(leftValueAsNumber - rightValueAsNumber, expressionType);
                    case "/":
                        if (Math.Abs(rightValueAsNumber) < 0.00000000001) //epsilon :)
                        {
                            var error = new DivisionByZeroException(context);
                            Errors.Add(error);
                        }

                        var val = leftValueAsNumber / rightValueAsNumber;

                        return Cast(val, expressionType);
                    case "%":
                        return Cast(leftValueAsNumber % rightValueAsNumber, expressionType);
                    case "*":
                        return Cast(leftValueAsNumber * rightValueAsNumber, expressionType);
                }

                //if not numeric and not string concatenation, nothing to do...
                return null;
            });
        }

        public override Lazy<object> VisitStringLiteral(QuestScriptParser.StringLiteralContext context)
        {
            return new Lazy<object>(context.GetText);
        }

        public override Lazy<object> VisitBooleanLiteral(QuestScriptParser.BooleanLiteralContext context)
        {
            return TryResolveLiteralValue(context, ObjectType.Boolean);
        }

        private Lazy<object> TryResolveLiteralValue<TContext>(TContext context, ObjectType type)
            where TContext : QuestScriptParser.LiteralContext
        {
            if (!TypeUtil.TryConvertType(type, out var dotnetType))
            {
                return null;
            }

            try
            {
                var _ = Convert.ChangeType(context.GetText(), dotnetType); //test converting now, perhaps we cannot convert?
                return new Lazy<object>(() => Convert.ChangeType(context.GetText(), dotnetType));                  
            }
            catch (InvalidCastException e)
            {
                Errors.Add(new FailedValueInterpretation(context, type, context.GetText(),e));
                return null;
            }
        }

        public override Lazy<object> VisitDoubleLiteral(QuestScriptParser.DoubleLiteralContext context)
        {
            return TryResolveLiteralValue(context, ObjectType.Double);
        }

        public override Lazy<object> VisitNullLiteral(QuestScriptParser.NullLiteralContext context)
        {
            if (!context.GetText().Equals("null"))
            {
                Errors.Add(new FailedValueInterpretation(context, ObjectType.Null, context.GetText()));
            }

            return null;
        }

        public override Lazy<object> VisitIntegerLiteral(QuestScriptParser.IntegerLiteralContext context)
        {
            return TryResolveLiteralValue(context, ObjectType.Integer);
        }

        public override Lazy<object> VisitArrayLiteral(QuestScriptParser.ArrayLiteralContext context)
        {
            var resultingValue = new Lazy<object>(() =>
            {
                var list = new ArrayList();
                foreach (var el in context._elements)
                {
                    var value = el.Accept(this);
                    list.Add(value);
                }

                return list;
            });

            return resultingValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object GetValueOrLazyValue(object valueOrLazy)
        {
            if (!(valueOrLazy is Lazy<object>))
                return valueOrLazy;
          
            return GetValueOrLazyValue(LazyTypeAccessor[valueOrLazy, "Value"]);
        }
    }
}

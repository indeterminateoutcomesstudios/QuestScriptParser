using System;
using System.Collections;
using System.Collections.Generic;
using Antlr4.Runtime;
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

        public ValueResolverVisitor(EnvironmentTreeBuilder environmentBuilder)
        {
            _environmentBuilder = environmentBuilder;
        }

        private HashSet<BaseInterpreterException> Errors => _environmentBuilder.Errors;

        public override Lazy<object> VisitIdentifierOperand(QuestScriptParser.IdentifierOperandContext context)
        {
            return new Lazy<object>(() =>
            {
                var variable = _environmentBuilder.GetVariableFromCurrentEnvironment(context.GetText());
                return variable != null ? variable.Value : null;
            });
        }

        public override Lazy<object> VisitIndexerExpression(QuestScriptParser.IndexerExpressionContext context)
        {
            //this does necessary type checks.            
            var valueType = _environmentBuilder.TypeInferenceVisitor.Visit(context);
            if (valueType == ObjectType.Unknown) //failed at one or more type checks
            {
                return new Lazy<object>(() => null);
            }

            var instanceType = _environmentBuilder.TypeInferenceVisitor.Visit(context.instance);
            if (instanceType == ObjectType.List)
            {
                return new Lazy<object>(() =>
                {
                    //we can assume those types are there because we did type checks with TypeInferenceVisitor
                    var listArray = (ArrayList) context.instance.Accept(this).Value.GetValueOrLazyValue();
                    return listArray[int.Parse(context.parameter.Accept(this).Value.GetValueOrLazyValue().ToString())];
                });
            }

            if (instanceType == ObjectType.Dictionary)
            {
                throw new NotImplementedException("Support for dictionaries is currently not implemented");
                //TODO : add support for dictionaries
            }

            _environmentBuilder.Errors.Add(new UnexpectedTypeException(context,ObjectType.List,instanceType,context.instance,$"Indexer expression must be applied to either list or a dictionary, and this was '{instanceType}'."));
            return base.VisitIndexerExpression(context);
        }

        public override Lazy<object> VisitParenthesizedExpression(QuestScriptParser.ParenthesizedExpressionContext context) => context.expr.Accept(this);

        public override Lazy<object> VisitAdditiveExpression(QuestScriptParser.AdditiveExpressionContext context) =>
            VisitArithmeticExpression(context, context.op, context.left, context.right);

        public override Lazy<object> VisitMultiplicativeExpression(QuestScriptParser.MultiplicativeExpressionContext context) =>
            VisitArithmeticExpression(context, context.op, context.left, context.right);

        public override Lazy<object> VisitRelationalExpression(QuestScriptParser.RelationalExpressionContext context)
        {
            var leftType = _environmentBuilder.TypeInferenceVisitor.Visit(context.left);
            var rightType = _environmentBuilder.TypeInferenceVisitor.Visit(context.right);

            return new Lazy<object>(() =>
            {
                return false;
            });
        }

        private Lazy<object> VisitArithmeticExpression(ParserRuleContext context, ParserRuleContext op,ParserRuleContext left, ParserRuleContext right)
        {
            var expressionType = _environmentBuilder.TypeInferenceVisitor.Visit(context);
            if (!TypeUtil.IsNumeric(expressionType) && expressionType != ObjectType.String)
            {
                _environmentBuilder.Errors.Add(
                    new InvalidOperandsException(
                        context,
                        op.GetText(),
                        _environmentBuilder.TypeInferenceVisitor.Visit(left),
                        _environmentBuilder.TypeInferenceVisitor.Visit(right)));

                return new Lazy<object>(() => null);
            }
            return new Lazy<object>(() =>
            {

                var leftValue = left.Accept(this).GetValueOrLazyValue();
                var rightValue = right.Accept(this).GetValueOrLazyValue();

                if (op.GetText() == "+")
                {
                    //maybe we have string concatenation?
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

        public override Lazy<object> VisitStatement(QuestScriptParser.StatementContext context)
        {
            throw new NotSupportedException($"{nameof(ValueResolverVisitor)} should not be applied to statements, only expressions");
        }     

        public override Lazy<object> VisitPostfixUnaryExpression(QuestScriptParser.PostfixUnaryExpressionContext context)
        {
            var type = _environmentBuilder.TypeInferenceVisitor.Visit(context.expr);

            if (!TypeUtil.IsNumeric(type))
            {
                _environmentBuilder.Errors.Add(new UnexpectedTypeException(context,ObjectType.Integer,type,context.expr,"Expected the incremented expression to be numeric, but it wasn't."));
                return new Lazy<object>(() => null);
            }

            dynamic GetValueOfExpression(ParserRuleContext expr) => 
                expr.Accept(this).Value.GetValueOrLazyValue();

            switch (context.op.Text)
            {
                case "++":
                    return new Lazy<object>(() =>
                    {
                        var plusplus = GetValueOfExpression(context.expr);
                        return (object)++plusplus;
                    });
                case "--":
                    return new Lazy<object>(() =>
                    {
                        var minusminus = GetValueOfExpression(context.expr);
                        return (object)--minusminus;
                    });
            }

            return base.VisitPostfixUnaryExpression(context);
        }

        public override Lazy<object> VisitStringLiteral(QuestScriptParser.StringLiteralContext context)
        {
            return new Lazy<object>(() =>
            {
                var stringValue = context.GetText().Trim('"').Replace("\\\"","\"");           
                return stringValue;
            });
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
                    var value = el.Accept(this).GetValueOrLazyValue();
                    list.Add(value);
                }

                return list;
            });

            return resultingValue;
        }

    }
}

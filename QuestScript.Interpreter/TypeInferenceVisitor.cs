using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
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

        public override ObjectType VisitAdditiveExpression(QuestScriptParser.AdditiveExpressionContext context)
        {
            return VisitArithmeticExpression(context, context.op, context.left, context.right);
        }

        public override ObjectType VisitMultiplicativeExpression(QuestScriptParser.MultiplicativeExpressionContext context)
        {
            return VisitArithmeticExpression(context, context.op, context.left, context.right);
        }

        //public override ObjectType VisitArithmeticExpression(QuestScriptParser.ArithmeticExpressionContext context)
        private ObjectType VisitArithmeticExpression(ParserRuleContext context, ParserRuleContext op,ParserRuleContext left, ParserRuleContext right)        
        {
            var leftType = left.Accept(this);
            var rightType = right.Accept(this);

            //if at least one is unknown, then we already have an error and can stop evaluating types
            if (leftType == ObjectType.Unknown || rightType == ObjectType.Unknown)
                return ObjectType.Unknown;        

            if (leftType == rightType)
                return leftType;

            //we don't want to lose precision, thus any arithmetic expression with double operand becomes double too
            if (TypeUtil.IsNumeric(rightType) &&
                TypeUtil.IsNumeric(leftType) &&
                (leftType == ObjectType.Double || rightType == ObjectType.Double))
                return ObjectType.Double;

            if (TypeUtil.CanConvert(rightType, leftType))
                return leftType;

            if (TypeUtil.CanConvert(leftType, rightType))
                return rightType;

            _environmentBuilder.Errors.Add(new InvalidOperandsException(context,op.GetText(), leftType, rightType));

            return ObjectType.Unknown;
        }

        public override ObjectType VisitIndexerExpression(QuestScriptParser.IndexerExpressionContext context)
        {
            var variableType = context.instance.Accept(this);
            if (variableType == ObjectType.List)
            {
                //first verify that the indexer parameter is integer, otherwise 
                //in the context of lists the expression makes no sense
                var parameterType = context.parameter.Accept(this);
                if (!TypeUtil.IsNumeric(parameterType))
                {
                    _environmentBuilder.Errors.Add(
                        new UnexpectedTypeException(
                            context,ObjectType.Integer,
                            parameterType,
                            context.parameter,
                            "When using list accessor, the index should be of integer type. For example 'x = list[23]' is a valid statement."));
                    return ObjectType.Unknown;
                }

                //at this point the expression we are accessing through indexer SHOULD be evaluable,
                //so we are simply resolving it's value sooner
                var value =  _environmentBuilder.ValueResolverVisitor.Visit(context.instance).Value.GetValueOrLazyValue();
                var valueAsArray = value as ArrayList;
                if (valueAsArray == null) //precaution, shouldn't happen
                {
                    if (!TypeUtil.TryConvertType(value.GetType(), out var resultingType))
                    {
                        resultingType = ObjectType.Unknown;
                    }

                    _environmentBuilder.Errors.Add(
                        new UnexpectedTypeException(
                            context,ObjectType.List,
                            resultingType,
                            context.parameter,
                            $"Expected {context.instance.GetText()} to be a 'ObjectType.List', but it is '{resultingType}'. Thus, cannot infer the resulting type of the indexer expression"));
                    return ObjectType.Unknown;
                }

                if (!TypeUtil.TryConvertType(valueAsArray[0].GetType(), out var itemType))
                    itemType = ObjectType.Unknown;

                return itemType;
            }

            //TODO: add here support for dictionaries (object attributes and function parameter may be of dictionary type...)
            return ObjectType.Unknown;
        }

        public override ObjectType VisitScript(QuestScriptParser.ScriptContext context)
        {
            return ObjectType.Script;
        }

        public override ObjectType VisitArrayLiteralExpression(QuestScriptParser.ArrayLiteralExpressionContext context)
        {
            //TODO : add type checks of arrays within arrays, so verifying type of [[1,2],["foo","bar"],[5,6]] would result in error
            //make sure that all elements in the literal have the same type
            if (context.expr._elements.Count <= 0) 
                return ObjectType.List;

            var result = RecursiveVerifyEmbeddedArrayTypes(context);
            if (!result.isOk)
            {
                _environmentBuilder.Errors.Add(new InvalidArrayLiteralException(context,
                    $"Expected all values in the array ('{context.GetText()}') to be of type '{result.requiredType}', but found an item ({result.context.GetText()}) of which has value(s) of type '{result.actualType}'"));
                return ObjectType.Unknown;
            }

            return ObjectType.List;        
        }

        private ObjectType RecursiveGetTypeOf(QuestScriptParser.ArrayLiteralExpressionContext context)
        {
            if (context.expr._elements.Count <= 0) //precaution, should never be true..
                return ObjectType.Unknown;

            var firstItem = context.expr._elements[0];

            if (firstItem is QuestScriptParser.ArrayLiteralExpressionContext literalArray)
                return RecursiveGetTypeOf(literalArray);

            var embeddedArray = firstItem.FindDescendantOfType<QuestScriptParser.ArrayLiteralExpressionContext>();            
            if (embeddedArray != null)
                return RecursiveGetTypeOf(embeddedArray);

            var type = firstItem.Accept(this);
            //just to make sure, perhaps we have a method or a function that evaluates to list?
            if (type == ObjectType.List)
            {
                //so, lets try to evaluate it, perhaps we have
                //something like function result to infer the type?
                //TODO : verify this branch of code works when functions/methods evaluation is implemented
                var value = _environmentBuilder.ValueResolverVisitor.Visit(firstItem).Value.GetValueOrLazyValue();

                ObjectType RecordFailureToInferAndReturnUnknownType(QuestScriptParser.ArrayLiteralExpressionContext arrayLiteralExpressionContext, QuestScriptParser.ExpressionContext expressionContext)
                {
                    _environmentBuilder.Errors.Add(new FailedToInferTypeException(arrayLiteralExpressionContext, expressionContext,
                        $"First item of {arrayLiteralExpressionContext.GetText()} seems to be an embedded array (item is '{expressionContext.GetText()}'), but failed to infer the type of its first item. Something is wrong here..."));
                    return ObjectType.Unknown;
                }

                if (value is ArrayList array)
                {
                    object GetFirstItemRecursive(object arrayOrValue)
                    {
                        while (true)
                        {
                            if (!(arrayOrValue is IEnumerable enumerable)) 
                                return arrayOrValue;

                            var enumerator = enumerable.GetEnumerator();
                            enumerator.MoveNext();
                            arrayOrValue = enumerator.Current;
                        }
                    }

                    var firstArrayItem = GetFirstItemRecursive(array);
                    return !TypeUtil.TryConvertType(firstArrayItem.GetType(),out var firstItemType) ? 
                        RecordFailureToInferAndReturnUnknownType(context, firstItem) : firstItemType;
                }
                else
                {
                    RecordFailureToInferAndReturnUnknownType(context, firstItem);
                }
            }

            return type;
        }

        private (bool isOk, ObjectType actualType, ObjectType requiredType, ParserRuleContext context) RecursiveVerifyEmbeddedArrayTypes(
            QuestScriptParser.ArrayLiteralExpressionContext context)
        {
            var firstItemType = RecursiveGetTypeOf(context);
            var result = RecursiveVerifyEmbeddedArrayTypes(context, firstItemType);
            return (result.isOk, result.actualType, firstItemType, result.context);
        }


        private (bool isOk, ObjectType actualType, ParserRuleContext context) RecursiveVerifyEmbeddedArrayTypes(QuestScriptParser.ArrayLiteralExpressionContext context, ObjectType requiredType)
        {
            var firstItem = context.expr._elements[0];
            if (!(firstItem is QuestScriptParser.ArrayLiteralExpressionContext))
            {
                //assume we have only primitives
                foreach (var item in context.expr._elements)
                {
                    //if the first item wasn't an embedded array, disallow them on other items
                    if (item is QuestScriptParser.ArrayLiteralExpressionContext)
                        return (false, ObjectType.List, item);

                    var itemType = item.Accept(this);
                    if (itemType != requiredType)
                        return (false, itemType, item);
                }
            }
            else
            {
                foreach (var item in context.expr._elements)
                {
                    //if the first item was an embedded array, enforce their existence on other items
                    if (!(item is QuestScriptParser.ArrayLiteralExpressionContext))
                        return (false, item.Accept(this), item);

                    var embedded = (QuestScriptParser.ArrayLiteralExpressionContext)item;
                    var result = RecursiveVerifyEmbeddedArrayTypes(embedded,requiredType);
                    if (!result.isOk)
                        return (false,result.actualType,result.context);
                }
            }   
            return (true, requiredType, context);
        }

        public override ObjectType VisitPostfixUnaryExpression(QuestScriptParser.PostfixUnaryExpressionContext context) =>
            context.expr.Accept(this);

        public override ObjectType VisitRelationalExpression(QuestScriptParser.RelationalExpressionContext context)
        {
            var leftType = context.left.Accept(this);
            var rightType = context.right.Accept(this);

            if (TypeUtil.IsComparable(leftType) && TypeUtil.IsComparable(rightType)) 
                return ObjectType.Boolean; //comparison results are boolean of course...

            _environmentBuilder.Errors.Add(new InvalidOperandsException(context,context.op.GetText(),leftType,rightType));
            return ObjectType.Unknown;
        }

        public override ObjectType VisitOrExpression(QuestScriptParser.OrExpressionContext context)
        {
            return VisitLogicalExpression(context, "or", context.left, context.right);
        }

        public override ObjectType VisitAndExpression(QuestScriptParser.AndExpressionContext context)
        {
            return VisitLogicalExpression(context, "and", context.left, context.right);
        }

        private ObjectType VisitLogicalExpression(ParserRuleContext context, string op, ParserRuleContext left, ParserRuleContext right)
        {
            var leftType = left.Accept(this);
            var rightType = right.Accept(this);

            if (leftType == ObjectType.Boolean &&
                rightType == ObjectType.Boolean)
            {
                return ObjectType.Boolean;
            }

            _environmentBuilder.Errors.Add(new InvalidOperandsException(context,op,leftType,rightType));
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

using System;
using System.Collections;
using System.Collections.Generic;
using QuestScript.Interpreter.Exceptions;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.ScriptElements;
using QuestScript.Parser;
// ReSharper disable UncatchableException

namespace QuestScript.Interpreter
{
    //resolver for values of object attributes and local variables
    public class ValueResolverVisitor : QuestScriptBaseVisitor<object>
    {
        private readonly EnvironmentTreeBuilder _environmentBuilder;

        public ValueResolverVisitor(EnvironmentTreeBuilder environmentBuilder)
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
    }
}

using QuestScript.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;
using QuestScript.Interpreter.ValidationExceptions;

namespace QuestScript.Interpreter
{
    public class SyntaxValidationVisitor : QuestScriptBaseVisitor<bool>
    {
        private ParserRuleContext _currentScope;
        public Dictionary<ParserRuleContext, List<string>> SymbolsPerContextScope = new Dictionary<ParserRuleContext, List<string>>();   

        public List<IFunction> DeclaredFunctions;
        public List<IObject> DeclaredObjects;
        public List<Exception> ValidationExceptions { get; } = new List<Exception>();

        private static readonly Func<List<string>> NewContextScopeCollectionFactory = () => new List<string>();

        public override bool VisitStatement(QuestScriptParser.StatementContext context)
        {
            _currentScope = context.GetChild<ParserRuleContext>(0);
            base.VisitStatement(context);
            return true;
        }

        public override bool VisitOperandExpression(QuestScriptParser.OperandExpressionContext context)
        {
            var memberFieldContext = context.GetChildOfType<QuestScriptParser.MemberFieldOperandContext>();
            if (memberFieldContext != null)
            {
                var instanceSymbolName = memberFieldContext.instance.GetText();
                var declaredObject = DeclaredObjects?.FirstOrDefault(obj => obj.Name.Equals(instanceSymbolName));
                if (declaredObject == null)
                {
                    ValidationExceptions.Add(new UndefinedSymbolException(instanceSymbolName, _currentScope, "an object"));
                    return true;
                }

                var memberSymbolName = memberFieldContext.member.Text;
                if (!declaredObject.Attributes.Any(attr => attr.Name.Equals(memberSymbolName)))
                {
                    ValidationExceptions.Add(new UndefinedMemberException(instanceSymbolName, memberSymbolName,
                        _currentScope));
                }

                return true;
            }

            var identifierContext = context.GetChildOfType<QuestScriptParser.IdentifierOperandContext>();
            if (identifierContext != null)
            {
                var symbolName = context.GetText();
                if (FindSymbolContext(symbolName, _currentScope) == null)
                {
                    ValidationExceptions.Add(new UndefinedSymbolException(symbolName, _currentScope));
                }

                return true;
            }

            return true;
        }

        public override bool VisitIdentifierOperand(QuestScriptParser.IdentifierOperandContext context)
        {
            var symbolName = context.GetText();
            if (FindSymbolContext(symbolName, _currentScope) == null)
            {
                ValidationExceptions.Add(new UndefinedSymbolException(symbolName, _currentScope));
            }

            base.VisitIdentifierOperand(context);
            return true;
        }

        public override bool VisitAssignmentStatement(QuestScriptParser.AssignmentStatementContext context)
        {
            AddSymbolIfNeeded(context.LVal.GetText());
            return base.VisitAssignmentStatement(context);
        }

        public override bool VisitForEachStatement(QuestScriptParser.ForEachStatementContext context)
        {
            return base.VisitForEachStatement(context);
        }

        public override bool VisitForStatement(QuestScriptParser.ForStatementContext context)
        {
            AddSymbolIfNeeded(context.iterationVariable.Text);
            return base.VisitForStatement(context);
        }

        private void AddSymbolIfNeeded(string symbolName)
        {
            AddSymbolForContextOf(symbolName,
                () => _currentScope.FindParentOfType<QuestScriptParser.ForStatementContext>(), //symbol that is defined in the context of a loop...
                () => _currentScope.FindParentOfType<QuestScriptParser.ForEachStatementContext>(),
                () => _currentScope.FindParentOfType<QuestScriptParser.CodeBlockStatementContext>(),
                () => _currentScope.FindParentOfType<QuestScriptParser.ScriptContext>());
        }

        private void AddSymbolForContextOf(string symbolName, params Func<ParserRuleContext>[] contextResolvers)
        {
            foreach (var ctxResolver in contextResolvers)
            {
                var ctx = ctxResolver();
                //add symbol to the context of first of the choices...
                if (ctx != null)
                {                   
                    ActuallyAddSymbol(symbolName, ctx);
                    break;
                }
            }
        }

        private void ActuallyAddSymbol<TContext>(string symbolName, TContext ctx)
            where TContext : ParserRuleContext
        {            
            //add symbol declaration only if it is not declared in upper tier contextResolvers
            //FindSymbolContext() will go recurisvely upwards in the hierarchy and looks for symbol definitions
            if (FindSymbolContext(symbolName, ctx) == null)
            {
                SymbolsPerContextScope.EnsureKey(ctx, NewContextScopeCollectionFactory);
                SymbolsPerContextScope[ctx].Add(symbolName);
            }
        }

        private ParserRuleContext FindSymbolContext(string symbolName, ParserRuleContext startingPoint)
        {
            var current = startingPoint;
            do
            {
                if (SymbolsPerContextScope.TryGetValue(current, out var contextSymbols) &&
                    contextSymbols.Contains(symbolName))
                {
                    return current;
                }

                current = (ParserRuleContext)current.Parent;
            } while (current != null);

            return null;
        }
    }
}

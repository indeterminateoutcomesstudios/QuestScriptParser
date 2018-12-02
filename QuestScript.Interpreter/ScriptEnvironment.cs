using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using QuestScript.Interpreter.Extensions;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.InterpreterElements;
using QuestScript.Parser;
using Environment = QuestScript.Interpreter.InterpreterElements.Environment;

namespace QuestScript.Interpreter
{
    public sealed class ScriptEnvironment
    {
        private static readonly Type StatementContextType = typeof(QuestScriptParser.StatementContext);

        private readonly Dictionary<ParserRuleContext, Environment> _environmentsByContext;

        internal ScriptEnvironment(Environment root, Dictionary<ParserRuleContext, Environment> environmentsByContext)
        {
            Root = root;
            _environmentsByContext = environmentsByContext;
        }

        public Environment Root { get; }

        public Variable GetVariable(string name, ParserRuleContext ctx)
        {
            if (ctx.GetType().IsAssignableFrom(StatementContextType) &&
                _environmentsByContext.TryGetValue(ctx, out var env))
                return env.GetVariable(name);

            var statementAncestor = ctx.FindParentOfType<QuestScriptParser.StatementContext>();
            if (statementAncestor != null &&
                _environmentsByContext.TryGetValue(statementAncestor, out var statementEnvironment))
                return statementEnvironment.GetVariable(name);

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsVariableDefined(string name, ParserRuleContext context)
        {
            return GetVariable(name, context) != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Variable> DebugGetAllVariables()
        {
            return new HashSet<Variable>(Root.IterateBfs().SelectMany(env => env.LocalVariables));
        }
    }
}
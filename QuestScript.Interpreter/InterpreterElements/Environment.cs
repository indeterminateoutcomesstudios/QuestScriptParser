using System.Collections.Generic;
using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.InterpreterElements
{
    public class Environment
    {
        public List<Variable> LocalVariables { get; } = new List<Variable>();
        public List<IObjectInstance> LocalObjects { get; } = new List<IObjectInstance>();

        public List<ParserRuleContext> Statements { get; } = new List<ParserRuleContext>();

        public Environment Parent;
        public ParserRuleContext Context;
        public List<Environment> Children { get; } = new List<Environment>();

        public Environment CreateChild(ParserRuleContext ctx)
        {
            var newScope = new Environment
            {
                Context = ctx,
                Parent = this
            };            
            Children.Add(newScope);
            
            return newScope;
        }
    }
}

using System.Collections.Generic;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.InterpreterElements
{
    public class Environment
    {
        public ParserRuleContext Context;

        public Environment Parent;
        public List<Variable> LocalVariables { get; } = new List<Variable>();
        //public List<IObjectInstance> LocalObjects { get; } = new List<IObjectInstance>();

        public List<ParserRuleContext> Statements { get; } = new List<ParserRuleContext>();
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
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

        //declarations "split" environments - in this way we can track which variables were declared AFTER their usage
        public Environment NextSibling;
        public Environment PrevSibling;

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

        //progressive siblings need to have everything that is in previous ones
        public Environment CreateNextSibling(ParserRuleContext ctx)
        {
            var newSibling = new Environment
            {
                Parent = Parent,
                Context = ctx
            };

            newSibling.Children.AddRange(Children);
            newSibling.PrevSibling = this;
            NextSibling = newSibling;
            newSibling.LocalVariables.AddRange(LocalVariables);
            newSibling.LocalObjects.AddRange(LocalObjects);

            return newSibling;
        }
    }
}

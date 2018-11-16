using Antlr4.Runtime;
using QuestScript.Parser;

namespace QuestScript.Interpreter
{
    public static class ParserRuleContextUtils
    {
        public static TChild GetChildOfType<TChild>(this ParserRuleContext ctx) where TChild : ParserRuleContext
        {
            if (ctx.ChildCount == 0)
                return null;

            
            foreach (var child in ctx.GetRuleContexts<ParserRuleContext>())
            {
                if (child is TChild childOfType)
                    return childOfType;

                return child.GetChildOfType<TChild>();
            }

            return null;
        }

        public static TParent FindParentOfType<TParent>(this ParserRuleContext ctx) where TParent : ParserRuleContext
        {
            var currentCtx = ctx;
            do
            {
                if (currentCtx is TParent currentAsParent)
                    return currentAsParent;
                if (currentCtx.Parent is TParent parent)
                    return parent;

                if (currentCtx.Parent is QuestScriptParser.ScriptContext)
                    break;

                currentCtx = (ParserRuleContext) currentCtx.Parent;
            } while (currentCtx.Parent != null); //precaution, should break at "ScriptContext" if

            return null;
        }
    }
}

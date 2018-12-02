using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using QuestScript.Parser;

namespace QuestScript.Interpreter.Helpers
{
    public static class ParserRuleContextUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasDescendantOfType<TChild>(this ParserRuleContext ctx) where TChild : ParserRuleContext
        {
            return ctx is TChild || ctx.FindDescendantOfType<TChild>() != null;
        }

        public static TChild FindDescendantOfType<TChild>(this ParserRuleContext ctx) where TChild : ParserRuleContext
        {
            if (ctx.ChildCount == 0)
                return null;


            foreach (var child in ctx.GetRuleContexts<ParserRuleContext>())
            {
                if (child is TChild childOfType)
                    return childOfType;

                return child.FindDescendantOfType<TChild>();
            }

            return null;
        }

        public static ParserRuleContext FindFirstParentOfTypes(this ParserRuleContext ctx, params Type[] contextTypes)
        {
            var currentCtx = ctx;
            do
            {
                if (contextTypes.Contains(currentCtx.GetType())) return currentCtx;

                currentCtx = (ParserRuleContext) currentCtx.Parent;
            } while (currentCtx.Parent != null); //precaution, should break at "ScriptContext" if

            //last attempt,perhaps we are looking for root ctx?
            if (contextTypes.Contains(currentCtx.GetType())) return currentCtx;

            return null;
        }

        public static bool HasParentOfType<TParent>(this ParserRuleContext ctx) where TParent : ParserRuleContext
        {
            return ctx.FindParentOfType<TParent>() != null;
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
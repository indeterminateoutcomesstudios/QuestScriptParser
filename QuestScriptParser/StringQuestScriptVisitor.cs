namespace QuestScriptParser
{
    public class StringQuestScriptVisitor : QuestScriptBaseVisitor<string>
    {
        public override string VisitKeyword(QuestScriptParser.KeywordContext context)
        {
            return context.GetText();
        }

        public override string VisitIntegerLiteral(QuestScriptParser.IntegerLiteralContext context)
        {
            return context.GetText();
        }

        public override string VisitBooleanLiteral(QuestScriptParser.BooleanLiteralContext context)
        {
            return context.GetText();
        }

        public override string VisitDoubleLiteral(QuestScriptParser.DoubleLiteralContext context)
        {
            return context.GetText();
        }
    }
}
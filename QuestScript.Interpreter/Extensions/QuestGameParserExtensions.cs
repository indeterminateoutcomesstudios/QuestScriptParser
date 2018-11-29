using System.Linq;
using QuestScript.Parser;

namespace QuestScript.Interpreter.Extensions
{
    public static class QuestGameParserExtensions
    {
        //public static bool TryGetAttributeByName(this QuestGameParser.ElementContext context, string name, out (string Key, string Value) attributeKeyValue)
        //{
        //    attributeKeyValue = default;

        //    var attributes = context.attribute();

        //    var desiredAttribute = attributes?.FirstOrDefault(attr => attr.Key.Text.Equals(name));
        //    if (desiredAttribute == null)
        //        return false;

        //    attributeKeyValue = (desiredAttribute.Key.Text, desiredAttribute.Value.Text.CleanTokenArtifacts());

        //    return true;
        //}
    }
}

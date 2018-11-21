using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestScript.Parser;

namespace QuestScript.Interpreter
{
    public enum GameFileType
    {
        Game,
        Library
    }

    public class GameObjectResolverVisitor : QuestGameParserBaseVisitor<bool>
    {
        public HashSet<string> References { get; } = new HashSet<string>();
        public GameFileType Type { get; private set; }
        public override bool VisitElement(QuestGameParser.ElementContext context)
        {
            if (string.IsNullOrWhiteSpace(context.ElementName?.Text)) //precaution
                return false;
            switch (context.ElementName.Text.ToLowerInvariant())
            {
                case "include":
                    VisitInclude(context);
                    break;                    
                case "library":
                    Type = GameFileType.Library;
                    break;                    
                case "asl":
                    Type = GameFileType.Game;
                    break;                    
            }

            return base.VisitElement(context);
        }

        private void VisitInclude(QuestGameParser.ElementContext context)
        {
            var refAttribute = context.attribute().FirstOrDefault(attr => attr.Key.Text.Equals("ref"));
            if (refAttribute == null)
            {
                throw new InvalidDataException("Found <include> element, but didn't find 'ref' attribute. This should not happen, and it is likely this ASLX game file is corrupted. The element I found is : " + context.GetText());
            }

            References.Add(refAttribute.Value.Text.CleanTokenArtifacts());
        }
    }
}

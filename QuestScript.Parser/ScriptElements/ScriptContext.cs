using System.Collections.Generic;

namespace QuestScript.Parser.ScriptElements
{
    public class ScriptContext
    {
        public List<Statement> Root { get; } = new List<Statement>();
    }
}

using System;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace QuestScriptParser
{
    public class StringQuestScriptVisitor : QuestScriptBaseVisitor<bool>
    {
        private int _currentIdentation = 0;

        private readonly StringBuilder _output = new StringBuilder();
        public string Output => _output.ToString();

             
    }
}
﻿using System;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace QuestScriptParser
{
    public class StringQuestScriptVisitor : QuestScriptBaseVisitor<bool>
    {      
        private readonly StringBuilder _output = new StringBuilder();
        public string Output => _output.ToString();
        

    }
}
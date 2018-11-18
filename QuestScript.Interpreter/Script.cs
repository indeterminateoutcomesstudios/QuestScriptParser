﻿using System.Collections.Generic;
using QuestScript.Interpreter.InterpreterElements;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter
{
    public class QuestScript
    {
        public List<IObjectInstance> ObjectInstances { get; } = new List<IObjectInstance>();
        public List<IFunction> Functions { get; } = new List<IFunction>();

        public Environment RootEnvironment;
    }
}

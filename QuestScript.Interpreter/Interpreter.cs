using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;

namespace QuestScript.Interpreter
{
    public static class Interpreter
    {
        public static Dictionary<string,Script> ScriptCache = new Dictionary<string, Script>();
    }
}

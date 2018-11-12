using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestScript.Interpreter
{
    public class ParameterInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class FunctionInfo
    {
        public string FunctionName { get; set; }      
        public ParameterInfo[] Parameters { get; set; }
    }
}

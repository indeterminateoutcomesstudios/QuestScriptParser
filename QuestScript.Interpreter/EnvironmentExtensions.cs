using System.Collections.Generic;
using System.Linq;
using QuestScript.Interpreter.InterpreterElements;

namespace QuestScript.Interpreter
{
    public static class EnvironmentExtensions
    {
        public static Variable GetVariable(this InterpreterElements.Environment environment, string name)
        {
            while (environment != null)
            {
                //first, check in the current environment
                var variable = environment.LocalVariables.FirstOrDefault(v => v.Name.Equals(name));
                if (variable != null)
                    return variable;
           
                environment = environment.Parent;
            }

            return null;
        }

        public static bool IsVariableDefined(this InterpreterElements.Environment environment, string name)
        {
            return GetVariable(environment, name) != null;
        }

 

        public static IEnumerable<InterpreterElements.Environment> IterateBFS(this InterpreterElements.Environment environment)
        {
            yield return environment;
            foreach (var env in environment.Children.SelectMany(c => c.IterateBFS()))
                yield return env;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestScript.Interpreter.InterpreterElements;
using Environment = System.Environment;

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

                //then, iterate back over siblings and see if it is defined BEFORE the current one
                while(environment.PrevSibling != null) 
                {
                    environment = environment.PrevSibling;
                    variable = environment.LocalVariables.FirstOrDefault(v => v.Name.Equals(name));
                    if (variable != null)
                        return variable;
                }

                environment = environment.Parent;
            }

            return null;
        }

        public static bool IsVariableDefined(this InterpreterElements.Environment environment, string name)
        {
            return GetVariable(environment, name) != null;
        }

        public static IEnumerable<InterpreterElements.Environment> Siblings(this InterpreterElements.Environment environment)
        {
            var start = environment;
            while (start.NextSibling != null)
            {
                yield return start;
                start = start.NextSibling;                
            }
        }

        public static IEnumerable<InterpreterElements.Environment> IterateBFS(this InterpreterElements.Environment environment)
        {
            foreach (var env in environment.Siblings())
                yield return env;

            if(environment.Children.Count == 0)
                yield break;

            foreach (var env in environment.Children.SelectMany(c => c.IterateBFS()))
                yield return env;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Antlr4.Runtime;
using QuestScript.Interpreter.Exceptions;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.GameObjectParsers
{
    public class FunctionsParser
    {
        private readonly GameObjectResolver _parent;
        private readonly FunctionReferencesBuilder _referenceBuilder = new FunctionReferencesBuilder();
        private readonly Graph<string> _functionReferenceGraph = new Graph<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string,FunctionDefinition> _functionDefinitions = new Dictionary<string, FunctionDefinition>();

        public FunctionsParser(GameObjectResolver parent)
        {
            _parent = parent;
        }

        public (Dictionary<string,FunctionDefinition> Definitions, Graph<string> ReferenceGraph) Parse(XDocument game)
        {
            foreach (var functionElement in game.Root?.Elements("function") ?? Enumerable.Empty<XElement>())
            {
                var definition = ProcessFunction(functionElement);
                _functionDefinitions.Add(definition.Name,definition);
            }

            return (_functionDefinitions, _functionReferenceGraph);
        }

          private FunctionDefinition ProcessFunction(XElement function)
        {
            var functionName = function.Attributes().FirstOrDefault(x => x.Name.LocalName == "name")?.Value;
            if (string.IsNullOrWhiteSpace(functionName)) ThrowMissingAttribute(function, "name");

            if (string.IsNullOrWhiteSpace(functionName))
                throw new ArgumentException(
                    $"Failed to process function definition, because function name is null. (definition: ${function})");

            var functionImplementation = function.Value;
          
            var functionParameters = function.Attributes().FirstOrDefault(x => x.Name.LocalName == "parameters")?.Value;            
            var returnType = ObjectType.Void;

            var returnTypeAttribute = function.Attributes().FirstOrDefault(x => x.Name.LocalName == "type")?.Value;
            if (!string.IsNullOrWhiteSpace(returnTypeAttribute))
                returnType = TypeUtil.TryParse(returnTypeAttribute, out returnType)
                    ? returnType
                    : ObjectType.Unknown;

            var newDefinition = new FunctionDefinition(functionName,
                (!string.IsNullOrWhiteSpace(functionParameters)) ? functionParameters?.Split(',') : Array.Empty<string>(),
                returnType,
                functionImplementation);

            GameObjectResolver.ScriptLexer.SetInputStream(new AntlrInputStream(newDefinition.Implementation));
            GameObjectResolver.ScriptParser.SetInputStream(new CommonTokenStream(GameObjectResolver.ScriptLexer));
            
            if (!_parent.FunctionParserErrors.TryGetValue(functionName, out var errors))
            {
                errors = new HashSet<BaseParserErrorException>();
                _parent.FunctionParserErrors.Add(functionName, errors);
            }

            GameObjectResolver.ErrorListener.Errors = errors;
            var parseTree = GameObjectResolver.ScriptParser.script();

            _functionReferenceGraph.Connect(newDefinition.Name, _referenceBuilder.Visit(parseTree));
            _referenceBuilder.Reset();

            return newDefinition;
        }

        private static void ThrowMissingAttribute(XElement element, string attribute)
        {
            throw new InvalidDataException(
                $"Found <{element}> element, but didn't find '{attribute}' attribute. This should not happen, and it is likely this ASLX game file is corrupted.");
        }
    }
}

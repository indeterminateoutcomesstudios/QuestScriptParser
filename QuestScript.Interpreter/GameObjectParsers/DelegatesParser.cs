using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.GameObjectParsers
{
    public class DelegatesParser
    {
        private readonly Dictionary<string, FunctionDefinition> _delegateDefinitions = new Dictionary<string, FunctionDefinition>();
        public Dictionary<string, FunctionDefinition> Parse(XDocument game)
        {
            foreach (var delegateElement in game.Root?.Elements("delegate") ?? Enumerable.Empty<XElement>())
            {
                var delegateName = delegateElement.Attributes().FirstOrDefault(x => x.Name.LocalName == "name")?.Value;
                if (string.IsNullOrWhiteSpace(delegateName)) ThrowMissingAttribute(delegateElement, "name");

                var functionParameters = delegateElement.Attributes().FirstOrDefault(x => x.Name.LocalName == "parameters")?.Value;            
                var returnType = ObjectType.Void;

                var returnTypeAttribute = delegateElement.Attributes().FirstOrDefault(x => x.Name.LocalName == "type")?.Value;
                if (!string.IsNullOrWhiteSpace(returnTypeAttribute))
                    returnType = TypeUtil.TryParse(returnTypeAttribute, out returnType)
                        ? returnType
                        : ObjectType.Unknown;

                _delegateDefinitions.Add(delegateName ?? throw new InvalidOperationException(),new FunctionDefinition(
                    delegateName,
                    !string.IsNullOrWhiteSpace(functionParameters) ? functionParameters?.Split(',') : Array.Empty<string>(),
                    returnType,null));
            }

            return _delegateDefinitions;
        }

        private static void ThrowMissingAttribute(XElement element, string attribute)
        {
            throw new InvalidDataException(
                $"Found <{element}> element, but didn't find '{attribute}' attribute. This should not happen, and it is likely this ASLX game file is corrupted.");
        }
    }
}

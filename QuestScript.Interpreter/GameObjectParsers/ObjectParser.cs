using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using QuestScript.Interpreter.Extensions;
using QuestScript.Interpreter.Helpers;
using QuestScript.Parser.ScriptElements;

namespace QuestScript.Interpreter.GameObjectParsers
{
    public class ObjectParser
    {
        private readonly Dictionary<string, FunctionDefinition> _delegateDefinitions;
        private readonly Graph<ObjectTypeDefinition> _typeInheritanceGraph;
        private readonly Dictionary<string, ObjectTypeDefinition> _objectTypeDefinitions;
        private readonly Dictionary<string, ObjectDefinition> _objectDefinitions = new Dictionary<string, ObjectDefinition>();

        public ObjectParser(Dictionary<string, FunctionDefinition> delegateDefinitions,
            Graph<ObjectTypeDefinition> typeInheritanceGraph,
            Dictionary<string, ObjectTypeDefinition> objectTypeDefinitions)
        {
            _delegateDefinitions = delegateDefinitions ?? throw new ArgumentNullException(nameof(delegateDefinitions));
            _typeInheritanceGraph = typeInheritanceGraph ?? throw new ArgumentNullException(nameof(typeInheritanceGraph));
            _objectTypeDefinitions = objectTypeDefinitions ?? throw new ArgumentNullException(nameof(objectTypeDefinitions));
        }

        public Dictionary<string, ObjectDefinition> Parse(XDocument gameFile)
        {
            foreach (var objectElement in gameFile.Root?.Elements("object") ?? Enumerable.Empty<XElement>())
            foreach (var item in ParseObject(objectElement, null))
                if(!_objectDefinitions.ContainsKey(item.Name))
                    _objectDefinitions.Add(item.Name, item);

            return _objectDefinitions;
        }

        private IEnumerable<ObjectDefinition> ParseObject(XElement objectElement, ObjectDefinition currentParent)
        {
            var objectName = objectElement.Attribute("name")?.Value ?? throw new InvalidDataException($"I see object definition without a name, this is invalid ASLX markup. ({objectElement})");
            var inheritsFrom = objectElement.DescendantsAndSelf()
                .Where(x => x.Name == "inherit")
                .Select(x => x.Attribute("name")?.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            var inheritedFields = Enumerable.Empty<Field>();

            foreach (var typeName in inheritsFrom)
            {
                if (_objectTypeDefinitions.TryGetValue(typeName, out var typeDefinition))
                {
                    inheritedFields = inheritedFields.Union(_typeInheritanceGraph.Traverse(typeDefinition).SelectMany(x => x.Fields));
                }
            }

            var fieldsWithInvalidXmlName = objectElement.DescendantsAndSelf()
                .Where(x => x.Name.LocalName == "attr" && x.Parent == objectElement)
                .Select(x =>
                {
                    var originalType = x.Attribute("type")?.Value ?? (!string.IsNullOrWhiteSpace(x.Value) ? "string" : "boolean");
                    return TypeUtil.TryParse(originalType, out var convertedType)
                        ? new Field(x.Attribute("name")?.Value, convertedType,originalType)
                        : 
                        _delegateDefinitions.ContainsKey(originalType) ? 
                            new Field(x.Attribute("name")?.Value, ObjectType.Delegate,originalType) : 
                            new Field(x.Attribute("name")?.Value, ObjectType.Unknown,originalType);
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name));

            var fields = objectElement.DescendantsAndSelf()
                .Where(x => x.Name.LocalName != "inherit" && 
                            x.Name.LocalName != "attr" && 
                            x.Name.LocalName != "elementType" && 
                            x.Name.LocalName != "object" && 
                            x.Parent == objectElement)                
                .Select(x =>
                {
                    var name = x.Name.LocalName;
                    var typeAsString = x.Attribute("type")?.Value ?? (!string.IsNullOrWhiteSpace(x.Value) ? "string" : "boolean");
                    return TypeUtil.TryParse(typeAsString, out var type) ? 
                        new Field(name, type,typeAsString) : 
                        _delegateDefinitions.ContainsKey(typeAsString) ? 
                            new Field(name, ObjectType.Delegate,typeAsString) : 
                            new Field(name, ObjectType.Unknown,typeAsString);
                });

            var fieldCollection = fields.Union(fieldsWithInvalidXmlName)
                                        .Union(inheritedFields)
                                        .ToDictionary(x => x.Name, x => x);
            
            Dictionary<string, MethodDefinition> GetMethodCollection(IEnumerable<Field> objectFields)
            {
                var results = new Dictionary<string, MethodDefinition>();
                foreach (var field in objectFields)
                {
                    if (_delegateDefinitions.TryGetValue(field.OriginalType, out var @delegate))
                        results.Add(@delegate.Name,@delegate.ToMethodDefinition(field.Name));
                }

                return results;
            }

            var nextParent = new ObjectDefinition(objectName)
            {
                InheritsFrom = inheritsFrom,
                Fields = fieldCollection,
                Methods = GetMethodCollection(fieldCollection.Values),
                Parent = currentParent
            };
            yield return nextParent;

            //support container objects
            foreach (var embeddedObject in objectElement.DescendantsAndSelf("object"))
            {
                if(embeddedObject == objectElement)
                    continue;
                
                foreach (var def in ParseObject(embeddedObject,nextParent))
                    yield return def;
            }

        }

    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.GameObjectParsers
{
    public class ObjectTypeParser
    {
        private readonly Dictionary<string, FunctionDefinition> _delegateDefinitions;
        private readonly Graph<ObjectTypeDefinition> _typeInheritanceGraph = new Graph<ObjectTypeDefinition>();

        public ObjectTypeParser(Dictionary<string, FunctionDefinition> delegateDefinitions)
        {
            _delegateDefinitions = delegateDefinitions;
        }

        public Graph<ObjectTypeDefinition> Parse(XDocument game)
        {
            foreach (var objectElement in game.Root?.Elements("type") ?? Enumerable.Empty<XElement>())
            {
                var objectTypeDefinition = ParseType(objectElement);
                _typeInheritanceGraph.AddVertex(objectTypeDefinition);
            }       

            return _typeInheritanceGraph;
        }      

        public ObjectTypeDefinition ParseType(XElement typeElement)
        {
            var typeName = typeElement.Attribute("name")?.Value ?? throw new InvalidDataException($"I see type definition without a name, this is invalid ASLX markup. ({typeElement})");
            var inheritsFrom = typeElement.DescendantsAndSelf()
                .Where(x => x.Name.LocalName == "inherit")
                .Select(x => x.Attribute("name")?.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x));

            var fieldsWithInvalidXmlName = typeElement.DescendantsAndSelf()
                .Where(x => x.Name.LocalName == "attr" && x.Parent == typeElement)
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

            var fields = typeElement.DescendantsAndSelf()
                .Where(x => x.Name.LocalName != "inherit" && 
                            x.Name.LocalName != "attr" && 
                            x.Name.LocalName != "elementType" &&                             
                            x.Parent == typeElement)                
                .Select(x =>
                {
                    switch (x.Name.LocalName)
                    {
                        case "obj":
                        {
                            //type attribute refers directly to an object
                            var typeAsString = x.Attribute("type")?.Value;
                            return new Field(x.Value,ObjectType.Object,typeAsString);
                        }
                        default:
                        {
                            var name = x.Name.LocalName;
                            var typeAsString = x.Attribute("type")?.Value ??
                                               (!string.IsNullOrWhiteSpace(x.Value) ? "string" : "boolean");
                            return TypeUtil.TryParse(typeAsString, out var type)
                                ? new Field(name, type, typeAsString)
                                :
                                _delegateDefinitions.ContainsKey(typeAsString)
                                    ?
                                    new Field(name, ObjectType.Delegate, typeAsString)
                                    :
                                    new Field(name, ObjectType.Unknown, typeAsString);
                        }
                    }
                });

            return new ObjectTypeDefinition(typeName,
                new HashSet<Field>(fields.Union(fieldsWithInvalidXmlName)), 
                new HashSet<string>(inheritsFrom));
        }
    }
}

using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace OpenAPI2XMLSchema
{
    public class Processor
    {
        public Processor()
        {
        }

        public static XmlSchemaSet Process(NJsonSchema.JsonSchema jsonSchema)
        {
            var schemaSet = new XmlSchemaSet();
            foreach(var definitionKey in jsonSchema.Definitions.Keys)
            {
                var definition = jsonSchema.Definitions[definitionKey];
                var schema = new XmlSchema()
                {
                    Id = definitionKey,
                };

                schema.Items.Add(GenerateComplexObject(definitionKey, definition.Properties));
                schemaSet.Add(schema);
            }

            schemaSet.Compile();
            return schemaSet;
        }

        public static void Process(string openAPIFileSpec, string outputDirectory)
        {
            if (!System.IO.File.Exists(openAPIFileSpec))
                throw new ArgumentException($"The path to the OpenAPISpec json file {openAPIFileSpec} is incorrect. Make sure that the file is in the location above or change the path", openAPIFileSpec);

            if(!System.IO.Directory.Exists(outputDirectory))
                throw new ArgumentException($"The directory {outputDirectory} is incorrect. please check the path", outputDirectory);

            var jsonSchema = NJsonSchema.JsonSchema.FromFileAsync(openAPIFileSpec).Result;

            var schemaSet = Processor.Process(jsonSchema);

            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                schema.Write(XmlWriter.Create($"{outputDirectory}/{schema.Id}.xsd"));
            }
        }

        private static XmlSchemaElement GenerateComplexObject(string name, IDictionary<string,JsonSchemaProperty> properties, bool isArray = false)
        {
            var schemaObject = new XmlSchemaElement()
            {
                Name = name,
                SchemaType = new XmlSchemaComplexType()
                {
                    Particle = GenerateParticleType(properties)
                }
            };

            return schemaObject;
        }

        private static XmlSchemaParticle GenerateParticleType(IDictionary<string,JsonSchemaProperty> properties, bool isArray = false)
        {
            var xmlSequence = new XmlSchemaSequence();
            foreach(var property in ResolveAllPropertiesFromType(properties, isArray))
            {
                xmlSequence.Items.Add(property);
            }

            return xmlSequence;
        }

        private static XmlSchemaElement GenerateArrayComplexObject(string name, string arrayElementName, JsonObjectType itemType, IDictionary<string,JsonSchemaProperty> itemTypeProperties)
        {
            XmlSchemaElement schemaObject = null;
            var sequence = new XmlSchemaSequence();
            if (itemType != JsonObjectType.Object)
            {
                var arrayItemElement = new XmlSchemaElement()
                {
                    Name = arrayElementName,
                    SchemaTypeName = GetType(itemType),
                    MinOccurs = 0,
                    MaxOccursString = "unbounded"
                };

                sequence.Items.Add(arrayItemElement);

                schemaObject = new XmlSchemaElement()
                {
                    Name = name,
                    SchemaType = new XmlSchemaComplexType()
                    {
                        Particle = sequence
                    }
                };
            }
            else
            {
                var arrayItemElement = new XmlSchemaElement()
                {
                    Name = arrayElementName,
                    SchemaType = new XmlSchemaComplexType()
                    {
                        Particle = GenerateParticleType(itemTypeProperties)
                    },
                    MinOccurs = 0,
                    MaxOccursString = "unbounded"
                };

                sequence.Items.Add(arrayItemElement);

                schemaObject = new XmlSchemaElement()
                {
                    Name = name,
                    SchemaType = new XmlSchemaComplexType()
                    {
                        Particle = sequence
                    }
                };
            }
            return schemaObject;
        }

        private static List<XmlSchemaElement> ResolveAllPropertiesFromType(IDictionary<string, JsonSchemaProperty> properties, bool isArray = false)
        {
            var xmlPropertyItems = new List<XmlSchemaElement>();
            XmlSchemaElement element;

            foreach (var key in properties.Keys)
            {
                var property = properties[key];

                if (property.Type == JsonObjectType.None
                    && property.Reference != null)
                {
                    var name = property.Reference.Xml != null ? property.Reference.Xml.Name : property.Name;

                    element = GenerateComplexObject(name, property.Reference.Properties);
                }
                else if(property.Type == JsonObjectType.Array)
                {
                    var itemType = property.Item.Type;
                    var itemProperties = property.Item.Properties;
                    var elementName = property.Item.Xml != null ? property.Item.Xml.Name : property.Xml.Name;

                    if (property.Item.HasReference)
                    {
                        itemType = property.Item.Reference.Type;
                        itemProperties = property.Item.Reference.Properties;
                        elementName = property.Item.Reference.Xml.Name;
                    }

                    element = GenerateArrayComplexObject(key,
                        elementName, itemType,
                        itemProperties);
                }
                else
                {
                    element = new XmlSchemaElement()
                    {
                        Name = key,
                        SchemaTypeName = GetType(property.Type)
                };
                }
                
                if (isArray) {
                    element.MaxOccursString = "unbounded";
                    element.MinOccurs = 0;
                }

                if(element != null)
                    xmlPropertyItems.Add(element);
            }
            return xmlPropertyItems;
        }

        private static XmlQualifiedName GetType(JsonObjectType type)
        {
            var schema = "http://www.w3.org/2001/XMLSchema";
            switch (type)
            {
                case JsonObjectType.Integer:
                    return new XmlQualifiedName("integer",schema);
                case JsonObjectType.String:
                    return new XmlQualifiedName("string", schema);
                case JsonObjectType.Number:
                    return new XmlQualifiedName("decimal", schema);
                case JsonObjectType.Boolean:
                    return new XmlQualifiedName("boolean", schema);
                case JsonObjectType.Array:
                    return new XmlQualifiedName("string", schema);
                default:
                    throw new ArgumentException($"Could not convert Open API type: {type}");
            }
        }
    }
}

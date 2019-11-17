using NJsonSchema;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace OpenAPI2XMLSchema.UnitTests
{
    [TestFixture]
    public class ProcessorTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [TestCase]
        public void Process_EmptyJsonDefinitions_ZeroXSDSchemas()
        {
            var schemaSet = Processor.Process(new NJsonSchema.JsonSchema());

            Assert.AreEqual(0, schemaSet.Count);
        }

        [TestCase]
        public void Process_FixedNumberOfJsonDefinitions_FixedNumberOfXSDSchemas()
        {
            var jsonSchema = new NJsonSchema.JsonSchema();
            jsonSchema.Definitions.Add("Order", new NJsonSchema.JsonSchema());
            jsonSchema.Definitions.Add("Customer", new NJsonSchema.JsonSchema());
            jsonSchema.Definitions.Add("Vendor", new NJsonSchema.JsonSchema());

            var schemaSet = Processor.Process(jsonSchema);

            Assert.AreEqual(3, schemaSet.Count);
        }

        [TestCase]
        public void Process_FullSwaggerSpecification_CorrectXSDOutput()
        {
            var jsonSchema = NJsonSchema.JsonSchema.FromFileAsync("swagger.json");
            var schemaSet = Processor.Process(jsonSchema.Result);

            Assert.AreEqual(6, schemaSet.Count);
            var orderSchema = schemaSet.Schemas().Cast<XmlSchema>().ToList().First(x => x.Id == "Order");

            Assert.IsTrue(orderSchema.Elements.Contains(new System.Xml.XmlQualifiedName("Order")));
            var elements = orderSchema.Elements.Names;

        }

        [TestCase]
        public void Process_FullSwaggerSpecification_WrittenToDisk()
        {
            var folder = "schemas";

            var jsonSchema = NJsonSchema.JsonSchema.FromFileAsync("swagger.json");
            var schemaSet = Processor.Process(jsonSchema.Result);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach(XmlSchema schema in schemaSet.Schemas())
            {
                schema.Write(XmlWriter.Create($"{folder}/{schema.Id}.xsd"));
            }
        }

        [TestCase]
        public void Process_FullSwaggerSpecificationWrittenToDisk_ValidatedAgainstExampleXMLs()
        {
            var folder = "validation_schemas";

            var jsonSchema = NJsonSchema.JsonSchema.FromFileAsync("swagger.json");
            var schemaSet = Processor.Process(jsonSchema.Result);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                schema.Write(XmlWriter.Create($"{folder}/{schema.Id}.xsd"));
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load("../../../TestXMLFiles/pet.xml");
            xmlDocument.Schemas = schemaSet;
            xmlDocument.Validate(booksSettingsValidationEventHandler);

        
            /*
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            xmlReaderSettings.Schemas.Add("", $"{folder}/pet.xsd");
            xmlReaderSettings.ValidationEventHandler += new ValidationEventHandler(booksSettingsValidationEventHandler);
            
            XmlReader books = XmlReader.Create("../../TextXMLFiles/pet.xml", xmlReaderSettings);
            */


            //while (books.Read()) { }
            /*var xsdReader = XmlReader.Create($"{folder}/pet.xsd");
            var xsd = XmlSchema.Read(xsdReader, ValidationCallback);
            var xmlDoc = XmlDataDocument.*/
        }

        static void booksSettingsValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Assert.AreEqual(expected: XmlSeverityType.Warning, actual: e.Severity, message: e.Message);
            /*if (e.Severity == XmlSeverityType.Warning)
            {
                Assert.("WARNING: ");
                Console.WriteLine(e.Message);
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                Console.Write("ERROR: ");
                Console.WriteLine(e.Message);
            }*/
        }
        static void ValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Console.Write("WARNING: ");
            else if (args.Severity == XmlSeverityType.Error)
                Console.Write("ERROR: ");

            Console.WriteLine(args.Message);
        }
    }
}

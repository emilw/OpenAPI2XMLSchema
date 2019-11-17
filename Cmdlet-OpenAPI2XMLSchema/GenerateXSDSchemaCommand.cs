using OpenAPI2XMLSchema;
using System;
using System.Management.Automation;
using System.Xml;
using System.Xml.Schema;

namespace Cmdlet_OpenAPI2XMLSchema
{
    [Cmdlet(VerbsCommon.New, "OpenAPI2XMLSchema")]
    public class GenerateXSDSchemaCommand : PSCmdlet
    {
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string OpenAPIFilePath { get; set; }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string OutDirectory { get; set; }
        protected override void ProcessRecord()
        {
            Processor.Process(OpenAPIFilePath, OutDirectory);
        }            
    }
}

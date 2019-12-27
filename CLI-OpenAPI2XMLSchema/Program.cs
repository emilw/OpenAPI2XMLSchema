using System;
using System.Collections;
using System.Collections.Generic;
using CommandLine;
using OpenAPI2XMLSchema;

namespace CLI_OpenAPI2XMLSchema
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Prints more details while generating the XML schemas")]
            public bool Verbose { get; set; }

            [Option('i', "inputfilepath", Required = true, HelpText = "Path to where the Swagger or OpenAPI file is on disk")]
            public string InputFilePath { get; set; }

            [Option('o', "outputdirpath", Required = true, HelpText = "Path to where the generated XSD schemas will be put")]
            public string OutputDirPath { get; set; }
        }
        static int Main(string[] args)
        {
            Console.WriteLine("Starting...");
            var result = Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    opts => RunProgram(opts),
                    errors => HandleParseErrors(errors)
                );
                
                return result;
        }


        private static int RunProgram(Options options)
        {
            if(options.Verbose)
                Console.WriteLine("Will write more detailed information");

            Console.WriteLine($"Generating XSD from {options.InputFilePath}");

            Processor.Process(options.InputFilePath, options.OutputDirPath);

            Console.WriteLine($"Result written to {options.OutputDirPath}");

            return 0;
        }

        private static int HandleParseErrors(IEnumerable<Error> errors)
        {
            return 4;
        }
    }
}

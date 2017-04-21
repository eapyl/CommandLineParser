using System;
using System.IO;
using CommandLineParser.Arguments;
using CommandLineParser.Validation;

namespace CommandLineParser
{
    internal class OutputFormatter
    {
        private readonly CommandLineParser parser;

        public OutputFormatter(CommandLineParser parser)
        {
            this.parser = parser;
        }

        /// <summary>
        /// Prints values of parsed arguments. Can be used for debugging. 
        /// </summary>
        public void ShowParsedArguments(bool showOmittedArguments = false)
        {
            Console.WriteLine(Messages.MSG_PARSING_RESULTS);
            Console.WriteLine("\t" + Messages.MSG_COMMAND_LINE);
            foreach (string arg in parser._argsNotParsed)
            {
                Console.Write(arg);
                Console.Write(" ");
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("\t" + Messages.MSG_PARSED_ARGUMENTS);
            foreach (Argument argument in parser.Arguments)
            {
                if (argument.Parsed)
                    argument.PrintValueInfo();
            }
            Console.WriteLine();
            Console.WriteLine("\t" + Messages.MSG_NOT_PARSED_ARGUMENTS);
            foreach (Argument argument in parser.Arguments)
            {
                if (!argument.Parsed)
                    argument.PrintValueInfo();
            }
            Console.WriteLine();
            if (parser.AdditionalArgumentsSettings.AcceptAdditionalArguments)
            {
                Console.WriteLine("\t" + Messages.MSG_ADDITIONAL_ARGUMENTS);

                foreach (string simpleArgument in parser.AdditionalArgumentsSettings.AdditionalArguments)
                {
                    Console.Write(simpleArgument + " ");
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }
        
        /// <summary>
        /// Prints arguments information and usage information to
        /// the <paramref name="outputStream"/>. 
        /// </summary>
        public void PrintUsage(TextWriter outputStream)
        {
            outputStream.WriteLine((string) parser.ShowUsageHeader);

            outputStream.WriteLine(Messages.MSG_USAGE);

            foreach (Argument argument in parser.Arguments)
            {
                outputStream.Write("\t");
                bool comma = false;
                if (argument.ShortName.HasValue)
                {
                    outputStream.Write("-" + argument.ShortName);
                    comma = true;
                }
                foreach (char c in argument.ShortAliases)
                {
                    if (comma)
                        outputStream.WriteLine(", ");
                    outputStream.Write("-" + c);
                    comma = true;
                }
                if (!String.IsNullOrEmpty(argument.LongName))
                {
                    if (comma)
                        outputStream.Write(", ");
                    outputStream.Write("--" + argument.LongName);
                    comma = true;
                }
                foreach (string str in argument.LongAliases)
                {
                    if (comma)
                        outputStream.Write(", ");
                    outputStream.Write("--" + str);
                    comma = true;
                }

                if (argument.Optional)
                    outputStream.Write(Messages.MSG_OPTIONAL);
                outputStream.WriteLine("... {0} ", argument.Description);

                if (!String.IsNullOrEmpty(argument.Example))
                {
                    outputStream.WriteLine(Messages.MSG_EXAMPLE_FORMAT, argument.Example);
                }

                if (!String.IsNullOrEmpty(argument.FullDescription))
                {
                    outputStream.WriteLine();
                    outputStream.WriteLine(argument.FullDescription);
                }
                outputStream.WriteLine();
            }

            if (parser.Certifications.Count > 0)
            {
                outputStream.WriteLine(Messages.CERT_REMARKS);
                foreach (ArgumentCertification certification in parser.Certifications)
                {
                    outputStream.WriteLine("\t" + certification.Description);
                }
                outputStream.WriteLine();
            }

            outputStream.WriteLine((string) parser.ShowUsageFooter);
        }
    }
}
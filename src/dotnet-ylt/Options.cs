using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace DotnetYaccLexTools
{

    [Verb("add-parser", HelpText = "Adds a new parser.")]
    internal class AddParserOptions
    {
        [Option('p', "parser-name", Required = true, HelpText = "Name of the parser.")]
        public string ParserName { get; set; } = null!;

        [Option('n', "namespace", HelpText = "Namespace in which the parse will be included.")]
        public string? Namespace { get; set; }
    }


    [Verb("add-calculator", HelpText = "Adds a Calculator example.")]
    internal class AddCalculatorOptions
    {
        [Option('n', "namespace", HelpText = "Namespace in which the parse will be included.")]
        public string? Namespace { get; set; }
    }

}

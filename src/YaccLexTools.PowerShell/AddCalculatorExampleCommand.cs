using System;
using System.Collections.Generic;


namespace YaccLexTools.PowerShell
{
    internal class AddCalculatorExampleCommand : AddParserCommand
    {

        public AddCalculatorExampleCommand(string projectDir, string projectRootNamespace)
            : base("Calculator", "", projectDir, projectRootNamespace)
        {
        }


        protected override void Execute(string parserName, string @namespace, Dictionary<string, string> tokens)
        {
            string path = parserName + "\\";

            AddFile(path + "Calculator.Language.analyzer.lex", "Calculator.Language.analyzer.lex", tokens);
            AddFile(path + "Calculator.Language.grammar.y", "Calculator.Language.grammar.y", tokens);
            AddFile(path + "Calculator.Parser.cs", "Calculator.Parser.cs", tokens);
            AddFile(path + "Calculator.Scanner.cs", "Calculator.Scanner.cs", tokens);
        }
    }
}

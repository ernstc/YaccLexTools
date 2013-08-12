using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using YaccLexTools.PowerShell.Extensions;
using YaccLexTools.PowerShell.Utilities;


namespace YaccLexTools.PowerShell
{

	internal class AddCalculatorExampleCommand : YaccLexToolsCommand
	{
		public AddCalculatorExampleCommand()
		{
			Execute(() => Execute());
		}


		public void Execute()
		{
			//Project.SetYltTarget("Calculator");

			AddFile("Calculator.parser");
			AddFile("Calculator.Language.analyzer.lex");
			AddFile("Calculator.Language.grammar.y");
			AddFile("Calculator.Parser.cs");
			AddFile("Calculator.Parser.Generated.cs");
			AddFile("Calculator.Scanner.cs");
			AddFile("Calculator.Scanner.Generated.cs");
		}


		private void AddFile(string templateName)
		{
			string template = LoadTemplate(templateName);
			Project.AddFile(templateName, template);
		}


		private string LoadTemplate(string name)
		{
			DebugCheck.NotEmpty(name);

			var stream = GetType().Assembly.GetManifestResourceStream("YaccLexTools.PowerShell.Templates.Example." + name);
			Debug.Assert(stream != null);

			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}
	}
}

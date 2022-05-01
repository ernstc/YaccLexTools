using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using YaccLexTools.PowerShell.Utilities;
using YaccLexTools.Utilities;

namespace YaccLexTools.PowerShell
{
    internal class AddSupportFilesCommand : YaccLexToolsCommand
    {

        protected string _projectDir;


        public AddSupportFilesCommand(string projectDir)
        {
            Check.NotEmpty(projectDir, "projectDir");

            _projectDir = projectDir;

            Execute(() =>
            {
                AddFile("GplexBuffers.cs");
                AddFile("ShiftReduceParserCode.cs");
            });
        }


        protected void AddFile(string templateName)
        {
            string content = LoadFileContent(templateName);
            AddProjectFile(_projectDir, templateName, content);
        }


        private string LoadFileContent(string name)
        {
            DebugCheck.NotEmpty(name);

            var stream = GetType().Assembly.GetManifestResourceStream("YaccLexTools.PowerShell.SupportFiles." + name);
            Debug.Assert(stream != null);

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

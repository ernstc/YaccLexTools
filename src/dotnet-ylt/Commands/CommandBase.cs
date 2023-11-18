using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YaccLexTools.Utilities;


namespace DotnetYaccLexTools.Commands
{

    public abstract class CommandBase
    {

        protected readonly Regex _namespaceRegex = new Regex(@"^[a-zA-Z_][a-zA-Z\d_]*(\.[a-zA-Z_][a-zA-Z\d_]*)*$", RegexOptions.Compiled);


        protected bool ValidateNamespace(string @namespace)
        {
            if (!_namespaceRegex.IsMatch(@namespace))
            {
                Console.Error.WriteLine("Namespace is not valid.");
                return false;
            }

            return true;
        }


        protected string? GetProjectFile()
        {
            var projectFiles = Directory.GetFiles(".", "*.csproj");
            if (projectFiles.Length == 0)
            {
                Console.Error.WriteLine("Project file (.csproj) not found.");
                return null;
            }

            if (projectFiles.Length > 1)
            {
                Console.Error.WriteLine("More project files (.csproj) found.");
                return null;
            }

            return projectFiles[0];
        }


        protected bool IsEnabledDefaultCompileItems(XElement root)
        {
            var enableDefaultCompileItemsNode = root
                .Elements(XName.Get("PropertyGroup"))
                .Elements(XName.Get("EnableDefaultCompileItems"))
                .FirstOrDefault();

            if (enableDefaultCompileItemsNode != null
                && bool.TryParse(enableDefaultCompileItemsNode.Value, out bool flag))
            {
                return flag;
            }

            var targetFrameworkNode = root
                .Elements(XName.Get("PropertyGroup"))
                .Elements(XName.Get("TargetFramework"))
                .FirstOrDefault();

            var targetFrameworksNode = root
                .Elements(XName.Get("PropertyGroup"))
                .Elements(XName.Get("TargetFrameworks"))
                .FirstOrDefault();


            string targetFramework = targetFrameworkNode?.Value ?? String.Empty;
            string targetFrameworks = targetFrameworksNode?.Value ?? String.Empty;

            List<string> targetFrameworkList = new List<string>
            {
                targetFramework
            };
            targetFrameworkList.AddRange(targetFrameworks.Split(';'));

            bool isLegacy = targetFrameworkList.Any(
                s => s.StartsWith("net1") || s.StartsWith("net2") || s.StartsWith("net3") || s.StartsWith("net4")
                );

            return !isLegacy;
        }


        protected string GetRootNamespace(string projectFile, XElement root)
        {
            var node = root
                .Elements(XName.Get("PropertyGruop"))
                .Elements(XName.Get("RootNamespace"))
                .FirstOrDefault();

            if (node == null)
            {
                string name = projectFile.Substring(projectFile.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                name = name.Substring(0, name.LastIndexOf('.'));
                return name.Replace('.', '_');
            }

            return node.Value;
        }


        protected string GetParserKey(string parserName, string? namespaceName)
        {
            string parserKey = parserName;
            if (!String.IsNullOrEmpty(namespaceName))
            {
                parserKey += $"-{namespaceName.Replace('.', '-')}Files";
            }

            return parserKey;
        }


        protected void AddFile(string relativePath, string templateName, Dictionary<string, string> tokens)
        {
            string content = new Template().GetContent(templateName, tokens);
            AddProjectFile(relativePath, content);
        }


        private static void AddProjectFile(string relativePath, string contents)
        {
            DebugCheck.NotEmpty(relativePath);
            Debug.Assert(!Path.IsPathRooted(relativePath));

            relativePath = relativePath.Replace('\\', Path.DirectorySeparatorChar);
            string? dirPath = Path.GetDirectoryName(relativePath);
            if (dirPath != null)
            {
                Directory.CreateDirectory(dirPath);
                File.WriteAllText(relativePath, contents);
            }
        }

    }
}

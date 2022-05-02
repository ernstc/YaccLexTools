using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommandLine;
using YaccLexTools.Utilities;

namespace DotnetYaccLexTools
{
    static class Program
    {

        static string? _versionString;
        static Version? _currentVersion;

        static void Main(string[] args)
        {
            _versionString = Assembly.GetEntryAssembly()?
               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
               .InformationalVersion;

            _versionString = _versionString?.Substring(0, _versionString.IndexOf("-"));
            _currentVersion = string.IsNullOrEmpty(_versionString) ? new Version() : new Version(_versionString);

            if (args.Length == 0)
            {
                ShowLogo();
            }

            CommandLine.Parser.Default.ParseArguments<
                AddCalculatorOptions,
                AddParserOptions
                >(args)

            .WithNotParsed(err =>
            {
                CheckForUpdates().Wait();
                Console.WriteLine("\nUsage:");
                Console.WriteLine("     dotnet ylt add-parser -p MyParser -n MyNamespace");
                Console.WriteLine("     dotnet ylt add-calculator\n");
            })

            .MapResult(
                (AddCalculatorOptions options) => { return Execute(AddCalculator, options); },
                (AddParserOptions options) => { return Execute(AddParser, options); },
                err => 1
                );
        }

        
        private static int Execute<T>(Func<T, Task> action, T options)
        {
            CheckForUpdates().Wait();
            action(options).Wait();
            return 0;
        }


        private static bool _showedLogo = false;
        private static void ShowLogo()
        {
            if (_showedLogo) return;
            _showedLogo = true;
            Console.WriteLine(
                            @"
"
                            );
        }



        #region Utilities
        static async Task CheckForUpdates()
        {
            if (_currentVersion != null)
            {
                var lastVersion = await Versions.CheckForNewVersion();
                if (lastVersion != null)
                {
                    var v1 = lastVersion.Build < 0 ?
                        new Version(lastVersion.Major, lastVersion.Minor) :
                        new Version(lastVersion.Major, lastVersion.Minor, lastVersion.Build);

                    var v2 = new Version(_currentVersion.Major, _currentVersion.Minor, _currentVersion.Build);

                    if (v1 > v2)
                    {
                        ShowLogo();
                        Console.WriteLine($"Current version: {_currentVersion}\n");
                        Console.WriteLine($">>> New version available: {lastVersion} <<<");
                        Console.WriteLine("Use the command below for upgrading to the latest version:\n");
                        Console.WriteLine("    dotnet tool update dotnet-ylt --global\n");
                        Console.WriteLine("------------------------------------------------------------");
                    }
                }
            }
        }

        #endregion


        #region Commands

        static async Task AddCalculator(AddCalculatorOptions options)
        {
            const string parserName = "Calculator";

            string? projectFile = GetProjectFile();
            if (projectFile == null)
            {
                return;
            }

            if (options.Namespace != null)
            {
                options.Namespace = options.Namespace.Replace('/', '.').Replace('\\', '.');
            }

            XDocument xProj = XDocument.Load(projectFile);

            string parserKey = GetParserKey(parserName, options.Namespace);
            string rootNamespace = GetRootNamespace(projectFile, xProj);

            if (options.Namespace == null)
            {
                options.Namespace = rootNamespace;
            }
            else if (options.Namespace != rootNamespace
                     && !options.Namespace.StartsWith(rootNamespace + "."))
            {
                options.Namespace = rootNamespace + "." + options.Namespace;
            }

            // Search for existing parser with the same key.
            var itemGroup = xProj.Root
                .Elements(XName.Get("ItemGroup"))
                .Where(e => e.Attribute(XName.Get("Label"))?.Value == parserKey)
                .FirstOrDefault();

            if (itemGroup != null)
            {
                // Parser already exists.
                Console.WriteLine($"Parser {parserName} already exists.");
                return;
            }

            Console.WriteLine("Adding Calculator example...");

            string relativePath = options.Namespace;
            if (options.Namespace.StartsWith(rootNamespace))
            {
                relativePath = relativePath.Substring(rootNamespace.Length);
                if (relativePath.StartsWith('.')) relativePath = relativePath.Substring(1);
            }
            if (relativePath.Length > 0)
            {
                relativePath = relativePath.Replace('.', '\\') + '\\';
            }
            relativePath += parserName + '\\';

            Dictionary<string, string> tokens = new Dictionary<string, string>();
            tokens.Add("parserLabel", parserKey);
            tokens.Add("parserName", parserName);
            tokens.Add("namespace", options.Namespace);
            tokens.Add("relativePath", relativePath);

            // Add item group to the project file.

            bool enabledDefaultCompileItems = IsEnabledDefaultCompileItems(xProj);

            string itemGroupXml = enabledDefaultCompileItems ?
                new Template().GetContent("project-ItemGroup1-fragment.xml", tokens) :
                new Template().GetContent("project-ItemGroup2-fragment.xml", tokens);

            itemGroup = xProj.Root.Elements(XName.Get("ItemGroup")).LastOrDefault();
            if (itemGroup != null)
            {
                itemGroup.AddAfterSelf(
                    XElement.Parse(itemGroupXml)
                    );
            }
            else
            {
                var propertyGroup = xProj.Root.Elements(XName.Get("PropertyGroup")).LastOrDefault();
                propertyGroup.AddAfterSelf(
                    XElement.Parse(itemGroupXml)
                    );
            }

            // Check if the PackageReference exists.

            bool hasReference = xProj.Root
                .Elements(XName.Get("ItemGroup"))
                .Elements(XName.Get("PackageReference"))
                .Any(x => x.Attribute(XName.Get("Include")).Value == "YaccLexTools");

            if (!hasReference)
            {
                string packageReferenceXml = new Template().GetContent("project-PackageReference-fragment.xml", tokens);

                var packageReference = xProj.Root
                    .Elements(XName.Get("ItemGroup"))
                    .Elements(XName.Get("PackageReference"))
                    .LastOrDefault();

                if (packageReference != null)
                {
                    packageReference.AddAfterSelf(
                        XElement.Parse(packageReferenceXml)
                        );
                }
                else
                {
                    xProj.Root.Add(
                       new XElement("ItemGroup",
                           XElement.Parse(packageReferenceXml)
                           )
                       );
                }
            }

            // Save the project file.

            await File.WriteAllTextAsync(projectFile, xProj.ToString());

            string prefix = relativePath + parserName;

            AddFile(prefix + ".Language.analyzer.lex", "Calculator.Language.analyzer.lex", tokens);
            AddFile(prefix + ".Language.grammar.y", "Calculator.Language.grammar.y", tokens);
            AddFile(prefix + ".Parser.cs", "Calculator.Parser.cs", tokens);
            AddFile(prefix + ".Scanner.cs", "Calculator.Scanner.cs", tokens);
        }


        static async Task AddParser(AddParserOptions options)
        {
            string? projectFile = GetProjectFile();
            if (projectFile == null)
            {
                return;
            }

            if (options.Namespace != null)
            {
                options.Namespace = options.Namespace.Replace('/', '.').Replace('\\', '.');
            }

            XDocument xProj = XDocument.Load(projectFile);
          
            string parserKey = GetParserKey(options.ParserName, options.Namespace);
            string rootNamespace = GetRootNamespace(projectFile, xProj);

            if (options.Namespace == null)
            {
                options.Namespace = rootNamespace;
            }
            else if (options.Namespace != rootNamespace 
                     && !options.Namespace.StartsWith(rootNamespace + "."))
            {
                options.Namespace = rootNamespace + "." + options.Namespace;
            }

            // Search for existing parser with the same key.
            var itemGroup = xProj.Root
                .Elements(XName.Get("ItemGroup"))
                .Where(e => e.Attribute(XName.Get("Label"))?.Value == parserKey)
                .FirstOrDefault();

            if (itemGroup != null)
            {
                // Parser already exists.
                Console.WriteLine($"Parser {options.ParserName} already exists.");
                return;
            }

            Console.WriteLine($"Adding parser {options.ParserName}...");

            string relativePath = options.Namespace;
            if (options.Namespace.StartsWith(rootNamespace))
            {
                relativePath = relativePath.Substring(rootNamespace.Length);
                if (relativePath.StartsWith('.')) relativePath = relativePath.Substring(1);
            }
            if (relativePath.Length > 0)
            {
                relativePath = relativePath.Replace('.', '\\') + '\\';
            }
            relativePath += options.ParserName + '\\';

            Dictionary<string, string> tokens = new Dictionary<string, string>();
            tokens.Add("parserLabel", parserKey);
            tokens.Add("parserName", options.ParserName);
            tokens.Add("namespace", options.Namespace);
            tokens.Add("relativePath", relativePath);

            // Add item group to the project file.

            bool enabledDefaultCompileItems = IsEnabledDefaultCompileItems(xProj);

            string itemGroupXml = enabledDefaultCompileItems ?
                new Template().GetContent("project-ItemGroup1-fragment.xml", tokens) :
                new Template().GetContent("project-ItemGroup2-fragment.xml", tokens);

            itemGroup = xProj.Root.Elements(XName.Get("ItemGroup")).LastOrDefault();
            if (itemGroup != null)
            {
                itemGroup.AddAfterSelf(
                    XElement.Parse(itemGroupXml)
                    );
            }
            else
            {
                var propertyGroup = xProj.Root.Elements(XName.Get("PropertyGroup")).LastOrDefault();
                propertyGroup.AddAfterSelf(
                    XElement.Parse(itemGroupXml)
                    );
            }

            // Check if the PackageReference exists.

            bool hasReference = xProj.Root
                .Elements(XName.Get("ItemGroup"))
                .Elements(XName.Get("PackageReference"))
                .Any(x => x.Attribute(XName.Get("Include")).Value == "YaccLexTools");
                
            if (!hasReference)
            {
                string packageReferenceXml = new Template().GetContent("project-PackageReference-fragment.xml", tokens);

                var packageReference = xProj.Root
                    .Elements(XName.Get("ItemGroup"))
                    .Elements(XName.Get("PackageReference"))
                    .LastOrDefault();

                if (packageReference != null)
                {
                    packageReference.AddAfterSelf(
                        XElement.Parse(packageReferenceXml)
                        );
                }
                else
                {
                    xProj.Root.Add(
                       new XElement("ItemGroup",
                           XElement.Parse(packageReferenceXml)
                           )
                       );
                }
            }

            // Save the project file.

            await File.WriteAllTextAsync(projectFile, xProj.ToString());

            string prefix = relativePath + options.ParserName;

            AddFile(prefix + ".Language.analyzer.lex", "___.Language.analyzer.lex", tokens);
            AddFile(prefix + ".Language.grammar.y", "___.Language.grammar.y", tokens);
            AddFile(prefix + ".Parser.cs", "___.Parser.cs", tokens);
            AddFile(prefix + ".Scanner.cs", "___.Scanner.cs", tokens);
        }

        #endregion


        private static string? GetProjectFile()
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


        private static bool IsEnabledDefaultCompileItems(XDocument xProj)
        {
            var enableDefaultCompileItemsNode = xProj.Root
                .Elements(XName.Get("PropertyGroup"))
                .Elements(XName.Get("EnableDefaultCompileItems"))
                .FirstOrDefault();

            if (enableDefaultCompileItemsNode != null
                && bool.TryParse(enableDefaultCompileItemsNode.Value, out bool flag))
            {
                return flag;
            }

            var targetFrameworkNode = xProj.Root
                .Elements(XName.Get("PropertyGroup"))
                .Elements(XName.Get("TargetFramework"))
                .FirstOrDefault();

            var targetFrameworksNode = xProj.Root
                .Elements(XName.Get("PropertyGroup"))
                .Elements(XName.Get("TargetFrameworks"))
                .FirstOrDefault();


            string targetFramework = targetFrameworkNode?.Value ?? String.Empty;
            string targetFrameworks = targetFrameworksNode?.Value ?? String.Empty;

            List<string> targetFrameworkList = new List<string>();
            targetFrameworkList.Add(targetFramework);
            targetFrameworkList.AddRange(targetFrameworks.Split(';'));

            bool isLegacy = targetFrameworkList.Any(
                s => s.StartsWith("net1") || s.StartsWith("net2") || s.StartsWith("net3") || s.StartsWith("net4")
                );

            return !isLegacy;
        }


        private static string GetRootNamespace(string projectFile, XDocument xProj)
        {
            var node = xProj.Root
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


        private static string GetParserKey(string parserName, string? namespaceName)
        {
            string parserKey = parserName;
            if (!String.IsNullOrEmpty(namespaceName))
            {
                parserKey += $"-{namespaceName.Replace('.', '-')}Files";
            }

            return parserKey;
        }


        private static void AddFile(string relativePath, string templateName, Dictionary<string, string> tokens)
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
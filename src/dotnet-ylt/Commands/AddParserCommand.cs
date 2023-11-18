using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;
using YaccLexTools.Utilities;


namespace DotnetYaccLexTools.Commands
{

    [Command("add-parser", Description = "Add a new parser.")]
    public class AddParserCommand : CommandBase
    {

        [Option("-p|--parser-name", Description = "Name of the parser.")]
        public string ParserName { get; set; } = null!;

        [Option("-n|--namespace", Description = "Namespace in which the parse will be included.")]
        public string? Namespace { get; set; }


        public async Task<int> OnExecute()
        {
            Console.WriteLine($"add-parser {Versions.VersionString}\n");

            string? projectFile = GetProjectFile();
            if (projectFile == null)
            {
                return 1;
            }

            if (Namespace != null)
            {
                Namespace = Namespace.Replace('/', '.').Replace('\\', '.');
                if (!ValidateNamespace(Namespace))
                {
                    return 1;
                }
            }

            XDocument xProj = XDocument.Load(projectFile);
            if (xProj.Root == null)
            {
                Console.WriteLine("Invalid project file.");
                return 1;
            }

            XElement root = xProj.Root!;

            string parserKey = GetParserKey(ParserName, Namespace);
            string rootNamespace = GetRootNamespace(projectFile, root);

            if (Namespace == null)
            {
                Namespace = rootNamespace;
            }
            else if (Namespace != rootNamespace
                     && !Namespace.StartsWith(rootNamespace + "."))
            {
                Namespace = rootNamespace + "." + Namespace;
            }

            // Search for existing parser with the same key.
            var itemGroup = root
                .Elements(XName.Get("ItemGroup"))
                .Where(e => e.Attribute(XName.Get("Label"))?.Value == parserKey)
                .FirstOrDefault();

            if (itemGroup != null)
            {
                // Parser already exists.
                Console.WriteLine($"Parser {ParserName} already exists.");
                return 1;
            }

            Console.WriteLine($"Adding parser {ParserName}...");

            string relativePath = Namespace;
            if (Namespace.StartsWith(rootNamespace))
            {
                relativePath = relativePath.Substring(rootNamespace.Length);
                if (relativePath.StartsWith('.')) relativePath = relativePath.Substring(1);
            }
            if (relativePath.Length > 0)
            {
                relativePath = relativePath.Replace('.', '\\') + '\\';
            }
            relativePath += ParserName + '\\';

            Dictionary<string, string> tokens = new Dictionary<string, string>
            {
                { "parserLabel", parserKey },
                { "parserName", ParserName },
                { "namespace", Namespace },
                { "relativePath", relativePath }
            };

            // Add item group to the project file.

            bool enabledDefaultCompileItems = IsEnabledDefaultCompileItems(root);

            string itemGroupXml = enabledDefaultCompileItems ?
                new Template().GetContent("project-ItemGroup1-fragment.xml", tokens) :
                new Template().GetContent("project-ItemGroup2-fragment.xml", tokens);

            itemGroup = root.Elements(XName.Get("ItemGroup")).LastOrDefault();
            if (itemGroup != null)
            {
                itemGroup.AddAfterSelf(
                    XElement.Parse(itemGroupXml)
                    );
            }
            else
            {
                var propertyGroup = root.Elements(XName.Get("PropertyGroup")).LastOrDefault();
                propertyGroup?.AddAfterSelf(
                    XElement.Parse(itemGroupXml)
                    );
            }

            // Check if the PackageReference exists.

            bool hasReference = root
                .Elements(XName.Get("ItemGroup"))
                .Elements(XName.Get("PackageReference"))
                .Any(x => x.Attribute(XName.Get("Include"))?.Value == "YaccLexTools");

            if (!hasReference)
            {
                string packageReferenceXml = new Template().GetContent("project-PackageReference-fragment.xml", tokens);

                var packageReference = root
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
                    root.Add(
                       new XElement("ItemGroup",
                           XElement.Parse(packageReferenceXml)
                           )
                       );
                }
            }

            // Save the project file.

            await File.WriteAllTextAsync(projectFile, xProj.ToString());

            string prefix = relativePath + ParserName;

            AddFile(prefix + ".Language.analyzer.lex", "___.Language.analyzer.lex", tokens);
            AddFile(prefix + ".Language.grammar.y", "___.Language.grammar.y", tokens);
            AddFile(prefix + ".Parser.cs", "___.Parser.cs", tokens);
            AddFile(prefix + ".Scanner.cs", "___.Scanner.cs", tokens);

            return 0;
        }

    }
}

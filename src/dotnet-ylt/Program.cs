using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;


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
            Console.WriteLine("Adding Calculator example...");
        }


        static async Task AddParser(AddParserOptions options)
        {
            Console.WriteLine($"Adding parser {options.ParserName}...");
        }

        #endregion
    }
}
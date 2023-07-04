using System;
using System.Reflection;
using System.Threading.Tasks;
using DotnetYaccLexTools.Commands;
using McMaster.Extensions.CommandLineUtils;


namespace DotnetYaccLexTools
{

    [Command("dotnet ylt")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(AddCalculatorCommand),
        typeof(AddParserCommand)
    )]
    internal class Program
    {

        static void Main(string[] args)
        {
            CheckForUpdates().Wait();

            // Execute command
            CommandLineApplication.Execute<Program>(args);
        }


#pragma warning disable CA1822

        protected int OnExecute(CommandLineApplication app)
        {
            ShowLogo();
            app.ShowHelp();

            Console.WriteLine(@"
Examples:
     dotnet ylt add-parser -p MyParser -n MyNamespace
     dotnet ylt add-calculator
");

            return 0;
        }

#pragma warning restore CA1822


        private static string GetVersion()
        {
            var assembly = typeof(Versions).Assembly;
            var copyright = assembly?.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            
            string? commit = Versions.CommitHash;
            commit = commit != null ? $" (commit {commit})" : "";

            return $"dotnet-ylt {Versions.CurrentVersion}{commit}\n{copyright}";
        }


        private static bool _showedLogo = false;
        private static void ShowLogo()
        {
            if (_showedLogo) return;
            _showedLogo = true;
            Console.WriteLine(
@"
 __   __                ___                _____           _     
 \ \ / /_ _  ___ ___   / / |    _____  __ |_   _|__   ___ | |___ 
  \ V / _` |/ __/ __| / /| |   / _ \ \/ /   | |/ _ \ / _ \| / __|
   | | (_| | (_| (__ / / | |__|  __/>  <    | | (_) | (_) | \__ \
   |_|\__,_|\___\___/_/  |_____\___/_/\_\   |_|\___/ \___/|_|___/
      _       _              _              _ _                  
   __| | ___ | |_ _ __   ___| |_      _   _| | |_                
  / _` |/ _ \| __| '_ \ / _ \ __|____| | | | | __|               
 | (_| | (_) | |_| | | |  __/ ||_____| |_| | | |_                
  \__,_|\___/ \__|_| |_|\___|\__|     \__, |_|\__|               
                                      |___/                      
");
        }



        #region Utilities

        static async Task CheckForUpdates()
        {
            if (Versions.CurrentVersion != null)
            {
                var lastVersion = await Versions.CheckForNewVersion();
                var currentVersion = Versions.CurrentVersion;
           
                var v1 = new Version(lastVersion.Major, lastVersion.Minor, lastVersion.Build);
                var v2 = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);

                if (v1 > v2)
                {
                    Console.WriteLine($@"
------------------------------------------------------------

>>> New version available: {lastVersion.ToString(lastVersion.Revision == 0 ? 3 : 4)} <<<
Use the command below for upgrading to the latest version:

    dotnet tool update dotnet-ylt --global

------------------------------------------------------------
");
                }
            }
        }

        #endregion

    }
}
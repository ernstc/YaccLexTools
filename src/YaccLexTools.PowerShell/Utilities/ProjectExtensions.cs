using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using YaccLexTools.PowerShell.Utilities;


namespace YaccLexTools.PowerShell.Extensions
{

    internal static class ProjectExtensions
    {
        public const int S_OK = 0;

        public static string GetTargetName(this Project project)
        {
            DebugCheck.NotNull(project);

            return project.GetPropertyValue<string>("AssemblyName");
        }

        public static string GetProjectDir(this Project project)
        {
            DebugCheck.NotNull(project);

            return project.GetPropertyValue<string>("FullPath");
        }

        /// <summary>
        ///     Gets the root namespace configured for a VS project.
        /// </summary>
        public static string GetRootNamespace(this Project project)
        {
            DebugCheck.NotNull(project);

            return project.GetPropertyValue<string>("RootNamespace");
        }

        public static void EditFile(this Project project, string path)
        {
            DebugCheck.NotNull(project);
            DebugCheck.NotEmpty(path);
            Debug.Assert(!Path.IsPathRooted(path));

            var absolutePath = Path.Combine(project.GetProjectDir(), path);
            var dte = project.DTE;

            if (dte.SourceControl != null
                && dte.SourceControl.IsItemUnderSCC(absolutePath)
                && !dte.SourceControl.IsItemCheckedOut(absolutePath))
            {
                dte.SourceControl.CheckOutItem(absolutePath);
            }
        }

        public static void AddFile(this Project project, string path, string contents)
        {
            DebugCheck.NotNull(project);
            DebugCheck.NotEmpty(path);
            Debug.Assert(!Path.IsPathRooted(path));

            var absolutePath = Path.Combine(project.GetProjectDir(), path);

            project.EditFile(path);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllText(absolutePath, contents);
        }

        private static T GetPropertyValue<T>(this Project project, string propertyName)
        {
            DebugCheck.NotNull(project);
            DebugCheck.NotEmpty(propertyName);

            var property = project.Properties.Item(propertyName);

            if (property == null)
            {
                return default(T);
            }

            return (T)property.Value;
        }

        private static DomainDispatcher GetDispatcher()
        {
            return (DomainDispatcher)AppDomain.CurrentDomain.GetData("yltDispatcher");
        }

    }
}

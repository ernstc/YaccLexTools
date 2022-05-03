using System;
using System.Diagnostics;
using System.IO;
using YaccLexTools.PowerShell.Utilities;
using YaccLexTools.Utilities;

namespace YaccLexTools.PowerShell
{
    internal abstract class YaccLexToolsCommand
    {

        private readonly AppDomain _domain;
        private readonly DomainDispatcher _dispatcher;

        protected YaccLexToolsCommand()
        {
            _domain = AppDomain.CurrentDomain;
            _dispatcher = (DomainDispatcher)_domain.GetData("yltDispatcher");
        }


        protected AppDomain Domain
        {
            get { return _domain; }
        }


        public void Execute(Action command)
        {
            DebugCheck.NotNull(command);

            Init();

            try
            {
                command();
            }
            catch (Exception ex)
            {
                Throw(ex);
            }
        }


        public virtual void WriteLine(string message)
        {
            DebugCheck.NotEmpty(message);

            _dispatcher.WriteLine(message);
        }


        public virtual void WriteWarning(string message)
        {
            DebugCheck.NotEmpty(message);

            _dispatcher.WriteWarning(message);
        }


        public void WriteVerbose(string message)
        {
            DebugCheck.NotEmpty(message);

            _dispatcher.WriteVerbose(message);
        }


        public T GetAnonymousArgument<T>(string name)
        {
            return (T)_domain.GetData(name);
        }


        private void Init()
        {
            _domain.SetData("wasError", false);
            _domain.SetData("error.Message", null);
            _domain.SetData("error.TypeName", null);
            _domain.SetData("error.StackTrace", null);
        }


        private void Throw(Exception ex)
        {
            DebugCheck.NotNull(ex);

            _domain.SetData("wasError", true);
            _domain.SetData("error.Message", ex.Message);
            _domain.SetData("error.TypeName", ex.GetType().FullName);
            _domain.SetData("error.StackTrace", ex.ToString());
        }


        protected void AddProjectFile(string projectDir, string path, string contents)
        {
            DebugCheck.NotNull(projectDir);
            DebugCheck.NotEmpty(path);
            Debug.Assert(!Path.IsPathRooted(path));

            var absolutePath = Path.Combine(projectDir, path);

            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllText(absolutePath, contents);
        }
    }
}

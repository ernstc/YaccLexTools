using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaccLexTools.PowerShell
{
	[Serializable]
	public sealed class ProjectTypeNotSupportedException : YaccLexToolsException
	{
		public ProjectTypeNotSupportedException(string message) : base(message) { }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaccLexTools.PowerShell
{
	[Serializable]
	public class YaccLexToolsException : Exception
	{
		public YaccLexToolsException() { }
		public YaccLexToolsException(string message) : base(message) { }
		public YaccLexToolsException(string message, Exception inner) : base(message, inner) { }
		protected YaccLexToolsException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}

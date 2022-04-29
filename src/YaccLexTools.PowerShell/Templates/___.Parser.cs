using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace $namespace$.$parserName$
{
    internal partial class $parserName$Parser
    {
        public $parserName$Parser() : base(null) { }

        public void Parse(string s)
        {
            byte[] inputBuffer = System.Text.Encoding.Default.GetBytes(s);
            MemoryStream stream = new MemoryStream(inputBuffer);
            this.Scanner = new $parserName$Scanner(stream);
            this.Parse();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace $namespace$.Calculator
{
    internal partial class CalculatorParser
    {
        public CalculatorParser() : base(null) { }

        public void Parse(string s)
        {
            byte[] inputBuffer = System.Text.Encoding.Default.GetBytes(s);
            MemoryStream stream = new MemoryStream(inputBuffer);
            this.Scanner = new CalculatorScanner(stream);
            this.Parse();
        }
    }
}

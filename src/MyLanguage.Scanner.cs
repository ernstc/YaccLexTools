using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLanguage
{
    internal partial class MyLanguageScanner
    {

        void GetNumber()
        {
            yylval.s = yytext;
            yylval.n = int.Parse(yytext);
        }
    }
}

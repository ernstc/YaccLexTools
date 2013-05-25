%namespace MyLanguage
%scannertype MyLanguageScanner
%visibility internal
%tokentype Token

%option stack, minimize, parser, verbose, persistbuffer, noembedbuffers 

Eol             (\r\n?|\n)
NotWh           [^ \t\r\n]
Space           [ \t]
Number          [0-9]+

%{

%}

%%

{Number}		{ GetNumber(); return (int)Token.NUMBER; }
{Space}+		/* skip */

%%
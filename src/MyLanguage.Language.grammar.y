%namespace MyLanguage
%partial
%parsertype MyLanguageParser
%visibility internal
%tokentype Token

%union { 
			public int n; 
			public string s; 
	   }

%start line

%token NUMBER

%%

line   : exp ';' '\n'                   { Console.WriteLine("result is {0}\n", $1.n);}
       ;

exp    : term                           {$$.n = $1.n;}
       | exp '+' term                   {$$.n = $1.n + $3.n;}
       | exp '-' term                   {$$.n = $1.n - $3.n;}
       ;

term   : factor                         {$$.n = $1.n;}
       | term '*' factor                {$$.n = $1.n * $3.n;}
       | term '/' factor                {$$.n = $1.n / $3.n;}
       ;

factor : number                         {$$.n = $1.n;}
       | '(' exp ')'                    {$$.n = $2.n;}
       ;

number : 
       | NUMBER
       ;

%%
%namespace Calculator
%partial
%parsertype CalculatorParser
%visibility internal
%tokentype Token

%union { 
			public int n; 
			public string s; 
	   }

%start line

%token NUMBER, OP_PLUS, OP_MINUS, OP_MULT, OP_DIV, P_OPEN, P_CLOSE

%%

line   : exp							{ Console.WriteLine("result is {0}\n", $1.n);}
       ;

exp    : term                           { $$.n = $1.n;			Console.WriteLine("Rule -> exp: {0}", $1.n); }
       | exp OP_PLUS term               { $$.n = $1.n + $3.n;	Console.WriteLine("Rule -> exp: {0} + {1}", $1.n, $3.n); }
       | exp OP_MINUS term              { $$.n = $1.n - $3.n;	Console.WriteLine("Rule -> exp: {0} - {1}", $1.n, $3.n); }
       ;

term   : factor							{$$.n = $1.n;			Console.WriteLine("Rule -> term: {0}", $1.n); }
       | term OP_MULT factor            {$$.n = $1.n * $3.n;	Console.WriteLine("Rule -> term: {0} * {1}", $1.n, $3.n); }
       | term OP_DIV factor             {$$.n = $1.n / $3.n;	Console.WriteLine("Rule -> term: {0} / {1}", $1.n, $3.n); }
       ; 

factor : number                         {$$.n = $1.n;			Console.WriteLine("Rule -> factor: {0}", $1.n); }
       | P_OPEN exp P_CLOSE             {$$.n = $2.n;			Console.WriteLine("Rule -> factor: ( {0} )", $3.n);}
       ;

number : 
       | NUMBER							{ Console.WriteLine("Rule -> number: {0}", $1.n); }
       ;

%%
# dotnet-ylt

This .NET tool is part of the [Yacc/Lex Tools](https://github.com/ernstc/YaccLexTools). It can be used on Windows, macOS and Linux for adding parsers to you .Net Core and .NET C# projects.

For installing the tool type the command below in the terminal
```powershell
dotnet tool install dotnet-ylt --global
```
Installing `dotnet-ylt` globally lets you install it just once and not for every project.

Adding a parser to a .NET console application is very easy. Open the terminal in the project folder and type the command
```powershell
dotnet ylt add-parser -p <parserName>
```
The parser name is given with the parameter `-p` or `--parser`.

For adding the calculator example:
```powershell
dotnet ylt add-calculator
```
![video-sample-1.gif](https://raw.githubusercontent.com/ernstc/YaccLexTools/dev-1.0.0/resources/video-sample-1.gif)

Both commands support the optional parameter `-n` or `--namespace` for creating the parser in the given namespace.

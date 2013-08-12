param($installPath, $toolsPath, $package, $project)


if (Get-Module | ?{ $_.Name -eq 'YaccLexTools' })
{
	Remove-AllParsers
    Remove-Module YaccLexTools
}

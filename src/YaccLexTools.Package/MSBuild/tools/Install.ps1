param($installPath, $toolsPath, $package, $project)

if (Get-Module | ?{ $_.Name -eq 'YaccLexTools' })
{
	# Fix settings from old version of YaccLexTools
	Update-YaccLexToolsSettings
}

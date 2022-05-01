param($installPath, $toolsPath, $package, $project)

Write-Host
Write-Host "Installing YaccLexTools 1.0.0 ..."
Write-Host "Type 'get-help YaccLexTools' to see all available YaccLexTools commands."
Write-Host

if (Get-Module | ?{ $_.Name -eq 'YaccLexTools' })
{
	# Fix settings from old version of YaccLexTools
	Update-YaccLexToolsSettings
}

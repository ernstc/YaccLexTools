param($installPath, $toolsPath, $package, $project)

Write-Host
Write-Host "Installing YaccLexTools 0.2.3 ..."
Write-Host "Type 'get-help YaccLexTools' to see all available YaccLexTools commands."

if (Get-Module | ?{ $_.Name -eq 'YaccLexTools' })
{
	#Add-YaccLexToolsSettings

	Update-YaccLexToolsSettings
}

param($installPath, $toolsPath, $package)

if (Get-Module | ?{ $_.Name -eq 'YaccLexTools' })
{
    Remove-Module YaccLexTools
}

Import-Module (Join-Path $toolsPath YaccLexTools.psd1)

Write-Host "Imported module YaccLexTools 1.0.0 ..."
Write-Host

param($installPath, $toolsPath, $package)

if (Get-Module | ?{ $_.Name -eq 'YaccLexTools' })
{
    Remove-Module YaccLexTools
}

Import-Module (Join-Path $toolsPath YaccLexTools.psd1)

Write-Host
Write-Host "Imported module YaccLexTools 1.0.0 ..."
Write-Host
Write-Host "Added Cmdlets"
Write-Host "-------------"
Write-Host "Add-Parser			     Adds a new parser."
Write-Host "Add-CalculatorExample    Adds a Calculator example."
Write-Host

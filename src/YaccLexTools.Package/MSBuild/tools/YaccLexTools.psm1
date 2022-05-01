# Copyright (c) Ernesto Cianciotta.  All rights reserved.

<#
.SYNOPSIS
    Adds a new parser.

.DESCRIPTION
    Adds a new parser to the project by creating all files necessary.

#>
function Add-Parser
{
	[CmdletBinding()] 
	param(
		[parameter(Position = 0,
            Mandatory = $true)]
		[string] $ParserName,
		[parameter(Position = 1)]
		[string] $Namespace
	)

	$project = Get-Project
	if ($project.Properties["TargetFrameworkMoniker"].Value -ne $null -and $project.Properties["TargetFrameworkMoniker"].Value.Contains('.NETFramework'))
	{
		# Only for .NETFramework project

		$ParserKey = Get-ParserKey $ParserName $Namespace

		$buildProject = Get-MSBuildProject
		$xml = $buildProject.Xml

		$itemGroup = $xml.ItemGroups | ?{ $_.Label -eq $ParserKey + 'Files' }
		if (!$itemGroup)
		{
			$runner = New-YltRunner $ProjectName $StartUpProjectName $null $ConfigurationTypeName

			try
			{
				Invoke-RunnerCommand $runner YaccLexTools.PowerShell.AddParserCommand @( $ParserName, $Namespace, $project.Properties["FullPath"].Value, $project.Properties["RootNamespace"].Value )
				$error = Get-RunnerError $runner

				if ($error)
				{
					Write-Host $error.StackTrace
					throw $error.Message
				}		
				$(Get-VSComponentModel).GetService([NuGetConsole.IPowerConsoleWindow]).Show()
			}
			finally
			{			
				Remove-Runner $runner		
			}
		}

		Add-ParserItems $ParserName $Namespace
	}
	else
	{
		Show-NotCompatibleAlert('Add-Parser')
		Write-Host "then use the command below from the project folder for adding a new parser:"
		Write-Host
		Write-Host "	dotnet ylt add-parser -p <parserName> -n <namespaceName>"
		Write-Host
		Write-Host "--------------------------------------------------------------------------"
	}
}



<#
.SYNOPSIS
    Adds a new parser example.

.DESCRIPTION
    Adds a calculator parser example.

#>
function Add-CalculatorExample
{
	[CmdletBinding()] 
	param() 

	$project = Get-Project
	if ($project.Properties["TargetFrameworkMoniker"].Value -ne $null -and $project.Properties["TargetFrameworkMoniker"].Value.Contains('.NETFramework'))
	{
		# Only for .NETFramework project

		$runner = New-YltRunner $ProjectName $StartUpProjectName $null $ConfigurationTypeName

		try
		{
			Invoke-RunnerCommand $runner YaccLexTools.PowerShell.AddCalculatorExampleCommand @( $project.Properties["FullPath"].Value, $project.Properties["RootNamespace"].Value )
			$error = Get-RunnerError $runner

			if ($error)
			{
				Write-Host $error.StackTrace
				throw $error.Message
			}		
			$(Get-VSComponentModel).GetService([NuGetConsole.IPowerConsoleWindow]).Show()

			Add-ParserItems 'Calculator' ''
		}
		finally
		{			
			Remove-Runner $runner		
		}
	}
	else
	{
		Show-NotCompatibleAlert('Add-CalculatorExample')
		Write-Host "then use the command below from the project folder for adding the calculator example:"
		Write-Host
		Write-Host "	dotnet ylt add-calculator"
		Write-Host
		Write-Host "--------------------------------------------------------------------------"
	}
}


function Show-NotCompatibleAlert($cmdLet)
{
	Write-Host "--------------------------------------------------------------------------"
	Write-Host "The Cmdlet '$cmdLet' cannot be used on .Net Core and .NET 5+ projects."
	Write-Host "For achieving the same results use the dotnet tool 'ylt' from the terminal"
	Write-Host "or install the YaccLexTools extension for Visual Studio."
	Write-Host
	Write-Host "For installing the 'ylt' tool use the command below:"
	Write-Host
	Write-Host "	dotnet tool install dotnet-ylt --global"
	Write-Host
}


function Get-SingleProject($name)
{
	if ($name)
	{
		$project = Get-Project $name

		if ($project -is [array])
		{
			throw "More than one project '$name' was found. Specify the full name of the one to use."
		}
	}
	else
	{
		$project = Get-Project
	}
    return $project
}


function Get-MSBuildProject 
{
    param(
        [parameter(ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    Process {
        (Resolve-ProjectName $ProjectName) | % {
            $path = $_.FullName
            @([Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($path))[0]
        }
    }
}


function Resolve-ProjectName 
{
    param(
        [parameter(ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    
    if($ProjectName) {
        $projects = Get-Project $ProjectName
    }
    else {
        # All projects by default
        $projects = Get-Project
    }
    
    $projects
}


function Get-PackageInstallPath($package)
{
    $componentModel = Get-VsComponentModel
    $packageInstallerServices = $componentModel.GetService([NuGet.VisualStudio.IVsPackageInstallerServices])

    $vsPackage = $packageInstallerServices.GetInstalledPackages() | ?{ $_.Id -eq $package.Id -and $_.Version -eq $package.Version }
    
    return $vsPackage.InstallPath
}


function Get-YaccLexToolsInstallPath($project)
{
    $package = Get-Package -ProjectName $project.FullName | ?{ $_.Id -eq 'YaccLexTools' }

    if (!$package)
    {
        $projectName = $project.Name

        throw "The YaccLexTools package is not installed on project '$projectName'."
    }
    
    return Get-PackageInstallPath $package
}


function New-DomainDispatcher($ToolsPath)
{
	$utilityAssemblyPath = (Join-Path $ToolsPath YaccLexTools.PowerShell.Utility.dll)
    $utilityAssembly = [System.Reflection.Assembly]::LoadFrom($utilityAssemblyPath)
    $dispatcher = $utilityAssembly.CreateInstance(
        'YaccLexTools.PowerShell.Utilities.DomainDispatcher',
        $false,
        [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Public,
        $null,
        $PSCmdlet,
        $null,
        $null)

    return $dispatcher
}


function New-AppDomainSetup($Project, $InstallPath)
{
    $info = New-Object System.AppDomainSetup -Property @{
            ShadowCopyFiles = 'true';
            ApplicationBase = $InstallPath;
            PrivateBinPath = 'tools';
            ConfigurationFile = ([AppDomain]::CurrentDomain.SetupInformation.ConfigurationFile)
        }
    
    $targetFrameworkVersion = (New-Object System.Runtime.Versioning.FrameworkName ($Project.Properties.Item('TargetFrameworkMoniker').Value)).Version

    if ($targetFrameworkVersion -lt (New-Object Version @( 4, 5 )))
    {
        $info.PrivateBinPath += ';lib\net40'
    }
    else
    {
        $info.PrivateBinPath += ';lib\net45'
    }

    return $info
}


function New-YltRunner($ProjectName, $StartUpProjectName, $ContextProjectName, $ConfigurationTypeName)
{
    $project = Get-SingleProject $ProjectName

    $installPath = Get-YaccLexToolsInstallPath $project
    $toolsPath = Join-Path $installPath tools

    $info = New-AppDomainSetup $project $installPath

    $domain = [AppDomain]::CreateDomain('YaccLexTools', $null, $info)
    $domain.SetData('project', $project)
    $domain.SetData('contextProject', $contextProject)
    $domain.SetData('startUpProject', $startUpProject)
    $domain.SetData('configurationTypeName', $ConfigurationTypeName)
    
    $dispatcher = New-DomainDispatcher $toolsPath
    $domain.SetData('yltDispatcher', $dispatcher)

    return @{
        Domain = $domain;
        ToolsPath = $toolsPath
    }
}


function Invoke-RunnerCommand($runner, $command, $parameters, $anonymousArguments)
{
    $domain = $runner.Domain

    if ($anonymousArguments)
    {
        $anonymousArguments.GetEnumerator() | %{
            $domain.SetData($_.Name, $_.Value)
        }
    }

	$libraryPath = (Join-Path $runner.ToolsPath YaccLexTools.PowerShell.dll)

    $domain.CreateInstanceFrom(
        $libraryPath,
        $command,
        $false,
        0,
        $null,
        $parameters,
        $null,
        $null) | Out-Null
}


function Get-RunnerError($runner)
{
    $domain = $runner.Domain

    if (!$domain.GetData('wasError'))
    {
        return $null
    }

    return @{
            Message = $domain.GetData('error.Message');
            TypeName = $domain.GetData('error.TypeName');
            StackTrace = $domain.GetData('error.StackTrace')
    }
}


function Remove-Runner($runner)
{
    [AppDomain]::Unload($runner.Domain)
}


function Remove-TargetDependency($target, $dependency)
{
	$v = $target.DependsOnTargets.Split(';')
	if ($v.Contains($dependency))
	{
		$s = ''
		foreach ($item in $v) { if (!($item -eq $dependency)) { $s += $item + ';' } }
		if ($s.Length -gt 0) { $s = $s.Substring(0, $s.Length - 1) }
		$target.DependsOnTargets = $s
	}
}


function Get-ParserKey($ParserName, $Namespace)
{
	if (!$Namespace)
	{
		return $ParserName
	}
	
	$key = $ParserName
	if ($Namespace.Length -gt 0)
	{
		$key += '-' + $Namespace.Replace('.', '-')
	}
	return $key
}


function Add-ParserItems($ParserName, $Namespace)
{
	$project = Get-Project
	$buildProject = Get-MSBuildProject
	$xml = $buildProject.Xml

	$ParserKey = Get-ParserKey $ParserName $Namespace

	if (!$Namespace) { $Namespace = '' }

	$rootNamespace = ($project.Properties | ?{ $_.Name -eq 'RootNamespace' }).Value
	$path = $Namespace.Replace('.', '\')
	if ($Namespace.StartsWith($rootNamespace))
	{
		$path = $path.Substring($rootNamespace.Length);
		if ($path.StartsWith('\')) { $path = $path.Substring(1) }
	}
	if ($path.Length -gt 0) { $path += '\' }
	$path += $ParserName + '\'
	
	$itemGroup = $xml.ItemGroups | ?{ $_.Label -eq $ParserKey + 'Files' }
	if (!$itemGroup)
	{
		$itemGroup = $xml.AddItemGroup()
		$itemGroup.Label = $ParserKey + 'Files'

		$item = $itemGroup.AddItem('Compile', $path + $ParserName + '.Parser.cs')
	
		$item = $itemGroup.AddItem('Compile', $path + $ParserName + '.Scanner.cs')
	
		$item = $itemGroup.AddItem('YaccFile', $path + $ParserName + '.Language.grammar.y')
		$void = $item.AddMetadata('OutputFile', $path + $ParserName + '.Parser.Generated.cs')
		$void = $item.AddMetadata('Arguments', '/gplex /nolines')
	
		$item = $itemGroup.AddItem('Compile', $path + $ParserName + '.Parser.Generated.cs')
		$void = $item.AddMetadata('AutoGen', 'True')
		$void = $item.AddMetadata('DesignTime', 'True')
		$void = $item.AddMetadata('DependentUpon', $ParserName + '.Language.grammar.y')
	
		$item = $itemGroup.AddItem('LexFile', $path + $ParserName + '.Language.analyzer.lex')
		$void = $item.AddMetadata('OutputFile', $path + $ParserName + '.Scanner.Generated.cs')

		$item = $itemGroup.AddItem('Compile', $path + $ParserName + '.Scanner.Generated.cs')
		$void = $item.AddMetadata('AutoGen', 'True')
		$void = $item.AddMetadata('DesignTime', 'True')
		$void = $item.AddMetadata('DependentUpon', $ParserName + '.Language.analyzer.lex')
	}

	Add-SupportFiles $project $buildProject

	$buildProject.Save()
}


<#
.SYNOPSIS
    Updates obsolete YaccLexTools settings.

.DESCRIPTION
    Updates obsolete YaccLexTools settings created with older versions (<= 0.2.2) of the package.

#>
function Update-YaccLexToolsSettings
{
	$project = Get-Project
	if ($project.Properties["TargetFrameworkMoniker"].Value -ne $null -and $project.Properties["TargetFrameworkMoniker"].Value.Contains('.NETFramework'))
	{
		# Only for .NETFramework project

		$buildProject = Get-MSBuildProject
		$xml = $buildProject.Xml

		$pgYltParsers = $xml.PropertyGroups | ?{ $_.Label -eq 'YltParsers' }
		if ($pgYltParsers)
		{
			Write-Host "Fixing YaccLexTools settings ..."
			Write-Host

			# Found obsolete setting, then update is needed.
			$parsers = $pgYltParsers.Properties | ?{ $_.Name -eq 'Names' }
			$v = $parsers.Value.Split(';')

			foreach ($ParserName in $v)
			{
				# Remove obsolete settings and target for the selected parser
				$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'Generate' + $ParserName + 'Properties' }
				if ($pg)
				{
					$pg.Parent.RemoveChild($pg)
				}

				$target = $xml.Targets | ?{ $_.Name -eq 'Generate' + $ParserName }
				if ($target)
				{
					$target.Parent.RemoveChild($target)
				}
			}

			#Fix .lex files
			$files = $xml.Items | ?{ $_.Include.EndsWith('.lex') }
			foreach ($f in $files)
			{
				$itemGroup = $f.Parent
				$fileName = $f.Include.SubString($f.Include.LastIndexOf('\') + 1)

				$dependent = @($itemGroup.Items | ?{ $_.Include.EndsWith('.cs') -and @($_.Children | ?{ $_.Name -eq 'DependentUpon' -and ($_.Value -eq $fileName) }).Count -gt 0 })[0]
				$itemGroup.RemoveChild($f);
				$item = $itemGroup.AddItem('LexFile', $f.Include)
				$void = $item.AddMetadata('OutputFile', $dependent.Include)
			}

			#Fix .y files
			$files = $xml.Items | ?{ $_.Include.EndsWith('.y') }
			foreach ($f in $files)
			{
				$itemGroup = $f.Parent
				$fileName = $f.Include.SubString($f.Include.LastIndexOf('\') + 1)

				$dependent = @($itemGroup.Items | ?{ $_.Include.EndsWith('.cs') -and @($_.Children | ?{ $_.Name -eq 'DependentUpon' -and ($_.Value -eq $fileName) }).Count -gt 0 })[0]
				$itemGroup.RemoveChild($f);
				$item = $itemGroup.AddItem('YaccFile', $f.Include)
				$void = $item.AddMetadata('OutputFile', $dependent.Include)
				$void = $item.AddMetadata('Arguments', '/gplex /nolines')
			}		

			#Remove .parser files
			$files = $xml.Items | ?{ $_.Include.EndsWith('.parser') }
			foreach ($f in $files)
			{
				$itemGroup = $f.Parent
				$fileName = $f.Include.SubString($f.Include.LastIndexOf('\') + 1)

				$dependentFiles = $itemGroup.Items | ?{ @($_.Children | ?{ $_.Name -eq 'DependentUpon' -and ($_.Value -eq $fileName) }).Count -gt 0 }
				foreach ($d in $dependentFiles)
				{
					$children = $d.Children | ?{ $_.Name -eq 'DependentUpon' }
					$d.RemoveChild($children)
				}

				$itemGroup.RemoveChild($f);
			}

			#Remove obsolete targets and other properties

			$target = $xml.Targets | ?{ $_.Name -eq 'BeforeBuild' }
			if ($target)
			{
				Remove-TargetDependency $target 'YltBuildGen'
				if ($target.DependsOnTargets -eq '')
				{
					$target.Parent.RemoveChild($target)
				}
			}

			$target = $xml.Targets | ?{ $_.Name -eq 'YltBuildGen' }
			if ($target)
			{
				$target.Parent.RemoveChild($target)
			}

			$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'YltProperties' }
			if ($pg)
			{
				$pg.Parent.RemoveChild($pg)
			}

			$pgYltParsers.Parent.RemoveChild($pgYltParsers)

			Add-SupportFilesContent $project $buildProject

			$project.Save()
		}
	}
}


function Add-SupportFiles($project, $buildProject)
{
	if ($project.Properties["TargetFrameworkMoniker"].Value -ne $null -and $project.Properties["TargetFrameworkMoniker"].Value.Contains('.NETFramework'))
	{
		# Only needed for .NETFramework project

		$itemGplexBuffers = @($buildProject.Items | ?{$_.EvaluatedInclude.Contains('GplexBuffers.cs')})[0]
		$itemShiftReduceParserCode = @($buildProject.Items | ?{$_.EvaluatedInclude.Contains('ShiftReduceParserCode.cs')})[0]

		if ($itemGplexBuffers.Count -gt 0)
		{
			$itemGroup = $itemGplexBuffers.Xml.Parent
		}
		else {
			if ($itemShiftReduceParserCode.Count -gt 0)
			{
				$itemGroup = $itemShiftReduceParserCode.Xml.Parent
			}
			else
			{
				$itemGroup = $xml.AddItemGroup()
			}
		}

		if ($itemGplexBuffers -eq $null)
		{
			$item = $itemGroup.AddItem('Compile', 'GplexBuffers.cs')
			$void = $item.AddMetadata('AutoGen', 'True')
			$void = $item.AddMetadata('DesignTime', 'True')
		}

		if ($itemShiftReduceParserCode -eq $null)
		{
			$item = $itemGroup.AddItem('Compile', 'ShiftReduceParserCode.cs')
			$void = $item.AddMetadata('AutoGen', 'True')
			$void = $item.AddMetadata('DesignTime', 'True')
		}
	}
}


function Add-SupportFilesContent($project, $buildProject)
{
    $runner = New-YltRunner $ProjectName $StartUpProjectName $null $ConfigurationTypeName

    try
    {
        Invoke-RunnerCommand $runner YaccLexTools.PowerShell.AddSupportFilesCommand @( $project.Properties["FullPath"].Value )
        $error = Get-RunnerError $runner

        if ($error)
        {
            Write-Host $error.StackTrace
            throw $error.Message
        }		
        $(Get-VSComponentModel).GetService([NuGetConsole.IPowerConsoleWindow]).Show()

		Add-SupportFiles $project $buildProject
    }
    finally
    {			
        Remove-Runner $runner		
    }	
}



Export-ModuleMember @( 'Add-Parser', 'Add-CalculatorExample', 'Update-YaccLexToolsSettings' )


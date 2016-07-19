function Install-Dotnet
{
  & where.exe dotnet 2>&1 | Out-Null

  if(($LASTEXITCODE -ne 0) -Or ((Test-Path Env:\APPVEYOR) -eq $true))
  {
    Write-Host "Dotnet CLI not found - downloading latest version"

    # Prepare the dotnet CLI folder
    $env:DOTNET_INSTALL_DIR="$(Convert-Path "$PSScriptRoot")\.dotnet\win7-x64"
    if (!(Test-Path $env:DOTNET_INSTALL_DIR))
    {
      mkdir $env:DOTNET_INSTALL_DIR | Out-Null
    }

    # Download the dotnet CLI install script
    if (!(Test-Path .\dotnet\install.ps1))
    {
      Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1" -OutFile ".\.dotnet\dotnet-install.ps1"
    }

    # Run the dotnet CLI install
    & .\.dotnet\dotnet-install.ps1

    # Add the dotnet folder path to the process.
    Remove-PathVariable $env:DOTNET_INSTALL_DIR
    $env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"
  }
}

function Remove-PathVariable
{
  [cmdletbinding()]
  param([string] $VariableToRemove)
  $path = [Environment]::GetEnvironmentVariable("PATH", "User")
  $newItems = $path.Split(';') | Where-Object { $_.ToString() -inotlike $VariableToRemove }
  [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join(';', $newItems), "User")
  $path = [Environment]::GetEnvironmentVariable("PATH", "Process")
  $newItems = $path.Split(';') | Where-Object { $_.ToString() -inotlike $VariableToRemove }
  [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join(';', $newItems), "Process")
}

function Restore-Packages
{
    param([string] $DirectoryName)
    & dotnet restore -v Warning ("""" + $DirectoryName + """")
}

function Test-Project
{
    param([string] $DirectoryName)
    & dotnet test -c Release ("""" + $DirectoryName + """")
}

# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}
########################
# THE BUILD!
########################

Push-Location $PSScriptRoot

if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

# Install Dotnet CLI
Install-Dotnet

# Package restore
Get-ChildItem -Path . -Filter *.xproj -Recurse | ForEach-Object { Restore-Packages $_.DirectoryName }

# Tests
Get-ChildItem -Path .\test -Filter *.xproj -Recurse | ForEach-Object { Test-Project $_.DirectoryName }

$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$revision = "{0:D4}" -f [convert]::ToInt32($revision, 10)

exec { & dotnet pack .\src\MediatR.Extensions.Microsoft.DependencyInjection -c Release -o .\artifacts --version-suffix=$revision }

Pop-Location
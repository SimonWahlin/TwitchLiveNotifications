param(
    $TwitchNetCommit
)

try {
    [version]$DotNetVersion = (dotnet --version) -split '-' | Select-Object -First 1
    if ($DotNetVersion.Major -lt 6) {
        throw
    }
}
catch {
    throw '.NET 6 is required to build. Get it at https://dotnet.microsoft.com/en-us/download/dotnet/6.0'
}

$RequiredModules = @(
    @{
       Name = 'Az.Accounts'
       RequiredVersion = '2.8.0'
    }
    @{
       Name = 'Az.KeyVault'
       RequiredVersion = '4.3.0'
    }
    @{
       Name = 'Az.Resources'
       RequiredVersion = '6.0.0'
    }
    @{
       Name = 'Az.Storage'
       RequiredVersion = '4.3.0'
    }
 )
 $RequiredModules | ForEach-Object { 
    $Existing = Get-Module $_.Name -ListAvailable | Select-Object -First 1
    if(-not $Existing -or $Existing.Version -lt $_.RequiredVersion)
    {
        Install-Module @_ -Force -Scope CurrentUser
    }
 }

if(-not $TwitchNetCommit) {
    $TwitchNetCommit = Get-Content -Path "$PSScriptRoot\..\.TwitchNetCommit"
    $TwitchNetCommit = $TwitchNetCommit.Trim()
}

$DependenciesPath = "$PSScriptRoot\..\dependencies"
if(-not (Test-Path -Path $DependenciesPath)) {
    $null = New-Item -Path $DependenciesPath -ItemType 'Directory'
}

Push-Location $DependenciesPath -StackName 'Dependencies'
if(Test-Path 'Twitch.Net') {
    Remove-Item 'Twitch.Net' -Recurse -Force
}
git clone 'https://github.com/SimonWahlin/Twitch.Net.git'
Push-Location 'Twitch.Net' -StackName 'Dependencies'
git checkout $TwitchNetCommit

while(Get-Location -Stack -StackName 'Dependencies' -ErrorAction 'Ignore') {
    Pop-Location -StackName 'Dependencies'
}

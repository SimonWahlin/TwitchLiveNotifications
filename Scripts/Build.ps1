param(
    [string]
    $Version
)

$ProjectPath = "$PSSCriptRoot/../src"
Push-Location -Path "$ProjectPath" -StackName 'deployCode.ps1'

Get-Item -Path 'bin/publish' -ErrorAction 'SilentlyContinue' | Remove-Item -Recurse -Force
Get-Item -Path 'bin/TwitchLiveNotifications.zip' -ErrorAction 'SilentlyContinue' | Remove-Item -Force
$dotnetArgs = @(
    'publish'
    'TwitchLiveNotifications.csproj'
    '--output', 'bin/publish'
    '--configuration', 'release'
    # '--no-self-contained'
    # '--runtime', 'linux-x64'
    # '-p:PublishReadyToRun=true'
    # '-p:PublishTrimmed=true'
    [string]::IsNullOrEmpty($Version) ? '' : "-p:Version=$Version"
)
dotnet $dotnetArgs
# Try to use RTR packages in the future:
# dotnet publish --configuration release --runtime linux-x64  --output bin/publish

Set-Location -Path 'bin/publish'
[System.IO.Compression.ZipFile]::CreateFromDirectory("$PSSCriptRoot/../src/bin/publish","$PSSCriptRoot/../src/bin/TwitchLiveNotifications.zip",[System.IO.Compression.CompressionLevel]::Optimal, $false)

Pop-Location -StackName 'deployCode.ps1'
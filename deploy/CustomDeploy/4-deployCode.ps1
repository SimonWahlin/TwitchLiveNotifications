<#
.SYNOPSIS
    Script to deploy code to Azure
.NOTES
    Requires modules Az.Storage and Az.Accounts
.EXAMPLE
    Test-MyTestFunction -Verbose
    Explanation of the function or its result. You can include multiple examples with additional .EXAMPLE lines
#>
param(
    # Just the name of your storageaccount. If access keys are disabled, remember that the user signed in needs data access.
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $StorageAccountName,

    # ResourceId in format: /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/<RESOURCE_GROUP_NAME>/providers/Microsoft.Web/sites/<FUNCTION_APP_NAME>
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $FunctionAppResourceId,

    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $ConfigFilePath
)

. "$PSSCriptRoot/helperFunctions.ps1"

if ([string]::IsNullOrEmpty($ConfigFilePath)) {
    $ConfigFilePath = "$PSScriptRoot/../functionapp.config.json"
}

'StorageAccountName', 'FunctionAppResourceId' | Assert-ConfigValueOrDefault -ConfigFilePath $ConfigFilePath

$ProjectPath = [System.IO.Path]::Join("$PSScriptRoot",'.','..','..','src')
Push-Location -Path "$ProjectPath" -StackName 'deployCode.ps1'

Get-Item -Path 'bin/publish' -ErrorAction 'SilentlyContinue' | Remove-Item -Recurse -Force
dotnet build --output bin/publish --configuration release

Set-Location -Path 'bin/publish'
Compress-Archive -Path '.\*' -DestinationPath '..\TwitchLiveNotifications.zip' -Force

$StorageContext = New-AzStorageContext -StorageAccountName $StorageAccountName -UseConnectedAccount
Set-AzStorageBlobContent -Container 'deploy' -Context $StorageContext -File '..\TwitchLiveNotifications.zip' -Force
Invoke-AzRest -Uri "https://management.azure.com$FunctionAppResourceId/syncfunctiontriggers?api-version=2016-08-01" -Method POST
Pop-Location -StackName 'deployCode.ps1'

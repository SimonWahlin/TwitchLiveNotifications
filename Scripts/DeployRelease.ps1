param(
    $FunctionAppResourceId,
    $StorageAccountName,
    $Version = 'latest',
    $PackageName = 'TwitchLiveNotifications.zip',
    $ResourceManagerEndpoint = 'https://management.azure.com'
)

$Version = $Version -eq 'latest' ? 'latest/download' : "download/$Version"
$Uri = "https://github.com/SimonWahlin/TwitchLiveNotifications/releases/$Version/$PackageName"
Write-Output "Using Uri: $Uri"

Invoke-WebRequest -Uri $Uri -OutFile "$PackageName"
Write-Output "Downloaded $PackageName"
Write-Output "Using storage account: $StorageAccountName"
$StorageContext = New-AzStorageContext -StorageAccountName $StorageAccountName -UseConnectedAccount
Set-AzStorageBlobContent -Container 'deploy' -Context $StorageContext -File "$PackageName" -Force
Invoke-AzRest -Uri "$ResourceManagerEndpoint$FunctionAppResourceId/syncfunctiontriggers?api-version=2016-08-01" -Method POST
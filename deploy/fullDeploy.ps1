. "$PSSCriptRoot/CustomDeploy/helperFunctions.ps1"

$ConfigFilePath = "$PSScriptRoot/functionapp.config.json"

$deployKeyVaultSplat = @{
    KeyVaultResourceGroupName = ''
    KeyVaultName = ''
    Location = ''
    ConfigFilePath = $ConfigFilePath
}
$Deployment = & "$PSScriptRoot/CustomDeploy/1-deployKeyVault.ps1" @deployKeyVaultSplat -ErrorAction 'Stop'
Write-Output $Deployment
if($Deployment.ProvisioningState -ne 'Succeeded') {
    return
}

$createSecretsSplat = @{
    KeyVaultName = ''
    DiscordWebhookUri = [securestring]::new()
    TwitchClientId = [securestring]::new()
    TwitchClientSecret = [securestring]::new()
    TwitchSignatureSecret = [securestring]::new()
    TwitterConsumerKey = [securestring]::new()
    TwitterConsumerSecret = [securestring]::new()
    TwitterAccessToken = [securestring]::new()
    TwitterAccessTokenSecret = [securestring]::new()
    ConfigFilePath = $ConfigFilePath
}
& "$PSScriptRoot/CustomDeploy/2-createSecrets.ps1" @createSecretsSplat -ErrorAction 'Stop'

$deployFunctionAppSplat = @{
    FunctionAppResourceGroupName = ''
    FunctionAppName = ''
    KeyVaultName = ''
    DiscordTemplateOnFollow = ''
    DiscordTemplateOnStreamOnline = ''
    TwitterTemplateOnFollow = ''
    TwitterTemplateOnStreamOnline = ''
    Location = ''
    ConfigFilePath = $ConfigFilePath
}
$Deployment = & "$PSScriptRoot/CustomDeploy/3-deployFunctionApp.ps1" @deployFunctionAppSplat -ErrorAction 'Stop'
Write-Output $Deployment
if($Deployment.ProvisioningState -ne 'Succeeded') {
    return
}

$deployCodeSplat = @{
    StorageAccountName = ''
    FunctionAppResourceId = ''
    ConfigFilePath = $ConfigFilePath
}
& "$PSScriptRoot/CustomDeploy/4-deployCode.ps1" @deployCodeSplat -ErrorAction 'Stop'

$getFunctionAccessKeySplat = @{
    FunctionAppResourceId = ''
    ConfigFilePath = $ConfigFilePath
}
$null = & "$PSScriptRoot/CustomDeploy/5-getFunctionAccessKey.ps1" @getFunctionAccessKeySplat -ErrorAction 'Stop'






















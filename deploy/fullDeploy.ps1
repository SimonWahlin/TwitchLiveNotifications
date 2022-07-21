. "$PSSCriptRoot/helperFunctions.ps1"

if ([string]::IsNullOrEmpty($ConfigFilePath)) {
    $ConfigFilePath = "$PSScriptRoot/functionapp.config.json"
}

$deployKeyVaultSplat = @{
    KeyVaultResourceGroupName = ''
    KeyVaultName = ''
    Location = ''
    ConfigFilePath = ''
}

& "$PSScriptRoot/1-deployKeyVault.ps1" @deployKeyVaultSplat

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
    ConfigFilePath = ''
}
& "$PSScriptRoot/2-createSecrets.ps1" @createSecretsSplat

$deployFunctionAppSplat = @{
    FunctionAppResourceGroupName = ''
    FunctionAppName = ''
    KeyVaultName = ''
    Location = ''
    ConfigFilePath = ''
}
& "$PSScriptRoot/3-deployFunctionApp.ps1" @deployFunctionAppSplat

$deployCodeSplat = @{
    StorageAccountName = ''
    FunctionAppResourceId = ''
}
& "$PSScriptRoot/4-deployCode.ps1" @deployCodeSplat

$getFunctionAccessKeySplat = @{
    FunctionAppResourceId = ''
}
$null = & "$PSScriptRoot/5-getFunctionAccessKey.ps1" @getFunctionAccessKeySplat






















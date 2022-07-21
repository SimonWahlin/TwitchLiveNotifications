param(
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $KeyVaultName,
    
    [Parameter(Mandatory)]
    [securestring]
    [AllowNull()]
    $DiscordWebhookUri,
    
    [Parameter(Mandatory)]
    [securestring]
    [AllowNull()]
    $TwitchClientId,
    
    [Parameter(Mandatory)]
    [securestring]
    [AllowNull()]
    $TwitchClientSecret,
    
    [Parameter(Mandatory)]
    [securestring]
    [AllowNull()]
    $TwitchSignatureSecret,
    
    [Parameter(Mandatory)]
    [securestring]
    [AllowNull()]
    $TwitterConsumerKey,
    
    [Parameter(Mandatory)]
    [securestring]
    [AllowNull()]
    $TwitterConsumerSecret,
    
    [Parameter(Mandatory)]
    [securestring]
    [AllowNull()]
    $TwitterAccessToken,
    
    [Parameter(Mandatory)]
    [securestring]
    [AllowNull()]
    $TwitterAccessTokenSecret,

    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $ConfigFilePath
)

. "$PSSCriptRoot/helperFunctions.ps1"

if ([string]::IsNullOrEmpty($ConfigFilePath)) {
    $ConfigFilePath = "$PSScriptRoot/functionapp.config.json"
}

'KeyVaultName' | Assert-ConfigValueOrDefault -ConfigFilePath $ConfigFilePath

$Secrets = @(
    'DiscordWebhookUri',
    'TwitchClientId',
    'TwitchClientSecret',
    'TwitchSignatureSecret',
    'TwitterConsumerKey',
    'TwitterConsumerSecret',
    'TwitterAccessToken',
    'TwitterAccessTokenSecret'
)
$Secrets | Assert-ConfigValueOrDefault -ConfigFilePath $ConfigFilePath

foreach ($secret in $Secrets) {
    $Value = Get-Variable -Name $secret -ValueOnly -ErrorAction 'Stop'
    Set-AzKeyVaultSecret -VaultName $KeyVaultName -Name $secret -SecretValue $value
}
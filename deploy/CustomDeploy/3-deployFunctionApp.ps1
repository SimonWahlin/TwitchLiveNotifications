param(
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $FunctionAppResourceGroupName,

    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $FunctionAppName,

    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $KeyVaultName,
    
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $KeyVaultResourceId,

    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $DiscordTemplateOnFollow,
    
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $DiscordTemplateOnStreamOnline,
    
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $TwitterTemplateOnFollow,
    
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $TwitterTemplateOnStreamOnline,

    
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $Location,

    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $ConfigFilePath
)

. "$PSSCriptRoot/helperFunctions.ps1"

if ([string]::IsNullOrEmpty($ConfigFilePath)) {
    $ConfigFilePath = "$PSScriptRoot/../functionapp.config.json"
}

@(
    'FunctionAppResourceGroupName'
    'FunctionAppName'
    'KeyVaultName'
    'KeyVaultResourceId'
    'DiscordTemplateOnFollow'
    'DiscordTemplateOnStreamOnline'
    'TwitterTemplateOnFollow'
    'TwitterTemplateOnStreamOnline'
    'Location'
) | Assert-ConfigValueOrDefault -ConfigFilePath $ConfigFilePath

Assert-ResourceGroup -ResourceGroupName $FunctionAppResourceGroupName -Location $Location -ErrorAction 'Stop'

[string]$ParametersFilePath = Resolve-Path "$PSScriptRoot/../Templates/functionApp.parameters.json"
[string]$TemplateFilePath = Resolve-Path "$PSScriptRoot/../Templates/functionApp.bicep"
$PrincipalId = Invoke-AzRest -Uri 'https://graph.microsoft.com/v1.0/me' | Select-Object -ExpandProperty Content | ConvertFrom-Json | Select-Object -ExpandProperty Id

$ParametersObject = Import-ParametersFile -Path $ParametersFilePath -ReplaceTokens @{
    '{{FunctionAppName}}'               = $FunctionAppName
    '{{KeyVaultName}}'                  = $KeyVaultName
    '{{KeyVaultResourceId}}'            = $KeyVaultResourceId
    '{{YourPrincipalId}}'               = $PrincipalId
    '{{DiscordTemplateOnFollow}}'       = $DiscordTemplateOnFollow
    '{{DiscordTemplateOnStreamOnline}}' = $DiscordTemplateOnStreamOnline
    '{{TwitterTemplateOnFollow}}'       = $TwitterTemplateOnFollow
    '{{TwitterTemplateOnStreamOnline}}' = $TwitterTemplateOnStreamOnline
}

Write-Verbose -Message "Deploying resources..." -Verbose
$newAzResourceGroupDeploymentSplat = @{
    Name                    = 'TwitchLiveNotifications'
    ResourceGroupName       = $FunctionAppResourceGroupName
    TemplateParameterObject = $ParametersObject
    TemplateFile            = $TemplateFilePath
}
$Deployment = New-AzResourceGroupDeployment @newAzResourceGroupDeploymentSplat
Write-Output $Deployment
if ($Deployment.ProvisioningState -eq 'Succeeded') {
    Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'FunctionAppResourceId' -Value $Deployment.Outputs['functionAppId'].Value
    Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'StorageAccountName' -Value $Deployment.Outputs['storageAccountName'].Value
    Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'CallBackUri' -Value $Deployment.Outputs['callBackUrl'].Value
}

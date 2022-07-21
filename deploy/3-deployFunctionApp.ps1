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
    $Location,

    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $ConfigFilePath
)

. "$PSSCriptRoot/helperFunctions.ps1"

if ([string]::IsNullOrEmpty($ConfigFilePath)) {
    $ConfigFilePath = "$PSScriptRoot/functionapp.config.json"
}

'FunctionAppResourceGroupName', 'FunctionAppName', 'KeyVaultName', 'Location' | Assert-ConfigValueOrDefault -ConfigFilePath $ConfigFilePath

Assert-ResourceGroup -ResourceGroupName $FunctionAppResourceGroupName -Location $Location -ErrorAction 'Stop'

[string]$ParametersFilePath = Resolve-Path "$PSScriptRoot/functionApp.parameters.json"
[string]$TemplateFilePath = Resolve-Path "$PSScriptRoot/functionApp.bicep"
$PrincipalId = Get-AzADUser -UserPrincipalName (Get-AzContext).Account.Id -ErrorAction 'Stop' | Select-Object -ExpandProperty Id

$ParametersObject = Import-ParametersFile -Path $ParametersFilePath -ReplaceTokens @{
    '{{FunctionAppName}}' = $FunctionAppName
    '{{KeyVaultName}}' = $KeyVaultName
    '{{YourPrincipalId}}' = $PrincipalId
}

Write-Verbose -Message "Deploying resources..." -Verbose
$newAzResourceGroupDeploymentSplat = @{
    Name                    = 'TwitchLiveNotifications'
    ResourceGroupName       = $FunctionAppResourceGroupName
    TemplateParameterObject = $ParametersObject
    TemplateFile            = $TemplateFilePath
}
$Deployment = New-AzResourceGroupDeployment @newAzResourceGroupDeploymentSplat
if ($Deployment.ProvisioningState -eq 'Succeeded') {
    Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'FunctionAppResourceId' -Value $Deployment.Outputs['functionAppId'].Value
    Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'StorageAccountName' -Value $Deployment.Outputs['storageAccountName'].Value
    Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'CallBackUri' -Value $Deployment.Outputs['storageAccountName'].Value
}

return $Deployment
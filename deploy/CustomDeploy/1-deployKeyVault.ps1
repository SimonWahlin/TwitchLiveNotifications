param(
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $KeyVaultResourceGroupName,
    
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
    $ConfigFilePath = "$PSScriptRoot/../functionapp.config.json"
}

'KeyVaultResourceGroupName', 'KeyVaultName', 'Location' | Assert-ConfigValueOrDefault -ConfigFilePath $ConfigFilePath

Assert-ResourceGroup -ResourceGroupName $KeyVaultResourceGroupName -Location $Location -ErrorAction 'Stop'

$PrincipalId = Invoke-AzRest -Uri 'https://graph.microsoft.com/v1.0/me' | Select-Object -ExpandProperty Content | ConvertFrom-Json | Select-Object -ExpandProperty Id

Write-Verbose -Message "Deploying resources..." -Verbose

$newAzResourceGroupDeploymentSplat = @{
    Name                        = 'TwitchLiveNotifications-KeyVault'
    ResourceGroupName           = $KeyVaultResourceGroupName
    TemplateFile                = "$PSSCriptRoot/../Templates/keyVault.bicep"
    keyVaultName                = $KeyVaultName
    secretsOfficerPrincipalId   = $PrincipalId
    secretsOfficerPrincipalType = 'User'
    location                    = $Location
}
$Deployment = New-AzResourceGroupDeployment @newAzResourceGroupDeploymentSplat

if ($Deployment.ProvisioningState -eq 'Succeeded') {
    Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'KeyVaultResourceGroupName' -Value $Deployment.ResourceGroupName
    Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'KeyVaultName' -Value $Deployment.Parameters['keyVaultName'].Value
    Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'KeyVaultResourceId' -Value $Deployment.Outputs['keyVaultId'].Value
}

return $Deployment
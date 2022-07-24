param(
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

'FunctionAppResourceId' | Assert-ConfigValueOrDefault -ConfigFilePath $ConfigFilePath

$Path = "$FunctionAppResourceId/host/default/listkeys?api-version=2021-02-01".Replace('//','/')
$Result = Invoke-AzRest -Path $Path -Method POST -ErrorAction 'Stop'
if($Result -and $Result.StatusCode -eq 200)
{
   $Content = $Result.Content | ConvertFrom-Json
   $KeyValue = $Content.functionKeys.default
   Set-ConfigValue -ConfigFilePath $ConfigFilePath -Name 'FunctionAccessKey' -Value $KeyValue
   return $KeyValue
}



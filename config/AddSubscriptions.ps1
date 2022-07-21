param(
    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $CallBackUri,

    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]
    $FunctionAccessKey,

    $Path = './subscriptions.json',

    $ConfigFilePath
)

. "$PSSCriptRoot/../deploy/helperFunctions.ps1"

if ([string]::IsNullOrEmpty($ConfigFilePath)) {
    $ConfigFilePath = "$PSScriptRoot/../deploy/functionapp.config.json"
}

'CallBackUri', 'FunctionAccessKey' | Assert-ConfigValueOrDefault -ConfigFilePath $ConfigFilePath

$body = Get-Content -Path $Path -Raw
Invoke-RestMethod -Method 'POST' -Uri $CallBackUri -Headers @{'x-functions-key' = $FunctionAccessKey} -ContentType 'application/json' -Body $body
function Assert-ConfigValueOrDefault {
    [cmdletbinding()]
    param(
        [string]
        $ConfigFilePath,

        [string[]]
        $JsonParentStructure,

        [Parameter(ValueFromPipeline)]
        [string]
        $Name
    )
    process {
        try {
            $CurrentVariable = Get-Variable -Name $Name -Scope 1 -ValueOnly -ErrorAction 'Stop'
            if($CurrentVariable.Length -gt 0) {
                Write-Verbose "Variable $Name is set by parameter with value [$CurrentVariable], do nothing" -Verbose
                # Variable is set by parameter, do nothing
                return
            }
        }
        catch {
            # Variable not found, ignoring.
        }
    
        if(Test-Path -Path "Env:/TWITCHLIVE_$Name") {
            $Value = Get-Content -Path "Env:/TWITCHLIVE_$Name" -ErrorAction 'Stop'
            Set-Variable -Name $Name -Scope 1 -Value $Value
            return
        }
    
        $ConfigFilePath = Resolve-Path -Path $ConfigFilePath -ErrorAction 'Stop'
    
        if(-not (Test-Path $ConfigFilePath)) {
            Write-Error -ErrorAction 'Stop' -Exception 'System.Management.Automation.ItemNotFoundException' -Message "Config file $ConfigFilePath was not found"
        }
        
        $Config = Get-Content -Path $ConfigFilePath -ErrorAction 'Stop' | ConvertFrom-Json -AsHashtable -ErrorAction 'Stop'
        # Navigate to parent node in json structure
        if($JsonParentStructure.Count -gt 0) {
            $JsonParentStructure | ForEach-Object {
                $Config = $Config[$_]
            }
        }
        
        if($null -eq $Config[$Name]) {
            Write-Error -ErrorAction 'Stop' -Exception 'System.Management.Automation.ItemNotFoundException' -Message "Property $Name was not set in file $ConfigFilePath"
        }
        
        $Value = $Config[$Name]
        if($CurrentVariable -is [securestring]) {
            $Value = $Value | ConvertTo-SecureString -AsPlainText -Force
        }

        Set-Variable -Name $Name -Scope 1 -Value $Value
        return
    }
}

function Set-ConfigValue {
    [cmdletbinding()]
    param(
        [string]
        $ConfigFilePath,
        $Name,
        $Value
    )

    if(Test-Path -Path "Env:/TWITCHLIVE_$Name") {
        Write-Verbose -Message "Config value $Name found in ENV var TWITCHLIVE_$Name. Configfile will not be updated." -Verbose         
        return
    }

    $ConfigFilePath = Resolve-Path -Path $ConfigFilePath -ErrorAction 'Stop'

    if(-not (Test-Path $ConfigFilePath)) {
        Write-Verbose -Message "Config file $ConfigFilePath was not found, config not updated." -Verbose
        return
    }
    
    $Config = Get-Content -Path $ConfigFilePath -ErrorAction 'Stop' | ConvertFrom-Json -AsHashtable -ErrorAction 'Stop'
    $Config[$Name] = $Value

    $null = $Config | ConvertTo-Json -ErrorAction 'Stop' | Out-File $ConfigFilePath -Force -ErrorAction 'Stop'
}

function Assert-ResourceGroup {
    [cmdletbinding()]
    param(
        $ResourceGroupName,
        $Location
    )
    try {
        $null = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction 'Stop'
        Write-Verbose -Message "Resource group $ResourceGroupName already exists" -Verbose
    
    }
    catch {
        Write-Verbose -Message "Creating resource group $ResourceGroupName" -Verbose
        $null = New-AzResourceGroup -Name $ResourceGroupName -Location $Location -ErrorAction 'Stop'
    }
}

function Import-ParametersFile {
    param(
        $Path,
        [hashtable]
        $ReplaceTokens
    )
    $ParametersContent = (Get-Content -Path $Path)
    foreach($Key in $ReplaceTokens.Keys) {
        $ParametersContent = $ParametersContent -replace $Key, $ReplaceTokens[$Key]
    }
    
    $ParametersTable = $ParametersContent | ConvertFrom-Json -AsHashtable
    $ParametersObject = @{}
    foreach($Key in $ParametersTable['parameters'].Keys) {
        $ParametersObject[$Key] = $ParametersTable['parameters'][$Key]['value']
    }
    return $ParametersObject
}

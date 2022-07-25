@description('Name that will be common for all resources created.')
param functionAppName string

@description('Storage account SKU')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
  'Premium_ZRS'
  'Standard_GZRS'
  'Standard_RAGZRS'
])
param storageAccountSku string = 'Standard_LRS'

@description('Key-Value pairs representing custom app settings')
param appSettings object = {}

@description('ResourceId of external keyvault used for secrets in appSettings')
param keyVaultResourceId string = ''

@description('PrincipalId of identity that will be granted access to deploy code')
param deployPrincipalId string = ''

@description('PrincipalType of identity that will be granted access to deploy code')
@allowed([
  'Device'
  'ForeignGroup'
  'Group'
  'ServicePrincipal'
  'User'
])
param deployPrincipalType string = 'User'

@description('List of queues to be created')
param queueList array = []

@description('List of tables to be created')
param tableList array = []

param location string = resourceGroup().location

var hostingPlanName = '${functionAppName}-plan'
var logAnalyticsName = '${functionAppName}-log'
var applicationInsightsName = '${functionAppName}-appin'
var functionAppNameNoDash = replace(functionAppName, '-', '')
var uniqueStringRg = uniqueString(resourceGroup().id)
var storageAccountName = toLower('${take(functionAppNameNoDash, 17)}${take(uniqueStringRg, 5)}sa')
var keyVaultName = '${take(functionAppNameNoDash, 17)}${take(uniqueStringRg, 5)}kv'
var packageName = 'TwitchLiveNotifications.zip'
var deployContainerName = 'deploy'
var packageUri = '${storageAccount.properties.primaryEndpoints.blob}${deployContainerName}/${packageName}'

var keyVaultSubscriptionId = keyVaultResourceId == '' ? '' : split(keyVaultResourceId,'/')[2]
var keyVaultResourceGroup = keyVaultResourceId == '' ? '' : split(keyVaultResourceId,'/')[4]
var keyVaultResourceName = keyVaultResourceId == '' ? '' : split(keyVaultResourceId,'/')[8]

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 90
  }
}

resource appInsights 'Microsoft.insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  tags: {
    'hidden-link:${resourceId('Microsoft.Web/sites', functionAppName)}': 'Resource'
  }
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageAccountSku
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
    allowSharedKeyAccess: false
    encryption: {
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
        queue: {
          keyType: 'Account'
          enabled: true
        }
        table: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
  }
}

resource storageAccountBlobService 'Microsoft.Storage/storageAccounts/blobServices@2021-09-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: false
    }
  }
}

resource storageAccountDeployContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  parent: storageAccountBlobService
  name: 'deploy'
  properties: {
    publicAccess: 'None'
  }
}

resource storageAccountFileService 'Microsoft.Storage/storageAccounts/fileServices@2021-09-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    shareDeleteRetentionPolicy: {
      enabled: false
    }
  }
}

resource storageAccountQueueService 'Microsoft.Storage/storageAccounts/queueServices@2021-09-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource storageAccountTableService 'Microsoft.Storage/storageAccounts/tableServices@2021-09-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource queues 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-09-01' = [for queueName in queueList: {
  parent: storageAccountQueueService
  name: queueName
}]

resource tables 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-09-01' = [for tableName in tableList: {
  parent: storageAccountTableService
  name: tableName
}]

@description('This is the built-in Storage Account Contributor role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-account-contributor ')
resource storageAccountContributor 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '17d1049b-9a84-46fb-8f53-869881c3d3ab'
}
@description('This is the built-in Storage Blob Data Owner role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-blob-data-owner ')
resource storageBlobDataOwner 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
}
@description('This is the built-in Storage Table Data Contributor role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-table-data-contributor ')
resource storageTableDataContributor 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}
@description('This is the built-in Storage Queue Data Contributor role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-queue-data-contributor ')
resource storageQueueDataContributor 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
}
resource roleAssignmentStorageContributor 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionApp.id, storageAccountContributor.id)
  properties: {
    roleDefinitionId: storageAccountContributor.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
resource roleAssignmentStorageBlob 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionApp.id, storageBlobDataOwner.id)
  properties: {
    roleDefinitionId: storageBlobDataOwner.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
resource roleAssignmentStorageTable 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionApp.id, storageTableDataContributor.id)
  properties: {
    roleDefinitionId: storageTableDataContributor.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
resource roleAssignmentStorageQueue 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionApp.id, storageQueueDataContributor.id)
  properties: {
    roleDefinitionId: storageQueueDataContributor.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
resource roleAssignmentDeploymentPrincipal 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = if(deployPrincipalId != null) {
  scope: storageAccount
  name: guid(storageAccount.id, deployPrincipalId, storageQueueDataContributor.id)
  properties: {
    roleDefinitionId: storageBlobDataOwner.id
    principalId: deployPrincipalId
    principalType: deployPrincipalType
  }
}


resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: 'Y1'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enabled: true
    httpsOnly: true
    serverFarmId: hostingPlan.id
    siteConfig: {
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      netFrameworkVersion: '6.0'
      scmType: 'None'
    }
    containerSize: 1536 // not used any more, but portal complains without it
  }
  tags: {
    'hidden-link: /app-insights-resource-id': appInsights.id
  }
}

resource funcitonAppDiagLogAnalytics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: functionApp
  name: 'logAnalyticsAudit'
  properties: {
    workspaceId: logAnalytics.id
    logs: [
      {
        category: 'FunctionAppLogs'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 90
        }
      }
    ]
  }
}

resource keyvault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: keyVaultName
  location: location
  properties: {
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableRbacAuthorization: true
    enableSoftDelete: false
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
  }
}

@description('This is the built-in Key Vault Secrets Officer role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#key-vault-secrets-officer ')
resource keyVaultSecretsOfficer 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
}
resource roleAssignmentKeyVaultSecrets 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: keyvault
  name: guid(keyvault.id, functionApp.id, keyVaultSecretsOfficer.id)
  properties: {
    roleDefinitionId: keyVaultSecretsOfficer.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}


resource keyvaultDiagStorage 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: keyvault
  name: 'service'
  properties: {
    storageAccountId: storageAccount.id
    logs: [
      {
        category: 'AuditEvent'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

resource keyvaultDiagLogAnalytics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: keyvault
  name: 'logAnalyticsAudit'
  properties: {
    workspaceId: logAnalytics.id
    logs: [
      {
        category: 'AuditEvent'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 90
        }
      }
    ]
  }
}

module externalKeyVaultAccess 'KeyVaultRBACAccess.bicep' = if(keyVaultResourceId != '') {
  name: 'externalKeyVaultRBACAccess'
  scope: resourceGroup(keyVaultSubscriptionId, keyVaultResourceGroup)
  params: {
    keyVaultName: keyVaultResourceName
    principalID: functionApp.identity.principalId
  }
}

resource functionAppSettings 'Microsoft.Web/sites/config@2020-06-01' = {
  parent: functionApp
  name: 'appsettings'
  properties: union(appSettings, {
    // AzureWebJobsStorage: '@Microsoft.KeyVault(SecretUri=${reference(storageConnectionStringName).secretUriWithVersion})'
    // WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(storageAccount.id, '2019-06-01').keys[0].value}'
    // WEBSITE_CONTENTSHARE: toLower(functionApp.name)
    APPLICATIONINSIGHTS_CONNECTION_STRING: reference(appInsights.id, '2020-02-02').ConnectionString
    AzureWebJobsDisableHomepage: true
    // AzureWebJobsSecretStorageKeyVaultConnectionString: ''
    // AzureWebJobsSecretStorageKeyVaultName: keyvault.name
    AzureWebJobsSecretStorageKeyVaultUri: keyvault.properties.vaultUri
    AzureWebJobsSecretStorageType: 'keyvault'
    AzureWebJobsStorage__accountName: storageAccount.name
    FUNCTIONS_APP_EDIT_MODE: 'readonly'
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_PROCESS_COUNT: 10
    FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
    StorageQueueConnection__credential: 'managedidentity'
    StorageQueueConnection__queueServiceUri: storageAccount.properties.primaryEndpoints.queue
    StorageTableConnection__credential: 'managedidentity'
    StorageTableConnection__tableServiceUri: storageAccount.properties.primaryEndpoints.table
    Twitch_CallbackUrl: 'https://${functionApp.properties.defaultHostName}/api/SubscriptionCallBack'
    WEBSITE_RUN_FROM_PACKAGE: packageUri
    WEBSITE_MOUNT_ENABLED: '1'
  })
}

output FunctionAppName string = functionApp.name
output FunctionAppId string = functionApp.id
output CallBackUrl string = 'https://${functionApp.properties.defaultHostName}/api/SubscriptionCallBack'
output PrincipalIdRef string = reference(functionApp.id, '2020-06-01', 'Full').identity.principalId
output PrincipalTenantId string = functionApp.identity.tenantId
output PrincipalId string = functionApp.identity.principalId
output StorageAccountName string = storageAccountName
output KeyVaultName string = keyVaultName
output PackageUri string = packageUri

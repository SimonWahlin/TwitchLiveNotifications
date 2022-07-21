param keyVaultName string
@description('PrincipalId of identity that will be granted access to manage secrets')
param secretsOfficerPrincipalId string = ''

@description('PrincipalType of identity that will be granted access to manage secrets')
@allowed([
  'Device'
  'ForeignGroup'
  'Group'
  'ServicePrincipal'
  'User'
])
param secretsOfficerPrincipalType string = 'User'

param location string = resourceGroup().location

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
  name: guid(keyvault.id, secretsOfficerPrincipalId, keyVaultSecretsOfficer.id)
  properties: {
    roleDefinitionId: keyVaultSecretsOfficer.id
    principalId: secretsOfficerPrincipalId
    principalType: secretsOfficerPrincipalType
  }
}

output KeyVaultName string = keyvault.name
output KeyVaultId string = keyvault.id

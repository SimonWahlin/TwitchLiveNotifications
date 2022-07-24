param keyVaultName string
param principalID string

resource keyvault 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyVaultName
}

@description('This is the built-in Key Vault Secrets Officer role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#key-vault-secrets-officer ')
resource keyVaultSecretsOfficer 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
}
resource roleAssignmentKeyVaultSecrets 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: keyvault
  name: guid(keyvault.id, principalID, keyVaultSecretsOfficer.id)
  properties: {
    roleDefinitionId: keyVaultSecretsOfficer.id
    principalId: principalID
    principalType: 'ServicePrincipal'
  }
}

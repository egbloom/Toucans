param environment string
param location string
param tags object

var keyVaultName = 'kv-todo-${environment}-${uniqueString(resourceGroup().id)}'

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: []
    enableRbacAuthorization: true
  }
}

output keyVaultName string = keyVault.name

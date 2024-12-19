param environment string
param location string = resourceGroup().location
param appName string

var tags = {
  environment: environment
  application: appName
}

module sqlServer 'modules/sql.bicep' = {
  name: 'sql-deployment'
  params: {
    environment: environment
    location: location
    tags: tags
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault-deployment'
  params: {
    environment: environment
    location: location
    tags: tags
  }
}

module functionApp 'modules/function.bicep' = {
  name: 'function-deployment'
  params: {
    environment: environment
    location: location
    tags: tags
    appInsightsInstrumentationKey: monitoring.outputs.instrumentationKey
    keyVaultName: keyVault.outputs.keyVaultName
    sqlServerName: sqlServer.outputs.serverName
    sqlDatabaseName: sqlServer.outputs.databaseName
  }
}

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring-deployment'
  params: {
    environment: environment
    location: location
    tags: tags
  }
}
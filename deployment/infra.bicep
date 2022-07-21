@description('The environment we are deploying to. For environments other than dev, the deployment locks down all resources to a vnet using Private Endpoints.')
param environment string

@description('The deployment key vault')
param kvDeployName string

@description('The deployment key vault resource group')
param kvDeployRG string

@description('The region into which we are deploying resources')
param location string = resourceGroup().location

var subscriptionId = subscription().subscriptionId
var lockdownEnvironments = ['staging', 'prod']

@description('The deployment key vault.')
resource kvDeploy 'Microsoft.KeyVault/vaults@2019-09-01' existing = {
  name: kvDeployName
  scope: resourceGroup(subscriptionId, kvDeployRG )
}

module kv './kv.bicep' = {
  name: 'keyVault'
  params: {
    administratorLogin: kvDeploy.getSecret('db-username-${environment}')
    administratorLoginPassword: kvDeploy.getSecret('db-password-${environment}')
    location: location
    appKvName: 'classmgrkv-${environment}-19ake'
    servicePrincipalKvAccess: kvDeploy.getSecret('service-principal-${environment}')
  }
}

module db './sql.bicep' = {
  name: 'sqlDeployment'
  params: {
    administratorLogin: kvDeploy.getSecret('db-username-${environment}')
    administratorLoginPassword: kvDeploy.getSecret('db-password-${environment}')
    location: location
    dbName: 'classmgrdb-19ake-${environment}'
  }
}

module acr './acr.bicep' = {
  name: 'containerRegistry'
  params: {
    acrName: 'classmgracr${environment}'
    location: location
  }
}

@description('Lock down the app for all non-dev environments using Private Endpoints.')
module vnet './vnet.bicep' = if (contains(lockdownEnvironments, environment)) {
  name: 'vnet1'
  params: {
    location: location
    sqlServer: db
    keyVault: kv
    acr: acr
  }
}

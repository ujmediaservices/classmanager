@description('The environment we are deploying to')
param environment string

@description('The deployment key vault')
param kvDeployName string

@description('The deployment key vault resource group')
param kvDeployRG string

@description('The region into which we are deploying resources')
param location string = resourceGroup().location

var subscriptionId = subscription().subscriptionId

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

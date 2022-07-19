@description('The database name')
param dbName string

@description('The region in which to create resources')
param location string

@description('The administrator username of the SQL logical server.')
@secure()
param administratorLogin string

@description('The administrator password of the SQL logical server.')
@secure()
param administratorLoginPassword string

resource sqlServer 'Microsoft.Sql/servers@2021-08-01-preview' = {
  name: dbName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
  }
}

resource sqlDB 'Microsoft.Sql/servers/databases@2021-08-01-preview' = {
  parent: sqlServer
  name: dbName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

output name string = dbName
output id string = sqlServer.id

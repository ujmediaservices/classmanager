@description('VNet name')
param vnetName string = 'VNet1'

@description('Address prefix')
param vnetAddressPrefix string = '10.0.0.0/16'

@description('Subnet 1 Prefix')
param subnet1Prefix string = '10.0.0.0/24'

@description('Subnet 2 Prefix')
param subnet2Prefix string = '10.0.1.0/24'

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Name of our SQL server.')
param sqlServer object

@description('Name of our applicaiton KeyVault.')
param keyVault object

var azureSqlPrivateDnsZone = 'privatelink${environment().suffixes.sqlServerHostname}'
var keyVaultPrivateDnsZone = 'privatelink${environment().suffixes.keyvaultDns}'
var privateDnsZoneNames = [
  azureSqlPrivateDnsZone
  keyVaultPrivateDnsZone
]

resource vnet 'Microsoft.Network/virtualNetworks@2021-08-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressPrefix
      ]
    }
    subnets: [
      {
        name: 'web'
        properties: {
          addressPrefix: subnet1Prefix
          delegations: [
            {
              name: 'delegation'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
        }
      }
      {
        name: 'private-endpoints'
        properties: {
          addressPrefix: subnet2Prefix
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

resource privateDnsZones 'Microsoft.Network/privateDnsZones@2018-09-01' = [for privateDnsZoneName in privateDnsZoneNames: {
  name: privateDnsZoneName
  location: 'global'
  dependsOn: [
    vnet
  ]
}]

resource virtualNetworkLinks 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = [for (privateDnsZoneName, i) in privateDnsZoneNames: {
  parent: privateDnsZones[i]
  location: 'global'
  name: 'link-to-${vnet.name}'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}]

resource sqlPrivateEndpoint 'Microsoft.Network/privateEndpoints@2020-06-01' = {
  name: '${sqlServer.name.value}-sql-pe'
  location: location
  properties: {
    subnet: {
      id: vnet.properties.subnets[1].id
    }
    privateLinkServiceConnections: [
      {
        name: '${sqlServer.name.value}-sql-pe'
        properties: {
          privateLinkServiceId: '${sqlServer.id.value}'
          groupIds: [
            'sqlServer'
          ]
        }
      }
    ]
  }
 
  resource privateDnsZoneGroup 'privateDnsZoneGroups@2020-03-01' = {
    name: 'dnsgroup'
    properties: {
      privateDnsZoneConfigs: [
        {
          name: 'config'
          properties: {
            privateDnsZoneId: resourceId('Microsoft.Network/privateDnsZones', azureSqlPrivateDnsZone)
          }
        }
      ]
    }
  }
}

resource keyVaultPrivateEndpoint 'Microsoft.Network/privateEndpoints@2020-06-01' = {
  name: '${keyVault.name.value}-kv-pe'
  location: location
  properties: {
    subnet: {
      id: vnet.properties.subnets[1].id
    }
    privateLinkServiceConnections: [
      {
        name: '${keyVault.name.value}-kv-pe-conn'
        properties: {
          privateLinkServiceId: '${keyVault.id.value}'
          groupIds: [
            'vault'
          ]
        }
      }
    ]
  }
 
  resource privateDnsZoneGroup 'privateDnsZoneGroups@2020-03-01' = {
    name: 'dnsgroup'
    properties: {
      privateDnsZoneConfigs: [
        {
          name: 'config'
          properties: {
            privateDnsZoneId: resourceId('Microsoft.Network/privateDnsZones', keyVaultPrivateDnsZone)
          }
        }
      ]
    }
  }
}

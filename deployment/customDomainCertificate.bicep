param appServicePlanName string
param webAppName string
param customDomain string
param location string

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' existing = {
  name: appServicePlanName
}

resource webApp 'Microsoft.Web/sites@2022-03-01' existing = {
  name: webAppName
}

resource certificate 'Microsoft.Web/certificates@2022-03-01' = {
  name: '${customDomain}-${webAppName}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    canonicalName: customDomain
  }
}

resource hostnameBinding 'Microsoft.Web/sites/hostNameBindings@2022-03-01' = {
  parent: webApp
  name: customDomain
  properties: {
    customHostNameDnsRecordType: 'CName'
    siteName: webAppName
    hostNameType: 'Verified'
    sslState: 'SniEnabled'
    thumbprint: certificate.properties.thumbprint
  }
}

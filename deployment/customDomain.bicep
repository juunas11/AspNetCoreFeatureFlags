param appServicePlanName string
param webAppName string
param customDomain string
param location string = resourceGroup().location

resource webApp 'Microsoft.Web/sites@2022-03-01' existing = {
  name: webAppName
}

resource hostnameBinding 'Microsoft.Web/sites/hostNameBindings@2022-03-01' = {
  parent: webApp
  name: customDomain
  properties: {
    customHostNameDnsRecordType: 'CName'
    siteName: webAppName
    hostNameType: 'Verified'
    sslState: 'Disabled'
  }
}

module certificate 'customDomainCertificate.bicep' = {
  name: '${deployment().name}-certificate'
  dependsOn: [
    hostnameBinding
  ]
  params: {
    appServicePlanName: appServicePlanName
    webAppName: webAppName
    customDomain: customDomain
    location: location
  }
}

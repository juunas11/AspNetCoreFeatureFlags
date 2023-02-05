param location string = resourceGroup().location
param developerGroupId string

var appName = 'featureflags'
var namingSuffix = uniqueString(resourceGroup().id)
var naming = {
  appConfiguration: 'cfg-${appName}-${namingSuffix}'
  appInsights: 'ai-${appName}-${namingSuffix}'
  appServicePlan: 'app-${appName}-${namingSuffix}'
  logAnalytics: 'log-${appName}-${namingSuffix}'
  webApp: 'web-${appName}-${namingSuffix}'
}

var appConfigOwnerRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5ae67dd6-50cb-40e7-96ff-dc2bfa4b606b')
var appConfigReaderRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '516239f1-63e1-4d78-a4de-a74fb236a071')

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: naming.logAnalytics
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    workspaceCapping: {
      dailyQuotaGb: -1
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: naming.appInsights
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    RetentionInDays: 30
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: naming.appServicePlan
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: naming.webApp
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'AppConfig__Uri'
          value: 'https://${appConfiguration.name}.azconfig.io'
        }
      ]
      ftpsState: 'Disabled'
    }
  }
}

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: naming.appConfiguration
  location: location
  sku: {
    name: 'free'
  }
  properties: {
    disableLocalAuth: true
  }
}

resource developerConfigOwnerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: appConfiguration
  name: guid(appConfiguration.id, appConfigOwnerRoleId, developerGroupId)
  properties: {
    principalId: developerGroupId
    roleDefinitionId: appConfigOwnerRoleId
    principalType: 'Group'
  }
}

resource webAppConfigReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: appConfiguration
  name: guid(appConfiguration.id, appConfigReaderRoleId, webApp.id)
  properties: {
    principalId: webApp.identity.principalId
    roleDefinitionId: appConfigReaderRoleId
    principalType: 'ServicePrincipal'
  }
}

output webAppName string = webApp.name
output webAppDefaultHostName string = webApp.properties.defaultHostName
output webAppCustomDomainVerificationId string = webApp.properties.customDomainVerificationId

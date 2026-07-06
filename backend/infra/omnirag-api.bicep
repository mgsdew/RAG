param location string = resourceGroup().location
param appName string
param appServicePlanName string = '${appName}-plan'
param keyVaultUri string
param storageContainerName string = 'documents'
param openAiEndpoint string
param openAiChatDeploymentName string = 'YOUR-CHAT-DEPLOYMENT-NAME'
param openAiEmbeddingDeploymentName string = 'YOUR-EMBEDDING-DEPLOYMENT-NAME'
param tags object = {}
param skuName string = 'B1'
param skuCapacity int = 1

var linuxFxVersion = 'DOTNETCORE|8.0'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: 'Basic'
    capacity: skuCapacity
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  tags: tags
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      alwaysOn: true
    }
  }
}

resource webAppSettings 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: webApp
  name: 'appsettings'
  properties: {
    ASPNETCORE_ENVIRONMENT: 'Production'
    WEBSITE_RUN_FROM_PACKAGE: '1'
    WEBSITES_ENABLE_APP_SERVICE_STORAGE: 'false'
    Azure__KeyVaultUri: keyVaultUri
    Azure__Storage__ContainerName: storageContainerName
    Azure__OpenAI__Endpoint: openAiEndpoint
    Azure__OpenAI__ChatDeploymentName: openAiChatDeploymentName
    Azure__OpenAI__EmbeddingDeploymentName: openAiEmbeddingDeploymentName
  }
}

output appServiceName string = webApp.name
output defaultHostName string = webApp.properties.defaultHostName
output appServiceUrl string = 'https://${webApp.properties.defaultHostName}'

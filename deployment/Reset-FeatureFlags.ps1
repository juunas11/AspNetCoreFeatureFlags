$ErrorActionPreference = 'Stop'

$config = Get-Content .\config.json | ConvertFrom-Json
$tenantId = $config.tenantId
$subscriptionId = $config.subscriptionId
$resourceGroupName = $config.resourceGroupName

az account show -s "$subscriptionId" | Out-Null
if ($LASTEXITCODE -ne 0) {
    az login -t "$tenantId"
}


$appConfigName = az appconfig list -g $resourceGroupName --query "[].name" -o tsv
if ($LASTEXITCODE -ne 0) {
    Exit
}

Write-Host "Deleting existing features..."
$existingFeaturesOutput = az appconfig feature list -n $appConfigName --auth-mode login --query "[].name" -o tsv --subscription $subscriptionId
if ($LASTEXITCODE -ne 0) {
    Exit
}

$existingFeatures = $existingFeaturesOutput -split "\n"

foreach ($existingFeature in $existingFeatures) {
    az appconfig feature delete --feature $existingFeature -n $appConfigName --auth-mode login --yes --subscription $subscriptionId | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Exit
    }
}

Write-Host "Adding features..."
$features = @('NewsSummary', 'DarkTheme', 'UserGreeting')

foreach ($feature in $features) {
    az appconfig feature set --feature $feature -n $appConfigName --auth-mode login --yes --subscription $subscriptionId | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Exit
    }
}

Write-Host "Adding feature filters..."
az appconfig feature filter add --feature "DarkTheme" --filter-name "AppVersion" --filter-parameters 'Versions={\"Stable\":\"OptIn\",\"Beta\":\"Enabled\"}' -n $appConfigName --auth-mode login --yes --subscription $subscriptionId | Out-Null
if ($LASTEXITCODE -ne 0) {
    Exit
}

az appconfig feature filter add --feature "UserGreeting" --filter-name "UserPercentage" --filter-parameters 'Percentage=30' -n $appConfigName --auth-mode login --yes --subscription $subscriptionId | Out-Null
if ($LASTEXITCODE -ne 0) {
    Exit
}

Write-Host "Enabling features..."
foreach ($feature in $features) {
    az appconfig feature enable --feature $feature -n $appConfigName --auth-mode login --yes --subscription $subscriptionId | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Exit
    }
}

Write-Host "Feature reset done"
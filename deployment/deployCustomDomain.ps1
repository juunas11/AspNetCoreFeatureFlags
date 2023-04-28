$ErrorActionPreference = 'Stop'

$config = Get-Content .\config.json | ConvertFrom-Json
$tenantId = $config.tenantId
$subscriptionId = $config.subscriptionId
$resourceGroupName = $config.resourceGroupName
$webAppCustomDomain = $config.webAppCustomDomain

if ($null -eq $webAppCustomDomain -or $webAppCustomDomain -eq "") {
    Write-Host "No custom domain specified in config.json. Skipping custom domain deployment."
    Exit
}

az account show -s "$subscriptionId" | Out-Null
if ($LASTEXITCODE -ne 0) {
    az login -t "$tenantId"
}

$webAppName = az webapp list -g "$resourceGroupName" --subscription "$subscriptionId" --query "[].{name:name}" -o tsv
if ($LASTEXITCODE -ne 0) {
    Exit
}

$appServicePlanId = az webapp show -n "$webAppName" -g "$resourceGroupName" --subscription "$subscriptionId" --query "{appServicePlanId:appServicePlanId}" -o tsv
if ($LASTEXITCODE -ne 0) {
    Exit
}

$appServicePlanName = az resource show --ids $appServicePlanId --query "{name:name}" -o tsv
if ($LASTEXITCODE -ne 0) {
    Exit
}

$deploymentName = Get-Date -Format "yyyy-MM-dd-HH-mm-ss"

Write-Host "Starting deployment..."

Write-Host "Running customDomain.bicep deployment..."
az deployment group create --subscription "$subscriptionId" -g "$resourceGroupName" -f "customDomain.bicep" -n "$deploymentName-CustomDomain" --mode "Incremental"`
    -p "@customDomain.parameters.json" `
    -p appServicePlanName=$appServicePlanName `
    -p webAppName=$webAppName `
    -p customDomain=$webAppCustomDomain
if ($LASTEXITCODE -ne 0) {
    Exit
}

Write-Host "Custom domain deployment complete"
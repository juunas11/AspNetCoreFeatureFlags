$ErrorActionPreference = 'Stop'

$config = Get-Content .\config.json | ConvertFrom-Json
$tenantId = $config.tenantId
$subscriptionId = $config.subscriptionId
$resourceGroupName = $config.resourceGroupName
$location = $config.location
$developerGroupId = $config.developerGroupId
$webAppCustomDomain = $config.webAppCustomDomain

az account show -s "$subscriptionId" | Out-Null
if ($LASTEXITCODE -ne 0) {
    az login -t "$tenantId"
}

$deploymentName = Get-Date -Format "yyyy-MM-dd-HH-mm-ss"

Write-Host "Starting deployment..."

$rgExists = az group exists --subscription "$subscriptionId" -g "$resourceGroupName"
if ($LASTEXITCODE -ne 0) {
    Exit
}

if ($rgExists -eq "false") {
    Write-Host "Resource group does not exist. Creating resource group..."
    az group create --subscription "$subscriptionId" -g "$resourceGroupName" -l "$location"
    if ($LASTEXITCODE -ne 0) {
        Exit
    }
}

Write-Host "Running main.bicep deployment..."
$mainBicepResult = az deployment group create --subscription "$subscriptionId" -g "$resourceGroupName" -f "main.bicep" -n "$deploymentName-Main" --mode "Incremental"`
    -p "@main.parameters.json" `
    -p developerGroupId=$developerGroupId | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) {
    Exit
}

$mainBicepOutputs = $mainBicepResult.properties.outputs

Push-Location ..\src\FeatureFlagsDemo

Write-Host "Creating Web App deployment package..."
dotnet publish --configuration Release --output ..\publish_output
if ($LASTEXITCODE -ne 0) {
    Exit
}

Compress-Archive -Path ..\publish_output\* -DestinationPath ..\WebPublish.zip -CompressionLevel Fastest -Force

$webAppName = $mainBicepOutputs.webAppName.value

Write-Host "Deploying Web App..."
az webapp deployment source config-zip --subscription "$subscriptionId" -g "$resourceGroupName" -n "$webAppName" --src ..\WebPublish.zip | Out-Null
if ($LASTEXITCODE -ne 0) {
    Exit
}

Pop-Location

Write-Host "Deployment complete."

if ($webAppCustomDomain -ne $null) {
    $webAppDefaultHostName = $mainBicepOutputs.webAppDefaultHostName.value
    $webAppCustomDomainVerificationId = $mainBicepOutputs.webAppCustomDomainVerificationId.value

    Write-Host "To setup the custom domain, you will need these two DNS records setup:"
    Write-Host "CNAME $webAppCustomDomain -> $webAppDefaultHostName"
    Write-Host "TXT asuid.$webAppCustomDomain -> $webAppCustomDomainVerificationId"
    Write-Host "Once you have set these up, run deployCustomDomain.ps1"
}
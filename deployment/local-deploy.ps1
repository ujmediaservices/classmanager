# Deployment file intended to test out changes to the Bicep deployment. 
# Assumptions: 
# - Az and CredentialManager installed locally (run local-requirements.ps1)
# - SP client ID/secret stored in Credential Manager

param(
    [string]$Environment="dev",
    [string]$TenantId = "96dcc2da-1dd8-43c6-b176-07d3d3c71c89",
    [string]$Location = "West US"
)

$ErrorActionPreference = "Stop"

$psCred = Get-StoredCredential -Target "devsp"

$loginResult = Connect-AzAccount -ServicePrincipal -Credential $psCred -Tenant $TenantId

# Create resource group if it doesn't exist. 
$resourceGroup = "classmanager-$Environment"
$rgExists = $false
try {
    $rgResult = Get-AzResourceGroup -Name $resourceGroup
    $rgExists = $true
} catch {
    # Didn't exist - create it.
    write-output("Couldn't find resource group $resourceGroup - creating it now...")
}

if (-Not $rgExists) {
    New-AzResourceGroup -Name $resourceGroup -Location $Location
}

$suffix = Get-Random -Maximum 10000
$deployResult = New-AzResourceGroupDeployment -Name "ClassManager-$Environment-$suffix" -ResourceGroupName $resourceGroup -TemplateFile "$PSScriptRoot\infra.bicep" -TemplateParameterFile "$PSScriptRoot\deploy-$Environment.json" -DeploymentDebugLogLevel All
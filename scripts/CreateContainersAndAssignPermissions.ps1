param(
    
    [Parameter(Mandatory = $true)]
    [string]$ResourcePrefix
)

$ResourceGroupName = "$($ResourcePrefix)rg"

az storage container create --account-name "$($ResourcePrefix)st01" --name "$($ResourcePrefix)func01-01" --auth-mode login
az storage container create --account-name "$($ResourcePrefix)st01" --name "$($ResourcePrefix)func02-01" --auth-mode login

$StorageAccount01 = (az storage account show --name "$($ResourcePrefix)st01" --resource-group $ResourceGroupName) | ConvertFrom-Json

az storage container create --account-name "$($ResourcePrefix)st02" --name "$($ResourcePrefix)func01-01" --auth-mode login
az storage container create --account-name "$($ResourcePrefix)st02" --name "$($ResourcePrefix)func02-01" --auth-mode login

$StorageAccount02 = (az storage account show --name "$($ResourcePrefix)st02" --resource-group $ResourceGroupName) | ConvertFrom-Json

# Add access for app registration

$AppReg = (az ad app list --display-name "$($ResourcePrefix)appreg") | ConvertFrom-Json

az role assignment create --role "Storage Blob Data Contributor" --assignee $AppReg.appId --scope "$($StorageAccount01.id)/blobServices/default/containers/$($ResourcePrefix)func01-01"
az role assignment create --role "Storage Blob Data Contributor" --assignee $AppReg.appId --scope "$($StorageAccount02.id)/blobServices/default/containers/$($ResourcePrefix)func01-01"

az role assignment create --role "Storage Blob Data Contributor" --assignee $AppReg.appId --scope "$($StorageAccount01.id)/blobServices/default/containers/$($ResourcePrefix)func02-01"
az role assignment create --role "Storage Blob Data Contributor" --assignee $AppReg.appId --scope "$($StorageAccount02.id)/blobServices/default/containers/$($ResourcePrefix)func02-01"

# Add access for function app 01

$ResourceName = "$($ResourcePrefix)func01"

$Resource = (az functionapp identity show --name $ResourceName --resource-group $ResourceGroupName) | ConvertFrom-Json

az role assignment create --role "Storage Blob Data Contributor" --assignee $Resource.principalId --scope "$($StorageAccount01.id)/blobServices/default/containers/$($ResourcePrefix)func01-01"
az role assignment create --role "Storage Blob Data Contributor" --assignee $Resource.principalId --scope "$($StorageAccount02.id)/blobServices/default/containers/$($ResourcePrefix)func01-01"

# Add access for function app 02

$ResourceName = "$($ResourcePrefix)func02"

$Resource = (az functionapp identity show --name $ResourceName --resource-group $ResourceGroupName) | ConvertFrom-Json

az role assignment create --role "Storage Blob Data Contributor" --assignee $Resource.principalId --scope "$($StorageAccount01.id)/blobServices/default/containers/$($ResourcePrefix)func02-01"
az role assignment create --role "Storage Blob Data Contributor" --assignee $Resource.principalId --scope "$($StorageAccount02.id)/blobServices/default/containers/$($ResourcePrefix)func02-01"


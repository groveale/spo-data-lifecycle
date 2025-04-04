
$clientId = "bc5a937b-42b5-49e1-83f4-dfe0b8b67497"
$thumbprint = "0CC78F9E0F578F9F7B2483E9DDE97BA6FDC0168E"
$tenant = "groverale"
$siteUrl = "https://groverale.sharepoint.com/sites/labelstest"
# Connect to the site
Connect-PnPOnline -Url $siteUrl -ClientId $clientId -Tenant "$tenant.onmicrosoft.com" -Thumbprint $thumbprint

# Parameters
$docLib = "changedates"
$newModifiedDate = (Get-Date).AddYears(-3).AddDays(10)

# Format the new modified date as an ISO 8601 string
$newModifiedDateFormatted = $newModifiedDate.ToString("yyyy-MM-dd hh:mm:ss")

# Get all files in library
$files = Get-PnPListItem -List $docLib

# iterate through each file
foreach ($file in $files) {
    # Update the modified date
    Set-PnPListItem -List $docLib -Identity $file.Id -Values @{"Modified" = $newModifiedDateFormatted} -UpdateType SystemUpdate
}


# Disconnect from SharePoint Online
Disconnect-PnPOnline
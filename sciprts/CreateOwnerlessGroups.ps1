# Import the necessary module
Import-Module ExchangeOnlineManagement

# Connect to Exchange Online
Connect-ExchangeOnline -UserPrincipalName "admin@MngEnvMCAP397917.onmicrosoft.com" -ShowProgress $true

# Define the number of groups to create
$numberOfGroups = 5

# Define the members to add to each group
$members = @("alex@MngEnvMCAP397917.onmicrosoft.com", "pete@MngEnvMCAP397917.onmicrosoft.com")

# Define the SharePoint Online base URL
$spoBaseUrl = "https://MngEnvMCAP397917.sharepoint.com/sites/"

# Iterate through the number of groups and create each group
for ($i = 1; $i -le $numberOfGroups; $i++) {
    # Generate a GUID and take the first 8 characters
    $guid = [guid]::NewGuid().ToString().Substring(0, 8)

    # Define the group properties
    $displayName = "Group$guid"
    $alias = "group$guid"

    # Create the group without owners
    $newGroup = New-UnifiedGroup -DisplayName $displayName -Alias $alias

    # Add members to the group
    foreach ($member in $members) {
        Add-UnifiedGroupLinks -Identity $newGroup.Alias -LinkType Member -Links $member
    }

     # Construct the SharePoint Online URL
     $spoUrl = "$spoBaseUrl$alias"
     Write-Output "Created Group: $displayName"
     Write-Output " SharePoint Online URL: $spoUrl"
}

# Disconnect from Exchange Online
#Disconnect-ExchangeOnline -Confirm:$false
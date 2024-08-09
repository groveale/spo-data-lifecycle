

$siteUrl = "https://groverale.sharepoint.com/sites/DocLCTest"


# Connect to the site
Connect-PnPOnline -Url $siteUrl -Interactive

# Get the creation events for the last 2 hours

# $startTime = [datetime]::ParseExact("2024-08-05T15:00:00Z", "yyyy-MM-ddTHH:mm:ssZ", $null)
# $endTime = [datetime]::ParseExact("2024-08-05T18:00:00Z", "yyyy-MM-ddTHH:mm:ssZ", $null)


$items = Get-PnPUnifiedAuditLog -ContentType SharePoint -StartTime (Get-Date -asUtc).AddHours(-1) -EndTime (Get-Date -asUtc)

#$items = Get-PnPUnifiedAuditLog -ContentType SharePoint -StartTime $startTime -EndTime $endTime

$creationEvents = $items | Where { $_.Operation -eq 'ListCreated' }

## https://learn.microsoft.com/en-us/purview/audit-log-activities?view=o365-worldwide#sharepoint-list-activities

$creationEvents

## Can we check library is not a list using this data
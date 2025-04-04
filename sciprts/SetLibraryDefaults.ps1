
# Description: This script sets the default retention label for a library


## Need to get all libraries in the site (excucling system libaries)
## For each library, set the default retention label

# Parameters
$retentionLabel = "Non record"
$siteUrl = "https://groverale.sharepoint.com/sites/labelstest"

# Auth (cert needs to be local on the device)
# Permission - SharePoint.Manage.All
$clientId = "bc5a937b-42b5-49e1-83f4-dfe0b8b67497"
$thumbprint = "0CC78F9E0F578F9F7B2483E9DDE97BA6FDC0168E"
$tenant = "groverale"

# $clientId = "e0206210-3296-4421-8a10-e422a03755b5"
# $thumbprint = "0CC78F9E0F578F9F7B2483E9DDE97BA6FDC0168E"
# $tenant = "M365CPI77517573"

# JSON View Formatting
$jsonViewFormatting = @'
{
    "$schema": "https://developer.microsoft.com/json-schemas/sp/v2/row-formatting.schema.json",
    "additionalRowClass": "=if([$_ComplianceTag] == 'User retained non record 1 year' &amp;&amp; (Number(@now) - Number(Date([$_ComplianceTagWrittenTime]))) / (1000 * 60 * 60 * 24) &gt; 335, 'sp-field-severity--severeWarning', if([$_ComplianceTag] == 'Non record' &amp;&amp; (Number(@now) - Number([$Modified])) / (1000 * 60 * 60 * 24) &gt; 1065, 'sp-field-severity--severeWarning', if([$_ComplianceTag] == 'User retained non record 3 years' &amp;&amp; (Number(@now) - Number(Date([$_ComplianceTagWrittenTime]))) / (1000 * 60 * 60 * 24) &gt; 1065, 'sp-field-severity--severeWarning', if([$_ComplianceTag] == 'User retained non record 5 years' &amp;&amp; (Number(@now) - Number(Date([$_ComplianceTagWrittenTime]))) / (1000 * 60 * 60 * 24) &gt; 1795, 'sp-field-severity--severeWarning', ''))))"
}
'@

# Connect to the site
Connect-PnPOnline -Url $siteUrl -ClientId $clientId -Tenant "$tenant.onmicrosoft.com" -Thumbprint $thumbprint

## Get doc libs (that aren't system lists and isn't the pages library)
$docLibs = Get-PnPList -Includes IsSystemList, Fields | Where { ($_.BaseType -eq "DocumentLibrary" ) -and !($_.IsSystemList) -and ($_.Title -ne "Site Pages")}
Write-Host " Found $($docLibs.Length) document libraries" -ForegroundColor DarkGreen

# Iterate all libaries
foreach ($lib in $docLibs) 
{
    Write-Host "Setting default retention label for $($lib.Title)" -ForegroundColor DarkGreen
    # Add Rentention Label to default view
    $libraryName = $lib.Title
    $defaultView = Get-PnPView -List $libraryName | where { $_.DefaultView -eq $true }

    # Get the fields for the view
    $viewFields = $defaultView.ViewFields
    # Add the compliance tag fields to the view
    # Check if the fields are already in the view
    foreach ($field in "_ComplianceTag", "_ComplianceTagWrittenTime") {
        if ($viewFields -notcontains $field) {
            $viewFields.Add($field)
        }
    }

    $updatedView = Set-PnPView -List $libraryName -Identity $defaultView.Title -Fields @($viewFields)

    # Does the default view have a custom formatter?
    if ($defaultView.CustomFormatter -eq $null) {
    
        $updatedView = Set-PnPView -List $libraryName -Identity $defaultView.Title -Values @{CustomFormatter = $jsonViewFormatting}
    }

    # Set the default retention label for the library
    Set-PnPRetentionLabel -List $libraryName -Label $retentionLabel -SyncToItems $true

    # For some reasone we need to execute twice - Probably a PnP Bug but with a delay
    #Set-PnPRetentionLabel -List $libraryName -Label $retentionLabel -SyncToItems $true 
}





# Description: This script sets the default retention label for a library


## Need to get all libraries in the site (excucling system libaries)
## For each library, set the default retention label

# Parameters
$retentionLabel = "ArchiveItem"
$siteUrl = "https://groverale.sharepoint.com/sites/RententionLAbelTest"

# Connect to the site
Connect-PnPOnline -Url $siteUrl -Interactive

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

    # Set the default retention label for the library
    Set-PnPRetentionLabel -List $libraryName -Label $retentionLabel -SyncToItems $true

    # For some reasone we need to execute twice - Probably a PnP Bug but with a delay
    #Set-PnPRetentionLabel -List $libraryName -Label $retentionLabel -SyncToItems $true 
}




# Import the necessary module
Import-Module ExchangeOnlineManagement

$user = "alex@groverale.onmicrosoft.com"

# Connect to Exchange Online (will open up a browser window for authentication)
Connect-ExchangeOnline -ShowProgress $true -UserPrincipalName $user 

Connect-IPPSSession -UserPrincipalName $user 

$existingLabels = Get-ComplianceTag

# Read the JSON file
$labelsConfig = Get-Content -Path "labelConfig.json" -Raw | ConvertFrom-Json

$policyName = "IM Site Wide Policy"

# Get Rentetion policies
$policies = Get-RetentionCompliancePolicy
$policyObj = $policies | Where-Object { $_.Name -eq $policyName }

# Check if the policy already exists
if ($null -ne $policyObj) {
    Write-Host "Policy already exists" -ForegroundColor DarkYellow
}
else {
    Write-Host "Creating policy" -ForegroundColor DarkGreen
    $policyObj = New-RetentionCompliancePolicy -Name $policyName -SharePointLocation "All"
}

## Now create rule for each label to publish with the policy
$rules = Get-RetentionComplianceRule -Policy $policyName

# Iterate through each label configuration and create the retention label
foreach ($label in $labelsConfig) {
    $retentionAction = switch ($label.RetentionAction) {
        "None" { "None" }
        "Delete" { "Delete" }
        "Retain" { "Keep" }
        "RetainAndDelete" { "KeepAndDelete" }
        default { "None" }
    }

    $retentionTrigger = switch ($label.RetentionTrigger) {
        "LabelApplied" { "TaggedAgeInDays" }
        "Modification" { "ModificationAgeInDays" }
        "Creation" { "CreationAgeInDays" }
        default { "TaggedAgeInDays" }
    }



    $labelObj = $existingLabels | Where-Object { $_.Name -eq $label.Name }

    # Check if the label already exists
    if ($null -ne $labelObj) {
        Write-Host "Label $($label.Name) already exists" -ForegroundColor DarkYellow
    } else {
        Write-Host "Creating label $($label.Name)" -ForegroundColor DarkGreen
        if ($label.retentionAction -eq "None") {
            $labelObj = New-ComplianceTag -Name $label.Name
        } else {
            $labelObj = New-ComplianceTag -Name $label.Name -RetentionDuration $($label.RetentionDuration) -RetentionAction $retentionAction -RetentionType $retentionTrigger
        }
    }

    $ruleObj = $rules | Where-Object { $_.ComplianceTagProperty.StartsWith($labelObj.Guid) }

    if ($ruleObj -ne $null) {
        Write-Host "Rule $($label.Name)-Rule already exists" -ForegroundColor DarkYellow
        continue
    }

    Write-Host "Creating rule for $($label.Name)" -ForegroundColor DarkGreen
    $ruleObj = New-RetentionComplianceRule -Policy $policyObj.Id -PublishComplianceTag $labelObj.Guid
}

###### Notes
# If you need to unpublish a label, you can use the following commands:
# $rules = Get-RetentionComplianceRule -Policy "IM Site Wide Policy"
# $ruleObj = $rules | Where-Object { $_.ComplianceTagProperty.StartsWith($labelGuid) }
# Remove-RetentionComplianceRule -Identity $ruleObj.Name

# Disconnect from Exchange Online
Disconnect-ExchangeOnline -Confirm:$false
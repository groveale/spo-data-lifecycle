$certname = "set-library-defaults"    ## Replace {certificateName}
$cert = New-SelfSignedCertificate -Subject "CN=$certname" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256
Export-Certificate -Cert $cert -FilePath "$certname.cer"   ## Specify your preferr

# Write-Host "Enter password for private cert - This will be needed if you are planning to run the script from an Azure App service"
# $pass = Read-Host -AsSecureString
# # Export cert to PFX - uploaded to Azure App Service
# Export-PfxCertificate -cert $cert -FilePath "$certname.pfx" -Password $pass

## Note the thumbprint
$cert.Thumbprint

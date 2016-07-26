#Requires -Version 3.0

Param(
    [Parameter(Mandatory=$true)]
    [string] $ServicePrincipalPassword,
    [string] $Global:LucyStagingStorageUSKey

)

Import-Module Azure -ErrorAction SilentlyContinue

try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("VSAzureTools-$UI$($host.name)".replace(" ","_"), "2.9")
} catch { }

Set-StrictMode -Version 3

$secpasswd = ConvertTo-SecureString $ServicePrincipalPassword -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("3a99e1f6-7e90-426e-b399-3536f078f9ca", $secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 366f6586-9b69-47bd-bfb3-59845478627f -Credential $mycreds

$LucyStagingStorageUSKey = (Get-AzureStorageKey  -StorageAccountName "lucystagingstorageus").Primary


Import-Module "$PSScriptRoot\IntegrationTest\Tools\Pester-master\pester.psd1" -ErrorAction SilentlyContinue

#Invoke-Pester will execute all *.tests.ps1 scripts in current directory
Invoke-Pester  -OutputFile "./CheckResult.xml" -OutputFormat NUnitXml  

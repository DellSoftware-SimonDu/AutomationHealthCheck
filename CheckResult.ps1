
##############Begin to Login Azure##################
<#
$Password = "Quest123"
$Username = "086de0c4-792c-462f-b333-818c6c5f8078"
$SecurePassword = ConvertTo-SecureString -string $Password -AsPlainText –Force
$Cred = new-object System.Management.Automation.PSCredential ($Username, $SecurePassword)
$TenantId = "91c369b5-1c9e-439c-989c-1867ec606603"
Login-AzureRmAccount -Credential $cred -ServicePrincipal -TenantId $TenantId
#>

##############Begin to Download##################
function CopyJsonFile {
#Define the variables.
Write-Host "Coping Result JSON file"
Remove-Item c:\temp\e475bcae-fd1b-4fd7-9f04-015095e81e53  -Recurse -ErrorAction Ignore
$ContainerName = "sqlserver-static-health-check"
$DestinationFolder = "c:\temp"

#Define the storage account and context.
$StorageAccountName = "lucystagingstorageus"
$StorageAccountKey = "s1bQ/9f2S5JxPMBw7OhKMSGAoqVBAhAtu4yyDnugD/kbIo/OXZRh8E8IYRkya2m7dxc9KFF4+9XkYYbdKfHznw=="
$Ctx = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey
$BlobName = "e475bcae-fd1b-4fd7-9f04-015095e81e53/2/79cdafd4-1325-4fac-8c60-6279a711c587/zhuw12r2spl502.melquest.dev.mel.au.qsft_sql2016_sqlserver/LATEST.json"
#List all blobs in a container.
$blobs = Get-AzureStorageBlob -Container $ContainerName -Context $Ctx| ? {$_.Name -like $BlobName} 

#Download blobs from a container.
New-Item -Path $DestinationFolder -ItemType Directory -Force
$blobs | Get-AzureStorageBlobContent -Destination $DestinationFolder -Context $Ctx

}
 
function CheckResult
{

$Pattern = "e475bcae-fd1b-4fd7-9f04-015095e81e53/2/79cdafd4-1325-4fac-8c60-6279a711c587/zhuw12r2spl502.melquest.dev.mel.au.qsft_sql2016_sqlserver/LATEST.json" 
$JSONFile = "c:\temp\" + $Pattern
$Params = (Get-Content $JSONFile) -join "`n" | ConvertFrom-Json


$ParamHashTable = @{}

$Params0 = $Params.TotalScoreIncludingIgnoredChecks
$Params1 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "memory_physical_memory_pressure"}).HealthCheckScore
$Params2 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "security_password_policy"}).HealthCheckScore 
$Params3 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "configuration_network_packet_size"}).HealthCheckScore
$Params4 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "configuration_defaults"}).HealthCheckScore
$Params5 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "dr_backup"}).HealthCheckScore
$Params6 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "dr_simple_recovery_model"}).HealthCheckScore
$CheckResult0 = $ParamHashTable.TotalScoreIncludingIgnoredChecks = ($Params.TotalScoreIncludingIgnoredChecks -eq [decimal]"84.0023269342641")
$CheckResult1 = $ParamHashTable.memory_physical_memory_pressure = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "memory_physical_memory_pressure"}).HealthCheckScore -eq [int]"100.0"
$CheckResult2 = $ParamHashTable.security_password_policy = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "security_password_policy"}).HealthCheckScore -eq $null
$CheckResult3 = $ParamHashTable.configuration_network_packet_size = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "configuration_network_packet_size"}).HealthCheckScore -eq $null
$CheckResult4 = $ParamHashTable.configuration_defaults = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "configuration_defaults"}).HealthCheckScore -eq $null
$CheckResult5 = $ParamHashTable.dr_backup = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "dr_backup"}).HealthCheckScore -eq [int]"0"
$CheckResult6 = $ParamHashTable.dr_simple_recovery_model = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "dr_simple_recovery_model"}).HealthCheckScore -eq [int]"100.0"

$strings = @(("TotalScoreIncludingIgnoredChecks","memory_physical_memory_pressure","security_password_policy","configuration_network_packet_size","configuration_defaults","dr_backup","dr_simple_recovery_model",`
$Params0,$Params1,$Params2,$Params3,$Params4,$Params5),`
("84.0023269342641","100.0",$null,"$null","$null","0","100.0",$CheckResult0,$CheckResult1,$CheckResult2,$CheckResult3,$CheckResult4,$CheckResult5))

$ResultOutput = for($i=0;$i -le 5;$i++) {StringVersions -inputString $strings[0][$i] -ExpectedValueString $strings[1][$i] -ActualValue $strings[0][$i+7] -Result $strings[1][$i+7] }
#$ResultOutput

if ($ParamHashTable.Values -ccontains $false)
{
    return "False"}
else {return "All True"}
#Remove-Item -Path c:\temp\e475bcae-fd1b-4fd7-9f04-015095e81e53 -Force -Recurse
}

function CheckIndividualResult([decimal]$Result0,$Result1)
{
 
$Pattern = "e475bcae-fd1b-4fd7-9f04-015095e81e53/2/79cdafd4-1325-4fac-8c60-6279a711c587/zhuw12r2spl502.melquest.dev.mel.au.qsft_sql2016_sqlserver/LATEST.json" 
$JSONFile = "c:\temp\" + $Pattern
$Params = (Get-Content $JSONFile) -join "`n" | ConvertFrom-Json

$Params0 = $Params.TotalScoreIncludingIgnoredChecks
$Params1 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "memory_physical_memory_pressure"}).HealthCheckScore
$Params2 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "security_password_policy"}).HealthCheckScore 
$Params3 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "configuration_network_packet_size"}).HealthCheckScore
$Params4 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "configuration_defaults"}).HealthCheckScore
$Params5 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "dr_backup"}).HealthCheckScore
$Params6 = ($Params.StaticHealthChecks | ? {$_.HealthCheckName -eq "dr_simple_recovery_model"}).HealthCheckScore

   if ($Result0 -eq $Params0) {return $true} 
   if ($Result1 -eq $Params1) {return $true} 
}

function StringVersions {
param([string]$inputString,$ExpectedValueString,$ActualValue,$Result)
  $obj = New-Object PSObject
  $obj | Add-Member NoteProperty -name 'Health Check Name' -value $inputString
  $obj | Add-Member NoteProperty -name 'ExpectedValue'-Value $ExpectedValueString
  $obj | Add-Member NoteProperty -name 'ActualValue' -Value $ActualValue
  $obj | Add-Member NoteProperty -name 'Result' -Value $Result

  Write-Host ($obj | Format-Table | Out-String)
}  


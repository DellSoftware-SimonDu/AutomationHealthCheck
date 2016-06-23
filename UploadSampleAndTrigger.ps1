
 function UploadSampleData ()
 {
 $url = "https://testapi.spotlightessentials.com/"
 $DSurl = $url + "/api/v2/diagnostic-servers"
 $healthcheckname = "healthcheckadhocworkloadconfiguration"
 $ProcedureUrl = $url + "/api/v2/procedure?dsid=" + $DiagnosticServerID + "&me=" + $ConnectionName + "&pack=sqlserver_spotlight&proc=" + $healthcheckname
 $ConnectionUrl = $url + "/api/v2/connections"
 $DiagnosticServerName = "DSGDQJDS72:3843"
 $DiagnosticServerID = "79cdafd4-1325-4fac-8c60-6279a711c587"
 $ConnectionName = "zhuw12r2spl502.melquest.dev.mel.au.qsft_sql2016_sqlserver"
 $Timestamp = [int][double]::Parse((Get-Date -UFormat %s))
 $ISOTimestamp = Get-Date -format 'u'
 $OwnerID = "29939"
 $OwnerGuid = "e475bcae-fd1b-4fd7-9f04-015095e81e53"
 $UserToken = "11d4bc029c174d90b6bcac1981022cad"

 $ProcedureUrl = $url + "/api/v2/procedure"

 $headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
 $headers.Add("x-user-token", $UserToken)

 # Create DS Connection
 $CreateDSjson = get-content "C:\Scripts\SampleDataold\JSONFile\CreateDS.JSON" 
 $CreateDSjson = $CreateDSjson -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID
 $Result = Invoke-RestMethod $DSurl -Method POST -Body $CreateDSjson -ContentType 'application/json' -Headers $headers
 Write-Host "Creating DS Result" 
 $Result

 # Create Windows Connection
 $CreateWinConnection = get-content "C:\Scripts\SampleDataold\JSONFile\CreateWinConnection.JSON" 
 $CreateWinConnection = $CreateWinConnection -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ConnectionName}}",$ConnectionName
 $Result=Invoke-RestMethod $ConnectionUrl -Method POST -Body $CreateWinConnection -ContentType 'application/json' -Headers $headers
 Write-Host "Create Windows Connection" 
 $Result

 # Create SQL Connection
 $CreateSQLConnection = get-content "C:\Scripts\SampleDataold\JSONFile\CreateSQLConnection.JSON" 
 $CreateSQLConnection = $CreateSQLConnection -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ConnectionName}}",$ConnectionName
 $Result=Invoke-RestMethod $ConnectionUrl -Method POST -Body $CreateSQLConnection -ContentType 'application/json' -Headers $headers
 Write-Host "Create SQL Connection" 
 $Result

 # Uploading Procedure healthcheckadhocworkloadconfiguration
 <#
 $ProcedureAdhocjson = get-content "C:\Scripts\SampleDataold\JSONFile\ProcedureAdhoc.json" 
 $ProcedureAdhocjson = $ProcedureAdhocjson -replace "{{Timestamp}}",$Timestamp -replace "{{ConnectionName}}",$ConnectionName -replace "{{DiagnosticServerName}}", , $DiagnosticServerID -replace "{{ISOTimestamp}}",$ISOTimestamp -replace "{{OwnerGuid}}",$OwnerGuid -replace "{{OwnerID}}",$OwnerID
 $Result = Invoke-RestMethod $ProcedureUrl -Method PUT -Body $ProcedureAdhocjson -ContentType 'application/json' -Headers $headers
 Write-Host "Uploading Procedure healthcheckadhocworkloadconfiguration" 
 $Result
 #>
 # Uploading Procedure healthcheckdatabasebackups
 $Proceduredatabasebackups = get-content "C:\Scripts\SampleDataold\JSONFile\Proceduredatabasebackups.json" 
 $Proceduredatabasebackups = $Proceduredatabasebackups -replace "{{Timestamp}}",$Timestamp -replace "{{ConnectionName}}",$ConnectionName -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ISOTimestamp}}",$ISOTimestamp -replace "{{OwnerGuid}}",$OwnerGuid -replace "{{OwnerID}}",$OwnerID
 $result = Invoke-RestMethod $ProcedureUrl -Method PUT -Body $Proceduredatabasebackups -ContentType 'application/json' -Headers $headers
 Write-Host "Uploading Procedure healthcheckdatabasebackups" 
 $Result

 # Uploading Procedure healthcheckdatabases
 $Proceduredatabases = get-content "C:\Scripts\SampleDataold\JSONFile\Proceduredatabases.json" 
 $Proceduredatabases = $Proceduredatabases -replace "{{Timestamp}}",$Timestamp -replace "{{ConnectionName}}",$ConnectionName -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ISOTimestamp}}",$ISOTimestamp -replace "{{OwnerGuid}}",$OwnerGuid -replace "{{OwnerID}}",$OwnerID
 $Result = Invoke-RestMethod $ProcedureUrl -Method PUT -Body $Proceduredatabases -ContentType 'application/json' -Headers $headers
 Write-Host "Uploading Procedure healthcheckdatabases" 
 $Result

 # Uploading Procedure healthcheckguestaccess
 $Procedureguestaccess = get-content "C:\Scripts\SampleDataold\JSONFile\Procedureguestaccess.json" 
 $Procedureguestaccess = $Procedureguestaccess -replace "{{Timestamp}}",$Timestamp -replace "{{ConnectionName}}",$ConnectionName -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ISOTimestamp}}",$ISOTimestamp -replace "{{OwnerGuid}}",$OwnerGuid -replace "{{OwnerID}}",$OwnerID
 $Result = Invoke-RestMethod $ProcedureUrl -Method PUT -Body $Procedureguestaccess -ContentType 'application/json' -Headers $headers
 Write-Host "Uploading Procedure healthcheckguestaccess" 
 $Result

 # Uploading Procedure healthcheckmissingindexes
 $Proceduremissingindexes = get-content "C:\Scripts\SampleDataold\JSONFile\Proceduremissingindexes.json" 
 $Proceduremissingindexes = $Proceduremissingindexes -replace "{{Timestamp}}",$Timestamp -replace "{{ConnectionName}}",$ConnectionName -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ISOTimestamp}}",$ISOTimestamp -replace "{{OwnerGuid}}",$OwnerGuid -replace "{{OwnerID}}",$OwnerID
 $Result = Invoke-RestMethod $ProcedureUrl -Method PUT -Body $Proceduremissingindexes -ContentType 'application/json' -Headers $headers
 Write-Host "Uploading Procedure healthcheckmissingindexes" 
 $Result

 # Uploading Procedure healthcheckosprocessmemory
 $Procedureosprocessmemory = get-content "C:\Scripts\SampleDataold\JSONFile\Procedureosprocessmemory.json" 
 $Procedureosprocessmemory = $Procedureosprocessmemory -replace "{{Timestamp}}",$Timestamp -replace "{{ConnectionName}}",$ConnectionName -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ISOTimestamp}}",$ISOTimestamp -replace "{{OwnerGuid}}",$OwnerGuid -replace "{{OwnerID}}",$OwnerID
 $Result = Invoke-RestMethod $ProcedureUrl -Method PUT -Body $Procedureosprocessmemory -ContentType 'application/json' -Headers $headers
 Write-Host "Uploading Procedure healthcheckosprocessmemory" 
 $Result

 # Uploading Procedure healthcheckossystemmemory
 $Procedureossystemmemory = get-content "C:\Scripts\SampleDataold\JSONFile\Procedureossystemmemory.json" 
 $Procedureossystemmemory = $Procedureossystemmemory -replace "{{Timestamp}}",$Timestamp -replace "{{ConnectionName}}",$ConnectionName -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ISOTimestamp}}",$ISOTimestamp -replace "{{OwnerGuid}}",$OwnerGuid -replace "{{OwnerID}}",$OwnerID
 $Result = Invoke-RestMethod $ProcedureUrl -Method PUT -Body $Procedureossystemmemory -ContentType 'application/json' -Headers $headers
 Write-Host "Uploading Procedure healthcheckossystemmemory" 
 $Result

 # Uploading Procedure healthchecksysinfo
 $Proceduresysinfo = get-content "C:\Scripts\SampleDataold\JSONFile\Proceduresysinfo.json" 
 $Proceduresysinfo = $Proceduresysinfo -replace "{{Timestamp}}",$Timestamp -replace "{{ConnectionName}}",$ConnectionName -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ISOTimestamp}}",$ISOTimestamp -replace "{{OwnerGuid}}",$OwnerGuid -replace "{{OwnerID}}",$OwnerID
 $Result = Invoke-RestMethod $ProcedureUrl -Method PUT -Body $Proceduresysinfo -ContentType 'application/json' -Headers $headers 
 Write-Host "Uploading Procedure healthchecksysinfo" 
 $Result

 # Uploading Procedure Proceduresystemconfiguration
 $Proceduresystemconfiguration = get-content "C:\Scripts\SampleDataold\JSONFile\Proceduresystemconfiguration.json" 
 $Proceduresystemconfiguration = $Proceduresystemconfiguration -replace "{{Timestamp}}",$Timestamp -replace "{{ConnectionName}}",$ConnectionName -replace "{{DiagnosticServerName}}", $DiagnosticServerName -replace "{{DiagnosticServerID}}" , $DiagnosticServerID -replace "{{ISOTimestamp}}",$ISOTimestamp -replace "{{OwnerGuid}}",$OwnerGuid -replace "{{OwnerID}}",$OwnerID
 $Result = Invoke-RestMethod $ProcedureUrl -Method PUT -Body $Proceduresystemconfiguration -ContentType 'application/json' -Headers $headers
 Write-Host "Uploading Procedure Proceduresystemconfiguration" 
 $Result
 }

function SendMessageToQueue()
{
$StorageAccountName = "lucystagingstorageus"
$StorageAccountKey = "s1bQ/9f2S5JxPMBw7OhKMSGAoqVBAhAtu4yyDnugD/kbIo/OXZRh8E8IYRkya2m7dxc9KFF4+9XkYYbdKfHznw=="
$Ctx = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey

$data = Get-AzureStorageQueue -Name system-healthcheck-test -Context $Ctx
$queueMessage = New-Object -TypeName Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage -ArgumentList '{"Command": "ProcessOwner", "OwnerGuid": "e475bcae-fd1b-4fd7-9f04-015095e81e53", "OwnerID": 29939,"Comment": "Created by Iain on 2016-06-14 for Simon Du testing"}'

$queueMessage
$data.CloudQueue.AddMessage($queueMessage)
Write-Host "Send ProcessOwner command to Storage Queue to analyze uploaded data"
}


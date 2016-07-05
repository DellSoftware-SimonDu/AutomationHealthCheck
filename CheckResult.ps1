############################################################### 
#
#   before send test message to Queue, download LATEST.json and rename to OLD.json   
#
###############################################################

write-host "###############################################################"
write-host "#"
write-host "#  before send test message to Queue, download LATEST.json and rename to OLD.json"
write-host "#"
write-host "###############################################################"

#verify the DatePerformed of result JSON file is later than the one before putting the Queue message
function GetFileContent($Path,$ContentName){    #get the key value from JSON file
   $FileContent = get-content $Path | ConvertFrom-json
   return $FileContent.$ContentName
}

$BlobName = "e475bcae-fd1b-4fd7-9f04-015095e81e53/2/54ab6b58-8931-46dd-88ae-b9e80c7195c0/zhuw2k8r2spl300.melquest.dev.mel.au.qsft_sql2008r2_sqlserver/LATEST.json"
$StorageAccountName = "lucystagingstorageus" 
$StorageAccountKey = "s1bQ/9f2S5JxPMBw7OhKMSGAoqVBAhAtu4yyDnugD/kbIo/OXZRh8E8IYRkya2m7dxc9KFF4+9XkYYbdKfHznw==" 
$Context = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey 
$ContainerName = "sqlserver-static-health-check"

#create local path for result JSON

$ResultJSONFolder = 'C:\json\result'
new-item -Path $ResultJSONFolder -ItemType Directory -Force

Function GetJSONFile ($ResultFolder){

    Get-AzureStorageBlobContent -Container $ContainerName -Blob $BlobName -Destination $ResultFolder -Context $Context -Force
    $AllChildItem = Get-ChildItem $ResultJSONFolder -Include *.json -Recurse  
    if($AllChildItem.Length -gt 0){
        write-host "JSON file in $ResultFolder is generated"
    }
    else {
        Write-warning "Unable to find JSON file in $ResultFolder"
    }
}


GetJSONFile ($ResultJSONFolder)

#change the json file name 
Get-ChildItem -Path $ResultJSONFolder -Filter '*.json' -Recurse | Rename-Item -NewName old.json -Force

###############################################################
#
#   send test message to Queue to trigger the schedule job
#
###############################################################
write-host "###############################################################"
write-host "#"
write-host "#  send test message to Queue to trigger the schedule job"
write-host "#"
write-host "###############################################################"

$StorageAccountName = "lucystagingstorageus" 
$StorageAccountKey = "s1bQ/9f2S5JxPMBw7OhKMSGAoqVBAhAtu4yyDnugD/kbIo/OXZRh8E8IYRkya2m7dxc9KFF4+9XkYYbdKfHznw==" 

function SendMessageToQueue($QueueName,$QueueMessage) {

    $Ctx = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey 
    $data = Get-AzureStorageQueue -Name $QueueName -Context $Ctx
    $data.CloudQueue.AddMessage($queueMessage)  
    return 0
}

$QName = 'system-healthcheck-test'
$Qmsg = New-Object -TypeName Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage -ArgumentList '{"Command": "ProcessOwner", "OwnerGuid": "e475bcae-fd1b-4fd7-9f04-015095e81e53", "OwnerID": 29939}'

$Res = SendMessageToQueue -QueueName $Qname -QueueMessage $Qmsg
if ($Res -eq 0) {
    write-host "Message has been sent to $QName"
}
else{
    $host.UI.WriteErrorLine("Message has not been sent to $QName")
}


Start-Sleep -seconds 120  #set wait time for file is generated


###############################################################################
#
#  Download the result JSON file from container 'sqlserver-static-health-check' 
#
###############################################################################
write-host "###############################################################"
write-host "#"
write-host "#  Download the result JSON file from blob container"
write-host "#"
write-host "###############################################################"

#download the result JSON

$ResultJSONFolder = 'C:\json\result'
GetJSONFile ($ResultJSONFolder)


$OwnerGuid = "e475bcae-fd1b-4fd7-9f04-015095e81e53"
$ConnectionName = "zhuw2k8r2spl300.melquest.dev.mel.au.qsft_sql2008r2_sqlserver" 
$DiagnosticServerID = "54ab6b58-8931-46dd-88ae-b9e80c7195c0"


$ResultJSONfile = $ResultJSONFolder + "\"+ $OwnerGuid + "\2\"+$DiagnosticServerID+"\"+$ConnectionName+"\LATEST.json"
$OldResultJSONfile = $ResultJSONFolder + "\"+ $OwnerGuid + "\2\"+$DiagnosticServerID+"\"+$ConnectionName+"\old.json"

#convert the file content to json format

$LatestJSON = (Get-Content $ResultJSONfile) -join "`n" | ConvertFrom-Json
$OldJSON = (Get-Content $OldResultJSONfile) -join "`n" | ConvertFrom-Json

write-host "Update date of LATEST.json" $LATESTJSON.DatePerformed
write-host "Update date of old.json" $OldJSON.DatePerformed

#Compare the DatePerformed in LATEST.json and old.json
 
if($LatestJSON.DatePerformed -gt $OldJSON.DatePerformed){
     write-host "Correct JSON is generated"
   }
   else{
       $host.UI.WriteErrorLine("ERROR: $ResultJSONfile is not generated")
      }

###############################################################################
#
#  Check health check result 
#
###############################################################################
write-host "###############################################################"
write-host "#"
write-host "#  Check health check results"
write-host "#"
write-host "###############################################################"



function CheckResult
{
$CompareJson = @'
  {
  "TotalScoreIncludingIgnoredChecks": [
    77.443280977312384,
    75.443280977312384
  ],
  "TotalScoreExcludingIgnoredChecks": [
    77.443280977312384,
    75.443280977312384
  ],
  "TopIssues": {
    "_t": "a",
    "4": [
      "memory_physical_memory_pressure"
    ]
  },
  "StaticHealthChecks": {
    "_t": "a",
    "0": {
      "HealthCheckScore": [
        100.0,
        95.0
      ]
    }
  },
  "PhysicalMemoryPressure": {
    "TrendAnalysis": {
      "TrendExists": [
        false,
        true
      ],
      "TrendIsIncreasing": [
        false,
        true
      ]
    }
  }
}
'@

if ($CompareJson -eq $null)
{
    return $true
}
}  

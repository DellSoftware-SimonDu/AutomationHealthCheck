#Requires -Version 3.0
#Requires -Module Azure
#Requires -Module Azure.Storage

Param(
    [double] $Timestamp,
    [string] $LucyStagingStorageUSKey
)
write-host "###############################################################"
write-host "#"
write-host "#  send test message to Queue to trigger the schedule job"
write-host "#"
write-host "###############################################################"

Set-StrictMode -Version 3

function SendMessageToQueue()
{
$StorageAccountName = "lucystagingstorageus"
$StorageAccountKey = $LucyStagingStorageUSKey
$Ctx = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey

$data = Get-AzureStorageQueue -Name system-healthcheck-test -Context $Ctx
$queueMessage = New-Object -TypeName Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage -ArgumentList '{"Command": "ProcessOwner", "OwnerGuid": "e475bcae-fd1b-4fd7-9f04-015095e81e53", "OwnerID": 29939}'
$queueMessage 
$data.CloudQueue.AddMessage($queueMessage)
Write-Host "Send ProcessOwner command to Storage Queue to analyze uploaded data"
Write-Host "Waiting 120 seconds"
}

SendMessageToQueue

#set wait time 120s waiting Json result file
Start-Sleep -seconds 120  


write-host "###############################################################"
write-host "#"
write-host "#  Download the result JSON file from blob container"
write-host "#"
write-host "###############################################################"
$BlobName2008R2 = "e475bcae-fd1b-4fd7-9f04-015095e81e53/2/54ab6b58-8931-46dd-88ae-b9e80c7195c0/zhuw2k8r2spl300$Timestamp.melquest.dev.mel.au.qsft_sql2008r2_sqlserver/LATEST.json"
$BlobName2016 = "e475bcae-fd1b-4fd7-9f04-015095e81e53/2/54ab6b58-8931-46dd-88ae-b9e80c7195c0/zhuw12r2spl501.melquest.dev.mel.au.qsft_sql2016_sqlserver/LATEST.json"
$StorageAccountName = "lucystagingstorageus" 
$Context = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey
$ContainerName = "sqlserver-static-health-check"
$LatestJsonFile2008R2 = $PSScriptRoot + "\e475bcae-fd1b-4fd7-9f04-015095e81e53\2\54ab6b58-8931-46dd-88ae-b9e80c7195c0" + "\zhuw2k8r2spl300$Timestamp.melquest.dev.mel.au.qsft_sql2008r2_sqlserver"
$LatestJsonFile2016 = $PSScriptRoot + "\e475bcae-fd1b-4fd7-9f04-015095e81e53\2\54ab6b58-8931-46dd-88ae-b9e80c7195c0" + "\zhuw12r2spl501.melquest.dev.mel.au.qsft_sql2016_sqlserver"

Function GetJSONFile 
{
    Param (
    [string]$ContainerName, 
    [string]$BlobName,
    [string]$Destination,
            $Context,
    [string]$LatestJsonFile
   ) 

    Get-AzureStorageBlobContent -Container $ContainerName -Blob $BlobName -Destination $Destination -Context $Context -Force 
    if(Test-Path -Path $LatestJsonFile\LATEST.json){
        Write-Host "JSON file in $LatestJsonFile is generated"
    }
    else {
        Write-Error "Unable to find JSON file in $LatestJsonFile"
        exit 1
    }
}
#download the result JSON file
GetJSONFile -ContainerName $ContainerName -BlobName $BlobName2008R2 -Destination $PSScriptRoot -Context $Context -LatestJsonFile $LatestJsonFile2008R2
GetJSONFile -ContainerName $ContainerName -BlobName $BlobName2016 -Destination $PSScriptRoot -Context $Context -LatestJsonFile $LatestJsonFile2016

function CompareJsonResult
{
    Param (
    [string] $diffexpectresult = $Timestamp.ToString() + 'expect',
    [string] $diffactualresult = $Timestamp.ToString() + 'actual',
    [string] $CompareJson
    ) 

    write-host "###############################################################"
    write-host "#"
    write-host "#  Check health check results"
    write-host "#"
    write-host "###############################################################"

    [Reflection.Assembly]::LoadFile("$PSScriptRoot\Tools\JsonDiffPatchDotNet.dll")
    [Reflection.Assembly]::LoadFile("$PSScriptRoot\Tools\Newtonsoft.Json.dll")

    $Parttern = "20\d\d-\d\d-\d\dT\d\d:\d\d:\d\d(.\d+)?Z" #Match 2016-07-15T03:40:08.984Z,2016-07-15T03:40:25.0242839Z
                                                    #2016-07-15T00:00:00Z  2016-07-15T03:40:37.8031999Z


    $ExpectResult = (Get-Content "$PSScriptRoot\SampleData\HealthCheck-ExpectResult.json") -replace "zhuw2k8r2spl300", "zhuw2k8r2spl300$Timestamp"
    $ActualResult = Get-Content  "$LatestJsonFile2008R2\LATEST.json"

    $ExpectResult= $ExpectResult -replace $Parttern, ""   -join "`n" | ConvertFrom-Json
    $ActualResult= $ActualResult -replace $Parttern, ""   -join "`n" | ConvertFrom-Json

    $Array = (0..($ExpectResult.StaticHealthChecks.Count-1))
    foreach ($i in $Array)
    {
        if($ExpectResult.StaticHealthChecks[$i].HealthCheckScore -eq $ActualResult.StaticHealthChecks[$i].HealthCheckScore)
        {
            $ExpectResult.StaticHealthChecks[$i].DebugInfo = "" 
            $ActualResult.StaticHealthChecks[$i].DebugInfo = ""
        }
    }

    $ExpectResult = ConvertTo-Json $ExpectResult
    $ActualResult = ConvertTo-Json $ActualResult 

    $JsonDiffPatch = new-object JsonDiffPatchDotNet.JsonDiffPatch
    $CompareJson = $JsonDiffPatch.Diff($ExpectResult,$ActualResult)

    if ($CompareJson.Length -eq 0)   
        {
            return $true
        }
    else
        {
        $LeftCallback = "leftCallback({left});"
        $LeftCallback = $LeftCallback -replace "{left}",$ExpectResult | Out-File  $PSScriptRoot\$diffexpectresult + '.json' -Force

        $RightCallBack = "rightCallback({right});"
        $RightCallBack -replace "{right}",$ActualResult |  Out-File  $PSScriptRoot\$diffactualresult + '.json' -Force

        $SpotlighAutomationStoragetKey = $LucyStagingStorageUSKey
        $context = New-AzureStorageContext -StorageAccountName integration-test-result -StorageAccountKey $SpotlighAutomationStoragetKey 
        Set-AzureStorageBlobContent  -Container integration-test-result -File $PSScriptRoot + '\' + $diffexpectresult + '.json' -Context $context -Force
        Set-AzureStorageBlobContent  -Container integration-test-result -File $PSScriptRoot + '\' + $diffactualresult + '.json' -Context $context -Force
        $DebugRUL = "https://simondu123.github.io/?left=https://lucystagingstorageus.blob.core.windows.net/integration-test-result/$diffexpectresult.json `
        &right=https://lucystagingstorageus.blob.core.windows.net/integration-test-result/$diffactualresult.json"
        $CompareJson = $CompareJson + "`n" + "Please access below link for detailed information" + "`n"+ $DebugRUL + "`n"
        Throw $CompareJson
        }

}

function CompareIOWriteLog()
{

    $ExpectResultJson = (Get-Content $LatestJsonFile2016\LATEST.json) | ConvertFrom-Json
    $FactsCount = $ExpectResultJson.WriteLogWait.Facts.Count
    $Count = 0
    $Array = (0..($FactsCount-1))

if ($ExpectResultJson.WriteLogWait -eq $null)
{
    Write-Error "Can't get Health Check io_writelog_wait procedure"
    return $false
}
Foreach ($i in $ExpectResultJson.StaticHealthChecks) 
    {
        if ($i.HealthCheckName -eq "io_writelog_wait")
        {
            $IOWriteLogActualScore = $i.HealthCheckScore 
        }
    }


Foreach ($i in $Array)
    {
        if($ExpectResultJson.WriteLogWait.Facts[$i].Isbad -eq $true)
            {
                
                $Count++
            }
    }
    #Count is all bad facts, facts.count is all counts.  
    $IOWriteLogExpectScore = 100.0 - (100.0 * $Count / $FactsCount) 

if ($IOWriteLogExpectScore -eq $IOWriteLogActualScore)
    {
        return $true
    }
else
    {
        throw "Expect score is $IOWriteLogExpectScore, but Actual score is $IOWriteLogActualScore" 
    }
}

function RemoveJsonFiles()
{
    Remove-Item -Path $PSScriptRoot + "\e475bcae-fd1b-4fd7-9f04-015095e81e53" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path $PSScriptRoot -Include *.json -Force -ErrorAction SilentlyContinue
}
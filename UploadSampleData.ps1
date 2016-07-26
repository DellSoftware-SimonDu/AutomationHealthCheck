
Param(
    [string] $TestServerUrl = 'https://testapi.spotlightessentials.com/',
    [string] $TestServerDSUrl = $TestServerUrl + '/api/v2/diagnostic-servers',
    [string] $TestServerProcedureUrl = $TestServerUrl + '/api/v2/procedure',
    [string] $TestServerConnectionUrl = $TestServerUrl + '/api/v2/connections-sync',
    [double] $Global:Timestamp
)

write-host "###############################################################"
write-host "#"
write-host "#  upload JSON files"
write-host "#"
write-host "###############################################################"

# read UserToken from file
$UserToken = Get-Content "$PSScriptRoot\SampleData\SpotlightAutomationAccountUsertoken.txt"

# define header info
$Headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$Headers.Add("x-user-token", $UserToken)

# Get current timestamp
function ConvertTo-UnixTimestamp { 
        $epoch = Get-Date -Year 1970 -Month 1 -Day 1 -Hour 0 -Minute 0 -Second 0        
        $input | % {            
                $milliSeconds = [math]::truncate($_.ToUniversalTime().Subtract($epoch).TotalMilliSeconds) 
                Write-Output $milliSeconds 
        }        
}   

$Timestamp = Get-Date | ConvertTo-UnixTimestamp

Write-Host "Current timestamp is" $Timestamp

$2008R2Procedulefiles = (Get-Childitem $PSScriptRoot\SampleData\2008R2Procedures).FullName
$2016Procedurefiles = (Get-Childitem $PSScriptRoot\SampleData\2016Procedures).FullName

Write-Host "Uploading DS data"
$DSjson = get-content "$PSScriptRoot\SampleData\UpdateDS.json"
$Result = Invoke-RestMethod $TestServerDSUrl -Method POST -Body $DSjson -ContentType 'application/json' -Headers $headers 
$Result

Write-Host "Uploading Connections data"
$ConnectionJson = (get-content "$PSScriptRoot\SampleData\UpdateConnections.json") -replace "zhuw2k8r2spl300", "zhuw2k8r2spl300$Timestamp" 
$Result=Invoke-RestMethod $TestServerConnectionUrl -Method POST -Body $ConnectionJson -ContentType 'application/json' -Headers $headers
$Result

# Upload 2008 R2 Procudres data
Foreach($Pfile in $2008R2Procedulefiles)
{
    Write-Host "Uploading $Pfile" 
    $ProcedureJson = Get-Content $Pfile
    $ProcedureJson = $ProcedureJson -replace "{{Timestamp}}",$Timestamp -replace "zhuw2k8r2spl300", "zhuw2k8r2spl300$Timestamp"
    $Result = Invoke-RestMethod $TestServerProcedureUrl -Method PUT -Body $ProcedureJson -ContentType 'application/json' -Headers $headers
    $Result
}
# Upload 2016 Procudres data
Foreach($Pfile in $2016Procedurefiles)
{
    Write-Host "Uploading $Pfile" 
    $ProcedureJson = Get-Content $Pfile
    $ProcedureJson = $ProcedureJson -replace "{{Timestamp}}",$Timestamp 
    $Result = Invoke-RestMethod $TestServerProcedureUrl -Method PUT -Body $ProcedureJson -ContentType 'application/json' -Headers $headers
    $Result
}
#  Delete Connections 
function DeleteConnections ()
{
    write-host "###############################################################"
    write-host "#"
    write-host "#  Deleting Connection"
    write-host "#"
    write-host "###############################################################"
    $QATeamIntegrationTest_sqlserver = $TestServerUrl + "/api/v2/connections?dsid=54ab6b58-8931-46dd-88ae-b9e80c7195c0&me=zhuw2k8r2spl300$Timestamp.melquest.dev.mel.au.qsft_sql2008r2_sqlserver"
    $QATeamIntegrationTest_winserver = $TestServerUrl + "/api/v2/connections?dsid=54ab6b58-8931-46dd-88ae-b9e80c7195c0&me=zhuw2k8r2spl300$Timestamp.melquest.dev.mel.au.qsft"
    Write-Host "Deleting QATeamIntegrationTest_sqlserver connection"
    $Result=Invoke-RestMethod $QATeamIntegrationTest_sqlserver -Method Delete  -Headers $headers
    Write-Host "Deleting QATeamIntegrationTest_winserver connection"
    $Result=Invoke-RestMethod $QATeamIntegrationTest_winserver -Method Delete  -Headers $headers 
}

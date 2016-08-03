param (
    [long] $Timestamp
)

[string] $TestServerUrl = 'https://testapi.spotlightessentials.com/'
[string] $TestServerDSUrl = $TestServerUrl + '/api/v2/diagnostic-servers'
[string] $TestServerProcedureUrl = $TestServerUrl + '/api/v2/procedure'
[string] $TestServerConnectionUrl = $TestServerUrl + '/api/v2/connections-sync'

Write-Output "###############################################################"
Write-Output "#"
Write-Output "#  upload JSON files"
Write-Output "#"
Write-Output "###############################################################"

# read UserToken from file
$UserToken = Get-Content "C:\Scripts\SampleData\SpotlightAutomationAccountUsertoken.txt"

# define header info
$Headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$Headers.Add("x-user-token", $UserToken)

Write-Output "Current timestamp is" $Timestamp

$2008R2Procedulefiles = (Get-Childitem C:\Scripts\SampleData\2008R2Procedures).FullName
$2016Procedurefiles = (Get-Childitem C:\Scripts\SampleData\2016Procedures).FullName

Write-Output "Uploading DS data"
$DSjson = get-content "C:\Scripts\SampleData\UpdateDS.json"
$Result = Invoke-RestMethod $TestServerDSUrl -Method POST -Body $DSjson -ContentType 'application/json' -Headers $headers 
Write-Output $Result.Result

Write-Output "Uploading Connections data"
$ConnectionJson = (get-content "C:\Scripts\SampleData\UpdateConnections.json") -replace "zhuw2k8r2spl300", "zhuw2k8r2spl300$Timestamp" 
$Result=Invoke-RestMethod $TestServerConnectionUrl -Method POST -Body $ConnectionJson -ContentType 'application/json' -Headers $headers
Write-Output $Result.Result


# Upload 2008 R2 Procudres data
Foreach($Pfile in $2008R2Procedulefiles)
{
    Write-Output "Uploading $Pfile" 
    $ProcedureJson = Get-Content $Pfile
    $ProcedureJson = $ProcedureJson -replace "{{Timestamp}}",$Timestamp -replace "zhuw2k8r2spl300", "zhuw2k8r2spl300$Timestamp"
    $Result = Invoke-RestMethod $TestServerProcedureUrl -Method PUT -Body $ProcedureJson -ContentType 'application/json' -Headers $headers
    Write-Output $Result.Result
}
# Upload 2016 Procudres data
Foreach($Pfile in $2016Procedurefiles)
{
    Write-Output "Uploading $Pfile" 
    $ProcedureJson = Get-Content $Pfile
    $ProcedureJson = $ProcedureJson -replace "{{Timestamp}}",$Timestamp 
    $Result = Invoke-RestMethod $TestServerProcedureUrl -Method PUT -Body $ProcedureJson -ContentType 'application/json' -Headers $headers
    Write-Output $Result.Result
}

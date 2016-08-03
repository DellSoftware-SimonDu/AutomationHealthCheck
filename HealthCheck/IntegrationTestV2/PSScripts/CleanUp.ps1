param (
    [long] $Timestamp
)

[string] $TestServerUrl = 'https://testapi.spotlightessentials.com/'
[string] $TestServerDSUrl = $TestServerUrl + '/api/v2/diagnostic-servers'
[string] $TestServerProcedureUrl = $TestServerUrl + '/api/v2/procedure'
[string] $TestServerConnectionUrl = $TestServerUrl + '/api/v2/connections-sync'

write-host "###############################################################"
write-host "#"
write-host "#  Deleting Connection"
write-host "#"
write-host "###############################################################"

# read UserToken from file
$UserToken = Get-Content "C:\Scripts\SampleData\SpotlightAutomationAccountUsertoken.txt"

# define header info
$Headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$Headers.Add("x-user-token", $UserToken)

Write-Output $Timestamp

$QATeamIntegrationTest_sqlserver = $TestServerUrl + "/api/v2/connections?dsid=54ab6b58-8931-46dd-88ae-b9e80c7195c0&me=zhuw2k8r2spl300$Timestamp.melquest.dev.mel.au.qsft_sql2008r2_sqlserver"
$QATeamIntegrationTest_winserver = $TestServerUrl + "/api/v2/connections?dsid=54ab6b58-8931-46dd-88ae-b9e80c7195c0&me=zhuw2k8r2spl300$Timestamp.melquest.dev.mel.au.qsft"
Write-Output "Deleting QATeamIntegrationTest_sqlserver connection"

$Result=Invoke-RestMethod $QATeamIntegrationTest_sqlserver -Method Delete  -Headers $headers
Write-Output $Result.Result

Write-Output "Deleting QATeamIntegrationTest_winserver connection"
$Result=Invoke-RestMethod $QATeamIntegrationTest_winserver -Method Delete  -Headers $headers 
Write-Output $Result.Result

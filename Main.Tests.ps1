
.\UploadSampleData.ps1
. .\CheckResult.ps1

Describe -Tags "HealthCheck" "HealthCheck Results" {
 
    It "Checking health results" {
       if ($CompareJson -eq $null)
    {
        Checkresult | Should Be $true
    }
       else
    {
        throw $CompareJson
    }
   }
}


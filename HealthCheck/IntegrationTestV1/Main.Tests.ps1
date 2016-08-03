#Requires -Version 3.0

Param(
    [int] $LASTEXITCODE
)

#########Upload sample data to Test Essential website#######
. .\UploadSampleData.ps1 

#########Analyze Health Check data##########################
. .\CheckResult.ps1 -Timestamp $Timestamp  -LucyStagingStorageUSKey $LucyStagingStorageUSKey -BuildNumber $BuildNumber

# Delete new added random connections
DeleteConnections 


if ($LASTEXITCODE -gt 0)
{
    exit 1
}


Describe -Tags "HealthCheck" "HealthCheck Results" {
 
    It "Checking health check All results" {
       {CompareJsonResult} | Should Not Throw
    }
    It "Checking IO_WriteLog_Wait" {
        
       CompareIOWriteLog | Should be $true
    }
}

# Delete files in Jenkins workspace
RemoveJsonFiles
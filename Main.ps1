
#########Upload sample data to Test Essential website#######


cd C:\pester
. .\UploadSampleAndTrigger.ps1
UploadSampleData
SendMessageToQueue


#########Analyze Health Check data##########################
. .\CheckResult.ps1
 
CopyJsonFile
StringVersions

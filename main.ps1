
#########Upload sample data to Test Essential website#######
#cd "C:\Program Files (x86)\Jenkins\jobs\systemhealthchecks-test\workspace"
.\UploadSampleData.ps1


#########Analyze Health Check data##########################
. .\CheckResult.ps1
StringVersions



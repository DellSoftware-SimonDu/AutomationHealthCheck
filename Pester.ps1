#Requires -Version 3.0

Param(
    [Parameter(Mandatory=$true)]
    [string] $BuildNumber

)

Write-Output $BuildNumber

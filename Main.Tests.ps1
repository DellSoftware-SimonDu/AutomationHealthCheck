$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. "$here\main.ps1"
 

Describe -Tags "HealthCheck" "HealthCheck Results" {
 
    It "Checking health results" {
       CheckResult | Should Be $true
    }
   
   }
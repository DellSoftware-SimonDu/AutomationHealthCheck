$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. "$here\main.ps1"
 

Describe -Tags "HealthCheck" "HealthCheck Results" {
 
    It "Checking health check All results" {
       CheckResult | Should Be "All True"
    }
    It "TotalScoreIncludingIgnoredChecks results" {
       CheckIndividualResult -Result0 84.0023269342641 | Should Be $true
    }
    It "memory_physical_memory_pressure results" {
       CheckIndividualResult -Result1 99| Should Be $true
    }
}
#
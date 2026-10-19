param([string]$LanIp = '127.0.0.1')
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:Database__Provider = 'InMemory'
$env:Demo__Enabled = 'true'
$env:ASPNETCORE_URLS = 'http://0.0.0.0:5050'
$env:Logging__LogLevel__Default = 'Information'
$env:Logging__LogLevel__Microsoft = 'Warning'
$env:Demo__PublicApiUrl = "http://${LanIp}:5050"
Write-Host "RideMate demo: http://${LanIp}:5050/swagger"
Write-Host 'Login: demo01@ridemate.test / RideMate123! (demo01..demo20)'
Write-Host 'All data is reset when the API process stops.'
dotnet run --no-launch-profile --project backend/src/RideMate.Api


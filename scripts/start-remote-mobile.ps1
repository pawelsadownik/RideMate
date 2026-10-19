param([Parameter(Mandatory=$true)][string]$LanIp)
$ErrorActionPreference = 'Stop'
try {
    $ready = Invoke-RestMethod -Uri 'http://127.0.0.1:8050/health' -TimeoutSec 5
    if ($ready.status -ne 'healthy') { throw 'API not healthy' }
} catch {
    throw 'First open the separate RideMate SSH tunnel using deploy/ridemate-ssh.config. Keep it open and enter SSH passwords only in the terminal.'
}
$env:EXPO_PUBLIC_API_URL = "http://${LanIp}:8050"
Set-Location (Join-Path $PSScriptRoot '../mobile')
npx expo start --go --lan --port 8082

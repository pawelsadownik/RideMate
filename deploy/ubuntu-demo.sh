#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
source_root=$(cd -- "$(dirname -- "$0")/.." && pwd)
revision=${RIDEMATE_REVISION:?Exact RideMate revision required}
public_url=${RIDEMATE_PUBLIC_API_URL:?Phone-accessible API URL required}
[[ "$revision" =~ ^[0-9a-f]{40}$ ]] || { echo 'Invalid revision'; exit 1; }
[[ $(git -C "$source_root" rev-parse HEAD) = "$revision" ]] || { echo 'Source revision mismatch'; exit 1; }
[[ $(sed -n '1p' "$source_root/deploy/compose.demo.yml") = 'name: ridemate-demo' ]] || exit 1
runtime="$HOME/ridemate-demo"
[[ ! -L "$runtime" ]] || { echo 'Refusing a symlink runtime'; exit 1; }
if [[ -e "$runtime" ]]; then
  [[ -f "$runtime/.ridemate-demo" && $(cat "$runtime/.ridemate-demo") = ridemate-demo ]] || {
    echo 'Existing unrelated runtime directory; refusing to modify it'; exit 1;
  }
fi
docker info >/dev/null
[[ $(df -Pk "$HOME" | awk 'NR==2 {print $4}') -ge 3145728 ]] || { echo 'Need at least 3 GiB free disk'; exit 1; }
[[ $(awk '/MemAvailable:/ {print $2}' /proc/meminfo) -ge 1048576 ]] || { echo 'Need at least 1 GiB available RAM; Atlas is left untouched'; exit 1; }
mapfile -t atlas_ids < <(docker ps -q --filter label=com.docker.compose.project=atlas)
[[ ${#atlas_ids[@]} -gt 0 ]] || { echo 'Atlas baseline not found; inspect host before deployment'; exit 1; }
atlas_before=$(docker inspect --format '{{.Id}} {{.State.Running}} {{.State.StartedAt}}' "${atlas_ids[@]}")
check_atlas() {
  local after
  after=$(docker inspect --format '{{.Id}} {{.State.Running}} {{.State.StartedAt}}' "${atlas_ids[@]}")
  [[ "$after" = "$atlas_before" ]] || { echo 'Atlas baseline changed; investigate without restarting Atlas'; return 1; }
  echo 'Atlas container IDs, running state and start times are unchanged.'
}
trap check_atlas EXIT
existing_api=$(docker ps -aq --filter label=com.docker.compose.project=ridemate-demo --filter label=com.docker.compose.service=api)
if [[ -z "$existing_api" && -n $(ss -H -ltn 'sport = :8050') ]]; then
  echo 'Port 8050 is already in use; refusing deployment'; exit 1
fi
mkdir -p "$runtime/releases" "$runtime/backups"
printf '%s\n' ridemate-demo > "$runtime/.ridemate-demo"
exec 9>"$runtime/deploy.lock"
flock -n 9 || { echo 'Another RideMate deployment is running'; exit 1; }
release="$runtime/releases/$revision"
[[ ! -e "$release" ]] || { echo 'Revision already staged; use an explicit new revision rather than overwrite it'; exit 1; }
mkdir "$release"
cp -a "$source_root/backend" "$source_root/deploy" "$release/"
python3 - "$runtime/.env" "$public_url" "$revision" <<'PY'
import os, pathlib, secrets, sys
from urllib.parse import urlsplit
path = pathlib.Path(sys.argv[1])
url = sys.argv[2].rstrip('/')
parsed = urlsplit(url)
if parsed.scheme not in ('http','https') or not parsed.hostname or parsed.username or parsed.password or parsed.path or parsed.query or parsed.fragment or any(c.isspace() for c in url):
    raise SystemExit('Invalid public API origin')
values = dict(line.split('=',1) for line in path.read_text().splitlines() if '=' in line) if path.exists() else {}
values.setdefault('POSTGRES_PASSWORD', secrets.token_hex(32))
values.setdefault('JWT_KEY', secrets.token_hex(48))
values.update(API_PORT='8050', PUBLIC_API_URL=url, RIDEMATE_REVISION=sys.argv[3])
path.write_text(''.join(k+'='+v+'\n' for k,v in values.items()))
os.chmod(path, 0o600)
PY
compose=(docker compose --project-name ridemate-demo --env-file "$runtime/.env" -f "$release/deploy/compose.demo.yml")
"${compose[@]}" config --quiet
if [[ -n "$existing_api" ]]; then
  "${compose[@]}" exec -T postgres pg_dump -U ridemate_demo -d ridemate_demo -Fc > "$runtime/backups/before-$revision.dump"
fi
export DOCKER_BUILDKIT=1
"${compose[@]}" build api
"${compose[@]}" up -d --wait --wait-timeout 120
python3 - <<'PY'
import json, time, urllib.request
base='http://127.0.0.1:8050'
for attempt in range(60):
    try:
        with urllib.request.urlopen(base+'/health',timeout=3) as r:
            if r.status==200: break
    except Exception:
        if attempt==59: raise SystemExit('RideMate API did not become healthy')
        time.sleep(2)
def request(path, data=None, token=None):
    headers={'Content-Type':'application/json'}
    if token: headers['Authorization']='Bearer '+token
    req=urllib.request.Request(base+path, data=json.dumps(data).encode() if data is not None else None, headers=headers)
    with urllib.request.urlopen(req,timeout=15) as response: return json.load(response)
session=request('/api/auth/login',dict(email='demo01@ridemate.test', password='RideMate123!'))
token=session['token']
profile=request('/api/profiles/me',token=token)
assert profile['nickname']=='Marek' and profile['age']==24
discovery=request('/api/discovery?maxDistanceKm=50',token=token)
conversations=request('/api/chat/conversations',token=token)
assert conversations, 'Demo conversation missing'
with urllib.request.urlopen(base+'/demo/01.jpg',timeout=10) as response:
    assert response.status==200 and response.headers.get_content_type()=='image/jpeg'
print(json.dumps(dict(health='healthy',demo_login='ok',discovery_profiles=len(discovery),conversations=len(conversations),local_photo='ok')))
PY
count=$("${compose[@]}" exec -T postgres psql -U ridemate_demo -d ridemate_demo -Atc 'SELECT count(*) FROM "Users" WHERE "Email" LIKE '\''demo%@ridemate.test'\'';')
[[ "$count" = 20 ]] || { echo 'Unexpected demo account count'; exit 1; }
printf '%s\n' "$revision" > "$runtime/current-revision"
ln -sfn "$release" "$runtime/current"
"${compose[@]}" ps
echo "RideMate deployed at revision $revision; 20 demo accounts. PostgreSQL is private and persistent."

---
name: sample-run
description: Run SampleWebApp against Maskinporten's test environment and a local echo server, to show that a change gets, attaches and exchanges real tokens. Use for changes to token requests, caching, Altinn token exchange, system user requests or client definitions, and for pull request evidence of them.
---

# Running SampleWebApp against Maskinporten

SampleWebApp (`samples/SampleWebApp`) registers two clients from configuration:

- `MaskinportenSettingsForSomeExternalApi`, the plain Maskinporten flow, behind `GET /MaskinportenTest`.
- `MaskinportenSettingsForSystemUserExample`, a system user token exchanged for an Altinn token, behind
  `GET /MaskinportenTest/systemuser?customerOrgNo=<orgno>`.

Each sends a request to `SampleWebApp:TargetUrl` with the token it got. Point that at a local echo server, so the
token never leaves this computer except to Maskinporten and Altinn.

## 1. Check the secrets

```sh
cd /home/agent/code/altinn-apiclient-maskinporten
dotnet user-secrets list --project samples/SampleWebApp
```

The plain flow needs `ClientId`, `Scope` and `EncodedJwk` under `MaskinportenSettingsForSomeExternalApi:`, and the
system user flow those and `Kid` under `MaskinportenSettingsForSystemUserExample:`. `Environment` is `test` in
`appsettings.json`. The secrets live in `~/.microsoft/usersecrets`, outside the checkout, and survive restarts.

If they are missing, stop here and report that the live run was skipped because no Maskinporten test client is set
up. Never ask for a key in a session transcript. A person sets them over `agentctl ssh`:

```sh
dotnet user-secrets set --project samples/SampleWebApp MaskinportenSettingsForSomeExternalApi:ClientId <client id>
dotnet user-secrets set --project samples/SampleWebApp MaskinportenSettingsForSomeExternalApi:Scope <scope>
dotnet user-secrets set --project samples/SampleWebApp MaskinportenSettingsForSomeExternalApi:EncodedJwk <base64 JWK>
```

## 2. Start the echo server

```sh
python3 - <<'PY' &
import http.server, json
class Echo(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        body = json.dumps({"path": self.path, "headers": dict(self.headers)}).encode()
        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        self.end_headers()
        self.wfile.write(body)
    def log_message(self, *args):
        pass
http.server.ThreadingHTTPServer(("127.0.0.1", 5080), Echo).serve_forever()
PY
echo $! >/tmp/echo.pid
```

## 3. Start SampleWebApp

Run it on plain HTTP, so it needs no development certificate. The `FileTokenCacheProvider` it registers keeps tokens
in `$TMPDIR/.maskinportenTokenCache.json` across runs; delete that file to force a new token.

```sh
rm -f "${TMPDIR:-/tmp}/.maskinportenTokenCache.json"
ASPNETCORE_ENVIRONMENT=Development SampleWebApp__TargetUrl=http://127.0.0.1:5080/echo \
    dotnet run --project samples/SampleWebApp --urls http://127.0.0.1:5000 >/tmp/samplewebapp.log 2>&1 &
echo $! >/tmp/samplewebapp.pid
timeout 180 sh -c 'until grep -q "Now listening" /tmp/samplewebapp.log; do sleep 1; done'
```

Each shell command may run in a new shell, so the servers are stopped by the process IDs written above.

## 4. Call it and read the token

```sh
curl -s http://127.0.0.1:5000/MaskinportenTest >/tmp/sample-response.json
# A failed token request returns the developer exception page instead of JSON; its first lines say why.
jq -e . /tmp/sample-response.json >/dev/null 2>&1 || { head -3 /tmp/sample-response.json; exit 1; }
jq '{DownstreamStatusCode, Example}' /tmp/sample-response.json
# The claims of the token the echo server received, without the signature.
jq '.DownstreamBody | fromjson | .headers.Authorization | ltrimstr("Bearer ") | split(".")[1]
    | gsub("-"; "+") | gsub("_"; "/") | @base64d | fromjson | {iss, scope, client_id, consumer, exp}' \
    /tmp/sample-response.json
```

A `DownstreamStatusCode` of 200 and claims with `iss` `https://test.maskinporten.no/` and the configured scope show
the flow worked. For the system user flow, call `/MaskinportenTest/systemuser?customerOrgNo=<orgno>` instead; an
exchanged token's `iss` is Altinn's (`platform.tt02.altinn.no`). Errors from Maskinporten are in
`/tmp/samplewebapp.log`. `Invalid assertion. Client authentication failed` means Maskinporten does not know the key:
the JWK, its `kid` or the client ID do not match the client's registration, which only its owner can fix.

## 5. Report and clean up

Put the commands and the claims, never the token itself, in the pull request. Then stop both servers:

```sh
kill "$(cat /tmp/echo.pid)" 2>/dev/null
pkill -P "$(cat /tmp/samplewebapp.pid)" 2>/dev/null; kill "$(cat /tmp/samplewebapp.pid)" 2>/dev/null
rm -f /tmp/echo.pid /tmp/samplewebapp.pid
```

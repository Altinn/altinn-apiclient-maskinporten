# Agents

An agent for developing this library, run with `agentctl` from
[digdir/digdir-agents](https://github.com/digdir/digdir-agents). Its README says how to install `agentctl` and how it
works.

`maskinporten-dev`, in [`minimal/`](minimal), has the .NET SDK CI uses. It builds, tests and packs the library as
[CI](../.github/workflows/ci-nuget.yml) does, runs SampleWebApp against Maskinporten's test environment, and opens
pull requests. It has no browser or desktop, which the library does not need.

| Variant        | Image and checkout                                                                  |
| -------------- | ----------------------------------------------------------------------------------- |
| default        | The published image and a fresh clone                                               |
| `worktree`     | The published image with this checkout mounted read-write                           |
| `nested`       | The published image, smaller, to run inside another agent                           |
| `nested-build` | `nested` with the image built from this checkout, to try a change to the Dockerfile |

[`.github/workflows/agents-images.yaml`](../.github/workflows/agents-images.yaml) builds the image from
[`Dockerfile`](Dockerfile) on amd64 and arm64, runs [`smoke.sh`](smoke.sh) in it, and publishes
`ghcr.io/altinn/altinn-apiclient-maskinporten/agent-minimal:latest` from `main`.

## Setting it up

1. Install `agentctl` and run `agentctl claude login`.
2. Run `agentctl setup` from this checkout, choose `maskinporten-dev`, and give it the values
   [`minimal/.env.sample`](minimal/.env.sample) names. `GITHUB_TOKEN` stays on your machine: the agent sees a
   placeholder, which the host replaces only in requests to GitHub. The `worktree` variant mounts this checkout, and
   `agentctl` refuses a checkout holding a `.env`, so keep that variant's environment file outside it.

## A Maskinporten test client

To let the agent run SampleWebApp against Maskinporten, give it a client in Maskinporten's **test** environment. Never
give it a production key. The library signs with the key in its own process, so the key cannot stay on the host as
the GitHub token does: it is stored in the agent, readable by everything that runs there. Set it over SSH, so it never
passes through a session:

```sh
agentctl ssh maskinporten-dev
cd ~/code/altinn-apiclient-maskinporten
dotnet user-secrets set --project samples/SampleWebApp MaskinportenSettingsForSomeExternalApi:ClientId <client id>
dotnet user-secrets set --project samples/SampleWebApp MaskinportenSettingsForSomeExternalApi:Scope <scope>
dotnet user-secrets set --project samples/SampleWebApp MaskinportenSettingsForSomeExternalApi:EncodedJwk <base64 JWK>
```

For the system user example, set the same under `MaskinportenSettingsForSystemUserExample:`, and its `Kid`.
[`samples/SampleWebApp/README.md`](../samples/SampleWebApp/README.md) describes the settings. Without them, the agent
runs every other check and says it skipped the live run.

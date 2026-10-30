#!/usr/bin/env bash
# Smoke-tests what the maskinporten-dev image adds to the agentctl base image, inside a freshly built image. The base
# image and its features are tested where they are built, in digdir/digdir-agents.
#
#   docker run --rm --entrypoint bash -v "$PWD/agents/smoke.sh:/smoke.sh:ro" <image> /smoke.sh
#
# Runs as the image's `agent` user.
set -euo pipefail

work="$(mktemp -d)"
cd "$work"
export HOME="$work"
fail() { echo "smoke: $*" >&2; exit 1; }

echo "## versions"
test "$(id -un)" = agent || fail "expected to run as agent, got $(id -un)"
dotnet --list-sdks
dotnet --list-sdks | grep -q '^10\.' || fail ".NET SDK 10 is missing"
dotnet --list-runtimes | grep -q '^Microsoft.AspNetCore.App 10\.' || fail "the ASP.NET Core 10 runtime is missing"
agentctl --version
agentd --version
podman --version
grep -qxF Altinn/altinn-apiclient-maskinporten /etc/agent-workspace.d/maskinporten.list \
    || fail "the workspace list does not clone the repository"

echo "## dotnet as the agent user"
# A root-owned cache in the agent's home, or an SDK that needs one, breaks the first build.
dotnet new classlib --output lib --framework net10.0 >/dev/null
dotnet build lib --nologo --verbosity quiet \
    || fail "dotnet could not build a class library"

echo "smoke: ok"

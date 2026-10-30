# The image of the maskinporten-dev agent, built on the agentctl base image. The base carries the agent user, the
# harnesses at the versions agentctl is tested with, Git, gh, Node.js, Python and the common build tools:
# https://github.com/digdir/digdir-agents/tree/main/images/agentctl-base
# Renovate moves the digest, and with it the harness versions.
FROM ghcr.io/digdir/agentctl-base:ubuntu-26.04@sha256:18cf52e80b6ccebb58fac85af78fea8964e854a310091b4a11ce827283055efb AS minimal

# The base ends as the unprivileged agent user; installing needs root again.
USER root

ENV DOTNET_ROOT=/usr/local/share/dotnet
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=1

# The ICU library .NET's globalization needs, which the base does not have and dotnet-install.sh does not bring.
# Ubuntu 26.04 ships ICU 78; a base on another release names another package.
RUN apt-get update \
    && apt-get install --yes --no-install-recommends libicu78 \
    && rm -rf /var/lib/apt/lists/*

ARG TARGETARCH
# CI installs the newest .NET 10 SDK (`dotnet-version: '10.0.x'` in .github/workflows/ci-nuget.yml), and global.json
# pins no SDK, so this is the newest 10.0 SDK when the image was last updated. SDK 10 builds every target framework
# the library has (net10.0, net9.0, net8.0); the tests and SampleWebApp run on net10.0 only.
ARG DOTNET_SDK_VERSION=10.0.403
ARG DOTNET_INSTALL_SCRIPT_COMMIT=6f559c420847ded38591392dafe785ad511f39f5
ARG DOTNET_INSTALL_SCRIPT_SHA256=082f7685e156738a1b2e2ed8381a621870d4ce8e8c59278034556f05c186eb2e

# dotnet-install.sh checks the SDK archive against the checksum Microsoft publishes for it.
RUN case "${TARGETARCH}" in \
        amd64) DOTNET_ARCH=x64 ;; \
        arm64) DOTNET_ARCH=arm64 ;; \
        *) echo "unsupported target architecture: ${TARGETARCH}" >&2; exit 1 ;; \
    esac \
    && curl -fsSL \
        "https://raw.githubusercontent.com/dotnet/install-scripts/${DOTNET_INSTALL_SCRIPT_COMMIT}/src/dotnet-install.sh" \
        -o /tmp/dotnet-install.sh \
    && echo "${DOTNET_INSTALL_SCRIPT_SHA256}  /tmp/dotnet-install.sh" | sha256sum -c - \
    && bash /tmp/dotnet-install.sh \
        --architecture "${DOTNET_ARCH}" \
        --install-dir "${DOTNET_ROOT}" \
        --no-path \
        --version "${DOTNET_SDK_VERSION}" \
    && ln -s "${DOTNET_ROOT}/dotnet" /usr/local/bin/dotnet \
    && install -d /etc/dotnet \
    && printf '%s\n' "${DOTNET_ROOT}" >/etc/dotnet/install_location \
    && rm -f /tmp/dotnet-install.sh

# Podman, the `docker` command and Docker Compose, which the nested-build variant builds this image with.
RUN agent-feature install containers

# agentctl and agentd run nested agents, so the agent can try a change to its own definition before it proposes it.
# Renovate moves it to new agentctl releases.
COPY --from=ghcr.io/digdir/agentctl:v0.1.0-preview.13@sha256:5451dd5e912865d7ed4544f0bf2e6d0912cd274b878515224cd05896a286b28f \
    /agentctl /agentd /usr/local/bin/

# The base image's workspace unit clones the repositories this list names at boot.
COPY common/workspace.list /etc/agent-workspace.d/maskinporten.list

# The version checks run with a scratch HOME, since the SDK writes caches there and a root-owned one would lock the
# agent out of its own.
RUN scratch="$(mktemp -d)" \
    && HOME="${scratch}" TMPDIR="${scratch}" XDG_RUNTIME_DIR="${scratch}" sh -c ' \
        dotnet --list-sdks | grep -q "^${DOTNET_SDK_VERSION} " \
        && dotnet --info >/dev/null \
        && agentctl --version \
        && agentd --version \
        && podman --version' \
    && rm -rf "${scratch}" \
    && agent-image-check

USER agent

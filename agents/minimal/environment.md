## This computer

- The .NET 10 SDK, which builds every target framework of the library. Only the .NET 10 runtime is installed, so
  tests and apps run on net10.0, as in CI. Running the tests on net8.0 or net9.0 needs those runtimes installed first.
- No browser or desktop. Evidence for a pull request is terminal output.
- Podman and `agentctl`, for trying a change to this agent's own definition in a nested agent.
- All traffic leaves through the host, which intercepts TLS with a CA the system store trusts, so .NET trusts it
  too. NuGet.org, test.maskinporten.no and platform.tt02.altinn.no are reachable.
- Maskinporten signs grants with the client's private key in the process, so the key has to be inside this computer:
  it is in SampleWebApp's `dotnet user-secrets` when someone has set it up, and nowhere else.

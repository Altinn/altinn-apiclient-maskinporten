# Maskinporten client developer

You develop `Altinn.ApiClients.Maskinporten`, the .NET library that gets Maskinporten tokens for `HttpClient`s and
exchanges them for Altinn tokens. Its repository is cloned at boot into
`/home/agent/code/altinn-apiclient-maskinporten`. If it is missing, run
`gh repo clone Altinn/altinn-apiclient-maskinporten /home/agent/code/altinn-apiclient-maskinporten`. Never repair a
checkout by deleting it or by a destructive reset.

The repository's `README.md` and `samples/SampleWebApp/README.md` describe the library and its sample, and
`.github/workflows/ci-nuget.yml` is the record of the checks a change must pass. Read them rather than relying on this
file for how to build.

## A task is done when

1. The solution builds, tests and packs as CI does, from the repository root:

   ```sh
   dotnet build --configuration Release src/Altinn.ApiClients.Maskinporten/Altinn.ApiClients.Maskinporten.sln
   dotnet test --configuration Release --no-build src/Altinn.ApiClients.Maskinporten/Altinn.ApiClients.Maskinporten.sln
   dotnet pack --configuration Release --output /tmp/nupkg src/Altinn.ApiClients.Maskinporten/Altinn.ApiClients.Maskinporten.sln
   ```

   The library targets net10.0, net9.0 and net8.0, so a build warning or error on any of them counts.
2. A change to how tokens are requested, cached or exchanged, or to the client definitions, is run for real against
   Maskinporten's test environment as the `sample-run` skill describes. When the sample's user secrets are not set,
   say that the live run was skipped and why, rather than claim it.
3. New behaviour has tests in `test/Altinn.ApiClients.Maskinporten.Tests`.
4. The pull request says what you ran and shows its results: the test summary, and the sample-run output when there
   was one.

## Git and pull requests

- Work on a branch from the current `origin/main`. Never push to `main` or merge pull requests.
- Push the branch before `gh pr create`, and pass `--base main`, `--head`, `--title` and `--body-file`.
- Pass multiline GitHub comments through `--body-file` or stdin, not `\n` in a quoted string.

## Keys and tokens

Maskinporten keys and the tokens they get are credentials, even in the test environment. Never commit them, print
them in full, or put them in a pull request, comment or log you share. Show a token by its decoded claims, without its
signature, as the `sample-run` skill does.

## Your own definition

This agent is made from the definition in `agents/` of the altinn-apiclient-maskinporten repository: `Dockerfile` and
`smoke.sh` for the image, `minimal/agent.yaml` and its variants for the agent, `instructions.md`, `improving.md` and
`minimal/environment.md` for what you are told, and `_skills/` for your skills. `agents/README.md` says how people
set it up.

When a task shows that the definition is wrong or missing something, such as a tool, a host or an instruction,
propose the change as a pull request, with the `agent-definition` skill's procedure if you have it: change the files,
build the image with `podman build --target minimal agents`, run `agents/smoke.sh` in it, and try it in a nested agent
with the `nested-build` variant, as the `agentctl` skill describes. Say in the pull request what you verified, and
say so when the change widens what the agent may reach, such as hosts or secrets.

Two traps in trying it, as of agentctl v0.1.0-preview.13:

- `nested-build` fails with `lseek(SEEK_HOLE) returned ENXIO` when `agents/` is on a virtiofs mount, such as the
  checkout the `worktree` variant mounts. Copy `agents/` to local disk, such as `~/trial/`, and apply from there.
- A nested agent's disk can be 50 to 100 times slower than this one's on some hosts, so a .NET restore and build there
  can take tens of minutes. Run the build and tests in a container of the image first (`podman run`), and keep the
  nested agent for what only it can show, such as that the agent boots and its instructions and skills install.

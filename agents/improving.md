## Your own definition

This agent is made from the definition in `agents/` of the altinn-apiclient-maskinporten repository: `Dockerfile` and
`smoke.sh` for the image, `minimal/agent.yaml` and its variants for the agent, `instructions.md`, `improving.md` and
`minimal/environment.md` for what you are told, and `_skills/` for your skills. `agents/README.md` says how people
set it up.

When a task shows that the definition is wrong or missing something, such as a tool, a host or an instruction,
propose the change as a pull request, with the `agent-definition` skill's procedure if you have it: change the files,
build the image with `podman build --target minimal agents`, run `agents/smoke.sh` in it, and run the build and tests
in a container of the image (`podman run`). Say in the pull request what you verified, and say so when the change
widens what the agent may reach, such as hosts or secrets.

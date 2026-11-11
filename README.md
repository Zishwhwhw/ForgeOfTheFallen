# FrankenArms

Fresh Unity project for the game described in [`Docs/FrankenArms_GDD_RU.html`](Docs/FrankenArms_GDD_RU.html).

- Unity Editor: `6000.6.7f1`
- Template: Universal 2D (URP 2D Renderer)
- First target: Windows PC
- Design and technical decisions: `Docs/`

The previous game was not copied into this project. The GDD is the design source of truth.

## Unity CLI

Unity CLI is installed on this computer and connected to the open Editor. Open this project with:

```powershell
unity open "D:\game\2dRoguelike\FrankenArmsFresh"
```

The Unity Pipeline package is installed. Check the live Editor connection with:

```powershell
unity status --project-path "D:\game\2dRoguelike\FrankenArmsFresh"
```

The current Codex environment still has a placeholder network proxy at `127.0.0.1:9`, which may block package and CLI updates. See `Docs/GITHUB_SETUP.md` for troubleshooting.

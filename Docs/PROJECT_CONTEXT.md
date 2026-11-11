# FrankenArms project context

This is a new local Unity project started from the FrankenArms GDD supplied in `Unity-AI-Game-Workspace.zip`. The owner explicitly chose to start over rather than reuse the previous game's code and assets.

## Project setup

- Project: `FrankenArms`
- Unity Editor: `6000.6.7f1`
- Render pipeline: Universal 2D / URP 2D Renderer
- Target: Windows PC, keyboard/mouse and gamepad
- Main design document: `Docs/FrankenArms_GDD_RU.html`
- Repository URL: `https://github.com/Zishwhwhw/FrankenArms.git`

## Current environment

- Unity CLI `1.0.0-beta.13` and Unity Editor `6000.6.7f1` are installed.
- `com.unity.pipeline` `1.0.0-pre.1` is in the manifest, lockfile, and package cache.
- Unity CLI reports the open Editor as `ready` for this project.
- The CLI reports `1.0.0-beta.17` as available; the update has not been run.
- The current session has a placeholder HTTP(S) proxy at `127.0.0.1:9`, so registry and CLI update requests may fail even though the installed package and live Editor connection work.

## Product principles

1. Follow the new GDD; old project files are intentionally excluded.
2. Prototype a narrow vertical slice before implementing the full 24-floor game.
3. Preserve readable combat, meaningful weapon fusion, and progression that survives a failed run.

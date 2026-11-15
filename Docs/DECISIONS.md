# Decision log

| Date | Decision | Reason | Status |
| --- | --- | --- | --- |
| 2026-11-11 | Start a new game project from the supplied FrankenArms GDD; leave the previous game's code and assets behind | The owner chose a clean restart | Active |
| 2026-11-11 | Use Unity Input System | Keyboard/mouse, gamepad, and rebinding support | Active |
| 2026-11-11 | Use ScriptableObjects for content definitions | Keeps weapons, heroes, enemies, rooms, and biomes data-driven | Active |
| 2026-11-11 | Generate controlled room graphs and place reviewed room templates | Keeps routes playable and supports authored combat spaces | Active |
| 2026-11-11 | Keep the existing Unity CLI install and use it from the agent terminal | The agent can launch/build the project; live Editor control will be enabled after Pipeline installation | Active |
| 2026-11-11 | Create a new hosted Git repository after local setup | The current environment cannot create or push a GitHub repository | Pending owner action |
| 2026-11-15 | Integrate only the parent project's modular weapon data and a compatibility bridge; keep the Fresh combat controller, scenes, route, and runtime bootstrap authoritative | Both projects contain same-named controllers; selective integration avoids duplicate classes and preserves the verified vertical slice | Active |
| 2026-11-15 | Import the parent project's authored module definitions under `Assets/Resources/ImportedModules`; do not import third-party art packs yet | The module data directly supports the FrankenArms USP, while art-pack licenses and texture dependencies need a separate review | Active |
| 2026-11-15 | Reuse six locally available SFX from the sibling Brackeys 2D Mega Pack under `Assets/Resources/Audio`; keep audio optional through `Resources.Load` | The GDD requires readable shot, dash, reward, forge, and ability feedback; no external generator or package is needed for this slice | Active |
| 2026-11-15 | Reuse the locally available `KitchenTheme.wav` as a looping Lobby ambient track; keep Run silent until a dedicated biome theme is selected | The GDD calls for a warm manor loop, and this avoids adding an external music package or unreviewed download | Active |

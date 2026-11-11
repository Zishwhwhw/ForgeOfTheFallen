# FrankenArms local playtest

The project bootstraps its playable slice at runtime, so no scene objects need to be wired manually in the Inspector.

## Start

1. Open `FrankenArmsFresh` in Unity `6000.6.7f1`.
2. Open the existing `SampleScene` and press Play.
3. The arena, player, enemies, walls, doors, reward, and fusion station are created automatically.

## Controls

- `WASD` or arrow keys: move.
- Left mouse button or `Space`: fire.
- `F`: toggle the fusion station.
- `1`, `2`, `3`: choose a fusion result while the station is open.
- `N`: start the next room after a clear.
- `R`: restart after defeat.

## Acceptance checklist

- Shoot enemies and confirm muzzle flash, impact burst, hit flash, and a small camera shake.
- Clear the wave; confirm the doors open and a `Room Reward` coin appears.
- Collect the coin; confirm the HUD credit total increases by 10.
- Use the right-side exit or `N`; confirm the next room has a different enemy count/layout.
- Touch an enemy; confirm HP decreases and temporary invulnerability prevents instant repeated damage.
- Let HP reach zero; confirm the defeat overlay and press `R` to restart.
- Quit and replay the scene; confirm the saved credit total is restored from `PlayerPrefs`.

## Automated checks used for this slice

- Unity CLI status: Editor `ready`, Unity `6000.6.7f1`.
- Unity CLI recompile: succeeded with no compilation errors or warnings.
- Play Mode hierarchy: camera, player, walls, combat room, fusion station, doors, exit trigger, and spawned enemies present.
- Console ground truth: `compilationFailed=false`, `consoleErrors=0`, `consoleWarnings=0`.

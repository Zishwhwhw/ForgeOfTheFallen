# FrankenArms local playtest

The project bootstraps its playable slice at runtime, so no scene objects need to be wired manually in the Inspector.

## Start

1. Open `FrankenArmsFresh` in Unity `6000.6.7f1`.
2. Press Play from `MainMenu.unity`.
3. Enter `Lobby / Manor`, then press `START NEW RUN`.
4. In `Run.unity`, choose Knight, Mage, or Mechanic with `1/2/3` (or click a card); the arena starts after selection.
5. The arena, player, enemies, walls, doors, reward, and fusion station are created automatically in the Run scene.

## Controls

- `WASD` or arrow keys: move.
- Left mouse button or `Space`: fire.
- `Shift`: directional Dash with short i-frames.
- `1`, `2`, `3` at run start: choose Knight, Mage, or Mechanic.
- `F`: open the fusion station only while standing near the forge.
- `1`, `2`, `3`: choose a fusion result while the station is open.
- After a clear: `1` safe route, `2` risk route, `3` forge route.
- `R`: restart after defeat.

## Acceptance checklist

- Shoot enemies and confirm muzzle flash, impact burst, hit flash, and a small camera shake.
- Clear the wave; confirm the doors open and a `Room Reward` coin appears.
- Collect the coin; confirm the HUD credit total increases by 10.
- Use the right-side exit or `N`; confirm the next room has a different enemy count/layout.
- Touch an enemy; confirm HP decreases and temporary invulnerability prevents instant repeated damage.
- Take a hit with armor; confirm armor breaks without HP overflow and returns after 3 seconds without damage.
- Enter Crossfire; confirm ranged bolts create pressure that can be answered by movement or Dash.
- Reach room 6; confirm the Boss Gate spawns one large boss with a readable radial burst.
- Choose the forge route; confirm the next room has no combat wave and the forge remains available as a safe decision space.
- Walk near the pet dog; confirm it follows and damages nearby threats without a player HP bar of its own.
- Let HP reach zero; confirm the defeat overlay and press `R` to restart.
- Quit and replay the scene; confirm the saved credit total is restored from `PlayerPrefs`.

## Automated checks used for this slice

- Unity CLI status: Editor `ready`, Unity `6000.6.7f1`.
- Unity CLI recompile: succeeded with no compilation errors or warnings.
- Play Mode hierarchy: camera, player, walls, combat room, fusion station, doors, exit trigger, and spawned enemies present.
- Console ground truth: `compilationFailed=false`, `consoleErrors=0`, `consoleWarnings=0`.

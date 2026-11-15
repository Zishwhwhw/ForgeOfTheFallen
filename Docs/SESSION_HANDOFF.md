# Session handoff

## Completed

- Created a clean Universal 2D project shell from the cached Unity template.
- Set the project version to Unity `6000.6.7f1` and product name to `FrankenArms`.
- Copied the supplied GDD and its three concept images into `Docs/`.
- Initialized local Git and connected `origin` to `https://github.com/Zishwhwhw/FrankenArms.git`.
- Local `main` commit `3bc9b97` matches `origin/main`.
- Unity CLI reports the Editor as `ready`; `com.unity.pipeline` `1.0.0-pre.1` is installed.
- The active `SampleScene` contains the template Main Camera and Global Light 2D. Unity reports no compile failure, errors, or warnings.
- Added the first runtime vertical-slice bootstrap: a generated test floor, orthographic camera setup, and a player with Rigidbody2D movement using WASD/arrow keys.
- Verified in Play Mode through Unity CLI: `/Test Room Floor` and `/Player` appear in the active scene hierarchy, the player loop is ticking, and current Console ground truth reports zero errors and warnings.
- Added one red enemy target, projectile firing with left mouse/Space, hit-point removal, and a first fusion demonstration on `F`: `Ember Pistol + Arc Grip -> Ember Arc`.
- Recompiled and verified the live hierarchy: `Player` has `PlayerCombat` and `WeaponFusionDemo`; `Enemy Target` has `EnemyTarget`; current Console ground truth remains at zero errors and warnings.
- Replaced the string-only fusion demo with runtime `WeaponPartDefinition` ScriptableObjects. Fusion now combines grip/barrel stats and applies the resulting damage, fire rate, projectile speed, and tint to the player weapon.
- Recompiled and restarted Play Mode after the change; the same hierarchy loads with zero current Console errors or warnings.
- Added a runtime `FusionStation` with three selectable outcomes: Ember Arc, Rapid Ember, and Heavy Arc. Options are available through the on-screen panel, mouse buttons, or keys `1`, `2`, and `3`; `F` toggles the panel.
- Verified the live hierarchy includes `/Fusion Station`; Play Mode starts cleanly with zero current Console errors and warnings.
- Added `CombatRoom`: a single runtime wave spawns three targets, tracks remaining enemies, and creates a `Room Reward` pickup after the wave is cleared.
- Verified the live hierarchy contains `/Combat Room` and three `/Enemy Target` instances; the current Console reports zero errors and warnings.
- Added player HP, contact damage, invulnerability frames, and a defeat overlay. Press `R` after defeat to reload the scene and start a fresh run.
- Verified the live hierarchy: `Player` has `PlayerHealth`, spawned enemies have `EnemyContactDamage`, projectiles are active, and current Console ground truth remains at zero errors and warnings.
- Added locked room doors that block the left and right exits until the wave is cleared; doors turn green and remove their colliders after the reward phase begins.
- Added persistent credits through `PlayerPrefs`; collecting the room reward adds 10 credits and the HUD displays the saved balance.
- Verified the live hierarchy contains two `/Room Door` objects with `BoxCollider2D`, three enemies with contact damage, and zero current Console errors or warnings.
- Added a repeatable room loop and result panel: after a clear, the player sees the completed room and credits, then presses `N` to lock the doors and start the next room wave.
- Verified the new runtime code compiles and starts Play Mode cleanly; current Console ground truth remains at zero errors and warnings.
- Added room variation: room numbers cycle through three enemy layouts with 3, 4, and 5 targets.
- Added a physical right-side `RoomExit` trigger. Once its door opens, walking into it starts the next room; `N` remains as a keyboard shortcut.
- Verified the active hierarchy includes the exit trigger and current Console ground truth remains at zero errors and warnings.
- Replaced the flat square visuals with procedural point-filtered pixel sprites for the player, enemies, projectiles, coins, and doors. The sprites use transparent silhouettes and are generated at runtime, so no Inspector wiring is required.
- Unity Editor remains `ready`; the one CLI recompile request returned HTTP 400, but Console ground truth shows compilation is not failed and has zero current errors or warnings. Play Mode starts successfully with the new sprites.
- Added two-frame point-filtered idle animation for the player and enemies through `PixelAnimator`.
- Verified the live hierarchy includes `PixelAnimator` on the player and all spawned enemies; Play Mode runs with zero current Console errors and warnings.
- Added the first room art pass: tiled pixel floor texture, four pixel wall strips, and colliders that keep the player inside the arena.
- Verified the live hierarchy contains North/South/West/East walls with `SpriteRenderer` and `BoxCollider2D`; Play Mode runs with zero current Console errors and warnings.
- Added combat feedback: two-frame idle animation, muzzle flashes, impact bursts, and camera shake on hits.
- Verified `ScreenShake` is attached to the camera, enemies have `PixelAnimator`, and the current Console ground truth remains at zero errors and warnings.
- Added a repeatable local playtest checklist in `Docs/PLAYTEST.md` covering combat, fusion, room progression, rewards, defeat/restart, and credit persistence.
- Final vertical-slice verification: Unity `6000.6.7f1` Editor is `ready` in Play Mode, the loop is ticking, compilation is not failed, and Console ground truth reports zero errors and zero warnings.
- Added a second combat archetype, `EnemyChaser`, with a kinematic Rigidbody2D that pursues the player and uses orange tinting for readability. Room variation 2 spawns one chaser alongside the stationary targets; other layouts remain unchanged.
- Verified the edited project imports in the live Unity Editor, Play Mode is ticking (`playerLoopTicking: true`), a Game View snapshot was captured at `Screenshots/chaser-room1.png`, and the cleared Console ground truth reports zero errors, zero warnings, and no compile failure. The snapshot is ignored by Git.
- Expanded the slice into a controlled five-room mini-run: Warmup, Pursuit, Crossfire, Gauntlet, and Elite Gate. Room 5 uses two tougher chasers, room labels and threat counts are exposed to the HUD, and `RunProgress` persists the best room reached.
- Added `RunHUD` with combat HP, room, threat, credits, weapon DPS, best room, and controls. It uses a conservative bottom-left layout so it does not overlap the fusion panel; the room balance is documented in `Docs/COMBAT_BALANCE.md`.
- Forced an AssetDatabase refresh after external script edits and verified the live Editor imports `RunHUD` and `EnemyChaser`. Play Mode is ticking and the final Game View snapshot is `Screenshots/mini-run-final-clean.png`; cleared Console ground truth reports zero errors, zero warnings, and no compile failure.
- Added the first missing player combat verb, `PlayerDash`: `Left Shift` launches a directional 0.16 s dodge at 11 units/s, starts a 1.05 s cooldown, and grants 0.20 s contact-damage invulnerability. The HUD exposes `DASH READY`/remaining cooldown and the controls line was updated.
- Verified after the mechanic change in live Unity `6000.6.7f1`: Play Mode starts, the Game View snapshot is `Screenshots/mechanics-dash-pass.png`, and Console ground truth reports zero errors, zero warnings, and no compile failure. Visual generation is intentionally paused until the next mechanics milestone is complete.
- Added the GDD resource rule for weapons: the base weapon is free, while a fused/found weapon spends 1 mana per shot. The player starts with 100 mana; room rewards restore 20 mana as well as 10 credits. HUD now shows mana and whether the current weapon is base or fused.
- Added the ranged enemy archetype: Crossfire room now contains two timed shooters plus one chaser. Enemy bolts use kinematic 2D trigger bodies, deal 1 damage, expire after 2.5 s, and are spaced for movement/Dash counterplay. The encounter timing is recorded in `Docs/COMBAT_BALANCE.md`.
- Added the first boss gate: room 6 now spawns a large 18 HP boss with slow pursuit and a readable three-bolt radial burst every 1.8 s. This gives the five-room mini-run a concrete endpoint instead of looping forever.
- Converted the forge from a permanently open demo panel into a real interaction: the player must move within 2.35 units and press `F`; the automatic `WeaponFusionDemo` shortcut was removed, leaving the three ScriptableObject-backed fusion candidates in `FusionStation`.
- Replaced linear room progression with controlled route choices after a clear: `1` Safe, `2` Risk, and `3` Forge. Safe keeps the normal authored wave, Risk adds ranged pressure, and Forge opens a non-combat decision room. The route behavior is deterministic and documented in `Docs/COMBAT_BALANCE.md`.
- Created authored `WeaponFusionRecipe` assets (`EmberArc`, `RapidEmber`, `HeavyArc`) under `Assets/Resources/Content/Fusions`; the live forge now reads these assets and the clean start shows the forge interaction prompt instead of the old always-open panel.
- Added GDD armor behavior: the player starts with 2 armor, a hit breaks armor without HP overflow, and the full armor value regenerates after 3 seconds without damage. HUD now reads HP and armor together.
- Added the first GDD pet, `Pet Dog`: it follows the player, cannot die, and bites the nearest enemy within 1.35 units every 1.25 seconds for 1 damage. This is intentionally a small companion layer before adding the remaining pet roster.
- Added shared status runtime: Crossfire bolts apply Poison, boss bolts apply short Freeze, and the dog applies Mark to bitten enemies. Status timing and counterplay are documented in `Docs/COMBAT_BALANCE.md`; HUD exposes the player status when active.
- Added a data-driven hero roster layer: `HeroDefinition` assets now contain combat stats plus Dash, active-skill, and ultimate/passive descriptions. The first three selectable heroes are Knight, Mage, and Mechanic; the roster asset pass also includes Vampire, Witch, Necromancer, Archer, Soldier, Druid, and Elf for the next unlock/meta pass.
- Added `HeroSelectionController`: a run starts paused until the player chooses hero `1/2/3` or clicks a card. The choice applies speed, HP, armor, mana, base damage/rate, and hero tint before room 1 begins. Verified by selecting Mage in live Play Mode: room 1 spawned with 3 threats, HUD showed HP 6/6 and MANA 130/130, and Console had zero errors/warnings.
- Added authored route content: `RoomNodeDefinition` and `DungeonRouteDefinition` ScriptableObjects, with 24 ordered floor nodes under `Assets/Resources/Content/Routes/Nodes` and a `FrankenArmsMainRoute` asset. Nodes cover combat, forge, reward, secret, and boss types, include threat/reward budgets, and keep the route controlled rather than random-walk generated.
- Wired `CombatRoom` to load the authored route asset with a safe fallback to the existing vertical-slice layouts. Verified 25 route assets import in Unity, Play Mode starts at the authored Warmup node, and Console remains at zero errors/warnings.
- Split the prototype into the required three-scene flow: `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/Lobby.unity`, and `Assets/Scenes/Run.unity`. Build Settings now contain those three scenes in order; Main Menu and Lobby have their own UI/controllers, and the runtime slice is created only in Run. `SceneManager.sceneLoaded` now rebuilds the run slice after every Lobby → Run transition.
- Unity verification for the scene flow: MainMenu and Lobby screenshots render their own UI, `Scene=Run mainMenu=False lobby=False bootstrap=True player=True` was observed after transition, and the final Console check reported 0 errors, 0 warnings, `compilationFailed=false`, with a ticking player loop. Final Editor state is MainMenu in Play Mode.
- Audited the sibling `2dRoguelike` project. It is an earlier/larger FrankenArms prototype with overlapping global classes and a richer modular Grip/Barrel data model. Imported only `WeaponGripSO`, `WeaponBarrelSO`, rarity/element definitions, and the nine authored module assets into `Assets/Resources/ImportedModules`; intentionally did not merge its conflicting controllers, scenes, or dungeon generator.
- Added `ImportedModuleForgeBridge`, wired to the existing runtime forge. Near the station, keys `4`, `5`, and `6` now equip combinations from the imported parent module assets through the current `PlayerCombat` adapter.
- Started the GDD active-ability slice: `HeroActiveAbilityController` adds `Q` abilities for Knight (temporary invulnerability plus close strike), Mage (arcane burst), and Mechanic (temporary auto-attacking turret). The ability HUD shows readiness and cooldown.
- Added optional GDD SFX feedback using six local sibling-project clips under `Assets/Resources/Audio`: shots, dash, rewards, forge/ability confirmation. The service is zero-setup and silently falls back when a clip is unavailable.
- Connected authored route node types to `CombatRoom`: Forge, Reward, and Secret nodes now open as non-combat rooms with authored rewards, distinct secret color/position, and route labels; Combat and Boss nodes retain their encounter behavior.
- Added the GDD HUD route pass: `RunMinimapUI` renders the 24-floor authored route with visited/current/unknown states and node-type colors, while `RunHUD` now shows elapsed run time alongside best-room progress.
- Added the first GDD shop interaction: near the forge, `B` spends 5 credits for +2 HP and `N` spends 5 credits for +25 mana. Purchases use persistent run credits and play the reward SFX.
- Added the GDD risk altar slice: near the forge, `V` trades 1 maximum HP for 25 maximum mana once per runtime shrine. The trade is rejected if it would reduce max HP below 1.
- Connected Lobby meta progression to persistent crystal spending: Garden unlock costs 3 crystals and Workshop unlock costs 5 crystals. The unlock buttons remain cosmetic-safe and survive scene reloads through `PlayerPrefs`.
- Connected boss blueprint rewards to imported weapon content: Lobby can spend 1 blueprint to unlock the full module pack; before unlock, the runtime forge exposes only the basic imported pair, after unlock it exposes all imported Grip/Barrel assets.
- Added physical meta-loot pickups: boss clear now spawns Crystal, Blueprint, and Diary pickups at the arena center; collecting them persists the reward and plays the bonus SFX instead of silently incrementing all counters at kill time.
- Added authored module loot: Reward and Secret route nodes now spawn a module cache below their credit pickup. Collecting it equips a Grip + Barrel assembly; Secret caches use a rare damage multiplier and respect the Lobby Module Pack unlock.
- Extended the route beyond the first mini-run boss: authored Boss nodes now spawn boss encounters and meta rewards at their own floors, and the summary continues through floor 24 instead of ending at floor 6.
- Added the first boss phase pass: `EnemyBoss` switches at 50% HP to faster pursuit, a shorter burst interval, a tighter comfort radius, and a five-bolt spread; the phase transition has an explosion SFX and Console-readable event.
- Added authored boss pattern variants by floor: floor 6 uses the readable radial burst, floor 10 uses crossfire, floor 15 a dense fan, floor 20 a wider fan, and floor 24 the largest final fan with red projectile readability.
- Added visible boss telegraphs: every burst now draws directional warning lines for 0.35 seconds before spawning projectiles, and the lines are cleared when the burst fires or the boss is destroyed.
- Added the GDD combat-feel pass: `GameFeel` provides 35 ms hit-stop for high-damage impacts, boss phase 2 adds 50 ms hit-stop and stronger shake, and player damage receives a readable shake pulse.
- Added the GDD pause/settings flow: `Escape` opens an overlay with Continue, Settings, and Surrender to Lobby; settings persist Master/SFX volume and Screen Shake through `PlayerPrefs`.
- Added the first gamepad pass: left stick moves, right stick aims, and the south face button triggers the active hero ability alongside keyboard controls.
- Completed the run result flow: defeat now offers Restart Run or Return to Lobby, while a floor-24 victory offers Return to Lobby beside the persistent reward totals.
- Added ephemeral run statistics: elapsed time, kills, credits collected, meta pickups, and hero abilities used are tracked and shown on cleared/defeat result panels.
- Added floating damage numbers for enemy and player hits, using warm non-blood colors and stronger emphasis for high-damage impacts, matching the GDD feedback language.
- Added a persistent localization foundation for EN/RU/UA/PL/DE/ES. Main Menu cycles the language, and core Run HUD labels use the selected language.
- Completed the gamepad combat mapping: right trigger fires and left shoulder dashes; Run HUD controls now expose the keyboard and controller equivalents.
- Added a scene-aware Lobby ambient loop using local `KitchenTheme.wav`; Run remains silent until a dedicated biome track is selected.
- Added the GDD Hardcore mode toggle in Lobby: enemy HP is doubled and room credit rewards are doubled; Normal remains the default and both choices persist between scene loads.
- Extended the mode selector to Normal/Hardcore/Daily. Daily uses the UTC date as a visible deterministic seed, 1.25x enemy HP, and 2x credit rewards; it has no monetized advantage.
- Daily seed is now applied to Unity's runtime random state at run start, making module loot and future authored random rolls reproducible for that UTC day.
- Daily now exposes two deterministic modifiers from the UTC seed (for example HOT CORE/FAST THREATS or RICH VEINS/ARCANE LOOT); their effects and labels are visible in Lobby and Run HUD.
- Added local Daily records keyed by UTC date: best floor and best kills persist for that day's seed and are shown in Lobby; no online leaderboard/backend was added.
- Added five persistent local achievements: first kill, first fusion, first meta pickup, boss clear, and Daily clear. Lobby shows the unlocked count.
- Added an achievement gallery toggle in Lobby with unlock state and short descriptions for all five achievements.
- Added controlled critical hits to fused/module weapons: 12% chance for 2x damage, with stronger impact feedback; base hero weapons remain deterministic and free of mana.
- Pause can now also be toggled with the gamepad Start/Menu button in addition to Escape.
- Connected local `Hit.wav` to projectile impacts and enemy contact damage, separating hit feedback from shot, dash, reward, and explosion sounds.
- Added a shared 100-projectile runtime budget for player and enemy bolts; Run HUD exposes the active count so boss stress tests have a visible safety metric.
- Added a lightweight Run HUD FPS counter next to the projectile budget for local 60 FPS stress checks on boss/Daily/Hardcore encounters.
- Extended `RoomNodeType` with authored `Shop` and `Shrine` types; they use the existing safe-room handling and receive distinct minimap colors, ready for route asset assignment.
- Assigned authored route assets through Unity AssetDatabase: `Floor12` is now `Shop`, and `Floor17` is now `Shrine`; both changes were saved and refreshed in the connected Editor.
- Gated station interactions by authored node: forge/module controls require Forge or Forge route, shop requires Shop, and shrine requires Shrine; combat rooms no longer expose every station.
- Added `NEXT` authored-node label to the Run HUD so players can see the upcoming route type before choosing the next room.

## Pending review

- The scene remains an authored template; the first room and player are intentionally generated at runtime by code.
- Unity generated uncommitted changes after Pipeline installation and Editor startup: package manifest/lock, URP global settings, PlayerSettings, `.vsconfig`, and Project Auditor settings. Review them before the next commit.
- The CLI is `1.0.0-beta.13`; it reports `1.0.0-beta.17` as available. No CLI update was performed.
- Registry access from this Codex session may still fail because its proxy is `127.0.0.1:9`.

## Next steps

1. Playtest the full route with the new armor, pet, statuses, and boss gate; tune fairness from runtime evidence.
2. Add hero roster definitions and a start-of-run hero selection for the first three GDD heroes.
3. Add authored room-node assets for combat, forge, reward, secret, and boss templates.
5. Playtest rooms 2–5 specifically to confirm chaser pressure and Elite Gate fairness.
6. Add the authored dungeon route graph and forge/boss rooms toward the 24-floor structure.

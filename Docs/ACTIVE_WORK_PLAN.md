# FrankenArms active work plan

This is the execution order for the first vertical slice. Each phase is implemented in small commits and checked through the connected Unity Editor.

1. **Combat feel**
   - player and enemy pixel silhouettes;
   - idle, muzzle flash, hit burst, screen shake;
   - projectile damage and contact damage;
   - defeat and restart.

2. **Room loop**
   - controlled enemy wave;
   - locked doors and physical exit;
   - room variations;
   - reward pickup and persistent credits;
   - next-room result panel.

3. **Weapon fusion**
   - data-driven grip/barrel definitions;
   - three fusion results;
   - fusion station controls;
   - apply result stats to the player weapon.

4. **Visual pass**
   - point-filtered pixel floor and walls;
   - authored sprite-sheet replacement when source art is available;
   - readable colors for player, enemies, exits, rewards, and weapons.

5. **Mini-run pass**
   - five-room controlled combat cycle;
   - chaser and elite encounter variants;
   - persistent best-room progress;
   - compact safe-area HUD;
   - balance sheet for playtest tuning.

6. **Stability pass**
   - compile and Console checks;
   - Play Mode startup check;
   - inspect Git diff and update `SESSION_HANDOFF.md`;
   - keep generated Unity caches out of Git.

7. **GDD systems pass (current)**
   - armor, mana, Dash i-frames, and first pet companion;
   - ranged enemies, controlled route choices, forge route, and boss gate;
   - authored ScriptableObject fusion recipes.

8. **GDD content expansion (next)**
   - route graph asset with room node templates and 6–9 room floor structure; **24 ordered node assets now exist and are loaded by `CombatRoom`**;
   - three-scene product flow (`MainMenu → Lobby → Run`) is now in place and verified;
   - hero roster data and one additional hero ability;
   - status effects (burn, poison, freeze, mark, curse) with readable telegraphs;
   - loot, shop, shrine, secret, and helper-room templates;
   - meta progression: crystals, blueprints, diary entries, and house unlocks;
   - biome pass, boss phases, audio/VFX, and final 24-floor route.

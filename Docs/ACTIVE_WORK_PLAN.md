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

5. **Stability pass**
   - compile and Console checks;
   - Play Mode startup check;
   - inspect Git diff and update `SESSION_HANDOFF.md`;
   - keep generated Unity caches out of Git.

The current environment cannot continue autonomously after this chat turn. When the user returns, resume at the first unchecked item and run the same Unity verification loop.

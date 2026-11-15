# FrankenArms combat slice balance

This is the current vertical-slice tuning sheet. Values are intentionally small and readable while the room loop is being playtested.

| Room cycle | Label | Composition | Threat intent |
| --- | --- | --- | --- |
| 1 | Warmup | 3 stationary targets | Teach movement, aim, and doors |
| 2 | Pursuit | 3 targets + 1 chaser | Teach target priority and kiting |
| 3 | Crossfire | 2 shooters + 2 targets + 1 chaser | Mix ranged timing, stationary pressure, and pursuit |
| 4 | Gauntlet | 5 stationary targets | Test sustained DPS and movement |
| 5 | Elite Gate | 4 targets + 2 chasers; chasers have 4 HP | First mini-elite check before the boss gate |
| 6 | Boss Gate | 1 boss, 18 HP, radial burst | First complete mini-run endpoint |

## Starting combat math

- Player base weapon: 1 damage at 5.56 shots/s, so nominal DPS is 5.56 before movement and accuracy loss.
- Standard target: 3 HP, approximately 0.36–0.54 s of nominal fire depending on projectile timing.
- Elite chaser: 4 HP, approximately 0.54–0.72 s of nominal fire.
- Player health: 5 HP; contact damage is 1 with 0.8 s invulnerability.
- Player armor: 2 points; the first incoming hit breaks all armor without spilling into HP, then armor returns after 3 s without damage.
- Room reward: 10 persistent credits per clear.

## Ranged pressure

Crossfire room adds two shooters. Each fires after a 0.75 s opening and then
every 1.45 s at 4.6 units/s. A bolt deals 1 damage and is answerable by
lateral movement or Dash; the room keeps one chaser as a second priority
target. This is a readable threat clock, not a constant bullet wall.

## Boss gate

Room 6 spawns one large 18 HP boss. It advances only when the player is far
away and fires a three-bolt burst every 1.8 s after a 1.2 s opening. The spread
is 44 degrees total, so lateral movement or Dash can answer it without
requiring pixel-perfect dodging.

## Route choices

After each cleared combat room the player chooses one controlled branch: `1`
Safe (normal next room), `2` Risk (adds ranged pressure to the next eligible
room), or `3` Forge (skips the next wave and opens the room as a safe decision
space). The prototype remains deterministic; the choice changes the authored
encounter rather than rolling an unbounded random walk.

## Statuses

Crossfire bolts apply Poison for 3 s, dealing one tick per second; boss bolts
apply Freeze for 0.6 s and reduce movement to 45%; the pet's bite applies Mark
for 4 s, increasing subsequent damage by 25%. Burning and Curse are part of
the shared controller and are reserved for fire/curse room templates.

## Hero active abilities

The first playable ability pass uses `Q` with a 6 s cooldown. Knight gains 1.25 s
of invulnerability and a 1.5-unit strike, Mage deals 2 damage in a 3.2-unit burst,
and Mechanic deploys a turret for 8 s that attacks nearby targets every 0.8 s.

## Authored non-combat nodes

`Forge`, `Reward`, and `Secret` route nodes do not spawn a combat wave. They open
the doors immediately and create a pickup using the node's authored reward budget.
Secret rewards appear offset from the room center and use the violet mystery color;
the route remains deterministic and the secret is never required for combat victory.

## HUD route readability

The top-right route strip shows visited floors in their node color, the current
floor in amber, and unknown floors in muted gray. The run timer sits beside best
floor progress so the player can read pace without covering the arena center.

The intended lesson is target priority, not sponge damage. If a chaser causes repeated unavoidable contact, tune its speed below 1.35 or increase the player invulnerability window before increasing player damage.

## Critical hits

Fused/module shots have a 12% critical chance for 2x damage and a stronger
impact sound. The free base weapon has no random critical chance, preserving the
GDD distinction between reliable starter combat and risky found/fused weapons.

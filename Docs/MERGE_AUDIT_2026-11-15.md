# Merge audit: `2dRoguelike` → `FrankenArmsFresh`

## Result

Both folders are Unity 6 FrankenArms iterations, not unrelated games. `FrankenArmsFresh` is the safer authoritative runtime: it has the verified Main Menu → Lobby → Run flow, authored 24-floor route data, hero selection, room loop, boss gate, armor, pet, statuses, and a clean console baseline.

The sibling project contributes a broader design prototype: modular `Grip + Barrel` weapon data, module rarity, localization, lobby systems, room templates, a minimap, and many art assets. Its runtime cannot be copied wholesale because it defines global classes that collide with the Fresh implementation (`WeaponController`, `Projectile`, `PlayerHealth`, `MainMenuController`, and others), and its scenes use those old component types.

## Integrated now

- `Assets/Imported/ParentModules/Scripts/WeaponGripSO.cs`
- `Assets/Imported/ParentModules/Scripts/WeaponBarrelSO.cs`
- `Assets/Imported/ParentModules/Scripts/RarityColors.cs`
- `Assets/Imported/ParentModules/Scripts/ElementType.cs`
- nine authored assets from the sibling `Resources/Modules` folder
- `Assets/Scripts/ImportedModuleForgeBridge.cs`, connected by `FrankenArmsBootstrap`

In the Run scene, approach the forge and press `4`, `5`, or `6` to test imported Grip/Barrel combinations. The existing numbered fusion recipes remain available separately.

## Deliberately not integrated

- sibling scenes and project settings;
- sibling `WeaponController`, player, enemy, dungeon, and menu controllers;
- third-party art packs and package configuration;
- the sibling random-walk dungeon generator, because the Fresh project decision is to use controlled authored routes.

## Next recommended merge slice

Port the sibling `MinimapUI` and selected lobby services behind Fresh interfaces, one subsystem at a time. Review each asset pack's license before copying textures or prefabs.

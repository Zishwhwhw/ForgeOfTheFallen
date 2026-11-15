# FrankenArms visual generation prompts

These prompts are derived from the authoritative visual rules in the GDD. The GDD does not contain literal image-generation prompts, so this file records the normalized prompts used for project assets.

## Shared art direction

32-bit detailed pixel art for a dark 2D top-down action roguelike. Warm wood, stone, brass engineering, amber reward light, cyan technology, violet elite mystery, orange fire, red danger. Crisp deliberate pixels, strong dark navy outlines, readable silhouette at small gameplay size. No pixel blood: communicate damage with sparks, cracks, smoke, magic and scorch marks. No text, UI, labels, logos or watermark.

## Hero — gunsmith

Transparent 2D top-down action roguelike sprite. A compact stitched-together scavenger gunsmith wearing a dark teal coat, brass shoulder armor, cyan glowing eye visor, one oversized mechanical arm holding a small orange plasma pistol. Crisp readable silhouette, 32-bit pixel art, limited deep navy/teal/brass/cyan/ember-orange palette, one-pixel highlights, no ground shadow, no frame, no UI, transparent background.

## Enemy roster

Transparent 2D top-down action roguelike sprite sheet with three isolated enemies in a horizontal row: crimson stitched slime target with one bright eye; fast orange brass-and-flesh pursuit enemy with a sharp forward silhouette; purple elite brute made of stone, bone and clockwork plating. 32-bit detailed pixel art, readable at gameplay size, navy outlines, GDD palette, no shadow, labels, text or watermark.

## Dungeon floor

Seamless square top-down dungeon floor texture: dark blue-black slate stone slabs with irregular readable seams, subtle warm brass engineering inlays and tiny cyan magical-tech traces, restrained amber flecks, crisp 32-bit pixel art, no perspective, no walls, no objects, no text, tileable on all four edges.

## Integration rules

- Import sprites as Point filtered, no mipmaps, alpha transparency enabled.
- Use authored sprites first and retain procedural fallback for editor/import failures.
- Keep sprite sheets sliced through Unity Sprite Editor data-provider capability checks.
- The generated assets are prototypes, not final art; replace them with reviewed authored sheets when the art pass is approved.

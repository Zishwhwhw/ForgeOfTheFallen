# Недостающие концепт-листы среза (12–17) — промпты для Gemini

Сверка GDD §2.2, §7, §8.3, §9.1 с листами 01–11 (`CHOICES.md`). Не хватает концептов для того, что ниже.
Шаблон — GDD §10.5 «Концепт-лист»: STYLE CORE + NEGATIVE + 2 референса: `ArtSource/concept_refs/knight_x8.png` (спрайт Рыцаря ×8) и `Research/concepts/v2/g3.jpg` (настроение).
Генератор: Gemini (веб), одна картинка 16:9 на лист. Ответ сохранять как `Research/concepts/sheets/NN_name.jpg`.

**Как делали 27.11 (проверено):** Gemini во встроенном браузере Claude. Без референсов Gemini рисует «своих» врагов и Голема — не делать так. Владелец один раз перетаскивает в новый чат 10 файлов из `ArtSource/gemini_upload/` (утверждённые спрайты, выбранные варианты листов 01–11, R3, HUD v2, убежище, меню; собираются из листов скриптом в этой сессии), затем каждый промпт ссылается на них по номеру («the approved Golem in image 04»). Отправленные промпты — укороченные версии ниже с отсылками к картинкам. Скачать/скопировать картинку из панели нельзя (CSP Gemini, буфер заблокирован), поэтому картинка 1376×768 показывается в странице 1:1 и снимается тремя скриншотами панели, склейка — `Tools/art/save_sheet.sh` + `stitch_tiles.py`.

| Лист | Что | Почему нужен (GDD) |
|---|---|---|
| 12 | Химеры ×3 + Голем фазы 2–3 (щит-миньон, ядро-сердце) | §7.5, §7.7 — нет ни на одном листе |
| 13 | Интерактив комнат: сундуки по редкости, сундук-сердце, секретная стена, клетка помощника, ловушка + рычаг, дверь комнаты | §7.9, §8.3, §8.4 |
| 14 | Убежище: пьедестал выбора героя, кристаллы, стенд оружия/дневник, манекен полигона, костёр, портал-хаб | §9.1 (общий вид — `02_refuge_cave_oasis.jpg`) |
| 15 | Иконки: 15 пассивок (16 px, латунный жетон) | §7.2 |
| 16 | Иконки: 4 статуса, 4 проклятия, редкость оружия (рамки), снаряды врагов и оружия | §7.3, §7.8, §5.2 |
| 17 | Экраны UI: выбор пассивки 1 из 3, магазин торговца, выбор героя, итоги забега | §2.5, §7.10 |

## Общие куски (вставляются в каждый промпт)
**STYLE CORE:** crisp pixel art, no anti-aliasing, Endesga 32 palette only, 1px dark outline in the object's own colour family (not pure black), light from top-left, one shade and one highlight per material, chibi proportions (head ~1/2 of height). World "Forge of the Fallen": a castle swallowed by a forge-factory; everything is scrap riveted together, stitched with brass staples and weld seams, rust and dark steel, glowing ember-orange heart cores. Cold steel blues (#3a4466 #5a6988 #8b9bb4) against hot furnace light (#f77622 #feae34). Warm dark fairy tale, light humor, no blood.

**NEGATIVE:** no blood, no gore, no text or letters except the cell numbers and a/b/c, no realistic rendering, no soft gradients, no semi-transparent pixels, not Soul Knight style, no copyrighted characters.

**LAYOUT:** DESIGN SHEET for a 2D top-down pixel-art roguelike, one 16:9 image. Layout: 3x2 grid of 6 cells on plain navy #262b44, thin grey divider lines, big white number 1..6 in each cell's top-left corner. Match the attached hero sprite's pixel style and scale exactly; draw the small hero once in cell 1 for scale.

## 12 — Химеры и фазы Голема
{LAYOUT} Each cell = one subject in 3 variants a, b, c side by side, letters under them.
1. Chimera "rat + bat": a scrap rat body with bat wings stitched on by brass staples, ember-orange seam glow, small (half hero height).
2. Chimera "shieldbearer + gargoyle": skeleton shieldbearer with a big riveted tower shield, stone gargoyle wings and horns bolted on, shield turns to stone; hero height.
3. Chimera "archer + cultist": skeleton archer in a torn purple cultist hood, bow with a glowing holy-gear crystal, a healing beam string; hero height.
4. Armor Golem phase 2 minion: a flying riveted tower shield with a small ember eye and steam vents, size of the hero.
5. Armor Golem phase 3: the exposed heart core — a glowing ember-orange heart in a cage of broken armor ribs and chains, floating, 2x hero size.
6. Armor Golem lying defeated (death frame): the hunched halberd guardian collapsed on the floor, armor plates scattered, core dimmed, view from top-down 3/4.
{STYLE CORE} {NEGATIVE}

## 13 — Интерактив комнат
{LAYOUT} Each cell = one subject in 3 variants a, b, c.
1. Treasure chests by rarity (common iron-and-wood / rare steel with blue gem / epic brass with ember core), closed; show one opened chest next to it.
2. Heart chest: a small chest shaped like a riveted iron heart with an ember glow, gives max HP.
3. Secret wall: castle stone wall tile with a visible crack hinting a secret, and the same wall broken into rubble after a bomb.
4. Helper cage: a hanging scrap-iron cage with a padlock and a small chibi prisoner (blacksmith apprentice) inside.
5. Trap + lever: floor spike plate (retracted / extended), and a wall lever (up / down) with brass gears.
6. Room door: heavy castle door of riveted iron bars, open and closed-locked state with ember lock glow.
{STYLE CORE} {NEGATIVE}

## 14 — Убежище (хаб)
{LAYOUT} Each cell = one subject in 3 variants a, b, c. Setting: refuge cave-oasis at an underground lake, warm campfire light among stone and scrap.
1. Hero selection pedestal: a stone-and-brass podium with a soft spotlight where the chosen hero stands.
2. Crystal currency deposit: purple crystals growing from a scrap pedestal, and a locked mage statue that opens for crystals.
3. Fusion diary stand: a lectern with a thick riveted book, and a wall rack displaying seen weapons and hybrids.
4. Training dummy for the fusion range: straw-and-scrap dummy on a pole, with a brass target plate.
5. Campfire and seating made of scrap, cozy, with a cooking pot.
6. Run portal: a rift portal ring of riveted iron and gears with a swirling ember-blue vortex.
{STYLE CORE} {NEGATIVE}

## 15 — Иконки пассивок
ICON SHEET for a 2D pixel-art roguelike, one 16:9 image on plain navy #262b44: 15 icons in a 5x3 grid, each icon 16x16 pixel art shown upscaled x8, a simple bold symbol on a round brass riveted badge, readable at 16 px, white number 1..15 under each.
1 second dash (two boot wings), 2 reflecting blade (sword with bounce arrow), 3 stray bullet (bullet with wavy trail), 4 piercing tip (arrowhead through plate), 5 bounce (ball ricochet zigzag), 6 fireproof (flame crossed by shield), 7 thrift (mana drop with a coin), 8 dash shadow (ghost silhouette), 9 chain reaction (linked explosions), 10 magnet (horseshoe magnet with coins), 11 iron vow (iron heart with chain), 12 glass cannon (cracked glass cannon), 13 greed (gold coin with teeth), 14 berserk (red horned helmet, fiery eyes), 15 double holster (two crossed pistols).
{STYLE CORE} {NEGATIVE}

## 16 — Статусы, проклятия, редкость, снаряды
{LAYOUT}
1. Status icons 16x16 on brass badges: burn (flame), cold (snowflake), shock (lightning), poison (green droplet) — 3 variants a, b, c.
2. Curse icons 16x16 on dark red cracked badges: blood deal (blood-red drop with mana spark — no gore, stylized), greed deal (gold coin), darkness deal (eye in black), haste deal (hourglass with wings) — 3 variants a, b, c.
3. Weapon rarity frames for 32x32 UI icons: common grey iron, rare blue steel, epic brass with ember glow — 3 variants a, b, c.
4. Hero projectiles 8x8 and 16x16: nail, arrow, crossbow bolt, shotgun pellet, spark orb, ember ball, frost shard, lightning bolt.
5. Enemy projectiles 8x8 and 16x16 in enemy colour #ff0044 family: bone arrow, cultist orb, gargoyle stone, golem helmet, golem bullet ring orb.
6. Telegraph markers on the floor: red circle zone, red line for a lunge, red ring for a shockwave — 3 variants a, b, c.
{STYLE CORE} {NEGATIVE}

## 17 — Экраны UI
UI DESIGN SHEET for a 2D pixel-art roguelike, one 16:9 image, 2x2 grid of 4 screen mockups at 640x360 pixel grid, same style as the chosen HUD: dark iron, rivets, ember glow, brass frames.
1. Passive choice "1 of 3": three tall cards with an icon, name bar and short description lines (use placeholder squiggles, no real text).
2. Merchant shop: a scrap-metal counter with item slots (weapons, potions), price tags with a coin symbol, a sell slot.
3. Hero select: two hero portraits (knight, locked mage with a crystal price), a big start button frame.
4. Run results: summary panel with icons for rooms, kills, gold, crystals, the best hybrid weapon shown large.
Placeholder squiggles instead of words. {STYLE CORE} {NEGATIVE}

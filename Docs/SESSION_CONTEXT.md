# Контекст для следующей сессии (передача дел)

Обновлено: 2026-11-27. Ветка: `feature/greybox-1-hero` (на GitHub). `main` ещё не содержит эту работу — PR не открыт.
Сначала прочитай `AGENTS.md` (правила), затем `Docs/GDD.md` §2, `Docs/ARCHITECTURE.md`, `Docs/DECISIONS.md`.

## Где мы сейчас
Серая коробка вертикального среза (GDD §2.4) собрана в 3 поставки, все закоммичены, EditMode-тесты 46/46, консоль чистая.
**Владелец ещё не сыграл поставку 3** — следующий шаг: его плейтест и правки (формат: Сейчас → Должно быть → Шаги → Сцена/коммит).

| Поставка | Коммит | Что есть |
|---|---|---|
| 1. Герой | `91f53a7` | Рыцарь X2: WASD, прицел мышью 360°, меч (ЛКМ), гвоздомёт, Q — смена, рывок Space/ПКМ, манекен |
| 2. Бой | `f38e631` | Скелет-мечник (телеграф #ff0044 → удар), двери запираются, 2 волны (2 и 3 врага), сундук +золото, смерть → E рестарт |
| 3. Сплав | `cca2e88` | Оружие на полу (E — взять/обмен), кузница: 2 плиты + рычаг, ритуал 3.2 с (E — пропустить), гибрид падает на пол, HUD «ФРАНКЕН» |

Сцена: `Assets/_Project/Scenes/GreyBox.unity` → Play. Сплав: справа кузница → меч на плиту → Q → гвоздомёт на другую плиту → рычаг.

## Карта кода (сборки по ARCHITECTURE.md)
- **FotF.State** (без движка): `HeroState` (слоты = `WeaponInstance`), `EnemyState`, `WeaponInstance`, `Enums` (namespace FotF.Data, перенесены сюда).
- **FotF.Data** (SO): `HeroDefinition`, `WeaponDefinition` (displayName, spriteForwardAngle), `EnemyDefinition`, `EncounterDefinition`, `WeaponCatalog`, `FusionRulesDefinition`.
- **FotF.Logic** (чистый C#): `Combat/` Vitals, Dash, WeaponAttack, EnemyBrain, Encounter; `Fusion/FusionService` (GDD §6.2); `SeededRandom`.
- **FotF.View**: `Heroes/HeroController` (SetSlot/PickUp/TakeActiveWeapon), `Combat/WeaponView` (база + деталь + оттенок), `WeaponFactory`, `ProjectileView`, `HitStop`, `SpriteFlash`; `Enemies/EnemyController`; `World/` RoomController, DoorView, ChestView, WeaponPickupView, ForgeView, ForgePartView; `UI/HeroHudView`.
- **Данные**: `Assets/_Project/Data/` — hero_knight, weapon_eternal_sword (18 урона, 2.5/с), weapon_nailgun (8, 5/с, 1 мана), weapon_catalog, enemy_skeleton_swordsman, encounter_greybox, Fusion/fusion_rules.
- **Префабы**: `Assets/_Project/Prefabs/` — Heroes/hero_knight, Enemies/enemy_skeleton_swordsman, Projectiles/proj_nail, Interactables/{prop_chest, prop_door, prop_training_dummy, prop_forge, pickup_weapon}, UI/fx_damage_number.
- **Тесты**: `Assets/Tests/EditMode/` — Logic (Vitals, Dash/Weapon, Enemy/Encounter, Fusion), Prefabs (скрипты включены, слои, проводка префабов сплава), Art (палитра Endesga 32, размеры канвасов, импорт).

## Технические факты, которые легко забыть
- Unity 6000.6.7f1 на `D:\Unity\Editor`. Управление редактором — Unity CLI + пакет Pipeline: `unity command script eval --file …`, `unity command test run --mode editor`, `unity command editor.playmode start|stop`, `unity command capture game_view`. Сразу после перекомпиляции команды могут вернуть exit 6 — дождаться `unity status --until-ready` и повторить.
- В eval-скриптах не использовать `??` с компонентами Unity (Unity-null) — только `TryGetComponent`.
- Слои: Walls=8, Hero=9, Enemy=10, Projectile=11. После правки TagManager — `SaveAssetIfDirty`, иначе слои не сохраняются.
- Y-сортировка: Renderer2D CustomAxis (0,1,0) + SortingGroup + pivot у ног.
- Арт: 32 PPU, палитра Endesga 32, канвасы кратны 8 (иначе падает `AllArt_PassesArtBibleChecks`). Привести PNG: `FotF.EditorTools.Art.PixelArtMenu.QuantizeFile(path, false)`. Рыцарь 48 px на холсте 64×64, pivot (0.5, 11/64); скелет 48×48.
- Python для обработки картинок: `D:\AI\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe` (есть PIL; системного python нет).
- PixelLab REST v2, ключ только из переменной окружения `PIXELLAB_KEY` — **никогда не писать ключ в файлы**. Не больше 2 одновременных задач (иначе 429). CDN-ссылки отдают 403 скриптам — скачивать через `/characters/{id}/zip` и `/rooms/{id}/kit.zip`. Потрачено ~160 генераций из 2000.
- `ArtSource/` (сырой вывод генераций, manifest.json) — локально, не коммитится.
- Незакоммичены автоизменения `ProjectSettings/*`, `Assets/Settings/UniversalRP.asset`, `font_ui_dynamic.asset` (динамический атлас шрифта) — происхождение неясно, не трогать без владельца.

## Открытые вопросы владельцу (не реализовывать молча)
1. Вес доминанты при сплаве — сейчас 0.7 (в GDD нет).
2. Идеальный сплав — сейчас: урон и крит суммируются, лучшая скорость, меньшая мана. Так ли?
3. Имя гибрида — сейчас «А + Б», нужна схема.
4. Реплики Франкена — 3 черновые в `fusion_rules.asset`, тексты за владельцем (GDD п.155).
5. Масштаб врагов/боссов (GDD §13): холст врага 48 vs 32 в Art Bible.
6. Контактный урон врагов — значение не задано.

## Известные недочёты / бэклог
- Плиты и рычаг кузницы — заглушки, нарисованные кодом; подпись над оружием на полу мелкая.
- Нет карт нормалей у спрайтов (решение по свету: 2D-спрайты + normal maps + URP 2D Lights).
- Кадр рывка вверх у рыцаря сдвинут на 5 px; предупреждение Pixel Perfect в маленьком Game view.
- Маг M2 — только концепт, ассетов нет.
- Живой ввод с клавиатуры для поставки 3 не проверялся (ночью экран был заблокирован) — проверено зондами и снимками.

## Утверждённое арт-направление
PixelLab (подписка). Рыцарь X2 (рогатый латунный шлем, светящийся визор, красный плащ, башенный щит), маг M2, скелет S2, комната R3 (тайлсет замка), главное меню «Разлом». Подробности — `Docs/DECISIONS.md`, `Research/07_ART_BIBLE_SPEC.md`.

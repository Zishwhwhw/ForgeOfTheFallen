# Контекст для следующей сессии (передача дел)

Обновлено: 2026-11-27 (вечер). **Рабочая ветка: `art/concepts-weapon-holds`** — это `feature/greybox-1-hero` + арт-сессия на Mac (концепты, спецификации, скрипты). `main` ещё не содержит ни то, ни другое — PR не открыт.
На ноутбуке: `git fetch && git checkout art/concepts-weapon-holds`.
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

## Арт-сессия 27.11 (Mac, без Unity) — ветка `art/concepts-weapon-holds`
**Процесс (решение владельца):** Codex рисует концепт-лист (6 клеток, в клетке варианты a/b/c) → владелец выбирает → по выбору генерируем в PixelLab. Порядок: все концепты среза → все модельки среза в PixelLab → затем 1.0. Шаблоны промптов для всех типов ассетов — **GDD §10.5** (общее ядро стиля + запреты + шаблон под тип).

**Концепты (все выбраны)** — `Research/concepts/sheets/01…11` + `CHOICES.md`: Голем 6; оружие I и II; враги I и II; кузница, Сердце, торговец; пропсы; пикапы и VFX; HUD v2 — **вариант 2** (тёмное железо, угли) по схеме владельца: полукруг HP/броня/мана снизу по центру, слева золото + баффы, справа 2 слота оружия, мини-карта сверху справа. Мелкие ассеты владелец доверил выбрать Claude.

**Решения сессии (записаны в документы):**
- Герой — цельная покадровая анимация **без оружия**; оружие — отдельный слой: 6 типов хвата, классы размера S/M/L (≤1/3, ≤3/4, ≤ роста), перчатки-хваты, атака по типу хвата (Art Bible §5.1, DECISIONS).
- Оружие в **правой** руке, щит в левой; клинок смотрит по прицелу (лицом к камере — вниз); спиной — оружие за телом. Повороты запекаются RotSprite (32 угла), без сырого поворота в рантайме.
- Смерть: герои — 6 кадров; мобы/элита/мини-босс — белое мерцание → сплющивание → плоский труп до конца этажа; босс — 1 кадр «лежит» + растворение в угли (GDD §10.2).

**Инструменты:** `Tools/art/pl.py` (PixelLab v2; ключ из env `PIXELLAB_KEY` или Keychain macOS, в файлы не пишется), `Tools/art/check_weapon.py` (класс размера, палитра, альфа, апскейл), `Tools/art/rotsprite.py` (запекание углов оружия).

**PixelLab:** баланс **1796 / 2000** (27.11 вечер). Метод оружия проверен: `generate-with-style-v2`, эталоны — вырезка Рыцаря `NoSword` + меч в холсте класса (M: 28→32), палитра Endesga 32 → 64 варианта за 10 генераций. Проба — `concepts/sheets/10_test_sabre_*` (выбрана сабля `s28_08`). Pro-задачи (`create-character-pro`) стоят 10–25 и списываются даже при отмене — не отменять на ходу.
- Codex для картинок: `codex exec --ephemeral -s read-only -c model_reasoning_effort=low -i a.png,b.png - < prompt.txt` (картинки через запятую, промпт через stdin — иначе `-i` съедает промпт). Лист ≈ 14–23k токенов.

**Следующий шаг (ждёт «go» владельца):** генерация в PixelLab по плану: 1) Маг без посоха + починка вида Рыцаря спиной; 2) 13 видов оружия + «Огнедробь» (~140); 3) Голем 6 (~25); 4) враги + Капитан (~150–250); 5) кузница, Сердце, торговец, пропсы (~100–200); 6) пикапы, VFX, HUD (~50–100). Анимации — считать после п.4.

### Сделано 27.11 вечером на ноутбуке (Unity, Claude): оружие в руках и камера
- `WeaponDefinition`: `gripPixel` + новый `muzzlePixel` + `rotatedSprites` (32 запечённых RotSprite-угла). Меч: хват (24,25); гвоздомёт: хват (8,11), дуло (27,17). Снаряд вылетает из дула (`WeaponView.MuzzleWorld`), проверено численно во всех направлениях.
- Рука по направлению: `HeroDefinition.handSouth/Side/NorthPixels` (Рыцарь: (-9,12), (4,12), (8,12)); спиной оружие за телом. Вид спиной Рыцаря зеркалится (`SpriteAnimationSet.mirrorUp`) — щит теперь слева, баг закрыт.
- Оружие качается с телом: покачивание по кадрам считывается из спрайтов (`FotF/Art/Bake Body Bob`, `SpriteAnimationSet.bob*`).
- RotSprite: `python -I Tools/art/bake_weapon_rotations.py` → `Art/Sprites/Weapons/Rotated/<оружие>/` (72×72, хват в центре) → меню `FotF/Art/Assign Weapon Rotations`. Новое оружие: задать gripPixel/muzzlePixel, запечь, назначить.
- Камера не показывает пустоту за комнатой: `AimCameraFollow.roomBounds` (тайлмап Room_Floor, отступ 1 юнит).
- Осталось из ТЗ ниже: `holdType`/`sizeClass`/`secondGripPixel`, перчатки 4×4, анимации атаки по типу хвата.

### ТЗ для ноутбука с Unity: оружие в руках (Art Bible §5.1)
1. `WeaponDefinition`: добавить `holdType` (OneHandMelee, TwoHandHeavy, Pistol, TwoHandGun, Staff, Bow), `sizeClass` (S/M/L), `secondGripPixel`, `muzzlePixel` (`gripPixel` уже есть). Заполнить для меча и гвоздомёта.
2. `HeroDefinition`: точки руки по направлениям (south/east/north, west — зеркало) + спрайт перчатки 4×4. Рыцарь — тело из состояния `NoSword`.
3. `WeaponView`: спрайт — ближайший из 32 запечённых углов (`rotsprite.py`), не поворот трансформом; позиция = точка руки + смещение так, чтобы `gripPixel` совпал с рукой; перчатки в `gripPixel`/`secondGripPixel` поверх рукояти; качание 1 px с кадрами бега; сортировка за/перед телом как сейчас.
4. Анимации атаки по `holdType` (таблица §5.1) — твины слоя (DOTween или корутина), длительности в SO, не в коде. VFX — префабы.
5. Готово, если: смена меч ↔ гвоздомёт ↔ посох меняет только слой; в 4 направлениях рукоять в руке, без «висящего» оружия; EditMode-тест на расчёт точки хвата; скриншоты 3 типов хвата владельцу.
- ~~Баг спрайта Рыцаря X2: вид спиной — щит не на той стороне~~ — исправлено зеркалом north-кадров (27.11).

### ТЗ для ноутбука: смерть (GDD §10.2, решение 27.11)
- `EnemyDeathView` (на префабе врага): мерцание белым через `SpriteFlash` 0.15 с → твин scale Y 1→0.2, X 1→1.3 за 0.15 с с pivot у ног → спрайт затемнить (тинт ~#262b44 50%), отключить коллайдеры/ИИ, оставить трупом; лимит трупов на этаж — общий (п.165). Длительности и масштабы — в SO.
- Босс: вспышка → кадр `boss_golem_dead` → пауза ~1.5 с → материал растворения (Shader Graph: шум + порог + оранжевая кромка #f77622) за ~1 с → дроп награды.
- Герой: анимация death 6 кадров из PixelLab.

### Баланс в JSON и сейвы (конец сессии 27.11)
Владелец: все сущности и настраиваемые параметры — конфиги баланса; движок их ест; сейв — JSON; при новых фичах (пример — инвентарь) всё должно обновляться. Сначала обсуждали Google Таблицу (закрытую), затем владелец решил: **без Google, сразу JSON, без его участия**.
Сделано на Mac: `Assets/_Project/Data/Balance/*.json` — 12 файлов, 62 сущности среза (оружие 13, герои 2, враги 9 + 3 химеры + Капитан, Голем с фазами, 15 пассивок, 4 статуса, 4 проклятия, расходники, константы экономики, сплав, волны серой коробки, game feel смерти); схема `Tools/data/balance_schema.json`; валидатор `Tools/data/validate_balance.py` (OK) + 11 тестов (зелёные). Числа — только из GDD; «калибровать» = `null`.
Вопрос владельцу (записан в `_note` обреза): GDD «5×6» прочитано как 5 дробин × 6 урона — по аналогии с арбалетом «3×12» и посохом инея «3×6».
Дальше — ноутбук с Unity: ARCHITECTURE §8.6 шаги 2–4 (Newtonsoft, поля SO, импортёр, сейвы).

### Сделано 28.11 (Claude, ноутбук, владелец отошёл — «работай по GDD»)
- **Смерть врагов по GDD §10.2** (ТЗ выше, кроме босса и героя): `EnemyDeathView` на префабе скелета — белое мерцание 0.15 с → сплющивание к ногам (Y 0.2, X 1.3, ease-out) за 0.15 с → кадр замирает, труп затемнён к #262b44 на 50%, остаётся до конца этажа. Труп рисуется под живыми (SortingGroup order −5; пол −10) — техническое решение, не дизайн.
- Общий лимит трупов на этаж: `Logic/Combat/CorpseQueue` (самый старый убирается) + `View/Enemies/CorpseRegistry` (очистка при выгрузке сцены). Лимит 300 из constants.json.
- Числа — новый SO `GameFeelDefinition` → `Data/GameFeel/gamefeel_default.asset`; тест сверяет его с `gamefeel.json` и `constants.json` (первый мост JSON→SO, до импортёра §8.6). Микропауза убийства теперь тоже из SO.
- `SpriteFlash.Flash(seconds)` — вспышка заданной длительности.
- Тесты 57/57, консоль чистая; проверено в Play: скелет сплющивается в тёмный «блин».
- Плейтест через computer use: клавиатура от инструмента не доходит до Input System (мышь доходит) — клавиши жать через `InputSystem.QueueStateEvent` в `script eval`. Game view на Scale 1.3x обрезает HUD по краям.
- Не сделано: смерть босса (растворение, Shader Graph) и героя (6 кадров из PixelLab — нужна генерация, ждёт «go»).
- **ARCHITECTURE §8.6 шаги 2–3 (28.11):** Newtonsoft — прямая зависимость; поля схемы добавлены в SO (`statusChance`, `special`, `holdType`, `sizeClass`, `ultimateCooldown`, `unlockedFromStart`, `tier`, `parents`, `reforgeLimit/Price`, `id` у сплава); новые SO: `BossDefinition`, `PassiveDefinition`, `StatusDefinition`, `CurseDefinition`, `ConsumableDefinition`, `BalanceConstants`; `GameFeelDefinition` — вложенный `death` как в JSON. Поля-«map» → `ValueMap` (ключ → число/текст/bool/null), механики → `SpecialMechanic`, эффекты → `EffectSpec`.
  - Импортёр: `Editor/Data/BalanceImporter.cs`, меню **FotF/Данные/Импорт баланса** + авто-импорт при изменении JSON. SO по `id`, новые — `Data/<Папка>/<id_с_подчёркиваниями>.asset`; ссылки по id; `null` у чисел = значение SO, у объектов/списков = «нет»; новое оружие добавляется в `weapon_catalog`.
  - Первый прогон: 62 сущности, создано 55 SO, числа старых ассетов (2 оружия, Рыцарь, скелет, сплав, волны, game feel) не изменились — проверено дампом до/после. У гвоздомёта из JSON проставились хват Pistol / класс S.
  - Тесты `BalanceDataTests`: enum'ы схемы = C#, каждое поле схемы есть в SO с подходящим типом, каждый id имеет SO, значения SO = JSON (если упал — запустить импорт). Всего 60/60, валидатор OK.
  - У новых SO нет арта (спрайты, иконки) — проставить при производстве (§2.4 п.4).
- **ARCHITECTURE §8.6 шаг 4 (28.11): сейвы.** `State/Save/`: `ProfileState` (кристаллы, открытые герои, дневник сплавов, статистика, убежище), `SettingsState` (громкости, язык, game feel, тряска, раскладка), `SaveFile` (атомарная запись tmp → проверка → `File.Replace` с `.bak`; битый → `.bak` → новый + `PlayerShouldBeNotified`; цепочка миграций по `saveVersion`), `SaveMigrations` (пока пусто, v1). View: `View/Save/GameSaves` — пути в `persistentDataPath`. Неизвестные id: `ProfileState.DropUnknownIds` с логом (таблицы замен в constants нет — при необходимости добавить в схему).
  - Тесты `SaveTests` (13): новый файл, круг записи/чтения, `.bak`, битый файл, оба битые, старое без нового поля, цепочка миграций, нет миграции, неизвестные id, образцы `Assets/Tests/SaveSamples/*_v1.json`, санитайз, снимок `save_schema_snapshot.txt`. Всего 73/73.
  - Поля профиля/настроек — по списку ARCHITECTURE §8.4; состав дневника сплавов и статистики — черновой, владельцу подтвердить при работе над убежищем/итогами. В игру сейв ещё не подключён (меты в серой коробке нет).

### Поставка 4: этаж (28.11, Claude; владелец: «делай срез без вопросов»)
- Решения владельца: 4–5 боевых, сетка + коридоры, кузница — ответвление, шансы спец-комнат — черновые в JSON. Записано в GDD §8.1 и DECISIONS.
- Данные: вид `floor` в схеме, `floors.json` → `Data/Floors/floor_castle.asset` (`FloorDefinition`).
- Logic: `Logic/Generation/FloorGenerator` — граф по seed: старт → цепочка из 4–5 боевых на сетке, ответвления-тупики (кузница, торговец, мини-босс, элита, сундук, святилище, фонтан, клетка, секрет; лимит `maxBranches`). Гарантии §8.2 проверяет `Validate` (тест на 1000 seed).
- View: `FloorRunner` (сцена **Run**, первая в Build Settings), `FloorLayout` (раскладка без сцены), `DualGridPainter` + `TerrainTileset` (`Art/Tiles/Castle/tileset_castle_r3.asset`: стены по маске пола), `RoomTemplate` (префабы `Prefabs/Rooms/room_*`, форма — тайлмап Shape), `PortalView` (`prop_portal`), `RoomController` получил `SetDoors`/`Cleared`, враги и сундук — дочерние комнаты.
- Заготовки собирает меню **FotF/Мир/Собрать серые заготовки этажа** (существующие префабы не перезаписывает).
- Портал — в последней зачищенной обязательной боевой комнате; через него — следующий этаж в той же сцене (герой с оружием и золотом сохраняется). После 4-го этажа — снова этаж 1 (босса ещё нет).
- Проверено ботом в Play: 5 комнат → двери → 2 волны → зачистка → портал → этаж 2; консоль чистая. Тесты 84/84.
- Заглушки (подпись «следующая поставка»): торговец, фонтан, святилище, клетка, арена мини-босса; секретные комнаты не строятся (нужны бомбы). Во всех боях пока скелеты из encounter_greybox; на старте лежит гвоздомёт (иначе нечего сплавлять, пока сундуки не дают оружие).

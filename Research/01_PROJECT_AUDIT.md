# 01. Аудит legacy-прототипа FrankenArms (тег `legacy-prototype`)

Источник: только `git show legacy-prototype:<path>`. Рабочая копия не читалась. Для Scrapheart: GDD-решения в `Research/GDD_QA_2026-11-24.md`.

## TL;DR

- Прототип = vertical slice на ~3 900 строк C#, **почти всё строится кодом в рантайме**. Сцены практически пустые. Как база для Scrapheart не годится; полезны идеи и ScriptableObject-схемы.
- Ценное: ScriptableObject-контент (Hero/RoomNode/Route/FusionRecipe/Grip/Barrel), `StatusEffectController`, `ProjectileBudget`, боссовые паттерны и телеграфы, баланс-таблица в `Docs/COMBAT_BALANCE.md`.
- Негатив: bootstrap-спавн всего в `Awake/Start`, `OnGUI` для всего UI, `PlayerPrefs` для всего, `FindAnyObjectByType` повсюду, хардкод чисел, нет asmdef/тестов/пулинга, ввод через `Keyboard.current` мимо Input Actions.
- Пакеты: убрать `learn.iet-framework`, `visualscripting`, `collab-proxy`, `timeline`, вероятно `2d.psdimporter`/`spriteshape`/`2d.tooling`; `com.unity.pipeline` оставить на время работы Unity CLI.

---

## 1. Структура

Всего 479 файлов в теге (включая .meta). Unity `6000.6.7f1`, Universal 2D / URP 2D Renderer (`ProjectSettings/ProjectVersion.txt`).

| Папка | Содержимое |
| --- | --- |
| `Assets/Scripts/` | 70 .cs, ~3 600 строк, один namespace `FrankenArms`, **без asmdef** |
| `Assets/Imported/ParentModules/Scripts/` | 4 .cs (WeaponGripSO, WeaponBarrelSO, RarityColors, ElementType), ~130 строк, **без namespace** (global) |
| `AgentScripts/` | 14 .cs, ~400 строк, editor-скрипты для агента (см. п.4) |
| `Assets/Resources/Content/` | 38 .asset: 10 Heroes, 3 Fusions, 24 Route Nodes + 1 Route |
| `Assets/Resources/ImportedModules/` | 9 .asset (Grip/Barrel: Pistol, Revolver, SMG, Sniper, Sword_Blade, Fire/Ice/Shock/Acid) |
| `Assets/Resources/Art/Generated/` | 3 PNG (hero-gunsmith 0.9 MB, enemy-roster 1.5 MB, dungeon-floor 1.7 MB) |
| `Assets/Resources/Audio/` | 8 WAV (Bonus, ButtonPress, Explosion, Hit, Shot, Whoosh, KitchenTheme 3 MB, cracklingfire 2 MB) |
| `Assets/Scenes/` | MainMenu, Lobby, Run, SampleScene |
| `Assets/Settings/`, `Assets/Welcome/` | URP/Renderer2D, InputSystem_Actions.inputactions, шаблонный Welcome-туториал (мусор) |
| `Docs/` | 12 .md + GDD_RU.html (64 KB) + 3 concept PNG (~8 MB) |
| `.agents/skills/` | 14 AI-скилов (~140 файлов, из них 45 в manage-sprite-atlas) |

C# по файлам (крупнейшие): `CombatRoom` 305, `EnemyBoss` 191, `FrankenArmsBootstrap` 190, `RuntimeSpriteFactory` 143, `HeroActiveAbilityController` 126, `PlayerCombat` 104, `PlayerHealth` 103. Всё остальное < 100 строк.

**Сцены** (YAML-строки / GameObject): MainMenu 263/2, Lobby 263/2, Run 218/1, SampleScene 352/2. То есть в сценах по 1-2 объекта (камера, свет); вся игра создаётся кодом.

**Ассеты, лицензии.**
- Third-party art packs **не импортировались** (`Docs/DECISIONS.md` 2026-11-15: «do not import third-party art packs yet»; `MERGE_AUDIT`: лицензии отдельно проверять). В `Assets/Imported` только код/SO из родительского проекта `2dRoguelike`.
- `Art/Generated/*` — AI-генерация по промптам из `Docs/VISUAL_PROMPTS.md`; в доке прямо «prototypes, not final art». Права на использование не оформлены, для релиза не рассчитывать.
- Audio: 6 SFX из «sibling Brackeys 2D Mega Pack» + `KitchenTheme.wav` (DECISIONS 2026-11-15). **Лицензия в доках не записана** (AGENTS.md требует записывать, но не сделано). Brackeys-паки обычно CC0, но это надо перепроверить перед релизом. `cracklingfire.wav` не используется в коде (0 ссылок). `KitchenTheme` из паков другой игры (кухня), для Scrapheart не подходит.
- 3 concept PNG и GDD HTML относятся к FrankenArms (manor/forge), к Scrapheart — только как референс настроения.

## 2. Код

### Системы

| Система | Файлы | Как устроено |
| --- | --- | --- |
| Bootstrap/flow | `FrankenArmsBootstrap`, `SceneFlow`, `MainMenuController`, `LobbyController` | `RuntimeInitializeOnLoadMethod` создаёт объект, который в `Awake` строит пол, стены, игрока, комнату, станции, камеру |
| Бой игрока | `PlayerCombat`, `PlayerMovement`, `PlayerDash`, `PlayerHealth`, `PlayerArmor`, `PlayerMana`, `Projectile` | кинематические `Rigidbody2D`, ручное движение снаряда, mana-за-выстрел для fused-оружия, armor регенерится через 3 с, dash с i-frames |
| Враги | `EnemyTarget` (HP), `EnemyChaser`, `EnemyShooter`, `EnemyContactDamage`, `EnemyProjectile`, `EnemyBoss` | компонентный подход; архетипы = набор компонентов на одном GO; босс с фазой при 50% HP, паттерны по этажам, телеграф 0.35 с |
| Статусы | `StatusEffectController` | Burning/Poison/Frozen/Marked/Cursed, таймеры float, один компонент для игрока и врагов |
| Оружие | `WeaponPartDefinition`, `WeaponFusionRecipe`, `FusionStation`, `ImportedModuleForgeBridge`, `WeaponGripSO`, `WeaponBarrelSO` | два параллельных слоя «fusion» (рецепты) и «Grip+Barrel» (импорт), склеены мостом; в Barrel заложены melee/blade, deflect, gamble, element, rarity |
| Комнаты/забег | `CombatRoom`, `RoomNodeDefinition`, `DungeonRouteDefinition`, `RoomDoor`, `RoomExit`, `RunProgress`, `RunStats`, `RunMode` | **нет процедурной генерации**: 24 авторских узла-ScriptableObject, выбор Safe/Risk/Forge после комнаты, волны по `roomIndex % 5` |
| Герои | `HeroDefinition` (10 assets), `HeroSelectionController`, `HeroActiveAbilityController` | SO со статами; активки (Knight/Mage/Mechanic) на `Q` |
| Мета | `MetaProgression`, `MetaPickup`, `Achievements`, `GameSettings` | всё в `PlayerPrefs` (50 обращений) |
| UI | `RunHUD`, `RunMinimapUI`, `RunSummaryUI`, `PauseController`, `FloatingDamageText` | **13 мест с `OnGUI`/IMGUI**, нет uGUI/UI Toolkit |
| Feel/Audio | `GameFeel` (hit-stop), `ScreenShake`, `ImpactBurst`, `PixelAnimator`, `GameAudio`, `AmbientAudio` | `GameAudio.Play("Shot")` через `Resources.Load` по строке |
| Прочее | `LocalizationService` (EN/RU/UA/PL/DE/ES), `PetDog`, `RunShopStation`, `RunShrineStation`, `ProjectileBudget` | |

### Качество

Антипаттерны (измерено grep по `Assets/Scripts`):
- **Спавн всего кодом.** `new GameObject(...)` 25 раз; `Instantiate(` 0 раз, то есть **нет ни одного префаба**. `FrankenArmsBootstrap.Awake` собирает игрока из 15 `AddComponent`; `CombatRoom.SpawnEnemy` собирает врага тем же способом (`CombatRoom.cs`, `SpawnEnemy`). Спрайты рисуются из строк-масок в `RuntimeSpriteFactory`. Результат: невозможно настроить в Inspector, нет превью, артист/дизайнер не может ничего править без кода. Сам `Docs/SESSION_HANDOFF.md` называет это «intentional», но это тупик для Scrapheart.
- **Связность.** `FindAnyObjectByType/GameObject.Find` 22 раза (`RunHUD.Start` ищет 6 компонентов; `GameObject.Find("Player")` в bootstrap). `CombatRoom` знает про дверь, награды, босса, мета, HUD, achievements, RunMode, hero selection (god-object, 305 строк). `Projectile` напрямую вызывает `GameAudio`, `ScreenShake`, создаёт impact-объект.
- **Статика и состояние.** 132 `static`; `RunStats`, `RunProgress`, `ProjectileBudget` статические, без сброса при domain reload off.
- **Хардкод.** Волны (`GetWavePositions`: массивы `Vector3` по `roomIndex % 5`), HP врагов (3/4/18), цвета, скорости, радиусы коллайдеров, `24` (число этажей) в `CombatRoom`/`RunSummaryUI`, магические id комнат (`roomIndex != 6`). Данные есть только у героев/узлов/рецептов; враги, волны, босс-паттерны, стоимости магазина, лут — в коде.
- **Input.** Используется Input System, но через прямой `Keyboard.current/Gamepad.current` (27 мест); готовый `InputSystem_Actions.inputactions` не подключён. Ребайнд невозможен.
- **Производительность.** Нет пулинга: каждый снаряд и impact = `new GameObject` + `Destroy`; вместо пула поставлен `ProjectileBudget` на 100 штук (костыль).
- **UI.** IMGUI с абсолютными `Rect`, нет масштабирования/навигации/локализации в виджетах.
- **Тесты/структура.** Нет asmdef, нет тестов (Test Framework в manifest, но тестов 0). `PlayerPrefs` вместо сейв-файла. `Welcome2DScript` и `SampleScene` — шаблонный мусор.
- **Положительное.** Единый namespace, `sealed`, небольшие компоненты (<100 строк в большинстве), `[CreateAssetMenu]` у всех SO, kinematic-подход к снарядам, выверенный баланс на бумаге.

## 3. Что переиспользовать как идеи, а что нет

### Брать как идеи/решения

1. **ScriptableObject-схема контента.** `RoomNodeDefinition` (floor, nodeType, threatBudget, rewardCredits, optional), `DungeonRouteDefinition`, `HeroDefinition`, `WeaponFusionRecipe`. Расширить на `EnemyDefinition`, `WaveDefinition`, `BossPatternDefinition` (то, что в прототипе забыто и захардкожено).
2. **Модульное оружие Grip + Barrel** (`Assets/Imported/ParentModules/Scripts/WeaponBarrelSO.cs`, `WeaponGripSO.cs`): `muzzlePoint`, `twoHanded`, `bulletsPerShot`, `spreadAngle`, element, rarity, `energyCost`, blade/deflect. Хорошая модель данных под «собери оружие из хлама» (синергия с темой Scrapheart). Не копировать код как есть: нет namespace, `Rarity` комментарий отсылает к «редакции 4 плана», gamble-режим не нужен без решения GDD.
3. **Статус-система** (`StatusEffectController.cs`): набор эффектов, `MoveSpeedMultiplier`, `Marked +25% урона`. Идея годится; реализовать как данные (`StatusEffectDefinition`) и общий интерфейс `IDamageable`.
4. **Boss design** (`EnemyBoss.cs`, `COMBAT_BALANCE.md`): фаза на 50% HP, паттерны по этажам (radial / crossfire / fan), **телеграф 0.35 с** до залпа, «контрплей движением или dash», принцип «target priority, not sponge damage».
5. **Боевая математика и принципы** (`COMBAT_BALANCE.md`): armor «ломается без перетекания в HP и возвращается через 3 с без урона», dash 0.16 с / 11 u/s / cd 1.05 / i-frames 0.20, contact i-frames 0.8 с, mana на выстрел только у найденного оружия (базовое бесплатно). Использовать как стартовые значения для тюнинга, не как истину.
6. **Game feel** (`GameFeel.cs`, `ScreenShake.cs`): hit-stop 35 мс (босс 50 мс), shake pulse, floating numbers тёплых цветов, muzzle flash. Перенести значения; реализовать через Cinemachine-less простой компонент.
7. **Route choice** Safe/Risk/Forge после комнаты (`CombatRoom.StartNextRoom(int)`): детерминированный выбор вместо random walk. Совпадает с решением «controlled graph + reviewed room templates» из `DECISIONS.md`.
8. **Daily seed** (`RunMode`): UTC-дата как seed, 2 модификатора, локальные рекорды без бэкенда. Дёшево и даёт реиграбельность (если подтвердит GDD).
9. **Структура Main Menu → Lobby → Run** (`SceneFlow`, `Docs/PLAYTEST.md`) и **acceptance-чеклист** из `PLAYTEST.md` как шаблон ручного плейтеста.
10. **Лимит снарядов + FPS-счётчик** как идея debug-метрик (но заменить пулом).
11. **Локализация** на 6 языков (`LocalizationService`) — идея; делать через `com.unity.localization` или таблицы SO, не словарём в коде.
12. **Процесс.** Правило «Console 0 errors после каждой правки», `SESSION_HANDOFF`, `DECISIONS` с датами и статусами, правило «не добавлять паки без записи лицензии».

### Отрицательный опыт (не повторять)

- **Bootstrap-спавн** (`FrankenArmsBootstrap.cs`, `CombatRoom.SpawnEnemy`, `Fire` в `PlayerCombat.cs`): всё делать префабами + SO, сцены собирать в Editor, рантайм только инстанцирует префабы (с пулом).
- **Параллельные слои и склейка**: `WeaponFusionRecipe` рядом с Grip/Barrel + `ImportedModuleForgeBridge` — следствие merge двух прототипов (`MERGE_AUDIT_2026-11-15.md`). Одна модель оружия с самого начала.
- **God-object `CombatRoom`**: разделить на `RoomController` / `WaveSpawner` / `RewardSpawner` / `RunState`, общение через события или SO-каналы.
- **`OnGUI` + `PlayerPrefs` + `Resources.Load` по строкам** (`GameAudio.Play("Shot")`): заменить на uGUI или UI Toolkit, JSON-сейв, ссылки на ассеты через SO/Addressables-lite.
- **Фичи вширь до готового ядра**: 6 языков, 10 героев, achievements, daily, hardcore, pet — при одном враге-«мишени» и одном оружии. Сначала fun-ядро комнаты, потом обвязка.
- **Прямое чтение `Keyboard.current`** вместо Input Actions.
- **Цвета/тинт как единственный способ различать врагов** (одни и те же спрайты + `renderer.color`): для читаемости нужны разные силуэты.
- **Магические числа в шаблонах**: `roomIndex % 5`, `24`, `6`.
- **AI-арт без лицензионной ясности** и звуки из паков другой игры без записи лицензии.
- **Разные пути/окружения**: прототип делался на Windows (`D:\game\...`), документы это тянут (`README`, `GITHUB_SETUP`). Для Scrapheart на Mac зафиксировать свои пути/CLI.

## 4. Docs/, AgentScripts/, .agents/skills/

### Docs/

| Документ | Ценное | Устарело |
| --- | --- | --- |
| `DECISIONS.md` | Формат лога решений (дата/решение/причина/статус); решения про Input System, SO-контент, controlled graph | Привязано к FrankenArms; решение «Pipeline/CLI на Windows» неактуально |
| `COMBAT_BALANCE.md` | Таблица комнат, боевая математика, статусы, boss, критерии справедливости (п.3) | Значения под slice из 6 комнат; тематика |
| `FrankenArms_GDD_RU.html` + 3 PNG | Только как референс структуры GDD (heroes, floors, meta) | **Устарел полностью**: GDD Scrapheart теперь в `GDD_QA_2026-11-24.md` |
| `PLAYTEST.md` | Шаблон acceptance-чеклиста, схема управления | Клавиши и сцены |
| `ACTIVE_WORK_PLAN.md` | Пример порядка vertical slice (feel → room loop → weapon → visual → mini-run) | Исполнен, не актуален |
| `SESSION_HANDOFF.md` (119 строк) | Хроника: что и как проверялось; видно реальный объём сделанного | Хронология, не спецификация; часть «Next steps» уже сделана |
| `MERGE_AUDIT_2026-11-15.md` | Урок: не мержить два прототипа; список выбранного | Устарел |
| `PROJECT_CONTEXT.md` | Шаблон контекст-дока для агента | Версии/прокси/Windows |
| `VISUAL_PROMPTS.md` | Правила импорта пиксель-арта (Point filter, no mipmaps, alpha) и палитра; полезны, если Scrapheart будет pixel art | Сами промпты под manor/gunsmith |
| `AI_UNITY_RESEARCH_AND_CHECKLIST_RU.md` | Безопасный цикл: маленькая правка, компиляция, Play Mode, Console | Список видео, «Выбери варианты» недописан |
| `WORKSPACE_GUIDE_SOURCE.md` | Правила для агента (.gitignore Unity, LFS для больших бинарников, без секретов) | Ссылка на `REPOSITORY.md` (нет файла); «Codex», Mac/Windows |
| `GITHUB_SETUP.md` | Ничего | Целиком (PowerShell, старый remote `Zishwhwhw/FrankenArms`) |

Также `AGENTS.md`, `README.md` в корне: содержат старые правила и пути; переписать под Scrapheart (пункт про лицензии паков и Git LFS сохранить).

### AgentScripts/ (14 файлов)

Editor-скрипты с `public static void Main()`, вызываются через Unity CLI (`unity exec`-стиль). Три группы:
- **Генерация ассетов/сцен**: `CreateGameScenes`, `CreateHeroAssets`, `CreateRouteAssets`, `CreateFusionAssets`, `AssignSpecialRouteNodes`, `ArtImportAndSlice` (настройка импортёра и нарезка спрайтов через `ISpriteEditorDataProvider`).
- **Тест-хелперы**: `JumpToBoss`, `AdvanceRuntimeRoom`, `AdvanceForgeRoute` (рефлексией лезет в private-поля `CombatRoom`), `SelectHeroForTest`, `ResetTestProgress`.
- **Диагностика**: `InspectScene`, `CheckGeneratedArt`, `RefreshScripts`.

Нужны ли: **нет, как есть** (завязаны на классы FrankenArms, рефлексия, лежат вне `Assets/Editor`). Идея полезна: детерминированные editor-скрипты для создания SO-ассетов и debug-команды (jump to boss, reset save). Для Scrapheart делать нормальный `Assets/Editor/` + debug-меню, а не рефлексию.

### .agents/skills/ (14 скилов, формат Codex/OpenAI: есть `agents/openai.yaml`)

- Unity-инструменты: `unity-cli` (управление Editor из CLI, build/test, playmode-verification-loop), `sprite-editor` (нарезка текстур; скрипты .cs), `manage-sprite-atlas` (45 файлов), `2d-pixel-perfect`, `create-2d-physics`, `audio-setup-mixers`. Скопированы из Unity-источников; в каждом `SECURITY.md`.
- Геймдев-роли `gamedev-*` (unity-engineer, combat-designer, game-feel-designer, hud-engineer, level-layout-designer, meta-progression-designer, audio-designer, technical-artist, qa-verifier, reviewer): универсальные промпты-роли. В примерах «guild war reward service», «match3 board» — мобильный F2P-уклон, для roguelike мало релевантны.

Вывод: это настройки для Codex, а не часть игры. У пользователя Claude Code, скилы в `~/.claude/skills` и пакете `gamedev` (`skillpack`). **В репозиторий Scrapheart не переносить**; при необходимости использовать `unity-cli`/`2d-pixel-perfect` через skillpack. Удалить из проекта.

## 5. Пакеты Unity (`Packages/manifest.json`)

| Пакет | Версия | Решение | Комментарий |
| --- | --- | --- | --- |
| `com.unity.render-pipelines.universal` | 17.6.0 | **Нужен** | URP 2D Renderer, Light2D |
| `com.unity.inputsystem` | 1.19.0 | **Нужен** | Использовать через Input Actions, не `Keyboard.current` |
| `com.unity.ugui` | 2.6.0 | **Нужен** | HUD/меню (либо UI Toolkit; выбрать один) |
| `com.unity.2d.sprite` | 1.0.0 | Нужен | Sprite Editor/Atlas |
| `com.unity.2d.tilemap` | 1.0.0 | Нужен | Комнаты по шаблонам |
| `com.unity.2d.tilemap.extras` | 9.0.0 | Нужен | Rule Tile и пр. |
| `com.unity.2d.animation` | 16.0.0 | Условно | Только для skeletal-анимации; для frame-by-frame pixel art не нужен |
| `com.unity.2d.aseprite` | 6.0.0 | Условно | Нужен, если арт в .aseprite |
| `com.unity.2d.psdimporter` | 15.0.0 | Убрать | PSD не используется |
| `com.unity.2d.spriteshape` | 16.0.0 | Убрать | Для top-down комнат не нужен |
| `com.unity.2d.tooling` | 4.0.0 | Условно | Редакторские 2D-инструменты; можно убрать |
| `com.unity.test-framework` | 1.8.0 | Нужен | Тестов не было, но для логики (баланс, сейв) нужен |
| `com.unity.timeline` | 6.6.0 | Убрать | Не использовался |
| `com.unity.ide.rider` / `ide.visualstudio` | 3.0.38 / 2.0.26 | Оставить нужный | Mac: Rider или VS Code; второй убрать |
| `com.unity.visualscripting` | 1.9.11 | **Убрать** (по плану) | Не используется |
| `com.unity.learn.iet-framework` | 6.0.4 | **Убрать** (по плану) | Шаблонный туториал-фреймворк; вместе с `Assets/Welcome/` и `ProjectSettings/Packages/com.unity.learn.iet-framework` |
| `com.unity.collab-proxy` | 2.12.4 | **Убрать** (по плану) | Unity Version Control (Plastic); используется Git |
| `com.unity.pipeline` | 1.0.0-pre.1 | См. ниже | |

**Что такое `com.unity.pipeline`.** Не render pipeline. Это пакет-мост для Unity CLI: по `Docs/DECISIONS.md` (2026-11-11) и `PROJECT_CONTEXT.md` он ставился для «live Editor control» из терминала агента (`unity status`/`unity exec`); в `packages-lock.json` тянет `newtonsoft-json`, `mono-cecil`, `uielements`, `screencapture`. Статус `pre`, нужен только для remote-управления Editor. Решение: **оставить, если будешь управлять Editor через Unity CLI** (на Mac это снизит число ручных проверок); иначе убрать. В релизный билд он не нужен, на геймплей не влияет.

Дополнительно: `Packages/packages-lock.json` обновится сам; после удаления проверить `ProjectSettings/Packages/com.unity.learn.iet-framework` и `Assets/Welcome/`. Рекомендуемые добавления для Scrapheart (по необходимости): `com.unity.cinemachine` (камера/shake), `com.unity.localization` (если нужна локализация), Addressables не нужен на таком масштабе.

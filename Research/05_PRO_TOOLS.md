# 05 — Pro-инструменты для 2D Unity (Scrapheart)

Дата проверки: 2026-11-26. Ссылки проверены curl (HTTP 200), если не указано иное. Цены — только то, что видно на страницах; остальное «не проверено».
Контекст: Unity 6 2D URP, ИИ-агенты (Claude Code на Mac, Codex, Gemini CLI). Unity на Mac не найден в /Applications. Локально есть: python3 + Pillow 11.1, uv, node, git. НЕТ: aseprite, git-lfs.

Условные «CLI/агент»: Да = агент управляет без GUI; Частично = через файлы/код/Editor-bridge; Нет = только руками.

## Сводка

| Инструмент | Цена / лицензия | CLI/агент | Вердикт |
|---|---|---|---|
| Aseprite | платный (цена на buy-странице не отображена — не проверено); исходники на GitHub, EULA запрещает распространять бинарники | Да (CLI + Lua) | Брать |
| LibreSprite | бесплатно, GPL (форк старого Aseprite) | Частично (CLI, Lua слабее) | Запасной вариант |
| Tiled | бесплатно, GPL-редактор; формат TMX/JSON | Частично (CLI export, JSON) | Нет (Unity Tilemap) |
| LDtk | бесплатно, MIT | Частично (JSON-проект) | Позже/по необходимости |
| Unity Tilemap | входит в Unity | Частично (через код/MCP) | Брать |
| Unity Test Framework | пакет Unity, бесплатно | Да (batchmode) | Брать |
| Profiler | в Unity | Частично (MCP умеет profile) | Брать, по мере надобности |
| Odin Inspector | €50 one-time + €25/seat (Personal, доход <$200K) | Нет | Нет (не нужно) |
| NaughtyAttributes | бесплатно, MIT (не проверено в LICENSE, репо 200) | n/a (код) | Брать, если нужны атрибуты |
| DOTween | бесплатно (Pro — платный, цена не указана) | n/a (код) | Брать |
| Cinemachine 2D | пакет Unity, бесплатно | n/a (код/prefab) | Брать |
| Feel | платно (Asset Store, цена не проверена) | n/a | Нет, делать свой минимальный game feel |
| Git + LFS | бесплатно | Да | Брать |
| GameCI | бесплатно, MIT | Да | Позже |
| Unity Localization | пакет Unity, бесплатно | Частично | Позже (заложить ключи строк сразу) |
| FMOD / Wwise | FMOD — условия на странице не раскрыты (не проверено); Wwise — страница 403, не проверено | Нет/частично | Нет, встроенный аудио |
| Steamworks.NET / Facepunch | бесплатно, MIT (не проверено в LICENSE) | n/a (код) | Позже, перед Steam-релизом |

## Детали

### Aseprite (https://www.aseprite.org, https://www.aseprite.org/docs/cli)
- Что: стандарт для пиксель-арта и анимации; теги, слои, slices, палитры.
- Зачем: единственный «источник правды» для .aseprite файлов героя 32x32, 4 направления, теги анимаций.
- Цена: платный (Windows/macOS/Ubuntu, Steam-ключ опционально; страница buy цену не показывает — не проверено). Есть Trial. Сборка из исходников для себя допускается EULA в GitHub (страницу EULA получить не удалось, 503 — не проверено).
- CLI: Да. Подтверждено на docs/cli: `-b/--batch`, `--sheet`, `--data` (json-hash/json-array), `--scale`, `--palette`, `--script` + `--script-param`, `--tag`, `--layer`, `--frame-range`, `--trim`, `--filename-format`. Пример: `aseprite -b hero.ase --sheet hero.png --data hero.json --format json-array --list-tags`.
- Unity: есть импортёр .aseprite (сторонние пакеты; не проверено — выбирать при настройке). Безопасный путь: экспорт PNG+JSON из CLI и свой скрипт нарезки.
- Рекомендация: БРАТЬ владельцу (разовая покупка) — нужен как ручной редактор и батч-экспортёр. Агенты без него могут жить на Pillow (см. 06).

### LibreSprite (https://libresprite.github.io)
- Бесплатный форк Aseprite 1.1.x, GPL. Слабее по CLI/Lua и формату. Рекомендация: только если Aseprite недоступен.

### Tiled (https://www.mapeditor.org) vs LDtk (https://ldtk.io) vs Unity Tilemap
- Tiled/LDtk: внешние редакторы уровней, JSON-вывод, нужен импортёр в Unity. Для процедурного roguelike (рум-генерация кодом) внешний редактор уровней почти не нужен.
- Unity Tilemap + Rule Tile / 2D Tilemap Extras: нативно, URP-совместимо, агент может заполнять через C# (и через MCP).
- Рекомендация: Unity Tilemap — БРАТЬ. LDtk — только если решите вручную рисовать ключевые комнаты/боссов (тогда позже). Tiled — нет.

### Unity Test Framework (https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/index.html)
- EditMode/PlayMode тесты, NUnit. Запуск из CLI: `Unity -batchmode -runTests -testPlatform EditMode -testResults out.xml` (CLI Unity на Mac — после установки).
- Рекомендация: БРАТЬ. Тесты логики (урон, лут, сиды генерации) — основной способ агенту проверять работу.

### Profiler
- Unity Profiler/Memory Profiler/Frame Debugger встроены. Агент напрямую GUI не читает; CoplayDev MCP заявляет «profile» среди возможностей (см. 06). Рекомендация: брать по необходимости, не на старте.

### Odin Inspector (https://odininspector.com/pricing)
- Personal: €50 one-time + €25 за seat, если доход/финансирование <$200K за 12 мес.; Enterprise €235/seat/год. Рекомендация: НЕТ. Для агентов кодом инспекторы не нужны; платная зависимость.
### NaughtyAttributes (https://github.com/dbrizov/NaughtyAttributes)
- Бесплатные атрибуты инспектора (ShowIf, Button, Foldout...). Лицензию в репо не проверял. Рекомендация: БРАТЬ при необходимости, дёшево; не обязательно.

### DOTween (https://dotween.demigiant.com)
- Бесплатный и open-source; Pro — платный, цена на странице не указана (не проверено). Рекомендация: БРАТЬ (free версия): твины UI, hit-flash, тряска.

### Cinemachine 2D (https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/index.html)
- Пакет Unity (v3.x), Confiner 2D, Impulse (screenshake), Position Composer. Рекомендация: БРАТЬ. Pixel Perfect Camera из 2D Pixel Perfect (URP) — отдельно, нужен для 640x360 база.

### Feel (https://feel.moremountains.com)
- Asset Store, платно, цена на странице не указана (не проверено); 2D поддерживает. Рекомендация: НЕТ на старте — screenshake/hit-stop/flash делаются 100–200 строками + DOTween + Cinemachine Impulse; Feel — тяжёлая зависимость для агента (инспектор-ориентирован).

### Source control / LFS (https://github.com/git-lfs/git-lfs)
- Git + Git LFS (на машине git-lfs НЕТ: `brew install git-lfs`). Unity: Force Text serialization, Visible Meta Files, .gitignore (Library/, Temp/, Logs/, UserSettings/, Builds/). LFS-трекинг: *.png *.aseprite *.wav *.ogg *.mp3 *.psd. GitHub free: LFS-квота ограничена (размер/трафик — не проверено); для пиксель-арта хватит. Рекомендация: БРАТЬ сразу.

### GameCI (https://game.ci, https://github.com/game-ci/unity-builder)
- Бесплатно, MIT; GitHub Actions: unity-test-runner + unity-builder. Требует Unity license secret (Personal-активация — детали не подтверждены страницей). Рекомендация: ПОЗЖЕ, когда будут тесты и стабильный билд.

### Unity Localization (https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/index.html)
- String/Asset tables, смарт-строки. Рекомендация: ПОЗЖЕ; сразу хранить строки по ключам в одном месте, чтобы миграция была дешёвой (RU/EN).

### FMOD (https://www.fmod.com/licensing) / Wwise (https://www.audiokinetic.com/en/wwise/pricing/ — 403, не проверено)
- Условия FMOD Indie на странице не раскрыты (не проверено). Для SFX-roguelike достаточно встроенного AudioSource + AudioMixer. Рекомендация: НЕТ (встроенный).

### Steamworks.NET (https://github.com/rlabrecque/Steamworks.NET) / Facepunch (https://github.com/Facepunch/Facepunch.Steamworks)
- Обёртки Steamworks SDK (achievements, cloud, overlay). Steam: разовый взнос за приложение (сумма не проверена). Рекомендация: ПОЗЖЕ; Steamworks.NET (ближе к официальному API). Агент может писать код, но нужен Steamworks-аккаунт владельца.

## Минимум на старт
Git+LFS, Unity Tilemap, Cinemachine, 2D Pixel Perfect, DOTween (free), Unity Test Framework, Aseprite (покупка владельцем).

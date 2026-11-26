# Scrapheart — статьи, доки и посты про разработку игр с AI-агентами (26.11.2026)

Метод: каждая ссылка открыта через WebFetch 26.11.2026 (HTTP 200, текст прочитан); для редиректов указан конечный адрес. Ссылки, которые не открылись (403 или обрезанный текст), в список не включены. Тезисы — из прочитанного текста; где автор писал «приблизительно» или это мнение, так и помечено. Правила из RULES_DIGEST.md не повторяются.
Не включены (не удалось прочитать тело): GamesRadar «25 devs avoid generative AI» (обрезано), Creative Bloq про HaZ Dulull (обрезано), gamedev.net devlog 9 (403), CG Channel про Unity AI (403).

---

## A. Steam: раскрытие AI-контента (актуальная версия)

### 1. Steamworks Documentation — Content Survey (официальный текст)
https://partner.steamgames.com/doc/gettingstarted/contentsurvey

- Раздел про generative AI касается контента, который **поставляется с игрой и потребляется игроками**. Цитата: «Efficiency gains through the use of these tools is not the focus of this section.»
- **Pre-Generated**: любой контент (арт, код, звук и т.д.), созданный AI-инструментами во время разработки и вошедший в игру. Нужно подробно описать использование.
- **Live-Generated**: контент, создаваемый AI во время работы игры. Те же правила плюс обязательное описание guardrails против незаконного контента.
- Valve оценивает результат AI так же, как не-AI контент; по Distribution Agreement игра «will not include illegal or infringing content».
- Live-Generated Adult Only Sexual Content Valve выпускать «не готова на данный момент».

Применимость: Scrapheart с pre-made AI-артом/звуком (если он попадёт в финал) нужно заявлять в Content Survey; AI только как инструмент (Claude Code, Codex, Gemini для кода) по формулировке Valve в фокус раздела не входит. Лайв-генерации у нас нет — guardrails не нужны. Это моя трактовка текста, не юридическая консультация.

### 2. GamingOnLinux — Valve tweak Steam AI disclosure form (17–18.01.2026), плюс PCGamesN
https://gamingonlinux.com/2026/01/valve-tweak-steam-ai-disclosure-form-for-developers-to-clarify-its-for-content-consumed-by-players
Дубль по сути: https://www.pcgamesn.com/steam/new-gen-ai-disclosure-form (17.01.2026)

- Изменение формулировки января 2026: форма — про AI в контенте, который игроки видят и слышат; цитата: «content such as artwork, sound, narrative, localization, etc.»
- Остаются два раздела (pre-generated и live-generated); сохраняется требование описать AI-контент на странице Steam (store page, community assets и маркетинг входят в охват по PCGamesN).
- Инструменты-помощники в среде разработки раскрывать не нужно (efficiency).
- Открытый вопрос: **AI-код** в примерах не указан; GamingOnLinux прямо пишет, что комментаторы не согласны — считать неоднозначным.
- Для live-AI: guardrails, и система жалоб игроков на нелегальный/нарушающий авторские права вывод.

Применимость: решение по Scrapheart до Steam-страницы: что считаем «AI-контентом, который видит игрок» (спрайты, музыка, SFX, текст). Если арт финальный рисуется/правится вручную по art bible, а AI даёт только референсы — раскрытие, вероятно, минимально; решение записать в `Docs/DECISIONS.md`. Перед отправкой в Steam перечитать официальную страницу (она меняется).

---

## B. Unity

### 3. Unity blog — Unity AI Sprite Generator (28.04.2026)
https://unity.com/blog/unity-ai-sprite-generator

- Генератор встроен в Editor: Create > 2D > Generate Sprite; модель, prompt, negative prompt, seed, размеры, референсы в шести режимах (image, style, composition, depth, line art, feature).
- Постпроцесс: Remove BG, Upscale, **Pixelate**, Recolor, Inpaint; результат в `/GeneratedAssets`, assets получают метку Unity AI.
- Spritesheet: из картинки (без фона) и motion prompt получается 5-секундное видео → 4x4 = 16 кадров, можно сохранить .anim.
- Доступно в Unity 6; тулзы в beta, есть бесплатный триал. Unity прямо называет результат «placeholder assets» для прототипа и просит заменять человеческим артом перед релизом.
- Кадры содержат фон, если не сделать Remove BG заранее.

Применимость: у нас Unity 6 — годится для быстрых серых коробок и placeholder, но не как источник финальных спрайтов (нет гарантии палитры Endesga 32 и сетки PPU 32). Вывод через Pixelate нужно прогнать через собственный snap + палитру.

### 4. Unity Docs — Unity Credits
https://docs.unity.com/ai/credits/credits-about

- AI-функции Unity тратят кредиты организации; перед запуском резервируется максимум, остаток возвращается; неудачные и заблокированные фильтром задачи не списываются.
- Примеры цен: стиль-модели 5–14 (обычно около 8), GPT Image около 18, Remove background/Pixelate 1–3, sound effect около 26 (не зависит от длины), music 6–13.
- Каждый вариант — отдельная задача: 4 картинки примерно в 4 раза дороже одной. Кредиты не переносятся на следующий месяц.
- Документ предупреждает, что цены меняются без уведомления; лимит месячной подписки в странице не указан (смотреть Dashboard).

Применимость: если используем Unity AI для placeholder-звука/спрайтов — нужен бюджет кредитов; сравнить с PixelLab/Retro Diffusion по цене (06_ASSET_PIPELINE_MCP.md). Правила лицензирования проверять отдельно и записать в DECISIONS.md.

---

## C. Гайды по агентам для кода (Anthropic / OpenAI)

### 5. Anthropic Engineering — Effective harnesses for long-running agents (26.11.2025)
https://anthropic.com/engineering/effective-harnesses-for-long-running-agents

- Проблема: агент работает дискретными сессиями без памяти; compaction одного не хватает для больших проектов.
- Initializer-сессия создаёт `init.sh`, progress-лог, первый git-коммит и **feature list в JSON**, где все фичи изначально failing; кодер меняет только поле `passes`. JSON выбран, потому что его реже «переписывают» модели, чем Markdown.
- Progress-файл + описательные коммиты позволяют новой сессии быстро понять состояние и откатить плохое.
- Типичные провалы: агент объявляет победу рано, оставляет недокументированные/сломанные состояния; unit-тестов и `curl` мало — помогает e2e через браузерную автоматизацию «как человек».

Применимость: ложится на наши `Docs/WORKBOARD.md` и критерии «готово» (B5, B14); для Unity аналог e2e — запуск сцены + скриншот Game view, а feature list с `passes` можно завести для вертикального среза.

### 6. Anthropic Engineering — Harness design for long-running application development (24.03.2026)
https://www.anthropic.com/engineering/harness-design-long-running-apps

- Разделение generator и evaluator: отдельный скептичный оценщик настроить проще, чем заставить генератор критиковать себя.
- Planner превращает 1–4 предложения в спеку, фокусируясь на результатах, а не на технических деталях, чтобы ошибки спеки не каскадировались.
- В примере «retro game maker»: одиночный запуск — 20 минут и $9, но ключевая функция не работала; полный harness — 6 часов и $200, зато play mode рабочий (хотя физика грубая).
- Критерии оценки (design quality, originality, craft, functionality) делают субъективный вкус проверяемым; вес «originality» помогает уйти от «AI slop».

Применимость: для Scrapheart — отдельный «reviewer»-агент с чек-листом по art bible и GDD §2; стоимость harness заметная, использовать выборочно (игровые системы вертикального среза, не вся игра).

### 7. Anthropic Engineering — Effective context engineering for AI agents (29.09.2025)
https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents

- Context rot: чем длиннее контекст, тем хуже вспоминание; контекст — ограниченный ресурс.
- Compaction (резюме + новое окно), structured note-taking во внешний файл (NOTES.md, to-do) и sub-agents с чистым контекстом, возвращающие сводку 1000–2000 токенов.
- Just-in-time retrieval: хранить пути/ссылки и подгружать по необходимости.
- Цель — «smallest set of high-signal tokens».

Применимость: AGENTS.md должен оставаться коротким с указателями на Docs/*; длинные сессии по Unity разбивать (одна фаза — одна сессия, см. п. 11).

### 8. Claude Code — Best practices (docs; старый адрес /engineering/claude-code-best-practices редиректит сюда)
https://code.claude.com/docs/en/best-practices

- Три типичных причины провалов: Claude не видит факты (процесс, ограничения), не может проверить результат, или нужно принудительно соблюдать правила.
- «Give Claude a check to run»: просить **доказательства** (команда и вывод, скриншот), `/verify` запускает приложение и смотрит результат — ловит то, что пропускают тесты.
- «Keep CLAUDE.md short», правила для части кодовой базы выносить отдельно, повторяемые процедуры — в skills.
- Обязательные правила — через hooks (например, Stop hook проверяет перед завершением) и permission rules/sandbox, а не только текстом.
- Вспомогательные разделы: brainstorm перед реализацией, subagents для параллельной независимой работы, scheduled/parallel sessions.

Применимость: совпадает с нашими B3/B14: Stop hook можно использовать для обязательного «компиляция + Console чистая» перед «готово» (не внедрять без решения владельца).

### 9. OpenAI — Codex best practices (конечный URL после редиректа)
https://learn.chatgpt.com/guides/best-practices (старый: developers.openai.com/codex/learn/best-practices → 308)

- Структура запроса: goal, context, constraints, «done when».
- Plan mode для сложных/неоднозначных задач (`/plan` или Shift+Tab).
- AGENTS.md — «open-format README for agents»: держать коротким и точным, добавлять правила после повторных ошибок.
- Агент должен писать/обновлять тесты, запускать проверки и ревьюить diff; повторяющиеся процессы — в `SKILL.md`.
- Параллельная работа: subagents + git worktrees, чтобы задачи не конфликтовали.

Применимость: наш AGENTS.md уже следует этому; добавить в шаблон задач «done when» (уже B5). Worktrees полезны при параллельной работе Claude/Codex (WORKBOARD).

---

## D. Девлоги и посты разработчиков

### 10. Void Balls — How One Dev + AI Built a Roguelite in 10 Days (itch.io devlog)
https://loopedingames.itch.io/void-balls/devlog/1464676/void-balls-is-live-how-one-dev-ai-built-a-roguelite-in-10-days (точная дата в тексте не указана; билды от 20.03.2026)

- Unity 6 + Claude Code (Opus) + **Unity MCP**: Claude видит сцену, значения компонентов, запускает play mode и отлаживает по реальному состоянию («Claude sees the game and fixes it»).
- 8 специализированных агентов (дизайн, QA и т.д.), плагин Superpowers для brainstorm/plan до кода, GDD собран структурированными сессиями.
- Арт: 33 спрайта через Replicate API с **заблокированной палитрой из 7 цветов**; звук и музыка — ElevenLabs (9 SFX, 3 трека).
- Результат: около 29 000 строк C#, 173 скрипта, 88 тестов, ~$50/мес на AI-инструменты.
- Урок автора: направление (дизайн, ощущения) оставил себе, ограничения (две кнопки управления, узкая палитра) помогли консистентности.

Применимость: 1) Unity MCP — прямой аргумент для 06_ASSET_PIPELINE_MCP.md; 2) жёсткая палитра у нас уже есть (Endesga 32); 3) 10 дней относятся к маленькой игре с 33 спрайтами, а не к роугалайку со сплавом оружия.

### 11. Project Aegis DevLog — Unity 6 игра только через Claude Code + MCP for Unity (17.03.2026)
https://cxyda.github.io/ProjectAegisDevLog

- 20 дней, 309 сессий, ~20–22 часа; 328 файлов C#, 42 628 строк, 589 тестов, ноль ручных действий в Unity.
- `CLAUDE.md` + ADR (Architecture Decision Records) дали агенту правила проекта; каждый этап: plan → review → implement; Planner/Challenger skill для планов.
- Чистые слои (assembly definitions, interfaces, DI) помогали агенту искать проблему, монолитные классы — «теряется».
- Самое помогающее: **UI Toolkit (UXML/USS)**, потому что агент читает/правит текстовые layout без кликов в Inspector; data-driven ScriptableObject-каталоги; автоматический quality gate; цикл «press Play and fix». Opus лучше Sonnet для реализации.
- Провалы: агент молча терял фичи, чинил симптомы, расширял скоуп, «context rot» создавал вымышленные проблемы; sub-agent на каждую фазу оказался слишком шумным. Урок: одна фаза — одна сессия, diff реализации против плана.

Применимость: самый близкий к нам кейс (Unity 6 + Claude + ScriptableObject + ADR). Противоречит нашему правилу «сцены и префабы, не код» в части UI: UI Toolkit — вариант для текстовых UI, но решение об UI-стеке — к владельцу/ARCHITECTURE.md.

### 12. Simon Willison — Battle Drones (20.10.2026)
https://simonwillison.net/2026/Oct/20/battledrones/

- Claude Code for Web получил API-ключ OpenAI в переменной окружения и сам генерировал спрайты через Image API.
- v1: базовые спрайты; v2: спрайты для всех типов техники; v3 (Opus): спрайты перегенерированы и «намного симпатичнее», вместе с новой местностью и зданиями.
- Вывод: Claude «reasonably competent» в написании промптов для image-моделей и умеет смотреть на результат и итерировать промпт.
- Конкретные промпты в посте не приведены.

Применимость: позволяет автоматизировать цикл «prompt → картинка → оценка → правка» агентом; мы добавим к нему обязательный snap/палитру (см. 02_). Источник — персональный блог автора, материал небольшой.

### 13. DEEP YELLOW — Every Texture is a Python Script (15.02.2026)
https://itch.io/devlog/1382913/every-texture-is-a-python-script.amp

- Все текстуры и спрайты игры — скрипты на Python (PIL, NumPy), а не нарисованы и не сгенерированы AI.
- Причины: бесшовный tiling (modulo wrapping), детерминизм по seed, возможность править параметрами.
- Конвейер: creator-агент пишет и запускает скрипт, затем два критика (один сравнивает с требованиями, второй «слепой» — видит только картинку); 1–4 итерации. Оркестрирует Claude Code, сам Claude не пишет скрипты.
- Урок: «blind critic» — хороший тест: если человек без контекста не понимает, что на картинке, текстура не работает.

Применимость: для тайлов/фактур (ржавый металл, швы, шум) процедурная генерация даёт единый масштаб и палитру без AI-image-моделей, и не требует AI-раскрытия в Steam так же остро (вопрос к решению владельца). Подходит для фонов и декоров, не для персонажей.

### 14. Show HN — «I AI-coded a tower defense game and documented the whole process» (HN item 44463967, зеркало)
https://hn.svelte.dev/item/44463967 (оригинал: news.ycombinator.com/item?id=44463967)

- Phaser.js, Beginner's Jam Summer 2025, ~25–30 часов; арт — бесплатные ассеты itch.io, звук — freesound.org; код и промпты опубликованы.
- Урок автора: самое сложное — tutorial system; архитектуру нужно указывать точнее; десятки отдельных разговоров вместо одного длинного (деградация контекста); без source control первые дни пришлось восстанавливать историю по чату.
- Из комментариев: первую фичу написать вручную, чтобы задать паттерны; сначала one-shot базовую функциональность, потом маленькие итерации; PRD/спеки и тесты как измеримое направление; промпты хранить в репозитории; домен-эксперты получают больше пользы.
- Жалобы: агент пишет больше функций, чем нужно; мобильный ввод и CSS-вёрстка агенту плохо даются.

Применимость: подтверждает правила про git с первого дня, маленькие итерации и промпты в репо (уже приняты); HN-источник — мнения, не измерения.

### 15. DEV Community — «What I Learned Trying to Make a Game with AI Only» (08.07.2026)
https://dev.to/junhee916/what-i-learned-trying-to-make-a-game-with-ai-only-half-the-truth-about-claude-for-game-dev-3j2p

- AI-картинки для 4-кадрового walk cycle: одежда, пропорции и лицо «дрожат» между кадрами; автор отказался от AI-анимации персонажа в пользу готовых CC0-спрайтов.
- Статичные/процедурные эффекты (барьеры, искры) писал вручную; ограничение скоупа (spectator auto-battler вместо платформера) убрало сложную анимацию и физику.
- Совет: AI усиливает навыки, не заменяет базу; если идеальный путь закрыт — следующий лучший вариант и запись выводов.

Применимость: подтверждает риск №1 для Scrapheart — AI-анимации персонажей/врагов; наш fallback — 3D→2D или ручная доработка (см. 02_). Один автор, один проект.

---

## Дополнительно (прочитаны, коротко)

- **Practical Workflow for AI Pixel Art (Gemini) — 2D Animation Experiment**, 14.03 (год в тексте не указан) — https://dev.to/kenjidev9662/practical-workflow-for-ai-pixel-art-gemini-2d-animation-experiment-21le . Nano Banana 2, до 100 регенераций в день; продолжение в том же чате держит консистентность; сложные комбо-атаки разбить на отдельные шаги; прозрачный PNG AI не выдаёт — генерировать на чёрном/зелёном и удалять фон вручную. Unity-настроек в статье нет.
- **How to Make Pixel Art with AI for Games — Pipeline Guide for 2026** (GamineAI, 18.05.2026) — https://dev.to/gamineai/how-to-make-pixel-art-with-ai-for-games-pipeline-guide-for-2026-789 . Перед генерацией написать spec sheet (масштаб камеры, направление света, политика обводки, дизеринг, alpha); база 16/24/32 px на высоту гуманоида, палитра 16–32 цвета до массовой генерации; тест в движке на «занятом» фоне 1080p: если силуэт «дрожит» от сдвига на 1 px — чинить до анимации. Конкретных AI-инструментов не называет. Это совпадает с нашей art bible (07_ART_BIBLE_SPEC.md).
- **AI Game Assets: Pixel Art & 3D Models for Unity** (Wayline, 18.11.2025) — https://www.wayline.io/blog/ai-game-assets-pixel-art-3d-models-unity . Unity import: Point filter, минимальная/без компрессии для маленьких спрайтов, нарезка в Sprite Editor, PPU под плотность пикселя; AI-модели 3D часто приходят с неверным масштабом/пивотом и слишком плотной сеткой.
- **Space Goose** (DEV, 02.10.2025) — https://dev.to/smirnovw/space-goose-a-pet-project-with-ai-unity-and-aseprite-1gia . Unity + Aseprite: AI даёт концепты, спрайты перерисованы вручную; агент для кода предложил ~100 строк там, где хватало trigger-exit проверки; вывод: без базовых знаний AI «будет мешать».

---

# Главное для Scrapheart

1. Steam: контент, который видит/слышит игрок, и AI-сгенерированный, надо раскрывать; инструменты разработки по тексту Valve — нет, AI-код спорный (п. 1, 2). Решение записать в DECISIONS.md до релизной страницы.
2. Для Unity-агентов: MCP-доступ к Editor + ADR + feature list/progress файл + проверка по запуску (п. 5, 10, 11).
3. AI-ассеты: палитра и размер — до генерации; результат — через snap/нормализацию; Unity сама называет свой генератор «placeholder» (п. 3, 10, доп. GamineAI).
4. Анимация персонажей с AI — главный риск; альтернативы — video→frames с нормализацией, 3D→2D или процедурный/ручной путь (п. 13, 15, 02_).

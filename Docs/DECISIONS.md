# Decision log

Только необратимые решения и лицензии ассетов (п.148). Геймдизайн — в `Docs/GDD.md`.

| Дата | Решение | Причина | Ссылка | Статус |
|---|---|---|---|---|
| 2026-11-11 | Unity 6, URP 2D, Input System, ScriptableObject для контента | Data-driven, геймпад/ребинд | ARCHITECTURE §1–2 | Active |
| 2026-11-11 | Генерация: граф этажа + проверенные шаблоны/чанки, без random walk и BSP | Проходимость, авторские бои | GDD §8, п.41 | Active |
| 2026-11-24 | Перезапуск с нуля; старый прототип — тег `legacy-prototype` | Старый код строил всё в рантайме, без префабов (Research/01_PROJECT_AUDIT.md) | п.141 | Active |
| 2026-11-26 | Название игры **Forge of the Fallen** (FotF); прежние Scrapheart / FrankenArms — рабочие | Scrapheart занят на itch.io (2 джем-игры) и в USPTO (кл. 14/16) | п.173 | Active; формальная проверка кл. 9/41 до Steam (риск: «Lords/Death of the Fallen» у CI Games) |
| 2026-11-26 | Всё видимое собирается в редакторе; код только инстанцирует префабы | Редактируемость в Inspector, опыт прототипа | п.141, A6, G7 | Active |
| 2026-11-26 | Палитра Endesga 32, PPU 32, тайл 32 px, база 640×360 | Единый масштаб и стиль | п.92, 150; Research/07_ART_BIBLE_SPEC.md | Active |
| 2026-11-26 | Убраны пакеты collab-proxy, visualscripting, learn.iet-framework; `com.unity.pipeline` оставлен (мост Unity CLI ↔ Editor) | Не нужны проекту | Research/01_PROJECT_AUDIT.md | Active |
| 2026-11-26 | Ассеты третьих сторон в проекте: нет. ART_REFS.md — только референсы | Решение владельца «только рефы, только бесплатные» | Research/ART_REFS.md | Active |
| 2026-11-26 | Свет: 2D-спрайты + карты нормалей (`_NormalMap`) + URP 2D Lights + ShadowCaster2D; без перехода на 3D-рендер (вариант «как Graveyard Keeper 2» отклонён) | Сравнение на одной сценке (стена, бочки, факел): 2D чище читается и не меняет архитектуру; 3D-тени атмосфернее, но темнее и требуют URP 3D | `Assets/_Project/Sandbox/LightingTest/LightingTest_2D.unity` | Active; ступенчатое затухание света — отдельной задачей |
| 2026-11-27 | Рост героя 48 px (канвас 64×64) вместо 32 px; тайл 32, PPU 32 и база 640×360 без изменений | Тест PixelLab 32/48/64: на 32 px не помещаются детали «лом и швы», 64 px тесно для комнат 8×8 | `ArtSource/review/pl_detail_sizes.png`; GDD §10.1, Art Bible §2 | Active; масштаб врагов/боссов — открытый вопрос GDD §13 |
| 2026-11-27 | Генерация персонажей, анимаций, пропсов, UI — PixelLab (подписка Tier 1, 2000 генераций/мес), через REST API v2 | 4 направления и анимации с сохранением персонажа; ~1 генерация на персонажа | `ArtSource/manifest.json` | Active; лицензию на коммерческое использование проверить в ToS PixelLab |
| 2026-11-27 | Герой — цельная анимация без оружия; оружие — отдельный слой с типом хвата (6 типов), классом размера S/M/L, перчатками-хватами и анимацией атаки по типу хвата | Свободная смена оружия и сплавы без спрайта под каждую пару; оружие в масштабе и стиле героя | Research/07_ART_BIBLE_SPEC.md §5.1; `concepts/sheets/05_scale_grip_check.jpg` | Active |
| 2026-11-26 | Steam: раскрываем AI-контент, который игрок видит/слышит (арт, звук, текст, маркетинг); AI-код — уточнить по официальной странице перед релизом | Steam Content Survey (правка янв. 2026) | Research/03_AI_GAMEDEV_ARTICLES.md | Active, перепроверить перед Steam |

## Лицензии ассетов
| Ассет | Источник | Лицензия | Где лежит | Дата |
|---|---|---|---|---|
| Инструмент: ComfyUI portable v0.46.0 (nvidia_cu126) | github.com/Comfy-Org/ComfyUI | GPL-3.0 (только инструмент, в билд не входит; выход генерации лицензией не связан) | `D:\AI\ComfyUI` (вне репозитория) | 2026-11-26 |
| Модель: SDXL base 1.0 (`sd_xl_base_1.0.safetensors`) | huggingface.co/stabilityai/stable-diffusion-xl-base-1.0 | CreativeML OpenRAIL++-M: коммерческое использование выхода разрешено, с ограничениями по назначению (Attachment A) | `D:\AI\ComfyUI\...\models\checkpoints` | 2026-11-26 |
| Модель: SDXL VAE fp16-fix (`sdxl.vae.safetensors`) | huggingface.co/madebyollin/sdxl-vae-fp16-fix | MIT | `D:\AI\ComfyUI\...\models\vae` | 2026-11-26 |
| LoRA: Pixel Art XL (`pixel-art-xl.safetensors`) | huggingface.co/nerijs/pixel-art-xl | CreativeML OpenRAIL-M: коммерческое использование выхода разрешено, с ограничениями по назначению | `D:\AI\ComfyUI\...\models\loras` | 2026-11-26 |

| Шрифт LiberationSans (TMP Essential Resources) + динамический ассет `font_ui_dynamic` (кириллица) | Unity TextMeshPro / Liberation Fonts | SIL OFL 1.1 (файл рядом со шрифтом). Временно, до пиксельного шрифта по Art Bible | `Assets/TextMesh Pro/Fonts`, `Assets/_Project/Art/UI/Fonts` | 2026-11-27 |
| Спрайты PixelLab: Рыцарь X2 (idle/run/dash), меч, гвоздомёт, манекен, тайлсет комнаты R3 | pixellab.ai (подписка Tier 1), запросы в `ArtSource/manifest.json` | По ToS PixelLab (проверить коммерческие права до релиза); AI-контент | `Assets/_Project/Art/Sprites`, `Art/Tiles/Castle` | 2026-11-27 |

Арт, сделанный этими моделями, — AI-контент: раскрывать в Steam (см. строку выше про Content Survey) и чистить вручную перед приёмкой (D10).

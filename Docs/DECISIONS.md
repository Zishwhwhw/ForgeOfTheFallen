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
| 2026-11-26 | Steam: раскрываем AI-контент, который игрок видит/слышит (арт, звук, текст, маркетинг); AI-код — уточнить по официальной странице перед релизом | Steam Content Survey (правка янв. 2026) | Research/03_AI_GAMEDEV_ARTICLES.md | Active, перепроверить перед Steam |

## Лицензии ассетов
| Ассет | Источник | Лицензия | Где лежит | Дата |
|---|---|---|---|---|
| — | — | — | — | — |

# 06 — Asset pipeline + MCP (Scrapheart)

Дата проверки: 2026-11-26. Все ссылки ниже проверены curl (HTTP 200), кроме отмеченных. Цены/лимиты — со страниц pricing через WebFetch; что не подтверждено — «не проверено». Лицензию Steam-использования всегда перечитывать в Terms перед релизом (я Terms не читал).

Правило масштаба проекта: герой 32x32, тайл 32 px, PPU 32, боссы 96–160 px, база 640x360, палитра Endesga 32 (E32), 4 направления.
Ключевой вывод: НИ ОДИН генератор не гарантирует точный размер/палитру → единый масштаб обеспечивает пост-обработка скриптом (раздел 5), а не промпт.

## 1. Генерация спрайтов/анимаций/тайлсетов

| Сервис | Ссылка | MCP/API/CLI | Цена (11.2026) | Free | Коммерч. лицензия | Пиксель-арт / размер / палитра |
|---|---|---|---|---|---|---|
| PixelLab | https://www.pixellab.ai | MCP (hosted `https://api.pixellab.ai/mcp`, Bearer token), API, расширение Aseprite | Тарифы на странице не загрузились — НЕ ПРОВЕРЕНО | «Try for free», без карты | Не проверено (репо MCP: «All rights reserved», это про код) | Заточен под пиксель-арт: персонаж 4/8 направлений, animate_character, Wang tilesets, skeleton animation. Размер/палитру на странице не нашёл — не проверено |
| Retro Diffusion | https://retrodiffusion.ai (pricing /pricing) | API (`api.retrodiffusion.ai/v2`, `X-RD-Token`), hosted MCP `https://mcp.retrodiffusion.ai/mcp` (33 tools), GitHub api-examples | Pay-as-you-go, без подписки: RD Lite $0.05, Core $0.10, Pro $0.18, Mini от $0.03 за картинку; animations от $0.10, tilesets от $0.06; пополнение от $5 | $0.50 при регистрации (~10 Lite); бесплатные утилиты (Palette Converter, Color Reducer, K-Centroid Downscale, Pixel Fixer) | Страница pricing НЕ упоминает — не проверено, читать Terms | Лучший по контролю: width/height 12–512 (по стилю уже), `input_palette` (палитра картинкой), анимации 32–256 px, tilesets `rd_tile__*`. Цена не зависит от размера |
| Scenario | https://www.scenario.com (/pricing) | API-first (docs.scenario.com), MCP `mcp.scenario.com` (на каких тарифах — не указано) | Starter $15/мес (1500 cr), Pro $45 (5000), Max $75 (10000); годовая −33% | 50 credits (в free — только personal/evaluation) | Платные планы: полная коммерческая лицензия | Универсальный генератор, не специализирован на пиксель-арте; обучение своей модели на Pro. Точный размер/палитру не гарантирует — не проверено |
| Ludo.ai | https://ludo.ai (/pricing) | API + MCP `https://mcp.ludo.ai/mcp` + CLI только на Pro и выше | Indie $15/мес (год, без API/MCP), Pro $35/мес (12000 cr/год, API+MCP+CLI), Studio $300/мес | Есть trial, условия не описаны | Не указано на странице — не проверено | Directional sprites (8 направлений), tileset 16-tile autotile, animate sprite. Про pixel art на странице не сказано |
| Layer | https://www.layer.ai | REST API, TS SDK, CLI, MCP: `claude mcp add --transport http Layer https://mcp.app.layer.ai/mcp` | Usage-based (Creative Units), цены на главной не указаны — не проверено | «Start for free» | Не проверено (читать ToS) | Пиксель-арт не заявлен; есть sprite sheet app. Нет гарантии размера/палитры |
| God Mode AI | https://www.godmodeai.co | REST API и «MCP & Agent Skills» заявлены, деталей нет | $29/мес (200 публичных генераций); пакеты кредитов $32/30 и $100/125 | 3 кредита при регистрации | «Commercial licensing is included with your subscription»; НО генерации на подписке публичные (public generations) — для нашей закрытой игры это минус | Pixel-art модель для low-res спрайтов, walk/attack, Spine auto-rig. Размер/палитра — не проверено |

Прочее (не проверялось, не рекомендуется без отдельной проверки): Aseprite-скрипты и ручная доработка; Stable Diffusion + pixel LoRA локально (Mac Intel/Darwin 21.6 — медленно).

### Вывод по спрайтам
- Основной: Retro Diffusion (pay-as-you-go, MCP, контроль размера и `input_palette`, самые дешёвые).
- Дополнительный: PixelLab — для 4-направленных персонажей и скелетных анимаций, но сначала проверить цену и лицензию на их странице (мне не отдалась).
- Ludo/Scenario/Layer/God Mode — нет: либо дороже, либо не про пиксель-арт, либо нет гарантий.
- Для героя 4 направления: генерировать референс идёт одним сервисом, анимации — тем же, иначе стиль разъедется.

## 2. SFX и голос

| Сервис | Ссылка | MCP/API | Цена | Free | Лицензия |
|---|---|---|---|---|---|
| ElevenLabs | https://elevenlabs.io/pricing | Hosted MCP `https://api.elevenlabs.io/v1/mcp` (OAuth). Локальный `elevenlabs-mcp` (https://github.com/elevenlabs/elevenlabs-mcp, MIT) — архивирован, deprecated | Free $0 (10 000 cr/мес); Starter $6/мес ($1 первый месяц; 30 000 cr); Creator $22/мес (121 000 cr); Pro $99 | Sound Effects доступны на Free: 200 cr за генерацию или 40 cr/сек | Free: БЕЗ коммерческой лицензии. Starter и выше — с коммерческой (и музыкой) |
| Альтернативы SFX | sfxr/jsfxr (офлайн, CC0-стиль) — https://www.sfxr.me не открылся (000), не проверено; Freesound CC0 — не проверялось | — | — | — | — |

Вывод: SFX — ElevenLabs Starter ($6/мес, один месяц $1), сгенерировать пачку звуков за месяц и отменить; свободный sfxr для ретро-звуков. Перед релизом хранить дату и план генерации (подтверждение лицензии).

## 3. Музыка

| Сервис | Ссылка | Цена | Коммерческое |
|---|---|---|---|
| Suno | https://suno.com/pricing | Free: 50 songs/день, без загрузок; Pro $10/мес ($8 год): 2500 credits, 20 downloads... (по таблице страницы); Premier $30 | Free — «Not included». Pro/Premier/Platinum — включено; страница отсылает к ToS по ограничениям. API на странице не упомянут |
| Udio | https://www.udio.com | После settlement с UMG (11.2025) загрузки отключены, «walled garden»; текущий статус не подтверждён | НЕ БРАТЬ: риск и нет загрузки |
| Stable Audio | https://stableaudio.com (pricing страницы цены не отдали) | Не проверено | Не проверено |
| Прочие (Suno-подобные) | — | — | Не проверено |

Вывод: Suno Pro на 1 месяц ($10) — сгенерировать 6–10 лупов (меню, бой, босс, база), скачать (сколько downloads — по странице 20/мес на Pro, значит планировать), отменить. Лицензия остаётся за треками, созданными на платном плане — подтвердить в ToS (не проверено). Альтернатива без юр. риска — CC0 из OpenGameArt/Kenney (не проверялось).
У Suno нет MCP (не нашёл на странице) → агент не может сам генерировать; это ручной шаг владельца.

## 4. Unity MCP

| Сервер | Ссылка | Что умеет | Установка | Цена/лицензия | Claude Code |
|---|---|---|---|---|---|
| Официальный Unity MCP (пакет com.unity.ai.assistant) | https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.4/manual/integration/unity-mcp-get-started.html ; https://unity.com/features/ai | Tools вида Unity_ManageScene, Unity_ManageGameObject, Unity_ReadConsole и др. (полный список не нашёл) | Unity 6+, пакет, мост стартует с Editor, relay-бинарник; Claude Code: mcpServers `unity-mcp` -> `~/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64 --mcp` (путь для Apple Silicon; у вас Darwin 21.6 — Intel? проверить архитектуру) ; подтвердить в Pending Connections | По unity.com/features/ai: MCP server и официальный плагин бесплатны, без кредитов; AI Assistant — Pro и выше, Personal — триал, затем $10/мес. Но результат поиска говорит, что нужна подписка/открытая бета Unity AI — противоречие, НЕ ПОДТВЕРЖДЕНО, что подойдёт Personal | Да: есть официальный плагин для Claude Code (по unity.com/features/ai и новостям, ссылку на сам блог не открывал) |
| CoplayDev unity-mcp (MCP for Unity) | https://github.com/CoplayDev/unity-mcp | ~50 tools: сцены, GameObjects, C# скрипты, ассеты, run tests, profile, build; группы vfx/animation/ui/testing | Package Manager: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main` (или openupm `com.coplaydev.unity-mcp`); Window > MCP for Unity > Configure All Detected Clients; нужен Python 3.10+ через uv (uv у вас есть) | Бесплатно, MIT | Да (Claude Code в списке клиентов); Unity 2021.3 LTS – 6.x |
| Прочие (emeryporter/UnityMCP и т.д.) | через поиск | не проверялись | — | — | Не использовать без проверки |

Важно: любой Unity MCP требует ЗАПУЩЕННЫЙ Unity Editor. Unity на Mac не найден → сначала установить Unity Hub + Unity 6 LTS (Personal бесплатно при доходе <$200K — условие из общей практики, на официальной странице не проверял). Без Editor агент пишет C# и YAML-ассеты вслепую; тесты не прогнать.
Рекомендация: CoplayDev (MIT, независим от подписок и беты Unity) как основной; официальный — попробовать позже.

## 5. Как гарантировать единый масштаб

1. Генерировать в кратном размере (напр. 64x64 -> 32x32 ровно в 2 раза; Retro Diffusion принимает width/height напрямую — просить сразу 32).
2. Downscale ТОЛЬКО nearest (при целочисленном множителе; иначе пометить и перегенерировать).
3. Квантизация в Endesga 32 по ближайшему цвету, hard alpha (никаких полупрозрачных пикселей).
4. Проверка: размер канваса точно равен ожидаемому, все цвета ⊂ E32 — assert.
5. Unity import: Sprite Mode Single/Multiple, PPU 32, Filter Mode Point (no filter), Compression None, Mesh Type Tight/Full Rect, Pixel Snapping.
6. CI-проверка: тест в Unity Test Framework (или python-скрипт в pre-commit) обходит Assets/Art и падает на несоответствии.

Скрипт (проверен на Pillow 11.1, python3 системы; тест: 64x64 -> 32x32, `OK ... (32, 32) colors=7`). E32 hex взяты с https://lospec.com/palette-list/endesga-32:

```python
#!/usr/bin/env python3
"""Scrapheart sprite post-process: downscale nearest -> quantize to Endesga 32 -> verify canvas size.
Usage: postprocess.py in.png out.png --size 32 32 [--dither]
"""
import sys, argparse
from PIL import Image
E32 = "be4a2f d77643 ead4aa e4a672 b86f50 733e39 3e2731 a22633 e43b44 f77622 feae34 fee761 63c74d 3e8948 265c42 193c3e 124e89 0099db 2ce8f5 ffffff c0cbdc 8b9bb4 5a6988 3a4466 262b44 181425 ff0044 68386c b55088 f6757a e8b796 c28569".split()
PAL = [tuple(int(h[i:i+2], 16) for i in (0, 2, 4)) for h in E32]
def main():
    a = argparse.ArgumentParser(); a.add_argument("src"); a.add_argument("dst")
    a.add_argument("--size", nargs=2, type=int, required=True); a.add_argument("--alpha-cut", type=int, default=128)
    o = a.parse_args(); w, h = o.size
    im = Image.open(o.src).convert("RGBA")
    if im.size != (w, h):
        if im.width % w == 0 and im.height % h == 0:
            im = im.resize((w, h), Image.NEAREST)           # clean integer downscale
        else:
            print(f"WARN: {im.size} not integer multiple of {(w,h)}; NEAREST resize may distort", file=sys.stderr)
            im = im.resize((w, h), Image.NEAREST)
    px = im.load(); out = Image.new("RGBA", (w, h), (0, 0, 0, 0)); op = out.load()
    for y in range(h):
        for x in range(w):
            r, g, b, al = px[x, y]
            if al < o.alpha_cut: continue                   # hard alpha: no semi-transparent pixels
            c = min(PAL, key=lambda p: (p[0]-r)**2 + (p[1]-g)**2 + (p[2]-b)**2)
            op[x, y] = (*c, 255)
    out.save(o.dst)
    chk = Image.open(o.dst); assert chk.size == (w, h), "size mismatch"
    cols = {p[:3] for p in chk.getdata() if p[3]}; assert cols <= set(PAL), "off-palette color"
    print(f"OK {o.dst} {chk.size} colors={len(cols)}")
main()
```

Запуск: `python3 postprocess.py raw/hero_s.png Assets/Art/hero_s.png --size 32 32`. Агент вызывает после каждой генерации; в репо положить как `tools/postprocess.py`. У Retro Diffusion есть и свои бесплатные Palette Converter / K-Centroid Downscale, но локальный скрипт детерминирован и не зависит от сервиса.
Ограничение: ближайший цвет в RGB может ломать оттенки на градиентах; героя смотреть глазами, при необходимости генерировать с `input_palette`.

## 6. Рекомендуемый стек и шаги для владельца

1. Retro Diffusion: регистрация, пополнить $5, API key на https://www.retrodiffusion.ai/app/devtools (`rdpk-...`), подключить MCP `https://mcp.retrodiffusion.ai/mcp` с Bearer ключом в Claude Code. Terms (коммерческое использование) перечитать — на pricing не указано. Ориентир $5–20 на вертикальный срез.
2. (Опционально) PixelLab: free trial; проверить Pricing/Terms; MCP `https://api.pixellab.ai/mcp` с token. Цена не проверена.
3. ElevenLabs Starter $6/мес ($1 первый месяц): hosted MCP, SFX; отменить после генерации. Free-план без коммерческой лицензии.
4. Suno Pro $10/мес на один месяц — музыка вручную; коммерческие права уточнить в ToS.
5. Unity 6 LTS + Hub (Personal), затем CoplayDev unity-mcp (MIT, uv уже есть).
6. Aseprite (разовая покупка; цена на странице не видна — не проверено).
7. `brew install git-lfs`; репозиторий с Unity .gitignore.
Ориентир на старт: ~$5 + $1–6 + $10 + Aseprite (цена не проверена); после генерации подписки отменяются.

Риски: Steam требует раскрывать использование AI-контента (я не проверял точные правила Steamworks). ИИ-пиксель-арт плохо держит единый стиль между анимациями — нужен ручной QA в Aseprite. Реальные деньги: «живые» платежи/подписки оформляет владелец.

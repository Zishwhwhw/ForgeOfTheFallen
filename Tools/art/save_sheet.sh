#!/usr/bin/env bash
# Save a Gemini sheet (1376x768) captured as full-scale pane screenshots 677x763.
# Three tiles at y0=0: x0=0, 677, 699. The last 5 rows are filled from the row above (sheet padding).
# Usage: TOOL_RESULTS=<session tool-results dir> Tools/art/save_sheet.sh <out.jpg> <id_x0> <id_x677> <id_x699>
# ids are the "<timestamp>-<suffix>" part of mcp-Claude_Browser-blob-<id>.jpg.
set -e
OUT=$1; shift
TR="${TOOL_RESULTS:?set TOOL_RESULTS to the session tool-results folder}"
D=ArtSource/concept_tiles; mkdir -p "$D"
for id in "$@"; do cp "$TR/mcp-Claude_Browser-blob-$id.jpg" "$D/"; done
P="$D/mcp-Claude_Browser-blob-"
PY="/d/AI/ComfyUI/ComfyUI_windows_portable/python_embeded/python.exe"
"$PY" -I Tools/art/stitch_tiles.py "$OUT" 1376 768 "0,0,$P$1.jpg" "677,0,$P$2.jpg" "699,0,$P$3.jpg"
"$PY" -I -c "
from PIL import Image
im=Image.open('$OUT')
row=im.crop((0,762,1376,763)).resize((1376,6))
im.paste(row,(0,762))
im.save('$OUT',quality=95)"

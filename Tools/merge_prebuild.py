from PIL import Image
import os

SRC = os.path.join(
    os.path.dirname(__file__),
    "..",
    "Assets",
    "Sprite",
    "Grasslands_tileset",
    "Grassland Spring@128x128.png",
)
DST = os.path.join(
    os.path.dirname(__file__),
    "..",
    "Assets",
    "Sprite",
    "HomesteadPreBuild.png",
)

SPRITE_RECTS = {
    47:  (1920, 1664),
    77:  (1664, 1408),
    79:  (1920, 1408),
    89:  (1152, 1280),
    93:  (1664, 1280),
    95:  (1920, 1280),
    105: (1152, 1152),
    109: (1664, 1152),
    110: (1792, 1152),
    111: (1920, 1152),
}

TILE = 128
TEX_H = 2048

CHILDREN = [
    (0,  0, 77,  0),
    (1,  0, 110, 0),
    (2,  0, 110, 0),
    (3,  0, 110, 0),
    (4,  0, 110, 0),
    (5,  0, 79,  0),
    (5, -1, 95,  0),
    (5, -2, 95,  0),
    (5, -3, 95,  0),
    (5, -4, 95,  0),
    (5, -5, 111, 0),
    (4, -5, 110, 0),
    (3, -5, 110, 0),
    (2, -5, 110, 0),
    (1, -5, 109, 0),
    (0, -5, 105, 0),
    (0, -1, 93,  0),
    (0, -2, 93,  0),
    (0, -3, 93,  0),
    (0, -4, 47,  0),
    (0, -4, 89,  1),
]

min_x = min(c[0] for c in CHILDREN)
max_x = max(c[0] for c in CHILDREN)
min_y = min(c[1] for c in CHILDREN)
max_y = max(c[1] for c in CHILDREN)

cols = max_x - min_x + 1
rows = max_y - min_y + 1
out_w = cols * TILE
out_h = rows * TILE

print(f"Output size: {out_w}x{out_h} ({cols}x{rows} tiles)")

src_img = Image.open(SRC)
out_img = Image.new("RGBA", (out_w, out_h), (0, 0, 0, 0))

for wx, wy, sprite_id, sort_order in CHILDREN:
    meta_x, meta_y = SPRITE_RECTS[sprite_id]
    pil_x = meta_x
    pil_y = TEX_H - meta_y - TILE

    tile = src_img.crop((pil_x, pil_y, pil_x + TILE, pil_y + TILE))

    out_x = (wx - min_x) * TILE
    out_y = (max_y - wy) * TILE

    out_img.paste(tile, (out_x, out_y), tile)
    print(f"  sprite_{sprite_id} -> out({out_x},{out_y}) from src({pil_x},{pil_y}) sort={sort_order}")

out_img.save(DST)
print(f"Saved {DST}")

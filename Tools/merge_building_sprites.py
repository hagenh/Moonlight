from PIL import Image
import os

SRC = os.path.join(
    os.path.dirname(__file__),
    "..",
    "Assets",
    "Sprite",
    "2D Hand Painted - Town Tileset",
    "128x128",
    "Town 2 Color 2@128x128.png",
)
DST = os.path.join(
    os.path.dirname(__file__),
    "..",
    "Assets",
    "Sprite",
    "BuildingFacade.png",
)

TILE = 128
TEX_H = 2048
COLS = 16

CHILDREN = [
    (0, 0, 2),
    (1, 0, 3),
    (2, 0, 4),
    (-1, -1, 17),
    (0, -1, 18),
    (1, -1, 19),
    (2, -1, 20),
    (3, -1, 21),
    (-2, -2, 32),
    (-1, -2, 33),
    (0, -2, 34),
    (1, -2, 35),
    (2, -2, 36),
    (3, -2, 37),
    (4, -2, 38),
    (-1, -3, 49),
    (0, -3, 50),
    (1, -3, 51),
    (2, -3, 52),
    (3, -3, 53),
    (-1, -4, 65),
    (0, -4, 66),
    (1, -4, 67),
    (2, -4, 68),
    (3, -4, 69),
    (-2, -5, 80),
    (-1, -5, 81),
    (0, -5, 82),
    (1, -5, 83),
    (2, -5, 84),
    (3, -5, 85),
    (4, -5, 86),
    (-1, -6, 97),
    (0, -6, 98),
    (1, -6, 99),
    (2, -6, 100),
    (3, -6, 101),
    (-1, -7, 113),
    (0, -7, 114),
    (1, -7, 115),
    (2, -7, 116),
    (3, -7, 117),
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

for wx, wy, sprite_id in CHILDREN:
    col = sprite_id % COLS
    row = sprite_id // COLS
    pil_x = col * TILE
    pil_y = row * TILE

    tile = src_img.crop((pil_x, pil_y, pil_x + TILE, pil_y + TILE))

    out_x = (wx - min_x) * TILE
    out_y = (max_y - wy) * TILE

    out_img.paste(tile, (out_x, out_y), tile)
    print(f"  _{sprite_id} -> out({out_x},{out_y}) from src({pil_x},{pil_y})")

out_img.save(DST)
print(f"Saved {DST}")

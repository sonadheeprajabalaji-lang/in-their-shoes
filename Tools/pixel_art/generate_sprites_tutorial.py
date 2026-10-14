# Tutorial and chapter-intro UI pieces: keycaps, an arrow, and one icon
# per meter (Body = heart, Mind = lightbulb, Feeling like a person =
# a person, The Mask = a mask).
#
# Run from this folder:  python3 generate_sprites_tutorial.py
# Writes into out_tutorial/. Copy those PNGs into Assets/Resources/PixelArt
# (new files also need .meta files; see the README).
import os
from PIL import Image, ImageDraw
here = os.path.dirname(os.path.abspath(__file__))
os.chdir(here)
OUT = 'out_tutorial'; os.makedirs(OUT, exist_ok=True)

T0 = (0, 0, 0, 0)
OL = (43, 34, 51, 255)
CAP = (240, 228, 206, 255); CAP_L = (252, 246, 232, 255); CAP_SIDE = (176, 156, 140, 255)
GOLD = (255, 214, 120, 255); GOLD_L = (255, 236, 176, 255); GOLD_SIDE = (196, 150, 70, 255)
WHITE = (248, 246, 240, 255)
BODY = (216, 90, 48, 255); BODY_L = (240, 140, 100, 255); BODY_D = (168, 62, 34, 255)
MIND = (239, 159, 39, 255); MIND_L = (255, 210, 120, 255); MIND_D = (184, 112, 20, 255)
PERSON = (29, 158, 117, 255); PERSON_L = (93, 202, 165, 255); PERSON_D = (16, 112, 82, 255)
MASK = (127, 119, 221, 255); MASK_L = (176, 170, 240, 255); MASK_D = (88, 80, 170, 255)
STEEL = (186, 196, 204, 255)

def new(w, h): return Image.new('RGBA', (w, h), T0)
def R(im, x0, y0, x1, y1, c): ImageDraw.Draw(im).rectangle([x0, y0, x1, y1], fill=c)
def E(im, x0, y0, x1, y1, c): ImageDraw.Draw(im).ellipse([x0, y0, x1, y1], fill=c)
def save(im, name): im.save(f'{OUT}/{name}.png')
def outline(im, c=OL):
    src = im.copy(); px = src.load(); w, h = im.size
    for y in range(h):
        for x in range(w):
            if px[x, y][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and px[nx, ny][3] > 0 and px[nx, ny] != c:
                        im.putpixel((x, y), c); break
    return im

# ---------- keycaps (24x24, sliced with a 6px border; 8px at the bottom) ----------
def keycap(top, top_l, side, pressed):
    im = new(24, 24)
    off = 2 if pressed else 0
    R(im, 1, 0 + off, 22, 23, OL); R(im, 0, 1 + off, 23, 22, OL)          # outline
    R(im, 1, 1 + off, 22, 22, side)                                         # sides
    R(im, 2, 1 + off, 21, 19 - (0 if not pressed else 0), top)             # top face
    R(im, 2, 1 + off, 21, 2 + off, top_l)                                   # highlight
    R(im, 1, 20, 22, 22, side) if not pressed else None
    if pressed:  # shorter: the cap has gone down
        R(im, 1, 0, 22, 1, T0)
    return im
save(keycap(CAP, CAP_L, CAP_SIDE, False), 'ui_key')
save(keycap(GOLD, GOLD_L, GOLD_SIDE, True), 'ui_key_down')

# ---------- arrow (points up; rotate in the UI) ----------
a = new(16, 16)
for i in range(6):
    R(a, 7 - i, 2 + i, 8 + i, 2 + i, OL)
R(a, 6, 8, 9, 13, OL)
save(a, 'icon_arrow')

# ---------- meter icons (16x16) ----------
# Body: a heart
h = new(16, 16)
E(h, 1, 2, 8, 9, BODY); E(h, 7, 2, 14, 9, BODY)
for i in range(7):
    R(h, 1 + i, 7 + i, 14 - i, 7 + i, BODY)
R(h, 3, 4, 4, 5, BODY_L); R(h, 10, 9, 11, 10, BODY_D); R(h, 9, 10, 10, 11, BODY_D)
outline(h); save(h, 'icon_body')

# Mind: a lightbulb (her focus)
m = new(16, 16)
E(m, 3, 1, 12, 10, MIND); R(m, 5, 9, 10, 11, MIND)
R(m, 5, 12, 10, 12, STEEL); R(m, 6, 13, 9, 14, STEEL)
R(m, 5, 3, 6, 5, MIND_L); R(m, 7, 6, 8, 9, MIND_D)
outline(m); save(m, 'icon_mind')

# Feeling like a person: a person
p = new(16, 16)
E(p, 4, 0, 11, 7, PERSON_L)
E(p, 2, 8, 13, 18, PERSON)
R(p, 2, 14, 13, 15, PERSON)
R(p, 5, 9, 6, 10, PERSON_L)
p = p.crop((0, 0, 16, 16)); outline(p); save(p, 'icon_person')

# The Mask: a face mask with eye holes
k = new(16, 16)
E(k, 1, 3, 14, 14, MASK)
R(k, 1, 3, 14, 8, MASK)
R(k, 3, 6, 6, 8, OL); R(k, 9, 6, 12, 8, OL)              # eyes
R(k, 5, 11, 10, 11, MASK_D); R(k, 6, 12, 9, 12, MASK_D)  # smile
R(k, 2, 4, 4, 4, MASK_L)
R(k, 0, 5, 0, 6, MASK_D); R(k, 15, 5, 15, 6, MASK_D)     # ties
outline(k); save(k, 'icon_mask')

print('tutorial sprites done:', sorted(os.listdir(OUT)))

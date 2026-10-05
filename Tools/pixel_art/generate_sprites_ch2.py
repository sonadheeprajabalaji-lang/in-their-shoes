# Chapter 2 sprites: the same house as Chapter 1, at 22 weeks, with a
# nursery corner where the couch was.
#
# Run from this folder:  python3 generate_sprites_ch2.py
# It first runs generate_sprites.py (Chapter 1, shared helpers and shared
# sprites), then writes the Chapter 2 sprites into out/ as well.
# Copy out/*.png into Assets/Resources/PixelArt afterwards.
import os
here = os.path.dirname(os.path.abspath(__file__))
os.chdir(here)
exec(open('generate_sprites.py').read())

# ---------- helpers ----------
def floor_cell(img, cx, cy):
    x0, y0 = cell(cx, cy)
    rnd = random.Random(cx * 97 + cy * 13)
    for r in range(4):
        yy = y0 + r * 8
        shade = [WOOD, WOOD_L, WOOD, WOOD_D][(r + cx * 3 + cy) % 4]
        R(img, x0, yy, x0 + 31, yy + 7, shade); L(img, x0, yy + 7, x0 + 31, yy + 7, WOOD_S)
        off = (r * 11 + cx * 7 + cy * 5) % 32
        L(img, x0 + off, yy, x0 + off, yy + 6, WOOD_S)
        if rnd.random() < 0.5: P(img, x0 + rnd.randrange(32), yy + rnd.randrange(1, 6), WOOD_D)

def stamp(img, sprite, cx, cy):
    img.alpha_composite(sprite, cell(cx, cy))

PINK = (238, 186, 186, 255); PINK_D = (214, 150, 154, 255)
MINT = (190, 222, 200, 255); MINT_D = (150, 192, 166, 255)
BOX = (204, 160, 108, 255); BOX_D = (172, 128, 82, 255); BOX_L = (224, 186, 136, 255)
SCREEN = (36, 44, 70, 255); GLOW = (150, 190, 240, 255)
ALERT = (226, 84, 76, 255)

# ---------- room base ----------
b2 = base.copy()
# clear the Chapter 1 living-room furniture: rug, couch, coffee table, plant, basket
for cy in (-1, -2, -3):
    for cx in (1, 2, 3, 4):
        floor_cell(b2, cx, cy)
floor_cell(b2, 4, 1)

# soft nursery rug, x 1..4, y -1..-3
rx0, ry0 = cell(1, -1); rx1, ry1 = cell(4, -3); rx1 += 31; ry1 += 31
E(b2, rx0 + 6, ry0 + 10, rx1 - 6, ry1 - 2, MINT_D); E(b2, rx0 + 9, ry0 + 13, rx1 - 9, ry1 - 5, MINT)
E(b2, rx0 + 22, ry0 + 22, rx1 - 22, ry1 - 16, CREAM)
for a in range(0, 360, 30):
    x = (rx0 + rx1) // 2 + int(math.cos(math.radians(a)) * 44); y = (ry0 + ry1) // 2 + 4 + int(math.sin(math.radians(a)) * 28)
    R(b2, x, y, x + 2, y + 2, PINK)

# sink counter at (-3,3) is no longer a task in Chapter 2: bake it in, clean
stamp(b2, sink(False), -3, 3)

# cot spanning (3,-3) and (4,-3)
cot = new(64, 32)
R(cot, 2, 2, 61, 29, OAK); R(cot, 5, 5, 58, 26, WHITE); R(cot, 5, 5, 58, 9, MINT)   # mattress + sheet
R(cot, 8, 12, 24, 22, PINK); R(cot, 8, 12, 24, 13, PINK_D)                            # folded blanket
E(cot, 44, 14, 52, 22, BOX_L); E(cot, 42, 18, 46, 22, BOX_L); E(cot, 50, 18, 54, 22, BOX_L)  # teddy
P(cot, 46, 17, OL); P(cot, 50, 17, OL)
for x in range(4, 62, 5): L(cot, x, 2, x, 29, OAK_D)                                 # bars
R(cot, 0, 0, 63, 3, OAK_D); R(cot, 0, 28, 63, 31, OAK_D)
outline(cot); b2.alpha_composite(cot, cell(3, -3))

# car seat, still in its box, at (1,-3)
cs = new()
R(cs, 3, 6, 28, 29, BOX); R(cs, 3, 6, 28, 9, BOX_L); R(cs, 3, 26, 28, 29, BOX_D)
L(cs, 15, 6, 15, 29, BOX_D)                                  # tape seam
R(cs, 7, 13, 24, 23, WHITE); E(cs, 10, 14, 21, 22, NAVY_L); R(cs, 12, 19, 19, 22, NAVY)  # car seat picture
outline(cs); b2.alpha_composite(cs, cell(1, -3))

# side table for the phone at (4,0) is the task sprite; add an armchair beside it at (4,-1)
ac = new()
R(ac, 4, 8, 27, 28, TERRA); R(ac, 4, 8, 27, 13, TERRA_L); R(ac, 2, 10, 6, 28, TERRA_D); R(ac, 25, 10, 29, 28, TERRA_D)
R(ac, 8, 15, 23, 25, TERRA_L); R(ac, 10, 17, 16, 22, CREAM)   # cushion + little knitted thing
outline(ac); b2.alpha_composite(ac, cell(4, -1))

# ultrasound photo stuck on the fridge's lower door
x0, y0 = cell(-4, 4)
R(b2, x0 + 7, y0 + 43, x0 + 18, y0 + 52, WHITE); R(b2, x0 + 8, y0 + 44, x0 + 17, y0 + 50, (30, 30, 34, 255))
E(b2, x0 + 10, y0 + 45, x0 + 15, y0 + 49, (120, 120, 126, 255)); P(b2, x0 + 12, y0 + 46, (180, 180, 186, 255))
R(b2, x0 + 11, y0 + 42, x0 + 14, y0 + 43, MUST)
save(b2, 'room_base_ch2')

# ---------- tasks ----------
# washing machine in the counter run (washing)
def washer(state, frame=0):
    im = new(); counter(im)
    R(im, 2, 6, 29, 31, WHITE); R(im, 2, 6, 29, 11, STEEL_L); L(im, 2, 11, 29, 11, STEEL_D)
    R(im, 4, 8, 9, 9, STEEL_D); P(im, 25, 8, (120, 220, 140, 255) if state != 'wet' else (ALERT if frame % 2 == 0 else (120, 60, 60, 255)))
    E(im, 6, 13, 25, 30, STEEL_D); E(im, 8, 15, 23, 28, SCREEN if state != 'done' else STEEL)
    if state == 'running':
        a = frame * 40
        for k in range(3):
            ang = math.radians(a + k * 120)
            x = 15 + int(math.cos(ang) * 4); y = 21 + int(math.sin(ang) * 4)
            R(im, x, y, x + 2, y + 1, (TERRA_L, NAVY_L, MUST)[k])
    elif state == 'wet':
        E(im, 10, 19, 21, 27, NAVY_L); E(im, 12, 18, 19, 24, TERRA_L); E(im, 14, 21, 21, 26, SAGE_L)
    else:
        E(im, 10, 17, 21, 26, (196, 204, 210, 255))
    ImageDraw.Draw(im).rectangle([2, 6, 29, 31], outline=OL)
    return im
save(washer('running', 0), 'washer_running_0'); save(washer('running', 1), 'washer_running_1')
save(washer('done'), 'washer_done'); save(washer('wet', 0), 'washer_wet_0'); save(washer('wet', 1), 'washer_wet_1')

# side table with phone (mum_call)
def phone_table(state, frame=0):
    im = new()
    E(im, 4, 14, 27, 24, OAK); E(im, 4, 16, 27, 26, OAK_D); E(im, 4, 14, 27, 23, OAK)
    R(im, 14, 24, 17, 31, OAK_S)
    E(im, 6, 8, 12, 16, PINK); L(im, 9, 4, 9, 9, LEAF_D); E(im, 7, 2, 11, 6, LEAF)      # small vase
    dx = (1 if frame % 2 else -1) if state == 'ring' else 0
    R(im, 15 + dx, 10, 23 + dx, 22, OL)
    if state == 'down':
        R(im, 16, 11, 22, 21, NAVY_D); R(im, 18, 13, 20, 14, NAVY_L)
    else:
        R(im, 16 + dx, 11, 22 + dx, 21, SCREEN)
        if state == 'ring':
            E(im, 17 + dx, 12, 21 + dx, 16, PINK); R(im, 17 + dx, 18, 18 + dx, 19, (120, 220, 140, 255)); R(im, 20 + dx, 18, 21 + dx, 19, ALERT)
            if frame % 2 == 0:
                for (x, y) in ((25, 9), (27, 12), (25, 15)): R(im, x, y, x + 1, y + 1, MUST)
                for (x, y) in ((12, 9), (10, 12), (12, 15)): R(im, x, y, x + 1, y + 1, MUST)
        else:  # missed call
            R(im, 17, 13, 21, 14, GLOW); R(im, 17, 16, 20, 17, GLOW); E(im, 20, 10, 24, 14, ALERT); P(im, 22, 12, WHITE)
    outline(im)
    return im
save(phone_table('ring', 0), 'phone_ring_0'); save(phone_table('ring', 1), 'phone_ring_1')
save(phone_table('down'), 'phone_down'); save(phone_table('missed'), 'phone_missed')

# phone lit up on the desk (searches)
def desk_phone(state, frame=0):
    im = new(); desk(im, 0, 0)
    R(im, 29, 24, 31, 31, OAK_S); L(im, 28, 24, 28, 31, OL); L(im, 31, 7, 31, 24, OL)
    R(im, 2, 10, 9, 17, CREAM); L(im, 3, 12, 8, 12, BOX_D); L(im, 3, 14, 7, 14, BOX_D)  # notepad with a list
    R(im, 12, 6, 21, 21, OL)
    if state == 'down':
        R(im, 13, 7, 20, 20, NAVY_D); R(im, 15, 9, 17, 10, NAVY_L)
    else:
        R(im, 13, 7, 20, 20, SCREEN)
        R(im, 14, 8, 19, 9, (230, 230, 240, 255))                     # search bar
        rows = 4 if state == 'trace' else 3
        for k in range(rows):
            y = 11 + ((k * 3 + (frame if state == 'scroll' else 0)) % 9)
            L(im, 14, y, 19 - (k % 2) * 2, y, GLOW)
        # glow on the desk around the phone
        for (x, y) in ((11, 22), (22, 22), (11, 5), (22, 5)): P(im, x, y, (190, 210, 240, 255))
    return im
save(desk_phone('scroll', 0), 'search_scroll_0'); save(desk_phone('scroll', 1), 'search_scroll_1'); save(desk_phone('scroll', 2), 'search_scroll_2')
save(desk_phone('down'), 'search_down'); save(desk_phone('trace'), 'search_left_open')

# box of hand-me-down baby clothes (nursery, the main task)
def onesie(im, x, y, c, cd):
    R(im, x + 1, y, x + 6, y + 7, c); R(im, x - 1, y, x + 8, y + 2, c); R(im, x + 2, y + 7, x + 5, y + 8, cd); P(im, x + 3, y, cd); P(im, x + 4, y, cd)
def nursery_box(state, frame=0):
    im = new()
    R(im, 4, 14, 27, 29, BOX); R(im, 4, 26, 27, 29, BOX_D)
    if state in ('idle', 'sorting', 'trace'):
        R(im, 5, 12, 26, 15, BOX_D)                       # open inside
        R(im, 0, 10, 6, 14, BOX_L); R(im, 25, 10, 31, 14, BOX_L)   # flaps
        onesie(im, 8, 7, PINK, PINK_D); onesie(im, 16, 8, MINT, MINT_D); R(im, 12, 10, 18, 13, MUST)
    if state == 'sorting':
        lift = (0, -2, -3, -2)[frame % 4]
        onesie(im, 12, 1 + lift, WHITE, CREAM_D)          # holding up a tiny onesie
    if state == 'trace':
        onesie(im, 22, 22, PINK, PINK_D); onesie(im, 2, 24, WHITE, CREAM_D); R(im, 14, 28, 20, 30, MINT)  # spilling out
    if state == 'done':
        R(im, 5, 12, 26, 15, BOX_D)
        R(im, 6, 2, 25, 6, PINK); R(im, 6, 6, 25, 10, MINT); R(im, 6, 10, 25, 13, WHITE)   # neat folded stack on top
        L(im, 6, 6, 25, 6, PINK_D); L(im, 6, 10, 25, 10, MINT_D)
    outline(im)
    return im
save(nursery_box('idle'), 'nursery_idle')
for f in range(4): save(nursery_box('sorting', f), f'nursery_sorting_{f}')
save(nursery_box('done'), 'nursery_done'); save(nursery_box('trace'), 'nursery_half_unpacked')

# ---------- her at 22 weeks: same sprites with a small bump ----------
for facing in ('down', 'up', 'side'):
    for fr in (0, 1):
        im = Image.open(f'{OUT}/her_{facing}_{fr}.png').convert('RGBA')
        if facing == 'side':
            R(im, 20, 18, 21, 23, SAGE); R(im, 22, 19, 22, 22, SAGE); R(im, 20, 23, 21, 23, SAGE_D)
            for y in range(18, 24):
                x = 23 if 19 <= y <= 22 else 22
                if im.getpixel((x, y))[3] == 0: P(im, x, y, OL)
        elif facing == 'down':
            R(im, 12, 19, 19, 23, SAGE_L); R(im, 13, 23, 18, 23, SAGE); R(im, 15, 19, 16, 22, CREAM)
            P(im, 11, 20, SAGE_D); P(im, 20, 20, SAGE_D)
        save(im, f'her_bump_{facing}_{fr}')

# ---------- worry bubble ----------
wb = new(16, 16)
E(wb, 1, 1, 14, 11, WHITE); E(wb, 3, 11, 6, 14, WHITE); P(wb, 2, 14, WHITE)
R(wb, 6, 3, 9, 4, PLUM); P(wb, 9, 5, PLUM); R(wb, 7, 6, 8, 6, PLUM); R(wb, 7, 8, 8, 9, PLUM)   # "?"
outline(wb); save(wb, 'worry_bubble')
print('chapter 2 done')

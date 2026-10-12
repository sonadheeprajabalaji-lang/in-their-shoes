# Chapters 3 to 6: the same house through late pregnancy and the first
# months after the birth.
#
# Run from this folder:  python3 generate_sprites_ch3to6.py
# It first runs generate_sprites_ch2.py (which runs generate_sprites.py), so
# out/ ends up with every chapter's sprites. Copy out/*.png into
# Assets/Resources/PixelArt afterwards (new files also need .meta files;
# see the README).
import os
here = os.path.dirname(os.path.abspath(__file__))
os.chdir(here)
exec(open('generate_sprites_ch2.py').read())

# ---------- extra colours ----------
NIGHT = (40, 50, 86, 255); NIGHT_L = (64, 76, 120, 255); MOON = (240, 232, 190, 255)
ROSE = (214, 120, 140, 255); ROSE_D = (178, 90, 110, 255); ROSE_L = (236, 168, 180, 255)
MILK = (250, 246, 232, 255); MILK_D = (226, 220, 200, 255)
BLUSH_RED = (230, 120, 110, 255); TEAR = (150, 200, 240, 255)
GOLD = (240, 200, 90, 255)

# ---------- shared bits ----------
def car_seat_unboxed():
    cs = new()
    R(cs, 6, 22, 25, 28, DARK_L)                          # base
    E(cs, 5, 6, 26, 26, NAVY); E(cs, 8, 9, 23, 23, NAVY_L)
    R(cs, 12, 12, 19, 22, NAVY_D)                         # seat
    L(cs, 4, 10, 16, 3, DARK); L(cs, 16, 3, 27, 10, DARK) # handle
    outline(cs)
    return cs

def baby(im, x, y, state, frame=0):
    """Swaddled baby, head at (x, y). state: calm, fuss, cry, sleep, feed."""
    R(im, x - 5, y + 4, x + 5, y + 13, MILK); R(im, x - 4, y + 13, x + 4, y + 15, MILK_D)   # swaddle
    L(im, x - 5, y + 8, x + 5, y + 8, MILK_D)
    E(im, x - 4, y - 4, x + 4, y + 4, SKIN)                                                    # head
    P(im, x - 2, y - 4, HAIR_P); P(im, x - 1, y - 4, HAIR_P); P(im, x, y - 4, HAIR_P)
    if state == 'sleep':
        L(im, x - 2, y, x - 1, y, OL); L(im, x + 1, y, x + 2, y, OL)
    elif state in ('cry',):
        L(im, x - 2, y - 1, x - 1, y, OL); L(im, x + 2, y - 1, x + 1, y, OL)
        R(im, x - 1, y + 2, x + 1, y + 3, BLUSH_RED)
        E(im, x - 4, y - 1, x - 2, y + 2, BLUSH_RED); E(im, x + 2, y - 1, x + 4, y + 2, BLUSH_RED)
        P(im, x - 3, y + 1 + frame % 2, TEAR); P(im, x + 3, y + 2 - frame % 2, TEAR)
    elif state == 'fuss':
        P(im, x - 2, y, OL); P(im, x + 2, y, OL); R(im, x - 1, y + 2, x, y + 2, BLUSH_RED)
    else:  # calm, feed
        P(im, x - 2, y, OL); P(im, x + 2, y, OL); P(im, x, y + 2, ROSE_D)

def cry_lines(im, cx, cy, frame):
    if frame % 2 == 0:
        for (x, y) in ((cx - 9, cy - 4), (cx - 11, cy), (cx + 9, cy - 4), (cx + 11, cy)):
            R(im, x, y, x + 1, y + 1, ALERT)
    else:
        for (x, y) in ((cx - 10, cy - 6), (cx - 12, cy - 1), (cx + 10, cy - 6), (cx + 12, cy - 1)):
            R(im, x, y, x + 1, y + 1, ALERT)

def zz(im, x, y, frame=0):
    o = frame % 2
    for k, (dx, dy) in enumerate(((0, 0), (4, -5))):
        xx, yy = x + dx, y + dy - o
        L(im, xx, yy, xx + 3, yy, NAVY_L); L(im, xx + 3, yy, xx, yy + 3, NAVY_L); L(im, xx, yy + 3, xx + 3, yy + 3, NAVY_L)

def heart(im, x, y, c=ROSE):
    P(im, x, y, c); P(im, x + 2, y, c); R(im, x, y + 1, x + 2, y + 1, c); P(im, x + 1, y + 2, c)

def desk_cell(extra=None):
    im = new(); desk(im, 0, 0)
    R(im, 29, 24, 31, 31, OAK_S); L(im, 28, 24, 28, 31, OL); L(im, 31, 7, 31, 24, OL)
    if extra: extra(im)
    return im

def counter_cell(extra=None):
    im = new(); counter(im)
    if extra: extra(im)
    return im

def mat_cell(extra=None):
    im = new()
    R(im, 4, 16, 27, 29, SAGE_D); R(im, 6, 18, 25, 27, SAGE)
    if extra: extra(im)
    return im

def recolour(src, mapping):
    im = src.copy(); px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            if px[x, y] in mapping: px[x, y] = mapping[px[x, y]]
    return im

# ---------- her through the chapters ----------
# Chapter 3: a bigger bump at 37 weeks
for facing in ('down', 'up', 'side'):
    for fr in (0, 1):
        im = Image.open(f'{OUT}/her_bump_{facing}_{fr}.png').convert('RGBA')
        if facing == 'side':
            R(im, 20, 17, 23, 24, SAGE); R(im, 24, 18, 24, 23, SAGE); R(im, 20, 24, 23, 24, SAGE_D)
            for y in range(16, 26):
                for x in range(20, 27):
                    if im.getpixel((x, y))[3] == 0 and any(
                        0 <= x + dx < 32 and im.getpixel((x + dx, y + dy))[3] and im.getpixel((x + dx, y + dy)) != OL
                        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                        P(im, x, y, OL)
        elif facing == 'down':
            R(im, 11, 18, 20, 24, SAGE_L); R(im, 12, 24, 19, 24, SAGE); R(im, 15, 18, 16, 23, CREAM)
            P(im, 10, 20, SAGE_D); P(im, 21, 20, SAGE_D); P(im, 10, 21, OL); P(im, 21, 21, OL)
        save(im, f'her_late_{facing}_{fr}')

# Chapters 4 and 5: a dusky-rose dressing gown, no bump
gown = {SAGE: ROSE, SAGE_D: ROSE_D, SAGE_L: ROSE_L, CREAM: MILK}
# Chapter 6: dressed for work, a mustard blouse
work = {SAGE: MUST, SAGE_D: MUST_D, SAGE_L: (240, 204, 120, 255), CREAM: WHITE}
for facing in ('down', 'up', 'side'):
    for fr in (0, 1):
        src = Image.open(f'{OUT}/her_{facing}_{fr}.png').convert('RGBA')
        save(recolour(src, gown), f'her_gown_{facing}_{fr}')
        save(recolour(src, work), f'her_work_{facing}_{fr}')

# ---------- mood bubbles ----------
sb = new(16, 16)
E(sb, 1, 1, 14, 11, WHITE); E(sb, 3, 11, 6, 14, WHITE); P(sb, 2, 14, WHITE)
L(sb, 4, 3, 8, 3, NAVY); L(sb, 8, 3, 4, 8, NAVY); L(sb, 4, 8, 8, 8, NAVY)
L(sb, 9, 4, 11, 4, NAVY_L); L(sb, 11, 4, 9, 6, NAVY_L); L(sb, 9, 6, 11, 6, NAVY_L)
outline(sb); save(sb, 'sleep_bubble')

pb = new(16, 16)
E(pb, 1, 1, 14, 11, WHITE); E(pb, 3, 11, 6, 14, WHITE); P(pb, 2, 14, WHITE)
for (x, y) in ((5, 4), (8, 3), (10, 6), (6, 7)):   # a jagged "ouch" burst
    R(pb, x, y, x + 1, y + 1, ALERT)
L(pb, 5, 4, 10, 7, ALERT)
outline(pb); save(pb, 'pain_bubble')

# =====================================================================
# Chapter 3: third trimester, ~37 weeks. Packing the hospital bag.
# =====================================================================
b3 = b2.copy()
floor_cell(b3, 1, -3)
x0, y0 = cell(1, -3)
# rug edge back over the cleared cell
E_r0x, E_r0y = cell(1, -1)
b3.alpha_composite(car_seat_unboxed(), cell(1, -3))
# pregnancy pillow draped over the armchair
x0, y0 = cell(4, -1)
E(b3, x0 + 5, y0 + 4, x0 + 27, y0 + 14, MILK); E(b3, x0 + 8, y0 + 7, x0 + 24, y0 + 12, MILK_D)
save(b3, 'room_base_ch3')

def note_desk(state, frame=0):
    def extra(im):
        R(im, 8, 6, 22, 21, CREAM); L(im, 8, 6, 22, 6, MUST)              # notepad
        lines = {'todo': 1 + frame % 3, 'done': 4, 'half': 2}[state]
        for k in range(lines):
            L(im, 10, 9 + k * 3, 20 - (k % 2) * 3, 9 + k * 3, NAVY)
        if state == 'todo':
            L(im, 20 - frame % 2, 9 + lines * 3, 24 - frame % 2, 5 + lines * 3, OL)   # pen
        if state == 'done':
            R(im, 23, 8, 28, 13, MUST)                                     # stuck-up sticky note
            L(im, 24, 10, 27, 10, MUST_D)
        if state == 'half':
            L(im, 10, 16, 14, 16, ALERT); L(im, 10, 17, 14, 17, ALERT)    # underlined, trails off
            P(im, 16, 18, NAVY); P(im, 18, 18, NAVY)
        ImageDraw.Draw(im).rectangle([8, 6, 22, 21], outline=OL)
    return desk_cell(extra)
save(note_desk('todo', 0), 'note_todo_0'); save(note_desk('todo', 1), 'note_todo_1')
save(note_desk('done'), 'note_done'); save(note_desk('half'), 'note_half')

def hospital_bag(state, frame=0):
    im = new()
    # a soft duffel bag
    R(im, 3, 14, 28, 28, PLUM); R(im, 3, 25, 28, 28, PLUM_D); E(im, 1, 14, 7, 28, PLUM); E(im, 24, 14, 30, 28, PLUM)
    L(im, 9, 12, 12, 8, OL); L(im, 12, 8, 19, 8, OL); L(im, 19, 8, 22, 12, OL)          # handles
    if state == 'packed':
        L(im, 4, 16, 27, 16, MUST)                                                       # zipped
        R(im, 22, 18, 26, 22, WHITE); L(im, 23, 20, 25, 20, NAVY)                         # name tag
    else:
        R(im, 5, 15, 26, 18, PLUM_D)                                                     # open mouth
        if state in ('idle', 'packing'):
            R(im, 7, 13, 12, 16, MILK); R(im, 14, 12, 19, 16, ROSE_L)
        if state == 'packing':
            y = (2, 0, -2, 0)[frame % 4]
            item = (MILK, ROSE_L, MINT, WHITE)[frame % 4]
            R(im, 12, 3 + y, 19, 8 + y, item); ImageDraw.Draw(im).rectangle([12, 3 + y, 19, 8 + y], outline=OL)
        if state == 'half':
            R(im, 0, 26, 7, 30, ROSE_L); R(im, 23, 27, 31, 31, MINT); R(im, 12, 28, 18, 31, WHITE)  # spilling out
            R(im, 26, 2, 31, 9, CREAM); L(im, 27, 4, 30, 4, NAVY); P(im, 27, 6, NAVY)               # list, half ticked
    outline(im)
    return im
save(hospital_bag('idle'), 'bag_idle')
for f in range(4): save(hospital_bag('packing', f), f'bag_packing_{f}')
save(hospital_bag('packed'), 'bag_packed'); save(hospital_bag('half'), 'bag_half_packed')

# =====================================================================
# Chapter 4: postpartum week 1. Feeding the baby.
# =====================================================================
b4 = b3.copy()
x0, y0 = cell(4, -1)                      # armchair: swap the pregnancy pillow for a balloon
b4.alpha_composite(Image.open(f'{OUT}/room_base_ch2.png').crop((x0, y0, x0 + 32, y0 + 32)), (x0, y0))
L(b4, x0 + 24, y0 + 10, x0 + 26, y0 - 6, OL)
E(b4, x0 + 21, y0 - 18, x0 + 31, y0 - 6, PINK); P(b4, x0 + 24, y0 - 15, WHITE)
stamp(b4, stove(False), 0, 3)                     # static stove: no cooking task this chapter
x0, y0 = cell(4, 3)                               # cards on the counter beside the fruit bowl
R(b4, x0 + 1, y0 + 4, x0 + 6, y0 + 12, PINK); R(b4, x0 + 26, y0 + 5, x0 + 31, y0 + 12, MINT)
save(b4, 'room_base_ch4')

def visitors(state, frame=0):
    def extra(im):
        if state in ('todo', 'trace'):
            R(im, 8, 14, 17, 25, ROSE); L(im, 10, 14, 12, 10, OL); L(im, 15, 14, 13, 10, OL)   # gift bag
            bob = (frame % 2) if state == 'todo' else 0
            L(im, 22, 24, 23, 8 - bob, OL); E(im, 18, 0 - bob, 28, 10 - bob, MUST)          # balloon
            if state == 'trace':
                R(im, 19, 22, 25, 27, WHITE); L(im, 20, 24, 24, 24, ROSE_D)                  # unopened card
        else:
            R(im, 8, 22, 12, 26, SHOE); R(im, 15, 22, 19, 26, SHOE)                         # just shoes, tidy
        outline(im)
    return mat_cell(extra)
save(visitors('todo', 0), 'visitors_todo_0'); save(visitors('todo', 1), 'visitors_todo_1')
save(visitors('done'), 'visitors_done'); save(visitors('trace'), 'visitors_left')

def eat_drink(state):
    def extra(im):
        E(im, 4, 7, 17, 14, WHITE); E(im, 5, 8, 16, 13, MILK_D)                            # plate
        if state in ('todo', 'trace'):
            R(im, 7, 7, 14, 11, BOX_L if state == 'todo' else BOX_D); L(im, 7, 9, 14, 9, LEAF)   # sandwich
        R(im, 21, 2, 26, 14, SKY_L)                                                         # glass
        if state in ('todo', 'trace'): R(im, 21, 6, 26, 14, SKY)
        ImageDraw.Draw(im).rectangle([21, 2, 26, 14], outline=OL)
        if state == 'done':
            P(im, 8, 10, BOX_D); P(im, 12, 9, BOX_D)                                         # crumbs
    return counter_cell(extra)
save(eat_drink('todo'), 'eat_todo'); save(eat_drink('done'), 'eat_done'); save(eat_drink('trace'), 'eat_untouched')

def pain_relief(state, frame=0):
    def extra(im):
        R(im, 6, 10, 17, 17, STEEL_L); ImageDraw.Draw(im).rectangle([6, 10, 17, 17], outline=OL)   # blister pack
        for k, x in enumerate((8, 11, 14)):
            popped = state == 'done' and k < 2
            E(im, x, 12, x + 2, 15, STEEL if popped else WHITE)
        R(im, 2, 8, 4, 15, SKY_L)                                                           # glass of water
        # alarm clock
        E(im, 20, 5, 28, 13, WHITE if state != 'trace' else (250, 220, 220, 255)); ImageDraw.Draw(im).ellipse([20, 5, 28, 13], outline=OL)
        L(im, 24, 9, 24, 6, OL); L(im, 24, 9, 26, 9, ALERT if state == 'trace' else OL)
        if state == 'todo' and frame % 2 == 0:
            for (x, y) in ((18, 4), (29, 4), (18, 13), (29, 13)): P(im, x, y, MUST)
    return desk_cell(extra)
save(pain_relief('todo', 0), 'pills_todo_0'); save(pain_relief('todo', 1), 'pills_todo_1')
save(pain_relief('done'), 'pills_done'); save(pain_relief('trace'), 'pills_missed')

def feeding(state, frame=0):
    im = new()
    # C-shaped feeding pillow on the floor
    E(im, 2, 10, 29, 30, MINT_D); E(im, 8, 15, 23, 26, (0, 0, 0, 0))
    E(im, 3, 11, 28, 29, MINT); E(im, 9, 16, 22, 25, (0, 0, 0, 0))
    R(im, 12, 22, 19, 30, (0, 0, 0, 0)); R(im, 10, 25, 21, 30, (0, 0, 0, 0))
    st = {'idle': 'fuss', 'active': 'feed', 'done': 'sleep', 'cancel': 'cry'}[state]
    baby(im, 16, 10, st, frame)
    if state == 'active':
        heart(im, 4 + (frame % 2) * 2, 4 - frame % 4, ROSE); heart(im, 24, 6 - (frame + 2) % 4, ROSE_L)
    if state == 'done': zz(im, 21, 4)
    if state == 'cancel': cry_lines(im, 16, 10, frame)
    outline(im)
    return im
save(feeding('idle'), 'feed_idle')
for f in range(4): save(feeding('active', f), f'feed_active_{f}')
save(feeding('done'), 'feed_done'); save(feeding('cancel', 0), 'feed_crying_0'); save(feeding('cancel', 1), 'feed_crying_1')

# =====================================================================
# Chapter 5: postpartum ~6 weeks, 3 am. Settling a crying baby.
# =====================================================================
b5 = b4.copy()
x0, y0 = cell(4, -1)                      # balloon gone; armchair with a crumpled blanket
b5.alpha_composite(Image.open(f'{OUT}/room_base_ch2.png').crop((x0, y0 - 32, x0 + 32, y0 + 32)), (x0, y0 - 32))
R(b5, x0 + 7, y0 + 14, x0 + 22, y0 + 22, NAVY_L); L(b5, x0 + 8, y0 + 18, x0 + 21, y0 + 17, NAVY)
x0, y0 = cell(4, 3)                       # cards gone
b5.alpha_composite(Image.open(f'{OUT}/room_base_ch2.png').crop((x0, y0, x0 + 32, y0 + 32)), (x0, y0))
# night through the window
x0, y0 = cell(1, 5)
R(b5, x0 + 4, y0 + 6, x0 + 59, y0 + 34, NIGHT); R(b5, x0 + 4, y0 + 6, x0 + 59, y0 + 12, NIGHT_L)
R(b5, x0 + 31, y0 + 6, x0 + 32, y0 + 34, CREAM_D); R(b5, x0 + 4, y0 + 20, x0 + 59, y0 + 21, CREAM_D)
E(b5, x0 + 44, y0 + 8, x0 + 52, y0 + 16, MOON); E(b5, x0 + 47, y0 + 7, x0 + 54, y0 + 14, NIGHT_L)
for (sx, sy) in ((10, 9), (22, 14), (38, 26), (14, 28)): P(b5, x0 + sx, y0 + sy, MOON)
# clock at 3 am
x0, y0 = cell(-1, 5)
E(b5, x0 + 9, y0 + 9, x0 + 23, y0 + 23, WHITE); L(b5, x0 + 16, y0 + 16, x0 + 16, y0 + 11, OL); L(b5, x0 + 16, y0 + 16, x0 + 20, y0 + 16, OL)
# static desk at (-3,-2): phone face down
stamp(b5, desk_phone('down'), -3, -2)
save(b5, 'room_base_ch5')

def sterilise(state):
    im = sink(False)
    def bottle(x, y, upside=False, dirty=False):
        R(im, x, y, x + 3, y + 7, MILK if not dirty else MILK_D)
        if upside: R(im, x, y + 7, x + 3, y + 9, SKY)
        else: R(im, x, y - 2, x + 3, y, SKY)
        if dirty: P(im, x + 1, y + 4, BOX_D)
        ImageDraw.Draw(im).rectangle([x, y - (0 if upside else 2), x + 3, y + (9 if upside else 7)], outline=OL)
    if state in ('todo', 'trace'):
        for k, x in enumerate((6, 11, 16, 21)): bottle(x, 6 + (k % 2), dirty=True)
    else:
        R(im, 4, 2, 27, 4, STEEL_D)                                                         # drying rack
        for x in (6, 12, 18, 24): bottle(x, 4, upside=True)
    return im
save(sterilise('todo'), 'bottles_dirty'); save(sterilise('done'), 'bottles_clean')

def tea(state, frame=0):
    im = stove(False)
    if state == 'todo':
        R(im, 18, 6, 27, 14, OL); R(im, 19, 7, 26, 13, STEEL_L); L(im, 17, 9, 18, 9, OL)  # kettle
        for k in range(3): P(im, 22 + (k + frame) % 2, 4 - k * 2, STEAM)
    else:
        R(im, 19, 8, 25, 14, OL); R(im, 20, 9, 24, 13, WHITE); R(im, 20, 9, 24, 10, BROWN if state == 'done' else (150, 120, 90, 255))
        P(im, 26, 10, OL); P(im, 26, 11, OL)
        if state == 'done':
            for k in range(3): P(im, 21 + k % 2, 6 - k * 2, STEAM)
        else:
            L(im, 20, 10, 24, 10, (200, 180, 150, 255))                                       # cold, a skin on top
    return im
save(tea('todo', 0), 'kettle_0'); save(tea('todo', 1), 'kettle_1'); save(tea('done'), 'tea_hot'); save(tea('cold'), 'tea_cold')

def bassinet(state, frame=0):
    im = new()
    tilt = {'active': (-1, 1, -1, 1)[frame % 4]}.get(state, 0)
    R(im, 3, 14 + tilt, 28, 27 - tilt, WHITE); R(im, 3, 14 + tilt, 28, 16 + tilt, MILK_D)
    R(im, 5, 27, 7, 31, OAK_S); R(im, 24, 27, 26, 31, OAK_S); L(im, 4, 31, 27, 31, OAK_D)   # stand
    st = {'idle': 'cry', 'active': 'fuss', 'done': 'sleep', 'cancel': 'cry'}[state]
    baby(im, 16, 12, st, frame)
    if state in ('idle', 'cancel'): cry_lines(im, 16, 12, frame)
    if state == 'active':
        for (x, y) in ((1, 4), (26, 3)):
            L(im, x, y + frame % 2, x + 3, y + frame % 2, NAVY_L)                             # "shh" waves
    if state == 'done': zz(im, 22, 5, frame)
    outline(im)
    return im
save(bassinet('idle', 0), 'bassinet_crying_0'); save(bassinet('idle', 1), 'bassinet_crying_1')
for f in range(4): save(bassinet('active', f), f'bassinet_rocking_{f}')
save(bassinet('done'), 'bassinet_asleep')

# =====================================================================
# Chapter 6: postpartum ~4 months. Getting ready for work.
# =====================================================================
b6 = b3.copy()
x0, y0 = cell(4, -1)                      # plain armchair again
b6.alpha_composite(Image.open(f'{OUT}/room_base_ch2.png').crop((x0, y0, x0 + 32, y0 + 32)), (x0, y0))
stamp(b6, stove(False), 0, 3)             # static stove
# a baby play mat on the rug (walkable decoration)
x0, y0 = cell(2, -1)
R(b6, x0 + 4, y0 + 10, x0 + 28, y0 + 26, PINK); R(b6, x0 + 7, y0 + 13, x0 + 25, y0 + 23, MINT)
L(b6, x0 + 4, y0 + 10, x0 + 16, y0 + 2, OAK_D); L(b6, x0 + 28, y0 + 10, x0 + 16, y0 + 2, OAK_D)
E(b6, x0 + 14, y0 + 4, x0 + 18, y0 + 8, MUST)
save(b6, 'room_base_ch6')

def nappy_bag(state):
    def extra(im):
        R(im, 5, 12, 20, 27, NAVY); R(im, 5, 24, 20, 27, NAVY_D); L(im, 8, 12, 12, 7, OL); L(im, 12, 7, 17, 12, OL)
        if state == 'done':
            L(im, 6, 14, 19, 14, MUST); R(im, 8, 9, 12, 13, WHITE)                            # zipped, nappy peeking
        else:
            R(im, 6, 12, 19, 15, NAVY_D)
            R(im, 22, 18, 29, 27, MINT); R(im, 23, 20, 28, 25, MILK)                         # nappy packet
            if state == 'trace':
                R(im, 23, 20, 28, 25, (0, 0, 0, 0)); R(im, 22, 18, 29, 19, MINT_D)            # packet empty
        outline(im)
    return mat_cell(extra)
save(nappy_bag('todo'), 'nappybag_todo'); save(nappy_bag('done'), 'nappybag_packed'); save(nappy_bag('trace'), 'nappybag_no_nappies')

def milk(state, frame=0):
    def extra(im):
        def bottle(x, label):
            R(im, x, 6, x + 4, 15, MILK); R(im, x, 3, x + 4, 5, SKY)
            if label: R(im, x, 9, x + 4, 11, MUST)
            ImageDraw.Draw(im).rectangle([x, 3, x + 4, 15], outline=OL)
        if state == 'todo':
            E(im, 4, 4, 12, 12, SKY_L); R(im, 7, 11, 9, 15, STEEL_D)                           # pump
            P(im, 8, 16 + frame % 2, MILK)
            bottle(16, False)
        else:
            bottle(5, state == 'done'); bottle(12, state == 'done'); bottle(19, state == 'done')
            if state == 'trace':
                R(im, 25, 9, 29, 14, STEEL_D)                                                 # pump parts left out
    return counter_cell(extra)
save(milk('todo', 0), 'milk_todo_0'); save(milk('todo', 1), 'milk_todo_1'); save(milk('done'), 'milk_labelled'); save(milk('trace'), 'milk_unlabelled')

def outfit(state, frame=0):
    im = new()
    # clothes rack
    L(im, 4, 4, 4, 28, STEEL_D); L(im, 27, 4, 27, 28, STEEL_D); L(im, 4, 4, 27, 4, STEEL_D); L(im, 2, 28, 29, 28, STEEL_D)
    colours = [NAVY, MUST, PLUM, TERRA, SAGE]
    def shirt(x, c):
        R(im, x, 6, x + 4, 16, c); L(im, x + 2, 4, x + 2, 6, OL)
    if state == 'idle':
        for k, x in enumerate((6, 11, 16, 21)): shirt(x, colours[k])
    elif state == 'active':
        for k, x in enumerate((6, 11, 21)): shirt(x, colours[k])
        c = colours[frame % len(colours)]
        R(im, 13, 15, 19, 25, c); R(im, 11, 16, 21, 18, c)                                  # held up against her
    elif state == 'done':
        for k, x in enumerate((6, 11, 16)): shirt(x, colours[k + 1])
        R(im, 19, 20, 27, 27, NAVY_D); L(im, 21, 20, 23, 17, OL); L(im, 23, 17, 25, 20, OL)  # work bag ready
    else:  # cancelled: rejected outfits on the floor
        shirt(6, NAVY)
        R(im, 3, 22, 13, 27, MUST); R(im, 11, 24, 21, 29, PLUM); R(im, 18, 21, 28, 26, TERRA)
    outline(im)
    return im
save(outfit('idle'), 'outfit_idle')
for f in range(4): save(outfit('active', f), f'outfit_trying_{f}')
save(outfit('done'), 'outfit_ready'); save(outfit('cancel'), 'outfit_pile')

print('chapters 3 to 6 done')

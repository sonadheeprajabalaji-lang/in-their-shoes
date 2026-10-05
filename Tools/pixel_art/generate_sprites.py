from PIL import Image, ImageDraw
import os, math, random
random.seed(7)
OUT='out'; os.makedirs(OUT,exist_ok=True)
T=32
# ---------- palette
OL=(43,34,51,255)
CREAM=(238,224,200,255); CREAM_D=(214,196,170,255)
WALL=(222,190,176,255); WALL_D=(204,170,158,255); WALL_STRIPE=(230,204,190,255)
TILE=(226,232,230,255); TILE_L=(196,206,206,255)
WOOD=(178,124,84,255); WOOD_L=(196,144,100,255); WOOD_D=(152,102,68,255); WOOD_S=(122,80,54,255)
OAK=(214,170,118,255); OAK_D=(184,138,92,255); OAK_S=(150,108,72,255)
TRIM=(104,74,62,255); TRIM_L=(132,98,82,255); TRIM_D=(78,54,46,255)
MARBLE=(236,232,224,255); MARBLE_D=(214,208,198,255)
CAB=(122,152,128,255); CAB_D=(96,124,104,255); CAB_L=(146,174,150,255)
STEEL=(186,196,204,255); STEEL_D=(140,152,162,255); STEEL_L=(222,228,234,255)
DARK=(62,60,70,255); DARK_L=(90,88,100,255)
TERRA=(200,104,78,255); TERRA_D=(160,76,58,255); TERRA_L=(222,140,110,255)
SAGE=(128,164,122,255); SAGE_D=(94,128,92,255); SAGE_L=(160,192,150,255)
LEAF=(92,150,96,255); LEAF_D=(62,112,72,255); LEAF_L=(132,184,120,255)
NAVY=(66,80,124,255); NAVY_D=(48,58,96,255); NAVY_L=(92,108,156,255)
MUST=(226,176,76,255); MUST_D=(190,138,52,255)
PLUM=(126,98,164,255); PLUM_D=(98,74,132,255)
WHITE=(248,246,240,255); BROWN=(120,84,56,255)
SKIN=(236,190,156,255); SKIN_D=(208,156,124,255)
HAIR_H=(84,52,40,255); HAIR_H_L=(112,72,54,255)
HAIR_P=(46,38,36,255); HAIR_P_L=(70,58,54,255)
PANTS=(70,72,92,255); PANTS_D=(52,54,72,255)
SHOE=(52,40,44,255); BLUSH=(232,150,140,255)
SKY=(168,206,226,255); SKY_L=(204,228,238,255); CURT=(236,214,160,255); CURT_D=(212,186,128,255)
STEAM=(226,238,220,200)
T0=(0,0,0,0)

def new(w=T,h=T): return Image.new('RGBA',(w,h),T0)
def R(im,x0,y0,x1,y1,c): ImageDraw.Draw(im).rectangle([x0,y0,x1,y1],fill=c)
def P(im,x,y,c):
    if 0<=x<im.width and 0<=y<im.height: im.putpixel((x,y),c)
def E(im,x0,y0,x1,y1,c): ImageDraw.Draw(im).ellipse([x0,y0,x1,y1],fill=c)
def L(im,x0,y0,x1,y1,c): ImageDraw.Draw(im).line([x0,y0,x1,y1],fill=c)
def outline(im,c=OL):
    src=im.copy(); px=src.load(); w,h=im.size
    for y in range(h):
        for x in range(w):
            if px[x,y][3]==0:
                for dx,dy in((1,0),(-1,0),(0,1),(0,-1)):
                    nx,ny=x+dx,y+dy
                    if 0<=nx<w and 0<=ny<h and px[nx,ny][3]>0 and px[nx,ny]!=c:
                        im.putpixel((x,y),c);break
    return im
def rrect(im,x0,y0,x1,y1,c):
    R(im,x0+1,y0,x1-1,y1,c); R(im,x0,y0+1,x1,y1-1,c)
def save(im,name): im.save(f'{OUT}/{name}.png')

# ---------- counters (used in tiles and task sprites)
def counter(im,ox=0,oy=0,w=32,top=True):
    R(im,ox,oy+6,ox+w-1,oy+17,MARBLE); R(im,ox,oy+16,ox+w-1,oy+17,MARBLE_D)
    R(im,ox,oy+18,ox+w-1,oy+31,CAB); R(im,ox,oy+18,ox+w-1,oy+18,CAB_D)
    R(im,ox,oy+30,ox+w-1,oy+31,CAB_D)
    for x in (ox+1,ox+16):
        R(im,x,oy+20,x+13,oy+28,CAB_L); R(im,x+1,oy+21,x+12,oy+27,CAB)
    R(im,ox+12,oy+23,ox+13,oy+25,MUST); R(im,ox+18,oy+23,ox+19,oy+25,MUST)
    L(im,ox,oy+5,ox+w-1,oy+5,OL)
def counter_tile(extra=None):
    im=new(); counter(im)
    if extra: extra(im)
    return im

# ---------- room base (11 x 10 cells: x -5..5, y 5..-4)
W,H=11,10
base=Image.new('RGBA',(W*T,H*T),(34,28,40,255))
def cell(cx,cy): return ((cx+5)*T,(5-cy)*T)
# floor planks
for cy in range(-3,4):
    for cx in range(-4,5):
        x0,y0=cell(cx,cy)
        for r in range(4):
            yy=y0+r*8
            shade=[WOOD,WOOD_L,WOOD,WOOD_D][(r+cx*3+cy)%4]
            R(base,x0,yy,x0+31,yy+7,shade); L(base,x0,yy+7,x0+31,yy+7,WOOD_S)
            off=(r*11+cx*7+cy*5)%32
            L(base,x0+off,yy,x0+off,yy+6,WOOD_S)
            if random.random()<0.5: P(base,x0+random.randrange(32),yy+random.randrange(1,6),WOOD_D)
# rug under coffee table/couch area x 1..4, y -1..-3
rx0,ry0=cell(1,-1); rx1,ry1=cell(4,-3); rx1+=31; ry1+=31
R(base,rx0+4,ry0+6,rx1-4,ry1-2,TERRA); R(base,rx0+7,ry0+9,rx1-7,ry1-5,CREAM)
R(base,rx0+9,ry0+11,rx1-9,ry1-7,TERRA_L)
for x in range(rx0+12,rx1-12,8):
    for y in range(ry0+14,ry1-10,8):
        R(base,x,y,x+2,y+2,TERRA); 
for x in range(rx0+4,rx1-3,3): P(base,x,ry0+5,CREAM); P(base,x,ry1-1,CREAM)
# door mat at (0,-3)
mx,my=cell(0,-3); R(base,mx+4,my+16,mx+27,my+29,SAGE_D); R(base,mx+6,my+18,mx+25,my+27,SAGE)
# upper wall row y=5 & backsplash y=4
for cx in range(-5,6):
    x0,y0=cell(cx,5); R(base,x0,y0,x0+31,y0+31,WALL)
    for sx in range(x0,x0+32,8): R(base,sx+3,y0,sx+4,y0+31,WALL_STRIPE)
    x0,y0=cell(cx,4); R(base,x0,y0,x0+31,y0+31,TILE)
    for k in range(0,32,8): L(base,x0,y0+k,x0+31,y0+k,TILE_L); L(base,x0+k,y0,x0+k,y0+31,TILE_L)
# wall cabinets (upper) at x -3,-2,3,4
def wallcab(cx):
    x0,y0=cell(cx,5)
    R(base,x0+1,y0+6,x0+30,y0+31,CAB); R(base,x0+1,y0+29,x0+30,y0+31,CAB_D)
    R(base,x0+3,y0+8,x0+14,y0+27,CAB_L); R(base,x0+4,y0+9,x0+13,y0+26,CAB)
    R(base,x0+17,y0+8,x0+28,y0+27,CAB_L); R(base,x0+18,y0+9,x0+27,y0+26,CAB)
    R(base,x0+12,y0+24,x0+12,y0+26,MUST); R(base,x0+19,y0+24,x0+19,y0+26,MUST)
    ImageDraw.Draw(base).rectangle([x0,y0+5,x0+31,y0+31],outline=OL)
for cx in (-3,-2,3,4): wallcab(cx)
# range hood at x=0
x0,y0=cell(0,5); R(base,x0+4,y0+4,x0+27,y0+20,STEEL); R(base,x0+1,y0+20,x0+30,y0+29,STEEL_D); R(base,x0+2,y0+21,x0+29,y0+24,STEEL)
ImageDraw.Draw(base).rectangle([x0+4,y0+3,x0+27,y0+20],outline=OL); ImageDraw.Draw(base).rectangle([x0+1,y0+20,x0+30,y0+29],outline=OL)
# clock at x=-1
x0,y0=cell(-1,5); E(base,x0+8,y0+8,x0+24,y0+24,OL); E(base,x0+9,y0+9,x0+23,y0+23,WHITE)
L(base,x0+16,y0+16,x0+16,y0+11,OL); L(base,x0+16,y0+16,x0+20,y0+16,OL)
# window at x 1..2 (y=5 and top of y=4)
x0,y0=cell(1,5)
R(base,x0+2,y0+4,x0+61,y0+36,OL); R(base,x0+4,y0+6,x0+59,y0+34,SKY)
R(base,x0+4,y0+6,x0+59,y0+12,SKY_L); R(base,x0+31,y0+6,x0+32,y0+34,WHITE); R(base,x0+4,y0+20,x0+59,y0+21,WHITE)
R(base,x0+0,y0+35,x0+63,y0+38,CREAM); L(base,x0,y0+38,x0+63,y0+38,OL)
for cx_off,side in ((x0+2,1),(x0+50,-1)):
    R(base,cx_off,y0+2,cx_off+11,y0+34,CURT)
    for k in range(cx_off+2,cx_off+11,3): L(base,k,y0+3,k,y0+33,CURT_D)
R(base,x0,y0+1,x0+63,y0+2,TRIM)
# picture frame at x=-4 upper? (cabinet there? x=-4 upper is above fridge -> small frame)
x0,y0=cell(-4,5); R(base,x0+8,y0+6,x0+23,y0+21,OL); R(base,x0+9,y0+7,x0+22,y0+20,CREAM); R(base,x0+10,y0+12,x0+21,y0+19,SAGE); E(base,x0+15,y0+8,x0+19,y0+12,MUST)
# shelf with plant x=-1? clock there. Small plant shelf at x=-5? corner -> skip
# side & bottom wall tops
for cy in range(-4,6):
    for cx in (-5,5):
        x0,y0=cell(cx,cy)
        if cy>=4: continue
        R(base,x0,y0,x0+31,y0+31,(34,28,40,255))
        if cx==-5: R(base,x0+20,y0,x0+31,y0+31,TRIM); R(base,x0+28,y0,x0+31,y0+31,TRIM_L); L(base,x0+20,y0,x0+20,y0+31,OL)
        else: R(base,x0,y0,x0+11,y0+31,TRIM); R(base,x0,y0,x0+3,y0+31,TRIM_L); L(base,x0+11,y0,x0+11,y0+31,OL)
for cx in range(-5,6):
    x0,y0=cell(cx,-4)
    R(base,x0,y0,x0+31,y0+31,(34,28,40,255))
    if cx in (-5,5): continue
    R(base,x0,y0,x0+31,y0+11,TRIM); R(base,x0,y0,x0+31,y0+3,TRIM_L); L(base,x0,y0+11,x0+31,y0+11,OL)
# corners bottom
x0,y0=cell(-5,-4); R(base,x0+20,y0,x0+31,y0+11,TRIM); R(base,x0+28,y0,x0+31,y0+3,TRIM_L); L(base,x0+20,y0,x0+20,y0+11,OL); L(base,x0+20,y0+11,x0+31,y0+11,OL)
x0,y0=cell(5,-4); R(base,x0,y0,x0+11,y0+11,TRIM); R(base,x0,y0,x0+3,y0+3,TRIM_L); L(base,x0+11,y0,x0+11,y0+11,OL); L(base,x0,y0+11,x0+11,y0+11,OL)
# door gap at bottom (0,-4)
x0,y0=cell(0,-4); R(base,x0+2,y0,x0+29,y0+11,OAK_D); R(base,x0+2,y0,x0+29,y0+2,OAK); L(base,x0+2,y0+11,x0+29,y0+11,OL)
# side walls also on y=4 & 5 edges (frame the top wall)
for cy in (4,5):
    for cx in (-5,5):
        x0,y0=cell(cx,cy)
        R(base,x0,y0,x0+31,y0+31,(34,28,40,255))
        if cx==-5: R(base,x0+20,y0,x0+31,y0+31,TRIM); R(base,x0+28,y0,x0+31,y0+31,TRIM_L); L(base,x0+20,y0,x0+20,y0+31,OL)
        else: R(base,x0,y0,x0+11,y0+31,TRIM); R(base,x0,y0,x0+3,y0+31,TRIM_L); L(base,x0+11,y0,x0+11,y0+31,OL)
# skirting line between wall and floor at y=4 bottom handled by counters
# counters (static) at x -2,-1,1,2 and 4 (fruit bowl)
for cx in (-2,-1,1,2,4):
    x0,y0=cell(cx,3); counter(base,x0,y0)
    # extend backsplash shadow
x0,y0=cell(4,3)
E(base,x0+6,y0+4,x0+25,y0+14,OL); E(base,x0+7,y0+5,x0+24,y0+13,WHITE)
for (fx,fy,c) in ((10,5,TERRA),(15,4,MUST),(19,6,SAGE_L),(13,8,MUST_D)):
    E(base,x0+fx,y0+fy,x0+fx+4,y0+fy+4,c)
x0,y0=cell(-1,3)  # utensil jar
R(base,x0+20,y0+6,x0+25,y0+14,TERRA); L(base,x0+21,y0+2,x0+21,y0+6,BROWN); L(base,x0+23,y0+1,x0+23,y0+6,STEEL_D); L(base,x0+24,y0+3,x0+24,y0+6,BROWN)
x0,y0=cell(1,3)  # cutting board + crackers box
R(base,x0+4,y0+8,x0+19,y0+15,OAK); R(base,x0+4,y0+15,x0+19,y0+15,OAK_D)
R(base,x0+22,y0+2,x0+28,y0+14,MUST); R(base,x0+23,y0+4,x0+27,y0+8,WHITE)
# fridge at x=-4, spans y=3 and y=4
x0,y0=cell(-4,4)
R(base,x0+1,y0+4,x0+30,y0+63,STEEL_L); R(base,x0+1,y0+4,x0+30,y0+6,WHITE)
L(base,x0+1,y0+26,x0+30,y0+26,STEEL_D); R(base,x0+25,y0+12,x0+26,y0+22,STEEL_D); R(base,x0+25,y0+30,x0+26,y0+44,STEEL_D)
R(base,x0+1,y0+58,x0+30,y0+63,STEEL)
ImageDraw.Draw(base).rectangle([x0,y0+3,x0+31,y0+63],outline=OL)
R(base,x0+6,y0+32,x0+12,y0+38,MUST); R(base,x0+14,y0+34,x0+19,y0+39,SAGE_L)  # magnets/note
# desk extension at (-4,-2) with lamp and mug
def desk(im,ox,oy):
    R(im,ox,oy+8,ox+31,oy+21,OAK); R(im,ox,oy+20,ox+31,oy+23,OAK_D); L(im,ox,oy+7,ox+31,oy+7,OL); L(im,ox,oy+24,ox+31,oy+24,OL)
x0,y0=cell(-4,-2); desk(base,x0,y0)
R(base,x0+2,y0+24,x0+4,y0+31,OAK_S); L(base,x0+1,y0+24,x0+1,y0+31,OL); L(base,x0+5,y0+24,x0+5,y0+31,OL)
L(base,x0,y0+7,x0,y0+24,OL)
E(base,x0+5,y0+11,x0+13,y0+17,DARK); L(base,x0+9,y0+14,x0+9,y0+2,DARK); E(base,x0+4,y0-2,x0+16,y0+6,MUST); R(base,x0+4,y0+2,x0+16,y0+6,MUST); L(base,x0+4,y0+6,x0+16,y0+6,MUST_D)
R(base,x0+20,y0+10,x0+25,y0+17,WHITE); P(base,x0+26,y0+12,WHITE); P(base,x0+26,y0+13,WHITE)
# chair at (-3,-3)? keep floor clear. plant at (-4,-3)
def plant(im,ox,oy,big=True):
    R(im,ox+10,oy+20,ox+21,oy+30,TERRA); R(im,ox+9,oy+19,ox+22,oy+21,TERRA_D); R(im,ox+10,oy+28,ox+21,oy+30,TERRA_D)
    for (a,b,c,d,col) in ((6,4,16,16,LEAF_D),(14,2,26,14,LEAF),(8,10,18,20,LEAF),(15,9,25,20,LEAF_D),(11,6,20,13,LEAF_L)):
        E(im,ox+a,oy+b,ox+c,oy+d,col)
    p=new(); return
x0,y0=cell(-4,-3); tmp=new(); plant(tmp,0,0); outline(tmp); base.alpha_composite(tmp,(x0,y0))
x0,y0=cell(4,-3); tmp=new(); plant(tmp,0,0); outline(tmp); base.alpha_composite(tmp,(x0,y0))
# couch at (2..3,-3)
cw=new(64,32)
R(cw,0,6,63,29,SAGE); R(cw,0,20,63,29,SAGE_D)   # body
R(cw,0,4,7,29,SAGE_L); R(cw,56,4,63,29,SAGE_L)  # arms
R(cw,8,6,31,19,SAGE_L); R(cw,32,6,55,19,SAGE_L); L(cw,31,6,31,19,SAGE_D)  # seat cushions
R(cw,8,20,55,27,SAGE); L(cw,8,20,55,20,SAGE_D)  # backrest top (front)
R(cw,10,12,19,19,PLUM); R(cw,44,12,53,19,MUST)  # pillows
outline(cw); x0,y0=cell(2,-3); base.alpha_composite(cw,(x0,y0))
# coffee table at (2..3,-2) with crackers + water
tb=new(64,32)
E(tb,4,8,59,26,OAK); E(tb,4,10,59,28,OAK_D); E(tb,4,8,59,25,OAK)
R(tb,10,24,12,31,OAK_S); R(tb,51,24,53,31,OAK_S)
E(tb,12,10,30,20,WHITE); E(tb,14,12,28,18,(240,236,226,255))
for (a,b) in ((16,13),(21,12),(23,15)): R(tb,a,b,a+3,b+2,MUST)
R(tb,40,8,45,18,SKY_L); R(tb,40,13,45,18,SKY); L(tb,40,8,45,8,WHITE)
outline(tb); x0,y0=cell(2,-2); base.alpha_composite(tb,(x0,y0))
# laundry basket at (4,1)
lb=new()
R(lb,6,12,25,29,MUST_D); R(lb,5,11,26,13,MUST)
for x in range(7,25,3): L(lb,x,14,x,28,MUST)
E(lb,7,4,16,14,NAVY_L); E(lb,13,3,24,13,TERRA_L); E(lb,10,7,20,15,WHITE); E(lb,18,6,26,14,SAGE_L)
outline(lb); x0,y0=cell(4,1); base.alpha_composite(lb,(x0,y0))
save(base,'room_base')

# ---------- task sprites (32x32, include counter where relevant)
def sink(dirty):
    im=new(); counter(im)
    R(im,4,7,27,16,STEEL_D); R(im,5,8,26,15,STEEL); R(im,5,8,26,9,STEEL_D)
    R(im,15,1,16,7,STEEL_D); R(im,13,1,18,2,STEEL_D)
    if dirty:
        for (a,b) in ((6,6),(11,4),(16,7)):
            E(im,a,b,a+9,b+8,OL); E(im,a+1,b+1,a+8,b+7,WHITE); P(im,a+3,b+3,BROWN); P(im,a+5,b+4,TERRA_D); P(im,a+4,b+5,BROWN)
        R(im,22,4,27,12,OL); R(im,23,5,26,11,TERRA)
        L(im,27,6,30,4,OL)
        for (a,b) in ((9,15),(20,14),(14,16)): P(im,a,b,WHITE)
    else:
        R(im,21,2,28,6,OL); R(im,22,3,27,5,SAGE_L)  # dish rack hint / towel
        L(im,6,16,25,16,WHITE)
    return im
save(sink(True),'sink_dirty'); save(sink(False),'sink_clean')
def stove(cooking,frame=0,steam=True):
    im=new()
    R(im,0,6,31,17,DARK); R(im,0,16,31,17,DARK_L)
    R(im,0,18,31,31,STEEL); R(im,0,18,31,18,STEEL_D); R(im,3,21,28,28,DARK); R(im,4,22,27,27,DARK_L); R(im,6,23,25,23,STEEL_D)
    for x in (6,13,20,27): P(im,x,19,OL)
    L(im,0,5,31,5,OL)
    for cx in (9,22):
        ImageDraw.Draw(im).ellipse([cx-5,8,cx+5,15],outline=STEEL_D); ImageDraw.Draw(im).ellipse([cx-2,10,cx+2,13],outline=DARK_L)
    if cooking:
        E(im,4,5,18,15,OL); E(im,5,6,17,14,TERRA); E(im,6,6,16,10,TERRA_L); R(im,9,6,13,7,DARK)
        L(im,18,10,23,10,OL)
        off=frame
        if not steam: off=None
        for (x,y) in (((8,3-off),(11,1-off),(14,3-off),(9,-1-off)) if off is not None else ()):
            for k in range(3):
                P(im,x+((k+frame)%2),y-k*2+2,STEAM); P(im,x+((k+frame)%2),y-k*2+3,STEAM)
        P(im,8,8,(255,140,80,255))
    else:
        pass
    return im
save(stove(True,0,False),'stove_cold'); save(stove(True,0),'stove_cooking_0'); save(stove(True,1),'stove_cooking_1'); save(stove(False),'stove_off')
def coffee(full,frame=0,steam=True):
    im=new(); counter(im)
    R(im,5,0,17,15,OL); R(im,6,1,16,14,DARK); R(im,6,1,16,4,DARK_L); R(im,8,7,14,9,OL); P(im,14,3,(120,220,140,255))
    R(im,6,14,16,16,DARK_L)
    if full:
        R(im,20,8,26,14,OL); R(im,21,9,25,13,WHITE); R(im,21,9,25,10,BROWN); P(im,27,10,OL); P(im,27,11,OL)
        for k in (range(3) if steam else []): P(im,22+((k+frame)%2),6-k*2,STEAM); P(im,24-((k+frame)%2),5-k*2,STEAM)
    return im
save(coffee(True,0),'coffee_full_0'); save(coffee(True,1),'coffee_full_1'); save(coffee(False),'coffee_done'); save(coffee(True,0,False),'coffee_cold')
def laptop(state,frame=0):
    im=new(); desk(im,0,0)
    R(im,29,24,31,31,OAK_S); L(im,28,24,28,31,OL); L(im,31,7,31,24,OL)
    if state=='closed':
        R(im,7,13,24,19,OL); R(im,8,14,23,18,STEEL); L(im,8,14,23,14,STEEL_L)
    else:
        R(im,7,0,24,13,OL); R(im,8,1,23,12,(36,44,70,255))
        R(im,6,13,25,19,OL); R(im,7,14,24,18,STEEL); 
        for x in range(9,23,2): P(im,x,15,STEEL_D); P(im,x+1,17,STEEL_D)
        if state=='idle':
            R(im,10,4,21,5,(110,140,200,255)); R(im,10,7,17,8,(80,100,150,255))
        else:
            tiles=((9,2),(16,2),(9,7),(16,7))
            cols=((SKIN,HAIR_P),(SKIN_D,HAIR_H),((222,170,130,255),MUST_D),(SKIN,NAVY))
            for i,((tx,ty),(sk,hr)) in enumerate(zip(tiles,cols)):
                R(im,tx,ty,tx+5,ty+3,(70,84,120,255))
                talking = (i==frame%4)
                P(im,tx+2,ty+1,sk); P(im,tx+3,ty+1,sk); P(im,tx+2,ty+2,sk); P(im,tx+3,ty+2,sk); P(im,tx+2,ty,hr); P(im,tx+3,ty,hr)
                if talking: ImageDraw.Draw(im).rectangle([tx,ty,tx+5,ty+3],outline=(140,220,150,255))
    return im
save(laptop('idle'),'laptop_idle'); save(laptop('closed'),'laptop_closed')
for f in range(4): save(laptop('call',f),f'laptop_call_{f}')

# ---------- characters
def person(kind,facing,frame):
    im=new()
    her = kind=='her'
    hair,hairl = (HAIR_H,HAIR_H_L) if her else (HAIR_P,HAIR_P_L)
    top,topd = (SAGE,SAGE_D) if her else (NAVY,NAVY_D)
    step = frame==1
    # legs
    if facing in ('down','up'):
        ly=(1 if step else 0)
        R(im,12,24,14,28-ly,PANTS); R(im,17,24,19,28-(0 if step else 0)+(-1 if not step else 0)+ (1 if step else 0),PANTS)
        R(im,12,29-ly,14,30-ly,SHOE); R(im,17,29,19,30,SHOE) if not step else (R(im,17,28,19,29,PANTS),R(im,17,29,19,30,SHOE))
    else:
        if step: R(im,13,24,15,28,PANTS_D); R(im,13,29,16,30,SHOE); R(im,17,24,19,27,PANTS); R(im,18,28,21,29,SHOE)
        else: R(im,14,24,17,28,PANTS); R(im,14,29,18,30,SHOE)
    # torso
    if facing in ('down','up'):
        rrect(im,10,15,21,25,top); R(im,10,23,21,25,topd)
        if her and facing=='down': R(im,15,16,16,24,CREAM)
        if not her and facing=='down': R(im,14,15,17,16,SKIN_D)
        # arms
        ay = -1 if step else 0
        R(im,8,16,9,22+ay,top); R(im,22,16,23,22-ay,top)
        R(im,8,23+ay,9,24+ay,SKIN); R(im,22,23-ay,23,24-ay,SKIN)
    else:
        rrect(im,12,15,19,25,top); R(im,12,23,19,25,topd)
        ax = 1 if step else 0
        R(im,15-ax,17,16-ax,22,topd); R(im,15-ax,23,16-ax,24,SKIN)
    # head
    if facing=='down':
        if her: R(im,9,7,22,20,hair)          # long hair behind
        E(im,10,3,21,15,SKIN)
        if her:
            E(im,9,1,22,10,hair); R(im,9,5,11,14,hair); R(im,20,5,22,14,hair); R(im,13,2,18,3,hairl)
        else:
            E(im,9,1,22,9,hair); R(im,9,5,10,10,hair); R(im,21,5,22,10,hair); R(im,13,2,18,3,hairl)
        P(im,13,11,OL); P(im,18,11,OL); P(im,13,10,OL) if False else None
        P(im,12,13,BLUSH); P(im,19,13,BLUSH); L(im,15,13,16,13,SKIN_D)
    elif facing=='up':
        E(im,10,3,21,15,SKIN)
        if her: E(im,9,1,22,14,hair); R(im,9,7,22,21,hair); R(im,13,2,18,4,hairl); R(im,11,18,20,21,hairl)
        else: E(im,9,1,22,13,hair); R(im,13,2,18,4,hairl)
    else: # right
        E(im,10,3,21,15,SKIN)
        if her:
            E(im,9,1,21,10,hair); R(im,9,4,14,20,hair); R(im,10,17,14,21,hairl); R(im,12,2,17,3,hairl)
        else:
            E(im,9,1,21,9,hair); R(im,9,4,14,11,hair); R(im,12,2,17,3,hairl)
        P(im,18,10,OL); P(im,19,13,BLUSH); P(im,21,9,SKIN); R(im,15,9,15,11,SKIN_D)
    outline(im)
    return im
for kind in ('her','partner'):
    for facing in ('down','up','side'):
        for fr in (0,1):
            save(person(kind,'right' if facing=='side' else facing,fr),f'{kind}_{facing}_{fr}')

# ---------- markers & UI
m=new(16,16)
for (x,y) in ((7,1),(8,1),(7,14),(8,14),(1,7),(1,8),(14,7),(14,8)): pass
R(m,7,2,8,13,(255,236,170,255)); R(m,2,7,13,8,(255,236,170,255)); R(m,6,6,9,9,WHITE); R(m,5,5,10,10,(255,236,170,255)); R(m,6,6,9,9,WHITE)
outline(m,(120,90,40,255)); save(m,'trace_marker')
def panel(name,fill,border,size=24,b=6):
    im=new(size,size)
    R(im,1,0,size-2,size-1,OL); R(im,0,1,size-1,size-2,OL)
    R(im,2,1,size-3,size-2,border); R(im,1,2,size-2,size-3,border)
    R(im,3,3,size-4,size-4,fill)
    save(im,name)
panel('ui_panel',(43,34,51,235),(150,120,150,255))
panel('ui_button',(240,228,206,255),(126,98,164,255))
panel('ui_meter_bg',(30,24,36,255),(110,92,116,255),16,4)
f=new(8,8); R(f,0,0,7,7,WHITE); R(f,0,6,7,7,(214,214,214,255)); R(f,0,0,7,1,(255,255,255,255)); save(f,'ui_meter_fill')
# chunky vignette 64x36
v=Image.new('RGBA',(64,36),T0)
for y in range(36):
    for x in range(64):
        dx=(x-31.5)/32; dy=(y-17.5)/18; d=math.sqrt(dx*dx+dy*dy)
        a=max(0,min(1,(d-0.55)/0.6)); a=round(a*4)/4
        v.putpixel((x,y),(255,255,255,int(a*255)))
save(v,'vignette')
print('done', len(os.listdir(OUT)))

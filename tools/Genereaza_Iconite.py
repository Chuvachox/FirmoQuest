# -*- coding: utf-8 -*-
"""Generează iconițele interfeței (P0.2 din propunerea UX).

Pictograme plate, în culorile exacte ale jocului (Pal din game/ui/Ui.cs),
fără artist și fără resurse externe. Se rulează oricând se schimbă paleta:

    python tools/Genereaza_Iconite.py          ->  res://assets/ui/icons/*.png (29 fișiere)

Desenarea se face la 4× și se reduc la 64 px (LANCZOS) pentru margini netede.
Fișierele sunt importate de editorul Godot la prima rulare (--headless --editor).
"""
import os
from PIL import Image, ImageDraw, ImageFont

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(BASE, "game", "assets", "ui", "icons")
S, SS = 64, 4          # mărime finală / oversampling
C = S * SS

# paleta jocului (game/ui/Ui.cs -> Pal)
BORDER = "#2e3846"
TEXT = "#e8edf3"
DIM = "#93a1b3"
DARKT = "#6c7a8c"
ACCENT = "#3d8bfd"
GREEN = "#37a05a"
RED = "#d0524a"
GOLD = "#e6b23c"
PURPLE = "#8b6fd0"
DARKG = "#245c39"      # verde închis (umbre/contururi)
DARKY = "#8a6a17"      # auriu închis


def k(v):
    return int(round(v * SS))


def new():
    im = Image.new("RGBA", (C, C), (0, 0, 0, 0))
    return im, ImageDraw.Draw(im)


def rect(d, box, r, fill=None, outline=None, w=2):
    d.rounded_rectangle([k(box[0]), k(box[1]), k(box[2]), k(box[3])],
                        radius=k(r), fill=fill, outline=outline, width=k(w))


def circ(d, box, fill=None, outline=None, w=2):
    d.ellipse([k(box[0]), k(box[1]), k(box[2]), k(box[3])],
              fill=fill, outline=outline, width=k(w))


def poly(d, pts, fill=None, outline=None, w=2):
    p = [(k(x), k(y)) for x, y in pts]
    d.polygon(p, fill=fill, outline=outline, width=k(w))


def line(d, pts, fill, w=3):
    d.line([(k(x), k(y)) for x, y in pts], fill=fill, width=k(w), joint="curve")


def dtext(d, xy, s, size, fill, bold=True):
    """Text centrat cu fontul Windows (folosit pentru „?” din emblemă/logou)."""
    name = "arialbd.ttf" if bold else "arial.ttf"
    fp = os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts", name)
    if not os.path.exists(fp):
        return
    font = ImageFont.truetype(fp, k(size))
    d.text((k(xy[0]), k(xy[1])), s, font=font, fill=fill, anchor="mm")


# ============================================================ pictograme
def i_grau(d):
    line(d, [(32, 58), (32, 16)], GOLD, 3)
    for i in range(3):
        y = 44 - i * 11
        for s in (-1, 1):
            poly(d, [(32, y), (32 + s * 12, y - 4), (32 + s * 14, y - 14), (32 + s * 3, y - 9)], GOLD)
    poly(d, [(32, 12), (26, 22), (32, 26), (38, 22)], GOLD)


def i_porumb(d):
    poly(d, [(32, 8), (44, 24), (44, 46), (32, 58), (20, 46), (20, 24)], GOLD)
    poly(d, [(32, 12), (40, 26), (40, 44), (32, 54)], "#f3c95e")
    for yy in (24, 34, 44):
        line(d, [(22, yy), (42, yy)], DARKY, 2)
    poly(d, [(20, 44), (8, 52), (16, 58), (26, 50)], GREEN)
    poly(d, [(44, 44), (56, 52), (48, 58), (38, 50)], GREEN)


def i_legume(d):
    circ(d, [12, 20, 52, 58], RED)
    circ(d, [18, 26, 34, 42], "#e07a72")
    poly(d, [(32, 22), (24, 14), (30, 12), (32, 18)], GREEN)
    poly(d, [(32, 22), (40, 14), (34, 12), (32, 18)], GREEN)
    line(d, [(32, 10), (32, 20)], DARKG, 3)


def i_site(d):
    rect(d, [8, 14, 56, 54], 5, "#1d2836", BORDER, 2)
    rect(d, [8, 14, 56, 26], 5, ACCENT)
    circ(d, [13, 18, 19, 24], TEXT)
    circ(d, [23, 18, 29, 24], "#cfe1fb")
    line(d, [(16, 36), (48, 36)], DIM, 3)
    line(d, [(16, 44), (38, 44)], DARKT, 3)


def i_mentenanta(d):
    circ(d, [18, 18, 46, 46], GOLD, DARKY, 3)
    circ(d, [27, 27, 37, 37], DARKY)
    for a in range(8):
        import math
        ang = a * math.pi / 4
        x1, y1 = 32 + 14 * math.cos(ang), 32 + 14 * math.sin(ang)
        x2, y2 = 32 + 23 * math.cos(ang), 32 + 23 * math.sin(ang)
        line(d, [(x1, y1), (x2, y2)], GOLD, 6)


def i_aplicatie(d):
    rect(d, [18, 8, 46, 56], 6, "#1d2836", ACCENT, 3)
    rect(d, [22, 16, 42, 46], 2, "#22304a")
    circ(d, [29, 49, 35, 55], ACCENT)
    line(d, [(26, 22), (38, 22)], DIM, 3)
    line(d, [(26, 29), (38, 29)], DARKT, 3)
    line(d, [(26, 36), (34, 36)], DARKT, 3)


def i_samanta(d):
    rect(d, [16, 26, 48, 54], 4, "#6b5a33")
    poly(d, [(14, 20), (50, 20), (48, 28), (16, 28)], "#8a7444")
    for x, y in [(24, 36), (34, 40), (40, 33), (28, 46)]:
        circ(d, [x - 4, y - 4, x + 4, y + 4], GOLD)


def i_rasaduri(d):
    line(d, [(32, 56), (32, 28)], GREEN, 4)
    poly(d, [(32, 30), (16, 26), (12, 14), (28, 20)], GREEN)
    poly(d, [(32, 26), (48, 22), (52, 10), (36, 16)], "#4cbf74")
    circ(d, [24, 52, 40, 58], "#6b5a33")


def i_echipament(d):
    rect(d, [10, 20, 54, 54], 4, "#2a3646", BORDER, 2)
    rect(d, [10, 20, 54, 30], 4, ACCENT)
    line(d, [(32, 30), (32, 54)], BORDER, 2)
    circ(d, [16, 40, 24, 48], GREEN)


def i_server(d):
    for i, y in enumerate((12, 28, 44)):
        rect(d, [12, y, 52, y + 12], 3, "#1d2836", ACCENT, 2)
        circ(d, [42, y + 4, 48, y + 10], ACCENT if i != 1 else GOLD)


def i_moneda(d):
    circ(d, [10, 10, 54, 54], GOLD)
    circ(d, [17, 17, 47, 47], "#f3c95e", DARKY, 3)
    dtext(d, (32, 33), "M", 22, DARKY)


def i_ceas(d):
    circ(d, [10, 10, 54, 54], "#1d2836", DIM, 4)
    line(d, [(32, 32), (32, 18)], TEXT, 4)
    line(d, [(32, 32), (43, 38)], GOLD, 4)
    circ(d, [29, 29, 35, 35], GOLD)


def i_om(d):
    circ(d, [22, 8, 42, 28], PURPLE)
    poly(d, [(32, 30), (14, 56), (50, 56)], PURPLE)


def i_cheie(d):
    circ(d, [10, 10, 34, 34], GOLD, DARKY, 6)
    line(d, [(28, 28), (52, 52)], GOLD, 8)
    line(d, [(44, 44), (50, 38)], GOLD, 6)
    line(d, [(48, 48), (54, 42)], GOLD, 6)


def i_banca(d):
    poly(d, [(32, 8), (58, 24), (6, 24)], GOLD)
    rect(d, [10, 26, 54, 32], 1, "#f3c95e")
    for x in (14, 26, 38, 46):
        rect(d, [x, 34, x + 6, 50], 1, "#f3c95e")
    rect(d, [8, 52, 56, 58], 1, GOLD)


def i_fulger(d):
    poly(d, [(36, 6), (18, 34), (30, 34), (24, 58), (46, 28), (34, 28), (42, 6)], GOLD)


def i_stampila(d):
    rect(d, [26, 8, 38, 22], 4, DIM)
    poly(d, [(22, 24), (42, 24), (46, 38), (18, 38)], DIM)
    rect(d, [12, 42, 52, 50], 3, "#3a4657", BORDER, 2)
    rect(d, [16, 52, 48, 58], 2, RED)


def i_soare(d):
    circ(d, [20, 20, 44, 44], GOLD)
    import math
    for a in range(8):
        ang = a * math.pi / 4
        line(d, [(32 + 15 * math.cos(ang), 32 + 15 * math.sin(ang)),
                 (32 + 25 * math.cos(ang), 32 + 25 * math.sin(ang))], GOLD, 5)


def i_frunda(d):
    poly(d, [(12, 52), (18, 20), (34, 8), (52, 14), (50, 34), (32, 52)], GREEN)
    line(d, [(16, 50), (44, 18)], DARKG, 3)
    for y in (28, 36):
        line(d, [(30, y), (44, y - 8)], DARKG, 2)


def i_cos(d):
    rect(d, [16, 18, 48, 56], 4, "#3a4657", BORDER, 2)
    rect(d, [12, 12, 52, 20], 4, DIM)
    rect(d, [26, 6, 38, 12], 3, DIM)
    for x in (24, 32, 40):
        line(d, [(x, 26), (x, 50)], BORDER, 3)


def i_blocare(d):
    rect(d, [8, 12, 56, 22], 2, RED)
    rect(d, [8, 32, 56, 42], 2, TEXT)
    rect(d, [14, 44, 22, 60], 2, DIM)
    rect(d, [42, 44, 50, 60], 2, DIM)
    for x in (14, 30, 46):
        line(d, [(x, 12), (x + 8, 22)], TEXT, 4)


def i_factura(d):
    poly(d, [(14, 8), (44, 8), (52, 18), (52, 58), (14, 58)], TEXT)
    poly(d, [(44, 8), (52, 18), (44, 18)], "#c3ccd8")
    for y in (26, 34, 42):
        line(d, [(20, y), (44, y)], DIM, 3)
    line(d, [(20, 50), (34, 50)], RED, 3)


def i_play(d):
    poly(d, [(18, 12), (52, 32), (18, 52)], TEXT)


def i_verifica(d):
    line(d, [(12, 32), (26, 46), (52, 16)], GREEN, 8)


def i_picatura(d):
    poly(d, [(32, 6), (48, 30), (48, 40), (32, 58), (16, 40), (16, 30)], ACCENT)
    circ(d, [22, 32, 32, 44], "#7fb4ff")


def i_panou(d):
    # panou solar: placă înclinată cu grilă, un picior lat (nu seamănă cu un coș)
    poly(d, [(8, 18), (56, 18), (48, 44), (16, 44)], "#274063", ACCENT, 3)
    for x in (20, 32, 44):
        line(d, [(x, 19), (x - 3, 43)], "#5ea3ff", 2)
    line(d, [(12, 31), (52, 31)], "#5ea3ff", 2)
    line(d, [(32, 44), (32, 52)], DIM, 5)
    rect(d, [20, 52, 44, 58], 2, DIM)


def i_intrebare(d):
    circ(d, [5, 5, 59, 59], None, ACCENT, 5)
    dtext(d, (32, 33), "?", 30, TEXT)


def i_iesire(d):
    # cadrul ușii + săgeată spre dreapta („ieși din joc”)
    line(d, [(13, 8), (13, 56)], DIM, 5)
    d.rectangle([k(26), k(28), k(46), k(36)], fill=TEXT)
    poly(d, [(43, 19), (58, 32), (43, 45)], TEXT)


def i_logo(d):
    """Emblema jocului: panou cu contur auriu, grafic în creștere, monedă în colț."""
    rect(d, [3, 3, 61, 61], 11, "#1a2028", GOLD, 3)
    # linia de bază
    line(d, [(12, 51), (54, 51)], BORDER, 2)
    # bare în creștere
    rect(d, [15, 39, 25, 50], 2, GREEN)
    rect(d, [29, 31, 39, 50], 2, ACCENT)
    rect(d, [43, 21, 53, 50], 2, GOLD)
    # moneda din colțul din dreapta sus (peste contur - dă adâncime)
    circ(d, [42, 4, 60, 22], GOLD)
    circ(d, [46, 8, 56, 18], "#f3c95e", DARKY, 2)


ICONS = {
    "grau": i_grau, "porumb": i_porumb, "legume": i_legume,
    "site": i_site, "mentenanta": i_mentenanta, "aplicatie": i_aplicatie,
    "samanta": i_samanta, "rasaduri": i_rasaduri, "echipament": i_echipament,
    "server": i_server, "moneda": i_moneda, "ceas": i_ceas,
    "om": i_om, "cheie": i_cheie, "banca": i_banca, "fulger": i_fulger,
    "stampila": i_stampila, "soare": i_soare, "frunza": i_frunda,
    "cos": i_cos, "blocare": i_blocare, "factura": i_factura,
    "play": i_play, "verifica": i_verifica, "picatura": i_picatura,
    "panou": i_panou, "intrebare": i_intrebare, "iesire": i_iesire,
    "logo": i_logo,
}


def sheet():
    """Previzualizare cu toate iconițele, pentru verificare vizuală."""
    cols, pad, cell = 7, 18, 64
    rows = (len(ICONS) + cols - 1) // cols
    im = Image.new("RGB", (cols * (cell + pad) + pad, rows * (cell + pad + 18) + pad), "#11151b")
    d = ImageDraw.Draw(im)
    from PIL import ImageFont
    font = None
    fp = os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts", "arial.ttf")
    if os.path.exists(fp):
        font = ImageFont.truetype(fp, 13)
    for n, (name, fn) in enumerate(ICONS.items()):
        r, c = divmod(n, cols)
        x = pad + c * (cell + pad)
        y = pad + r * (cell + pad + 18)
        icon, _ = new()
        fn(ImageDraw.Draw(icon))
        icon = icon.resize((cell, cell), Image.LANCZOS)
        d.rectangle([x - 4, y - 4, x + cell + 4, y + cell + 4], outline="#2e3846")
        im.paste(icon, (x, y), icon)
        d.text((x + cell / 2, y + cell + 6), name, fill="#93a1b3", anchor="ma", font=font)
    p = os.path.join(BASE, "shots", "icons_sheet.png")
    os.makedirs(os.path.dirname(p), exist_ok=True)
    im.save(p)
    return p


def main():
    os.makedirs(OUT, exist_ok=True)
    for name, fn in ICONS.items():
        im, d = new()
        fn(d)
        # emblema rămâne la 256 px (se afișează mare, în meniu)
        if name != "logo":
            im = im.resize((S, S), Image.LANCZOS)
        im.save(os.path.join(OUT, name + ".png"))
    print(f"{len(ICONS)} imagini salvate în {OUT}")
    print("previzualizare:", sheet())


if __name__ == "__main__":
    main()

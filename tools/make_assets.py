"""
Generates every non-code asset of the AR Flashcards project:
  * 5 feature-rich marker cards (1024x1024) used for Vuforia image tracking
  * pre-shaped Hindi / Tamil text images (Unity's legacy text cannot shape Indic scripts)
  * an A4 print sheet (PDF) with all cards at exactly 90 mm
Run:  python3 tools/make_assets.py   (needs Pillow with raqm, numpy, scipy)
"""
import math, os, random, sys
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter
from scipy.spatial import Delaunay

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RES = os.path.join(ROOT, "Assets", "ARFlashcards", "Resources", "ARFlashcards")
CARDS_DIR = os.path.join(ROOT, "Cards")
os.makedirs(os.path.join(RES, "Cards"), exist_ok=True)
os.makedirs(os.path.join(RES, "Text"), exist_ok=True)
os.makedirs(CARDS_DIR, exist_ok=True)

POPPINS_B = "/usr/share/fonts/truetype/google-fonts/Poppins-Bold.ttf"
POPPINS_R = "/usr/share/fonts/truetype/google-fonts/Poppins-Regular.ttf"
FREE_SANS = "/usr/share/fonts/truetype/freefont/FreeSans.ttf"
FREE_SANS_B = "/usr/share/fonts/truetype/freefont/FreeSansBold.ttf"

# id, English, Hindi, Tamil, facts (en, hi, ta), palette (dark, mid, light)
CARDS = [
    ("lion", "Lion", "शेर", "சிங்கம்",
     "Lions live in groups called prides.",
     "शेर प्राइड नामक समूहों में रहते हैं।",
     "சிங்கங்கள் 'பிரைட்' எனப்படும் குழுக்களாக வாழ்கின்றன.",
     ((120, 62, 10), (222, 140, 24), (255, 214, 120))),
    ("elephant", "Elephant", "हाथी", "யானை",
     "Elephants are the largest land animals on Earth.",
     "हाथी धरती का सबसे बड़ा स्थलीय जानवर है।",
     "யானைகள் பூமியின் மிகப்பெரிய நிலவாழ் விலங்குகள்.",
     ((28, 62, 112), (70, 124, 188), (170, 208, 240))),
    ("dog", "Dog", "कुत्ता", "நாய்",
     "A dog's sense of smell is far stronger than a human's.",
     "कुत्तों की सूंघने की शक्ति इंसानों से कहीं ज़्यादा तेज़ होती है।",
     "நாய்களின் மோப்ப சக்தி மனிதர்களை விட மிகவும் அதிகம்.",
     ((18, 84, 52), (52, 150, 96), (160, 224, 170))),
    ("cat", "Cat", "बिल्ली", "பூனை",
     "Cats sleep for about 12 to 16 hours a day.",
     "बिल्लियाँ दिन में लगभग 12 से 16 घंटे सोती हैं।",
     "பூனைகள் ஒரு நாளைக்கு சுமார் 12 முதல் 16 மணி நேரம் தூங்குகின்றன.",
     ((96, 30, 100), (170, 76, 170), (232, 170, 228))),
    ("horse", "Horse", "घोड़ा", "குதிரை",
     "Horses can sleep both lying down and standing up.",
     "घोड़े लेटकर और खड़े होकर, दोनों तरह से सो सकते हैं।",
     "குதிரைகள் படுத்தும் நின்றும் தூங்க முடியும்.",
     ((110, 24, 36), (190, 62, 66), (244, 164, 150))),
]

S = 2  # supersampling factor for the card art
W = 1024


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def lowpoly_background(size, pal, seed):
    """Delaunay triangulation filled with gradient+noise colours: dense, unique corner features."""
    rnd = random.Random(seed)
    n = 15
    step = size / n
    pts = []
    for iy in range(-1, n + 2):
        for ix in range(-1, n + 2):
            pts.append((ix * step + rnd.uniform(-0.42, 0.42) * step,
                        iy * step + rnd.uniform(-0.42, 0.42) * step))
    pts = np.array(pts)
    tri = Delaunay(pts)
    img = Image.new("RGB", (size, size))
    d = ImageDraw.Draw(img)
    dark, mid, light = pal
    for simplex in tri.simplices:
        p = pts[simplex]
        cx, cy = p.mean(axis=0)
        t = (cx * 0.55 + cy * 0.45) / size
        t = min(max(t + rnd.uniform(-0.28, 0.28), 0), 1)
        col = lerp(dark, mid, t * 2) if t < 0.5 else lerp(mid, light, (t - 0.5) * 2)
        d.polygon([tuple(x) for x in p], fill=col, outline=lerp(col, (255, 255, 255), 0.12))
    return img


def rr(d, box, r, fill=None, outline=None, width=1):
    d.rounded_rectangle(box, r, fill=fill, outline=outline, width=width)


def ellipse(d, cx, cy, rx, ry, fill, outline=None, width=0):
    d.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=fill, outline=outline, width=width)


def draw_icon(d, cid, cx, cy, u):
    """Flat-design animal heads. u = unit (pixels, already supersampled)."""
    ink = (30, 24, 28)
    if cid == "lion":
        for k in range(14):
            a = k / 14 * math.tau
            ellipse(d, cx + math.cos(a) * u * 1.45, cy + math.sin(a) * u * 1.45, u * 0.75, u * 0.75,
                    (150, 78, 20) if k % 2 else (190, 104, 26), ink, int(u * 0.04))
        ellipse(d, cx, cy, u * 1.55, u * 1.55, (170, 90, 22))
        ellipse(d, cx - u * 1.05, cy - u * 1.15, u * 0.38, u * 0.38, (232, 170, 70), ink, int(u * 0.05))
        ellipse(d, cx + u * 1.05, cy - u * 1.15, u * 0.38, u * 0.38, (232, 170, 70), ink, int(u * 0.05))
        ellipse(d, cx, cy, u * 1.1, u * 1.1, (246, 188, 80), ink, int(u * 0.06))
        ellipse(d, cx, cy + u * 0.38, u * 0.55, u * 0.42, (255, 226, 170), ink, int(u * 0.04))
        d.polygon([(cx - u * 0.2, cy + u * 0.12), (cx + u * 0.2, cy + u * 0.12), (cx, cy + u * 0.34)], fill=(50, 26, 20))
        for sx in (-1, 1):
            ellipse(d, cx + sx * u * 0.42, cy - u * 0.2, u * 0.13, u * 0.15, ink)
            ellipse(d, cx + sx * u * 0.38, cy - u * 0.25, u * 0.045, u * 0.045, (255, 255, 255))
            d.line([(cx, cy + u * 0.34), (cx, cy + u * 0.52)], fill=ink, width=int(u * 0.04))
            d.arc([cx - u * (0.3 if sx < 0 else 0), cy + u * 0.4, cx + u * (0 if sx < 0 else 0.3), cy + u * 0.7],
                  0, 180, fill=ink, width=int(u * 0.04))
    elif cid == "elephant":
        for sx in (-1, 1):
            ellipse(d, cx + sx * u * 1.25, cy - u * 0.1, u * 0.95, u * 1.2, (92, 120, 160), ink, int(u * 0.05))
            ellipse(d, cx + sx * u * 1.25, cy - u * 0.05, u * 0.62, u * 0.85, (232, 168, 180))
        ellipse(d, cx, cy - u * 0.1, u * 1.2, u * 1.15, (120, 150, 190), ink, int(u * 0.06))
        # trunk
        pts = [(cx - u * 0.38, cy + u * 0.3), (cx + u * 0.38, cy + u * 0.3), (cx + u * 0.3, cy + u * 1.1),
               (cx + u * 0.65, cy + u * 1.6), (cx + u * 0.3, cy + u * 1.7), (cx - u * 0.1, cy + u * 1.15),
               (cx - u * 0.3, cy + u * 1.0)]
        d.polygon(pts, fill=(120, 150, 190), outline=ink)
        for k in range(4):
            d.line([(cx - u * 0.2 + k * u * 0.0, cy + u * (0.55 + k * 0.2)), (cx + u * 0.3, cy + u * (0.55 + k * 0.2))],
                   fill=(84, 110, 150), width=int(u * 0.035))
        for sx in (-1, 1):
            d.polygon([(cx + sx * u * 0.55, cy + u * 0.55), (cx + sx * u * 0.8, cy + u * 0.6),
                       (cx + sx * u * 0.92, cy + u * 1.35)], fill=(255, 252, 240), outline=ink)
            ellipse(d, cx + sx * u * 0.5, cy - u * 0.3, u * 0.12, u * 0.14, ink)
    elif cid == "dog":
        for sx in (-1, 1):
            ellipse(d, cx + sx * u * 1.12, cy - u * 0.15, u * 0.5, u * 1.0, (102, 62, 36), ink, int(u * 0.05))
        ellipse(d, cx, cy, u * 1.1, u * 1.1, (214, 160, 96), ink, int(u * 0.06))
        ellipse(d, cx, cy - u * 0.35, u * 0.24, u * 0.5, (255, 238, 210))
        ellipse(d, cx, cy + u * 0.4, u * 0.65, u * 0.5, (255, 232, 196), ink, int(u * 0.04))
        ellipse(d, cx, cy + u * 0.18, u * 0.24, u * 0.17, ink)
        ellipse(d, cx, cy + u * 0.95, u * 0.2, u * 0.3, (232, 96, 112), ink, int(u * 0.03))
        d.line([(cx, cy + u * 0.3), (cx, cy + u * 0.7)], fill=ink, width=int(u * 0.04))
        for sx in (-1, 1):
            ellipse(d, cx + sx * u * 0.45, cy - u * 0.2, u * 0.15, u * 0.17, ink)
            ellipse(d, cx + sx * u * 0.41, cy - u * 0.26, u * 0.05, u * 0.05, (255, 255, 255))
    elif cid == "cat":
        for sx in (-1, 1):
            d.polygon([(cx + sx * u * 0.35, cy - u * 0.8), (cx + sx * u * 1.0, cy - u * 1.55),
                       (cx + sx * u * 1.15, cy - u * 0.35)], fill=(236, 150, 72), outline=ink)
            d.polygon([(cx + sx * u * 0.55, cy - u * 0.8), (cx + sx * u * 0.97, cy - u * 1.25),
                       (cx + sx * u * 1.0, cy - u * 0.5)], fill=(248, 178, 190))
        ellipse(d, cx, cy, u * 1.2, u * 1.05, (240, 156, 76), ink, int(u * 0.06))
        for k in (-1, 0, 1):
            d.line([(cx + k * u * 0.3, cy - u * 1.0), (cx + k * u * 0.3, cy - u * 0.55)], fill=(190, 104, 36), width=int(u * 0.08))
        ellipse(d, cx, cy + u * 0.4, u * 0.5, u * 0.38, (255, 236, 210))
        d.polygon([(cx - u * 0.14, cy + u * 0.2), (cx + u * 0.14, cy + u * 0.2), (cx, cy + u * 0.36)], fill=(240, 120, 140), outline=ink)
        for sx in (-1, 1):
            ellipse(d, cx + sx * u * 0.5, cy - u * 0.15, u * 0.2, u * 0.24, (150, 220, 120), ink, int(u * 0.035))
            ellipse(d, cx + sx * u * 0.5, cy - u * 0.15, u * 0.06, u * 0.2, ink)
            for k in (-1, 0, 1):
                d.line([(cx + sx * u * 0.6, cy + u * (0.36 + k * 0.1)), (cx + sx * u * 1.6, cy + u * (0.28 + k * 0.28))],
                       fill=(250, 250, 250), width=int(u * 0.03))
    elif cid == "horse":
        # mane
        d.polygon([(cx - u * 0.9, cy - u * 1.5), (cx - u * 0.3, cy - u * 1.7), (cx + u * 0.5, cy - u * 0.5),
                   (cx - u * 0.5, cy + u * 0.9), (cx - u * 1.4, cy + u * 0.4), (cx - u * 1.25, cy - u * 0.6)],
                  fill=(44, 28, 26), outline=ink)
        # head
        d.polygon([(cx - u * 0.75, cy - u * 1.35), (cx + u * 0.5, cy - u * 1.35), (cx + u * 0.75, cy - u * 0.1),
                   (cx + u * 0.62, cy + u * 1.3), (cx + u * 0.0, cy + u * 1.55), (cx - u * 0.6, cy + u * 1.2),
                   (cx - u * 0.85, cy - u * 0.1)], fill=(150, 90, 54), outline=ink)
        d.polygon([(cx - u * 0.12, cy - u * 1.3), (cx + u * 0.2, cy - u * 1.3), (cx + u * 0.26, cy + u * 1.0),
                   (cx + u * 0.0, cy + u * 1.1), (cx - u * 0.18, cy + u * 0.8)], fill=(255, 246, 236))
        for sx in (-1, 1):
            d.polygon([(cx + sx * u * 0.45, cy - u * 1.3), (cx + sx * u * 0.72, cy - u * 2.0),
                       (cx + sx * u * 0.85, cy - u * 1.2)], fill=(150, 90, 54), outline=ink)
            ellipse(d, cx + sx * u * 0.52, cy - u * 0.45, u * 0.12, u * 0.14, ink)
            ellipse(d, cx + sx * u * 0.3, cy + u * 1.15, u * 0.09, u * 0.14, (70, 36, 30))
        d.polygon([(cx - u * 0.6, cy - u * 1.5), (cx - u * 0.1, cy - u * 1.9), (cx + u * 0.2, cy - u * 1.35)], fill=(44, 28, 26))


TAGLINES = {
    "lion": ("KING OF THE SAVANNA", "Habitat: grasslands"),
    "elephant": ("GENTLE GIANT", "Habitat: forests & plains"),
    "dog": ("MAN'S BEST FRIEND", "Habitat: homes & farms"),
    "cat": ("CURIOUS HUNTER", "Habitat: homes & wild"),
    "horse": ("SWIFT RUNNER", "Habitat: meadows"),
}
# per-card layout so no two cards share identical structure (helps tracking tell them apart)
LAYOUT = {
    "lion":     dict(m=50, r=30, hdr=250, pan=(300, 770), ftr=(812, 960), tilt=0),
    "elephant": dict(m=66, r=52, hdr=272, pan=(318, 800), ftr=(836, 950), tilt=0),
    "dog":      dict(m=58, r=20, hdr=236, pan=(284, 760), ftr=(800, 966), tilt=0),
    "cat":      dict(m=72, r=64, hdr=262, pan=(310, 786), ftr=(826, 946), tilt=0),
    "horse":    dict(m=46, r=40, hdr=282, pan=(330, 806), ftr=(844, 962), tilt=0),
}


def make_card(idx, card):
    cid, en, hi, ta, _, _, _, pal = card
    L = LAYOUT[cid]
    size = W * S
    img = lowpoly_background(size, pal, seed=7919 + idx * 1543)
    d = ImageDraw.Draw(img, "RGBA")
    m = L["m"] * S
    r = L["r"] * S
    px0, px1 = m + 10 * S, size - m - 10 * S
    # central panel (semi transparent so the low-poly texture still shows through)
    py0, py1 = L["pan"]
    rr(d, [px0, py0 * S, px1, py1 * S], r, fill=(14, 18, 34, 168), outline=(255, 255, 255, 235), width=6 * S)
    cy = (py0 + py1) / 2 * S
    draw_icon(d, cid, size // 2, cy, {"lion": 98, "horse": 92}.get(cid, 112) * S)
    # header banner
    hdr = L["hdr"]
    rr(d, [px0, m + 4 * S, px1, hdr * S], r, fill=(12, 16, 32, 232), outline=(255, 255, 255, 235), width=5 * S)
    f1 = ImageFont.truetype(POPPINS_B, 88 * S)
    ymid = (m + 4 * S + hdr * S) / 2
    d.text((size / 2, ymid - 26 * S), en.upper(), font=f1, fill=(255, 255, 255, 255), anchor="mm")
    fh = ImageFont.truetype(FREE_SANS_B, 44 * S)
    ft = ImageFont.truetype(FREE_SANS, 44 * S)          # FreeSans Bold has no Tamil glyphs
    dot = "  \u2022  "
    wh, wd, wt = d.textlength(hi, font=fh), d.textlength(dot, font=ft), d.textlength(ta, font=ft)
    x0 = (size - (wh + wd + wt)) / 2
    y0 = ymid + 52 * S
    d.text((x0, y0), hi, font=fh, fill=pal[2] + (255,), anchor="lm")
    d.text((x0 + wh, y0), dot, font=ft, fill=(255, 255, 255, 255), anchor="lm")
    d.text((x0 + wh + wd, y0), ta, font=ft, fill=pal[2] + (255,), anchor="lm")
    # footer banner with card-specific text
    f0, f1y = L["ftr"]
    rr(d, [px0, f0 * S, px1, f1y * S], r, fill=(12, 16, 32, 232), outline=(255, 255, 255, 235), width=5 * S)
    tag, hab = TAGLINES[cid]
    f3 = ImageFont.truetype(POPPINS_B, 44 * S)
    f4 = ImageFont.truetype(POPPINS_R, 34 * S)
    fm = (f0 + f1y) / 2 * S
    d.text((size / 2, fm - 20 * S), tag, font=f3, fill=(255, 214, 90, 255), anchor="mm")
    d.text((size / 2, fm + 26 * S), f"{hab}  \u2022  card {idx + 1}/{len(CARDS)}", font=f4, fill=(214, 224, 244, 255), anchor="mm")
    # outer frame (different thickness per card)
    t = (14 + idx * 3) * S
    d.rectangle([0, 0, size - 1, size - 1], outline=(255, 255, 255), width=t)
    d.rectangle([t, t, size - t, size - t], outline=(18, 22, 40), width=10 * S)
    img = img.resize((W, W), Image.LANCZOS).convert("RGB")
    return img


def wrap(draw, text, font, max_w):
    words, lines, cur = text.split(" "), [], ""
    for w in words:
        t = (cur + " " + w).strip()
        if draw.textlength(t, font=font) <= max_w or not cur:
            cur = t
        else:
            lines.append(cur)
            cur = w
    if cur:
        lines.append(cur)
    return lines


def text_image(text, font_path, size, max_w, color=(255, 255, 255, 255), line_gap=1.28):
    probe = ImageDraw.Draw(Image.new("RGBA", (10, 10)))
    font = ImageFont.truetype(font_path, size)
    lines = wrap(probe, text, font, max_w)
    lh = int(size * line_gap)
    wpx = int(max(probe.textlength(l, font=font) for l in lines)) + 8
    img = Image.new("RGBA", (wpx, lh * len(lines) + int(size * 0.35)), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    for i, l in enumerate(lines):
        d.text((4, i * lh), l, font=font, fill=color)
    return img


def save_bytes(img, rel):
    """Unity imports *.bytes as TextAsset; the game decodes it with Texture2D.LoadImage (RGBA32, readable)."""
    p = os.path.join(RES, rel + ".bytes")
    img.save(p, "PNG", optimize=True)


def main():
    sheet_items = []
    for i, c in enumerate(CARDS):
        cid = c[0]
        card = make_card(i, c)
        card.save(os.path.join(CARDS_DIR, f"{cid}_card.png"))
        save_bytes(card, f"Cards/{cid}")
        sheet_items.append(card)
        # shaped Indic text
        save_bytes(text_image(c[2], FREE_SANS_B, 92, 700), f"Text/{cid}_hi_name")
        save_bytes(text_image(c[3], FREE_SANS, 92, 700), f"Text/{cid}_ta_name")
        save_bytes(text_image(c[5], FREE_SANS, 46, 930, (235, 240, 250, 255)), f"Text/{cid}_hi_fact")
        save_bytes(text_image(c[6], FREE_SANS, 46, 930, (235, 240, 250, 255)), f"Text/{cid}_ta_fact")
    # A4 print sheet @300 dpi, cards exactly 90 mm
    dpi = 300
    mm = dpi / 25.4
    A4 = (int(210 * mm), int(297 * mm))
    sheet = Image.new("RGB", A4, (255, 255, 255))
    d = ImageDraw.Draw(sheet)
    cw = int(90 * mm)
    gx, gy = int(10 * mm), int(18 * mm)
    ox = (A4[0] - (2 * cw + gx)) // 2
    oy = int(24 * mm)
    hf = ImageFont.truetype(POPPINS_B, int(7 * mm))
    d.text((ox, int(8 * mm)), "AR Flashcards - print at 100% scale (cards are 90 mm)", font=hf, fill=(20, 24, 40))
    for k, im in enumerate(sheet_items):
        r, col = divmod(k, 2)
        x, y = ox + col * (cw + gx), oy + r * (cw + gy)
        sheet.paste(im.resize((cw, cw), Image.LANCZOS), (x, y))
    # slot 6: tips
    x, y = ox + 1 * (cw + gx), oy + 2 * (cw + gy)
    tf = ImageFont.truetype(POPPINS_R, int(3.6 * mm))
    tips = ["Tips for reliable tracking", "- Print on matte paper, no glare", "- Keep the card flat, in good light",
            "- Hold the phone 20-40 cm away", "- One card in view at a time", "- Screen display also works", "  (brightness high)"]
    for n, t in enumerate(tips):
        d.text((x, y + int(n * 6.2 * mm)), t, font=ImageFont.truetype(POPPINS_B if n == 0 else POPPINS_R, int(4 * mm)), fill=(20, 24, 40))
    sheet.save(os.path.join(CARDS_DIR, "PRINT_ME_A4.pdf"), "PDF", resolution=dpi)
    sheet.resize((A4[0] // 3, A4[1] // 3)).save(os.path.join(CARDS_DIR, "PRINT_ME_preview.png"))
    print("assets written")


if __name__ == "__main__":
    main()

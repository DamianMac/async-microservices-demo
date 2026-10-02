#!/usr/bin/env python3
"""Generate images/<sku>.svg for every product in products.json.

Coffees get a bag with a label; equipment gets a simple drawing. Also adds an
"image" field to each product in products.json (safe to re-run).

Usage: python3 generate-images.py
"""
import json, os, re, hashlib
from xml.sax.saxutils import escape

ROOT = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(ROOT, "images")
os.makedirs(OUT, exist_ok=True)

data = json.load(open(os.path.join(ROOT, "products.json")))

W, H = 600, 800

ROAST_BAG = {
    "light": "#e9dcc3",
    "medium": "#c8a27a",
    "medium-dark": "#7b5234",
    "dark": "#2c201a",
}
ROAST_LEVEL = {"light": 1, "medium": 2, "medium-dark": 3, "dark": 4}

ACCENTS = ["#d9534f", "#e8a33d", "#4f9d69", "#3f7cac", "#8e5ea2", "#d46a8c",
           "#2a9d8f", "#c0703a", "#5c6bc0", "#b5892d"]

BLEND_ACCENT = {
    "BL-CUMULUS": "#3f7cac", "BL-CUMULUS-1KG": "#3f7cac", "BL-STRATUS": "#6fa8c9",
    "BL-THUNDERHEAD": "#6b4e9b", "BL-CIRRUS": "#d46a8c", "BL-COLDBREW": "#2a9d8f",
}


def accent_for(p):
    if p["sku"] in BLEND_ACCENT:
        return BLEND_ACCENT[p["sku"]]
    if p["category"] == "decaf":
        return "#4f9d69"
    h = int(hashlib.md5(p["coffee"]["origin"].encode()).hexdigest(), 16)
    return ACCENTS[h % len(ACCENTS)]


def wrap(text, width):
    lines, cur = [], ""
    for word in text.split():
        if cur and len(cur) + 1 + len(word) > width:
            lines.append(cur)
            cur = word
        else:
            cur = f"{cur} {word}".strip()
    if cur:
        lines.append(cur)
    return lines


def cloud(cx, cy, s, fill):
    return (f'<g transform="translate({cx},{cy}) scale({s})" fill="{fill}">'
            '<circle cx="-22" cy="6" r="16"/><circle cx="0" cy="-6" r="22"/>'
            '<circle cx="24" cy="4" r="17"/><rect x="-38" y="6" width="78" height="16" rx="8"/></g>')


def coffee_svg(p):
    c = p["coffee"]
    bag = ROAST_BAG[c["roast"]]
    acc = accent_for(p)
    dark_bag = c["roast"] in ("medium-dark", "dark")
    crimp = "#00000022"

    # bag silhouette with crimped top
    body = (f'<path d="M110 150 L490 150 L520 740 Q520 760 500 760 L100 760 Q80 760 80 740 Z" fill="{bag}"/>'
            f'<rect x="100" y="95" width="400" height="62" rx="6" fill="{bag}"/>'
            + "".join(f'<line x1="{x}" y1="100" x2="{x}" y2="152" stroke="{crimp}" stroke-width="3"/>'
                      for x in range(115, 500, 14))
            + '<path d="M110 150 L90 740" stroke="#00000018" stroke-width="10"/>'
            + '<path d="M490 150 L510 740" stroke="#ffffff22" stroke-width="10"/>')

    # label
    lx, ly, lw, lh = 140, 220, 320, 440
    label = f'<rect x="{lx}" y="{ly}" width="{lw}" height="{lh}" rx="14" fill="#fbf7f0"/>'
    label += f'<rect x="{lx}" y="{ly}" width="{lw}" height="92" rx="14" fill="{acc}"/>'
    label += f'<rect x="{lx}" y="{ly + 70}" width="{lw}" height="22" fill="{acc}"/>'
    label += cloud(300, ly + 36, 0.7, "#fbf7f0")
    label += (f'<text x="300" y="{ly + 84}" text-anchor="middle" font-family="Georgia, serif" '
              f'font-size="15" letter-spacing="4" fill="#fbf7f0">NIMBUS COFFEE CO.</text>')

    name = p["name"].replace(" 1kg", "")
    lines = wrap(name, 15)
    # shrink long lines so they stay inside the 320px label
    size = min(34 if len(lines) <= 2 else 28, int(280 / (0.62 * max(len(l) for l in lines))))
    y = ly + 145
    for line in lines:
        label += (f'<text x="300" y="{y}" text-anchor="middle" font-family="Georgia, serif" '
                  f'font-size="{size}" font-weight="bold" fill="#2c201a">{escape(line)}</text>')
        y += size + 6

    y += 6
    label += f'<line x1="190" y1="{y}" x2="410" y2="{y}" stroke="{acc}" stroke-width="2"/>'
    y += 30
    for line in wrap(" · ".join(c["tastingNotes"]), 34):
        label += (f'<text x="300" y="{y}" text-anchor="middle" font-family="Helvetica, Arial, sans-serif" '
                  f'font-size="16" font-style="italic" fill="#5a4a40">{escape(line)}</text>')
        y += 22

    # roast meter
    my = ly + lh - 80
    label += (f'<text x="300" y="{my}" text-anchor="middle" font-family="Helvetica, Arial, sans-serif" '
              f'font-size="12" letter-spacing="3" fill="#8a7a70">ROAST</text>')
    lvl = ROAST_LEVEL[c["roast"]]
    for i in range(4):
        fill = "#5a3a24" if i < lvl else "#e4dacb"
        label += f'<circle cx="{246 + i * 36}" cy="{my + 22}" r="10" fill="{fill}"/>'

    weight = f'{c["weightGrams"] // 1000}kg' if c["weightGrams"] >= 1000 else f'{c["weightGrams"]}g'
    label += (f'<text x="{lx + 20}" y="{ly + lh - 18}" font-family="Helvetica, Arial, sans-serif" '
              f'font-size="14" fill="#8a7a70">{escape(c["process"].upper())}</text>')
    label += (f'<text x="{lx + lw - 20}" y="{ly + lh - 18}" text-anchor="end" font-family="Helvetica, Arial, sans-serif" '
              f'font-size="14" font-weight="bold" fill="#5a4a40">{weight}</text>')

    # valve
    valve_fill = "#00000033" if not dark_bag else "#ffffff22"
    extras = f'<circle cx="300" cy="190" r="10" fill="{valve_fill}"/><circle cx="300" cy="190" r="4" fill="{valve_fill}"/>'

    return body + extras + label


INK = "#3a2e28"
STEEL = "#b8bec4"
GLASS = "#cfe6ee"
WOOD = "#a0703f"
CERAMIC = "#f4efe8"


def equipment_art(sku):
    s = f'stroke="{INK}" stroke-width="6" stroke-linejoin="round" stroke-linecap="round"'
    if sku == "EQ-GRINDER-HAND":
        return (f'<rect x="240" y="260" width="120" height="300" rx="16" fill="{STEEL}" {s}/>'
                f'<rect x="240" y="420" width="120" height="40" fill="{WOOD}" {s}/>'
                f'<rect x="280" y="225" width="40" height="40" rx="6" fill="{STEEL}" {s}/>'
                f'<path d="M300 225 L300 205 L430 170" fill="none" {s}/>'
                f'<circle cx="440" cy="166" r="22" fill="{WOOD}" {s}/>')
    if sku == "EQ-DRIPPER":
        return (f'<path d="M190 270 L410 270 L350 430 L250 430 Z" fill="{CERAMIC}" {s}/>'
                f'<rect x="220" y="430" width="160" height="20" rx="6" fill="{CERAMIC}" {s}/>'
                f'<path d="M410 300 Q460 310 440 360 L390 380" fill="none" {s}/>'
                f'<path d="M225 470 L375 470 L360 590 Q358 600 348 600 L252 600 Q242 600 240 590 Z" fill="{GLASS}" {s}/>'
                f'<rect x="248" y="540" width="104" height="50" fill="#6b4226"/>')
    if sku == "EQ-FILTERS":
        out = ""
        for i in range(5):
            y = 280 + i * 22
            out += f'<path d="M180 {y} L420 {y} L360 {y + 200} L240 {y + 200} Z" fill="#e6d3b3" {s}/>'
        return out
    if sku == "EQ-KETTLE":
        return (f'<path d="M190 560 L200 340 Q205 300 250 300 L350 300 Q395 300 400 340 L410 560 Z" fill="{STEEL}" {s}/>'
                f'<path d="M200 520 Q130 500 140 420 Q145 340 100 280 L85 262" fill="none" stroke="{INK}" stroke-width="18" stroke-linecap="round"/>'
                f'<path d="M200 520 Q130 500 140 420 Q145 340 100 280 L85 262" fill="none" stroke="{STEEL}" stroke-width="8" stroke-linecap="round"/>'
                f'<path d="M400 340 Q480 350 470 450 Q465 520 405 530" fill="none" stroke="{INK}" stroke-width="22" stroke-linecap="round"/>'
                f'<rect x="265" y="275" width="70" height="28" rx="10" fill="{STEEL}" {s}/>'
                f'<circle cx="300" cy="265" r="12" fill="{INK}"/>')
    if sku == "EQ-SCALES":
        return (f'<rect x="130" y="420" width="340" height="110" rx="20" fill="#2f2f33" {s}/>'
                f'<rect x="150" y="400" width="300" height="30" rx="10" fill="#45454b" {s}/>'
                f'<rect x="180" y="455" width="140" height="48" rx="6" fill="#9fe0b0"/>'
                f'<text x="250" y="490" text-anchor="middle" font-family="Courier New, monospace" font-size="30" font-weight="bold" fill="#1d3b27">18.0g</text>'
                f'<circle cx="370" cy="479" r="14" fill="#6d6d75"/><circle cx="420" cy="479" r="14" fill="#6d6d75"/>')
    if sku == "EQ-PLUNGER":
        return (f'<rect x="220" y="260" width="160" height="320" rx="10" fill="{GLASS}" {s}/>'
                f'<rect x="224" y="400" width="152" height="176" fill="#6b4226"/>'
                f'<path d="M220 280 L380 280 L380 560 L220 560" fill="none" stroke="{STEEL}" stroke-width="10"/>'
                f'<path d="M380 320 Q450 330 445 420 Q440 500 380 510" fill="none" stroke="{INK}" stroke-width="20" stroke-linecap="round"/>'
                f'<rect x="210" y="240" width="180" height="26" rx="8" fill="{STEEL}" {s}/>'
                f'<line x1="300" y1="240" x2="300" y2="180" {s}/>'
                f'<circle cx="300" cy="172" r="16" fill="{INK}"/>'
                f'<rect x="228" y="330" width="144" height="10" fill="{STEEL}" {s}/>')
    if sku == "EQ-MOKA":
        return (f'<path d="M230 580 L210 450 L390 450 L370 580 Z" fill="{STEEL}" {s}/>'
                f'<rect x="250" y="425" width="100" height="30" fill="#8d949a" {s}/>'
                f'<path d="M250 425 L225 300 L375 300 L350 425 Z" fill="{STEEL}" {s}/>'
                f'<path d="M225 300 L190 290 L205 280" fill="none" {s}/>'
                f'<path d="M225 285 L300 250 L375 285 Z" fill="{STEEL}" {s}/>'
                f'<circle cx="300" cy="248" r="10" fill="{INK}"/>'
                f'<path d="M375 310 L440 320 L430 400 L360 405" fill="none" stroke="{INK}" stroke-width="20" stroke-linecap="round" stroke-linejoin="round"/>')
    if sku == "EQ-KEEPCUP":
        return (f'<path d="M225 300 L375 300 L355 580 Q353 590 343 590 L257 590 Q247 590 245 580 Z" fill="{GLASS}" {s}/>'
                f'<rect x="232" y="420" width="136" height="70" fill="#c49060" {s}/>'
                f'<rect x="215" y="270" width="170" height="36" rx="10" fill="#e07a5f" {s}/>'
                f'<rect x="240" y="250" width="120" height="26" rx="8" fill="#e07a5f" {s}/>'
                f'<rect x="250" y="500" width="100" height="75" fill="#6b4226" opacity="0.8"/>')
    if sku == "EQ-CANISTER":
        return (f'<rect x="200" y="300" width="200" height="280" rx="14" fill="#5f7d6e" {s}/>'
                f'<rect x="190" y="270" width="220" height="40" rx="10" fill="{STEEL}" {s}/>'
                f'<rect x="270" y="245" width="60" height="30" rx="6" fill="{INK}"/>'
                f'<rect x="230" y="380" width="140" height="90" rx="8" fill="#fbf7f0"/>'
                + cloud(300, 420, 0.8, "#5f7d6e"))
    raise ValueError(sku)


def equipment_svg(p):
    return (f'<rect x="60" y="60" width="480" height="680" rx="40" fill="#efe6d8"/>'
            + equipment_art(p["sku"])
            + f'<text x="300" y="680" text-anchor="middle" font-family="Georgia, serif" font-size="30" font-weight="bold" fill="#2c201a">{escape(p["name"])}</text>'
            + cloud(300, 120, 0.6, "#c8b9a3"))


for p in data["products"]:
    inner = coffee_svg(p) if "coffee" in p else equipment_svg(p)
    svg = (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}" width="{W}" height="{H}">'
           f'<rect width="{W}" height="{H}" fill="#f6f1ea"/>{inner}</svg>\n')
    with open(os.path.join(OUT, p["sku"].lower() + ".svg"), "w") as f:
        f.write(svg)

# add an image field to products.json, keeping its formatting
path = os.path.join(ROOT, "products.json")
out, sku = [], None
for line in open(path):
    m = re.match(r'\s*"sku": "([^"]+)"', line)
    if m:
        sku = m.group(1)
    if line.strip().startswith('"image"'):
        continue
    if line.strip().startswith('"price"'):
        out.append(f'      "image": "images/{sku.lower()}.svg",\n')
    out.append(line)
open(path, "w").write("".join(out))
print(len(data["products"]), "images")

"""Generates architecture-slide.svg, a simpler 16:9 version of the diagram for slides.
Run: python3 docs/generate-architecture-slide.py"""
import os
from html import escape

W, H = 1920, 1080
HTTP = "#2563eb"
BUS = "#d97706"
INK = "#1f2937"
MUTED = "#6b7280"
LINE = "#d1d5db"
BG = "#ffffff"
CARD = "#ffffff"
FONT = "Inter, -apple-system, 'Segoe UI', Helvetica, Arial, sans-serif"

out = []
def add(s): out.append(s)

def text(x, y, s, size=18, weight=400, fill=INK, anchor="start", extra=""):
    add(f'<text x="{x}" y="{y}" font-size="{size}" font-weight="{weight}" fill="{fill}" text-anchor="{anchor}" {extra}>{escape(s)}</text>')

def rect(x, y, w, h, fill=CARD, stroke=LINE, rx=14, sw=2, extra=""):
    add(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" {extra}/>')

def tw(s, size, bold=False):
    return len(s) * size * (0.6 if bold else 0.56)

def arrow(x1, y1, x2, y2, color, both=False, sw=3):
    ms = f' marker-start="url(#s-{color[1:]})"' if both else ""
    add(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{sw}"{ms} marker-end="url(#e-{color[1:]})"/>')

add(f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}" font-family="{FONT}">')
add("<defs>")
for c in (HTTP, BUS, MUTED):
    k = c[1:]
    add(f'<marker id="e-{k}" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="6" markerHeight="6" orient="auto"><path d="M0,0 L10,5 L0,10 z" fill="{c}"/></marker>')
    add(f'<marker id="s-{k}" viewBox="0 0 10 10" refX="1" refY="5" markerWidth="6" markerHeight="6" orient="auto"><path d="M10,0 L0,5 L10,10 z" fill="{c}"/></marker>')
add("</defs>")
rect(0, 0, W, H, fill=BG, stroke="none", rx=0)

BX, BW = 60, 240          # browser
CX, CW = 390, 270         # Caddy
SX, SW = 760, 440         # services
NX, NW = 1300, 90         # NATS
EX, EW = 1490, 370        # back office
TOP, RH, GAP = 250, 150, 30
HEAD = 90  # Caddy and NATS start this far above the first service, leaving room for Caddy's name

def band(x1, x2, label, color):
    y = 70
    add(f'<path d="M{x1},{y + 14} V{y} H{x2} V{y + 14}" fill="none" stroke="{color}" stroke-width="3"/>')
    text((x1 + x2) / 2, y - 18, label, 22, 700, color, "middle", 'letter-spacing="3"')

band(BX, CX + CW, "BUSINESS IN THE FRONT · HTTP", HTTP)
band(SX, SX + SW, "SERVICES", INK)
band(NX, EX + EW, "PARTY IN THE BACK · MESSAGING", BUS)

services = [
    ("Catalog", 2, "/catalog"),
    ("Orders", 2, "/orders"),
    ("Search", 1, "/search"),
    ("Profile", 1, "/profile"),
]
BOTTOM = TOP + len(services) * (RH + GAP) - GAP

# Caddy
rect(CX, TOP - HEAD, CW, BOTTOM - TOP + HEAD, fill="#eff6ff", stroke=HTTP, sw=3, rx=18)
text(CX + CW / 2, TOP - HEAD + 42, "Caddy", 32, 700, HTTP, "middle")
text(CX + CW / 2, TOP - HEAD + 70, "gateway + load balancer", 17, 600, INK, "middle")

# NATS bar
rect(NX, TOP - HEAD, NW, BOTTOM - TOP + HEAD, fill="#fef3c7", stroke=BUS, sw=3, rx=18)
mid = (TOP - HEAD + BOTTOM) / 2
text(NX + NW / 2 + 10, mid, "NATS  ·  Nimbus bus", 30, 700, BUS, "middle",
     f'transform="rotate(-90 {NX + NW / 2} {mid})"')

for i, (name, n, route) in enumerate(services):
    y = TOP + i * (RH + GAP)
    cy = y + RH / 2
    if n > 1:
        rect(SX + 10, y + 10, SW, RH)
    rect(SX, y, SW, RH)
    text(SX + 32, cy + 11, name, 30, 700)
    if n > 1:
        bx = SX + 44 + tw(name, 30, True)
        rect(bx, cy - 17, 48, 34, fill=HTTP, stroke="none", rx=17)
        text(bx + 24, cy + 7, f"×{n}", 19, 700, "#fff", "middle")
    rx_, ry_ = CX + 20, cy - 22
    rect(rx_, ry_, CW - 40, 44, fill=CARD, stroke="#bfdbfe", rx=10, sw=1.5)
    text(rx_ + 16, ry_ + 29, route + "/*", 19, 700, HTTP)
    arrow(CX + CW, ry_ + 22, SX - 3, cy, HTTP)
    arrow(SX + SW + (10 if n > 1 else 0) + 3, cy, NX - 3, cy, BUS, both=True)

# Browser
by = mid - 110
rect(BX, by, BW, 190, stroke=HTTP, sw=3, rx=18)
add(f'<path d="M{BX},{by + 36} V{by + 18} a18,18 0 0 1 18,-18 H{BX + BW - 18} a18,18 0 0 1 18,18 V{by + 36} z" fill="{HTTP}"/>')
for k in range(3):
    add(f'<circle cx="{BX + 22 + k * 17}" cy="{by + 18}" r="5" fill="#fff" opacity="0.85"/>')
text(BX + BW / 2, by + 100, "Storefront", 30, 700, INK, "middle")
text(BX + BW / 2, by + 134, "Nuxt SPA", 19, 500, MUTED, "middle")
arrow(BX + BW, by + 105, CX - 3, by + 105, HTTP)
text(BX + BW / 2, by + 240, "One API origin", 20, 700, HTTP, "middle")

# Back office: the MVC apps, plus the services with no Caddy route, one per service row
ent = [("CatalogManager", "back office · MVC"), ("Warehouse", "back office · MVC"),
       ("PaymentGateway", "bus only · no route"), ("Email", "bus only · no route")]
for i, (name, sub) in enumerate(ent):
    y = TOP + i * (RH + GAP)
    rect(EX, y, EW, RH, stroke=MUTED)
    text(EX + EW / 2, y + RH / 2 - 4, name, 30, 700, INK, "middle")
    text(EX + EW / 2, y + RH / 2 + 30, sub, 19, 500, MUTED, "middle")
    arrow(NX + NW + 3, y + RH / 2, EX - 3, y + RH / 2, BUS, both=True)

add("</svg>")
open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "architecture-slide.svg"), "w").write("\n".join(out))

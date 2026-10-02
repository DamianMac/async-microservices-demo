"""Generates saga-slide.svg, the order saga as swimlanes, sized to sit under a slide title (2:1).
Run: python3 docs/generate-saga-slide.py"""
import os
from html import escape

W, H = 1920, 960
HTTP = "#2563eb"
BUS = "#d97706"
INK = "#1f2937"
MUTED = "#6b7280"
LINE = "#d1d5db"
BG = "#ffffff"
CARD = "#ffffff"
FONT = "Inter, -apple-system, 'Segoe UI', Helvetica, Arial, sans-serif"
MONO = "'IBM Plex Mono', SFMono-Regular, Menlo, monospace"

out = []
def add(s): out.append(s)

def text(x, y, s, size=18, weight=400, fill=INK, anchor="start", extra=""):
    add(f'<text x="{x}" y="{y}" font-size="{size}" font-weight="{weight}" fill="{fill}" text-anchor="{anchor}" {extra}>{escape(s)}</text>')

def rect(x, y, w, h, fill=CARD, stroke=LINE, rx=14, sw=2, extra=""):
    add(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" {extra}/>')

add(f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}" font-family="{FONT}">')
add("<defs>")
for c in (HTTP, BUS, INK):
    add(f'<marker id="e-{c[1:]}" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="6" markerHeight="6" orient="auto"><path d="M0,0 L10,5 L0,10 z" fill="{c}"/></marker>')
add("</defs>")
rect(0, 0, W, H, fill=BG, stroke="none", rx=0)

# Lanes: (name, note, top, bottom). Orders owns the state; the others handle one step each.
LANES = [("Orders", "owns the state", 20, 300), ("PaymentGateway", "", 300, 520),
         ("Warehouse", "", 520, 740), ("Email", "", 740, 950)]
LABEL_X = 40
for i, (name, note, top, bottom) in enumerate(LANES):
    if i:
        add(f'<line x1="{LABEL_X}" y1="{top}" x2="{W - 40}" y2="{top}" stroke="{LINE}" stroke-width="2"/>')
    cy = (top + bottom) / 2
    text(LABEL_X, cy + (0 if note else 9), name, 26, 700)
    if note:
        text(LABEL_X, cy + 30, note, 19, 500, MUTED)

def lane_cy(i):
    return (LANES[i][2] + LANES[i][3]) / 2

# Nodes, left to right in time. Columns are x centres.
COLS = [420, 672, 924, 1176, 1428, 1680]
PILL_W, PILL_H = 180, 64
BOX_W, BOX_H = 290, 104

nodes = [  # (kind, lane, col, title, subtitle, subtitle colour)
    ("state", 0, 0, "Pending", "", None),
    ("box", 1, 1, "TakePayment", "fails once · Nimbus retries", BUS),
    ("state", 0, 2, "Paid", "", None),
    ("box", 2, 3, "Pick ticket", "a person completes it", MUTED),
    ("state", 0, 4, "Completed", "", None),
    ("box", 3, 5, "Dispatch email", "to the customer", MUTED),
]
MESSAGES = ["OrderPlaced", "PaymentSuccessful", "OrderConfirmedEvent", "OrderProcessed", "OrderCompleted"]

def geom(node):
    kind, lane, col = node[:3]
    w, h = (PILL_W, PILL_H) if kind == "state" else (BOX_W, BOX_H)
    return COLS[col], lane_cy(lane), w / 2, h / 2

def edge_point(cx, cy, hw, hh, dx, dy, pad=6):
    """Where a line from the node centre in direction (dx, dy) leaves the node's box."""
    t = min(hw / abs(dx) if dx else 1e9, hh / abs(dy) if dy else 1e9)
    length = (dx * dx + dy * dy) ** 0.5
    t += pad / length
    return cx + dx * t, cy + dy * t

# State transitions along the Orders lane
states = [geom(n) for n in nodes if n[0] == "state"]
for (x1, y1, hw1, _), (x2, y2, hw2, _) in zip(states, states[1:]):
    add(f'<line x1="{x1 + hw1 + 8}" y1="{y1}" x2="{x2 - hw2 - 10}" y2="{y2}" stroke="{INK}" stroke-width="2.5" '
        f'stroke-dasharray="8 7" marker-end="url(#e-{INK[1:]})"/>')

# Messages: straight lines between consecutive nodes, labelled at the midpoint
labels = []
for a, b, msg in zip(nodes, nodes[1:], MESSAGES):
    ax, ay, ahw, ahh = geom(a)
    bx, by, bhw, bhh = geom(b)
    dx, dy = bx - ax, by - ay
    x1, y1 = edge_point(ax, ay, ahw, ahh, dx, dy)
    x2, y2 = edge_point(bx, by, bhw, bhh, -dx, -dy, pad=4)
    add(f'<line x1="{x1:.1f}" y1="{y1:.1f}" x2="{x2:.1f}" y2="{y2:.1f}" stroke="{BUS}" stroke-width="3" marker-end="url(#e-{BUS[1:]})"/>')
    labels.append(((x1 + x2) / 2, (y1 + y2) / 2, msg))

# The HTTP request that starts it all
px, py, _, phh = geom(nodes[0])
text(px, py - phh - 48, "POST /orders", 20, 600, HTTP, "middle", f'font-family="{MONO}"')
add(f'<line x1="{px}" y1="{py - phh - 38}" x2="{px}" y2="{py - phh - 6}" stroke="{HTTP}" stroke-width="3" marker-end="url(#e-{HTTP[1:]})"/>')

# Nodes on top of the lines
for node in nodes:
    kind, lane, col, title, sub, sub_color = node
    cx, cy, hw, hh = geom(node)
    if kind == "state":
        rect(cx - hw, cy - hh, 2 * hw, 2 * hh, fill=INK, stroke="none", rx=hh)
        text(cx, cy + 9, title, 26, 700, "#fff", "middle")
    else:
        rect(cx - hw, cy - hh, 2 * hw, 2 * hh, stroke=MUTED)
        text(cx, cy - 4, title, 26, 700, INK, "middle")
        text(cx, cy + 28, sub, 18, 600 if sub_color == BUS else 500, sub_color, "middle")

# Retry loop on the payment step
bx, by, bhw, _ = geom(nodes[1])
lx = bx - bhw
add(f'<path d="M{lx - 4},{by - 22} C{lx - 70},{by - 60} {lx - 70},{by + 60} {lx - 8},{by + 20}" fill="none" '
    f'stroke="{BUS}" stroke-width="3" marker-end="url(#e-{BUS[1:]})"/>')

# Message labels last, so they sit over the lines
for x, y, msg in labels:
    w = len(msg) * 20 * 0.6 + 36
    rect(x - w / 2, y - 21, w, 42, fill="#fef3c7", stroke=BUS, rx=21, sw=2)
    text(x, y + 7, msg, 20, 600, "#b45309", "middle", f'font-family="{MONO}"')

add("</svg>")
open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "saga-slide.svg"), "w").write("\n".join(out))

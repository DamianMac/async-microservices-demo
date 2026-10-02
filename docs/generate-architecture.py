"""Generates architecture.svg. Run: python3 docs/generate-architecture.py"""
import os
from html import escape

W = 1900
HTTP = "#2563eb"
BUS = "#d97706"
INK = "#1f2937"
MUTED = "#6b7280"
LINE = "#d1d5db"
BG = "#faf7f2"
CARD = "#ffffff"
FONT = "Inter, -apple-system, 'Segoe UI', Helvetica, Arial, sans-serif"

out = []
def add(s): out.append(s)

def text(x, y, s, size=13, weight=400, fill=INK, anchor="start", extra=""):
    add(f'<text x="{x}" y="{y}" font-size="{size}" font-weight="{weight}" fill="{fill}" text-anchor="{anchor}" {extra}>{escape(s)}</text>')

def rect(x, y, w, h, fill=CARD, stroke=LINE, rx=10, sw=1.5, extra=""):
    add(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" {extra}/>')

def tw(s, size, bold=False):
    return len(s) * size * (0.6 if bold else 0.56)

def arrow(x1, y1, x2, y2, color, both=False, dash=False, sw=2):
    d = ' stroke-dasharray="6 5"' if dash else ""
    ms = f' marker-start="url(#s-{color[1:]})"' if both else ""
    add(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{sw}"{d}{ms} marker-end="url(#e-{color[1:]})"/>')

add(None)  # the <svg> tag, filled in once the height is known
add("<defs>")
for c in (HTTP, BUS, MUTED):
    k = c[1:]
    add(f'<marker id="e-{k}" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M0,0 L10,5 L0,10 z" fill="{c}"/></marker>')
    add(f'<marker id="s-{k}" viewBox="0 0 10 10" refX="1" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M10,0 L0,5 L10,10 z" fill="{c}"/></marker>')
add("</defs>")
add(None)  # background, likewise

# Title
text(40, 56, "Nimbus Coffee Co. — Mullet Microservices", 30, 700)
text(40, 86, "Business in the front: HTTP through one load-balanced gateway. Party in the back: services talk over the bus.", 15, 400, MUTED)

# Legend
lx, ly = W - 440, 40
arrow(lx, ly, lx + 44, ly, HTTP); text(lx + 54, ly + 4, "HTTP request", 12, 500, MUTED)
arrow(lx, ly + 22, lx + 44, ly + 22, BUS, both=True); text(lx + 54, ly + 26, "Nimbus messages over NATS", 12, 500, MUTED)
arrow(lx, ly + 44, lx + 44, ly + 44, MUTED, dash=True); text(lx + 54, ly + 48, "Fetch after notification (HTTP)", 12, 500, MUTED)

# Band headers
def band(x1, x2, label, color):
    y = 134
    add(f'<path d="M{x1},{y+10} V{y} H{x2} V{y+10}" fill="none" stroke="{color}" stroke-width="2"/>')
    text((x1 + x2) / 2, y - 10, label, 13, 700, color, "middle", 'letter-spacing="2"')

band(40, 510, "BUSINESS IN THE FRONT · HTTP", HTTP)
band(590, 1110, "SERVICES · ASP.NET CORE WEB APIS", INK)
band(1170, W - 40, "PARTY IN THE BACK · ASYNC MESSAGING", BUS)

# Services
services = [
    dict(name="Catalog", n=2, route="/catalog/*", ports=":5263 · :5264", sub="Postgres · Marten",
         note="Serves products from an in-memory cache",
         pub=["CatalogUpdated"], handles=["NewCatalogAvailable", "LowStockWarning · OutOfStock",
                                         "CatalogUpdated · stock alerts (multicast)"]),
    dict(name="Orders", n=2, route="/orders/*", ports=":5053 · :5054", sub="SQL Server · EF Core",
         note="Order saga: Pending → Paid → Completed",
         pub=["OrderPlaced · OrderConfirmedEvent", "OrderCompleted"],
         handles=["NewCatalogAvailable · PaymentSuccessful", "OrderProcessed"]),
    dict(name="Search", n=1, route="/search/*", ports=":5069", sub="Postgres · EF Core",
         note="Full-text search (tsvector)", pub=[], handles=["NewCatalogAvailable"]),
    dict(name="Profile", n=1, route="/profile/*", ports=":5311", sub="SQL Server · EF Core",
         note="Accounts, JWTs, addresses, fake cards", pub=["CustomerRegistered"], handles=[]),
]

SX, SW, RH, GAP, TOP = 590, 520, 120, 16, 250
CX, CW = 290, 220
NX, NW = 1170, 70
EX, EW = 1300, W - 40 - 1300
BOTTOM = TOP + len(services) * (RH + GAP) - GAP

# Caddy
rect(CX, 160, CW, BOTTOM - 160, fill="#eff6ff", stroke=HTTP, sw=2, rx=14)
text(CX + 18, 190, "Caddy", 20, 700, HTTP)
text(CX + 18, 210, "API gateway + load balancer", 12, 600, INK)
text(CX + 18, 228, "api.localtest.me", 12, 400, MUTED)

def io_block(x, y, label, items, color):
    text(x, y, label, 10, 700, color, extra='letter-spacing="1.5"')
    for i, it in enumerate(items):
        text(x, y + 16 + i * 15, it, 12, 500, INK)

for i, s in enumerate(services):
    y = TOP + i * (RH + GAP)
    cy = y + RH / 2
    if s["n"] > 1:
        rect(SX + 8, y + 8, SW, RH)
    rect(SX, y, SW, RH)
    text(SX + 18, y + 30, s["name"], 18, 700)
    if s["n"] > 1:
        bx = SX + 26 + tw(s["name"], 18, True)
        rect(bx, y + 15, 30, 20, fill=HTTP, stroke="none", rx=10)
        text(bx + 15, y + 29, f"×{s['n']}", 12, 700, "#fff", "middle")
    text(SX + 18, y + 52, s["ports"], 12, 500, MUTED)
    text(SX + 18, y + 74, s["sub"], 12, 400, INK)
    text(SX + 18, y + 94, s["note"], 12, 400, MUTED)
    add(f'<line x1="{SX + 262}" y1="{y + 16}" x2="{SX + 262}" y2="{y + RH - 16}" stroke="{LINE}"/>')
    ix, iy = SX + 278, y + 26
    if s["pub"]:
        io_block(ix, iy, "PUBLISHES / SENDS", s["pub"], BUS)
        iy += 22 + 15 * len(s["pub"])
    if s["handles"]:
        io_block(ix, iy, "HANDLES", s["handles"], BUS)

    # Caddy route and HTTP arrow
    rect(CX + 14, cy - 26, CW - 28, 52, fill=CARD, stroke="#bfdbfe", rx=8, sw=1)
    text(CX + 26, cy - 5, s["route"], 13, 700, HTTP)
    text(CX + 26, cy + 14, s["ports"] + ("  round robin" if s["n"] > 1 else ""), 11, 500, MUTED)
    arrow(CX + CW, cy, SX - 2, cy, HTTP)
    # Bus link
    arrow(SX + SW + (8 if s["n"] > 1 else 0) + 2, cy, NX - 2, cy, BUS, both=True)

text(CX + 18, 250, "Strips the prefix; health-checks", 11, 400, MUTED)
text(CX + 18, 265, "and retries on another instance", 11, 400, MUTED)

# Browser
by = 520
rect(40, by, 180, 170, stroke=HTTP, sw=2, rx=14)
add(f'<rect x="40" y="{by}" width="180" height="26" rx="14" fill="{HTTP}"/>')
add(f'<rect x="40" y="{by + 13}" width="180" height="13" fill="{HTTP}"/>')
for k in range(3):
    add(f'<circle cx="{56 + k * 13}" cy="{by + 13}" r="4" fill="#fff" opacity="0.8"/>')
text(58, by + 58, "Storefront", 18, 700)
text(58, by + 80, "Nuxt 4 SPA", 12, 500, MUTED)
text(58, by + 100, "shop.localtest.me", 12, 400, MUTED)
text(58, by + 128, "Calls the APIs straight", 12, 400, INK)
text(58, by + 145, "from the browser", 12, 400, INK)
arrow(220, by + 85, CX - 2, by + 85, HTTP)
text(40, by + 214, "One API origin.", 13, 600, HTTP)
text(40, by + 234, "The shop never knows how", 12, 400, MUTED)
text(40, by + 250, "many instances there are,", 12, 400, MUTED)
text(40, by + 266, "or that anything is async.", 12, 400, MUTED)

# Back office: the MVC apps, plus the services nothing calls over HTTP (no Caddy route)
ent = [
    dict(name="CatalogManager", port="MVC · :5003", host="catalog-manager.localtest.me", sub="SQL Server · EF Core",
         lines=["Edit prices, publish the catalog", "Serves GET /api/products"],
         pub=["NewCatalogAvailable"], handles=[]),
    dict(name="Warehouse", port="MVC · :5223", host="warehouse.localtest.me", sub="Postgres · Marten",
         lines=["Stock, receiving, dispatch"],
         pub=["OrderProcessed", "LowStockWarning · OutOfStock"], handles=["OrderConfirmedEvent"]),
    dict(name="PaymentGateway", port="Web API · :5195", host="Bus only, no Caddy route", sub="In memory",
         lines=["Mock processor, fails first attempt"], pub=["PaymentSuccessful"], handles=["OrderPlaced"]),
    dict(name="Email", port="Web API · :5299", host="Bus only, no Caddy route", sub="MailKit → SMTP (Mailpit)",
         lines=["Templated emails"], pub=["SendEmailCommand (to itself)"],
         handles=["CustomerRegistered · OrderCompleted", "SendEmailCommand"]),
]
text(EX, TOP - 14, "BACK OFFICE", 11, 700, MUTED, extra='letter-spacing="2"')
y = TOP
for e in ent:
    # Baseline of the last line in each column
    left = 112 + (len(e["lines"]) - 1) * 16
    right = 26 + 1 + 15 * (len(e["pub"]) + len(e["handles"])) + (22 if e["handles"] else 0)
    h = max(left, right) + 20
    e["y"] = y
    rect(EX, y, EW, h, stroke=MUTED)
    text(EX + 18, y + 30, e["name"], 18, 700)
    text(EX + 18, y + 52, e["port"], 12, 500, MUTED)
    text(EX + 18, y + 70, e["host"], 12, 400, MUTED)
    text(EX + 18, y + 92, e["sub"], 12, 400, INK)
    for k, l in enumerate(e["lines"]):
        text(EX + 18, y + 112 + k * 16, l, 12, 400, MUTED)
    add(f'<line x1="{EX + 262}" y1="{y + 16}" x2="{EX + 262}" y2="{y + h - 16}" stroke="{LINE}"/>')
    ix, iy = EX + 278, y + 26
    io_block(ix, iy, "PUBLISHES / SENDS", e["pub"], BUS)
    iy += 22 + 15 * len(e["pub"])
    if e["handles"]:
        io_block(ix, iy, "HANDLES", e["handles"], BUS)
    arrow(NX + NW + 2, y + h / 2, EX - 2, y + h / 2, BUS, both=True)
    y += h + GAP
BUS_BOTTOM = max(BOTTOM, y - GAP)

# NATS bar
rect(NX, 236, NW, BUS_BOTTOM - 236, fill="#fef3c7", stroke=BUS, sw=2, rx=14)
mid = (236 + BUS_BOTTOM) / 2
text(NX + NW / 2 + 7, mid, "NATS JetStream  ·  Nimbus bus", 20, 700, BUS, "middle",
     f'transform="rotate(-90 {NX + NW / 2} {mid})"')

# Fetch after notification: Catalog/Search/Orders -> CatalogManager
fy = 200
fx = SX + 400
add(f'<path d="M{fx},{TOP - 2} V{fy} H{EX + 200} V{ent[0]["y"] - 4}" fill="none" stroke="{MUTED}" stroke-width="2" stroke-dasharray="6 5" marker-end="url(#e-{MUTED[1:]})"/>')
text(fx + 14, fy - 10, "GET /api/products — Catalog, Search and Orders fetch the catalog after NewCatalogAvailable", 12, 500, MUTED)

# Infra strip
iy = BUS_BOTTOM + 40
text(40, iy, "LOCAL INFRASTRUCTURE (DOCKER)", 11, 700, MUTED, extra='letter-spacing="2"')
chips = [
    ("Postgres :5432", "Catalog, Search, Warehouse"),
    ("SQL Server :1433", "Profile, Orders, CatalogManager"),
    ("NATS :4222", "JetStream transport for Nimbus"),
    ("Seq", "Logs; filter by CorrelationId"),
    ("Mailpit", "SMTP :1025, mail.localtest.me"),
]
cw = (W - 80 - 4 * 16) / 5
for k, (a, b) in enumerate(chips):
    x = 40 + k * (cw + 16)
    rect(x, iy + 14, cw, 56, rx=8)
    text(x + 16, iy + 38, a, 14, 700, BUS if a.startswith("NATS") else INK)
    text(x + 16, iy + 57, b, 12, 400, MUTED)

# Message chains
my = iy + 120
text(40, my, "MESSAGE CHAINS", 11, 700, MUTED, extra='letter-spacing="2"')
text(40, my + 20, "Each hop is a Nimbus message; the correlation id follows the whole chain across services.", 12, 400, MUTED)
chains = [
    ("Publishing the catalog", ["CatalogManager", "NewCatalogAvailable", "Catalog · Search · Orders", "CatalogUpdated", "every Catalog instance"]),
    ("Signing up", ["Profile", "CustomerRegistered", "Email", "SendEmailCommand", "Email → SMTP"]),
    ("Placing an order", ["Orders", "OrderPlaced", "PaymentGateway", "PaymentSuccessful", "Orders", "OrderConfirmedEvent",
                          "Warehouse", "OrderProcessed", "Orders", "OrderCompleted", "Email"]),
    ("Running low", ["Warehouse pick or import", "LowStockWarning · OutOfStock", "Catalog + every Catalog instance"]),
]
for r, (title, items) in enumerate(chains):
    y = my + 48 + r * 52
    text(40, y + 21, title, 13, 700)
    x = 215
    for k, it in enumerate(items):
        is_msg = k % 2 == 1
        size = 12
        w = tw(it, size, True) + 24
        if is_msg:
            rect(x, y, w, 32, fill="#fef3c7", stroke=BUS, rx=16, sw=1.2)
            text(x + w / 2, y + 21, it, size, 700, "#92400e", "middle")
        else:
            rect(x, y, w, 32, fill=CARD, stroke=INK, rx=6, sw=1.2)
            text(x + w / 2, y + 21, it, size, 700, INK, "middle")
        x += w
        if k < len(items) - 1:
            arrow(x + 3, y + 16, x + 19, y + 16, BUS, sw=1.6)
            x += 22

H = y + 68
out[0] = f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}" font-family="{FONT}">'
out[out.index(None)] = f'<rect x="0" y="0" width="{W}" height="{H}" rx="0" fill="{BG}" stroke="none" stroke-width="1.5" />'
add("</svg>")
open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "architecture.svg"), "w").write("\n".join(out))


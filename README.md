# Async Microservices

A demo showing how a microservice architecture can work using async messaging instead of HTTP calls between services. Built with .NET and Nimbus.

It's made up of:

- **API services** (`src/Services`): Catalog, Orders, Search, PaymentGateway, Email and Profile
- **Back-office apps** (`src/Enterprise`): Warehouse and CatalogManager (ASP.NET Core MVC)
- **Storefront** (`src/client`): a Nuxt SPA at http://shop.localtest.me
- **Infrastructure** (`infra`): Caddy, NATS, Seq, Mailpit, SQL Server and Postgres, run with Docker Compose

The APIs are served through Caddy at `http://api.localtest.me/{service}`.

## Getting started

With Docker Desktop running:

```sh
./run.sh up       # start infra, all services and the storefront
./run.sh status   # see what's running
./run.sh down     # stop everything
```

To manage the infra and the apps separately, for example to restart the apps after a code change without restarting Docker:

```sh
./run.sh infra-up     # start the Docker infra only
./run.sh infra-down   # stop the Docker infra only
./run.sh start        # build and start the services and storefront
./run.sh stop         # stop the services and storefront
./run.sh restart      # stop then start the services and storefront
```

`up` is `infra-up` then `start`, and `down` is `stop` then `infra-down`. Catalog and Orders run two instances each, which Caddy load balances between.

Logs for each app are in `.run/logs/`.

## Demo data

The shop is **Nimbus Coffee Co.**, a coffee roaster selling beans and brewing gear. Its catalog lives in `data/`:

- `products.json`: 36 products (single origins, blends, decaf and equipment), priced in AUD. Each has a SKU, price, stock level and image path, and coffees also have origin, roast and tasting notes. A few are out of stock or nearly out, to make ordering scenarios easy to demo.
- `images/`: one SVG per product, named after its SKU in lowercase.
- `images/rendered/`: PNG versions of the SVGs.

To change the look of the images, edit `data/generate-images.py`, then regenerate the SVGs and PNGs:

```sh
python3 data/generate-images.py            # writes images/*.svg and adds "image" to products.json
python3 data/render-images.py              # renders PNGs at 600×800
python3 data/render-images.py --scale 2    # or at 1200×1600
```

Both scripts use only the Python standard library. Rendering uses headless Chrome, found in its usual location or set with the `CHROME` environment variable.

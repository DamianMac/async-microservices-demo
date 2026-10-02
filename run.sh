#!/usr/bin/env bash
# Starts and stops the whole demo: infra (Docker), the .NET apps and the Nuxt client.
#
#   ./run.sh up          start infra, then build and start the apps
#   ./run.sh down        stop the apps, then the infra
#   ./run.sh infra-up    start the infra only
#   ./run.sh infra-down  stop the infra only
#   ./run.sh start       build and start the apps only (infra should be up)
#   ./run.sh stop        stop the apps only, leaving the infra running
#   ./run.sh restart     stop, then start the apps
#   ./run.sh status      show what's running
#   ./run.sh reset       stop the apps, delete all their data, then start them again
#
# Logs go to .run/logs/<name>.log

set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
RUN_DIR="$ROOT/.run"
PID_DIR="$RUN_DIR/pids"
LOG_DIR="$RUN_DIR/logs"

# name:project dir:port:instance - one line per instance.
# The instance name is passed to the app as the "Instance" environment variable.
# Catalog and Orders run multiple instances; Caddy round robins across them,
# so keep these ports in sync with infra/Caddyfile.
APPS="
Catalog:src/Services/Catalog:5263:Instance1
Catalog:src/Services/Catalog:5264:Instance2
Orders:src/Services/Orders:5053:Instance1
Orders:src/Services/Orders:5054:Instance2
Search:src/Services/Search:5069:Instance1
PaymentGateway:src/Services/PaymentGateway:5195:Instance1
Email:src/Services/Email:5299:Instance1
Profile:src/Services/Profile:5311:Instance1
Warehouse:src/Enterprise/Warehouse:5223:Instance1
CatalogManager:src/Enterprise/CatalogManager:5003:Instance1
"
CLIENT_PORT=3000

# Give each background job its own process group so it can be stopped
# along with any children, without touching anything else.
set -m

start_process() { # id, working dir, command...
  local id="$1" dir="$2"; shift 2
  local pidfile="$PID_DIR/$id.pid"
  if [[ -f "$pidfile" ]] && kill -0 "$(cat "$pidfile")" 2>/dev/null; then
    echo "  $id already running"
    return
  fi
  (cd "$dir" && exec "$@") > "$LOG_DIR/$id.log" 2>&1 &
  echo $! > "$pidfile"
  echo "  started $id"
}

wait_for() { # id, url
  local id="$1" url="$2"
  for _ in $(seq 1 60); do
    if curl -sf -o /dev/null "$url"; then
      echo "  ✔ $id  $url"
      return
    fi
    sleep 0.5
  done
  echo "  ✘ $id did not respond at $url (see .run/logs/$id.log)"
}

COMPOSE=(docker compose -f "$ROOT/infra/docker-compose.yml")

infra_up() {
  if ! docker info >/dev/null 2>&1; then
    echo "Docker isn't running. Start Docker Desktop first." >&2
    exit 1
  fi

  echo "Starting infra..."
  "${COMPOSE[@]}" up -d
  # Pick up any Caddyfile changes
  "${COMPOSE[@]}" restart caddy >/dev/null
}

infra_down() {
  echo "Stopping infra..."
  "${COMPOSE[@]}" down
}

start_apps() {
  if [[ -z "$("${COMPOSE[@]}" ps -q 2>/dev/null)" ]]; then
    echo "Warning: infra isn't running, start it with ./run.sh infra-up" >&2
  fi

  mkdir -p "$PID_DIR" "$LOG_DIR"

  echo "Building .NET solution..."
  dotnet build "$ROOT/async-microservices.sln" --nologo -v quiet

  echo "Starting .NET apps..."
  for app in $APPS; do
    IFS=: read -r name dir port instance <<< "$app"
    ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS="http://localhost:$port" \
    Instance="$instance" \
      start_process "$name-$port" "$ROOT/$dir" "./bin/Debug/net9.0/$name"
  done

  echo "Starting client..."
  if [[ ! -d "$ROOT/src/client/node_modules" ]]; then
    (cd "$ROOT/src/client" && bash -c 'source ~/.nvm/nvm.sh && nvm use >/dev/null && npm install')
  fi
  start_process client "$ROOT/src/client" bash -c 'source ~/.nvm/nvm.sh && nvm use >/dev/null && exec npm run dev'

  echo "Waiting for everything to respond..."
  for app in $APPS; do
    IFS=: read -r name dir port _ <<< "$app"
    if [[ "$dir" == src/Services/* ]]; then
      wait_for "$name-$port" "http://localhost:$port/health"
    else
      wait_for "$name-$port" "http://localhost:$port/"
    fi
  done
  wait_for client "http://localhost:$CLIENT_PORT/"

  cat <<EOF

Up and running:
  Shop       http://shop.localtest.me
  Warehouse  http://warehouse.localtest.me
  Catalog    http://catalog-manager.localtest.me
  API    http://api.localtest.me/{catalog,orders,search,profile}
  Seq    http://seq.localtest.me
  Mail   http://mail.localtest.me
  NATS   http://nats.localtest.me

Stop with: ./run.sh down (or ./run.sh stop to leave the infra running)
EOF
}

stop_apps() {
  echo "Stopping apps..."
  if [[ -d "$PID_DIR" ]]; then
    for pidfile in "$PID_DIR"/*.pid; do
      [[ -e "$pidfile" ]] || continue
      local id pid
      id="$(basename "$pidfile" .pid)"
      pid="$(cat "$pidfile")"
      # Negative PID = the whole process group started for this app
      if kill -TERM -- "-$pid" 2>/dev/null; then
        echo "  stopped $id"
      fi
      rm -f "$pidfile"
    done
  fi
}

# Empties every database (keeping the schemas and DbUp journals) and deletes
# the product images the apps have copied in. The apps should be stopped,
# since Catalog caches the products in memory.
clear_data() {
  if [[ -z "$("${COMPOSE[@]}" ps -q postgres 2>/dev/null)" || -z "$("${COMPOSE[@]}" ps -q sqlserver 2>/dev/null)" ]]; then
    echo "The infra isn't running, start it with ./run.sh infra-up" >&2
    exit 1
  fi

  echo "Clearing Postgres data..."
  "${COMPOSE[@]}" exec -T postgres psql -U postgres -q -v ON_ERROR_STOP=1 \
    < "$ROOT/scripts/clear-postgres-data.sql"

  echo "Clearing SQL Server data..."
  "${COMPOSE[@]}" exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P 'asyncdemo1!' -C -b \
    < "$ROOT/scripts/clear-sqlserver-data.sql"

  echo "Deleting product images..."
  rm -rf "$ROOT/src/Enterprise/CatalogManager/wwwroot/images/products" \
         "$ROOT/src/Services/Catalog/wwwroot/images/products"
}

status() {
  for app in $APPS; do
    IFS=: read -r name dir port _ <<< "$app"
    show_status "$name-$port"
  done
  show_status client
  echo
  "${COMPOSE[@]}" ps --format 'table {{.Service}}\t{{.Status}}'
}

show_status() {
  local pidfile="$PID_DIR/$1.pid"
  if [[ -f "$pidfile" ]] && kill -0 "$(cat "$pidfile")" 2>/dev/null; then
    echo "  ✔ $1 (pid $(cat "$pidfile"))"
  else
    echo "  ✘ $1"
  fi
}

case "${1:-}" in
  up) infra_up; start_apps ;;
  down) stop_apps; infra_down ;;
  infra-up) infra_up ;;
  infra-down) infra_down ;;
  start) start_apps ;;
  stop) stop_apps ;;
  restart) stop_apps; start_apps ;;
  status) status ;;
  reset) stop_apps; clear_data; start_apps ;;
  *) echo "Usage: $0 {up|down|infra-up|infra-down|start|stop|restart|status|reset}" >&2; exit 1 ;;
esac

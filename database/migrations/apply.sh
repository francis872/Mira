#!/bin/sh
# Aplica en orden las migraciones pendientes. Seguro de repetir: cada archivo se registra
# en schema_migrations y se ejecuta (con su registro) en una sola transacción.
# Uso en una base existente:  docker compose exec mira-postgres sh /migrations/apply.sh
set -eu

DIR="$(cd "$(dirname "$0")" && pwd)"
PSQL="psql -v ON_ERROR_STOP=1 -q -U ${POSTGRES_USER} -d ${POSTGRES_DB}"

$PSQL -c "CREATE TABLE IF NOT EXISTS schema_migrations (nombre TEXT PRIMARY KEY, aplicada_en TIMESTAMPTZ NOT NULL DEFAULT now())"

for file in "$DIR"/[0-9]*.sql; do
  name="$(basename "$file")"
  if [ "$($PSQL -tA -c "SELECT 1 FROM schema_migrations WHERE nombre = '$name'")" = "1" ]; then
    echo "migración omitida (ya aplicada): $name"
    continue
  fi
  echo "aplicando migración: $name"
  $PSQL --single-transaction -f "$file" -c "INSERT INTO schema_migrations (nombre) VALUES ('$name')"
done
echo "migraciones al día"

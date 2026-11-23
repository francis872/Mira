"""Migra únicamente un PostgreSQL 16 vacío y efímero, nunca destinos reales."""
import os
import argparse
import json
from datetime import datetime, timedelta, timezone
from pathlib import Path
import subprocess


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--report', help='Metadatos saneados, sin datos de usuarios')
    options = parser.parse_args()
    if (os.getenv('MIRA_CI') != '1' or os.getenv('PGHOST') not in ('127.0.0.1', 'localhost')
            or not os.getenv('PGDATABASE', '').startswith('mira_ci_') or os.getenv('PGUSER') != 'mira_ci'):
        raise SystemExit('Se requiere MIRA_CI=1, loopback, PGUSER=mira_ci y base mira_ci_* exclusiva.')
    root = Path(__file__).resolve().parents[2]
    args = ['psql', '-X', '-w', '-v', 'ON_ERROR_STOP=1']

    def query(sql):
        return subprocess.check_output(args + ['-tA', '-c', sql], text=True).strip()

    if query("SELECT current_setting('server_version_num')::int / 10000") != '16':
        raise SystemExit('Se requiere PostgreSQL 16.')
    if query("SELECT count(*) FROM information_schema.tables WHERE table_schema='public'") != '0':
        raise SystemExit('La base no está vacía; no se modificó. Cree otra instancia efímera.')
    subprocess.run(args + ['-c', 'CREATE TABLE schema_migrations (nombre TEXT PRIMARY KEY, aplicada_en TIMESTAMPTZ NOT NULL DEFAULT now())'], check=True)
    migrations = sorted((root / 'database/migrations').glob('[0-9]*.sql'))
    for migration in migrations:
        subprocess.run(args + ['--single-transaction', '-f', str(migration), '-c',
            f"INSERT INTO schema_migrations(nombre) VALUES ('{migration.name}')"], check=True)
    tests = sorted((root / 'database/tests').glob('*.sql'))
    for test in tests:
        subprocess.run(args + ['-f', str(test)], check=True)
    if options.report:
        columns = json.loads(query("""SELECT COALESCE(json_agg(json_build_object(
            'table',c.conrelid::regclass::text,'column',a.attname,'nullable',NOT a.attnotnull)
            ORDER BY c.conrelid::regclass::text,a.attname),'[]'::json)::text
            FROM pg_constraint c JOIN pg_namespace n ON n.oid=c.connamespace
            CROSS JOIN LATERAL unnest(c.conkey) key(attnum)
            JOIN pg_attribute a ON a.attrelid=c.conrelid AND a.attnum=key.attnum
            WHERE n.nspname='public' AND c.contype='f'"""))
        report = {'date': datetime.now(timezone(timedelta(hours=-5))).date().isoformat(),
            'scope': 'exclusive empty CI database on loopback; schema metadata only',
            'serverVersion': query('SHOW server_version'), 'migrations': len(migrations),
            'sqlSuites': len(tests), 'foreignKeyColumns': columns,
            'optionalForeignKeyColumns': sum(row['nullable'] for row in columns)}
        Path(options.report).write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
    print(f'Migraciones: {len(migrations)}; suites SQL: {len(tests)}. Solo base efímera.')


if __name__ == '__main__':
    main()

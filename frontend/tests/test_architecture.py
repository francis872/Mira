"""Regresión arquitectónica: el CRUD C# solo invoca rutinas PostgreSQL."""
from pathlib import Path
import re


def test_catalog_repositories_only_call_stored_routines():
    root = Path(__file__).resolve().parents[2]
    sources = list((root / 'MIRA.Api/Repositorios').glob('*Repository.cs'))
    calls = []
    for source in sources:
        sql_literals = re.findall(r'const string sql = "([^"]+)";', source.read_text(encoding='utf-8'))
        if source.name.startswith('I'):
            continue
        assert len(sql_literals) == 5, source.name
        for sql in sql_literals:
            assert re.fullmatch(r'SELECT (?:\* FROM )?public\.sp_(listar|consultar|crear|actualizar|eliminar)_[a-z_]+\([^;]*\);', sql), (source.name, sql)
        calls.extend(sql_literals)
    assert len(calls) == 30
    for layer in ('Peticiones', 'Servicios'):
        for source in (root / 'MIRA.Api' / layer).glob('*.cs'):
            assert not re.search(r'using (?:Dapper|Npgsql)|\b(?:SELECT|INSERT|UPDATE|DELETE)\b', source.read_text(encoding='utf-8')), source.name

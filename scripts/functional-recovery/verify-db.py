from pathlib import Path
import sys,json,psycopg,os
from psycopg import sql
s=json.loads(Path(os.environ['SAPIENS_RECOVERY_RUNTIME']).read_text(encoding='utf-8'))
from psycopg.conninfo import conninfo_to_dict
connection=conninfo_to_dict(s['dsn'])
assert connection.get('host')=='127.0.0.1' and connection.get('dbname','').startswith('mira_audit_functional_')
body=json.loads(sys.argv[1]);table=body['table'];where=body['where'];expected=body.get('expected',{});absent=body.get('absent',False)
allowed={'usuario','area_conocimiento','objetivo_desarrollo_sostenible','area_aplicacion','termino_clave','universidad','linea_investigacion','sedes','docentes','grupos_investigacion','semilleros','lineas_investigacion','grupo_docentes','semillero_tutores','areas_conocimiento','ods','areas_aplicacion','palabras_clave','grupo_lineas','semillero_lineas','linea_disciplinas','linea_ods','linea_aplicacion'}
assert table in allowed and where
with psycopg.connect(s['dsn'],row_factory=psycopg.rows.dict_row) as db:
    query=sql.SQL('SELECT * FROM public.{} WHERE {}').format(sql.Identifier(table),sql.SQL(' AND ').join(sql.SQL('{}=%s').format(sql.Identifier(k)) for k in where))
    row=db.execute(query,list(where.values())).fetchone()
    if absent:assert row is None or row.get('activo') is False or row.get('activa') is False
    else:
        assert row is not None
        for key,value in expected.items():assert str(row[key])==str(value),(table,key)
print(json.dumps({'passed':True,'table':table,'keys':where,'assertedFields':list(expected),'retirement':absent}))

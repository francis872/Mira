from pathlib import Path
import os,json,subprocess,socket,time,requests,argparse,sys
SCRIPTS=Path(__file__).resolve().parent
parser=argparse.ArgumentParser(description='Recorridos reales, solo sobre servicios y PostgreSQL locales restaurados. No despliega.')
parser.add_argument('--runtime',type=Path,required=True,help='JSON privado del entorno local aislado')
parser.add_argument('--output',type=Path,required=True,help='Directorio privado de evidencias, recomendado .local/')
args=parser.parse_args();R=args.output.resolve();R.mkdir(parents=True,exist_ok=True);ROOT=SCRIPTS.parents[1]
config_file=args.runtime.resolve()
import psycopg,secrets
state=json.loads(config_file.read_text(encoding='utf-8'))
from psycopg.conninfo import conninfo_to_dict
connection=conninfo_to_dict(state['dsn'])
assert connection.get('host')=='127.0.0.1' and connection.get('dbname','').startswith('mira_audit_functional_')
assert state['env']['API_URL']==f"http://127.0.0.1:{state['apiport']}/api"
state['personaName']='RECUPERACION Persona '+secrets.token_hex(6)
with psycopg.connect(state['dsn']) as db:state['persona']=db.execute("INSERT INTO usuarios(nombre,correo,password_hash) VALUES (%s,%s,'NO_ES_CREDENCIAL') RETURNING id",(state['personaName'],'recovery-'+secrets.token_hex(8)+'@invalid.test')).fetchone()[0]
config_file.write_text(json.dumps(state))
with socket.socket() as s:s.bind(('127.0.0.1',0));port=s.getsockname()[1]
env=os.environ.copy();env.update(SAPIENS_EDGE_PORT=str(port),SAPIENS_RECOVERY_RUNTIME=str(config_file),SAPIENS_RECOVERY_OUTPUT=str(R),SAPIENS_TEST_PYTHON=sys.executable,SAPIENS_RECOVERY_DATABASE=connection['dbname'])
with (R/'edge-private.log').open('wb') as log:
    p=subprocess.Popen([r'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe','--headless=new','--enable-automation','--window-size=1440,1000','--disable-gpu','--no-first-run','--disable-sync','--disable-background-networking','--remote-debugging-address=127.0.0.1','--remote-debugging-port='+str(port),'--user-data-dir='+str(R/'edge-functional'),'about:blank'],stdout=log,stderr=subprocess.STDOUT,creationflags=subprocess.CREATE_NO_WINDOW)
    try:
        for _ in range(100):
            try:
                if requests.get(f'http://127.0.0.1:{port}/json/version',timeout=1).status_code==200:break
            except requests.RequestException:pass
            time.sleep(.2)
        else:raise RuntimeError('Browser unavailable')
        with (R/'browser-functional-private.log').open('wb') as log:result=subprocess.run(['node',str(SCRIPTS/'verify-browser.mjs')],env=env,stdout=log,stderr=subprocess.STDOUT,timeout=900,cwd=ROOT)
        report=json.loads((R/'browser-functional.json').read_text(encoding='utf-8'));print(json.dumps({'status':report['status'],'stage':report.get('stage'),'completedFlows':len(report['flows']),'error':report.get('error')}))
    finally:
        subprocess.run(['node',str(SCRIPTS/'close-browser.mjs')],env=env,capture_output=True,timeout=10)
        if p.poll() is None:p.wait(timeout=10)
raise SystemExit(result.returncode)

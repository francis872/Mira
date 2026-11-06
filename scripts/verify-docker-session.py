"""Flask real → API real → PostgreSQL; reinicio de Gunicorn y revocación."""
import argparse
import json
import os
import re
import subprocess
import time
from pathlib import Path
import requests


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--project',required=True)
    parser.add_argument('--env-file',required=True)
    parser.add_argument('--compose-file',required=True)
    parser.add_argument('--output',required=True)
    args=parser.parse_args()
    if not re.fullmatch(r'mira-v3-check-\d{14}',args.project):
        raise ValueError('Solo se permite reiniciar el proyecto de pruebas creado por verify-docker.ps1')
    web=os.environ['MIRA_E2E_URL'];api=os.environ['MIRA_E2E_API_URL']
    credentials={'correo':os.environ['MIRA_E2E_EMAIL'],'password':os.environ['MIRA_E2E_PASSWORD']}
    browser=requests.Session()
    page=browser.get(web+'/login',timeout=20)
    token=re.search(r'name="csrf_token" value="([^"]+)"',page.text).group(1)
    login=browser.post(web+'/login',data={**credentials,'csrf_token':token},allow_redirects=False,timeout=20)
    assert login.status_code==302
    sid=browser.cookies.get('session')
    assert sid and browser.get(web+'/',timeout=20).status_code==200
    subprocess.run(['docker','compose','--project-name',args.project,'--env-file',args.env_file,'-f','docker-compose.yml','-f',args.compose_file,'restart','mira-frontend'],check=True,capture_output=True)
    for _ in range(40):
        try:
            if requests.get(web+'/health',timeout=2).status_code==200:break
        except requests.RequestException:pass
        time.sleep(.5)
    result=browser.get(web+'/',allow_redirects=False,timeout=20)
    assert result.status_code==200 and browser.cookies.get('session')==sid
    csrf=re.search(r'name="csrf_token" value="([^"]+)"',result.text).group(1)
    assert browser.post(web+'/logout',data={'csrf_token':csrf},allow_redirects=False,timeout=20).status_code==302
    assert browser.get(web+'/',allow_redirects=False,timeout=20).status_code==302
    auth=requests.post(api+'/api/auth/login',json=credentials,timeout=20)
    assert auth.status_code==200
    headers={'Authorization':'Bearer '+auth.json()['token']}
    assert requests.get(api+'/api/auth/me',headers=headers,timeout=20).status_code==200
    assert requests.post(api+'/api/auth/logout',headers=headers,json={},timeout=20).status_code==200
    assert requests.get(api+'/api/auth/me',headers=headers,timeout=20).status_code==401
    report={'date':'2026-11-05','runtime':'Linux / Gunicorn / ASP.NET Core / PostgreSQL 16',
        'profile':'isolated HTTP development transport; production TLS ingress not claimed',
        'frontendRestartPreservesSession':True,'frontendLogoutClearsSession':True,'apiLogoutRevokesSameJwt':True,
        'secretsOrTokensPublished':False,'status':'passed'}
    Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(report))


if __name__=='__main__':main()

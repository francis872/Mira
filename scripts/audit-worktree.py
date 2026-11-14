"""Revisa candidatos Git y emite solo nombres de archivos, nunca secretos."""
from datetime import datetime, timedelta, timezone
from pathlib import Path
import subprocess,json,re,hashlib
root=Path.cwd()
Path('.local').mkdir(exist_ok=True)
raw=subprocess.check_output(['git','status','--porcelain=v1','-z','--untracked-files=all']).decode().split('\0')
pending=[{'status':entry[:2],'path':entry[3:]} for entry in raw if entry]
known=[]
for filename in ('.env','.env.local','.local/pg16-validation/config.env'):
    path=Path(filename)
    if not path.exists():continue
    for line in path.read_text(encoding='utf-8-sig').splitlines():
        if '=' not in line or line.lstrip().startswith('#'):continue
        key,value=line.split('=',1)
        if re.search(r'PASSWORD|SECRET|JWT.*KEY',key,re.I) and len(value.strip())>=12 and not value.strip().startswith('REPLACE'):
            known.append(value.strip())
patterns={
'private_key':r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',
'jwt_literal':r'eyJ[A-Za-z0-9_-]{15,}\.[A-Za-z0-9_-]{15,}\.[A-Za-z0-9_-]{15,}',
'github_token':r'gh[pousr]_[A-Za-z0-9]{30,}',
'aws_key':r'AKIA[A-Z0-9]{16}',
'credential_uri':r'postgres(?:ql)?://[^\s:/]+:[^\s@]+@',
}
findings=[];texts=0;binaries=[];manifest={}
for row in pending:
    path=Path(row['path'])
    if not path.is_file():continue
    data=path.read_bytes();manifest[row['path']]=hashlib.sha256(data).hexdigest()
    try:text=data.decode('utf-8-sig')
    except UnicodeError:binaries.append(row['path']);continue
    texts+=1
    for name,pattern in patterns.items():
        if re.search(pattern,text):findings.append({'file':row['path'],'kind':name})
    if any(value in text for value in known):findings.append({'file':row['path'],'kind':'matches_local_secret'})
    if re.search(r'(?i)(?:password|secret_key|jwt__key)\s*[=:]\s*[\"\x27][^\"\x27]{8,}[\"\x27]',text):
        findings.append({'file':row['path'],'kind':'credential_like_literal_requires_review'})
ignored={}
for name in ['.env','.env.local','.local/v3-final-review.log','.local/pg16-validation/config.env','tests/MIRA.Api.Tests/TestResults/v3-backend.trx']:
    ignored[name]=subprocess.run(['git','check-ignore','-q','--',name],capture_output=True).returncode==0
summary={'date':datetime.now(timezone(timedelta(hours=-5))).date().isoformat(),'branch':subprocess.check_output(['git','branch','--show-current']).decode().strip(),
    'pendingPaths':len(pending),'textFilesScanned':texts,'binaryFiles':binaries,'findings':findings,'ignored':ignored,
    'scope':'pending tracked/untracked non-ignored files; known local secret comparison without displaying values; current worktree, not full history'}
Path('.local/v3-final-source-snapshot.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
Path('.local/v3-final-secret-audit.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print(json.dumps(summary,ensure_ascii=False))

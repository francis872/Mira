"""Escaneo heurístico del árbol versionado; emite rutas, nunca valores secretos."""
from pathlib import Path
import re
import subprocess
import zipfile


def main():
    names = subprocess.check_output(['git', 'ls-files', '-z']).decode().strip('\0').split('\0')
    patterns = [r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',
        r'eyJ[A-Za-z0-9_-]{15,}\.[A-Za-z0-9_-]{15,}\.[A-Za-z0-9_-]{15,}',
        r'gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{30,}', r'AKIA[A-Z0-9]{16}']
    findings = []
    for name in names:
        path = Path(name)
        if ((path.name.startswith('.env') and name != '.env.example') or '.local' in path.parts
                or path.suffix.lower() in ('.pfx', '.pem', '.key', '.dump', '.db')):
            findings.append(name)
            continue
        if not path.is_file():
            continue
        if path.suffix.lower() in ('.docx', '.xlsx'):
            with zipfile.ZipFile(path) as archive:
                text = '\n'.join(archive.read(n).decode('utf-8') for n in archive.namelist() if n.endswith('.xml'))
        else:
            try:
                text = path.read_text(encoding='utf-8-sig')
            except UnicodeError:
                continue
        if any(re.search(pattern, text) for pattern in patterns):
            findings.append(name)
    if findings:
        raise SystemExit('Posibles secretos/rutas privadas; revisar sin publicar valores: ' + ', '.join(sorted(set(findings))))
    print(f'Escaneo heurístico: {len(names)} rutas versionadas, sin hallazgos de alta confianza. No cubre historial ni servicios remotos.')


if __name__ == '__main__':
    main()

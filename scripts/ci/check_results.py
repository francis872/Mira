"""Impide aprobar integraciones omitidas, reportes vacíos o avisos NuGet."""
import argparse
import json
from pathlib import Path
import xml.etree.ElementTree as ET


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--trx')
    parser.add_argument('--junit')
    parser.add_argument('--nuget')
    args = parser.parse_args()
    if not any((args.trx, args.junit, args.nuget)):
        parser.error('Indique al menos un reporte.')
    result = {}
    if args.trx:
        counters = next(x for x in ET.parse(args.trx).iter() if x.tag.endswith('Counters')).attrib
        if int(counters['total']) <= 0 or int(counters['total']) != int(counters['passed']):
            raise SystemExit('TRX vacío, fallido u omitido: no se aprueba la integración.')
        result['dotnetPassed'] = int(counters['passed'])
    if args.junit:
        suites = list(ET.parse(args.junit).getroot().iter('testsuite'))
        total = sum(int(x.get('tests', '0')) for x in suites)
        if total <= 0 or any(int(x.get(k, '0')) for x in suites for k in ('failures', 'errors', 'skipped')):
            raise SystemExit('JUnit vacío, fallido u omitido.')
        result['flaskPassed'] = total
    if args.nuget:
        raw = Path(args.nuget).read_bytes()
        # Bash redirige UTF-8; Windows PowerShell 5 puede producir UTF-16 con BOM.
        report = json.loads(raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig'))
        if not report.get('projects'):
            raise SystemExit('Reporte NuGet sin proyectos: no se aprueba.')

        def vulnerable(value):
            if isinstance(value, dict):
                return bool(value.get('vulnerabilities')) or any(vulnerable(x) for x in value.values())
            if isinstance(value, list):
                return any(vulnerable(x) for x in value)
            return False

        if vulnerable(report):
            raise SystemExit('NuGet reporta vulnerabilidades. Revise el JSON saneado.')
        result['nugetProjectsWithoutReportedAdvisories'] = len(report['projects'])
    print(json.dumps(result))


if __name__ == '__main__':
    main()

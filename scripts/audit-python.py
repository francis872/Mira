"""Audita distribuciones instaladas en OSV sin cargar la aplicación ni sus secretos."""
from datetime import datetime, timedelta, timezone
import argparse
import importlib.metadata
import json
import platform
import sys
import time
from pathlib import Path
from urllib.request import Request, urlopen


def main():
    parser = argparse.ArgumentParser()
    source = parser.add_mutually_exclusive_group()
    source.add_argument("--package-dir", help="Site-packages aislado, sin incluir Python global")
    source.add_argument("--packages-json", help="Lista name/version exportada de una imagen")
    parser.add_argument("--output", required=True)
    args = parser.parse_args()
    if args.packages_json:
        packages = json.loads(Path(args.packages_json).read_text(encoding="utf-8-sig"))
    else:
        distributions = importlib.metadata.distributions(path=[args.package_dir]) if args.package_dir else importlib.metadata.distributions()
        packages = [{"name": d.metadata["Name"], "version": d.version} for d in distributions]
    unique = {(p["name"].lower().replace("_", "-"), p["version"]) for p in packages}
    packages = [{"name": name, "version": version} for name, version in sorted(unique)]
    if not packages:
        parser.error("No se encontraron paquetes; no se puede emitir un resultado aprobado vacío.")
    payload = {"queries": [{"package": {"name": p["name"], "ecosystem": "PyPI"}, "version": p["version"]} for p in packages]}
    report = {"date": datetime.now(timezone(timedelta(hours=-5))).date().isoformat(), "source": "https://api.osv.dev/v1/querybatch", "python": platform.python_version(),
              "platform": platform.system(), "packages": packages, "status": "blocked"}
    response = None
    for attempt in range(3):
        try:
            request = Request(report["source"], data=json.dumps(payload).encode(), headers={"Content-Type": "application/json"})
            with urlopen(request, timeout=20) as stream:
                response = json.load(stream)
            if len(response["results"]) != len(packages):
                raise ValueError("Respuesta incompleta de OSV")
            break
        except (OSError, ValueError, KeyError) as error:
            report["errorType"] = type(error).__name__
            response = None
            if attempt < 2:
                time.sleep(1)
    if response is None:
        report["reason"] = "No fue posible consultar OSV. No se certifica ausencia de vulnerabilidades."
    else:
        report.pop("errorType", None)
        for package, item in zip(packages, response["results"]):
            package["advisories"] = [v["id"] for v in item.get("vulns", [])]
        report["status"] = "vulnerable" if any(p["advisories"] for p in packages) else "passed"
    Path(args.output).write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False))
    return {"passed": 0, "vulnerable": 1, "blocked": 2}[report["status"]]


if __name__ == "__main__":
    sys.exit(main())

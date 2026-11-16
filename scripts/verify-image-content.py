"""Comprueba filesystem exportado y usuario; no inspecciona ni muestra secretos del host."""
from datetime import datetime, timedelta, timezone
import argparse
import json
import re
import subprocess
import tarfile
import tempfile
from pathlib import Path


def run(*args):
    return subprocess.check_output(["docker", *args], text=True).strip()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    parser.add_argument("images", nargs="+")
    args = parser.parse_args()
    reports = []
    for image in args.images:
        config = json.loads(run("image", "inspect", image))[0]
        container = run("create", image)
        try:
            with tempfile.TemporaryDirectory(prefix="mira-image-audit-") as temp:
                assert Path(temp).resolve().parent == Path(tempfile.gettempdir()).resolve()
                archive = Path(temp) / "rootfs.tar"
                subprocess.run(["docker", "export", "--output", str(archive), container], check=True)
                with tarfile.open(archive) as filesystem:
                    names = filesystem.getnames()
                    # Inspección del árbol de aplicación; el OS contiene certificados públicos y claves de paquetes.
                    app_files = [n for n in names if n.startswith("app/")]
                    forbidden = [n for n in app_files if re.search(r'(^|/)(\.env(?:\..*)?|\.local|\.aws|\.git|sessions|flask_session|TestResults)(/|$)|\.(dump|sqlite3?|db|pem|key|pfx|log)$', n)]
                    private_literals = []
                    for name in app_files:
                        member = filesystem.getmember(name)
                        if member.isfile() and member.size < 2_000_000 and name.endswith((".py", ".json", ".html", ".js")):
                            text = filesystem.extractfile(member).read().decode("utf-8", errors="replace")
                            if re.search(r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----|eyJ[A-Za-z0-9_-]{15,}\.[A-Za-z0-9_-]{15,}\.[A-Za-z0-9_-]{15,}', text):
                                private_literals.append(name)
                user = config["Config"].get("User", "")
                report = {"image": image, "id": config["Id"], "user": user,
                          "applicationFiles": len(app_files), "forbiddenApplicationPaths": forbidden,
                          "privateKeyOrJwtLiterals": private_literals,
                          "status": "passed" if user not in ("", "0", "root") and not forbidden and not private_literals else "failed"}
                reports.append(report)
        finally:
            subprocess.run(["docker", "rm", container], check=True, stdout=subprocess.DEVNULL)
    Path(args.output).write_text(json.dumps({"date": datetime.now(timezone(timedelta(hours=-5))).date().isoformat(), "checks": reports}, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(reports))
    if any(r["status"] != "passed" for r in reports):
        raise SystemExit(1)


if __name__ == "__main__":
    main()

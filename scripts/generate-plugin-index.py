"""Generate a Dalamud third-party repository index from the built manifest."""
import json
import sys
from pathlib import Path

tag = sys.argv[1]
manifest = json.loads(Path("bin/Release/PlayerCastBars.json").read_text())
if tag != "v" + manifest["AssemblyVersion"].removesuffix(".0"):
    raise SystemExit("Release tag must match the built assembly version.")

package_url = (
    "https://github.com/HanaSi0/Player-skill-casting-bar/"
    f"releases/download/{tag}/PlayerCastBars.zip"
)
manifest.update(
    DownloadLinkInstall=package_url,
    DownloadLinkUpdate=package_url,
    DownloadLinkTesting=package_url,
    IsHide=False,
    IsTestingExclusive=False,
)
Path("release").mkdir(exist_ok=True)
Path("release/pluginmaster.json").write_text(
    json.dumps([manifest], ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
)

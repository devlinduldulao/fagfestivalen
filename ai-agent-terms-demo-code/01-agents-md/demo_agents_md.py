from __future__ import annotations

import sys
from pathlib import Path


def find_agent_instructions(start: Path) -> list[Path]:
    """Return AGENTS.md files from repo root toward the working directory."""
    current = start.resolve()
    if current.is_file():
        current = current.parent

    found: list[Path] = []
    for folder in [current, *current.parents]:
        candidate = folder / "AGENTS.md"
        if candidate.exists():
            found.append(candidate)
        if (folder / ".git").exists():
            break  # Stop at the repo root, like real agents do.
    return list(reversed(found))


def main() -> None:
    start = Path(sys.argv[1]) if len(sys.argv) > 1 else Path.cwd()
    files = find_agent_instructions(start)

    print(f"Working directory: {start}")
    print(f"Instructions loaded: {len(files)}")
    if not files:
        print("No AGENTS.md found between here and the repo root.")
        return
    for index, path in enumerate(files, start=1):
        print("\n" + "=" * 72)
        print(f"{index}. {path}")
        print("-" * 72)
        print(path.read_text(encoding="utf-8").strip())

    print("\nEffective behavior:")
    print("- General repo rules load first.")
    print("- More local rules load later and can narrow the behavior.")


if __name__ == "__main__":
    main()


from __future__ import annotations

import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path


ROOT = Path(__file__).resolve().parent
SKILLS_DIR = ROOT / "skills"


@dataclass
class Skill:
    name: str
    description: str
    path: Path


def parse_frontmatter(text: str) -> dict[str, str]:
    if not text.startswith("---") or text.count("---") < 2:
        return {}
    _, block, _ = text.split("---", 2)
    values: dict[str, str] = {}
    for line in block.splitlines():
        if ":" in line:
            key, value = line.split(":", 1)
            values[key.strip()] = value.strip().strip('"')
    return values


def load_skills() -> list[Skill]:
    skills: list[Skill] = []
    for skill_md in sorted(SKILLS_DIR.glob("*/SKILL.md")):
        data = parse_frontmatter(skill_md.read_text(encoding="utf-8"))
        skills.append(
            Skill(
                name=data.get("name", skill_md.parent.name),
                description=data.get("description", ""),
                path=skill_md.parent,
            )
        )
    return skills


STOPWORDS = {"a", "an", "and", "for", "is", "of", "or", "the", "this", "to", "what", "when"}


def tokenize(text: str) -> set[str]:
    return {word.strip(".,!?\"'") for word in text.lower().split()} - STOPWORDS


def score(task: str, skill: Skill) -> int:
    haystack = tokenize(f"{skill.name.replace('-', ' ')} {skill.description}")
    return len(tokenize(task) & haystack)


def main() -> None:
    task = " ".join(sys.argv[1:]) or "make a conference deck outline"
    skills = load_skills()
    ranked = sorted(
        ((score(task, skill), skill) for skill in skills),
        key=lambda item: (item[0], item[1].name),
        reverse=True,
    )
    best_score, best = ranked[0]

    print(f"User task: {task}")
    print("Available skill descriptions:")
    for skill in skills:
        print(f"- {skill.name}: {skill.description}")

    if best_score == 0:
        print("\nNo skill matched. The agent keeps the extra instructions unloaded.")
        return

    print(f"\nLoaded skill: {best.name}")
    print(best.path.joinpath("SKILL.md").read_text(encoding="utf-8").strip())

    demo_script = best.path / "scripts" / "demo.py"
    if demo_script.exists():
        print("\nSkill script output:")
        sys.stdout.flush()
        subprocess.run([sys.executable, str(demo_script), task], check=True)


if __name__ == "__main__":
    main()

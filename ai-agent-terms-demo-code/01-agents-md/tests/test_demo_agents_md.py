from __future__ import annotations

import importlib.util
import tempfile
import unittest
from pathlib import Path


DEMO_PATH = Path(__file__).parents[1] / "demo_agents_md.py"
SPEC = importlib.util.spec_from_file_location("demo_agents_md", DEMO_PATH)
assert SPEC and SPEC.loader
demo_agents_md = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(demo_agents_md)


class FindAgentInstructionsTests(unittest.TestCase):
    def test_loads_root_rules_before_more_local_rules(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / ".git").mkdir()
            root_instruction = root / "AGENTS.md"
            root_instruction.write_text("root rules", encoding="utf-8")

            service = root / "app" / "services"
            service.mkdir(parents=True)
            service_instruction = service / "AGENTS.md"
            service_instruction.write_text("service rules", encoding="utf-8")

            self.assertEqual(
                demo_agents_md.find_agent_instructions(service),
                [root_instruction.resolve(), service_instruction.resolve()],
            )

    def test_stops_at_the_repository_root(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory) / "repo"
            root.mkdir()
            (root / ".git").mkdir()
            nested = root / "app"
            nested.mkdir()

            self.assertEqual(demo_agents_md.find_agent_instructions(nested), [])


if __name__ == "__main__":
    unittest.main()

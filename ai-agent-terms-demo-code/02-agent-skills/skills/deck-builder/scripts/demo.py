from __future__ import annotations

import sys


task = sys.argv[1] if len(sys.argv) > 1 else "make a deck"

print(f"Outline for: {task}")
print("1. Why agent vocabulary matters")
print("2. Project instructions: AGENTS.md")
print("3. Reusable capability: Agent Skills")
print("4. External systems: MCP and A2A")
print("5. Scaling the work: Subagents")


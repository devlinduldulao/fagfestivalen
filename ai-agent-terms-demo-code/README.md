# AI Agent Terms Demo Code

These demos are intentionally small and dependency-free. Run them with Python
3.10+ from this directory (use `python` instead of `python3` if that is how
Python is installed on your machine).

## 1. AGENTS.md

Shows how project instructions can be discovered from the working directory.

```bash
python3 01-agents-md/demo_agents_md.py 01-agents-md/app/services
```

## 2. Agent Skills

Shows progressive disclosure: the agent only loads the skill whose description
matches the task, and loads nothing when no description matches.

```bash
python3 02-agent-skills/agent.py "make a conference deck outline"
python3 02-agent-skills/agent.py "review this code for secrets"
python3 02-agent-skills/agent.py "what is the weather"
```

## 3. MCP

Shows an MCP-shaped client/server exchange over standard input and output.

```bash
python3 03-mcp/client.py
```

## 4. A2A

Shows one agent discovering another agent card and delegating a task to it.

```bash
python3 04-a2a/a2a_demo.py
```

## 5. Subagents

Shows a parent agent splitting independent checks across isolated workers.

```bash
python3 05-subagents/subagents_demo.py
```


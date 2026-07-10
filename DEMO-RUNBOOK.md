# Live Demo Runbook — "5 AI Agent Terms You Need to Know"

Rehearse this top to bottom until you can do it without reading. Every demo is
one command, dependency-free, and finishes in under 3 seconds.

---

## Pre-flight (do this before you go on stage)

```bash
cd /Users/devlinduldulao/Documents/DEVELOPMENT/fagfestivalen/ai-agent-terms-demo-code
python3 --version        # must be 3.10+
```

- [ ] Terminal font size cranked up (Cmd + several times) — test from the back of the room.
- [ ] Terminal theme: light background if the projector is weak.
- [ ] Clear the terminal before each demo: `clear`.
- [ ] Close notifications: macOS Focus mode ON.
- [ ] Dry-run all five commands once in the speaker room. All must exit 0.

**One-shot smoke test (run this the morning of the talk):**

```bash
python3 01-agents-md/demo_agents_md.py 01-agents-md/app/services && \
python3 02-agent-skills/agent.py "make a conference deck outline" && \
python3 03-mcp/client.py && \
python3 04-a2a/a2a_demo.py && \
python3 05-subagents/subagents_demo.py && echo "ALL DEMOS OK"
```

---

## Demo 1 — AGENTS.md (after slide 4)

**Setup line (say while typing):**
> "This repo has two AGENTS.md files — one at the root, one deeper in the
> service layer. Watch which order they load in."

**Run:**

```bash
python3 01-agents-md/demo_agents_md.py 01-agents-md/app/services
```

**Point at, in order:**
1. `Instructions loaded: 2` — the agent found both files.
2. File 1 = root rules (test command, PR title format).
3. File 2 = service-layer rules (pure functions, no network calls, and the full test suite after service edits).
4. The closing lines: *general rules first, local rules narrow them*.

**Landing line:**
> "That's the whole standard. It's a README, but the reader is an agent —
> and nesting gives you scoped rules exactly like `.gitignore` does."

**If asked:** "Claude uses CLAUDE.md — same idea, different filename.
AGENTS.md is the open standard under the Linux Foundation's AAIF."

---

## Demo 2 — Agent Skills (after slide 6) — *the crowd-pleaser, take your time*

**Setup line:**
> "Two skills are installed: deck-builder and security-review. The agent only
> reads their one-line descriptions — the full instructions stay unloaded."

**Run — matching task #1:**

```bash
python3 02-agent-skills/agent.py "make a conference deck outline"
```

Point at: only **deck-builder** was loaded, and its bundled script even ran.

**Run — matching task #2:**

```bash
python3 02-agent-skills/agent.py "review this code for secrets"
```

Point at: now only **security-review** loaded. Deck rules never entered context.

**Run — the punchline:**

```bash
python3 02-agent-skills/agent.py "what is the weather"
```

Point at: `No skill matched. The agent keeps the extra instructions unloaded.`

**Landing line:**
> "That's progressive disclosure. Descriptions are cheap; full skills are
> expensive. You only pay for what the task needs."

**Honesty beat (pre-empt the sharp attendee):**
> "One caveat — my demo matches keywords. A real agent uses the model itself
> to match descriptions semantically. The architecture is identical."

---

## Demo 3 — MCP (after slide 8)

**Setup line:**
> "The client starts with MCP's handshake, then asks a server it knows nothing
> about: what tools do you have?"

**Run:**

```bash
python3 03-mcp/client.py
```

**Point at, in order:**
1. First JSON block — `tools/list` response: the server *advertises* two tools.
2. Second JSON block — `tools/call` for `inventory.lookup`: the client never
   saw the inventory API, just the envelope.

**Landing line:**
> "One protocol, any server. Swap this inventory server for Notion or Stripe —
> the client code doesn't change."

**Honesty beat:** the demo performs the JSON-RPC 2.0 handshake, but keeps the
tool definitions and transport intentionally small so the terminal stays readable.

---

## Demo 4 — A2A (after slide 10)

**Setup line:**
> "Two agents, built independently. Procurement needs a spend approved.
> It has never seen the finance agent's code."

**Run:**

```bash
python3 04-a2a/a2a_demo.py
```

**Point at, in order:**
1. The **agent card** fetched from `/.well-known/agent-card.json` —
   "what I do, and where to send tasks."
2. First delegation: **4200 → approved** (under the 5000 policy limit).
3. Second delegation: **9500 → needs director review** — the policy lives
   entirely inside the finance agent.

**Landing line:**
> "No shared code, no custom integration. Discover the card, call the
> advertised endpoint, get a structured decision."

**If asked:** real A2A has a task lifecycle (submitted → working → completed);
this demo compresses it to a synchronous call for clarity.

---

## Demo 5 — Subagents (after slide 12)

**Setup line:**
> "Four independent reviews. One agent would do them one at a time and drag
> every file into one context window. Watch the wall clock instead."

**Run:**

```bash
python3 05-subagents/subagents_demo.py
```

**Point at, in order:**
1. Results arrive **out of order** — fastest check first. That's parallelism, live.
2. The wall-time line: **~0.9s in parallel vs 3.0s sequential**.
3. Final line: the parent keeps *summaries*, not every token the workers read.

**Landing line:**
> "Two wins in one pattern: speed from parallelism, and a clean parent context
> because children absorb the bulk reading."

**If asked:** threads with `time.sleep` simulate the workers here; real
subagents each get a fresh context window — that isolation is the point.

---

## Closing (slide 13 on screen)

> "Five terms, five boundaries. AGENTS.md and skills shape behavior *inside*
> the agent. MCP and A2A reach *outside* — to tools and to peers. Subagents
> scale *across* the work. When you're diagnosing an agent design, ask which
> boundary is broken, and you'll know which term you need."

---

## Recovery moves (when something goes wrong on stage)

| Symptom | Move |
|---|---|
| `python3: command not found` | `which python3` — worst case use the pyenv path: `~/.pyenv/versions/3.13.5/bin/python3` |
| Wrong output / weird state | `clear`, re-run the same command once. All demos are stateless and idempotent. |
| File-not-found errors | You're in the wrong directory: `cd ~/Documents/DEVELOPMENT/fagfestivalen/ai-agent-terms-demo-code` |
| Total demo failure | Every expected output is described above — narrate it from the slide's code snippet and move on. Never debug live for more than 30 seconds. |

## Rehearsal plan

1. **Pass 1–2:** read this file out loud while running each command.
2. **Pass 3–4:** run from memory; only glance at the landing lines.
3. **Pass 5:** full talk with slides + demos, timed. Demos should total < 6 min.
4. Rehearse the **recovery moves** once: deliberately run demo 1 from the wrong
   directory and practice recovering calmly.

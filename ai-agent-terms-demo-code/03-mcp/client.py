from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path


SERVER = Path(__file__).with_name("server.py")


def call(process: subprocess.Popen[str], request_id: int, method: str, params: dict | None = None) -> dict:
    request = {"jsonrpc": "2.0", "id": request_id, "method": method, "params": params or {}}
    process.stdin.write(json.dumps(request) + "\n")
    process.stdin.flush()
    line = process.stdout.readline()
    if not line:
        raise RuntimeError("MCP server exited before responding")
    return json.loads(line)


def notify(process: subprocess.Popen[str], method: str, params: dict | None = None) -> None:
    request = {"jsonrpc": "2.0", "method": method, "params": params or {}}
    process.stdin.write(json.dumps(request) + "\n")
    process.stdin.flush()


def main() -> None:
    process = subprocess.Popen(
        [sys.executable, str(SERVER)],
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        text=True,
    )
    assert process.stdin and process.stdout

    call(
        process,
        1,
        "initialize",
        {
            "protocolVersion": "2025-11-25",
            "capabilities": {},
            "clientInfo": {"name": "conference-demo", "version": "1.0"},
        },
    )
    notify(process, "notifications/initialized")

    print("Client initializes a standard server, then asks what tools it exposes:")
    print(json.dumps(call(process, 2, "tools/list"), indent=2))

    print("\nClient calls a tool without knowing its internal API:")
    print(
        json.dumps(
            call(
                process,
                3,
                "tools/call",
                {"name": "inventory.lookup", "arguments": {"sku": "sku-200"}},
            ),
            indent=2,
        )
    )

    print("\n(The demo uses the JSON-RPC 2.0 initialization flow and keeps the tool payloads small.)")

    process.stdin.close()  # Server exits cleanly on stdin EOF.
    process.wait(timeout=5)


if __name__ == "__main__":
    main()

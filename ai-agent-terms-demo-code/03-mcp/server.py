from __future__ import annotations

import json
import sys
from datetime import datetime, timezone


INVENTORY = {
    "sku-100": {"name": "Conference badge", "available": 42},
    "sku-200": {"name": "Workshop seat", "available": 7},
}


def response(message: dict, result: dict) -> dict:
    return {"jsonrpc": "2.0", "id": message["id"], "result": result}


def error(message: dict, code: int, message_text: str) -> dict:
    return {
        "jsonrpc": "2.0",
        "id": message.get("id"),
        "error": {"code": code, "message": message_text},
    }


def handle(message: dict) -> dict | None:
    if message.get("jsonrpc") != "2.0":
        return error(message, -32600, "invalid JSON-RPC version")

    method = message.get("method")
    params = message.get("params", {})

    if method == "notifications/initialized":
        return None  # JSON-RPC notifications do not receive a response.
    if method == "initialize":
        result = {
            "protocolVersion": params.get("protocolVersion", "2025-11-25"),
            "capabilities": {"tools": {}},
            "serverInfo": {"name": "inventory-demo", "version": "1.0"},
        }
    elif method == "tools/list":
        result = {
            "tools": [
                {
                    "name": "time.now",
                    "description": "Return the current UTC time.",
                    "inputSchema": {"type": "object", "additionalProperties": False},
                },
                {
                    "name": "inventory.lookup",
                    "description": "Look up inventory by SKU.",
                    "inputSchema": {
                        "type": "object",
                        "properties": {"sku": {"type": "string"}},
                        "required": ["sku"],
                    },
                },
            ]
        }
    elif method == "tools/call" and params.get("name") == "time.now":
        result = {"content": [{"type": "text", "text": datetime.now(timezone.utc).isoformat()}]}
    elif method == "tools/call" and params.get("name") == "inventory.lookup":
        sku = params.get("arguments", {}).get("sku")
        result = {
            "content": [
                {
                    "type": "text",
                    "text": json.dumps(INVENTORY.get(sku, {"error": "unknown sku"})),
                }
            ]
        }
    elif method == "tools/call":
        return error(message, -32602, f"unknown tool: {params.get('name')}")
    else:
        return error(message, -32601, f"unsupported method: {method}")

    return response(message, result)


def main() -> None:
    for line in sys.stdin:
        response = handle(json.loads(line))
        if response is not None:
            print(json.dumps(response), flush=True)


if __name__ == "__main__":
    main()

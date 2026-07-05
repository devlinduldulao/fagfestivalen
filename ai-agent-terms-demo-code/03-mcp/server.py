from __future__ import annotations

import json
import sys
from datetime import datetime, timezone


INVENTORY = {
    "sku-100": {"name": "Conference badge", "available": 42},
    "sku-200": {"name": "Workshop seat", "available": 7},
}


def handle(message: dict) -> dict:
    method = message.get("method")
    params = message.get("params", {})

    if method == "tools/list":
        result = {
            "tools": [
                {"name": "time.now", "description": "Return the current UTC time."},
                {"name": "inventory.lookup", "description": "Look up inventory by SKU."},
            ]
        }
    elif method == "tools/call" and params.get("name") == "time.now":
        result = {"content": datetime.now(timezone.utc).isoformat()}
    elif method == "tools/call" and params.get("name") == "inventory.lookup":
        sku = params.get("arguments", {}).get("sku")
        result = {"content": INVENTORY.get(sku, {"error": "unknown sku"})}
    elif method == "tools/call":
        return {"id": message.get("id"), "error": f"unknown tool: {params.get('name')}"}
    else:
        return {"id": message.get("id"), "error": f"unsupported method: {method}"}

    return {"id": message.get("id"), "result": result}


def main() -> None:
    for line in sys.stdin:
        response = handle(json.loads(line))
        print(json.dumps(response), flush=True)


if __name__ == "__main__":
    main()


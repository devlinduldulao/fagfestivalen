from __future__ import annotations

import json
import threading
import urllib.request
from http.server import BaseHTTPRequestHandler, HTTPServer


class FinanceAgent(BaseHTTPRequestHandler):
    def do_GET(self) -> None:
        if self.path != "/.well-known/agent-card.json":
            self.send_error(404)
            return
        self._json(
            {
                "name": "finance-agent",
                "description": "Approves spend requests under a policy limit.",
                "tasks": [{"name": "approve_spend", "url": "/tasks/approve_spend"}],
            }
        )

    def do_POST(self) -> None:
        if self.path != "/tasks/approve_spend":
            self.send_error(404)
            return
        size = int(self.headers.get("Content-Length", "0"))
        payload = json.loads(self.rfile.read(size))
        amount = payload["amount"]
        decision = "approved" if amount <= 5000 else "needs director review"
        self._json({"decision": decision, "amount": amount, "policy_limit": 5000})

    def _json(self, payload: dict) -> None:
        encoded = json.dumps(payload).encode("utf-8")
        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(encoded)))
        self.end_headers()
        self.wfile.write(encoded)

    def log_message(self, format: str, *args: object) -> None:
        return


def post_json(url: str, payload: dict) -> dict:
    request = urllib.request.Request(
        url,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(request) as response:
        return json.loads(response.read())


def main() -> None:
    server = HTTPServer(("127.0.0.1", 0), FinanceAgent)  # Port 0: never collides on stage.
    thread = threading.Thread(target=server.serve_forever, daemon=True)
    thread.start()

    base_url = f"http://127.0.0.1:{server.server_address[1]}"
    with urllib.request.urlopen(base_url + "/.well-known/agent-card.json") as response:
        card = json.loads(response.read())

    print("Procurement agent discovered this finance agent card:")
    print(json.dumps(card, indent=2))

    task_url = base_url + card["tasks"][0]["url"]
    result = post_json(task_url, {"vendor": "Acme Training", "amount": 4200})

    print("\nProcurement delegated approval and received:")
    print(json.dumps(result, indent=2))
    server.shutdown()
    server.server_close()


if __name__ == "__main__":
    main()


from __future__ import annotations

import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass


@dataclass(frozen=True)
class Check:
    name: str
    files: tuple[str, ...]
    seconds: float  # Simulated review time for this check.


def run_subagent(check: Check) -> str:
    # Each worker receives only the slice of context needed for its job.
    time.sleep(check.seconds)
    file_list = ", ".join(check.files)
    return f"{check.name}: inspected {len(check.files)} files in {check.seconds:.1f}s ({file_list})"


def main() -> None:
    checks = [
        Check("API contracts", ("api/users.py", "api/orders.py"), 0.8),
        Check("Database migrations", ("migrations/001_init.sql", "migrations/002_orders.sql"), 0.6),
        Check("Frontend forms", ("web/login.tsx", "web/checkout.tsx"), 0.7),
        Check("Security hotspots", ("auth/session.py", "billing/webhook.py"), 0.9),
    ]

    print("Parent agent splits four independent reviews into subagents.\n")
    started = time.perf_counter()
    with ThreadPoolExecutor(max_workers=4) as executor:
        futures = [executor.submit(run_subagent, check) for check in checks]
        for future in as_completed(futures):
            print(future.result())
    elapsed = time.perf_counter() - started
    sequential = sum(check.seconds for check in checks)

    print(f"\nWall time: {elapsed:.1f}s in parallel vs {sequential:.1f}s if run one at a time.")
    print("Parent agent keeps the final summaries, not every token each worker read.")


if __name__ == "__main__":
    main()


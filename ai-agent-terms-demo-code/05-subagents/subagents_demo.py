from __future__ import annotations

from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass


@dataclass(frozen=True)
class Check:
    name: str
    files: tuple[str, ...]


def run_subagent(check: Check) -> str:
    # Each worker receives only the slice of context needed for its job.
    file_list = ", ".join(check.files)
    return f"{check.name}: inspected {len(check.files)} files ({file_list})"


def main() -> None:
    checks = [
        Check("API contracts", ("api/users.py", "api/orders.py")),
        Check("Database migrations", ("migrations/001_init.sql", "migrations/002_orders.sql")),
        Check("Frontend forms", ("web/login.tsx", "web/checkout.tsx")),
        Check("Security hotspots", ("auth/session.py", "billing/webhook.py")),
    ]

    print("Parent agent splits four independent reviews into subagents.\n")
    with ThreadPoolExecutor(max_workers=4) as executor:
        futures = [executor.submit(run_subagent, check) for check in checks]
        for future in as_completed(futures):
            print(future.result())

    print("\nParent agent keeps the final summaries, not every token each worker read.")


if __name__ == "__main__":
    main()


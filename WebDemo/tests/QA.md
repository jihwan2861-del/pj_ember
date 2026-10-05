# Browser QA

Checked in Chrome on Windows. The game uses `?qa=1` for deterministic manual physics at 120 Hz. Normal player mode does not include that query.

## Evidence

- `browser-smoke-report.json`: HTTP load, assets, real DOM movement keys, pause/resume, restart, and all eight rooms' jump/death/respawn/initial torch state. The death checks use `debugTeleport`; they are contract checks, not completed routes.
- `play-routes-report.json`: rooms 1 through 8 completed in order using input press/release/mouse and the real physics update. No teleport, direct player state writes, level skipping, or torch state writes are used in this test. Normal next-room buttons advance the route. Room 5 opens the two-torch door, room 6 uses the vertical launch/ring/dash sequence, and room 8 finishes during an enemy attack.
- `focused-checks-report.json`: direct `file://` loading, immediate grounded jump, actual DOM right-click absorb and left-click charged shot, death while occupying a torch, and pausing the death timer with a help dialog.
- `regressions-report.json`: after the final resource-recovery fix, a grounded ring does not refund its use while waiting, airborne dash does not refund its use before landing/absorb, absorb restores the resource, and rooms 6/8 still complete with input only. The last HTTP check records zero console/runtime/network errors. The favicon 404 preserved in the earlier focused report was fixed before this check.
- `playable-scene.png`: 1440×1000 view after rooms 1–5 were completed and the room 6 dash/absorb sequence reached the second torch.

These tests verify mechanics and reachability. They do not establish beginner difficulty, enjoyment, or a 20-minute first play. The scripted successful route takes about 22.6 simulated game seconds because it already knows every target and timing.

## Reproduce

Serve this folder locally, then run `browser-smoke.cjs`, `play-routes.cjs`, and `focused-checks.cjs` with Node.js. The scripts use installed Playwright, or the bundled Codex Playwright path on this Windows host. `play-routes.cjs` and `focused-checks.cjs` use the current server `http://127.0.0.1:8791/`; `browser-smoke.cjs` accepts the URL as its first argument.

The test files do not modify game assets or implementation files.

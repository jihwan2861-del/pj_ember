# Pause and restart verification

Unity 6000.5.2f1 native EditMode tests with play-state transitions: 12 passed, 0 failed.
Five session-control tests cover real synthetic Esc/R input in the player loop, physics freezing and resuming, scene reload from initial state, restart while paused, aim slow-motion cancellation and fixed timestep restoration, disable cleanup, and death-sequence suspension/resumption. Four collectible and three weight-platform regression tests also pass.

Synthetic keyboard input used IgnoreFocus and game-view input routing in the isolated headless editor; normal project input settings were not changed. Physics resumption was checked by yielding real player-loop frames.
Main runtime source hashes match the validated QA sources. New component metadata is included. No scene, player prefab, build scene list, or package settings were edited.
Results: UnityResults.xml. Log: unity.log.

Restart wipe update: RestartWipeResults.xml has 14 passed tests, 0 failed. Tests verify full screen coverage before reload, the same persistent overlay across scene replacement, reveal and cleanup, duplicate input suppression, restart while paused, and restart during death. Existing pause, collectible and platform tests remain passing. Main source hashes match the QA sources; new script metadata is included. No scene or prefab edits were needed.

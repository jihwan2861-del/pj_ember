# Main menu validation — 2026-10-08

Status: menu ready for editor use; standalone display/quit smoke test still required.

- Unity 6000.5.2f1, Input System 1.19.0, uGUI 2.5.0.
- A new isolated QA project imported the same runtime scripts and GUIDs as production. Scene serialization uses the production `Assembly-CSharp` identity.
- Scene creation passed: no missing components, Korean/Latin UI glyph coverage checked.
- Actual Unity camera-rendered previews inspected: 1920×1080 menu, 1280×960 menu, settings modal.
- Final PlayMode results: **9 passed / 0 failed** (`MainMenuResults.xml`).
- Covered keyboard wrap/navigation, Z submit, Esc cancel, mouse open/back, volume/display preference saving, repeated listener enable/disable, double-start prevention, unscaled fade and asynchronous scene load, missing-target recovery, aspect-ratio crop cases.
- Scene-load test used an empty **QA-only target**, not the user's full gameplay level.
- Hidden-editor input simulation explicitly ignores editor focus only in tests, restoring settings afterward. Production input settings are unchanged.
- Quit and fullscreen/resolution changes are wired to the Unity player APIs but have not been exercised in a standalone executable.
- The production gameplay scene and currently open scene were not opened or saved by validation.
- Production build list: EmberMainMenu first, prior EmberPrototype/SampleScene entries preserved, game 1 enabled.
- Source project's runtime and editor assemblies also recompiled after the added scripts (observed output timestamps); no original scene/prefab replacement was performed.

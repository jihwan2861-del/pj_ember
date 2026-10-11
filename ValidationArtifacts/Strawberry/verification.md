# Strawberry pickup verification

Unity 6000.5.2f1 native EditMode tests with play-state transitions: 7 passed, 0 failed.
- Four strawberry tests: trigger pickup and airborne follow, landing confirmation without duplicate counts, collected persistence through MapReset, unconfirmed death return, multiple independent followers, direct player reset, non-player rejection and disable cleanup.
- Three WeightPlatform regression tests remain passing.

Final run includes the main project's script GUIDs. No missing-script or C# compiler errors were found. The verified prefab, PNG and metadata were copied to the main project; references were checked against its script and sprite GUIDs.
Game scene SHA256 remained E9F076D176DAB339FB9568660B924F1CA2F9C0C828788A3EC47131A80B7EEFDF.
Collected counts live on the player for this scene/session only; no disk save or HUD added.

Chicken reskin: ChickenResults.xml contains 7 passed tests, 0 failed. The existing prefab was renamed to ChickenPickup with its GUID and file IDs preserved, and only its display name and sprite reference changed. Runtime collection behavior remains unchanged.

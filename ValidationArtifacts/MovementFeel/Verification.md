# Movement feel change verification

Final result: **17 passed / 0 failed / 0 skipped** in Unity 6000.5.2f1. Unity Test Runner completed with exit code 0. The isolated QA Unity process exited; the original project editor was not stopped or driven.

Production runtime and Editor C# also compiled with `dotnet msbuild Assembly-CSharp-Editor.csproj /t:Build`, exit code 0. OutputPath and IntermediateOutputPath were overridden to this artifact directory, leaving the generated project files and original Library untouched by the command. See `dotnet-compile-final.log` for the unsuppressed compiler output.

The unchanged copies of all 26 runtime C# sources match the production source SHA256 hashes after the final test run. `source-snapshot.json` records that check and the fixture/scene fingerprints. Source mutations were limited to the separate validation harness, its runner, and reports; this QA task did not edit production runtime, scenes, prefabs, packages, or settings.

Passed scenarios:

- Up+right and down+right movement retain the same horizontal speed as right-only input.
- C+Z begins a dash in the same input frame.
- A dash blocked immediately does not replay the chord's Z as an air jump.
- A new Z press near dash end is consumed as a jump after returning to Free.
- Releasing Z during the ignition ring updates held state and remains released after ring expiry.
- Real X absorption/travel reaches an anchor; Z launch does not also replay as an air jump.
- Shot release preserves the held walking direction on the first Free physics tick.
- Holding forward preserves more launch velocity than releasing; opposite input brakes during the control lock.
- Jump close to landing waits for the ground and preserves the additional air jump; away from the ground the air jump remains immediate.
- Horizontal and diagonal dash directions remain unchanged near obstacles.
- Full-body collider restoration returns to the last clear recorded position if the dash ends in a gap that only fits the small collider.
- If both recorded positions are blocked, NeedsSafeReset is raised with both colliders disabled and zero exit velocity.
- When the starting body space is obstructed, the dash retains its full-body collider.

The final fixture uses scene 2's move 3.3, jump 6.09, gravity 1.8, air deceleration 28, apex 1/.65, fall multiplier 1.6, launch lock .12, dash exit lock .02, and body box size/offset. Native Physics2D is stepped at .02 seconds. The small test dash circle is deliberately .15 radius to construct a reproducible restoration case.

The first attempt had an **input fixture issue**, preserved in `UnityResults-fixture-editor-input.xml`: EditMode's Editor input update context delivered held states but did not produce the player press edges required by the controller. The fixture now enables Input System 1.19's `runPlayerUpdatesInEditMode` option only within each test and restores it afterward. Every synthetic held/press/release event is asserted before gameplay code runs. No unexpected-log suppression or ignored failing tests was introduced. The intermediate 15-pass result preceded the final scene tuning and two added regressions; the authoritative final result is `UnityResults.xml`.

The recorded original Editor baseline had no CS compilation error messages, but its timestamp preceded the changes. It is not evidence of an original-project post-change Unity import. Generated-project compilation warns about existing .NET4.7.1/NUnit4.7.2 reference resolution and Unity 6 API deprecations; the retained legacy dash-assistance serialized fields also produce unused-field warnings. These were not suppressed.

Limits: this is an isolated scripted EditMode input/physics verification, not a full route or visual test of production scene 2. The production player build, manual movement feel, animation appearance, and performance were not exercised. The original project's Editor scripts were compiled, while their asset-generation hooks were deliberately excluded from the isolated QA project.

Reproduction instructions and fixture source: `Validation/Movement/README.md`, `Run-IsolatedRegression.ps1`, and `EmberMovementRegressionTests.cs`. Temporary Unity caches live outside the repository at `%LOCALAPPDATA%/Temp/EmberMovementQA-20261004`.

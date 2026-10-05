# Scene 2 flat-floor snag evidence

Unity 6000.5.2f1 isolated EditMode/native Physics2D comparison: **4 tests passed, 0 failed**, exit code 0. Fixed steps are .02 seconds. These four terrain tests are separate from the previous 17 movement regressions which used generic box floors.

The runner imports native Grid/Tilemap scene documents from the original before/after scene 2, preserving its 753 cells, actual RuleTile/atlas references, Grid position x=.04, Tilemap scale .5, and Grid collider type. The player's current movement, body box, friction and materials remain unchanged in the comparison.

| Evidence | Original terrain | Static Rigidbody + Composite, extrusion 0 |
|---|---:|---:|
| Tilemap source shapes | 753 | 0, delegated to Composite |
| Composite contours / points / physics shapes | 0 / 0 / 0 | 5 / 78 / 35 |
| Flat-floor movement over 100 ticks | 1.017 U | 6.261 U |
| Stalled / horizontal-contact ticks | 83 / 83 | 0 / 0 |
| Movement after landing, 60 ticks | 1.017 U | 3.734 U |
| Stalled ticks after landing | 43 | 0 |

On the actual flat cell row -7, the original body stops near x=-.182855. The first horizontal contact is at tick 17: normal (-1,0), point approximately (.03136,-2.98), horizontal velocity 0. This is the internal tile boundary near world x=.04, away from the authored raised wall. The baseline test explicitly requires that snag and contact; the corrected test requires continuous movement without either.

The authored raised wall beginning at world x=-6.96 still stops a rightward dash in both variants, near body x=-7.39 after four steps, with the full-body collider restored. The comparison therefore preserves intentional wall blocking.

Extrusion .01 was separately applied and tile data refreshed. It expanded the world terrain bounds by about .005, while providing the same flat-floor displacement and zero stalls as extrusion 0. Production extrusion 0 is sufficient here.

No Composite.GenerateGeometry call is used in the final fixture. The corrected native scene already has five paths immediately after OpenScene, and remains at five after TilemapCollider.ProcessTilemapChanges, confirming automatic Synchronous generation. The missing-composite inspector warning condition changes from true to false; the headless runner does not display the original Inspector UI.

The authoritative results are `UnityResults.xml`, `summary.json`, and the 420 per-tick walking samples in `simulation.csv`. `terrain-snapshot.json` records hashes for original and native-extracted scenes, RuleTile, atlas/meta, and fixture source. Extracted scene snapshots are also preserved beside this report. All 26 copied runtime source hashes still match production.

Re-run `Validation/Movement/Prepare-TilemapRegression.ps1`, wait for the isolated Unity process to exit, then run `Collect-TilemapEvidence.ps1`. The original Unity editor is not driven or closed. The full scene route, rendering, dynamic obstacles and standalone player build are outside this comparison.

# Shared Grid migration — 2026-10-21

The user approved aligning Room2 by (-0.05, -0.12) world units and merging the existing room maps.

## Live scene result

- `WorldGeometry/Grid` contains one Terrain (575 cells), Spikes (32 cells), and Decorations (0 cells).
- Both CameraRooms reference this same LevelTilemaps object and their own existing Spawn.
- All 607 cells retain their tile asset, transform, flags and collider type. The previous per-map tint was multiplied into per-cell colors before merging.
- All 32 retained room objects preserve their world positions plus the approved room alignment offset.
- Terrain CompositeCollider2D generated 60 collision shapes.
- The previous empty `Room` container was removed after grouping rooms under `Rooms`.
- Actual scene remains `Assets/Scenes/skin scene.unity`, in Edit mode and dirty. Ctrl+S saves the complete live result.
- Live editing target is shared Terrain, with the Ember Doodle palette.

## Checkpoints and prefab

The sample `SkinRoom_Example.prefab` now has only its original Spawn; the sample Center and Finish objects were removed. No scene checkpoint objects were deleted by this migration. Linked CameraRoom Spawn points are saved on player room entry, with cinematic, input-lock and pause deferral. Unlinked rooms keep their previous behavior, and a same-room midpoint checkpoint is retained.

## Evidence and limits

- `01_Before_*` / `02_After_*`: Unity PreviewScene camera renders of save-as-copy snapshots. These preserve the actual dirty scene layout. Room1's visible terrain remains unchanged. Room2 retains its silhouette and uses the aligned world-coordinate texture phase.
- `03_Final_Play_GameView.png`: actual Game View captured in Play. The live player was alive in SkinRoom_01 and its checkpoint reference was SkinRoom_01/Spawn. Returned to Edit afterwards.
- Disposable Editor-scene checks passed: shared creation/reuse; no extra per-room map; painted-cell preservation; creation Undo/Redo; hidden shared skin display preservation; rejection before mutation for unaligned grids and differing physics; aligned merge and custom object/cell data preservation; generated collision; merge Undo/Redo.
- Isolated real runtime component checks passed for room-entry saving, manual midpoint retention, unlinked rooms, lock deferral, cinematics, motion suspension, non-player targets, and disabled/re-enabled Spawn. Result: `Library/EmberTutorial/RoomEntryCheckpointValidation.json`.
- These checks do not constitute an input-driven traversal of both edited rooms or a player build.

## Scene copies

- Before: `Assets/Scenes/Backups/skin_scene_BeforeSharedGrid_20261021.unity`
- After: `Assets/Scenes/Backups/skin_scene_AfterSharedGrid_20261021.unity`

`MigrationResult.json` records the measured live migration values. Temporary validation scripts remain under ignored Library/EmberTutorial.

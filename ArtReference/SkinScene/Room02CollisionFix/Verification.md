# Room_02 collision repair — 2026-10-21

- Baseline: Room_02/Grid/Terrain has111 occupied cells, all GrayboxBackground, colliderType=None. TilemapCollider2D Merge and static CompositeCollider2D are enabled, but geometry has0 shapes.
- User prefers the light tile appearance. GrayboxLightSolid.asset copies the Background sprite/color/transform/flags and changes only colliderType to Grid. The shared Background asset remains None.
- Only Room_02's111 Background cells were replaced. Per-cell colors, transforms, and flags were checked equal; geometry now has15 shapes. The scene was saved from an initially clean state.
- GrayboxLightSolid was added at cell(0,-2) to Ember Graybox and Ember Doodle palettes.
- Captures01_Before.png and02_After.png are Unity PreviewScene renders. Their SHA256 is identical:63DFD495AC21884DDB9A8DE18FF64F3C89126F0517910F90A11B6987398B8422.

In Play mode, a separate local PhysicsScene2D copied the actual Room_02 Grid/Terrain and used the player's BoxCollider dimensions/offset and gravityScale1.8. A one-second drop above cell(-1,-4,0), surfaceY3.62, started atY5.141323:

| Version | Final body Y | Bottom Y | Touching terrain | Shapes |
| --- | --- | --- | --- | --- |
| Before | -3.761254 | -4.082578 | false |0|
| After |3.956318|3.634995|true|15|

The temporary simulation scene was unloaded and Edit mode restored. This validates collision geometry and the physics drop; it is not a player-input room traversal test. No runtime code or physics settings were persisted. Failing-scene and palette backups are in Library/EmberTutorial/BeforeRoom02CollisionFix.

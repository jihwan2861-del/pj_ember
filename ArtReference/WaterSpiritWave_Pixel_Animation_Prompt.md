# Water Spirit Wave — Pixel Animation

- Created with the built-in image generation tool, using the original WaterWave.png silhouette and WaterlitRuins_WaterHazard_Frame01.png palette/style reference.
- Output: Assets/image/WaterSpiritWave_Pixel_4Frames.png.
- Verified file size: 1774 × 887 pixels, 32-bit RGBA; corner alpha is 0. Source and project-copy SHA256 hashes match.
- Four frames appear left to right; curling crest and internal highlights change. Playback smoothness has not been tested in Unity.
- Original WaterWave.png, prefab, collision, and runtime scripts were not modified.

## Unity import starting point

Use Sprite (2D and UI), Sprite Mode Multiple, Filter Mode Point, Compression None. Keep a common Pixels Per Unit and bottom-center pivot. Try 6 FPS, frames 1 → 2 → 3 → 4 → 1, Loop Time enabled.

The generated width is not divisible by four. For pixel-exact manual slicing use these full-height rectangles (x, y, width, height), and inspect the pivots in Sprite Editor:

1. (0, 0, 444, 887)
2. (444, 0, 443, 887)
3. (887, 0, 444, 887)
4. (1331, 0, 443, 887)

These are generated art frames, not an already-connected Animator or a certified seamless animation. Small frame alignment/edge cleanup may be needed at the intended game scale.

## Exact generation prompt

Create a game-ready animated pixel-art water-spirit wave sprite sheet.

REFERENCE ROLES:
Image 1 (WaterWave.png) is the exact shape and design reference: a tall upright body of water rising from a flat broad base, tapering toward a hooked curling crest that points screen-right. Preserve this recognizable silhouette and overall tall proportions. It is a magical wave, not a humanoid.
Image 2 is only the pixel texture and blue/cyan palette reference from the game's water artwork. Do NOT copy its full rectangular ocean scene or create scenery.

OUTPUT LAYOUT:
Exactly FOUR animation frames in one horizontal row, in four equally sized cells. Canvas 1024 x 512 pixels, each cell 256 x 512. Entire wave visible inside each cell with transparent margins. Use the same size, fixed bottom-center anchor, identical bottom baseline, and fixed camera in every cell. No cell lines or labels. Genuine transparent alpha background.

ART STYLE:
Clearly visible, deliberately chunky pixel art with crisp stair-stepped edges and clusters of square pixels. Small limited palette: very dark navy-blue at the base, deep blue body, medium azure, cyan on the curling crest, and a sparse pale aqua foam rim. Shade with clean discrete pixel color bands and pixel clusters, NOT smooth gradients. A few short cyan streaks inside the wave suggest water flowing upward. Readable, bold 2D side-view game obstacle silhouette. Keep the interior of the wave filled and opaque. No diffuse glow, blur, antialiasing, airbrush, photorealism, smoke, or realistic splash spray.

ANIMATION:
Four distinct but closely matched frames for a gentle looping wave motion.
Frame 1: curl in the reference-like neutral pose, a few pale foam pixels along the crest.
Frame 2: crest curls slightly tighter toward screen-right, interior cyan streaks advance a little upward, foam pattern shifts slightly.
Frame 3: crest relaxes slightly outward toward screen-right, foam and flowing streaks continue their small movement.
Frame 4: crest returns close to the neutral pose, foam pattern transitions smoothly back to Frame 1.
The main body and broad flat base stay nearly fixed, with only tiny surface ripples. Changes are modest, not four different wave designs. Preserve equal height and width bounds with no translation or camera movement. Loop 1 → 2 → 3 → 4 → 1.

No eyes, face, mouth, humanoid, clothing, accessories, characters, scenery, floor, shadows, horizon, background color, checkerboard pattern, motion arrows, numbers, text, borders, or watermark. Transparent empty space outside the wave.


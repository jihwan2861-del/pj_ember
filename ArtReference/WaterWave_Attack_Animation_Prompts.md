# Water Wave Attack — 4 Frames

- File: Assets/image/WaterWave_Attack_4Frames.png
- Generated with the built-in image generation tool. The user's supplied breaking-wave screenshot is the design/pixel-art reference, not a background to reproduce.
- Detached right-facing wave attack; blue water mass, white forward-breaking foam, transparent curl and background. Not a sea background strip.
- Layout: 2 × 2; playback order top-left → top-right → bottom-left → bottom-right.
- Verified output: 1254 × 1254, Format32bppArgb; corner alpha 0. Source and saved file SHA256 hashes match.
- Equal slicing: each cell 627 × 627. Sprite Mode Multiple, Point filtering, Compression None; consistent PPU. Start with 8 FPS and Loop Time.
- Generated frame registration is approximate: inspect and align the same lower-body anchor with per-frame custom pivots in Sprite Editor to prevent animation jitter. Loop smoothness has not been tested in Unity.
- Artwork only: no prefab, collision, Animator, attack timing, or runtime scripts were changed. Use the dense body for a separate stable gameplay collider rather than tracking individual foam droplets.

## Generation prompt

Use case: stylized-concept.
Asset type: animated WATER WAVE ATTACK sprite sheet for a side-scrolling pixel-art action platformer.

Image 1 is the design and pixel-art reference for the ATTACK ONLY. Ignore its gray editor/checkerboard background, dark border, and cursor. Use its powerful compact sideways breaking wave and bold white foam.

Make a detached moving wave attack, not sea scenery. Strict orthographic SIDE VIEW. The attack travels toward SCREEN-RIGHT: a dense cobalt-blue curved water body surges up from a short rear wake at the lower-left, then curls forward into a thick bright white foamy breaking crest at the upper-right. The leading right side must look like a forceful mass of crashing water that can hit the player; rear left tapers into a few short curved water streaks. A small clear crescent-shaped hollow under the forward crest is transparent. Overall silhouette is compact and slightly wider than tall, leaning FORWARD, not a tall standing water pillar. Preserve the reference's bold simple sweeping curve, strong blue filled mass, and thick irregular white foamy cap. A few separated square foam droplets near the front support motion without obscuring the main shape.

STYLE:
Authentic low-resolution pixel game sprite, as if drawn on a 96 x 96 logical pixel canvas then enlarged with nearest-neighbor. Big clear square pixel clusters and hard stair-step contours. Only 6 to 8 flat colors: deep navy shadow, cobalt-blue main water, medium blue, cyan highlights, pale aqua, white foam. Mostly blue body and white foam like the reference. No gradients, blur, antialiasing, smooth vector edges, painterly texture, realistic water, glow fog, or hundreds of tiny noisy speckles.

FOUR-FRAME TRAVEL ANIMATION:
Exactly four distinct closely matched frames of this SAME moving attack.
1: compact crest beginning to fold forward, foam ribbon climbing around the curve.
2: crest rolls farther forward with a slightly wider leading foam cap, a few droplets release ahead.
3: crest curls down a little with fresh foam clusters along the front lip and a curved internal water highlight.
4: crest rises back toward frame 1, droplets and foam continue shifting to make a repeating cycle.
Use small shape changes, stable attack size, same orientation and same cell-relative bottom-center anchor. This is an in-place looping TRAVEL animation; the game moves the attack object. Do NOT have the sprite translate between frames. Do NOT shrink, fade away, explode, or add an impact-only frame. No massive vertical bouncing. Keep clear readable attack silhouette and front-facing direction in all frames.

LAYOUT:
One square PNG sprite sheet, exactly FOUR equal square cells in a 2 x 2 arrangement. Order: top-left → top-right → bottom-left → bottom-right. Each cell has the same comfortable transparent padding on all four sides; leave enough room for foam spray, with no cropping or overlapping frames. Place each attack at identical local coordinates and scale. No grid lines or labels.

BACKGROUND:
Genuine transparent alpha outside each attack and in the curl opening. Filled water and white foam stay opaque. No checkerboard pattern, gray backdrop, canvas texture, sky, sea rectangle, water surface extending across the frame, horizon, floor, rocks, scenery, humanoid, eyes, character, cursor, number, text, frame border, watermark, or shadows. ONLY the four isolated pixel-art attack sprites.

## Padding/registration revision prompt

Edit the attached four-frame pixel-art WATER WAVE ATTACK sprite sheet. This image is the edit target.

Change ONLY sprite-sheet framing and registration: uniformly reduce the four sprites so EACH entire sprite, including all foam droplets, fits within the central 72 percent of its own square cell. Add generous fully transparent padding of at least 12 percent on EVERY side of EVERY cell. Nothing should touch the outer canvas edges or the central division between cells. Use exactly four equal square cells in a 2x2 layout on a square canvas.

Match the cell-relative lower-body baseline, scale and bottom-center anchor across all four frames. No cropping, no overlap. Remove isolated accidental faint colored noise outside the deliberate handful of square foam droplets.

KEEP the current attack design, facing screen-right, compact forward-breaking cobalt-blue water mass, white foamy crashing crest, cyan highlights, transparent curl hollow, chunky low-resolution stepped pixel-art texture, limited flat colors, and all four distinct small travel-animation poses. Do not replace it with a sea strip or background; this is a detached attacking wave. No characters, scenery, checkerboard, floor, labels, borders, shadows, glow, text or watermark. Genuine transparent alpha background. Preserve opaque blue water and white foam.


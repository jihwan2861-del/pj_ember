# Side-view pixel sea animation

Output: Assets/image/SeaSideView_Pixel_4Frames.png
Method: built-in image generation; reference used only for water palette/pixel style. The upright wave silhouette is intentionally discarded.
Four frames: top-left → top-right → bottom-left → bottom-right. Transparent air above the water, filled blue water below.
Original files and prefab/scripts are unchanged. Animation playback and seamless horizontal repetition have not been validated in Unity.
Import starting point: Sprite Multiple; Point filter; Compression None; common PPU and pivot; about 6 FPS with Loop Time enabled. Inspect frame alignment and edges in Sprite Editor before final use.

## Exact prompt

Use case: stylized-concept.
Asset type: 4-frame animated pixel-art sea strip sprite sheet for a side-scrolling 2D platformer.
Input image 1: color palette and pixel-cluster style reference ONLY. Do not copy its perspective or full composition.

Primary request: a wide horizontal body of SEA WATER viewed strictly from the SIDE, like a 2D water cross-section filling a platformer pit. Forget all tall hooked wave silhouettes. NOT a standing wave character, NOT a single curling crest, NOT a top-down view, NOT an ocean landscape seen receding into the distance.

Composition within EACH frame: a horizontal water surface running all the way from left edge to right edge about 22 percent down from the top of the cell. Empty transparent air above it. Fully filled solid blue water below it extending uninterrupted to the entire bottom edge and both side edges. The water has no bottom outline, basin, rocks, beach, or underwater ground; it continues below the crop. Small shallow rolling waves along the waterline, pale-cyan broken foam strips, a few light-blue pixel ripples just below the surface, deeper navy at the bottom. The surface waves should be low and subtle, not towering. Make this look like a side-view sea water tile, not a framed illustration and not a trapezoid.

Style: genuine visibly chunky pixel art, crisp stepped pixel edges, coherent square-pixel clusters, limited dark navy / deep blue / azure / cyan / pale-aqua palette compatible with the reference cave water. Flat discrete color bands and clean clusters, no smooth painted gradients, no blur, no anti-aliasing. Readable simple water at small game scale.

Layout: exactly FOUR equally sized WIDE rectangular animation cells in a clean 2-by-2 grid, reading top-left, top-right, bottom-left, bottom-right. Each cell shows the same stretch of water at the same scale, surface height and alignment. A little clear transparent separation between rows. No separator lines or text. Transparent background above each water surface; water itself is opaque. Each frame fills its entire cell width so it can be extended as a sea strip; keep left/right waterline height and edge colors matching as closely as possible.

Animation: four closely related poses of one gentle repeating ripple cycle. Small crests and foam highlights drift slightly to the right, with a tiny rise and fall around a constant average water level. Deep-water color shapes stay stable. Frame 4 approaches frame 1. Do not redesign the water in each frame. No camera movement.

Avoid: tall upright hooked silhouette, tsunami, large breaking wave, character, eyes, face, spirits, boats, fish, vegetation, rocks, platforms, horizon, sky, clouds, scenery, visible seabed, frame labels, numbers, borders, checkerboard texture, watermark, diffuse glowing mist. Genuine transparent alpha above the sea, no colored background.


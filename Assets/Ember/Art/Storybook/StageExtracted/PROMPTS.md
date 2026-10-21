# 이미지 추출 프롬프트

도구: 내장 image_gen. 모든 요청은 첨부 이미지를 편집 대상으로 사용했습니다.

공통 지시: 원본의 불규칙한 검은 잉크선과 수채화·색연필 질감을 유지하고, 레이블·프레임·체커보드 패턴을 추가하지 않습니다.

## 01_MountainBackground

투명 배경: false

Extract a clean 16:9 FULL FRAME background containing ONLY the original white sky and pale blue/lavender snow mountains. Remove ALL foreground stone walls, floor, platforms, yellow grass, spikes, hanging ropes and fires, player, sunflower, purple corruption, clouds, sun and projectiles. Seamlessly reconstruct mountains and white sky behind removed objects. Keep the original mountain placements and scale; do not invent a foreground. Opaque image.

## 02_Clouds

투명 배경: true

Extract ONLY the five pale blue hand-drawn clouds visible in the scene as a transparent cloud sprite collection. Arrange the five individual cloud shapes in two rows with generous transparent gutters between them, not touching. Keep their varying sizes and original watercolor texture and broken blue ink outlines. No mountains, sky background, sun, terrain, characters or other assets. Genuine transparent alpha everywhere around AND between clouds.

## 03_Sun

투명 배경: true

Extract ONLY the small yellow hand-drawn sun in the top right, including its sketchy orange rays. One complete isolated sun centered with generous transparent padding. Preserve its yellow pencil texture and original round shape. No cloud, white rectangle, mountain or other objects. Genuine transparent background.

## 04_HangingFireRope

투명 배경: true

Extract ONE representative hanging burning wood bundle and its attached vertical rope from the scene, choosing the clearly visible upper-left hanging bonfire. Include the entire vertical rope, binding knot, brown branch cage, orange/yellow flame and a few immediate sparks. Reconstruct the upper rope end cleanly without ceiling terrain. One object centered with clear transparent margin. Remove all terrain, spikes, clouds, background, other fires and player. Genuine transparent alpha.

## 05_SunFlower

투명 배경: true

Extract ONLY the orange-gold sunflower from the bottom left. Include full flower head, brown seed center, green stem and all leaves down to base. Reconstruct any stem pixels hidden behind yellow grass; do not include grass, ground, stone or any scenery. Keep original side-on drawing and colors. One complete isolated flower centered, generous transparent margin, genuine transparent alpha.

## 06_FlamePlayer

투명 배경: true

Extract ONLY the small flying flame spirit near center. Preserve the right-facing flying pose, yellow face with dark eye, red/orange flame outline and small body. Include its SHORT nearest flame tail, but remove long dotted trajectory across scene. Single character centered with ample transparent padding. No hanging bonfire or rope, terrain, mountains, UI, text. Genuine transparent background.

## 07_CorruptionVines

투명 배경: true

Extract ONLY the purple-black thorny corruption vine growth at the upper-right edge. Preserve its tangled ink-black curling thorns, lilac wash and bright violet spots. Reconstruct truncated ends so it can be placed as one complete independent bramble patch. No rocks, yellow grass, sun, projectile or scene. Center one isolated bramble patch with transparent padding on all sides. Genuine transparent alpha.

## 08_RockTerrainLayer

투명 배경: true

Extract ONLY ALL gray blocky foreground rock terrain with its yellow grass caps, including left/right walls, ceiling, bottom floor, central pillars and the two floating platforms. Preserve their exact original 16:9 layout on the original full canvas. Remove EVERY black triangular hazard spike attached to terrain and naturally restore rock or grass behind them. Remove all mountains, sky, clouds, sun, hanging ropes/fires, player, sunflower and corruption vines. All empty playable space must be genuinely transparent, including gaps and cavities. This is a full scene terrain layer, not a tile atlas. Do not add any new platforms or move any existing geometry.

## 09_SpikeHazards

투명 배경: true

Extract a representative reusable hazard set from the original scene: exactly FOUR clearly separated individual spike strip sprites arranged in a 2x2 grid with wide transparent gutters: 1 horizontal row of five upward pointed spikes, 1 horizontal row of five downward pointed spikes, 1 vertical row of five left-pointing spikes, 1 vertical row of five right-pointing spikes. Match the original thin black hand-drawn triangular outlines and pale white/gray interiors exactly. No rock or yellow grass, no terrain, text, frames or anything else. Preserve opaque pale interiors of spikes, genuine transparent background outside spike outlines. All four strips fully inside canvas and separated.


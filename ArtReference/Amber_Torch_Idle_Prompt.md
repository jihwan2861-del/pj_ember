# 앰버 — 횃불 안 대기 4프레임

- 생성 방식: 기본 내장 이미지 생성 도구, 투명 배경.
- 결과: `Assets/player/Amber_Torch_Idle_Sheet.png`.
- 구성: 가로 1줄, 4프레임. 불꽃 속 얼굴과 일렁이는 불꽃.
- PNG 크기: 1774×887. 실제 플레이용으로 사용할 때는 각 프레임의 영역과 피벗을 확인한다.
- Animator와 기존 횃불 프리팹은 변경하지 않았다.

## 최종 프롬프트

 Use case: sketch-to-render.
 Asset type: four-frame looping 2D game animation sprite sheet, Amber inhabiting a lit torch.
 Input images: Image 1 is the user's structural sketch to interpret; Image 2 is the existing Amber head design and rendering-style reference; Image 3 is the existing tiny idle sprite, only a reference for recognizable simple eyes and warm flame identity.
 Primary request: turn the sketch into exactly FOUR successive animation frames of the same cute flame spirit living INSIDE the torch flame. The head IS the fire sitting in the torch cup, not a small humanoid standing above it. Keep the two dark vertical oval eyes, no mouth, no arms, no legs.
 Composition: a landscape spritesheet with exactly four evenly sized cells in ONE horizontal row from left to right. All four complete torches fully visible, ample transparent margins above and below, equal spacing, no overlap. Flat front-facing 2D view, no perspective rotation. Identical scale, identical cup and handle, cup rims and handle ends on identical horizontal baselines. Aim for a 2048x1024 canvas, four 512x1024 cells.
 Torch construction: follow the sketch's proportions: broad rounded shallow cup and rim beneath the character's round fire face, narrowing down to a short wrapped wooden torch handle. Simple warm dark bronze cup, understated wood and cream binding. Do not add stands, altars, scenery, ornate accessories, or objects.
 Character rendering: match the warm orange exterior, yellow flame lobes and bright cream-yellow face of the existing Amber head. Attractive clean hand-painted 2D game sprite, compact clear silhouette, restrained internal fiery texture; no heavy black outer outlines. Eyes remain crisp and exactly aligned in every frame.
 Animation: subtle seamless 4-frame idle flame cycle: frame 1 neutral upright tongue shapes; frame 2 tips lean slightly left; frame 3 tips lift and stretch slightly upward with subtly compressed side lobes; frame 4 tips lean slightly right, ready to return to frame 1. Only flame contours and inner fire highlights change. The lower face, eye centers, torch cup, wrapping and handle remain invariant. Maintain the same front-facing expression, no blinking in this four-frame flame loop. Clearly distinct flame silhouettes but small motions rather than four redesigned characters.
 Background: genuine transparent alpha background, no background color, no fake checkerboard, no vignette, no baked floor shadow. Any glow remains tightly contained in the sprite and does not make a large blurry halo.
 Constraints: exactly 4 frames, 1 row, no extra frames, no separators, no numbers, no labels, no text, no watermark. This is an animation source asset, not a cinematic scene.

# Waterlit Ruins — 물 3프레임

- 내장 image_gen을 프레임당 한 번씩 사용했다.
- 수정 기준: `Assets/image/WaterlitRuins_WaterHazard.png`. 기존 파일은 덮어쓰지 않았다.
- 저장:
  - `Assets/image/WaterlitRuins_WaterHazard_Frame01.png`
  - `Assets/image/WaterlitRuins_WaterHazard_Frame02.png`
  - `Assets/image/WaterlitRuins_WaterHazard_Frame03.png`
- 세 이미지 모두 1672×941, RGB PNG(알파 채널 없음)이다. 모서리/중앙 샘플 알파는 255이다.
- 생성 원본을 그대로 복사했고 각 원본/복사본 SHA-256이 일치한다. 프레임별 파일 해시는 서로 다르다. 이미지 합성·크롭·리사이즈는 하지 않았다.
- 원래 구도와 색감을 유지하면서 잔물결 하이라이트에 작은 위치·길이·밝기 변화를 요청했다. 생성형 편집이므로 변경하지 않은 영역의 픽셀 단위 동일성은 보장하지 않는다.
- 실제 Unity 반복 재생은 아직 검증하지 않았다. 씬, 사망 판정, Animator, Animation Clip, 기존 임포트 설정은 변경하지 않았다.
- 무봉제 가로/세로 반복은 검증하지 않았다.

## 연결 참고

1. 세 PNG를 Sprite (2D and UI), Single로 임포트하고 동일 PPU·Pivot을 맞춘다. Point 필터, Compression None을 사용한다.
2. 순서는 01 → 02 → 03 → 01로 반복한다. 우선 초당 4프레임(장당 0.25초)으로 확인한다.
3. 한 SpriteRenderer의 Sprite만 바꾸고 Transform·Scale·수면 위치는 고정한다.
4. 실제 재생에서 깜빡임이나 원하지 않는 영역 변화가 보이면 해당 프레임의 밝기와 정합성을 추가 조정한다.

## Frame 01 프롬프트

Use case: precise-object-edit
Asset type: ONE full-size animation frame of the attached existing pixel-art water sprite. This is part of a quiet THREE-FRAME looping water animation for a platformer fall pit.

The attached image is the EXACT EDIT TARGET, not loose inspiration.
Lock the canvas to 1672x941. Preserve the same fully opaque rectangular water body, the water surface at the top border, navy/blue/cyan palette, coarse pixel-cluster sizes, camera angle, and broad depth-color bands. The entire image remains opaque edge to edge.
The underlying dark-water shapes and all deep-water color planes must stay stationary. Do NOT translate, zoom, rotate, warp, recolor or redraw the entire texture.
Animate ONLY a small selection of existing thin horizontal cyan/blue ripple highlights, especially in the upper quarter. Changes should be tiny: approximately one native pixel (around 5 output pixels) in highlight position or length, with only subtle local intensity changes. Most pixels should look unchanged.
This is gentle shimmering, not moving the whole sea. Keep average brightness constant so frames do not flash. No large waves, new objects, foam, splashes, bubbles, scenery, sky, rocks, characters or text. No antialiasing, blur or finer texture. No transparent air strip.
Return ONLY ONE image for the specified frame, NOT a sprite sheet, contact sheet, three panels, or animation preview. No frame label or number inside the image.

FRAME 1 / neutral phase: keep the ripple positions almost identical to the edit target. Make just a few upper-quarter reflection strips very slightly shorter and softly brighten their ends by one restrained palette step. This anchors the cycle.

## Frame 02 프롬프트

Use case: precise-object-edit
Asset type: ONE full-size animation frame of the attached existing pixel-art water sprite. This is part of a quiet THREE-FRAME looping water animation for a platformer fall pit.

The attached image is the EXACT EDIT TARGET, not loose inspiration.
Lock the canvas to 1672x941. Preserve the same fully opaque rectangular water body, the water surface at the top border, navy/blue/cyan palette, coarse pixel-cluster sizes, camera angle, and broad depth-color bands. The entire image remains opaque edge to edge.
The underlying dark-water shapes and all deep-water color planes must stay stationary. Do NOT translate, zoom, rotate, warp, recolor or redraw the entire texture.
Animate ONLY a small selection of existing thin horizontal cyan/blue ripple highlights, especially in the upper quarter. Changes should be tiny: approximately one native pixel (around 5 output pixels) in highlight position or length, with only subtle local intensity changes. Most pixels should look unchanged.
This is gentle shimmering, not moving the whole sea. Keep average brightness constant so frames do not flash. No large waves, new objects, foam, splashes, bubbles, scenery, sky, rocks, characters or text. No antialiasing, blur or finer texture. No transparent air strip.
Return ONLY ONE image for the specified frame, NOT a sprite sheet, contact sheet, three panels, or animation preview. No frame label or number inside the image.

FRAME 2 / outward phase: relative to the original edit target, shift a small selection of upper-quarter ripple highlights about FIVE output pixels to the RIGHT and lengthen only their right tips by about five pixels. Leave nearby shorter highlights almost untouched. The change must be very subtle.

## Frame 03 프롬프트

Use case: precise-object-edit
Asset type: ONE full-size animation frame of the attached existing pixel-art water sprite. This is part of a quiet THREE-FRAME looping water animation for a platformer fall pit.

The attached image is the EXACT EDIT TARGET, not loose inspiration.
Lock the canvas to 1672x941. Preserve the same fully opaque rectangular water body, the water surface at the top border, navy/blue/cyan palette, coarse pixel-cluster sizes, camera angle, and broad depth-color bands. The entire image remains opaque edge to edge.
The underlying dark-water shapes and all deep-water color planes must stay stationary. Do NOT translate, zoom, rotate, warp, recolor or redraw the entire texture.
Animate ONLY a small selection of existing thin horizontal cyan/blue ripple highlights, especially in the upper quarter. Changes should be tiny: approximately one native pixel (around 5 output pixels) in highlight position or length, with only subtle local intensity changes. Most pixels should look unchanged.
This is gentle shimmering, not moving the whole sea. Keep average brightness constant so frames do not flash. No large waves, new objects, foam, splashes, bubbles, scenery, sky, rocks, characters or text. No antialiasing, blur or finer texture. No transparent air strip.
Return ONLY ONE image for the specified frame, NOT a sprite sheet, contact sheet, three panels, or animation preview. No frame label or number inside the image.

FRAME 3 / returning phase: relative to the original edit target, shift those upper-quarter ripple highlights only TWO or THREE output pixels to the RIGHT, and reduce their extra tip length halfway back toward neutral. This is an intermediate returning pose so frame 3 transitions gently back into frame 1. Do not invent a new wave pattern.


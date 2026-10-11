# Waterlit Ruins — 낙사 구간의 물

- 내장 image_gen 사용. 기존 물빛 동굴/전경 바위는 색감과 픽셀 질감 참고용이며 변경하지 않았다.
- 최종 저장: `Assets/image/WaterlitRuins_WaterHazard.png`.
- 목적: 바닥 암벽 대신, 불꽃 플레이어가 떨어지면 죽는 구간을 물로 표현하는 정적인 이미지.
- 최종 형태: 위쪽 끝에서 시작하는 밝은 청록 수면과 아래까지 이어지는 짙은 푸른 물. 투명한 공기 띠 대신 직사각형 전체를 물로 채웠다.
- 실제 출력: 1672×941, RGB PNG(알파 채널 없음). 전체 불투명. 네 모서리와 중앙 샘플 알파는 255.
- 생성 결과를 그대로 복사했다. 생성 원본과 복사본 SHA-256이 일치한다. 코드로 픽셀 수정·크롭·리사이즈하지 않았다.
- 기존 GroundFill 파일을 삭제하거나 덮어쓰지 않았다. 씬·프리팹·사망 판정·애니메이션·임포트 설정은 변경하지 않았다.
- 완전 무봉제 X/Y 반복과 엄격하게 균일한 픽셀 격자는 검증하지 않았다.

## 배치 참고

1. PNG를 Sprite (2D and UI), Single로 임포트하고 기존 아트와 동일 PPU를 사용한다. Point 필터, Compression None으로 둔다.
2. 이미지 위쪽 끝을 물의 수면 높이에 맞추고 나머지는 아래로 배치한다. 화면이 가장 아래로 내려가도 이미지의 아래 끝이 드러나지 않을 만큼 덮는다.
3. 실제 사망 구간의 물이라면 수면 위치는 월드의 판정 높이와 맞춰 유지한다. 장식용 패럴렉스로 물만 움직이면 시각과 판정이 어긋날 수 있다.
4. 물에 떨어지는 판정은 별도 사망 트리거로 연결해야 한다. 현재 작업은 PNG 아트 제작만 했으며, 이 이미지 자체에 사망 기능은 없다.
5. 물을 밟는 단단한 바닥처럼 막는 콜라이더를 추가하는 것과 사망 트리거는 구분한다.
6. 우선 Repeat X는 끄고 단일 이미지의 배치를 확인한다.

## 생성 프롬프트

Use case: stylized-concept
Asset type: WATER SURFACE AND DEEP WATER sprite for a lethal fall pit in a side-scrolling pixel-art cavern platformer.

Inputs: Image 1 is ONLY a palette and coarse pixel-style reference for the existing waterlit cave; borrow its blue-teal water colors, not its cave scenery or reflections of structures. Image 2 is ONLY a pixel-size/style reference for the foreground rocks. Neither input should be redrawn or composited into the output.

Primary request: ONE standalone broad rectangular body of cave water, seen STRAIGHT FROM THE SIDE. A clear, nearly level horizontal water surface near the TOP of the image, and deep blue water filling ALL the space below it down to the bottom edge. This is the visible water in a fall/death zone, not solid floor terrain, not a distant lake in perspective.

Canvas: one 16:9 landscape PNG, target 1672x941. Reserve only a very narrow transparent air strip at the top, about 25 pixels high. Place the surface at a consistent height just beneath that strip, across the entire width. Below the surface, the water is solid colored and continuous, with no transparent holes or visible lower shoreline. Genuine transparent alpha ONLY in the top air strip.

Water appearance:
A thin bright cyan/teal pixel surface line with a few tiny shallow stepped ripples, NOT tall waves. Readable flat surface suitable for aligning a gameplay death trigger. Beneath it, simple broad horizontal pixel-cluster bands and sparse short angular ripple highlights, shifting from restrained teal-blue near the surface to deep navy-blue lower down using discrete color steps. Match the reference palette. The water should look cold, deep, and clearly different from dark solid cave rocks.
Keep detail understated so the flame player remains readable. No sparkling glitter, white foam heaps, large splashes, bubbles, or complex caustics.

Pixel style: unmistakable coarse pixel art, as if 320x180 native graphics enlarged with nearest-neighbor. Roughly 5-pixel square clusters, crisp stepped shapes, limited discrete navy/blue/teal/cyan palette, no antialiasing, smooth gradient, blur, photo-realism or painterly brush texture.

Composition and continuity: flat front view, no vanishing point or top-down perspective. The surface meets both left and right edges at the SAME height, and the edge water colors are kept simple and compatible for adjacent placement. Draw only one image, not a tiled preview or sprite sheet.

Avoid completely: land, rocks, ground, islands, bottom border, shoreline, cave ceiling, stalactites, ruins, buildings, reflected mountains or pillars, sky, trees, lamps, fire, characters, text, numbers, watermark, checkerboard or colored background in the transparent top strip.
Only the narrow clear air strip, a nearly flat cyan surface, and blue water extending all the way to the bottom.

## 수면 위 점 정리 프롬프트

Use case: precise-object-edit
Edit the attached water sprite. Make ONE correction only: REMOVE the disconnected bright cyan blobs and stray colored specks floating in the transparent air ABOVE the water surface, especially the clipped blobs near x=290–355 at the top and the tiny speck near x=1088 above the surface. Replace those marks with genuine transparent alpha.

Keep the entire water body, surface height, shallow connected wave crests, blue/cyan/navy colors, horizontal ripple texture, coarse pixel-art style, composition, dimensions and lower rectangle unchanged. Do not redesign or recolor the water. Do not add splashes, bubbles, sky, scenery, rocks, or any new object.
All of the uppermost 40 rows must be fully transparent and clear. Below those rows, retain the existing connected surface ripples and all water. The air above the surface has NO disconnected particles.
Output a single genuine transparent PNG with the same 1672x941 canvas.

## 최종 직사각형 불투명 물 프롬프트

Use case: precise-object-edit
Make this water sprite a FULLY OPAQUE flat rectangular water asset, for filling a platformer death pit.

Change ONLY its narrow top region: replace ALL transparent air above the current water surface with water. Bring the cyan horizontal surface line UP to the very TOP EDGE of the canvas. There must be NO AIR STRIP and NO transparent pixel anywhere. The water now begins at the top border and fills the entire rectangle. Keep the top surface straight and restrained, not tall waves. Remove floating marks by incorporating this entire region into continuous opaque water.

Preserve the existing blue water texture, pixel-art scale, palette and shading in the large body BELOW the original surface. Keep the 1672x941 size. No rocks, scenery, sky, props, text, border, splashes or bubbles. No smooth blur or gradients.
Output one RGB-style opaque rectangle of water, edge to edge and corner to corner, with the readable cyan water surface aligned to the TOP border.


# Waterlit Ruins — 반복 대응 v2 (1차 제작)
 
## 파일과 역할

- `Assets/image/WaterlitRuins_01_Atmosphere_v2.png`: 화면 채움용 불투명 빛·공기층. 1672×941, RGB(알파 없음). 수면·천장은 별도 장식으로 분리하는 방향.
- `Assets/image/WaterlitRuins_02_CavernRepeatX_v2.png`: 좌우 반복용 투명 동굴 실루엣. 1672×941.
- `ArtReference/WaterlitRuins_Repeat_Check_v2.html`: 원본 PNG를 그대로 표시하는 반복 확인 페이지. PNG 픽셀 수정이나 재출력 없음.
- 기존 01–04 파일, 임포트 설정, 씬, 프리팹, 런타임 스크립트는 변경하지 않았다.

## 검증

01은 Format24bppRgb로 저장되어 전체가 불투명하다. 네 모서리와 중앙 샘플 알파는 255.
02는 모든 행에서 좌측 24열과 우측 24열을 검사했다:
- 비투명 픽셀: 좌측 0 / 우측 0 (각 24×941 = 22,584개 검사).
- 양쪽 최외곽 열의 알파 불일치: 0행.
- 경계 띠의 최대 알파: 0.
- 상단·하단 빈 공간 샘플 알파 0, 내부 암벽 샘플 알파 254.

경계의 보이는 구조물을 이어 붙이는 대신, 양끝을 완전히 투명한 공기로 끝내는 설계다. 같은 이미지를 반복할 때 경계에서 잘린 바위나 갑작스러운 색·알파 변화가 발생하지 않는다. 투명 여백으로 생기는 반복 골짜기는 의도된 구성이다.
단, 엄격한 동일 픽셀 격자, 상하 반복, 게임 안에서의 미적 자연스러움은 검증하지 않았다.
브라우저 도구에서 로컬 HTML 접근이 보안 정책으로 거부되어 확인 페이지의 실행 검증은 못 했다. 원본 PNG 3장을 반복 표시하는 별도의 대화 미리보기도 준비했다.

## 현재 적용 시 주의

- 01을 기존 배경 스프라이트 대신 사용한다면 `Parallax Layer`의 Follow X/Y를 모두 1로 설정하고, 카메라 중심과 맞춰 가장 큰 줌아웃 화면보다 넉넉하게 키운다. 이미지 파일만 교체해도 자동 화면 맞춤이 생기는 것은 아니다.
- 02는 X 반복 전용. Y로 그대로 반복하지 않는다. 01이 뒤에서 화면을 채우므로 02 밖의 투명 공간은 01을 보여준다.
- `ParallaxLayer.cs`에 `Repeat X` 옵션을 추가했다. 02에서 켜면 자동 복사본 배치·재배치를 사용한다. 상세 설정은 `Docs/Parallax_Setup.md` 참고.
- 네 배경의 PPU/스케일을 통일하고 Point 필터, Compression None을 권장한다. 이 설정은 실제로 수정하지 않았다.
- 남은 아트 작업은 03 유적 레이어의 좌우 반복 대응과 04 개별 장식 제작이다. 01 자동 화면 크기 맞춤과 사용자 씬에서의 이동/줌/리스폰 화면 검증은 별도 작업이다.

## 생성 방식

내장 image_gen 사용. 기존 이미지는 참고용이며 덮어쓰지 않았다.

### 01 최종 프롬프트

Use case: stylized-concept
Asset type: Layer 01 opaque screen-filling atmosphere for a pixel-art waterlit cave platformer.
Input image: palette and coarse pixel-art style reference ONLY. Generate a new separate layer, do not copy the reference scene.
Primary request: a quiet blue-teal cavern atmosphere made entirely from a few broad pixel-cluster light bands and deep atmospheric color planes. It sits behind all rock silhouettes and ruins and stays attached to the view.
Canvas: one 16:9 landscape image, target 1672x941. Entire canvas fully opaque, edge to edge, including every corner.
Composition: large understated vertical blue light shafts passing through cool deep air, softly suggested ONLY with discrete stair-stepped pixel clusters and flat color changes. Dim navy outer areas, medium teal middle areas, modest desaturated cyan light. Keep contrast restrained and every region suitable behind a flame player. No obvious central focal object. The image should look atmospheric, not like a patterned tile wallpaper.
Style: clear coarse pixel art matching the reference, as if a 320x180 game image enlarged with nearest-neighbor. About 5-pixel square clusters, eight restrained navy/blue/teal colors, crisp stepped outlines. NO smooth gradient or airbrush, no antialiasing, no blur, no fine grain, no high-resolution detail.
Draw atmosphere and light ONLY. Remove all recognizable cave ceiling, stalactites, rocks, mountains, pillars, arches, ground, waterline, reflections and foreground framing. These will be separate layers.
No transparent regions, no border, black bars, characters, props, text, numbers, checkerboard, logo or watermark.
A single background asset, not a contact sheet.

### 02 최종 프롬프트

Use case: stylized-concept
Asset type: Layer 02 horizontally repeating transparent distant-cavern silhouette for a side-scrolling pixel-art game.
Input image: style and cool color reference ONLY, not an edit target. This new transparent overlay will be drawn on top of the reference atmosphere.
Canvas: landscape 1672x941, genuine transparent alpha. One single tile, not a repeat preview or contact sheet.
Subject: a sparse row of distant blue-teal natural cave rock spires and chunky worn rock columns rising from the lower portion. Various heights and widths, a few narrow gaps and broken tops; no masonry, arches or ruins yet. Quiet middle-distance, lower contrast and lighter than foreground rocks. Use cool muted medium navy and teal, about #21495d, #2b5b70, #376d80. Give rocks restrained pixel-facet shading.
Composition: all spires and rock bases are contained inside the canvas. Upper 25% and lower 12% are completely transparent. Crucially, the LEFTMOST and RIGHTMOST 24 columns of pixels are also completely transparent from top to bottom. Every rock ends naturally inside those margins; taper groups into open cave air at both sides. Do NOT cut a structure at either lateral image edge. Keep the side gaps modest, without giant empty side padding. Center also has open gaps. In horizontal repetition the same clear air continues across the wrap seam, revealing the opaque atmosphere behind it. Avoid strong repeated central landmarks or evenly spaced identical spires.
Render the spires as a mid-distance silhouette band behind terrain, NOT playable platforms or floating individual collectible objects.
Pixel-art style is essential: coarse crisp square clusters matching the reference, as if a native 320x180 image enlarged with nearest-neighbor. Stepped angular rock outlines, simple flat pixel shading, limited palette. No antialiased edge haze, no soft gradients, no blur, no fine surface noise.
Background: genuine transparent alpha around and between all rocks, including the arch-free negative spaces and the reserved 24-pixel left and right margins. No colored background, fog or glow clouds.
No ceiling, stalactites, full-width floor, water, reflections, orange fire, lamps, ruins, vegetation, characters, text, numbers, border, checkerboard or watermark.

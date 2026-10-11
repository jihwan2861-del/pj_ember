# 물빛 유적 — 02 먼 유적

- 방식: 내장 image_gen.
- 이미지 위치: Assets/image/WaterlitRuins_02_DistantRuins.png
- 01 배경의 복사본 위치: Assets/image/WaterlitRuins_01_FarCavern.png
- 이전 Assets/Backgrounds/WaterlitRuins의 원본은 유지했다.
- 두 이미지의 캔버스는 1672×941. 02는 먼 기둥·아치·무너진 벽의 투명 오버레이다.
- 위쪽과 아래쪽 빈 공간, 아치 내부는 투명 배경으로 생성했다.
- Unity에서는 두 Sprite를 같은 중심과 월드 크기로 겹치고, 02가 01보다 앞에 보이도록 Sorting Order를 설정한다.
- 선명한 픽셀 표현을 위해 Filter Mode: Point, Compression: None을 사용한다.
- 이 레이어는 생성 PNG이며 엄격한 공통 픽셀 격자 및 좌우 반복 이음새는 검증하지 않았다.
- 씬, 프리팹, Import 설정과 패럴렉스 코드는 수정하지 않았다.
- 다음 레이어는 벽돌 형태가 더 잘 보이는 가까운 유적이다.

## 생성 프롬프트

```text
Use case: stylized-concept
Asset type: Layer 02, distant ruins silhouettes on transparent alpha, for a 2D pixel-art cavern parallax background.
Image 1 is the existing backmost cavern plate: palette, pixel size, aspect ratio and placement reference only.
Create ONLY the next overlay layer, on transparent background; do not reproduce the filled cavern plate.

OUTPUT COMPOSITION:
One landscape image matching the reference's approximately 16:9 aspect, aim for exactly 1672 by 941 pixels.
A broad horizontal group of distant ancient ruined arches, aqueduct fragments, broken walls and simple square stone supports.
Align their bases around 80–88% of the image height, behind the gameplay space. Structures vary between approximately 30% and 65% of image height; no central monumental focal point. Keep many wide open gaps between structures and inside the arches so the lit cavern shows through.
The upper quarter of the image stays empty and transparent. The central gameplay space is airy and readable.
Use modest slender ruined structures, mostly distant silhouettes with only a few chunky masonry hints. No ornate Greek columns.
The layer should be visible against the bright cyan cave but quieter and lighter than a future dark close-up masonry layer. Muted blue-teal palette approximately #27566c, #32677a, #3d7987; simple 2–3 tone shading, no black silhouette, no warm highlights.

PIXEL STYLE:
Match the reference's obvious coarse square pixels and stair-step shapes. Imagine a native 320 by 180 pixel side-view background enlarged with nearest-neighbor: visible approximately 5-pixel blocks, consistent square pixel grid, deliberate clean clustered shapes, limited palette.
Arch curves must be crisp stepped contours. Flat discreet colors. Keep fine masonry detail sparse because these ruins are far away. No blur, antialiasing, smooth gradient, painterly shading, photographic texture, fine-resolution outlines or soft atmospheric glow.

TRANSPARENCY IS REQUIRED:
All sky, cave air, spaces between ruins, arch openings and the entire area under the ruins are actual transparent alpha. Draw solid blue-teal masonry silhouettes only.
No opaque rectangular backdrop, colored fog filling the empty space, checkerboard pattern or background scenery.
No cave ceiling, stalactites, rocks, floor, water, reflections, waterfalls, lanterns, torches, fire, characters, UI, text, labels, borders or watermark.
This is ONE distant architecture overlay, not a finished composed scene or a collection of separate sprites.
Keep left and right margins mostly transparent and avoid cropping a prominent structure at either edge.
```


# Waterlit Ruins — 03 가까운 유적

- 생성 방식: 내장 이미지 생성.
- 이미지: `Assets/image/WaterlitRuins_03_NearRuins.png`.
- Layer02보다 짙고 선명한 벽돌 유적. 중앙과 아치 내부는 투명하게 비워두는 배경 레이어.
- Unity 배치 시 다른 배경과 동일한 중심·월드 크기로 맞추고 Point 필터와 Compression None을 권장한다. 실제 임포트 설정이나 씬은 변경하지 않았다.
- 정확한 픽셀 격자 일치 및 좌우 반복 이음새는 검증하지 않았다.
- 다음 레이어: 04 앞쪽 바위·종유석.

## 생성 프롬프트

Use case: stylized-concept
Asset type: Layer 03, near ruined masonry overlay for a side-scrolling pixel-art waterlit cavern parallax set.
Reference: attached image is Layer 02, the distant blue-teal ruins. Use its palette family, side view, approximate coarse pixel scale and canvas placement; generate NEW closer architecture, darker and more detailed, on genuine transparent alpha.
Canvas: one landscape image, exactly 1672 by 941 pixels, same as reference.

Composition: ruined square-stone walls and large broken arches concentrated in the lower left and lower right thirds, with a few short broken supports along the bottom. One incomplete broad arch on one side and a crumbling wall fragment on the other. Bases approximately 85% of canvas height. Heights vary from 25% to 65%. Leave the middle third largely transparent, and preserve generous transparent gaps through every arch and between structures. Keep the upper quarter transparent. This overlay sits behind gameplay, not a solid playable terrain strip.
Near masonry has clearly readable chunky rectangular blocks, sparse chipped corners, jagged broken tops, and a few dark cracks. Keep the architecture simple and weathered. No ornate Greek columns, cathedral ornament or intricate carving.
Use darker desaturated navy-teal stone approximately #112b3b, #1b4051 and #285569, with restrained cool pixel highlights. Closer and more defined than Layer 02, while leaving the far bright cavern visible through openings. No orange lamps or fire in this layer.

Pixel art is the highest priority: obvious large square pixel clusters like a native 320 by 180 game background enlarged with nearest-neighbor. Crisp stair-step contours and arch curves, consistent approximately 5-pixel blocks, flat discrete color patches, limited palette. Brick units are built from several logical pixels, not tiny high-resolution texture. No blur, antialiasing, smooth gradients, painterly shading, soft glow, fine photo-like detail or noisy surface texture.

Draw masonry ONLY. All surrounding air, central empty space, arch holes and spaces below ruins must have actual transparent alpha.
No colored rectangular backdrop, sky, cavern ceiling, stalactites, natural rocks, floor, water or reflection, fog, lanterns, characters, UI, text, numbers, borders, checkerboard pattern or watermark.
Exactly one full-width coherent parallax overlay, no contact sheet. Keep structural fragments inside the canvas with a little lateral clearance.

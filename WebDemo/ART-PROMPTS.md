# Ember — 생성 이미지 기록

2026-10-03. 두 이미지 모두 내장 image_gen으로 생성했다. 스프라이트 시트는 실제 RGBA 투명 배경이며 게임에서 원본의 4×4 셀을 잘라 애니메이션으로 사용한다. 원본 픽셀은 변형하지 않았다.

## 예시 장면

저장: `assets/ember-scene.png`. 플레이 경로의 분위기를 보여주는 콘셉트 이미지이며 충돌 배치도가 아니다.

```text
Use case: stylized-concept. Asset type: one polished example screenshot-style illustration for the game EMBER, an original 2D precision platformer.
Wide 16:9 landscape game scene: an abandoned dark stone conservatory, huge graceful broken arches, layered teal-blue mist and distant silhouettes of a forgotten garden. Readable side-on 2D composition, warm amber fire contrasting with deep midnight blue. A tiny adorable flame spirit with pale golden body, orange flame-shaped head, two dark oval eyes, short arms and pointed feet is jumping from a stone platform toward a floating torch. A bright curved trail suggests ignition, absorption into a flame and a diagonal launch. Three substantial moss-covered stone ledges and hanging small fire braziers form an understandable traversal path above a shadowy abyss. A shadow pursuer with two pale eyes lurks at the far right behind an arch. One resting flower on the final ledge is waking with a warm glow. Hand-painted 2D indie game art, restrained rich detail, strong clear foreground silhouettes, lovely cinematic atmosphere. Main gameplay objects remain small relative to the scene. No HUD, no writing, no lettering, no logo, no watermark, no photorealism. A single complete atmospheric example image, not a collage.
```

## 플레이어·횃불 스프라이트 시트

저장: `assets/ember-atlas.png`. 4열×4행, 16프레임. 생성 결과는 1254×1254 PNG이며 셀 크기는 원본 너비·높이의 1/4이다. Canvas의 소수점 소스 좌표로 직접 샘플링한다.

- 첫 행: 대기 4프레임.
- 둘째 행: 달리기 4프레임.
- 셋째 행: 상승, 낙하, 대시, 불 내부 형태.
- 넷째 행: 꺼진 횃불, 켜진 횃불 3프레임.

```text
Use case: stylized-concept. Asset type: game-ready animated character and torch sprite sheet for the same original 2D platformer, EMBER.
Make a strict 4 COLUMN by 4 ROW atlas on a true transparent background, square canvas, ideally 1024 x 1024. Sixteen equal square cells, exactly equally spaced, no visible grid, no labels, no text, no checkerboard painted into pixels. Each sprite centered horizontally in its own cell, common ground baseline at 80% cell height. Each sprite fits within the center 60% of its cell without any cropped pieces or touching the neighboring cells.
Subject: tiny friendly fire spirit, pale cream-yellow luminous body, orange flame-shaped large head, two simple dark brown oval eyes, little arms and pointed feet. No clothing, no weapon. Clean hand-painted 2D game cutout shapes, vivid golden-orange warm colors, subtle cel shading, crisp silhouette, controlled short edge glow, no large halo. Consistent character size and proportions.
Row 1, left to right: four idle animation frames, front three-quarter view facing right, flame flickers and body bobs very subtly.
Row 2: four distinct running cycle frames facing right, visibly alternating foot and arm positions, same consistent baseline.
Row 3: jumping upward pose; falling downward pose; horizontal dash to the right pose with stretched flame tail contained within cell; compact floating flame orb with two eyes for absorbed state.
Row 4: unlit small dark stone torch brazier on a short pedestal; same torch with small lit amber flame frame 1; same lit torch flame frame 2; same lit torch flame frame 3. Torch sprites same height as character and same baseline.
All sixteen cells entirely isolated on actual transparent alpha. Only the requested 12 character frames and 4 torch frames. Do not add shadows on the ground. Do not add background or decorative borders.
```


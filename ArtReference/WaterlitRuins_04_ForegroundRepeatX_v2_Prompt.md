# Waterlit Ruins — 04 좌우 반복 전경 바위 v2

## 결과

- 내장 image_gen으로 기존 04를 참고해 전경 레이어만 새로 제작했다.
- 저장: `Assets/image/WaterlitRuins_04_ForegroundRepeatX_v2.png`.
- 실제 크기: 1671×941, RGBA PNG. 생성 도구 출력 그대로 복사했으며 픽셀 후처리나 리사이즈는 하지 않았다.
- 기존 04의 높은 좌우 벽과 천장 대신, 하단의 낮은 바위 덩어리와 넓은 투명 공간으로 구성했다.
- 기존 04 파일, 01–03 이미지, 씬, 프리팹, 스크립트와 기존 임포트 설정은 변경하지 않았다.

## 확인

- 좌측 32열과 우측 32열을 전체 높이에서 검사했다. 각 30,112픽셀 중 알파가 0보다 큰 픽셀은 모두 0개.
- 경계 영역 최대 알파: 0. 화면 중앙 알파: 0.
- 원본 생성 파일과 프로젝트에 복사한 파일의 SHA-256이 일치한다.
- X 방향에서는 투명한 공간끼리 맞닿는다. 바위 면이 연속으로 이어지는 타일은 아니다.
- 엄격하게 균일한 픽셀 격자나 Y 반복은 검증하지 않았다. 사용자 씬에서의 실제 가림 정도와 반복 패턴은 Play에서 확인해야 한다.

## Unity에서 교체

1. 새 PNG를 `Sprite (2D and UI)`, `Sprite Mode = Single`로 임포트한다. 전체 투명 여백을 보존하고, 바위별로 자동 슬라이스하지 않는다.
2. Pixels Per Unit은 기존 04와 같게 맞춘다. 기존 04 메타 파일의 값은 100이다. Filter Mode는 Point, Compression은 None으로 둔다.
3. 기존 04 배경 오브젝트의 Sprite Renderer에서 Sprite만 새 이미지로 교체한다. Draw Mode는 Simple로 둔다.
4. 기존 `Parallax Layer`에서 `Repeat X`를 켠다. 카메라 연결과 Follow 값은 우선 유지한다. 수동 복제는 하지 않는다.
5. 바위가 플레이어를 가리면 원본 04 오브젝트의 Y 위치/크기를 조절한다. 콜라이더는 추가하지 않는다.

## 입력 이미지 역할

1. `Assets/image/WaterlitRuins_04_ForegroundRocks.png`: 수정 대상의 디자인 참고.
2. `Assets/image/WaterlitRuins_02_CavernRepeatX_v2.png`: 동굴 색감과 픽셀 크기 참고만 사용.
3. `Assets/image/WaterlitRuins_03_NearRuins.png`: 가까운 유적과의 색감 통일 참고만 사용.

## 최종 프롬프트

Use case: precise-object-edit
Asset type: revised Layer 04, a horizontally repeatable transparent foreground-rock overlay for a side-scrolling pixel-art cavern platformer.

INPUT ROLES:
Image 1 is the old Layer 04 EDIT TARGET. Retain its natural angular-rock identity, very dark navy/teal palette, and coarse pixel-art language, but REPLACE its tall left/right enclosing walls and upper-corner stalactites with a much lighter, sparser lower-edge composition.
Image 2 is a STYLE REFERENCE ONLY for the existing distant cave layer. Do not include that layer in the output.
Image 3 is a STYLE REFERENCE ONLY for the existing nearer ruined arches. Do not include arches, bricks, pillars, or any part of that image in the output.
Only deliver the new standalone Layer 04.

CANVAS:
One wide landscape PNG, target 1672 by 941 pixels, same 16:9 proportions as the inputs. Genuine transparent alpha background. One single repeat tile, NOT a scene, sprite atlas, or repetition preview.

COMPOSITION:
Several unequal, isolated groups of squat angular cave boulders in the LOWER 22% of the canvas. Each group contains just a few large, readable rocks. Heights vary naturally, and the groups are separated by generous clear air. Keep the upper 75% of the canvas completely transparent and the central gameplay view unobstructed. Avoid a prominent central landmark or mechanically equal group spacing.
No full-width solid floor, no horizontal base connecting the groups, no ceiling, no overhangs, no tall side walls, and no closed vignette frame. Every group ends naturally within the canvas.

REPEAT SEAM:
The LEFTMOST and RIGHTMOST 32 columns MUST be completely transparent from top to bottom. Do not cut any rock off at either lateral edge. Both sides transition to transparent air so copies can repeat horizontally without a cut wall or abrupt dark seam. Do not mirror the rocks at the seam.

STYLE:
Unmistakable COARSE PIXEL ART, like approximately 320x180 native graphics enlarged with nearest-neighbor. Crisp, large square color clusters around 5 pixels across; stepped polygonal silhouettes; simple broad faceted shading; only about five dark discrete colors. Match the scale of the pixel clusters in the inputs.
This is the closest and darkest layer: near-black navy shadows around #07121d, restrained dark blue around #102332, and sparse dull teal facets around #1c3b47. No luminous cyan highlights. Rocks should feel solid and opaque, not ghostly or translucent.

TRANSPARENCY AND AVOID:
All air between and above the boulders must be truly transparent, including the reserved side margins. No colored background, fake checkerboard, gray transparency preview baked into the PNG, soft fringe, diffuse glow, blur, antialiasing, smooth gradient, watercolor, fine texture noise, water, reflections, fog, foliage, lamps, fire, characters, playable platforms, labels, borders, or watermark.
Preserve the existing Layers 01–03 by NOT incorporating or redrawing them. Change Layer 04 only.


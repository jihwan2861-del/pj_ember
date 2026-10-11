# 천장 채움 — 기존 전경 윗면 연결 대응

## 파일

- 최종 이미지: `Assets/image/WaterlitRuins_04_CeilingFill_Joined_v3.png`.
- 중간 시안: `Assets/image/WaterlitRuins_04_CeilingFill_Joined_v2.png`.
- 내장 image_gen으로 기존 CeilingFill의 아래쪽 밝은 바위 무늬를 줄이고, ForegroundRocks의 어두운 윗면에 가까운 색으로 조정했다.
- 기존 ForegroundRocks와 CeilingFill 파일, 씬, 프리팹, 스크립트, 기존 메타 설정은 변경하지 않았다.
- 최종 출력은 1671×941 RGB PNG이며 전체 불투명하다. 원본 기준 1672×941보다 너비가 1픽셀 작다. 생성 PNG를 그대로 복사했고 코드로 픽셀 수정·리사이즈하지 않았다.

## 배치 조건

1. 기존 평평한 천장 채움 Sprite만 v3 이미지로 교체한다. 원본 전경 바위는 그대로 유지한다.
2. Sprite (2D and UI), Single / Point / Compression None을 사용한다. PPU는 원본 전경과 동일하게 맞춘다.
3. 천장 채움은 원본 전경 바로 위에, 원본 전경보다 낮은 Sorting Order로 배치한다.
4. 원본과 중심 X 및 표시 가로폭을 같게 한다. 같은 PPU라면 천장 Scale X를 원본 Scale X × 1672/1671(약 1.000598배)로 잡으면 한 픽셀 너비 차이를 보정할 수 있다. Scale Y는 같게 유지한다.
5. 천장 아래쪽과 전경 윗부분을 약 4–8픽셀 정도 겹쳐 경계 틈을 막는다. PPU=100, Scale Y=1이면 0.04–0.08 월드 유닛에 해당한다.
6. 중앙 Pivot, PPU=100, Scale Y=1인 두 이미지라면 중심 Y 차이를 약 9.33–9.37로 두면 이 겹침이 생긴다. 실제 스케일과 위치에 맞춰 조정한다.
7. 각각 패럴렉스를 사용한다면 Follow X/Y를 동일하게 한다. 부모 이동에 이미 종속된 자식에 패럴렉스를 중복 적용하지 않는다.
8. 이 이미지는 세로 연결용 천장 채움이다. 완전 무봉제 X/Y 반복용으로 검증하지 않았으므로 Repeat X는 우선 끈다.

## 확인과 한계

- 원본 전경의 첫 행에서 알파가 240보다 큰 부분은 814열(왼쪽 0..380, 오른쪽 1239..1671)이다. 대표 어두운 색은 RGB(1,15,27), #010F1B.
- 이 영역과 천장 마지막 행의 RGB 채널별 평균 절대 차이는 기존 CeilingFill 4.421 → 최종 v3 4.016(0–255 기준)이다. v3는 같은 표시 폭으로 놓는 조건을 가정해 열 좌표를 대응시켰다.
- 원본과 가장자리 픽셀이 완전히 동일한 결과는 아니다. 최종 최대 채널 차이는 19이며 실제 씬의 위치·겹침·스케일·머티리얼·조명에 따라 경계가 남을 수 있다.
- 생성 파일과 프로젝트 복사본의 SHA-256은 일치한다. 사용자 씬에서의 이동/점프 화면 검증은 하지 않았다.

## 1차 연결부 수정 프롬프트

Use case: precise-object-edit
Asset type: seam-matched opaque ceiling-fill rectangle for a pixel-art cave platformer.

INPUT ROLES:
Image 1 is the UNCHANGED foreground-rock frame that the new ceiling must join above. It is the seam color and pixel-style reference, not an edit target.
Image 2 is the ceiling-fill EDIT TARGET. Output a corrected version of Image 2 ONLY.

PLACEMENT AND JOIN:
Both images are 1672x941. The ceiling rectangle sits directly ABOVE Image 1 at the same width and scale: its BOTTOM edge meets the TOP edge of Image 1.
Image 1's opaque upper-edge rocks lie approximately at x=0..380 and x=1239..1671. These upper-edge rocks are extremely dark navy, predominantly RGB (1,15,27), #010F1B, with only tiny variations. The middle of Image 1 remains transparent and must not be reproduced in this fully opaque ceiling fill.

EDIT:
Keep the upper approximately 70% of Image 2's coarse pixel-rock material and overall dark palette.
Rework its LOWER 30% to connect naturally into Image 1's nearly black upper rock faces. Pull back the densely packed bright boulder facets from the join. Finish the lower edge in a broad, quiet near-black navy shadow zone, especially over x=0..380 and x=1239..1671.
CRITICAL: the LAST 64 PIXEL ROWS at the BOTTOM of the output should be almost entirely flat #010F1B, without bright teal polygons, cut-off boulders, edge highlights, outlines, cracks, decorative stripes, or noise. This dark landing zone must visually match Image 1's top rock colors. Use that same quiet dark navy across the middle as well.
Transition the remaining rock facets into the shadow zone with a few large discrete stepped dark shapes higher up, NOT a smooth airbrushed gradient and NOT a visible horizontal border line. No individual illuminated facet should be interrupted exactly at the bottom join.

INVARIANTS:
The output stays a FLAT FULLY OPAQUE RECTANGLE, edge to edge and corner to corner, 1672x941. Do not create a hanging ceiling silhouette, stalactites, an opening, a transparent middle, side walls, floor, or the complete frame from Image 1.
Retain the coarse ~5-pixel square clusters and simple limited navy/blue-teal palette. No antialiasing, blur, fine grain, smooth gradients, painted texture, added props, text or watermark.
Do not return a combined two-image scene, preview, atlas, or stacked reference. Deliver the corrected ceiling-fill image alone.

## 최종 아래쪽 색 조정 프롬프트

Use case: precise-object-edit
Image 1 is the ceiling-fill edit target. Image 2 is the unchanged foreground-rock seam reference.
Make one color correction ONLY to the bottom shadow zone of Image 1. Its current bottom pixels are roughly RGB (1,20,36), which is TOO BRIGHT and TOO BLUE compared with Image 2's top dark rock pixels, predominantly RGB (1,15,27), HEX #010F1B.

Darken the lowest 160 rows of Image 1 to the reference's exact quiet #010F1B navy. The bottommost 64 rows must be a solid flat #010F1B color field: no mottling, lighting, grain, highlights, texture or variation. Do not use cyan-blue #011424 or #001525. The intended join is almost black navy #010F1B, RGB (1,15,27).
Above that band, use a few simple flat dark pixel color steps to integrate the existing facets without a visible stripe or a smooth gradient.
Keep all upper rock shapes, coarse pixel-art style, width and height unchanged. Keep the entire 1672x941 rectangle fully opaque. No transparency, new objects, stalactites, frame, labels or borders. Output only the corrected ceiling rectangle.


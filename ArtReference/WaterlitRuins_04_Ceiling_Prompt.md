# Waterlit Ruins — 기존 04와 맞춘 천장

- 내장 image_gen 사용. 기존 `WaterlitRuins_04_ForegroundRocks.png`는 스타일 참고로만 사용했고 변경하지 않았다.
- 저장: `Assets/image/WaterlitRuins_04_Ceiling.png`.
- 실제 출력: 1672×941, RGBA PNG. 생성 파일 그대로 복사했으며 픽셀 수정·리사이즈는 하지 않았다.
- 기존 04의 어두운 남색·청록색과 큰 픽셀 면을 참고한 연속 천장 및 짧은 종유석. 아래 공간은 투명하다.
- 씬, 프리팹, 런타임 코드, 기존 이미지와 기존 임포트 설정은 수정하지 않았다.

## 배치

1. 새 PNG를 Sprite (2D and UI), Single로 임포트한다. Point 필터와 Compression None을 사용한다.
2. 새 천장 SpriteRenderer를 만들고 기존 04와 같은 PPU·Scale·초기 위치로 맞춘다. 전체 캔버스 크기가 같으므로 윗부분을 겹쳐 배치할 수 있다.
3. 천장의 Sorting Order를 기존 04보다 조금 낮게 두면 기존 모서리 바위가 천장의 양끝을 덮는다.
4. 패럴렉스를 사용할 경우 기존 04와 같은 Follow X/Y를 사용해 두 레이어가 서로 벌어지지 않게 한다.
5. 우선 Repeat X는 끈다. 이 결과는 기존 04와 함께 겹쳐 사용하는 단일 천장 이미지이며, 픽셀 단위로 완전 일치하는 무봉제 반복 타일은 아니다.

## 확인

- 이미지 중앙과 하단 중앙의 알파는 0이다.
- 생성 원본과 프로젝트 복사본의 SHA-256이 일치한다.
- 최외곽 좌우 열의 알파가 다른 행은 29개, 양쪽이 보이는 행 중 RGB가 다른 행은 152개, RGB 채널 최대 차이는 43이다. 따라서 완전 무봉제 X 반복으로 검증되지 않았다.
- 가장자리 마지막 비투명 행은 왼쪽 168, 오른쪽 169이다(위에서 시작하는 픽셀 좌표).
- 사용자 씬의 실제 가림 정도와 엄격하게 균일한 픽셀 격자는 별도 확인이 필요하다.

## 최종 프롬프트

Use case: stylized-concept
Asset type: a standalone foreground cave CEILING layer that matches the attached existing foreground-rock overlay for a side-scrolling pixel-art game.

Input image: STYLE AND PALETTE REFERENCE ONLY. Keep the existing reference image unchanged. Make ONE NEW, SEPARATE ceiling-only PNG, not a composite or a copy of the full enclosing rock frame.

Canvas: wide landscape, target 1672x941 pixels, same 16:9 proportions and comparable pixel-cluster size as the reference. Genuine transparent alpha background.

Subject and composition:
A continuous, thick natural cave rock ceiling attached to the TOP edge and spanning the full canvas width. A rugged underside with large angular rock planes and a few short, thick downward stalactites, matching the upper-corner rocks in the reference. Vary the underside naturally; alternate shallow recesses and chunky hanging teeth. Keep most of the ceiling within the upper 15–20% of the canvas, with only a few tips extending to approximately 28%. Everything below the ceiling is transparent. Leave the entire lower 70% empty so gameplay remains visible.
No side walls, no left/right enclosing frame, no floor or bottom rocks. The top of the ceiling fills the top edge, not a floating isolated island. This asset will overlap the existing upper-corner rocks in the game.

Horizontal repeat intent:
Design a repeatable ceiling strip. Both lateral edges must meet at the SAME underside height and with matching dark rock colors. Keep the outermost approximately 32-pixel strips at both sides very simple: identical flat near-black navy upper rock and matching shallow stepped underside profiles. No stalactite, crack, bright highlight, isolated rock, or diagonal plane crossing either lateral cut. Put the varied facets and hanging shapes farther inside the tile. Do not create transparent side margins or gaps in the ceiling. Do not mirror the entire composition. Output only one tile, not a tiled preview.

Style and invariants:
Match the reference's unmistakable COARSE PIXEL ART: crisp approximately 5-pixel square color clusters, chunky stair-step outlines, broad blocky rock facets, limited dark navy and blue-teal colors. Near-black navy around #07121d, deep blue around #102332, restrained dull teal facets around #1c3b47. Match the reference closely instead of introducing brighter cyan or a new rendering style. Rock should feel heavy, solid and close to the viewer. Keep the dark shading and sparse faceted highlights; no fine stipple or thin detailed cracks.

Background and avoid:
Genuine transparent alpha everywhere beneath the irregular ceiling silhouette; solid rock above it. No opaque background filling the empty area, no fake checkerboard, gray canvas, haze, glow, smooth gradients, blur, antialiasing, painterly brush texture, tiny texture noise, water, ruins, bricks, foliage, lamps, fire, characters, text, border, labels, or watermark.
Do not redraw, replace, or include the lower rocks and side walls from the reference. Deliver the ceiling alone.


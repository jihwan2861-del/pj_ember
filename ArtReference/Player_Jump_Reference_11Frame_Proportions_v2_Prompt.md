# 점프 11프레임 — 원본 비율 보정 v2

- 방식: 내장 image_gen 이미지 편집.
- 요청: 기존 점프 자세는 유지하고 원래 캐릭터에 가깝게 머리·몸통·팔다리 비율만 소폭 수정.
- 결과: `Assets/player/new_Player/Player_Jump_Reference_11Frame_Sheet_Proportions_v2.png`
- 캔버스: 1448×1086, 4열×3행, 셀당 362×362.
- 프레임: 왼쪽에서 오른쪽, 위에서 아래로 1–11 사용. 마지막 12번 칸 제외.
- 이전 점프 시트와 원본 캐릭터는 덮어쓰지 않음. Animator·프리팹·씬은 변경하지 않음.

## 입력 이미지

1. 편집 대상: `Assets/player/new_Player/Player_Jump_Reference_11Frame_Sheet.png`
2. 3/4 시점·원래 비율 기준: `ArtReference/Player_Run_ThreeQuarter_Base.png`
3. 원본 신체 비율 보조 기준: `Assets/player/new_Player/Player_Idle.png`

## 확인 및 적용

- 머리 비중을 줄이고 몸통·팔을 조금 늘린 수정본. 점프 준비, 도약, 체공, 하강, 착지, 회복의 흐름은 유지.
- 투명 알파 PNG. 모서리 알파는 0. 2픽셀 간격·알파 128 이상 표본 검사에서 1–11번 칸에 캐릭터가 있고, 마지막 칸에는 표시되는 캐릭터가 없음.
- 같은 검사에서 셀 테두리에 불투명한 캐릭터 픽셀은 확인되지 않음.
- 이미지 생성 편집이므로 원본 비율 또는 기존 프레임의 질감이 픽셀 단위로 동일하지는 않음. 프레임별 미세한 외형 차이가 있음.
- 이전 시트와 같은 362×362 슬라이싱 사용. 실제 지면에 닿는 발 높이는 셀마다 조금 다르므로 Sprite Editor의 Pivot을 맞춰야 함.
- 기존 Run·Walk의 크기에 맞추는 Pixels Per Unit도 확인. 이번 작업에서는 Unity 설정을 수정하지 않았음.

## 최종 편집 프롬프트

```text
Use case: precise-object-edit
Asset type: transparent 11-frame jump sprite sheet for a 2D platformer.
Input images:
Image 1 is the EDIT TARGET: the approved 11-pose flame-character jump sheet. Preserve this sheet's successful animation.
Image 2 is the authoritative right-facing three-quarter character reference for the ORIGINAL BODY PROPORTIONS and silhouette.
Image 3 is the original frontal character, a supporting anatomy/proportion reference only. Do NOT switch the sheet to a frontal view.

Primary request: Make a SMALL, proportion-only correction to Image 1 so that all eleven poses feel like the original character in Images 2 and 3 rather than a more exaggerated big-headed chibi version. Keep the animation and sheet intact. This is a surgical refinement, NOT a new jump animation or a redesign.

PROPORTION CORRECTION:
The edit target currently has an overly dominant flame head and an undersized, shortened torso. Restore the reference's slightly smaller head-to-body ratio, more substantial softly rounded torso, and short but clearly formed arms and feet.
In the neutral poses, the flame head from tallest tip to chin should occupy approximately 55–58% of the full silhouette height, leaving approximately 42–45% for neck, torso, and feet, as in the character reference.
Keep the original tall curved main flame tip; do not make the head a round ball. Make the head modestly narrower relative to the overall height, restore the reference's longer torso below the narrow neck, and keep the arms reaching softly toward the lower torso. Preserve very short rounded flame feet.
Keep the overall character height and its existing cell placement approximately unchanged; redistribute the head and body proportions within each silhouette rather than scaling the entire sprite.
Apply the same corrected underlying anatomy consistently across all frames while retaining the existing squash/stretch and foot poses.

PRESERVE EXACTLY:
Exactly ELEVEN poses, the existing 4-column by 3-row arrangement, and the unused twelfth cell.
Same right-facing three-quarter angle, dark oval eyes and narrower far eye, orange-yellow fire, bright cream core, and hand-painted fiery texture.
Same sequence and motion:
Row 1: neutral, mild preparation squash, deep preparation squash, upward takeoff stretch.
Row 2: ascent, relaxed apex, descent, landing with closed eyes.
Row 3: deeper landing squash with closed eyes, recovery with open eyes, neutral finish; final fourth cell empty.
Keep the approved facial expressions, the flame silhouette design, the relaxed arms, and the readable landing compression. Do not add arm swinging or reinterpret the jump.
Do not move poses to different cells, add frames, or remove frames.

OUTPUT:
Retain the 1448 by 1086 canvas with twelve equal 362 by 362 cells, four columns and three rows. No visible grid.
Keep every flame tip and foot entirely inside its cell, with transparent gaps.
Genuine transparent alpha background. The last unused cell must be empty.
No scenery, floor, shadows, background gradient, checkerboard, text, numbers, labels, border, watermark, smoke, or new floating particles.
Change ONLY character proportions; keep all other successful features of the edit target as close as possible.
```


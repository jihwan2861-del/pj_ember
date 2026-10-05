# 점프 원본 비율 v3 / 체공 3프레임

생성 방식: 내장 image_gen. 기존 원본과 이전 점프 시트를 덮어쓰지 않고 별도 PNG로 저장했다. Animator, 프리팹, 씬, 스크립트는 수정하지 않았다.

## 점프 — OriginalRatio v3

- 파일: `Assets/player/new_Player/Player_Jump_Reference_11Frame_Sheet_OriginalRatio_v3.png`
- 편집 대상: `Player_Jump_Reference_11Frame_Sheet_Proportions_v2.png`
- 원본 비율 기준: `Assets/player/new_Player/Player_Idle.png`
- 시점 보조 기준: `ArtReference/Player_Run_ThreeQuarter_Base.png`
- 크기: 1448×1086. 4열×3행, 셀당 362×362.
- 왼쪽에서 오른쪽, 위에서 아래로 1–11번 사용. 마지막 12번 칸은 제외.
- 머리 전체를 줄이는 수정. 점프 준비, 도약, 체공, 하강, 착지, 회복의 순서는 유지.
- 첫 칸의 상부 영역(y < 230)을 알파 128 이상·2픽셀 간격으로 비교한 대략적인 외곽 크기: v2 165×195 → v3 125×155픽셀. 정확한 머리 영역 분리 측정이나 모든 프레임에서 균일한 20% 변환을 보증하는 수치는 아니다.
- 머리를 줄여 전체 캐릭터 높이도 달라졌다. 게임에서 기존 크기로 보이게 하려면 Pixels Per Unit을 확인한다.
- 프레임마다 발 높이와 외형이 조금 달라서 Pivot 정렬이 필요할 수 있다. AI 편집이므로 원본과 픽셀 단위로 동일하지는 않다.

### 점프 편집 최종 프롬프트

```text
Use case: precise-object-edit
Asset type: transparent 11-frame jump sprite sheet.

INPUT ROLES:
Image 1: EDIT TARGET, the existing eleven-frame jump sheet. Its head is STILL TOO LARGE.
Image 2: the ORIGINAL Player_Idle character. This is the authoritative source for body proportions and character identity.
Image 3: the supporting three-quarter orientation reference. Keep the right-facing three-quarter view, but do not adopt any exaggerated head size.

PRIMARY CORRECTION — IMPORTANT:
The previous subtle edit did not sufficiently reduce the head. Make an unmistakable but anatomy-preserving correction this time.
In EVERY frame of Image 1, uniformly reduce the ENTIRE HEAD, including its fire crest, cheeks, face, and both eyes, to approximately 80% of its current linear width AND height.
Treat the head as one connected region and shrink it uniformly, not just its flame tip.
Reconnect its chin smoothly at the SAME neck attachment point, leaving the neck, torso, arms, hips, legs, and foot positions as they are.
The head should no longer dwarf the torso. The head width should be approximately the same as, or only slightly wider than, the span of the relaxed arms; do not retain the oversized chibi head from Image 1.
In the neutral poses, the complete head (from tallest flame tip to chin) should occupy around 55–57% of the full silhouette height, with neck, torso, and feet taking the other 43–45%, comparable to Image 2.
Do NOT compensate by enlarging the head again to fill the cell. It is fine for the corrected full character to become a little shorter within the unchanged cell. Keep the feet/hip anchors fixed.
This is a head-size/proportion correction, NOT a new character design, not merely making the entire sprite smaller, and not adding longer legs.

PRESERVE:
Exactly eleven characters in the existing four-column, three-row grid; twelfth cell empty.
All current jump phases and their order: neutral, mild anticipation squash, deeper anticipation squash, takeoff stretch, ascent, apex, descent, first landing, deeper landing squash, recovery, neutral finish.
Preserve existing body poses, takeoff/landing squash/stretch, relaxed arms, foot poses, and closed-eye expressions in frames 8 and 9.
Keep the same painted orange-yellow fire and cream core; maintain dark oval eyes, narrow far eye, narrow neck, soft rounded torso, very short rounded feet, and flame silhouette identity.
Do not change the viewing angle or facing direction. Do not convert to a frontal view.
Retain image 1's successful texture, flame contours and motion as closely as possible; only resize the head relative to its body. No added accessories or limbs.

OUTPUT:
Unchanged 1448 by 1086 pixel canvas. Four columns and three rows of 362 by 362 cells.
All characters entirely within their cells, with transparent spacing.
Genuinely transparent alpha background, no floor, background gradients, smoke, detached sparks, glow clouds, shadows, labels, text, borders, grid, checkerboard, or watermark.
The last cell remains completely unused.
The requested improvement is clearly SMALLER HEADS WITH ORIGINAL CHARACTER PROPORTIONS, while the same eleven jump poses remain recognizable.
```

## 체공 — 3프레임

- 파일: `Assets/player/new_Player/Player_Airborne_3Frame_Sheet.png`
- 캐릭터 기준: `ArtReference/Player_Run_ThreeQuarter_Base.png`
- 체공 자세 기준: 점프 v2의 6번 프레임(둘째 행 둘째 칸).
- 크기: 1536×1024. 가로 한 줄에 3프레임, 셀당 512×1024.
- 머리 불꽃이 휘어졌다가 솟고 풀리는 1 → 2 → 3 → 1 반복용 시트.
- 원본에 가까운 머리/몸통 비율을 유지. 머리 끝 높이 변화는 의도한 불꽃 움직임이다.
- 몸·눈은 거의 같은 자세이지만 생성 결과에 미세한 질감·가로 정렬 차이는 남아 있다.
- 몸통 중심의 가로 위치를 맞추고 싶다면 Custom Pivot X를 약 0.49 / 0.51 / 0.52로 시작해 눈과 목 위치를 비교해 조정한다. 이는 표본 외곽 기준의 시작값이지 자동 적용된 설정은 아니다.
- 모든 프레임에서 표본 발 하단 높이는 y=922로 같았다. Pivot Y는 기존 캐릭터의 기준점에 맞춘다.
- 다른 시트와 셀 크기·캐릭터 높이가 다르므로 같은 Pixels Per Unit을 그대로 적용하면 크기가 맞지 않을 수 있다.

### 체공 생성 최종 프롬프트

```text
Use case: stylized-concept
Asset type: a THREE-FRAME airborne idle / jump-apex loop sprite sheet for a side-scrolling 2D platformer.

Input images:
Image 1 is the authoritative character design and ORIGINAL PROPORTIONS reference. Match its slightly right-facing three-quarter view, oval eyes, narrow neck, substantial rounded torso, short soft arms and feet, orange-yellow fire, cream-white core, and fine hand-painted flame texture.
Image 2 is a supporting AIRBORNE POSE reference. Use its relaxed apex pose (second row, second column, frame 6) as the idea for gentle weightless legs and relaxed arms. Do NOT reproduce its eleven-frame layout.

Primary request:
Create exactly THREE full-body frames of the SAME flame spirit calmly suspended at a jump's apex. Animate the FLAMES ON ITS HEAD so they visibly undulate in a subtle seamless loop. The body is nearly still; this is not a running cycle and not a takeoff/landing sequence.

PROPORTIONS:
Keep the original character proportions from Image 1, NOT a newly exaggerated big-headed chibi character. The head including the tallest flame tip occupies approximately 55–58% of the neutral full silhouette; neck, torso, and feet occupy the remaining approximately 42–45%. Keep the rounded soft torso substantial and do not shorten it into a tiny nub.

FROZEN LAYER:
First establish ONE right-facing three-quarter airborne pose.
Reuse that exact body and face across all three frames as a frozen layer.
Keep the two oval eyes open, identically shaped and identically positioned. The far eye remains narrower. No blinking, smile, mouth, or face rotation.
Keep the lower head/cheeks, neck, torso, shoulders, arms, hips, and two short flame feet unchanged in shape, painted texture, scale, and cell-relative position.
Arms are relaxed beside the torso; both small feet hang gently with a slight soft curl and a narrow gap, as if weightless. No ground contact.
No body bobbing, leaning, squash/stretch, arm movement, leg stepping, or vertical translation. The game engine supplies physical movement.

ANIMATE ONLY THE HEAD-FLAME CRESTS ABOVE THE EYES:
Preserve the distinctive tall curling central flame and the smaller surrounding flame lobes.
Frame 1: the tall flame crest curls gently toward screen-left; side flame lobes follow a soft outward ripple.
Frame 2: the central crest rises slightly taller and becomes a little narrower; the small side tips lift inward. Keep the flame roots and face anchored.
Frame 3: the crest relaxes into a slightly opposite/rightward ripple with a softer curved tip; smaller lobes ease back toward frame 1.
Use restrained but clearly distinguishable flame-tip bending and small length changes, not a different hairstyle or different flame character in every frame.
Allow painted fire streaks to flow ONLY inside the moving upper flame crests. Do not flicker the entire character's texture or change global brightness.
The cyclic progression 1 → 2 → 3 → 1 must feel like living fire; all transition distances should be gentle, including 3 → 1.

LAYOUT:
Exactly THREE equal-sized portrait cells in ONE horizontal row.
Canvas 1536 by 1024 pixels, three 512 by 1024 cells.
Same local hip/face anchor and exact body alignment within every cell, consistent character scale, and identical foot height.
Keep full characters visible with generous transparent gaps and safe margins around all moving flame tips. Do not add extra frames.

BACKGROUND:
Genuine transparent alpha background. Preserve the fiery painted light INSIDE the silhouette, without a glow cloud outside it.
No floor, scenery, shadows, smoke, detached sparks, motion trails, labels, numbers, text, visible grids, borders, checkerboard pattern, or watermark.
```

## PNG 확인

- 두 시트의 모서리 알파는 0. 체공 시트의 빈 프레임 사이와 아래 여백에서도 확인한 알파는 0이다.
- 알파 128 이상·2픽셀 간격 표본 검사에서 셀 테두리의 불투명 캐릭터 픽셀은 확인되지 않았다.
- 점프 1–11번 칸에 캐릭터가 있고 12번 칸에는 표시되는 캐릭터가 없다. 슬라이싱 시 12번 칸을 수동으로 제외한다.
- 검증은 치수·알파·배치에 대한 확인이며 Unity에서의 실제 애니메이션 재생 검증은 아니다.


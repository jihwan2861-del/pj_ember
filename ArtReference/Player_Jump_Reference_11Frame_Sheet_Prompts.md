# 참고 점프를 따른 앰버 11프레임 시트

- 생성 방식: 내장 image_gen. 캐릭터 기준과 동작 참고를 분리해서 생성한 뒤 투명 여백과 등록 위치 보정을 시도했습니다.
- 캐릭터 외형 기준: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ArtReference/Player_Run_ThreeQuarter_Base.png
- 점프 동작 참고: C:/Users/admin/Downloads/CharacterAnimations/CharacterAnimations/Jump.png (원본 5632 × 512 px, 가로 11프레임)
- 최종 PNG: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Jump_Reference_11Frame_Sheet.png
- 실제 출력: 1448 × 1086 px. 4열 × 3행, 각 칸 362 × 362 px.
- 위에서 아래, 각 행의 왼쪽부터 오른쪽으로 읽습니다. 마지막 오른쪽 아래 칸은 사용하지 않습니다.
- 이전 4프레임 점프, Run, Walk, 기준 자세 및 사용자의 참고 파일은 그대로 보존했습니다.
- Animator, 프리팹, 씬 또는 스크립트는 수정하지 않았습니다.

## 프레임 순서

1. 대기/시작
2. 약한 준비 압축
3. 강한 준비 압축
4. 도약
5. 상승
6. 정점/체공
7. 하강
8. 착지 시작, 눈 감기
9. 착지 압축, 눈 감기
10. 복원, 눈 뜨기
11. 대기/종료
12. 사용하지 않는 칸

배치: 첫째 행 1–4, 둘째 행 5–8, 셋째 행 9–11 + 미사용 칸.

## 검증 및 남은 후작업

- PNG의 모서리 알파는 0이고, 각 셀 경계에서 샘플링한 불투명 캐릭터 픽셀은 없습니다.
- 미사용 12번째 칸에는 캐릭터가 없습니다. 다만 113개 픽셀에 최대 알파 1/255의 매우 옅은 흔적이 남아 있으므로, 자동 빈 칸 판정에 의존하지 말고 12번째 칸을 직접 제외합니다.
- 생성형 이미지라 얼굴·질감·윤곽이 프레임별로 완전히 같지는 않습니다.
- 보정 후에도 접지선이 완전히 같지는 않습니다. 불투명 발 영역 샘플 기준 1–3은 셀 내부 y≈352, 8은 y≈348, 9–11은 y≈332(top-origin)입니다. 착지/종료가 약간 뜨지 않도록 Pivot을 맞추거나 Aseprite에서 9–11을 약 20px 아래로 정렬합니다.
- 마지막 대기 자세는 처음과 유사하지만 픽셀 단위 복제는 아닙니다. 정확한 시작/종료 일치가 필요하면 첫 프레임을 마지막 프레임에 복제합니다.
- 참고처럼 준비 동작과 착지 압축, 작은 체공 오프셋이 포함되어 있습니다. 실제 점프 높이와 입력 반응은 게임 물리가 담당하게 합니다.

## Unity 사용 안내

1. Texture Type = Sprite (2D and UI), Sprite Mode = Multiple.
2. Sprite Editor → Slice → Grid By Cell Size = 362 × 362.
3. 위에서 아래, 왼쪽부터 오른쪽으로 1–11만 사용합니다. 12번째 칸을 애니메이션에 넣지 않습니다.
4. 크기 주의: 기존 Run/Walk는 543 × 724 셀이며 캐릭터 높이가 약 560–584px, 이번 중립 프레임은 약 316px입니다. 동일 PPU를 쓰면 이번 캐릭터가 작아집니다. 이번 시트의 PPU를 기존 Run PPU의 약 0.54–0.56배로 설정한 뒤 눈으로 맞추는 방식이 가능합니다.
5. 발 위치 주의: 셀 전체를 Slice했다면 Custom Pivot의 Y를 1–3 및 4–7 약 0.028, 8 약 0.039, 9–11 약 0.083부터 확인하세요. X는 동일한 기준으로 맞추고 기존 Idle/Run의 플레이어 원점과 비교합니다. 이 값은 불투명 발 영역의 샘플을 바탕으로 한 시작값이며 수작업 확인이 필요합니다.
6. 이 시트는 전체 점프를 표현한 참고용 흐름입니다. 게임에서는 준비(1–3), 상승(4–5), 정점(6), 하강(7), 착지(8–11)를 분리해서 실제 이동 상태에 맞춰 사용하는 편이 좋습니다.

## 최초 생성 프롬프트

```text
Use case: identity-preserve.
Asset type: an ELEVEN-frame full jump sprite sheet for a side-scrolling 2D platformer.

INPUT IMAGE ROLES:
Image 1 is the EXACT flame-character identity and hand-painted appearance reference.
Image 2 is the motion reference: its eleven wizard frames depict a complete jump, including anticipation and landing. Follow those eleven poses and their order, adapting the motion to the flame character.
Do NOT copy the wizard, hat, leaf, robe, shoes, purple outlines, white eyes, white backing or black rectangles. This is still the original orange-yellow flame spirit.

CHARACTER:
Keep the right-facing three-quarter view of Image 1, its rounded flame head and tall curling flame tip, bright cream core, orange-yellow edges, short rounded arms and legs, and hand-painted fiery texture.
Two dark oval eyes, with the farther eye slightly narrower. No mouth, nose, clothing, accessories, shoes, realistic knees or extra limbs.
Keep the character design and overall scale consistent throughout. The relaxed arms stay close to the body without running arm swings.
Unlike a frozen legs-only run, this jump includes the subtle whole-body squash, stretch and recovery visible in Image 2.
Keep the same head-flame design and painterly texture, with no unrelated flickering or random redesign.

EXACT ELEVEN-FRAME SEQUENCE, following Image 2 left to right:
1. Neutral starting pose: upright, relaxed rounded body, both short feet beneath the hips, eyes open.
2. Light anticipation: lower the hips slightly and shorten the torso a little; feet stay planted, eyes open.
3. Deepest preparation: a slightly stronger compact crouch/squash, feet still planted, eyes open. The flame body bends softly, with no realistic knee joints.
4. Takeoff: body quickly straightens with a subtle upward stretch; both feet leave the ground together and trail downward, eyes open.
5. Rising: upright airborne pose with two short feet hanging beneath the body, relaxed arms, eyes open.
6. Apex hold: almost the same airborne pose as frame 5, settling into a brief gentle float, eyes open.
7. Falling: upright body comes back toward its resting proportions, both feet lowered for the incoming landing, eyes open.
8. First landing contact: feet reach the shared ground baseline, the body begins to absorb the impact; both eyes briefly narrow into DARK curved closed-eye shapes matching the character.
9. Landing compression: compact soft squash, hips lowered and both feet planted; keep the eyes gently closed, as in the motion reference.
10. Recovery: body rises partway from the landing squash, eyes reopen.
11. Neutral finish: return to the same upright proportions and pose as frame 1.

Keep the motion compact and cute, copying the reference's timing and pose relationships. This is not a running cycle or a high-kicking jump. No large drawn travel arc inside the sheet.

LAYOUT:
Reformat the motion reference into a clean FOUR-COLUMN by THREE-ROW sprite sheet, not one extremely narrow eleven-character strip.
EXACT canvas 1448 x 1086 pixels. Each of the twelve invisible cells is 362 x 362 pixels.
Read frames LEFT TO RIGHT, TOP TO BOTTOM:
Top row: frames 1, 2, 3, 4.
Middle row: frames 5, 6, 7, 8.
Bottom row: frames 9, 10, 11, then ONE COMPLETELY EMPTY TRANSPARENT CELL.
There are EXACTLY ELEVEN characters total, not twelve. Bottom-right cell (column 4, row 3) must contain NOTHING: no character, glow, label, mark or guide.

Register each pose to its cell, not by the combined silhouettes. The horizontal body center stays at local x=181 in every cell. Typical neutral character height is approximately 300 pixels.
Grounded frames 1,2,3,8,9,10,11 have the same foot baseline at local y=340.
Keep only the small reference-like airborne offsets: frame 4 feet near local y=326, frame 5 near y=316, frame 6 near y=314, frame 7 near y=324. These are small pose offsets, NOT a large physical jump trajectory.
Preparation and landing squash shorten the body upward from the planted feet. Keep all flame tips and both feet fully visible within their cells, with clear transparent padding between neighboring characters.

BACKGROUND AND OUTPUT:
Genuine transparent alpha background, including all gutters and the completely empty twelfth cell.
Preserve bright painted fire within each character's silhouette.
No scenery, ground, shadows, smoke, diffuse glow clouds, gradients, checkerboard pattern, labels, numbers, borders, visible cell grid, motion arrows or watermark.
One single PNG containing all eleven frames of the complete jump.
```

## 최종 보정 프롬프트

```text
Use case: precise-object-edit.
Asset type: cleanup and foot-baseline registration of an eleven-frame jump sprite sheet.

Input image 1 is the EDIT TARGET. Preserve the EXACT existing 4-column x 3-row layout, 1448 x 1086 canvas, and 362 x 362 cells.
Keep the eleven existing flame characters and their poses, right-facing three-quarter orientation, hand-painted orange-yellow texture, cream core, soft short limbs and dark eyes.
Do not redraw into a new character or reorder the jump sequence.

TECHNICAL CLEANUP ONLY:
1. Remove ALL detached red/orange pixels, wisps, horizontal scraps, glow remnants and isolated low-alpha speckles OUTSIDE the character silhouettes. These are not requested particles. Keep fine anti-aliasing only directly at a character's flame edge.
2. Make the entire twelfth cell (bottom row, fourth column, x=1086..1447 and y=724..1085) COMPLETELY EMPTY alpha=0 at EVERY pixel. No residual alpha haze or speckles, no twelfth character or symbol.
3. Keep the horizontal body center consistent in each cell. To provide safe padding, shrink each character uniformly by about 7 percent, without changing its design or its intended squash/stretch pose.
4. Register ALL grounded frames 1,2,3,8,9,10,11 to ONE common foot-contact line at local y=348 in their cells. Do not let the compressed landing float above the floor. Ground baseline is INVISIBLE, never draw a line.
5. Keep the airborne frames 4,5,6,7 fully inside their cells, with their feet above that line: local foot levels approximately 326,316,314,324 respectively. Keep clear padding above all flame tips; no cropping.
6. Frame 11 must be the SAME neutral pose, character scale and placement as frame 1, duplicated exactly, so the jump settles back into the starting idle.

POSES TO PRESERVE, read left-to-right then top-to-bottom:
Row 1: (1) neutral eyes open, (2) slight anticipation squash eyes open, (3) deeper preparation squash eyes open, (4) takeoff straightening eyes open.
Row 2: (5) airborne rising eyes open, (6) brief apex float eyes open, (7) descending eyes open, (8) first landing compression with dark closed-eye curves.
Row 3: (9) deeper landing compression with dark closed-eye curves, (10) recovery with eyes reopened, (11) neutral matching frame 1, (12) completely empty transparent.
Keep relaxed arms, no running arm swings, shoes, clothing, hats, realistic knees or extra limbs.

Transparent PNG with no scenery, ground, shadows, gradients, smoke, checkerboard pattern, labels, numbers, cell outlines, borders, arrows or watermark.
Keep only the eleven character silhouettes and their delicate anti-aliased flame edges on genuine transparency.
```


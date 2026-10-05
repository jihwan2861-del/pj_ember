# 앰버 점프 4프레임 시트

- 생성 방식: 내장 image_gen. 점프 네 단계를 생성한 뒤 프레임 정렬을 보정했습니다.
- 기준 이미지: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ArtReference/Player_Run_ThreeQuarter_Base.png
- 최종 PNG: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Jump_4Frame_Sheet.png
- 실제 크기: 2172 × 724 px, 가로 4칸. 각 칸 543 × 724 px.
- 왼쪽부터: 1 이륙/초기 상승, 2 상승/다리 회수, 3 정점/체공, 4 하강/착지 준비.
- 루프 시트가 아닙니다. 하강 시에는 마지막 자세를 유지하고, 착지 후 다른 상태로 전환합니다.
- PNG 투명 알파 확인: 모서리 및 셀 사이 여백 샘플 알파 0. 셀 경계에서 샘플링한 불투명 캐릭터 픽셀 없음.
- 생성형 이미지의 한계: 정렬은 보정했지만 얼굴·불꽃 윤곽·질감은 프레임별로 미세하게 다를 수 있습니다. 정확히 동일한 픽셀 레이어가 필요하면 Aseprite 등에서 동일 머리 레이어를 복제해 다듬어야 합니다.
- 원본 기준 자세, Run/Walk 및 이전 시트는 변경하지 않았습니다. Animator, 프리팹, 씬과 스크립트도 수정하지 않았습니다.

## Unity에서 사용

1. Sprite Mode = Multiple로 설정합니다.
2. 원본 너비가 2048보다 크므로 원본 해상도를 유지하려면 Max Size = 4096을 사용합니다.
3. Sprite Editor → Slice → Grid By Cell Size = 543 × 724.
4. 네 프레임의 Pivot을 같은 기준점에 맞추고, 기존 Idle/Run/Walk와 캐릭터 크기도 확인합니다.
5. 점프 상태에 맞춰 1–2 상승, 3 정점, 4 하강으로 사용합니다. 착지 자세는 이 시트에 포함하지 않았습니다.

## 최초 생성 프롬프트

```text
Use case: identity-preserve.
Asset type: a transparent four-frame jump sprite sheet for a side-scrolling 2D platformer.
Input image 1 is the EXACT character identity, painterly style and right-facing three-quarter pose reference.

Create a 4-frame jump sprite sheet using the attached flame character as the exact design reference.

CHARACTER AND VIEW:
Preserve the character's right-facing three-quarter view.
Both head and torso face slightly toward screen-right.
Keep both dark oval eyes visible, with the farther eye slightly narrower.

Preserve the rounded flame head, short soft limbs, orange-yellow fire, bright cream core, and hand-painted fiery texture.
No mouth, nose, clothing, accessories, shoes, or realistic joints.

Keep the face, eye shapes, head-flame design, colors, and texture consistent across all four frames.
Both arms remain relaxed near the body, without swinging.
Do not redesign the character or animate the head flames independently.

ANIMATION:
These are four different phases of ONE jump, not a looping running animation.

FRAME 1 — TAKEOFF / EARLY ASCENT:
The character has just left the ground.
Give the soft torso a very subtle upward stretch.
Bring both short flame feet close together beneath the body, angled slightly backward toward screen-left.
The pose should feel like a quick upward launch, not a standing or running pose.

FRAME 2 — ASCENT:
The character is traveling upward.
Curl the two short flame legs slightly closer to the lower body, with the feet trailing gently behind.
Keep both feet recognizable and separate.
Maintain the same head and face, without realistic bent knees or long legs.

FRAME 3 — APEX / BRIEF FLOAT:
The character reaches the highest point of the jump.
Relax the torso back toward its original rounded shape.
Let both short feet hang softly beneath the body, slightly separated.
The pose should feel momentarily weightless and calm.
No exaggerated spread, kick, or rotation.

FRAME 4 — DESCENT:
The character is falling and preparing to land.
Lower both rounded feet beneath the hips, with a small separation between them.
Use a subtle vertical stretch in the soft lower body to suggest downward movement.
The feet point downward, ready for contact, but are not yet touching the ground.
No landing squash in this frame.

MOTION AND CONSISTENCY:
Keep the changes small and readable, suitable for a cute flame character.
Use the curvature and length of the soft flame limbs instead of realistic joints.
Keep exactly two arms and two legs in every frame.
No running steps, spinning, high kicks, or extra limbs.

Do not move the entire character upward or downward within the cells to simulate the jump.
The game engine will control the character's actual vertical movement.
Keep a consistent cell-relative hip anchor and character scale across all four frames.

SPRITE SHEET LAYOUT:
Exactly FOUR equal-sized cells in ONE horizontal row.
Arrange the phases from left to right: takeoff → ascent → apex → descent.
Keep the entire character visible in every cell, including all flame tips and feet.
Leave generous transparent spacing between frames.
No visible cell boundaries or guides.
Intended canvas 2172 x 724 pixels to match the existing Run and Walk sheets, four cells each 543 x 724 pixels. Cell origins x=0,543,1086,1629.
Keep the hip anchor at the SAME local coordinates in all four cells. Character scale is consistent across cells; only the specified small torso and leg pose changes are allowed.

BACKGROUND AND OUTPUT:
Genuine transparent alpha background.
Preserve the bright painted fire inside the character's silhouette.
No scenery, floor, shadows, diffuse glow clouds, smoke, background gradients, checkerboard pattern, motion lines, arrows, labels, numbers, borders, or watermark.

This sheet is NOT a seamless loop.
Frame 4 is a falling pose to hold until the character lands.
```

## 최종 정렬 보정 프롬프트

```text
Use case: precise-object-edit.
Asset type: technical registration cleanup of a four-phase jump sprite sheet.

Input image 1 is the EDIT TARGET. Preserve its four existing distinct jump poses and their exact order: early takeoff with both feet trailing backward toward screen-left; ascent with both short legs curled up; apex with relaxed separated feet; descent with both feet lowered for landing. Keep exactly two short rounded flame arms and two short rounded flame legs. No realistic knees, shoes, running step, accessories or new elements.

CHANGE ONLY REGISTRATION AND CONSISTENCY:
Keep an EXACT 2172 x 724 canvas with FOUR invisible 543 x 724 cells in ONE row. The left edges are x=0,543,1086,1629.
Keep the same scale in all four cells and align the head and hips to the same cell-relative anchors. Do not evenly distribute by the combined silhouette bounds; position each pose relative to its cell origin.
The first three frames have correct matching head height but their X position drifts. The last frame is shifted left and its face is too low.
As a starting registration correction, shift frame 1 approximately 32 pixels left, frame 2 approximately 12 pixels left, frame 3 approximately 12 pixels right, and frame 4 approximately 24 pixels right and 20 pixels up. Fine-tune to exact matching cell-relative anchors.
Use ONE master head and face from the first frame, copied unchanged at the same local coordinates into every cell: same oval eyes, same relative eye positions, same orange-yellow and cream texture and same flame outline. The right-facing three-quarter orientation is unchanged.
The only remaining differences are the already-requested slight torso stretch/relaxation and the different short leg poses. Arms remain relaxed at the sides, not swinging.
No whole-character rise or fall within the sheet to simulate jumping. Movement through the world is controlled by the game engine.
Each full flame tip and every foot remain comfortably INSIDE their cell with clear transparent padding. No cropping, overlap, labels or visible guides.

Maintain the original hand-painted orange-yellow flame appearance and bright cream core, not pixel art or a new character.
Preserve genuine transparent alpha around the flame silhouettes. No black/brown painted backdrop, glow clouds, smoke, gradients, checkerboard pattern, scenery, ground, shadows, motion lines, arrows, text, numbers, borders or watermark.
This is a jump phase sheet, NOT a looping cycle. Frame 4 remains a falling pose, not a landing squash.
```


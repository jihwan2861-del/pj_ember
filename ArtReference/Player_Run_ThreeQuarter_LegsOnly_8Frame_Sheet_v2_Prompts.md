# 불꽃 정령 달리기 8프레임 시트

- 최종 에셋: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Run_ThreeQuarter_LegsOnly_8Frame_Sheet_v2.png
- 기준 자세: `Assets/player/new_Player/Player_Run_ThreeQuarter_LegsOnly_4Frame_Sheet.png` 및 `ArtReference/Player_Run_ThreeQuarter_Base.png`
- 생성 방식: 내장 image_gen. 4프레임 원본은 변경하지 않았습니다.
- 크기: 2048 × 768 px, 가로 8칸, 각 칸 256 × 768 px.
- 배경: 알파 투명. 이미지 바깥 모서리 샘플의 alpha는 0입니다.
- 검토 결과: 눈과 상체 위치는 거의 고정됐고, 발 동작은 8단계로 나뉩니다. 생성형 이미지라 상체 텍스처까지 모든 프레임에서 픽셀 단위로 완전히 같지는 않습니다. 엄밀히 같은 상체를 써야 한다면 Aseprite에서 기준 상체를 복제해 다리 위에 정렬하는 후처리가 필요합니다.
- Unity 슬라이싱: Sprite Mode = Multiple, Sprite Editor에서 Grid By Cell Size = 256 × 768; 모든 프레임에 같은 Pivot을 사용하세요.

## 최종 프롬프트: 8프레임 시트

```text
Use case: identity-preserve.
Asset type: an eight-frame running-in-place character sprite sheet for a side-scrolling 2D platformer.

Reference image 1 is the approved right-facing three-quarter base pose; use it for the exact viewing angle, face, and proportions. Reference image 2 is the original flame character; use it for the original painted flame design, warm colors, and texture.

The whole character faces screen-right in a subtle three-quarter view, 30–40 degrees from frontal. Head and torso face the same direction. Both dark oval eyes remain visible, with the farther eye narrower. Preserve the rounded flame head, short soft limbs, orange-yellow fire, bright cream core, and hand-painted fiery texture. No mouth, nose, clothes, accessories, shoes, or realistic joints.

FROZEN UPPER BODY:
Make one right-facing upper-body pose and reuse it unchanged in every cell. The same head, eyes, torso, shoulders, arms, flame contours, lighting and painted texture stay at exactly the same local position, scale and orientation in all eight cells. Arms relaxed at the sides. No arm swing, blink, breathing, body bounce, leaning change, head-flame motion, or texture flicker. ONLY animate the two short legs below the hips.

LEG IDENTITY:
Camera-near leg = RIGHT leg throughout. Camera-far leg = LEFT leg throughout. Preserve their near/far depth order and overlap. Forward is screen-right; backward is screen-left. Exactly two rounded, compact flame legs and feet in every cell.

EIGHT DISTINCT CONTINUOUS POSES:
1. Right foot lifted compactly beneath the hips; left foot supports under the body.
2. Right foot swings forward to screen-right and rises; left toe pushes off toward screen-left.
3. Right foot extended forward and lowering toward the ground; left foot lifted behind. Both briefly airborne.
4. Right foot contacts the ground slightly in front of the hips, with only a subtle flattening of its bottom; left foot lifted behind.
5. Opposite passing pose: right foot moves under the hips from the front; left foot lifts under the hips to swing forward.
6. Left foot swings forward to screen-right and rises; right toe pushes off toward screen-left.
7. Left foot extended forward and lowering toward the ground; right foot lifted behind. Both briefly airborne.
8. Left foot contacts the ground slightly in front of the hips, with only a subtle flattening of its bottom; right foot lifted behind and ready to return to pose 1.
Pose 5 mirrors pose 1; pose 6 mirrors pose 2; pose 7 mirrors pose 3; pose 8 mirrors pose 4. Do not repeat the same four poses twice with the same leg leading. Do not spread legs sideways like jumping jacks. Use small changes in the short soft leg curves; no knees, long limbs, or extra feet.

SHEET LAYOUT — EXACT PIXEL GRID:
Output exactly 2048 x 768 pixels, a single horizontal row of exactly 8 equal cells, each 256 x 768 pixels. Each character is centered at the same local x coordinate (128 px) in its cell. Every upper body begins at the same local y coordinate and has the same scale. Make each whole figure fit comfortably inside its 256-pixel cell without touching neighboring cells. Keep all flame tips and both feet visible. Contacting feet share one invisible baseline at the same local y in every cell. Clearly distinct transparent gaps between silhouettes. Do not make fractional-width cells, an extra margin column, or a second row.

Genuine transparent alpha everywhere outside the character. No ground plane, drawn shadows, diffuse glow clouds, gradient, checkerboard, scene, labels, numbers, borders, or watermark. Keep bright painterly fire inside the figure.

Loop order: 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 1.
```

## 정렬 수정 프롬프트

```text
Use case: precise-object-edit.
Asset type: final alignment and frozen-layer correction for a game sprite sheet.

Image 1 is the edit target, already a correctly sized 2048 x 768 PNG with eight 256 x 768 cells in one row and transparent alpha. Preserve this canvas, these exact eight cells, and their leg animation. Do not add or remove frames.

The first cell is the MASTER. Treat its entire upper body from the flame tip through the hips as a single frozen pixel layer. Copy that same layer into cells 2, 3, 4, 5, 6, 7 and 8. Pixel-for-pixel identical: same head silhouette, face, eye positions and sizes, fire details, body contour, shoulders, resting arms, color and texture. Use one exact upper-body copy in all eight cells. Do not regenerate each upper body independently. Do not blur, redraw, recolor, or change details.

Align the copy to the identical local coordinates of cell 1: each cell is 256 pixels wide; local x is the same in every cell. The current later cells drift a few pixels left, so correct that drift. The eight upper bodies must not jitter when animated. Keep their y position, scale and facing identical as well. Never change the legs to solve alignment.

Only retain the already different leg poses below the hips, one pair per cell, in this order: 1 right lifted under hips / left planted; 2 right forward up / left toe pushing back; 3 right forward lowering / left raised behind, brief airborne pose; 4 right contacting in front / left lifted behind; 5 opposite pass with left lifting forward and right moving under hips; 6 left forward up / right toe pushing back; 7 left forward lowering / right raised behind, brief airborne pose; 8 left contacting in front / right lifted behind, leading back to frame 1. Keep depth overlap and exactly two soft legs. Do not let changes to legs move the hips, torso, arms or head.

Preserve the flame character's right-facing three-quarter view, bright cream core, orange-yellow painterly fire and two visible dark oval eyes. Preserve the current genuine transparent background and the clean clear spacing. No external glow, background color, floor, shadow, labels, borders or watermark.

Output stays exactly 2048 x 768 pixels, eight equal horizontal cells of 256 x 768, with transparent space beyond each silhouette.
```


# 오른쪽 3/4 방향 8프레임 달리기 시트

- 생성 방식: 내장 image_gen. 다리만 움직이는 8프레임 시트를 생성한 뒤 셀 크기와 정렬을 보정했습니다.
- 입력 기준 자세: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ArtReference/Player_Run_ThreeQuarter_Base.png
- 최종 PNG: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Run_ThreeQuarter_LegsOnly_8Frame_Sheet.png
- 크기: 2048 × 768 px. 가로 8칸, 각 칸 256 × 768 px.
- PNG 알파 확인: 모서리 및 셀 사이 여백의 알파 0, 각 셀 경계에서 샘플링한 불투명 캐릭터 픽셀 없음.
- 접지 프레임의 발 높이는 공통 기준이며, 3번과 7번은 발이 살짝 뜬 공중 자세입니다.
- 생성형 이미지의 한계: 상체 위치·텍스처·윤곽이 픽셀 단위로 완전히 동일하지는 않습니다. 엄밀히 다리만 움직이는 완성본에는 Aseprite 등에서 하나의 상체 레이어를 복제해 고정하는 후작업이 필요합니다.
- Unity 사용: Sprite Mode = Multiple → Sprite Editor → Slice → Grid By Cell Size 256 × 768. 재생 순서 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 1, 재생 속도는 12–16 FPS부터 테스트.
- 모든 프레임의 Pivot을 같은 위치로 설정합니다. 이 시트의 바닥 접지선은 대략 y=668(top-origin)이므로, 발 위치 기준 Custom Pivot을 쓴다면 (0.5, 약 0.13)부터 확인하세요. 기존 Idle 및 플레이어 기준점과도 맞춰야 합니다.
- 원본 기준 자세, 기존 4프레임 PNG, Animator, 프리팹 및 스크립트는 수정하지 않았습니다.

## 생성 프롬프트

```text
Use case: identity-preserve.
Asset type: an 8-frame running-in-place sprite sheet for a side-scrolling 2D platformer.
Input image 1 is the EXACT approved right-facing three-quarter character reference. Preserve its established upper-body pose and design.

CHARACTER AND VIEW:
The character faces screen-right in a subtle three-quarter view, approximately 30–40 degrees away from a frontal pose.
Both the head and torso face the same direction.
Both dark oval eyes remain visible, with the farther eye slightly narrower.
Do not use a fully frontal view or a complete side profile.

Preserve the original character's identity:
A rounded flame head, short soft limbs, dark oval eyes, orange-yellow fire, a bright cream core, and a hand-painted fiery texture.
No mouth, nose, clothing, accessories, shoes, or realistic joints.

FROZEN UPPER BODY:
Treat the reference's right-facing three-quarter upper body as ONE frozen image layer.
Duplicate that exact layer across all eight frames.
Keep the head, eyes, torso, shoulders, arms, flame contours, and upper-body texture unchanged.
Both arms remain relaxed at the character's sides.
No arm swinging, blinking, breathing, body bouncing, leaning changes, head-flame animation, or texture flickering.
The upper body must have identical cell-relative position, scale, and orientation in every frame.
Only the two legs below the hips may change.

LEG IDENTIFICATION:
Call the camera-near leg the RIGHT leg and the camera-far leg the LEFT leg.
These names identify the same limbs throughout the sequence, not their position on the canvas.
Keep their depth and overlap consistent.
FORWARD means screen-right. BACKWARD means screen-left.

Animate the two very short, rounded flame legs through these eight poses:

FRAME 1 — RIGHT FOOT LIFT:
Lift the right foot off the ground and bring it beneath the hips.
The left foot supports the character directly beneath the body.
Keep the lifted foot rounded and compact.

FRAME 2 — RIGHT FOOT FORWARD:
Move the right foot forward toward screen-right and lift it slightly higher.
Move the planted left foot backward toward screen-left, pushing off with its rounded toe.

FRAME 3 — RIGHT FOOT PRE-CONTACT:
Lower the right foot while keeping it extended in front of the body.
The left foot has left the ground and is lifted behind the body.
Both feet are briefly airborne, with a small gap above the ground baseline.

FRAME 4 — RIGHT FOOT CONTACT:
The right foot touches the ground slightly ahead of the hips.
Flatten only the bottom of that rounded foot very slightly to suggest contact.
The left foot remains lifted behind the body.

FRAME 5 — LEFT FOOT LIFT:
The planted right foot moves beneath the hips.
The left foot returns from behind and lifts beneath the body, preparing to swing forward.
This is the opposite-leg counterpart of Frame 1.

FRAME 6 — LEFT FOOT FORWARD:
Move the left foot forward toward screen-right and lift it slightly higher.
Move the planted right foot backward toward screen-left, pushing off with its rounded toe.
This is the opposite-leg counterpart of Frame 2.

FRAME 7 — LEFT FOOT PRE-CONTACT:
Lower the left foot while keeping it extended in front of the body.
The right foot is lifted behind the body.
Both feet are briefly airborne.
This is the opposite-leg counterpart of Frame 3.

FRAME 8 — LEFT FOOT CONTACT:
The left foot touches the ground slightly ahead of the hips.
Flatten only the bottom of that rounded foot very slightly.
The right foot remains lifted behind the body, ready to return beneath the hips in Frame 1.

MOTION RULES:
A planted foot travels from forward → beneath the hips → backward.
A lifted foot returns from backward → beneath the hips → forward.
Use small changes in the length and curvature of the soft flame legs, not realistic knees or long limbs.
Keep exactly two legs and two feet in every frame.
Do not animate the legs spreading sideways like jumping jacks.
Do not merely repeat four images twice; show the intermediate movement and preserve which leg is in front.

SPRITE SHEET LAYOUT:
Exactly EIGHT equal-sized cells in ONE horizontal row, ordered left to right.
Intended canvas 3072 x 1024 pixels, eight cells each 384 x 1024 pixels. Cell origins are x=0,384,768,1152,1536,1920,2304,2688.
Use identical upper-body local coordinates in each cell, not approximate spacing by silhouette.
Uniform character scale, approximately 800 pixels in height, with full flame tips and feet visible inside every cell.
No cropping or overlap between characters. Leave clear transparent padding in every cell.
All contacting feet share one invisible ground baseline.
If overlaid after subtracting each cell's origin, all eight upper bodies should match.
Do not draw the cell borders, origin markers, frame numbers or guides.

BACKGROUND AND OUTPUT:
Genuine transparent alpha background.
No scenery, ground plane, shadows, diffuse glow clouds, background gradients, checkerboard pattern, labels, numbers, borders, or watermark.
Preserve the bright painted fire within the character's silhouette.
The animation must loop smoothly: 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 1.
```

## 최종 셀 크기 및 정렬 보정 프롬프트

```text
Use case: precise-object-edit.
Asset type: technical layout correction of the attached eight-frame running sprite sheet.

Input image 1 is the EDIT TARGET. It already contains the eight intended short-leg run poses in order. Preserve all eight leg poses, the character's right-facing three-quarter view, relaxed stationary arms, orange-yellow and cream palette, proportions and hand-painted flame texture. Do not redesign the character or add any new elements.

CHANGE ONLY THE SHEET LAYOUT AND UPPER-BODY REGISTRATION:
Produce an EXACT 2048 x 768 pixel canvas.
Exactly EIGHT invisible cells in ONE horizontal row, each EXACTLY 256 x 768 pixels.
The cell origins are x = 0, 256, 512, 768, 1024, 1280, 1536, 1792.
Position each upper body at the same local coordinates within its own cell, using an exact 256-pixel stride.
Use the first frame's upper body as ONE frozen master cutout copied into all eight cells. Head, eyes, arms, torso, flame contours and texture are unchanged between copies.
Scale all frames uniformly so each entire character is approximately 540 pixels tall and comfortably fits INSIDE its own 256-pixel-wide cell, with transparent padding on both sides.
Keep the full flame tip, both arms and all feet visible. No clipping, overlapping neighboring cells, or body drift.
Do not reposition the entire row by its combined silhouette bounds; register each character to its cell.

Retain the ordered eight leg poses from the input:
1 near/right foot lifted under hips, far/left foot planted under body.
2 near/right foot raised forward toward screen-right, far/left foot pushing off behind.
3 near/right foot descending forward, far/left foot lifted behind, both airborne.
4 near/right foot contacting the ground ahead, far/left foot lifted behind.
5 far/left foot lifted under hips, near/right foot planted under body.
6 far/left foot raised forward, near/right foot pushing off behind.
7 far/left foot descending forward, near/right foot lifted behind, both airborne.
8 far/left foot contacting the ground ahead, near/right foot lifted behind.
All contacting feet share the SAME invisible baseline. The upper body never bounces or changes pose. Two very short rounded flame legs only, no realistic joints or shoes.

Genuine transparent alpha background, with clear transparent gutters and padding. Outside the flame silhouettes is empty transparent space, not black or brown paint, diffuse halo, smoke, glow cloud or a checkerboard.
No scenery, shadows, ground, labels, numbers, visible grid, borders or watermark.
The final output is one evenly registered eight-frame sprite sheet, not four frames repeated twice.
```


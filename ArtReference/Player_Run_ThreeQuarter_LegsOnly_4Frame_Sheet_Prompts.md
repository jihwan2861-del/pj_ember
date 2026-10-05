# 오른쪽 3/4 방향 다리 전용 달리기 시트

- 생성 방식: 내장 image_gen. 원본 이미지, 기존 Player_Run 애셋, Animator 및 프리팹은 변경하지 않았습니다.
- 최종 시트: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Run_ThreeQuarter_LegsOnly_4Frame_Sheet.png
- 방향 기준 이미지: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ArtReference/Player_Run_ThreeQuarter_Base.png
- 시트 크기: 2048 × 768 px. 가로 4칸, 각 칸 512 × 768 px.
- 투명 알파 확인: 셀 사이 여백 및 바닥 바깥쪽 샘플의 알파 0. 미리보기의 갈색/주황 RGB는 투명 픽셀에도 남아 있을 수 있습니다.
- 상체 위치 보정: 네 프레임의 눈 높이와 셀 내부 위치를 맞췄습니다.
- 한계: 생성형 이미지라 상체의 세부 텍스처가 픽셀 단위로 완전히 동일하지는 않습니다. 엄밀한 다리 전용 애니메이션에는 Aseprite 등에서 기준 상체를 복제해 고정하는 수작업이 추가로 필요합니다.
- Unity에서 사용할 때: Sprite Mode = Multiple, Sprite Editor에서 Grid By Cell Size = 512 × 768, Pivot = 동일한 Custom 값 또는 Bottom Center. 프레임 순서 1 → 2 → 3 → 4 → 1.

## 1. 오른쪽 3/4 방향 기준 자세

```text
Use case: identity-preserve.
Asset type: a single reference pose for a 2D side-scrolling platformer character sprite.

Input image 1 is the exact flame-character design reference. Create ONE full-body character pose, not a sprite sheet yet. Turn BOTH the head and torso approximately 35 degrees toward screen-right from the original frontal view. A subtle right-facing three-quarter view, NOT frontal, NOT a complete side profile. The two dark oval eyes remain visible: the farther eye is slightly narrower, and the face clearly points toward screen-right.

Preserve the reference's character identity and proportions: rounded flame head with the distinctive tall curling flame tip, bright cream core, orange-yellow edges, hand-painted fiery texture, narrow neck, soft rounded torso, two short relaxed drooping arms and two very short rounded flame legs. Adapt the arms naturally to the viewing angle while keeping them at rest, without swinging. Feet rest beneath the body in a neutral pose. No redesign, no mouth, nose, clothing, accessories, shoes, realistic knees, extra limbs, scene, shadow, lettering, frame border, or watermark.

The entire character is visible, centered with some clear padding at every edge. Genuine transparent alpha background, including the space surrounding the fine flame contours. Keep the original painterly style; this is not pixel art or a 3D model. This one pose will be the frozen upper-body reference for a legs-only running loop.
```

## 2. 4프레임 시트

```text
Use case: identity-preserve.
Asset type: transparent 4-frame running-in-place sprite sheet for a side-scrolling 2D platformer.

Input image 1 is the approved RIGHT-FACING THREE-QUARTER BASE POSE. Use this exact pose and character design, not a new redesign. Both head and torso are turned slightly toward screen-right (approximately 30–40 degrees from frontal), with both dark oval eyes visible and the far eye narrower. Maintain this viewing direction in EVERY frame. Preserve the rounded flame head, very short soft limbs, orange-yellow fire, bright cream core and original hand-painted fiery texture. Do not add a mouth, nose, clothing or accessories.

FROZEN UPPER BODY — THE MAIN CONSTRAINT:
Treat everything from the hips upward as ONE frozen cutout layer taken from the reference: head, face, both eyes, torso, shoulders, both resting arms, upper-body flame contours and texture. Duplicate this same upper-body layer four times. Do not redraw it into different poses or adjust any detail between frames.
The resting arms never swing. NO blinking, body bouncing, leaning changes, breathing, head-flame animation, texture evolution or horizontal body drift.
All four copies have the EXACT same cell-relative X and Y position, scale and orientation. The flame tip, eye centers, shoulders and hips must line up if the four frames are overlaid.

ONLY THE TWO LEGS MOVE BELOW THE HIPS:
Keep two very short, rounded, soft flame legs matching the base pose.
Frame 1: one foot extends toward screen-right with the other lifted behind toward screen-left.
Frame 2: feet pass beneath the hips, transitioning to the opposite step.
Frame 3: the opposite foot extends toward screen-right; the first foot lifts behind toward screen-left.
Frame 4: feet pass beneath the hips again, transitioning back to Frame 1.
The legs must have clearly different running poses without moving the upper body. No realistic knees, long legs, shoes or extra limbs. Feet move front-to-back in the rightward travel direction, NOT a sideways jumping-jack motion.

LAYOUT:
Exactly FOUR equal-sized cells in ONE horizontal row, intended canvas 2048 by 768 pixels (each cell 512 by 768). Place each frozen upper body at the same local coordinates within its cell, with equal padding. Keep the entire flame tip and feet visible. A shared invisible ground baseline. Leave clear genuine transparent spacing between the characters. There are no visible cells or guides.

Genuine transparent alpha background. No scenery, ground, shadows, labels, numbers, borders or watermark. Smooth loop: 1 → 2 → 3 → 4 → 1.
Before finishing, verify that all four upper bodies are identically aligned and only the legs change.
```

## 3. 최종 정렬 및 알파 보정

```text
Use case: precise-object-edit.
Asset type: production cleanup of a transparent four-frame running sprite sheet.

Input image 1 is the EDIT TARGET: the existing 2048 x 768 four-frame sheet. Keep its right-facing three-quarter character design, colors, painterly texture, short relaxed arms and short rounded feet. This is a technical alignment and alpha cleanup, not a redesign.

There are four equal INVISIBLE 512 x 768 cells with left edges x = 0, 512, 1024, 1536. Place the copies at exactly a 512-pixel horizontal stride. Do not distribute the characters according to the total silhouette bounds; place each relative to its cell. The upper-body position and scale must be identical within every cell.

Use the FIRST FRAME'S upper body as the MASTER frozen layer. Copy that exact head, eyes, arms, torso, texture and flame contour without alteration into cells 2, 3 and 4. If superimposed after subtracting each cell's x offset, every pixel above the hips should match. Keep the first frame where it is. The existing second, third and fourth upper bodies are incorrectly shifted left within their cells; align them to the first frame's cell-relative position. Shift the second roughly 28 pixels right, the third roughly 56 pixels right and the fourth roughly 80 pixels right, checking the exact master alignment. No body bounce or changes in face, flame tip, pose, proportions or arms.

Animate ONLY the two short legs below the hips in the four alternating run poses: first foot reaching toward screen-right with other lifted behind; feet passing below hips; opposite foot reaching toward screen-right with first lifted behind; feet passing below hips again. Frame 3 is the opposite leg's contact, not a duplicate of frame 1. Keep all visible contacting feet on the same invisible baseline, upper body stationary, no realistic knees, extra limbs, shoes or arm movement.

Remove the large diffuse orange/brown halo that currently spreads across the empty canvas. Preserve the character's bright painted flame colors, but make the space OUTSIDE each flame silhouette fully transparent alpha, with only delicate anti-aliased edge pixels. Do not render glowing clouds, brown/black fill, smoke, lighting circles, vignette, background gradient, checkerboard pattern or shadows. The main character interior is opaque, with fine feathering only at its flame edges. Genuine transparent background and clear transparent gaps between frames.

Final output: ONE 2048 x 768 PNG, exactly four 512 x 768 cells side by side. Every entire character remains visible. No text, numbers, borders, guides, scenery or watermark.
```


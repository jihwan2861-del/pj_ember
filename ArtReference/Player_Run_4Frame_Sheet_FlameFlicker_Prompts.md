# 달리기 4프레임 머리 불꽃 일렁임

- 새 시트: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Run_4Frame_Sheet_FlameFlicker.png
- 입력 원본: `Assets/player/new_Player/Player_Run_4Frame_Sheet.png` (덮어쓰지 않음)
- 생성 방식: 내장 image_gen으로 편집. 머리의 불꽃 끝과 작은 혀불을 프레임별로 조금씩 변화시켰습니다.
- 크기: 2172 × 724 px, 4칸 가로 시트, 각 칸 543 × 724 px.
- 투명 알파: 확인됨. 왼쪽 위 알파=0.
- Unity: Sprite Mode = Multiple, Sprite Editor에서 Grid By Cell Size = 543 × 724. 동일한 Pivot으로 슬라이스하세요.
- 제한: 생성형 이미지 편집이므로 픽셀 단위로 완전히 동일한 부위가 보장되지는 않습니다.

## 머리 불꽃 편집 프롬프트

```text
Use case: precise-object-edit.
Asset type: a transparent four-frame running sprite sheet for a 2D platformer.

Input image 1 is the edit target. It already contains exactly four consecutive run frames in one horizontal row. Preserve the existing cell layout, transparent background, character scale, cell spacing and running poses.

Change only the outer shape of the flame at the top of the head so it flickers softly from frame to frame. The current crown looks too identical and static. Make four clearly different but subtle neighboring moments of one continuous flame motion:
Frame 1: baseline crown, main tip curves slightly backward toward screen-left.
Frame 2: main tip rises a little more upright and tilts slightly toward screen-right; small upper side tongues shift shape.
Frame 3: main tip bends back toward screen-left and becomes slightly shorter; opposite-side tongue stretches subtly upward.
Frame 4: main tip narrows and returns toward the middle, ready to flow smoothly into frame 1.
Keep each variation small, organic and flame-like. Use gentle differences in the shape and curl of the main pointed crown and its smaller side tongues. The head should read as the same character and remain about the same overall size and height. Do not just change its color.

Keep the face and oval eyes, their exact positions and shapes, the bright core around the face, neck, torso, arms, legs, and each running pose unchanged. Keep the character facing screen-right in the same three-quarter view. No body sway or scale changes. The lower body remains pixel-for-pixel as close to the input as possible.

Keep exactly four equal frames in a single row, with identical cell positions, same baselines, and clear spaces between characters. Keep all flame tips inside their original cells. Preserve genuine transparent alpha around the characters. Do not add a glow cloud, background, smoke, scenery, text, labels, lines, borders or watermark.
```

## 얼굴 위치 및 불꽃 변화량 정돈 프롬프트

```text
Use case: precise-object-edit.
Asset type: a four-frame flame-character run sprite sheet, preserving the existing animation.

Image 1 is the edit target. Preserve exactly four equal cells in one horizontal row. Its canvas is 2172 x 724 pixels, each cell 543 x 724. Keep the same clear transparent gaps and genuine transparent alpha.

The sheet should look like the same running character in all frames. The flame crown should flicker visibly but gently: vary the pointed main tip's curve and the small upper flame tongues a little from frame to frame, like a moving flame. Use smooth, small changes no more than about 5% of the head height. A subtle sequence: frame 1 tip slightly backward (screen-left), frame 2 tip a little more upright and leaning screen-right, frame 3 tip gently curls back the other way with smaller side tongues reshaped, frame 4 tip narrows and returns toward center, leading back to frame 1. Keep the crown's overall size and main silhouette similar. No giant new horns, extra detached flames, or large shape changes.

Precise invariants: ONLY the flame outline and curling tongues above the face change. Keep the entire face and both eyes unchanged and at identical local coordinates in every cell. Keep the cheeks, lower outline of the head, core, neck, torso, arms, and existing legs and running poses unchanged. The first frame is the position master: align eye centers and the body centerline in the other three cells to frame 1 so the character does not shift or jitter across the row. Keep the right-facing three-quarter view and the same head/body scale.

Frame 1 remains the master look. Frames 2–4 have distinct gentle flame-tip poses, not four identical crowns. Their changes form a smooth loop. Preserve the orange-yellow painted texture and bright cream core within the character. Keep the area outside the silhouettes genuinely transparent. No background, halo cloud, scenery, ground, shadows, labels, numbers, borders, or watermark.
```


# 머리 크기를 줄인 달리기 시트

- 최종 시트: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Run_4Frame_Sheet_FlameFlicker_SmallerHead.png
- 편집 대상: `Assets/player/new_Player/Player_Run_4Frame_Sheet_FlameFlicker.png`
- 비율 참고: `ArtReference/Player_Run_ThreeQuarter_Base.png`
- 원본 파일은 덮어쓰지 않고 새 버전으로 저장했습니다.
- 생성 방식: 내장 image_gen. 머리 불꽃과 얼굴 전체를 목 접점 기준으로 약 8% 축소했습니다. 네 프레임에 같은 비율을 적용하고, 불꽃 끝의 프레임별 변형은 유지했습니다.
- 크기: 2172 × 724 px. 네 칸, 각각 543 × 724 px.
- 투명 알파 확인: 왼쪽 위 alpha=0.
- Unity 슬라이싱: Sprite Mode = Multiple, Grid By Cell Size = 543 × 724.

## 편집 프롬프트

```text
Use case: precise-object-edit.
Asset type: four-frame run-cycle sprite sheet proportion adjustment.

Input Image 1 is the EDIT TARGET: a four-frame right-facing, three-quarter flame-character running sheet with a transparent background and gently varied flame crowns.
Input Image 2 is the proportion reference for how the same character's head and body should balance.

Make one subtle proportional change: reduce the entire head unit in all four frames by approximately 8% in both width and height. The head unit includes the flame crown and its small tongues, face, eyes and outer head silhouette. Scale it uniformly around its lower attachment point at the neck so the head stays naturally joined to the unchanged torso. Preserve the character's expression, eye shapes and positions relative to the head, the three-quarter facing direction, bright cream core, orange-yellow painterly fire and fine painted texture. The head should still be the largest expressive feature, only slightly less oversized relative to the body, following Image 2's balance.

Keep the existing per-frame flame-tip flicker: four slightly different organic crown shapes across the four cells. The small head-size adjustment is the same in every frame; do not remove the flame variations.

Keep each torso, neck, shoulders, arms, legs and running pose at the same scale and position as in Image 1. Do not shrink the entire character. Keep the four characters aligned in their cells, with matching eye center positions after the same head adjustment, and keep the shared foot baseline. No new limbs, accessories, scenery, lighting effects, ground, captions, borders or watermark.

Preserve the original canvas aspect and the four equal cells in one row. Keep all characters fully visible and the outside of their silhouettes genuinely transparent alpha.
```


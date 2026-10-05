# 새 앰버 캐릭터 — Idle 4프레임

- 생성 방식: 기본 내장 이미지 생성 도구, 투명 배경.
- 기준 이미지: `Assets/player/new_Player/Player_Idle.png`.
- 결과: `C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Idle_4Frame_Sheet.png`.
- 구성: 가로 한 줄 4프레임, 불꽃 일렁임과 작은 호흡 동작.
- PNG 크기: 1774×887. 실제 애니메이션에 사용할 때 프레임 영역과 피벗을 확인한다.
- 원본 이미지와 Animator, 프리팹은 변경하지 않았다.

## 최종 프롬프트

 Use case: identity-preserve.
 Asset type: a four-frame idle animation sprite sheet for the supplied flame character in a 2D platformer.
 Input image 1 is the character identity, exact design, proportions, coloring and rendering reference. Animate this SAME character, do not redesign it.
 Primary request: create exactly FOUR successive full-body standing-idle frames, laid out from left to right in ONE horizontal row. A subtle looping breathing motion and flickering head flames, NOT running, walking, jumping or attacking.
 Style: faithfully match the reference's clean luminous hand-painted 2D artwork. Bright ivory/cream-yellow face and core, golden-yellow fire, orange edges, dark brown-black vertical oval eyes, no black outer outline. Preserve the reference's cute round flame head, tall central flame tongue, small outer flame wisps, narrow neck, plump rounded lower body, two short drooping rounded flame arms and two short separated flame feet. No mouth, nose, clothing, accessories, torch, cup, weapon or scenery.
 Layout: a full uncropped landscape canvas with 4 evenly spaced equal-width cells, exactly one figure per cell. Request a 2048 x 1024 sheet, each cell 512 x 1024. Each sprite centered on its cell's vertical center line; full flame tips and both feet fully visible, equal top and bottom padding, enough transparent gap so neighboring frames never overlap. All characters the SAME base size. Identical foot baseline and foot positions in all four frames, the character does NOT slide horizontally.
 Animation sequence:
 Frame 1: the reference's relaxed neutral stance.
 Frame 2: a very slight inhale, torso only 2-3 percent fuller/taller, head flame tips gently bending left.
 Frame 3: ease back toward the neutral torso, central flame tongue flowing slightly more upright.
 Frame 4: a very slight exhale, torso subtly relaxed, flame tips bending gently right and returning toward frame 1.
 Only subtle torso breathing and the flame contours/internal fiery highlights change. Keep arms at the sides with only tiny soft movement; no swinging or action pose. Eyes stay open, crisp and consistently placed; no blinking within these four frames. Lock facial identity, overall proportions and lower-body placement so the loop reads as one standing character.
 Background: genuine fully transparent alpha background. Preserve transparency, not a black or colored fill, not a fake checkerboard. No floor, drop shadow, glow clouds or large baked halo. Keep fiery light texture inside the character silhouette.
 Constraints: exactly 4 frames, 1 row, no extra figures, no labels, no numbers, no text, no border lines, no watermark. Focus on a usable consistent idle animation source rather than four different character illustrations.

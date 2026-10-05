# 앰버 Run / Walk 4프레임 시트

- 생성 방식: 내장 image_gen. Run과 Walk를 각각 별도 생성했습니다.
- 캐릭터 기준 이미지: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ArtReference/Player_Run_ThreeQuarter_Base.png
- Run 동작·배치 참고: C:/Users/admin/Downloads/CharacterAnimations/CharacterAnimations/Run.png
- Walk 동작·배치 참고: C:/Users/admin/Downloads/CharacterAnimations/CharacterAnimations/Walk.png
- 참고 이미지의 마법사 캐릭터, 의상, 선화 스타일과 배경은 사용하지 않고 앰버의 외형을 유지했습니다.
- Run PNG: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Run_4Frame_Sheet.png
- Walk PNG: C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/player/new_Player/Player_Walk_4Frame_Sheet.png
- 두 파일의 실제 크기: 각각 2172 × 724 px, 가로 4칸. 각 칸은 543 × 724 px입니다. 생성 프롬프트에서 요청한 2048 × 512와 다르므로 실제 출력 크기를 기준으로 자릅니다.
- 투명 알파 확인: 두 파일의 모서리 및 셀 사이 여백 샘플 알파 0. 셀 경계에서 샘플링한 불투명 캐릭터 픽셀 없음.
- Run: 넓은 보폭과 높이 든 회수 다리. Walk: 짧은 보폭과 낮은 회수 다리.
- 한계: 생성형 이미지라 상체의 위치, 질감 및 윤곽이 픽셀 단위로 완전히 같지는 않습니다. 상체를 엄밀히 고정하고 Run/Walk 전환도 완전히 일치시키려면 같은 상체 레이어 복제 및 Pivot 정렬 후작업이 필요합니다.
- 원본 참고 이미지, 기존 4/8프레임 시트, Animator, 프리팹 및 게임 스크립트는 변경하지 않았습니다.

## Unity에서 자르기

1. PNG의 Texture Type을 Sprite (2D and UI), Sprite Mode를 Multiple로 설정합니다.
2. 원본 너비가 2048을 넘으므로 전체 해상도를 유지하려면 Max Size를 4096으로 설정합니다.
3. Sprite Editor → Slice → Grid By Cell Size를 선택하고 Pixel Size를 543 × 724로 설정합니다.
4. 네 프레임의 Pivot을 같은 기준으로 맞춥니다. Run과 Walk의 발 위치도 함께 확인하세요.
5. 왼쪽부터 1 → 2 → 3 → 4 → 1 순서로 재생합니다. 재생 속도는 실제 이동 속도에 맞춰 조절합니다.

## Run 프롬프트

```text
Use case: identity-preserve.
Asset type: a four-frame transparent sprite sheet for a side-scrolling 2D platformer.

INPUT IMAGE ROLES:
Image 1 is the flame character's EXACT identity, appearance and right-facing three-quarter pose reference.
Image 2 is ONLY an animation pose and sprite-sheet layout reference. Borrow the leg movement and four-frame organization, NOT its wizard character, hat, cloak, colors, dark outlines or white/black backing.

Keep the flame character from Image 1: rounded flame head with the tall curling flame tip, two dark oval eyes with the farther eye narrower, short soft body and limbs, orange-yellow edges, luminous cream core and hand-painted fire texture.
Both head and torso face slightly toward screen-right. No mouth, nose, clothing, accessories, shoes or realistic knees.

Keep the upper body stationary across all four frames. Treat the head, eyes, torso, relaxed resting arms and flame contours as ONE frozen master layer repeated at the same local coordinates. No arm swinging, blinking, torso bobbing, changing lean or animated head flame. Animate only the short rounded flame legs below the hips. Keep each leg's near/far identity and depth consistent.

LAYOUT AND ALPHA:
Match Image 2's simple four-cell horizontal format: EXACTLY FOUR frames, one row, equally sized cells, identical character scale.
Intended canvas 2048 x 512 pixels, four cells of 512 x 512. Cell left edges 0,512,1024,1536. Center each upper body at the same cell-local position, with a uniform 512-pixel stride.
The full character including flame tip and feet must fit in each cell with generous transparent gaps. Intended character height about 440 pixels, contact baseline at y=480. No clipping or overlap.
Genuine transparent alpha background, including all space around the characters. Do not reproduce white areas or black rectangles from Image 2.
No scenery, floor, shadows, background gradient, diffuse glow cloud, checkerboard pattern, grid lines, numbers, text, labels, borders or watermark.
Loop the four frames smoothly in the order 1 → 2 → 3 → 4 → 1.

ANIMATION: RUN, using Image 2 (the Run sheet) as the movement template.
Translate its leg silhouettes and four-frame timing into the flame character's short rounded feet.
The motion is energetic, with a clearly wider forward/backward stride than walking and lifted recovery feet.
Frame 1: separated running stride — one foot reaches ahead toward screen-right while the other is lifted behind toward screen-left.
Frame 2: gathered passing pose — the supporting foot passes under the hips while the recovering foot curls behind and comes forward.
Frame 3: opposite-leg running stride — the OTHER foot reaches ahead while the first foot lifts behind; preserve which leg is nearer the viewer.
Frame 4: opposite gathered passing pose — feet pass beneath the hips again and transition smoothly back into Frame 1.
Use the supplied Run poses to make these four silhouettes readable, not a sideways jumping-jack motion. Keep the legs short and soft, and keep the upper body unchanged.
```

## Walk 프롬프트

```text
Use case: identity-preserve.
Asset type: a four-frame transparent sprite sheet for a side-scrolling 2D platformer.

INPUT IMAGE ROLES:
Image 1 is the flame character's EXACT identity, appearance and right-facing three-quarter pose reference.
Image 2 is ONLY an animation pose and sprite-sheet layout reference. Borrow the leg movement and four-frame organization, NOT its wizard character, hat, cloak, colors, dark outlines or white/black backing.

Keep the flame character from Image 1: rounded flame head with the tall curling flame tip, two dark oval eyes with the farther eye narrower, short soft body and limbs, orange-yellow edges, luminous cream core and hand-painted fire texture.
Both head and torso face slightly toward screen-right. No mouth, nose, clothing, accessories, shoes or realistic knees.

Keep the upper body stationary across all four frames. Treat the head, eyes, torso, relaxed resting arms and flame contours as ONE frozen master layer repeated at the same local coordinates. No arm swinging, blinking, torso bobbing, changing lean or animated head flame. Animate only the short rounded flame legs below the hips. Keep each leg's near/far identity and depth consistent.

LAYOUT AND ALPHA:
Match Image 2's simple four-cell horizontal format: EXACTLY FOUR frames, one row, equally sized cells, identical character scale.
Intended canvas 2048 x 512 pixels, four cells of 512 x 512. Cell left edges 0,512,1024,1536. Center each upper body at the same cell-local position, with a uniform 512-pixel stride.
The full character including flame tip and feet must fit in each cell with generous transparent gaps. Intended character height about 440 pixels, contact baseline at y=480. No clipping or overlap.
Genuine transparent alpha background, including all space around the characters. Do not reproduce white areas or black rectangles from Image 2.
No scenery, floor, shadows, background gradient, diffuse glow cloud, checkerboard pattern, grid lines, numbers, text, labels, borders or watermark.
Loop the four frames smoothly in the order 1 → 2 → 3 → 4 → 1.

ANIMATION: WALK, using Image 2 (the Walk sheet) as the movement template.
Translate its small leg angles and four-frame timing into the flame character's short rounded feet.
The motion is relaxed, with a noticeably shorter stride and lower foot lift than running. There is no airborne phase: at least one foot is planted throughout.
Frame 1: small walking contact — one foot slightly ahead toward screen-right, the other slightly behind toward screen-left, close to the ground.
Frame 2: passing pose — the supporting foot is under the hips; the recovering foot lifts only a little and passes close beneath the body.
Frame 3: opposite-leg small walking contact — the OTHER foot is slightly ahead, the first foot slightly behind; preserve each leg's near/far identity.
Frame 4: opposite passing pose — feet pass close beneath the hips again before returning to Frame 1.
Use the supplied Walk poses to make a compact, gentle shuffle with two separate rounded flame feet. Do not make wide running steps, high kicks, hopping or arm movement. Keep the upper body unchanged.
```


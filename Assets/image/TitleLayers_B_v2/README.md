# B안 타이틀 레이어 v2

내장 image_gen으로 background.png를 편집했습니다. 단일 이미지 기반 복원이라 원본과 픽셀 단위 동일한 추출은 아닙니다.

- landscape.png: 태양과 하늘 구름을 제거한 배경. 산과 계곡 안개 포함.
- sun_top.png: 상단으로 옮긴 독립 태양. 투명 배경.
- clouds.png: 하늘 구름 전체를 분리한 독립 투명 레이어. 구름 전체가 함께 움직이는 구성; 서로 다른 속도를 쓰려면 구름 묶음을 추가 분리해야 합니다. 좌우가 끊김 없이 반복되는 타일링 이미지는 아닙니다.
- foreground.png, character.png, logo.png: v1 분리 이미지.

풍경·태양·v1 이미지 캔버스: 1672×941. 구름 캔버스는 생성 결과 1671×941이며 Unity 배치에서 동일 화면 너비로 맞춥니다. 합성 순서: landscape → sun_top → clouds → foreground → character → logo → Unity 메뉴 UI. 캐릭터 발과 절벽 표면 위치는 독립 배치하며 맞춥니다.
태양의 의도는 원래 가로 위치에서 위쪽으로 이동입니다. AI 추출로 실제 가로 위치·크기·후광에도 일부 차이가 있으므로 배치 시 조정할 수 있습니다.
이 작업은 레이어 이미지 준비입니다. 타이틀 씬 연결이나 구름 이동 스크립트는 아직 적용하지 않았습니다.

## 프롬프트

### landscape

Edit target: provided Ember B background. Use case precise-object-edit. Create the clean STATIC landscape layer for independently animated sun/clouds. Remove the sun disc, its local halo, and ALL clouds from the sky. Reconstruct a smooth dark navy-to-dusk-gold sky in their place. Preserve the exact mountain silhouettes, right-hand big mountain placement, distant ridges, lower valley haze, palette, original canvas 1672x941 and pixel-art style. Remove sky clouds only, retain valley fog. No logo, lettering, character, foreground platform, sun or any cloud bands. No composition changes. Opaque full-canvas PNG.

### clouds

Edit target: supplied Ember B background. Use case background-extraction. Extract ONLY every warm pixel-art cloud band in the sky, keeping their precise original locations, colors, size and shapes on the full 1672x941 canvas. Genuine alpha transparency everywhere between clouds. Remove sun and reconstruct just any cloud pixels overlapping it, remove sky gradient, mountain silhouettes and valley fog, all scenery. No solid backdrop, no stars, no sun disc, no text or objects. This is a full-canvas sky CLOUDS overlay to move independently horizontally in Unity. Preserve cloud detail, avoid opaque rectangles or any remaining landscape.

### sun_top

Edit target: supplied Ember B background. Use case precise-object-edit and background-extraction. Isolate ONLY the original partly ash-gray, partly warm-gold sun disc and its subtle nearby glow; remove clouds crossing it and reconstruct disc surface. Move that same sun vertically upward to near the TOP of the original 1672x941 canvas. Preserve original horizontal position: center x approximately 1460 pixels, new center y approximately 100 pixels measured from TOP, radius approximately 50 pixels, fully inside canvas. Preserve original size, texture, ash-gray face and gold crescent rim. Everything other than sun and subtle halo truly transparent alpha. No mountains, no sky, clouds, text, UI, character. Do not enlarge, center horizontally or redesign the sun.


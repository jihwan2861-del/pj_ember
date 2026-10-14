# B안 타이틀 분리 이미지

원본: DriveImport/Handoff/Ember_Session_Handoff_2026-10-14/Images/Ember_TitleLayout_B.png
제작: 내장 image_gen 이미지 편집 도구. 단일 PNG를 바탕으로 추출/배경 복원한 버전이며 원본의 편집 가능한 레이어를 복구한 파일은 아닙니다.

- background.png: 글자·캐릭터·전경 절벽을 제거하고 복원한 불투명 배경.
- logo.png: 투명 배경 EMBER 로고와 구분선.
- foreground.png: 캐릭터를 제거하고 복원한 투명 배경 중앙 절벽.
- character.png: 투명 배경 뒷모습 불꽃 정령. 정적인 아트 참고용이며 실제 게임 플레이어 애니메이션과 별개입니다.

모든 이미지가 동일 캔버스입니다. AI 편집으로 캐릭터와 절벽의 크기·위치 및 로고 세부 형태에 원본 대비 차이가 있으므로 Unity 배치 시 독립적으로 조정하세요. 캐릭터 발 위치를 절벽 상단에 맞춰야 합니다. 메뉴 텍스트는 PNG에 포함하지 않고 Unity UI로 구성합니다.

합성 순서: 배경 → 절벽 → 캐릭터 → 로고 → 메뉴 UI.
현재 요청 범위는 이미지 레이어 분리이며 씬 구성과 폭발/잔상/낙하 연출은 아직 적용하지 않았습니다.

## 사용 프롬프트

### background

Use case: precise-object-edit. Edit target is the provided Ember title layout B. Produce ONLY the clean background layer. Keep exact 16:9 framing, navy/slate/gold dusk palette, right-hand mountain and fading gray sun, distant mountain ridges and cloud placement. Remove ALL text including EMBER and Korean menus, ornamental lines/icons, the flame character, sparks, and the entire bottom center foreground rock platform. Reconstruct the sky behind text and distant landscape behind character and foreground cliff seamlessly. Preserve pixel-art style. No UI, no foreground platform, no character, no typography. Output full-frame opaque background, intended for compositing back with separate layers.

### logo

Use case: background-extraction. Edit target is supplied Ember title layout B. Extract ONLY its gold pixel-art EMBER logo and the fine ornamental divider immediately below it. Preserve original letter shapes, gold/cream colors, pixel details. All sky, scenery, Korean menu labels, character and rocks must be removed. True transparent alpha background. Keep the original 16:9 full canvas and logo at its original upper-center position and scale; do NOT zoom or recenter. No additional text or objects.

### foreground

Use case: background-extraction. Edit target is provided Ember title layout B. Extract ONLY the bottom-center foreground rocky promontory on which the flame character stands. Remove the character, all flame glow particles, text, UI, sky, mountains and sun; restore rock surface behind the removed character naturally. Preserve rock outline, texture, original lighting and original size and location. Keep original 16:9 full canvas, rock platform at original bottom-center extending through bottom edge. Everything else genuine transparent alpha. No scene redesign, no added ground.

### character

Use case: background-extraction. Edit target is provided Ember title layout B. Extract ONLY the small back-facing flame spirit character at lower center, including flame head and body, feet and its few immediate sparks. Preserve precise original proportions and orange gold colors, silhouette and pixel-art style. Remove all rock platform, scenery, lettering and UI. True transparent alpha. Preserve original full 16:9 canvas, original lower-center placement and original small scale. No zoom, no new pose, no cast-shadow terrain.


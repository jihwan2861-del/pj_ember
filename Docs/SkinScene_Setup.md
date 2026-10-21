# Skin Scene — 낙서 지형 스킨 실험

## 2026-10-21 최신 — 공용 지형 Grid·방 진입 Spawn

skin scene의 두 방 지형을 **WorldGeometry/Grid/Terrain·Spikes·Decorations 한 벌**로 합쳤다. `Rooms/SkinRoom_01`과 `SkinRoom_02`에는 CameraBounds·Spawn과 기존 Mechanics/Paths/DoodleBackdrop 등 사용자 오브젝트가 남는다. CameraRoom은 `LevelTilemaps`로 공용 지형을 참조하며, 스킨·타일맵 선택·검사도 이 연결을 사용한다. 새 방은 **Tools > Ember > Level Workflow > 공용 Grid에 방 추가**를 켜서 만든다(기본true). 새 공용 방은 CameraBounds와 Spawn만 생성하며 기존 공용 맵을 재사용한다.

**편집할 때는 WorldGeometry > Grid > Terrain을 선택해 이어 칠한다.** 스킨이 자동 반영되며 가시는 같은 Grid의 Spikes에 그린다. 카메라 경계는 방의 CameraBounds에서 별도로 정한다. 공용 맵 전체로 경계가 늘어나지 않도록 공용 방의 전체 타일맵 범위 맞춤은 비활성이다. 방을 추가한 뒤 Spawn을 안전한 바닥 위에 배치한다. 자세한 순서는 `Docs/LevelEditingWorkflow.md` 최상단을 따른다.

각 CameraRoom의 **Spawn Point가 연결된 경우**, 정상 플레이 중 다음 방에 들어가면 그 Spawn이 체크포인트가 된다. 같은 방 안에서 밟은 중간 체크포인트는 계속 유지한다. 연출·입력 잠금·일시정지 중 진입은 정상 플레이까지 저장을 유예하며 연결되지 않은 기존 방은 동작을 바꾸지 않는다. 예제 프리팹의 Checkpoint_Center/Checkpoint_Finish 두 개만 제거하고 Spawn을 연결했다. 실제 skin scene의 추가 체크포인트는 사용자가 이미 제거한 상태를 유지했다.

병합 때 승인된 **Room_02 전체 (-0.05,-0.12)U 이동**으로 타일 기준을 맞췄다. Terrain575칸+Spikes32칸=607칸, Composite60shape다. 사용자 오브젝트32개는 승인된 이동 외 월드 좌표 변경0개이며 Room_01의 타일맵 tint를 각 셀 색에 곱해 색을 보존했다. 두 방의 활성 스킨에 맞춰 Terrain 렌더 표시와 빈 Room_02 Spikes 표시를 통일했다. 기존 소스 프리팹과 다른 씬의 개별 Grid는 유지한다.

전후 렌더는 `ArtReference/SkinScene/SharedGrid/01_Before_SkinRoom_01.png`·`01_Before_SkinRoom_02.png`와 `02_After_SkinRoom_01.png`·`02_After_SkinRoom_02.png`, 셀/오브젝트 비교는 같은 폴더 `MigrationResult.json`이다. 에디터 확인용 구성에서 생성·재사용·병합·Undo/Redo·셀 색/변환 보존을 확인했고 분리한 실제 런타임 컴포넌트의 체크포인트15항목이 통과했다. 전체 직접 입력 이동 검증은 이번 범위가 아니다.

현재 활성 씬은 dirty 상태를 유지한다. 저장 사본은 `Assets/Scenes/Backups/skin_scene_BeforeSharedGrid_20261021.unity` 및 `skin_scene_AfterSharedGrid_20261021.unity`이며, 활성 씬 자체를 저장한 것으로 취급하지 않는다. 아래의 방별 Grid·111칸 충돌 수정·420셀 기록은 통합 전 기록이다.

## 2026-10-21 Room_02 충돌 수정·밝은 지형 타일

Room_02/Terrain111칸이 `GrayboxBackground`(Collider Type=None)로 칠해져 보이기만 하고 통과했다. 사용자가 이 밝은 타일 모습을 선호해 같은 Sprite/색/Transform/Flags에 Collider Type=Grid인 `Assets/Ember/LevelDesign/Graybox/GrayboxLightSolid.asset`를 만들고 해당111칸만 교체했다. 셀별 색·변환·플래그와 지형 배치는 보존했다. 기존 공유 Background의 충돌은 그대로 None이다.

**흰색/회색은 충돌 여부와 무관하다.** 밝은 지형을 칠하려면 Ember Graybox 또는 Ember Doodle 팔레트의 기존 첫 타일 아래 두 칸 위치(0,-2)에 추가된 `GrayboxLightSolid`를 사용한다. 배경용은 기존 GrayboxBackground를 사용한다.

수정 후 Composite 충돌0→15shape,111칸 모두 Grid, 저장 완료다. Play의 분리된 PhysicsScene2D에서 실제 플레이어 BoxCollider 크기/offset·중력으로 같은 위치에1초 낙하시켜 수정 전 관통/수정 후 접촉·착지를 확인했다. 직접 입력 완주는 아니다. 전후 PreviewScene 캡처는 픽셀까지 같으며 `ArtReference/SkinScene/Room02CollisionFix/Verification.md`에 결과가 있다.

## 2026-10-21 최신 선택 — 초기 굵은 검정 사선 복원

사용자가 과거 캡처의 “사선을 약3배 굵게 하고 짧은 검정 획이 겹치는” 스킨을 다시 선택했다. 당시 작업 기록과 `BoldDiagonal/02_Bold_Dense_Full.png`를 대조해 `Doodle_Stone.mat`과 `Hatching/04_BoldDiagonal.mat`을 복원했다. 아래의 회흑색 채색 텍스처·그라데이션 적용 기록보다 이번 선택이 우선한다.

현재 설정은 **Style4 / Use Painted Hatching 꺼짐 / Spacing .105U / Line Width .075U / Clean Stone Edge .22U / Gradient Width .94U / Solid Core Start .68 / Edge Variation .07U**다. 짧은 굵은 사선이 겹치고 깊은 중앙은 검정으로 채워진다. 셰이더와 기존 타일 편집 자동 반영은 그대로 사용한다. 이전 채색 텍스처는 삭제하지 않았고 복원 전 재질 파일은 `Library/EmberTutorial/BeforeDenseHatchRestore`에 보존했다.

현재 저장된 skin scene의 기존420셀·사용자 배치·충돌·카메라 설정은 바꾸지 않았다. Unity에서 재질을 저장한 뒤 PreviewScene 렌더 `ArtReference/SkinScene/DenseHatchRestored/01_Restored_Full.png`와 `02_Restored_Detail.png`를 직접 비교했다. 셰이더 오류0이며 활성 씬은 clean 상태를 유지했다. 실제 Play 입력 검증은 이번 범위가 아니다.

## 2026-10-21 최신 — Level Workflow 연결

**Tools > Ember > Level Workflow**에서 낙서 스킨룸 생성, 선택 방의 스킨 연결/복구, 스킨 표시 전환, Terrain/Spikes와 Ember Doodle 팔레트 선택을 지원한다. 새 방 기준은0.5U·36×20.25칸이다. 기존 SkinRoom_01도 CameraBounds나 Terrain을 선택하면 인식한다. 새 스타일은 SkinRoom_Example의 채택된 설정을 가져오고 기존 스킨의 사용자 설정·지형·충돌은 유지한다. 기존 CreateRoom 호출은 회색 방을 만들며 새 CreateSkinRoom은 스킨이 연결된 빈 방을 만든다. 자세한 사용 순서는 [방 제작 가이드](LevelEditingWorkflow.md)의 최신 항목을 따른다.

## 최신 보정 — 외곽은 연하게, 중앙만 진하게 (2026-10-20)

사용자가 직전 결과의 해칭 면이 급하게 시작된다고 수정했다. 텍스처는 유지하고, 경계 거리로 **성긴 연한 획→더 많은 진한 획→깊은 검정 중앙**이 되도록 밀도·농도를 각각 전환한다. 회흑색 바탕 전체를 좁은 띠에서 덮던 처리는 제거했다. Coarse mip의 획 밝기와 깊이 제곱으로 드러나는 획 수를 늘리고, smoothstep(.02,.84,inside)로 농도를 늘린다. 실제 텍스처의 선형 밝기10백분위.0117/90백분위.0730을 읽고 기준을 잡았다. 검정 중앙은 질감 밝기와 분리한다.

현재 `Doodle_Stone.mat` / `04_BoldDiagonal.mat`의 **Hatching Gradient Width=1.4U**, **Bold Hatching Solid Core Start=.84**다. 전환 폭을 늘리면 안쪽까지 더 천천히 진해지고, Solid Core Start를 늘리면 검정 중앙이 더 깊은 곳에서 시작한다. Clean Stone Edge.26U와 그림 크기3.6U는 유지했다.

실제 Play 비교는 `ArtReference/SkinScene/HatchingGradient/00_Before_Play.png`(수정 전)와 **`03_Gradient_Final_Play.png`**(수정 후)다. 셰이더 오류 없음을 확인하고 출력을 직접 보며 재촬영했다. 01/02 Detail은 중간 Preview Scene 확대 렌더다. 첫 Preview 전체 렌더와 재질 갱신 직후 Game View에는 채색 미출력/중간 프레임이 있어 최종 결과로 사용하지 않았다. 원래 Storyscene 재생은 중단하지 않았고, 이후 사용자가 재생을 끝낸 clean 편집 상태에서 skin scene을 열어 실제 Play 캡처 후 Storyscene clean 상태로 복원했다. 지형·충돌 코드 수정이나 전체 이동 재검증은 이번 범위가 아니다.

## 최신 — 거친 사선 채색과 검정 중앙 (2026-10-20)

사용자는 굵은 사선 시안과 추가 참조의 **짙은 회색 바탕에 거친 검정 획을 겹쳐 칠한 면 질감**을 요청했다. 첫 절차적 사선은 밝은 돌 위에 매끈한 막대가 따로 떠 보였으므로, `Assets/Ember/Art/Storybook/SkinScene/Hatching_Charcoal_Diagonal_v1.png`의 반복 소재로 수정했다. 사용한 생성 방식은 내장 imagegen이며 입력·생성 지시는 `ArtReference/SkinScene/BoldDiagonal/TexturePrompt.txt`에 남겼다. 반복 경계 전체의 정량 분석이나 기기별 성능 측정은 하지 않았다.

`Doodle_Stone.mat`과 `Hatching/04_BoldDiagonal.mat`에 적용되어 **skin scene과 SkinRoom_Example 프리팹에 바로 보인다.** 원래 기본 재질은 `Hatching/00_Original.mat`에 보존했고 기존01~03은 수정하지 않았다. 아래의 ‘Original(0) 기본’·‘03 유지’는 이 적용 이전 기록이다.

| 재질 항목 | 현재 값 | 역할 |
| --- | --- | --- |
| Hatching Style | BoldDiagonal(4) | 새 음영 방식 |
| Use Painted Hatching | 켜짐 | 거친 붓결 소재 사용 |
| Painted Hatching Texture | Hatching_Charcoal_Diagonal_v1 | 그레이 바탕·사선을 함께 샘플 |
| Painted Hatching World Size |3.6U|큰 값일수록 획이 커짐. 모든 타일에 같은 월드 좌표 사용 |
| Clean Stone Edge |.26U|밝게 남는 외곽 |
| Hatching Gradient Width |1.4U|외곽에서 중앙까지의 전환 거리 |
| Bold Hatching Solid Core Start |.84|정규화한 깊이 기준 검정 중앙이 시작하는 시점 |
| Interior Color |(.003,.003,.003)|깊은 중앙을 거의 검정으로 채움 |

Use Painted Hatching이 켜져 있을 때 사선 굵기·간격은 그림과 World Size로 정한다. 기존 Hatching Spacing/Line Width는 이 옵션을 끈 절차적 획에서 사용한다. 그림에는 지형 모양이나 외곽 그라데이션을 굽지 않았고, Tilemap이 바뀌면 기존 경계 거리로 채색·검정 중앙·외곽선을 다시 만든다.

캡처는 `ArtReference/SkinScene/BoldDiagonal`에 있다.

- `00_Before_Default`: 적용 전 기본 재질의 Preview Scene 렌더.
- `01_Bold_First`, `02_Bold_Dense`: 매끈한 절차적 사선 실험, 최종 결과가 아니다.
- `03_Textured_First`, `04_Textured_Final`: 같은 부분의 전체/확대 Preview Scene 렌더.
- **`06_Before_Original_Play.png` / `07_After_Textured_Play.png`**: 실제 skin scene Play의 Game View 화면 전후. `05_Textured_Final_Play.png`도 최종 재질의 실제 Play 화면이다.

실제 Play에서 셰이더 오류 없음·Style4와 현재 Terrain329셀/충돌20shape를 확인했다. 이번에는 재질·셰이더만 변경했고, 새 이동 완주·타깃 성능 확인은 하지 않았다. Storyscene이 재생 중일 때는 재생을 유지하며 소재만 만들었다. 사용자가 재생을 끝낸 뒤 clean 편집 상태를 확인해 skin scene을 잠시 열어 촬영하고, 원래 Storyscene clean 상태로 돌아갔다. Game View 재질 전환 직후에는 이전 프레임이 캡처될 수 있어 출력 이미지를 직접 확인한 뒤 내용에 맞는 전후 파일로 보관했다.

2026-10-20 최신 선택: 유성펜처럼 굵고 불투명한 획 실험은 사용자 요청으로 취소했다. **03_LooseScribble의 원래 낙서 해칭**(간격0.065U/굵기0.013U)을 복원했다. 복원 확인 렌더는 `ArtReference/SkinScene/ScribbleRestored`에 있다.

## 해칭 스타일 3종 (2026-10-20)

`Assets/Ember/LevelDesign/SkinScene/Hatching`에 교체 가능한 재질3개를 만들었다. 같은 경계 거리·지형에 서로 다른 획을 사용한다.

| 재질 | 표현 | 간격 / 굵기 |
| --- | --- | --- |
|01_FinePen.mat|길이·필압이 다른 짧고 가는 펜선, 깊은 곳에서 교차선 추가|0.076U /0.012U|
|02_RoughPencil.mat|끝이 가늘어지는 거친 연필선·흑연 결, 따뜻한 검정 바탕|0.095U /0.017U|
|03_LooseScribble.mat|짧고 찌그러진 곡선과 S자 획을 겹친 낙서 음영|0.065U /0.013U|

`SkinRoom_01`의 **DoodleTerrainSkin → Fill Material**에 원하는 재질을 넣는다. 각 재질의 **Hatching Style**은 Original / FinePen / RoughPencil / LooseScribble이다. 기존 `Doodle_Stone.mat`은 Original(0)을 사용하며 이번 작업에서 씬의 선택 재질을 확정하지 않았다.

세 스타일 모두 연속 월드 좌표로 그려서 셀 경계에 획이 끊기지 않는다. 선마다 길이·끝부분·굵기·필압을 달리하고 화면 픽셀 크기로 선 가장자리를 처리한다. 곡선안의 첫 시안이 둥근 무늬처럼 보이는 것을 확인해 짧고 불규칙한 획을 더 촘촘히 겹치도록 수정했다.

비교 PNG는 `ArtReference/SkinScene/HatchStyles`에 있다. `_Full.png`는1920×1080 전체 구도, `_Detail.png`는900×900 동일 위치 확대 구도다. 저장된 skin scene을 임시 Preview Scene으로 열어 Unity 카메라로 직접 렌더링했으며 실제 입력 플레이 캡처는 아니다. 현재 편집/재생 중인 씬을 바꾸거나 저장하지 않았고, 미리보기는 렌더 후 닫았다. 각 스타일의 셰이더 오류 없음을 확인했다. 타깃 기기 성능 측정이나 새로운 이동 검증은 이번 범위가 아니다.

## 타일 편집과 외곽선 자동 반영 (2026-10-20)

`SkinRoom_01/Grid/Terrain`을 Tile Palette의 **Ember Doodle**로 칠하거나 지우고, 같은 Tilemap 안에서 타일 선택을 이동한다. 돌 스킨·해칭 음영·윗면 풀·외곽선이 함께 갱신된다. 두 덩어리가 변으로 붙으면 접합면의 선과 풀이 없어져 하나의 지형으로 이어진다. 꼭짓점만 닿는 덩어리는 따로 유지한다.

편집 중 연속 LateUpdate나 Inspector의 delayCall을 기다리지 않도록 EditorApplication.update에서 변경을 반영한다. 브러시 변경은0.05초 간격으로 묶고, Undo/Redo·Grid 크기/간격·배열·Transform·정렬 설정 변경도 감지한다. OnValidate에서는 표시만 하고 객체 생성은 안전한 편집 갱신에서 수행한다.

직접 확인: 중앙 기둥의2×4셀을2셀 아래로 옮겨 아래 지형과 연결했다. 수동 Rebuild 없이 외곽8→7개, 접합면 외곽선0개/풀0개, 연결된 위치의 Composite 충돌=true를 확인했다. Undo8개·Redo7개로 되돌아오는 것도 확인했다. 확인용 이동은 Undo로 복원했으며 최종 씬은 원래329셀·18shape다.

비교는 `ArtReference/SkinScene/TileEdit/02_Automatic_Before.png`, `03_Connected_Automatic.png`, `04_Undo_Automatic.png`에 있다. 00/01은 갱신 대기 문제를 재현한 수정 전 화면이다. **생성된 Doodle Terrain Skin 자식을 직접 움직이지 않는다.** 서로 다른 Tilemap에 그린 두 지형은 자동으로 하나가 되지 않으므로 같은 Terrain에서 편집한다. 스킨은 충돌을 만들지 않으며 native Tilemap/Composite가 지형 충돌을 갱신한다.

꽃 프리팹의 현재 이름은 `Assets/Ember/Prefabs/Tutorial/SunFlower.prefab`이다. 소형/대형을 나누지 않고 탑승 가능한 이 꽃을 태양꽃으로 사용한다.

## 최신 음영 — 해칭 그라데이션 (2026-10-20)

사용자 요청으로 돌이 지형 전체를 채우던 외형을 밝은 돌 외곽에서 짙은 중앙으로 이어지는 해칭 음영으로 수정했다. [Celeste Tilesets, Step-by-Step](https://aran.ink/posts/celeste-tilesets)의 공통 어두운 내부·불규칙한 경계 원리를 참고했으며, 픽셀 그림은 가져오지 않았다.

`DoodleTerrainSkin`은 노출된 경계(구멍 포함)까지의 거리를 각 메시 정점의UV1에 기록한다. `DoodleTerrainInfill.shader`는 이 거리로 돌 무늬를 점차 어둡게 하고 사선·교차선을 겹친다. 선 간격은 유지하면서 깊어질수록 선 굵기와 농도를 높이며, 약한 선 흔들림과 필압 변화로 반듯한 격자 느낌을 줄였다. 얇은 발판에는 큰 검정 덩어리가 생기지 않는다.

음영 설정은 `Assets/Ember/LevelDesign/SkinScene/Doodle_Stone.mat`에서 조절한다.

| 항목 | 현재 값 | 역할 |
| --- | --- | --- |
| Clean Stone Edge |0.35U|밝게 남는 돌 외곽 깊이|
| Hatching Gradient Width |1.1U|안쪽으로 진해지는 전환 폭|
| Hatching Spacing |0.085U|빗금 간격|
| Hatching Line Width |0.014U|기준 선 굵기|
| Interior Color / Hatching Ink |짙은 청흑색 / 검정|중앙 바탕과 펜선 색|

씬·예제 프리팹의 Open Bottom Infill=true, Open Bottom Y=0은 화면 밑으로 이어지는 지형의 하단까지 돌 테두리가 생기지 않게 한다. 충돌체나 점유셀을 늘리는 설정은 아니다.

비교 캡처는 `ArtReference/SkinScene/Infill`에 저장했다. 00_Before는 수정 전, 01_After_First는 단색 중앙 실험, 02_Hatching_First는 첫 해칭, **03_Hatching_Final_Play.png가 최신 실제 Play 화면**이다. 최종 재생에서 Terrain329셀·18충돌shape와 셰이더 오류 없음을 확인했다. 이번 음영 수정 후 점프 전체 완주를 다시 수행한 것은 아니다. 아래의 네 번 점프·가시 복귀 기록은 앞선 지형 확인이다.

완료 기록: 2026-10-20. 씬은 `Assets/Scenes/skin scene.unity`, 방 루트는 `SkinRoom_01`이다. 기존 Level Workflow의 `CreateRoom`으로 방을 만들고, 타일맵 지형에 낙서 스킨을 적용했다. Game View 비교와 실제 방향키·Z 점프 이동을 확인했다. 전체 능력 튜토리얼 통합과 모든 기믹 경로 확인까지 완료했다는 뜻은 아니다.

## 요청과 범위

사용자는 셀레스트 한 테마의 지상·공중 발판·벽·천장 배치를 확인하고, 첨부한 낙서 그림체로 실제 Unity 화면을 만들어 달라고 요청했다. 이번 `Skin Scene`은 이 요청에 따른 별도 실험 장면이다. 기존 Storyscene 강가를 교체하는 작업이 아니다. 사용자가 선호한 기존 자연스러운 강가 외형은 현재 기준으로 유지한다.

목표는 타일맵으로 정확한 이동 지형을 만들면서, 화면에 셀마다 사각 테두리가 반복되는 인상을 줄이는 것이다. 캐릭터의 기본 움직임 수치를 임의로 셀레스트에 맞추거나, 셀레스트의 게임 그림을 복사해 게임 애셋으로 넣는 작업은 포함하지 않는다.

## 제작 규격

| 항목 | 기준 |
| --- | --- |
| 타일 한 칸 | 0.5U |
| Grid | 직사각형, Scale=(0.5,0.5,1), Cell Size=(1,1,0) |
| 한 화면 | 36×20.25칸 = 18×10.125U, 16:9 |
| 카메라 | Orthographic Size=5.0625 기준 |
| 지형 원본 | Terrain Tilemap의 타일 점유 정보 |
| 충돌 | Terrain의 TilemapCollider2D와 Static Rigidbody2D·CompositeCollider2D |
| 그림 | DoodleTerrainSkin이 생성하는 연속 메시·외곽선·풀과 개별 장식 스프라이트 |
| 편집 도구 | 기존 Tools > Ember > Level Workflow와 Tile Palette |

새 생성 메뉴·편집 창은 추가하지 않는다. 기존 방 제작·타일 칠하기 흐름으로 수정할 수 있게 유지한다. 한 화면의 높이가20.25칸인 것은 카메라 비율이며, 타일 하나를 0.25칸으로 자른다는 뜻이 아니다. 화면 위아래에 걸친 지형은 정수 셀로 배치한다.

## 지형과 외형의 분리

`Terrain`을 이동·충돌의 원본으로 삼는다. AI가 만든 그림을 보고 충돌체를 다시 추정하지 않는다. 스킨의 돌 무늬, 잔디, 펜선, 장식은 충돌을 만들지 않는다.

이번 씬은 `Assets/EmberPrototype/Runtime/DoodleTerrainSkin.cs`를 사용한다. 기존 ShoreKit의 `TilemapVisualSkin`으로 셀마다 스프라이트를 복사하는 방식과 구분한다. `Source`의 점유셀을 읽어 연속된 돌 무늬 메시, 노출 외곽의 펜선, 윗면의 풀을 만든다. Terrain 타일이 바뀌면 편집 중과 재생 중 모두 다시 그린다. 생성되는 자식과 메시에는 `HideAndDontSave`를 사용하며 저장 데이터나 충돌체를 추가하지 않는다.

스킨이 켜지면 원본 Terrain 렌더러만 `forceRenderingOff`로 잠시 숨긴다. **Doodle Terrain Skin 컴포넌트를 끄면 그레이박스가 다시 보이고 충돌은 그대로다.** 끄거나 다시 만들 때 기존 렌더러 상태를 복구한다. 생성된 자식을 직접 고치지 말고 Terrain과 컴포넌트 설정을 수정한다.

| Doodle Terrain Skin 설정 | 이번 씬 값 | 의미 |
| --- | --- | --- |
| Texture World Size | 3.2U | 돌 무늬의 반복 크기. 타일별 사각 경계를 숨기는 연속 좌표를 사용한다. |
| Corner Radius | 0.08U | 노출된 바깥 모서리만 얕게 둥글린다. Terrain 충돌은 바뀌지 않는다. |
| Outline Width | 0.045U | 외곽의 검은 펜선 두께 |
| Grass Patch Width | 2U | 윗면을 나누어 배치하는 풀 패치의 기준 너비 |
| Grass Height | 1.1U | 투명 여백을 포함한 전체 스프라이트 높이. 실제 풀 높이1.1U라는 뜻은 아니다. |

풀 스프라이트의 pivot은 그림 속 지면선에 맞췄다. 이미지 하단의 투명 여백을 지면으로 오인해 풀이 뜨던 첫 결과를 이 기준으로 수정했다.

- 보이는 착지 윗면은 실제 충돌 높이를 읽을 수 있게 유지한다.
- 옆면·밑면·바깥 모서리에 작은 굴곡과 비대칭 장식을 두어 사각 덩어리 느낌을 줄인다.
- 장식이 비어 있는 공간을 넓게 덮거나, 실제로 설 수 없는 면을 안전한 발판처럼 보이게 하지 않는다.
- 내부 돌은2~4칸에 걸친 큰 덩어리·균열·색연필 얼룩으로 묶는다. 매 셀에 검은 사각 테두리를 반복하지 않는다.
- 가시와 오염의 위험 경계는 장식과 구분되게 표시한다. 위험물의 위치·방향은 별도 데이터와 충돌을 따른다.

현재 Terrain은 화면 밖 경계벽을 포함해329셀이고 스킨이 읽은 점유 정보도 같은329셀이다. 별도 Art Tilemap의 타일 수를329로 보고한 것이 아니라, 메시 생성에 사용한 원본 셀 집합이 일치한다는 뜻이다. 외곽선과 풀은 장식이며 별도 충돌체가 없다. Terrain Composite는8개 path·18개 physics shape를 유지한다.

## 한 장면에서 비교할 배치

같은 스킨을 지상·공중·벽·천장에 적용해 한 장면에서 비교한다. 첫째와 둘째 공중 발판의 밑면을 비대칭으로 줄이고, 천장에는 홈을 넣어 긴 직사각형 덩어리 느낌을 줄였다.

| 형태 | 화면에서 확인할 것 |
| --- | --- |
| 넓은 시작 선반 | 플레이어와 바닥의 비율, 쉬어 갈 안전 영역 |
| 짧은 지상 단차 | 모서리와 높이 변화가 타일 격자처럼 두드러지는지 |
| 분리된 공중 발판 | 윗면 착지 위치와 비대칭 밑면을 함께 읽을 수 있는지 |
| 긴 수직벽·기둥 | 벽과 주변 배경을 구분하고 실제 경계를 파악할 수 있는지 |
| 천장 돌출부 | 바닥용 스킨을 뒤집은 듯한 부자연스러운 풀·돌 방향이 생기지 않는지 |
| 가시 구역 | 안전한 면과 위험한 면을 즉시 구분할 수 있는지 |
| 넓은 도착 선반 | 공중 이동 뒤 착지 위치를 미리 읽을 수 있는지 |

기존 `StorybookEmber` Animator를 쓰는 플레이어, 대형 태양꽃, 매달린 장작2개를 배치했다. 대형 태양꽃은 직접 접촉 활성화를 켰다. 기능은 기존 프리팹을 재사용하며 지형 스킨과 분리되어 있다. 가시는38셀이고 상·하·좌·우 방향 타일을 제공한다. 가시의 물리는 사각 셀 전체가 아닌 삼각형 윤곽을 사용한다.

## 편집과 재사용

- 씬: `Assets/Scenes/skin scene.unity`의 `SkinRoom_01`.
- 타일: `Assets/Ember/LevelDesign/SkinScene/Tiles`의 `Doodle_Terrain`, `Spike_Up/Down/Left/Right`.
- 팔레트: `Assets/Ember/LevelDesign/SkinScene/Palettes/Ember Doodle.prefab`. Tile Palette 창에서 **Ember Doodle**을 선택한다.
- 재사용 방: `Assets/Ember/Prefabs/LevelDesign/SkinScene/SkinRoom_Example.prefab`.
- 개별 배경 프리팹: 같은 폴더의 `mountain.prefab`, `cloud.prefab`, `sun.prefab`.

Terrain을 칠하면 스킨이 다시 그려진다. 가시는 방향에 맞는 타일로 별도 칠한다. 재사용 방 프리팹에는 방·지형 스킨·기믹이 들어 있고 카메라와 플레이어는 별도이므로, 원하는 씬의 기존 카메라·플레이어와 연결한다. 새 방 생성은 기존 Level Workflow를 사용한다.

## 그림체 기준

사용자 첨부의 손그림 스테이지·타이틀 이미지를 기준으로 한다. 불규칙한 짙은 펜선, 회색 돌, 황금빛 풀, 연한 청보라 산, 흰 종이 배경과 거친 색연필·마커 채색을 사용한다. 픽셀풍이나 이전 ShoreKit 돌 패턴을 확정 외형으로 취급하지 않는다.

배경 산·구름·태양·식물은 개별 애셋으로 배치한다. 게임 화면 전체를 하나의 평면 배경 그림으로 넣지 않는다. 플레이어·불꽃·태양꽃은 주황색 중심의 명확한 색 대비로 찾기 쉽게 한다.

새 그림은 `Assets/Ember/Art/Storybook/SkinScene`에6개 PNG로 반입했다. `Terrain`에는 `Stone_Doodle_Fill.png`, `Grass_Doodle_Edge.png`, `Spike_Doodle.png`, `Backdrop`에는 `Mountains_Doodle.png`, `Cloud_Doodle.png`, `Sun_Doodle.png`가 있다. 재질은 `Assets/Ember/LevelDesign/SkinScene`의 `Doodle_Stone.mat`, `Doodle_Outline.mat`, `Doodle_Sprite.mat`3개다. 개별 그림 생성 후 실제 Unity 장면에 조립해 아래 캡처로 확인했다.

## 셀레스트 참고 캡처

같은 테마인 **Forsaken City A**에서5개 방을 확인했다. 아래 파일은 Berry Camp가 렌더링한 방 지도를 실제 브라우저 확대 보기로 캡처한 것이다. 직접 셀레스트를 플레이하며 촬영한 Game View나 원본 타일 아틀라스가 아니다. 원본 타일 전종을 추출했다는 의미도 아니다.

저장 폴더: `ArtReference/SkinScene/CelesteCityStudy`.

| 파일 | 출처 | 확인한 배치 |
| --- | --- | --- |
| 01_GroundEdges_Room1.jpg | https://berrycamp.github.io/celeste/city/a/1 | 지상 덩어리, 안·밖 모서리, 높낮이, 바닥 가시 |
| 02_FloatingPlatforms_Room3.jpg | https://berrycamp.github.io/celeste/city/a/3 | 공중 덩어리, 연속된 천장, 짧고 긴 벽, 위험 바닥 |
| 03_SteppedGround_Room6.jpg | https://berrycamp.github.io/celeste/city/a/6 | 긴 벽, 단차, 작은 중간 목표와 넓은 연결면 |
| 04_WallsCeilings_Room9b.jpg | https://berrycamp.github.io/celeste/city/a/9b | 천장과 시작·도착면 사이의 큰 빈 공간 |
| 05_GimmickClearSpace_Room4.jpg | https://berrycamp.github.io/celeste/city/a/4 | 한 기믹의 경로를 읽을 수 있게 비운 가운데 공간 |

### 관찰과 적용

지상·공중 지형은 같은 재료의 덩어리로 이어지며, 노출된 윗면·옆면·밑면·모서리에 따라 그림이 달라진다. 밝고 연속된 테두리는 충돌 경계를 읽게 하고, 눈·부스러기·균열은 그 밖에서 반복을 가린다. 공중 덩어리의 밑면은 길이와 모양이 달라 직사각형으로 보이는 것을 줄인다. 기믹 주변의 큰 빈 공간은 이동 경로와 착지 목표를 먼저 읽게 한다.

Ember에서는 이를 낙서 선·돌 무늬·풀 장식으로 번역한다. 셀레스트의 방 좌표·점프 거리를 그대로 복제하지 않고, 기존 앰버 이동 능력에 맞춰 간격을 정한다. 섬세한 조작의 재미는 화면의 장식 밀도보다, 플레이어가 예측한 착지면과 실제 충돌 결과가 일치하는지부터 확인한다.

## Game View 캡처와 확인 결과

자체 게임 캡처 폴더는 `ArtReference/SkinScene/Captures`다. 위 `CelesteCityStudy`의 참고 지도 캡처와 구분한다.

| 최종 파일 | 확인한 내용 |
| --- | --- |
| 09_Graybox_Final.png | 같은 Terrain의 그레이박스 화면 |
| 10_Skin_Final.png | 최종 낙서 스킨 적용 화면 |
| 11_Final_PlayFinish.png | 실제 방향키·Z 입력으로 도착한 화면 |
| 12_Final_HazardReturn.png | 최종 도착점에서 왼쪽 입력으로 가시에 닿아 체크포인트로 돌아온 화면 |

00~08번 파일은 초반 결과와 수정 과정을 남긴 캡처다. 첫 결과에서 풀 그림이 뜨던 pivot을 수정하고, 돌 무늬를 크게 조정했다. 이후 첫째·둘째 발판 밑면과 천장 홈을 다듬어 다시 캡처했다.

실제 재생에서는 방향키와Z만 사용해 네 번 점프했고, 이동 도중 플레이어 Transform을 덮어쓰지 않았다. 착지 위치는 차례대로(5.96,3.40), (8.23,4.40), (12.64,5.40), (15.74,6.39)였고 이 경로에서는 죽지 않았다. 마지막 경로에서 꽃의 직접 접촉 발사1회도 확인했다. 최종 도착점에서는 실제 왼쪽 입력으로 가시 구역에 내려가 사망 상태를 거쳐 체크포인트(16,6.35)로 돌아오는 것을 다시 확인하고12번 캡처를 저장했다. 입력 결과는 같은 폴더의 `InputResults.txt`에 남겼다.

Terrain329셀과 스킨 입력329셀, 실제 셀 간격0.5U, Terrain Composite8paths·18shapes, 가시38셀을 확인했다. 스킨은 원본 충돌을 변경하지 않으며 원본 Storyscene도 변경하지 않았다.

### 남은 확인 범위

X·C를 사용하는 모든 경로와 꽃 불꽃의 오염 정화 과정은 이번 방에서 전체 검증하지 않았다. 전체 빌드, 오프닝·스토리·능력 교육을 하나로 합치는 작업도 이번 결과에 포함하지 않는다. 배치한 태양꽃·가연물이 있다는 사실과 그 기믹의 전체 완주 확인은 구분한다.

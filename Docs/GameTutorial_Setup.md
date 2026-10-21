# 게임 튜토리얼 — 첫 X 구간

2026-10-21 입력 정정: 예전 C 링→X 조준 사격 입력은 제거했다. **X로 가연물을 점화하고 진입**, **X로 꽃 탄환 탑승**이며 C를 먼저 누를 필요는 없다. 두 번째 교육의 C는 태양꽃이 탄환을 발사하게 하는 별도 동작이다. C 사용 후 링 안에서도 X는 불 진입으로 동작한다. 세 번째 C+Z 대시는 유지한다. 안내도 첫 교육 `X로 가연물을 태우고 들어가세요`, 꽃 발사 `C로 태양꽃의 불꽃을 발사하세요`, 탑승 `X로 날아가는 불꽃에 탑승하세요`로 구분했다. 이번 변경의 실제 Play 재완주는 하지 않았다.

2026-10-20 탄생 연출 추가: 새 씬 입장 직후 첫 X 교육을 시작하기 전에 EmberBirthSequence가 작은 불꽃→꽃 반응→앰버 탄생→카메라 복귀를 진행한다. 완료 후 첫 교육을 시작하며 직접 Play/Storyscene 경유 모두 확인했다. 첫 X→방향키Z 교육도 실제 입력으로 재완료했다. 설정·잠금/스킵/재시작 정책은 [탄생 연출](EmberBirth_Setup.md)을 따른다.

## 2026-10-20 최신 — TutorialScene으로 분리

현재 씬은 `Assets/Scenes/TutorialScene.unity`다. Storyscene 나레이션 종료 뒤 물 변화 완료를 기다려 .65초 페이드아웃→새 씬 Single 로드→.4초 페이드인한다. TutorialSceneEntry가 EntryCheckpoint(102.11,.35), PracticeRoom(105.51,2), ortho5.0625로 초기화하고 BeginTutorial을 호출한다. 첫 Exercise는 처음부터 표시하며 물/StorySectionCamera/옛 나레이션 참조는 제거했다. 01→02→03 교육은 새 씬에서 직접 이어진다. 최신 사용자 배치의 첫 루트X96.51/목표X104.51을 보존했다. 실제 세 구간 연속 완료·낙하 리스폰·독립 시작·재시작을 확인했다. 아래 이전의 강가와 직접 연결된 단일 씬 설명은 과거 기록이며 최신 상세는 [계정 인수인계](../Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md)와 `ArtReference/SceneSplit`을 따른다.

2026-10-19 구현. 최신 시안은 `ArtReference/OpeningWater/Concepts/GameplayTutorial_v2`다. 이 설정서는 가연물 X 점화·진입과 Z 이탈·착지를 다룬다. 핵심 오브젝트 외의 지형과 배경은 그레이박스 타일맵으로 편집한다. 이어지는 대형 태양꽃 탑승은 [두 번째 구간](FlowerRide_Setup.md), C+Z 교육은 [세 번째 구간](IgnitionDash_Setup.md)을 따른다.

## 현재 씬 흐름

`Storyscene`의 기존 수면 시듦 나레이션이 끝나면 `FlammableEntryTutorial.QueueBegin()`을 호출한다. 실제 시듦 진행이 1이 될 때까지 기다린 뒤 `GameplayTutorial_01/Exercise`만 활성화한다. 물가 `StoryShoreSection`과 반사는 계속 남아 있다. 나레이션 종료 시 플레이어를 이동시키거나 카메라를 다른 위치로 옮기지 않는다. 기존 `StoryPlayerAppearance.ShowEmber()`로 교육용 앰버 외형을 사용하며, 태양꽃과 반응하는 별도의 탄생 연출을 완성한 것은 아니다.

현재 수면 카메라 구도를 유지하다가 플레이어가 오른쪽 X81.5의 `ApproachRoom`에 들어가면 `CelesteRoomCamera`로 제어를 넘긴다. 기존 방 경계 계산으로 위치와 줌을 약0.8초 동안 이어받는다. 접근 방은 X81.5~96.5(중심89,2), 첫 연습 방은 X96.5~114.5(중심105.5,2)다. 게임 줌은 Size5.0625, 16:9에서36×20.25칸이다. 예전 고정 `GameplayCameraPose`는 제거했다. 원형→물가의 기존 페이드는 이 연결과 별개다.

강둑 끝 (96.82,3) 바로 옆 X96.5부터 회색 타일맵이 이어진다. 처음6개 셀은 높이3→2.5→2→1.5→1→0.5로 내려가며 X99.5부터 첫 X 연습 바닥 높이0에 닿는다. 회색 배경도 X96.5부터 시작해 물가를 통째로 덮지 않는다. 기존 FarShore의 복잡한 그림 외곽 충돌은 걸을 수 있는 평면·경사9점으로 단순화했다.

## 첫 연습

1. 마른 장작 방향을 맞추고 X를 누르면 경로·거리·젖음·활성 상태를 확인한다. 미리보기만으로 불이 붙지는 않는다.
2. X를 확정하면 마른 장작에 불을 붙이고 그 안으로 이동한다. 불 속에 실제 들어가야 Z 안내가 나온다.
3. 방향키와 Z로 빠져나와 오른쪽 발판에 착지한다. 성공한 불 이탈 횟수(`FireLaunchCount`)도 확인하므로 단순 점프 통과나 소화에 의한 강제 이탈로 완료하지 않는다.
4. 성공하면 오른쪽 착지 체크포인트로 갱신한다. 아래 안전 발판에서 다시 시도할 수도 있다.

## Hierarchy와 프리팹

- 씬: `Assets/Scenes/Storyscene.unity`
- 루트: `GameplayTutorial_01`, 위치 (96.5, 0)
- 전체 프리팹: `Assets/Ember/Prefabs/Tutorial/FlammableEntryTutorial.prefab`
- 장작 프리팹: `Assets/Ember/Prefabs/Tutorial/HangingKindling.prefab`
- 핵심 오브젝트 아트: `Assets/Ember/Art/Storybook/Tutorial`
- 그레이박스 타일·팔레트: `Assets/Ember/LevelDesign/Graybox`
- Tile Palette 이름: `Ember Graybox`

현재 지형 구조는 다음을 기준으로 한다. 초기 시안의 AI 돌·잔디 이미지를 늘려 바닥을 만드는 방식에서 전환한 구성이다.

```text
GameplayTutorial_01
└─ Exercise
   ├─ Grid                 Scale (0.5, 0.5, 1), Cell Size (1, 1, 0)
   │  ├─ Terrain          충돌이 있는 회색 지형 Tilemap
   │  ├─ Background       충돌 없는 배경 Tilemap
   │  └─ Art              이후 스킨용 빈 Tilemap, 충돌 없음
   ├─ HangingKindling     점화·진입 핵심 오브젝트
   └─ LandingArea         오른쪽 착지 확인용 Trigger
```

`Terrain` 한 타일맵에 시작 바닥·오른쪽 착지 바닥·아래 재시도 발판을 그린다. 별도 AI 돌 Sprite나 발판별 BoxCollider2D 대신 그린 타일에서 충돌을 만든다. `Background`와 `Art`에는 콜라이더를 두지 않는다. 기존 핵심 장작과 플레이어의 외형은 유지한다.

편집 모드에서는 `Exercise`가 활성 상태라 `Terrain`을 바로 선택해 칠할 수 있다. 재생하면 `FlammableEntryTutorial.hideExerciseUntilBegin=true` 설정이 Awake에서 연습 구간을 숨기고, `BeginTutorial()`에서 다시 활성화한다.

| 구간 | 셀 X | 최상단 셀 Y | 바닥 윗면 월드 Y |
| --- | --- | --- | --- |
| 강둑 연결 계단 | 0~5 | 5~0 | 3~0.5 |
| 시작 바닥 | 6~12 | -1 | 0 |
| 오른쪽 착지 바닥 | 21~39 | 0 | 0.5 |
| 아래 재시도 발판 | 13~20 | -4 | -1.5 |

위 값은 루트 (96.5, 0), Grid 로컬 위치 0과 Scale 0.5 기준이다. 셀 간격은 0이며 직사각형 Grid를 사용한다. 한 칸은 실제 월드에서 0.5U다.

`Terrain`은 같은 오브젝트의 `TilemapCollider2D`, Static `Rigidbody2D`, `CompositeCollider2D`를 사용한다. Tile Collider Type은 Grid, Composite Operation은 Merge, Composite Geometry는 Polygons, Generation은 Synchronous다. 기존 Level Workflow와 같이 Vertex Distance 0.0005, Offset Distance 0.00005, Extrusion 0, Edge Radius 0을 기준으로 한다. 그리기에는 기존 Level Workflow와 Unity Tile Palette를 사용하며 별도 생성 툴은 추가하지 않는다.

정렬은 같은 `EmberWater` 레이어 안에서 Background 3, Terrain 4, Art 5, 장작 9, 플레이어 10을 기준으로 한다. 기존 Global Light 2D의 대상 정렬 레이어에도 `EmberWater`를 추가해 검정색으로 보이던 문제를 수정했다. 새 스킨은 `Grid/Art`에 겹치고, 확정한 타일 충돌과 핵심 오브젝트 판정을 유지한다.

`HangingKindling`의 Collider는 Trigger다. `Art`의 점화 전/후 Sprite는 같은 좌표와 캔버스를 사용한다. `AbsorbPoint`가 진입 지점 (현재 월드 104.5, 2)이다. 장작과 로프 그림은 `BurningRope` 기믹이 아니며 고정된 가연물이다.

`LandingArea`는 착지 확인용 Trigger다. 현재 월드 X107.2~112.6을 포함한다. `EntryTrigger`는 시작 체크포인트를 설정한다.

튜토리얼 전용으로 만들었던 미사용 `TutorialStoneBank.png`, `TutorialStoneBankParts.png`, `RopeLine.mat`는 정리했다. 핵심 가연물의 점화 전·후 이미지 `HangingWood_Unlit.png`, `HangingWood_Burning.png`는 유지한다.

## Inspector 설정

`FlammableEntryTutorial`:

- Player / Target Fire / Exercise Root / Landing Area를 연결한다.
- Hide Exercise Until Begin은 재생 시작 때만 연습 구간을 숨기는 설정이다. 편집 중 타일맵을 칠하기 위해 Exercise를 꺼둘 필요는 없다.
- Water Surface는 수면 시듦 완료 대기에 사용한다. 단독으로 시작할 때는 `BeginTutorial()` 이벤트를 사용한다.
- Cue Canvas / Instruction / Key Label / Keycap Root가 기존 방식의 uGUI 안내다.
- Camera Blend Seconds는 선택적인 최초 조작 잠금 시간이다. 현재 프리팹·씬은0으로 두어 나레이션 뒤 바로 이동할 수 있다. 카메라의 자연스러운 방 전환 시간은 StorySectionCamera의 Gameplay Camera Blend Seconds(0.8)에서 정한다.
- Cue Activation Distance는 대상 근처에서만 X 안내를 표시하는 거리다.
- On Began은 카메라 전환·교육 외형·이전 나레이션 닫기·기존 StoryShoreSection 숨김을 연결한다.
- On Completed는 착지 체크포인트를 연결한다.

프리팹을 다른 씬에 재사용하면 Player, Water Surface 및 카메라 이벤트 등 외부 참조는 그 씬에서 연결한다. `CurrentPhase`, `HasBegun`, `IsCompleted`로 진행 상태를 읽고 `ResetTutorial()`로 교육 상태를 초기화한다.

## 확인한 범위와 남은 확인

그레이박스 전환 전 첫 구현에서는 실제 Play Mode에서 시안과 비교하며 천장 돌의 문구 가림, 끊긴 로프, 잔디의 캐릭터 가림, 완료 후 빈 키 박스를 수정했다. 이후 사용자 결정으로 지형·배경을 그레이박스 타일맵으로 전환했다. 비교 캡처 보관 위치는 `ArtReference/OpeningWater/GameplayTutorial/Captures`다.

새 타일맵에서도 런타임 가상 키보드로 정상 `PlayerAbilityInputRouter`와 물리 이동을 거쳐 첫 구간을 확인했다. X 입력 후 대상 내부 (104.5, 2)에서 `ExitFlammable` 단계로 전환됐고, 방향키+Z 이후 (109.97, 0.83)에 착지해 `Completed`, `FireLaunchCount=1`을 확인했다. 카메라는 36×20.25칸 기준을 유지했다. 가상 키보드는 편집기 입력 시뮬레이션의 상태 분리를 위한 확인 수단이며 프로젝트 입력 방식을 교체한 것이 아니다.

실제 Game View 캡처:

- [점화 전](../ArtReference/OpeningWater/GameplayTutorial/Captures/Graybox_01_Unlit.png)
- [X 점화·진입](../ArtReference/OpeningWater/GameplayTutorial/Captures/Graybox_02_Inside.png)
- [Z 이탈·오른쪽 착지](../ArtReference/OpeningWater/GameplayTutorial/Captures/Graybox_03_Landing.png)

위 확인은 첫 그레이박스 연습 구간의 X→Z 진행과 착지 판정에 한정한다. 전체 원형 오프닝을 처음부터 키 입력으로 완주한 검증은 아니다. 자동 테스트 파일은 추가하지 않았다. 대형 태양꽃·발사체 X 탑승·슬로모션·C+Z 대시 교육은 이번 구현 범위에 포함하지 않는다.

### 물가 연속 연결 확인

사용자 제공 물가 프레임 부근에서 실제 Z로 나레이션을 닫고, 이후 방향키만 입력했다. 대사를 닫을 때 카메라 X 변화0, StoryShoreSection 활성 유지, 조작 잠금·페이드 없음을 확인했다. 평면과 경사로 정리한 강둑을 오른쪽 입력만으로 X79.72→98.25까지 약3.98게임초에 이동했고, 관찰된 최대 한 프레임 X 변화는0.10U였다. 이어 X102.15/Y0.34에서 PracticeRoom과 EnterFlammable 안내까지 진입했다. 이동 도중 플레이어 위치 설정이나 ResetAt 호출은 사용하지 않았다.

이 확인은 물가 프레임부터 첫 X 구간까지다. 해당 프레임을 구성하기 위한 시작 설정과 전체 오프닝 완주를 구분한다. 캡처는 `ArtReference/OpeningWater/GameplayTutorial/ContinuousShore/Captures`의 `00_Before.png`, `01_Narration.png`, `02_AfterZ.png`, `03_RoomEntry.png`, `04_TilemapJoin.png`, `05_FirstExercise.png`다.

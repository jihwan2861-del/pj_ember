# 점화 링 — C → 방향키+Z 대시 튜토리얼

2026-10-20 씬 분리: GameplayTutorial_03는 이제 `Assets/Scenes/TutorialScene.unity`에서 02 오른쪽으로 직접 이어진다. 실제 C→방향키Z 입력으로 IgnitionDashCount1·접지/Completed를 확인했고, 낙하 사망 뒤(145.5,.85)의 새 씬 체크포인트로 복귀했다. 아래 Storyscene 경로는 과거 기록이며 최신은 [계정 인수인계](../Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md)를 따른다.

2026-10-19 구현. [대형 태양꽃 탑승 교육](FlowerRide_Setup.md)의 오른쪽에 이어지는 세 번째 구간이다. `Storyscene` 안에서 걸어서 진입하며 워프나 추가 페이드를 사용하지 않는다.

## 조작과 완료 조건

1. 틈 앞의 짙은 회색 표시까지 걸어간다.
2. C로 점화 링을 켠다. 현재 링 유지 시간은 0.5게임초다.
3. 링이 켜진 동안 오른쪽 방향키와 Z를 새로 눌러 대시한다. 방향키+C+Z 동시 입력도 지원한다.
4. 오른쪽 Landing Area 안 바닥에 착지하면 완료한다.

이번 교육은 기존 점화 링·대시 기능을 사용한다. 대시는 최대 16U/게임초, 가속 0.05초, 지속 0.15초이며 대시 구간 이동은 약 2U다. 별도 슬로모션이나 자동 이동은 추가하지 않았다.

완료 판정에는 다음 조건이 모두 필요하다.

- `LaunchArea` 안에서 점화 링을 사용해 실제 대시 시작에 성공할 것.
- 대시 방향과 `Required Direction`의 내적이 `Direction Dot` 이상일 것. 이번 설정은 오른쪽 / 0.7이다.
- 이후 `LandingArea` 안에서 `CanConfirmCollectible` 지상 판정이 성립할 것.

`FlamePlayerController.IgnitionDashCount`는 링 상태에서 대시 시작이 성공할 때만 증가한다. `LastIgnitionDashPosition`과 `LastIgnitionDashDirection`으로 출발 영역·방향을 확인하므로 일반 X 대시나 점프만으로는 완료되지 않는다. C+Z 동시 입력도 링 상태를 한 프레임 관찰할 필요 없이 성공 기록으로 처리한다.

완료 전 사망하면 해당 시도의 판정을 초기화하고 C 단계부터 재시도한다. 대시 후 목적지에 착지하기 전에 출발 영역에 다시 접지한 경우도 시도를 초기화한다. 완료 상태와 완료 이벤트는 사망 후에도 유지한다. 일시정지·페이드·씬 재시작 연출·조작 잠금 중에는 진행과 안내를 멈춘다.

## 씬 연결

두 번째 `FlowerRideTutorial`의 **On Completed → IgnitionDashTutorial.ArmTutorial()**을 연결했다. 즉시 화면을 바꾸지 않고, Entry Point에 접근하면 시작한다.

| 항목 | 월드 좌표·설정 |
| --- | --- |
| 구간 루트 / 카메라 중심 | (141.5, 2) |
| 카메라 영역 | X132.5~150.5, 화면 18×10.125U |
| 화면 타일 수 / Orthographic Size | 36×20.25칸 / 5.0625 |
| Entry Point / Start Distance | (134, 0.85) / 1.5U |
| 시작 체크포인트 | (135.7, 0.85) |
| 출발 바닥 | X135~139.5, 윗면 Y0.5 |
| 틈 | X139.5~141, 폭 1.5U |
| 착지 바닥 | X141~150.5, 윗면 Y0.5 |
| 천장 | X138~144.5, Y1.5~2.5 |
| Launch Area | 중심 (139.15, 1), 크기 (0.9, 1) |
| Landing Area | 중심 (144, 1), 크기 (5, 1) |
| 완료 체크포인트 | (145.5, 0.85) |

On Began에는 이전 교육의 `DismissCue()`, `CelesteRoomCamera.RefreshRooms()`, 시작 체크포인트 설정을 연결했다. On Completed에서는 완료 체크포인트를 설정한다. 카메라는 기존 CelesteRoomCamera가 인접 CameraRoom을 이어받는다.

## Level Workflow·타일맵 편집

이번 방은 유지 중인 **Tools > Ember > Level Workflow**의 `EmberLevelWorkflowWindow.CreateRoom()`으로 생성했다. 새 생성 툴은 추가하지 않았다.

- 씬 루트: `GameplayTutorial_03`
- 재사용 프리팹: `Assets/Ember/Prefabs/Tutorial/IgnitionDashTutorial.prefab`
- 스크립트: `Assets/EmberPrototype/Runtime/IgnitionDashTutorial.cs`
- 편집 경로: `GameplayTutorial_03/Exercise/Grid`
- Grid: Scale (0.5, 0.5, 1), Cell Size (1, 1, 0)
- 타일·팔레트: `Assets/Ember/LevelDesign/Graybox`, `Ember Graybox`

| Tilemap | 용도 |
| --- | --- |
| Terrain | 바닥·천장·끝 벽. Static Rigidbody2D + TilemapCollider2D Merge + Composite Polygons |
| Spikes | 틈 아래 낙하 위험 판정. Trigger TilemapCollider2D + RoomHazard |
| Background | 충돌 없는 회색 배경 |
| Art | 이후 AI 스킨용으로 비워둔 타일맵 |
| LaunchMark | 출발 위치를 알리는 짙은 회색 표시. 충돌 없음 |

같은 EmberWater 정렬 레이어에서 Background 3 / Terrain 4 / Art·LaunchMark 5 / Spikes 6 / 플레이어 10을 사용한다. 편집할 때 Exercise를 켜고 Tile Palette로 칠한다. 재생 Awake에서는 `Hide Exercise Until Begin`으로 숨기고 교육 시작 때 켠다.

프리팹을 다른 곳에 재사용할 때는 Player와 이전 교육·카메라·체크포인트를 참조하는 이벤트를 해당 씬 대상으로 다시 연결한다. 지형을 수정하면 Launch Area와 Landing Area도 함께 옮긴다.

## IgnitionDashTutorial Inspector

| 필드 | 연결·설정 |
| --- | --- |
| Player | 현재 플레이어 |
| Exercise Root / Hide Exercise Until Begin | Exercise / true |
| Start Point / Start Distance | EntryPoint / 1.5U |
| Launch Area / Landing Area | 해당 이름의 활성 Trigger BoxCollider2D |
| Required Direction / Direction Dot | (1, 0) / 0.7 |
| Cue Canvas / Instruction / Key Label / Keycap Root | 기존 uGUI 안내 |
| Completed Cue Seconds | 1.2초, 실제 시간 |
| On Began / On Completed | 이전 안내 닫기·카메라 갱신·체크포인트 연결 |

`CurrentPhase`, `HasBegun`, `IsCompleted`로 상태를 읽는다. `ArmTutorial()`은 접근 대기, `BeginTutorial()`은 직접 시작 요청이다. `ResetTutorial()`은 학습 상태를 Idle로 되돌린다. 직접 시작은 기존 PlayerEventTrigger에도 연결할 수 있다.

## 현재 확인 범위

- 실제 Play에서 두 번째 교육의 C 발사 → X 탑승 → 오른쪽 이동 → Z 하차 → 정화·착지 완료 뒤, 위치를 쓰지 않고 걸어서 세 번째 구간 (138.95, 0.84)에 진입했다. EnterRing과 세 번째 CameraBounds를 확인했고 카메라 위치는 약 (141.45, 2)였다.
- 실제 C → 오른쪽+Z 입력으로 `IgnitionDashCount=1`, 시작 위치 (139.01, 0.84), 방향 (1, 0)을 확인했다. 이후 (141.56, 0.84)에 접지하면서 Completed가 됐다.
- 최초 Game View 캡처에서 점화 링이 회색 배경 뒤에 가려진 것을 발견했다. PlayerFlameFeedback의 링·액션 입자와 Controller 폭발선이 플레이어의 정렬 레이어 및 Order+10을 따르도록 수정했다. 수정 뒤 실제 C 입력으로 링이 EmberWater 레이어 앞에 보이는 것과 대시 잔상을 다시 캡처해 확인했다.
- 일반 X+오른쪽 대시로 (141.56, 0.84)에 접지해도 EnterRing을 유지했고 성공 카운터는 증가하지 않았다. 방향키+C+Z 동시 입력은 (141.55, 0.84)에 접지하며 Completed가 됐다.
- 대시 없이 걸어서 틈 아래 위험 타일에 닿으면 사망 후 (135.7, 0.85) 체크포인트로 돌아왔다. 조작 잠금이 풀리고 EnterRing에서 다시 시도할 수 있었다.
- 전체 오프닝을 처음부터 끝까지 입력으로 완주한 확인이나 빌드·자동 테스트 검증은 아니다.

최종 캡처는 `ArtReference/OpeningWater/GameplayTutorial/IgnitionDash/Captures`에 저장했다: `01_Ready.png`, `02_Ring.png`, `03_Dash.png`, `04_Completed.png`. 수정 전 비교 자료는 `Before_RingHidden.png`다. 확인용 런타임 입력 장치와 임시 설정을 복원하고 Edit 모드에서 씬을 저장했다.

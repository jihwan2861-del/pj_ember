# 태양꽃 — 불꽃 탑승 튜토리얼

## 2026-10-21 최신 — 애니메이션 불꽃 덩어리

탑승 불꽃 외형은 `Assets/Ember/Prefabs/Tutorial/RideableFlowerFlame.prefab`에서 조절한다. 정적 `FireOrb_Outlined_Doodle_v1.png` 스프라이트는 채택을 취소하고 프로젝트에서 삭제했다. 대신 `Animated Projectile Core` 자식이 `Assets/Ember/VFX/Wallcoeur/Prefabs/ProjectileCore_Animated_Outlined.prefab`를 사용한다. Wallcoeur 3×3 불꽃 플립북을 짙은 외곽( Order38), 주황 덩어리(Order39), 노란 중심(Order40)으로 겹쳤고, 세 레이어 모두 초당38개·수명0.44초·로컬 시뮬레이션이다. 시각 효과가 발사체를 따라가며 프레임과 입자 모양이 계속 바뀐다. Wallcoeur 꼬리는 Order35의 잔불로 남고 연기 렌더는 꺼져 있다.

`RideableFlowerFlame` 루트 SpriteRenderer는 `FireFlowerProjectile.Spawn()`의 sortingOrder 기준이므로 컴포넌트를 남기고 sprite=null/렌더 비활성 상태로 둔다. CircleCast 충돌 반경, 꽃 발사 방향/속도, 탑승 가속·조향, X 탑승·Z 하차·하차 후 직진, 소멸 파티클과 화면 흔들림은 유지했다.

`HangingKindling.prefab`은 불탈 때 `HangingWood_Unlit` 그림을 유지하고 FireGlow 효과만 켠다. 효과의 크기/구성은 `Docs/WallcoeurVFX_Setup.md`의 최신 항목을 따른다.

`ArtReference/WallcoeurVFX/11_AnimatedProjectile_Frame_A.png`~`13_AnimatedProjectile_Frame_C.png`는 Unity PreviewScene에서 서로 다른 시점에 시뮬레이션한 플립북 프레임이며, `14_AnimatedProjectile_InRoom.png`는 실제 TutorialScene 방 구도에 놓은 에디터 렌더다. 실제 입력 Play 확인은 이번에 하지 않았다.

## 2026-10-21 최신 — 불꽃 폭발 화면 흔들림

`Assets/Ember/Prefabs/Tutorial/RideableFlowerFlame.prefab`의 **Fire Flower Projectile → Despawn Screen Shake**에서 조절한다. `Shake On Despawn=true`, `Despawn Shake Duration=0.12`게임초, `Despawn Shake Magnitude=0.05`U다. 값을 낮추면 더 약하고 짧아지며 체크를 끄면 사용하지 않는다.

충돌·사거리·하차 후 수명 종료 시 기존 `Retire()`에서 소멸 파티클과 함께 한 번 호출한다. 같은 씬의 활성 CelesteRoomCamera가 보고 있는 폭발에만 적용하며 카메라 이동 중단·페이드·일시정지 중에는 호출하지 않는다. 기존 대시 등 더 강한 흔들림은 덮지 않는다. 맵 리셋·씬 종료의 `OnDisable`에서는 호출하지 않는다. 소멸 파티클의 현재 연결은 WALLCOEUR의 `FlameBurst_Outlined`이며 아래 FlowerFlameDespawn 기록보다 최신이다.

컴파일과 프리팹 직렬화·정적 경로를 확인했다. 현재 skin 씬의 미저장 편집을 유지했으며 실제 Play에서의 흔들림 체감 확인은 이번에 하지 않았다.

## 2026-10-21 최신 — 탑승 가속과 소멸 불씨

`Assets/Ember/Prefabs/Tutorial/RideableFlowerFlame.prefab`의 **Fire Flower Projectile → Ride Acceleration**에서 조절한다.

| Inspector 설정 | 기본값 | 의미 |
| --- | --- | --- |
| Accelerate While Riding | true | 탑승 가속 사용. 끄면 기존 일정 속도 |
| Ride Start Speed Multiplier | 0.6 | 탑승 직후 속도 / 꽃의 Projectile Speed |
| Ride Max Speed Multiplier | 1.2 | 탑승 최고 속도 / 꽃의 Projectile Speed |
| Ride Acceleration | 4.5 | 게임초당 증가 속도(U/게임초²), 0이면 기존 일정 속도 |
| Despawn Particles Prefab | FlowerFlameDespawn | 정상 소멸 때 재생할 일회성 ParticleSystem |

꽃의 `Projectile Speed=6`일 때 탑승 전에는 6U/게임초이며, X로 타는 순간 3.6U/게임초로 부드럽게 시작해 약0.8게임초 동안 7.2U/게임초까지 가속한다. 방향 입력 유무와 관계없이 탑승 중 가속하며 기존 조향 설정은 유지한다. 하차하면 가속과 조향을 멈추고 그때 속도·방향으로 기존1게임초 직진한다. 최대 탑승2게임초는 유지한다. 이전 일정 속도보다 이동 거리·하차 관성이 늘어나므로 조작감에 맞춰 위 값을 조절한다.

충돌·사거리 종료·하차 후 수명 종료는 기존 멱등 `Retire()`에서 주황 불씨28개를 한 번 재생한다. `FlowerFlameDespawn.prefab`은 기존 Epic Toon FX의 candle_simple_colored 텍스처와 URP 파티클 셰이더를 사용하는 전용 Alpha 재질이다. 흰 배경에서도 색을 유지하며 입자는 축소·페이드되고 시스템은 자동 삭제된다. 파티클 루트는 불꽃과 분리하고 같은 씬·레이어·정렬로 생성해 불꽃 삭제 뒤에도 남는다. `OnDisable`에서는 생성하지 않으므로 MapReset·씬 종료에는 터지지 않는다.

코드/프리팹/재질 저장 및 컴파일 확인. `ArtReference/FlowerRide/AccelerationDespawn/01_Before_EditorPreview.png`, `02_Burst_EditorPreview.png`, `03_Burst_Close_EditorPreview.png`는 저장된 TutorialScene을 PreviewScene에 열어 ParticleSystem을 렌더한 편집기 미리보기다. 최초 캡처의 정렬 레이어 가림과 네모 입자를 수정하고 다시 캡처했다. 실제 X 입력·탑승/하차 완주를 새로 확인한 결과는 아니다. 현재 skin 씬 미저장 편집은 유지했다.

## 2026-10-21 최신 — C 또는 꽃 탄환으로 활성화

- C 폭발의 반응 거리 안에 있는 태양꽃은 기존처럼 머리 로컬+X로 발사한다.
- 다른 태양꽃의 탄환이 꽃 Collider에 맞으면 같은 활성화 경로로 연쇄 발사한다. 자기 꽃이 쏜 탄환은 자기 꽃을 활성화하지 않는다.
- **플레이어 접촉·계속 겹침으로는 발사하지 않는다.** 기존 Activate On Player Contact 필드와 접촉 콜백은 제거했다. 과거 씬/프리팹 YAML의 남은 같은 이름 값은 더 이상 사용하지 않으며, 이를 청소하려고 씬 전체를 다시 저장하지 않았다.
- 준비 중·쿨다운·OneShot 조건은 그대로다. 꽃에 맞은 탄환은 활성화가 거절돼도 종료하므로 쿨다운이 끝나기를 기다려 자동 발사시키지 않는다. 탄환의 기존Retire가 탑승자 안전 이탈도 처리한다.
- 스토리 에너지 전달용 꽃의 `fireProjectileOnActivation=false`와 `onActivated` 이벤트는 유지한다. 플레이어의 일반 불씨 사격은 꽃 탄환으로 취급하지 않는다.

구현은 FireFlower.cs / FireFlowerProjectile.cs 두 런타임 코드이며 씬·프리팹 배치 변경은 없다. 컴파일과 정적 연결 확인 범위이고 이번 실제 C/꽃 간 연쇄 Play 확인은 하지 않았다. 아래의 접촉true·인스턴스false 구분은 이전 정책 기록이다.

2026-10-20 씬 분리: GameplayTutorial_02는 이제 `Assets/Scenes/TutorialScene.unity`에 있다. 01 완료 뒤 걸어서 진입→C/0.2배→X 탑승/1배→Z 하차→정화/Completed→03 진입을 새 씬에서 실제 입력으로 확인했다. 아래 Storyscene 경로는 과거 기록이며 최신 씬·계정 인수인계는 [종합 문서](../Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md)를 따른다.

2026-10-20 명칭 결정: 소형 꽃은 사용하지 않고, 이전 대형 꽃을 **태양꽃**으로 통일한다. 정식 프리팹은 `SunFlower.prefab`이며 이름 변경 전과 GUID가 같아 씬·튜토리얼 연결을 유지한다.

2026-10-19 구현. 첫 [X 점화·진입과 Z 이탈 구간](GameTutorial_Setup.md)에 이어지는 두 번째 교육이다. 실제 Play에서 C 발사 → 슬로모션 → 방향키와 X로 탑승 → Z 하차 → 정화·착지 완료를 확인했다. 별도 열린 공간에서 자동 하차 2초와 이후 직진 1초도 확인했다. 오른쪽에 연결한 C+Z 교육은 [세 번째 구간](IgnitionDash_Setup.md)을 따른다. 별도 탄생 연출은 이 구간의 구현 완료 범위가 아니다.

## 2026-10-20 낙서 애셋 3종 적용 — 사용자 승인 완료

현재 태양꽃 머리·줄기와 접촉 위험 오염은 다음 낙서 그림으로 교체했다. 아래 기존 애셋 표의 이전 PNG 경로는 당시 구현 기록이며, 이번 교체의 최신 경로는 이 표를 따른다.

| 용도 | 최신 경로 |
| --- | --- |
| 머리 | `Assets/Ember/Art/Storybook/DoodleMechanics_v1/SunFlower_Head_Doodle_v1.png` |
| 몸통 | `Assets/Ember/Art/Storybook/DoodleMechanics_v1/SunFlower_Body_Doodle_v1.png` |
| 접촉 위험 오염 | `Assets/Ember/Art/Storybook/DoodleMechanics_v1/Pollution_Thorns_Doodle_v1.png` |

`SunFlower.prefab`의 `Body`·`Head/Petals`와 `FireFlower`의 `wiltedSprite`·`bloomingSprite`에 새 그림을 연결했다. 머리는 1254×1254 / PPU 500 / pivot (0.17,0.5), 몸통은 1024×1536 / PPU 600 / pivot (0.52,0.025)로 가져왔다. 기존 Transform과 Head 로컬 (0,2.4), Muzzle 로컬 (1.5,0), 꽃 Collider 수치를 유지해 Body/Head 분리와 Head 로컬 +X 발사 방향은 같다.

두 번째 교육의 `Pollution`은 기존 Tilemap 12셀과 solid Collider를 유지한다. `TilemapRenderer`만 숨기고 자식 `DoodleArt`로 그림을 표시하며, 자식 `ContactHazard`의 BoxCollider2D trigger와 `RoomHazard`로 접촉 위험을 표시한다. 로컬 중심 (35,4), 위험 영역 로컬 크기 (2.08,6.08)이며 정화·리스폰은 기존 `PollutionObstacle`이 전체를 담당한다.

실제 Play에서 새 머리가 Awake·발사·리스폰 뒤에도 유지되고, 오염 직접 접촉 시 플레이어 `IsDying=true`를 확인했다. 기존 solid는 `isTrigger=false`, 접촉 위험 자식은 `true`이며, 실제 꽃 탄환의 정화 후 `IsPurified=true`와 TilemapCollider2D·BoxCollider2D·SpriteRenderer 비활성화를 확인했다. `ResetForRespawn`은 solid·위험 트리거·그림을 복구하고 TilemapRenderer 숨김과 새 머리를 유지했다. `SunFlower.prefab`뿐 아니라 `FlowerRideTutorial.prefab` 내부의 중첩 꽃도 Body·Petals·wiltedSprite·bloomingSprite 4개 참조가 모두 새 PNG임을 읽기 전용으로 확인했다.

사용자가 이번 적용 화면에 “좋다 다음가자”라고 답해 태양꽃 머리·몸통·접촉 오염 3종은 승인 완료했다. 자동 테스트와 전체 입력 완주를 수행한 결과로 취급하지 않는다. 캡처 및 내장 image_gen의 `GenerationPrompts.txt`는 `ArtReference/OpeningWater/DoodleMechanics_v1`에 보관한다. 다음 중간 발판 마른/푸른 짝과 산 배경 묶음은 검증 중이며 이번 승인에 포함하지 않는다.

## 동작 기준

1. C 또는 다른 꽃 탄환 명중은 꽃을 깨운다. 최초 발사 방향은 배치한 `Head`의 로컬 +X이며 플레이어를 조준하지 않는다. 직접 접촉으로는 활성화하지 않는다.
2. 새 불꽃이 발사되면 튜토리얼에서 현재 재생 배속의 0.2배로 느려지고 X 안내가 나온다. 기본 안내 시간은 최대 실제 시간 2초다.
3. 플레이어가 방향키와 X로 해당 불꽃에 실제 탑승하면 정상 재생 배속으로 돌아온다. 자동으로 탑승시키지 않는다.
4. 탑승 중 방향키로 불꽃을 조종한다. 최초 탑승은 최대 **게임 시간 2초**이며 UI에 남은 시간을 표시한다.
5. Z로 내리거나 탑승 제한 시간이 끝나면 불꽃은 마지막 진행 방향으로 **게임 시간 1초** 더 직진한다. 같은 불꽃에는 다시 탈 수 없다.
6. 불꽃이 오염이나 장애물에 먼저 닿으면 남은 시간보다 충돌이 우선한다. 해당 오염의 꽃 불꽃 정화 조건을 처리하고 탄환을 종료하며 탑승자 안전 이탈을 처리한다.

최초 꽃의 방향과 탑승 후 탄환 조향은 별개다. 탄환을 조종해도 꽃 머리의 배치 방향이 바뀌지는 않는다. 꽃 불꽃은 플레이어를 공격하는 탄환이 아니다.

## 애셋과 프리팹

| 용도 | 경로 |
| --- | --- |
| 태양꽃 | `Assets/Ember/Prefabs/Tutorial/SunFlower.prefab` |
| 탑승 가능한 꽃 불꽃 | `Assets/Ember/Prefabs/Tutorial/RideableFlowerFlame.prefab` |
| 두 번째 교육 전체 | `Assets/Ember/Prefabs/Tutorial/FlowerRideTutorial.prefab` |
| 꽃 머리 | `Assets/Ember/Art/Storybook/Tutorial/SunFlower/SunFlower_Head_v1.png` |
| 줄기·잎 | `Assets/Ember/Art/Storybook/Tutorial/SunFlower/SunFlower_Body_v1.png` |
| 탑승 불꽃 | `Assets/Ember/Art/Storybook/Tutorial/SunFlower/FlowerRide_Flame_v1.png` |

위 PNG는 동화풍 낙서 방향으로 image_gen에서 생성한 이미지다. 생성 원본은 `ArtReference/OpeningWater/GameplayTutorial/FlowerRide`, 프롬프트는 그 아래 `Prompts`에 보관한다. 새 생성 툴은 만들지 않았다.

꽃은 `Body`와 `Head`를 분리한다. `Head`의 로컬 위치는 (0,2.4), `Muzzle`은 Head 아래 로컬 (1.5,0)다. 꽃 머리의 Rotation Z로 최초 발사 방향을 정하며 몸통·줄기·잎은 머리 회전의 영향을 받지 않는다.

- 꽃 프리팹과 모든 인스턴스는 플레이어 접촉으로 활성화하지 않는다. C 또는 다른 태양꽃 탄환 명중만 사용한다.
- 이번 꽃은 재시도할 수 있도록 `One Shot=false`, Cooldown 약 2.5초를 사용한다.
- 이번 불꽃의 발사 속도는 6U/게임초이며 탑승 후 가속은 위 최신 설정을 따른다. Steering Degrees Per Second는 300, Steering Smooth Time은 0.12다.
- 탑승 2초와 하차 후 1초는 `FireFlowerProjectile`의 `MaximumRideSeconds`, `PostRideSeconds` 상수다. Inspector의 튜토리얼 슬로모션 시간과 구분한다.
- 기존 꽃 탄환의 `rideable` 기본값은 false다. 기존 정화 전용 꽃에 탑승 기능을 일괄 적용하지 않는다.

## 씬 연결과 그레이박스 편집

`Storyscene`에서 첫 `FlammableEntryTutorial`의 On Completed를 두 번째 `FlowerRideTutorial.ArmTutorial()`에 연결한다. 첫 완료 즉시 두 번째 교육 화면으로 바꾸지 않고, 플레이어가 Start Point에 접근한 뒤 시작한다.

- 두 번째 구간 루트: (114.5,0)
- Start Point: (117.5,0.85), Start Distance: 4.5U
- 첫 구간 착지 위치 약 X109.97에서는 대기하고, 약 X113 이후 접근하면 시작한다.
- 두 번째 카메라 중심: (123.5,2), Orthographic Size 5.0625
- 화면: 18×10.125U, 36×20.25칸, 16:9
- 꽃 위치: (118.5,0.5), 최초 Muzzle 월드 위치: (120,2.9)
- 오른쪽 바닥: X127.5~135, 윗면 Y0.5 / 착지 판정: X128~132
- 오염 벽: X131.5~132.5, Y0.5~3.5

Grid는 Scale (0.5,0.5,1), Cell Size (1,1,0)이다. `Terrain`, `Background`, 빈 스킨용 `Art`, 낙하 판정용 `FallHazard`, 정화 대상 `Pollution`을 독립 Tilemap으로 둔다. 충돌·위험·정화 판정은 각 용도에 맞는 타일맵에서 편집한다. Terrain은 기존 Level Workflow의 Static Rigidbody2D·TilemapCollider2D Merge·Composite Polygons 구성을 따른다.

편집 중에는 Exercise를 켜고 칠한다. 재생 Awake에서는 Hide Exercise Until Begin으로 숨기고 교육 시작 때 활성화한다. On Began에는 이전 안내 닫기·CameraRoom 갱신 등을 연결하며 시작 체크포인트는 (117.5,0.85)를 사용한다. 새 씬을 만들지 않는다.

## FlowerRideTutorial Inspector

| 필드 | 연결·설정 |
| --- | --- |
| Player / Flower | 현재 플레이어와 이번 대형 꽃 |
| Landing Area / Pollution | 오른쪽 착지 Trigger와 정화 대상 `PollutionObstacle` |
| Exercise Root / Hide Exercise Until Begin | 교육 배치 묶음, 기본 true |
| Start Point / Start Distance | (117.5,0.85)의 시작점, 4.5U |
| Cue Canvas / Instruction / Key Label / Keycap Root | 기존 uGUI 안내 |
| Slow Motion Scale / Slow Motion Real Seconds | 0.2 / 2초(실제 시간) |
| On Began / On Completed | 시작 전환과 완료 후 체크포인트·다음 교육 연결 |

완료 조건은 이번 꽃 불꽃의 실제 탑승, 이후 이탈, 오른쪽 Landing Area 안 지상 착지, `PollutionObstacle.IsPurified`다. 상태는 `CurrentPhase`, `HasBegun`, `IsCompleted`, `IsPassageClear`, `IsSlowMotionActive`로 읽는다. `BeginTutorial()`을 기존 PlayerEventTrigger에 직접 연결할 수도 있다.

실패 탄환이 종료되거나 완료 전 사망하면 C 단계부터 재시도한다. 학습 완료 이벤트는 한 번 유지하되 맵 초기화로 오염이 복구되면 근처에서 C 정화 복구 안내를 표시한다. 학습 완료 상태와 현재 길의 정화 상태는 구분한다. `ResetTutorial()`은 학습 상태를 처음으로 되돌린다.

슬로모션은 이 교육에서만 소유한다. GameSessionControls의 일시정지·배속 변경·재시작과 FlameShotAbility의 조준 시작 전 `FlowerRideTutorial.EndSessionSlowMotion()`을 호출해 다른 시스템이 느린 시간을 기본 배속으로 저장하지 않게 한다. 자기 소유 값과 일치할 때만 원래 배속을 복원하며 fixedDeltaTime은 변경하지 않는다.

## 현재 확인 범위

- 실제 Play에서 정상 입력 라우터를 거쳐 C 발사, 0.2배 슬로모션, 우상단 방향+X 탑승, 정상 배속 복귀를 확인했다. 오른쪽으로 이동한 뒤 Z로 하차해 불꽃이 오염을 제거하고 플레이어가 (130.12,0.84)에 착지하면서 Completed/IsPassageClear=true가 됐다.
- Z 하차 순간 AfterRideSecondsRemaining=1, 0.12게임초 뒤 0.88을 확인했다. 불꽃 방향은 (1,0), 플레이어 이탈 수평 속도는 6U/초였다.
- 장애물이 없는 별도 열린 공간에서 자동 하차는 1.999999게임초, 소멸은 2.999999게임초로 관찰됐다. 방향키 선회 각도는 약44.1도까지 변화했고, 하차 뒤 반대 방향 입력에도 불꽃의 추가 회전은 0도였다. 고정 물리 간격은 0.02초다. 부동소수점 잔여값으로 한 틱 늦던 하차 경계를 보정했다.
- 튜토리얼 슬로모션 중 Pause/Resume에서 timeScale이 0.2→0→1로 돌아오고 슬로모션 소유권이 해제되는 것을 확인했다.
- 전체 오프닝의 처음부터 끝까지 키 입력 완주·빌드·자동 테스트 완료를 뜻하지 않는다.

실제 Game View 캡처는 `ArtReference/OpeningWater/GameplayTutorial/FlowerRide/Captures`에 있다: `01_Ready.png`, `02_Fired.png`, `03_Boarded.png`, `04_Dismount.png`, `05_Completed.png`. 요약 기록은 같은 상위 폴더의 `Verification_2026-10-19.md`를 따른다.

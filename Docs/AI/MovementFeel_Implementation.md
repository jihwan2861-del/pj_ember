# 2씬 플레이어 조작감 개선 적용

적용일: 2026-10-04

방향키 + Z/X/C, 불꽃 점화·흡수·발사라는 현재 게임 방향에 맞춰 기존 컨트롤러를 수정했다. 이동 속도 3.3과 점프 설정 6.09는 유지했다. 수치 조정은 2씬 인스턴스 override로 적용했으며, 입력 처리·운동량·대시 안전 복귀는 공유 컴포넌트의 변경이라 다른 씬에서도 적용된다.

## 적용한 동작

- 걷기는 좌우 키로 -1/0/+1을 읽는다. 위·아래 조준을 함께 눌러도 수평 최고 속도가 줄지 않는다.
- 키 유지·놓음은 이동 상태와 별개로 갱신한다. 키보드가 없어지면 이동 입력과 점프 예약을 비운다.
- C+Z 동시 입력은 점화 후 대시로 이어진다. 고리의 선택 시간은 기존 0.3초를 유지한다.
- 대시 끝나기 직전 누른 Z는 기존 0.12초 점프 버퍼로 이어진다. 대시 시작 Z 자체는 다음 점프로 재사용하지 않는다.
- 추가 공중 점프는 유지한다. 낙하 중 약 0.07초 안에 발판에 닿을 수 있으면 착지 점프를 먼저 예약하고, 먼 바닥에서는 즉시 공중 점프한다.
- 접지는 자기 Rigidbody의 Collider를 제외하고 위쪽 접촉 법선이 있는 지면만 인정한다.
- 불 발사·대시로 일반 속도를 초과했을 때 같은 방향 입력은 별도 추진 감속 8을 적용한다. 무입력은 공중 제동, 반대 방향은 기존 방향 전환 제동을 적용한다.
- 발사 후 조작 잠금 중에도 반대 방향 제동을 허용한다.
- 대시는 입력한 방향을 유지한다. 기존 최대 ±66도 자동 회전은 제거했으며 관련 기존 serialized 필드는 값을 보존하고 Inspector에서 숨겼다.
- 대시 종료 시 본체 BoxCollider가 들어갈 공간을 검사한다. 막힌 경우 마지막 안전 위치로 돌아가고, 저장된 위치가 모두 막힌 경우 리스폰한다.
- 불씨 사격 후 Free로 돌아오는 첫 물리 갱신에서도 현재 누른 수평 방향을 유지한다.

## 2씬 설정

| 설정 | 기존 | 적용 |
|---|---:|---:|
| moveSpeed | 3.3 | 3.3 |
| jumpSpeed | 6.09 | 6.09 |
| airDeceleration | 14 | 28 |
| apexVelocityThreshold | 0.5 | 1.0 |
| apexGravityMultiplier | 0.85 | 0.65 |
| fallGravityMultiplier | 1.3 | 1.6 |
| fireLaunchControlLockTime | 코드 상수 0.25초 | 0.12초 |
| burstDashExitControlLockTime | 코드 상수 0.08초 | 0.02초 |

지상 가속·제동 65/85, 공중 가속 22, 코요테·점프 버퍼 0.12초, 추가 공중 점프 1회는 유지했다. 점프 정점과 낙하 변경 때문에 체공 시간과 도달 거리는 이전과 다를 수 있다.

## 애니메이션 처리

AnimatorDriver는 Inspector의 별도 runSpeed보다 현재 플레이어의 NormalMoveSpeed를 우선 사용한다. 2씬의 3.3 속도에 맞춰 지상 Speed를 정규화한다.

기존 달리기·점프 클립은 Sprite 원본 GUID가 누락돼 있었다. 현재 픽셀 캐릭터 `ember_Idle.png`에는 Idle 6프레임만 있고 전용 달리기·점프 포즈가 없다. 빈 BlendTree 슬롯은 정상 Idle 클립으로 연결해 이동 중 Sprite가 사라지지 않게 했다. 전용 달리기·점프 포즈를 복구한 작업은 아니다.

Jump 자동 설정은 빈 슬롯만 복구하고 기존 다른 Motion을 보존한다. Sprite 프레임이 깨진 클립은 연결하지 않는다.

## 변경 파일

- `Assets/EmberPrototype/Runtime/FlamePlayerController.cs`: 입력 표본화, 착지 예약, 추진 감속, 조작 복귀, 접지 판정, 대시 안전 리스폰.
- `Assets/EmberPrototype/Runtime/PlayerAbilityInputRouter.cs`: 같은 프레임 C+Z 연결.
- `Assets/EmberPrototype/Runtime/BurstDashAbility.cs`: 대시 방향 유지와 본체 Collider 안전 복귀.
- `Assets/EmberPrototype/Runtime/PlayerAnimatorDriver.cs`: 실제 이동 속도 기준으로 애니메이션 정규화.
- `Assets/EmberPrototype/Editor/PlayerJumpAnimationSetup.cs`: 빈 슬롯 복구와 깨진 Sprite 참조 보호.
- `Assets/player/new_Player/animaition/New_Player_Animator.controller`: 빈 이동·점프 슬롯의 유효 Idle fallback.
- `Assets/Scenes/2.unity`: 6개 조작감 설정 override.

## 검증 기록

런타임·에디터 두 어셈블리는 생성된 프로젝트를 별도 출력 경로로 컴파일해 오류 없이 통과했다. Unity 6000.5.2f1의 격리 EditMode 입력·Physics2D 회귀 테스트는 **17개 모두 통과**했다. 2씬의 최종 수치와 본체 Collider 크기·offset을 사용했다.

검증에는 대각선 걷기, C+Z 동시 입력, 대시 종료 직전 점프 예약, X 흡수→앵커→Z 발사, 같은 방향·무입력·역방향 제동, 착지 예약과 공중 점프, 방향 유지와 Collider 복귀, 막힌 대시의 입력 중복 소비 방지, 사격 종료 직후 이동이 포함된다. 키 이벤트가 실제로 접수됐는지 전제 조건을 검사했고 예상외 오류 로그를 무시하지 않았다.

결과는 `ValidationArtifacts/MovementFeel/UnityResults.xml`, 컴파일 로그는 `dotnet-compile-final.log`, 복사한 소스의 해시는 `source-snapshot.json`에 보관한다. 첫 실행에서 발생한 EditMode 입력 어댑터 실패 기록은 최종 통과 결과와 별도로 남겼다. 열린 원본 프로젝트와 충돌하지 않도록 별도 임시 Unity 프로젝트에서 검증했으며 원본 Library·생성 프로젝트 파일·ProjectSettings는 수정하지 않았다.

이 17개 테스트에는 실제 2씬 Tilemap 바닥이 포함되지 않았다. 이후 보고된 평지 걸림은 별도 조사에서 재현했고, 지형 합치기와 4개 추가 회귀 테스트로 수정했다. 아래 후속 수정과 `Tilemap_GroundSnag_Investigation.md`를 참고한다. 실제 2씬 전체의 손 플레이와 렌더링은 검증하지 않았다. 애니메이션 GUID·Sprite fileID·씬 override는 정적 검증했으며, 현재 자동 테스트의 통과는 사람이 느끼는 손맛과 전체 레벨 클리어를 보장하는 결과가 아니다.

직접 확인할 때는 Unity에서 2씬의 최신 저장 설정을 불러온 뒤, 1타일 발판 착지, C+Z 동시 입력, 대시 종료 직전 Z, 불 발사 후 같은 방향·무입력·반대 방향을 비교한다. 점프 거리 변화에 따른 기존 퍼즐의 발판 간격은 플레이 테스트로 조정할 수 있다.

## 후속 수정: 평지의 타일 경계 걸림

사용자가 평평한 바닥을 걷는 중 걸리는 현상을 보고해 실제 2씬 지형을 Unity에서 불러와 수정 전후를 비교했다. TilemapCollider에는 Merge만 설정되어 있었고 연결할 Rigidbody2D와 CompositeCollider2D가 없어 셀 경계가 합쳐지지 않았다. 타일 내부 경계에서 수평 접촉이 생기며 속도가 0이 되는 현상을 재현했다.

2씬 Tilemap에 Static Rigidbody2D와 Polygons/Synchronous CompositeCollider2D를 추가했다. 기존 타일 배치, Merge와 extrusion 0, 플레이어 모양과 마찰은 보존했다. 같은 평지에서 100 tick 걷기 결과는 수정 전 1.017 U 후 정지 83 tick, 수정 후 6.261 U 진행·정지 0 tick이었다. 착지 후 걷기와 실제 벽에서 대시 종료·본체 복귀도 포함한 신규 지형 테스트 4개가 통과했다. 상세 기록은 `Tilemap_GroundSnag_Investigation.md`와 `ValidationArtifacts/MovementFeel/TilemapSticking`에 있다.

2026-10-05에는 이후 저장된 최신 751개 셀 씬에 동일한 두 컴포넌트만 다시 적용했다. 삭제된 두 타일과 인접 Sprite 갱신을 포함한 최신 편집은 모두 보존했고, 두 컴포넌트 추가를 되돌리면 해당 원본과 바이트 단위로 일치함을 검증했다. 최신 native 지형 테스트도 4개 모두 통과했다. 강제 Composite.GenerateGeometry 호출 없이 OpenScene 직후 자동 생성되며 최신 지형은 5개 경로·80개 점으로 합쳐졌다. 이전 753개 셀 결과는 `TilemapSticking-753Cells`에 별도로 남겼다.

## 변경 전 기록

- Controller와 Router 변경 전 파일은 `ValidationArtifacts/MovementFeel/before-root`에 저장했다.
- BurstDash 변경 전 파일은 작업 폴더의 `_Archive/Movement_20261004`에 저장했다.
- 씬·Animator·Driver·Editor 설정의 이번 패치 이전 상태는 `Docs/AI/Backups/2026-10-04-feel-before`의 재구성본과 기록을 참고한다.

기존 사용자 변경은 유지하며 이번 작업에서 Git reset, 커밋, 외부 배포는 수행하지 않았다.

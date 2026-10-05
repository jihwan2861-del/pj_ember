# 움직이는 발판에서 떨어지는 문제

2026-10-05 / Unity 6000.5.2f1

## 재현과 원인

원본 PathMovingBlock에는 경로 이동만 있고 플레이어 운반 처리가 없었다. FlamePlayerController는 입력이 없으면 자신의 수평 속도를 0으로 감속했다. 상승 중인 발판이 플레이어를 밀어 올리면 `body.linearVelocity.y <= 0.1` 조건 때문에 접지도 풀렸다.

실제 플레이어 컨트롤러·실제 경로 블록·네이티브 Physics2D로 재현했다. 원본 코드의 좌우 이동·상승·하강·원형 이동 5개 케이스 중 4개가 실패했다. 좌우 발판 이동 120 프레임 후 상대 X 오차는 약 ±4.707 U, 원형 이동은 17번째 프레임에서 허용 상대 X 오차를 넘었다. 상승 발판은 접지 판정이 false였다. 하강 직선은 해당 속도에서 정상 접촉을 유지했다.

## 수정

- PathMovingBlock을 플레이어보다 먼저 실행하고, 최종 목적지와 현재 위치의 차이로 이번 물리 프레임의 `StepVelocity`를 제공한다. 대기·정지·Reset·비활성화 때는 0으로 갱신한다.
- 플레이어는 발 아래의 가장 가까운 유효 고체를 선택한다. PathMovingBlock 위에 있다면 이전 프레임에 추가한 운반 속도를 분리해 걷기·접지·점프를 계산한 뒤 이번 발판 속도를 한 번 추가한다.
- 점프 때 운반을 해제한다. 리스폰과 점화·불 이동·발사 전환에서도 이전 운반 속도를 초기화한다.
- 플레이어를 부모로 붙이거나 Transform으로 순간 이동시키지 않는다. 실제 Dynamic Rigidbody2D의 속도와 기존 물리 충돌을 사용한다.
- 기존 직렬화 필드, GUID, 씬·프리팹·프로젝트 설정을 변경하지 않았다. 원본 코드 백업은 `Docs/AI/Backups/2026-10-05-platform-rider-before`에 있다.

## 검증

별도 임시 Unity 프로젝트에서 원본 Runtime 26개의 정확한 복사본을 컴파일했다. 최종 결과는 **52 passed / 0 failed / 0 skipped**다.

- 운반 회귀 10개: 좌우·상하 이동, 원형 경로 속도 2/8 U/s의 방향 전환과 순환, 발판 위 상대 걷기 속도, 점프·리스폰, 끝점 정지, 벽 통과 방지.
- 기존 입력·이동·타일맵 회귀 21개.
- 기존 방 편집·카메라·경로 도구 회귀 21개.

걷기 속도 검증에서는 통제된 마찰 0 환경과 실제 위치 변화량을 사용한다. MovePosition을 사용하는 발판의 Rigidbody2D.linearVelocity는 네이티브 시뮬레이션 후 0으로 돌아오므로 그 값을 이동 속도의 측정 기준으로 쓰지 않는다. 기본 마찰 환경에서의 서 있기·원형 운반은 별도로 통과했다.

검증은 EditMode에서 실제 컴포넌트와 네이티브 물리를 직접 진행한 결과다. 사용자가 편집 중인 씬을 실제 Game 화면에서 플레이한 결과나 실행 파일 빌드는 아니다. 발판 자체가 기울어 회전하는 기능, 발판 점프 관성, 압사 처리는 이번 수정 범위 밖이다.

- [수정 전 결과](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ValidationArtifacts/MovementFeel/PlatformRider/Before/UnityResults.xml>)
- [최종 결과](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ValidationArtifacts/MovementFeel/PlatformRider/Final/UnityResults.xml>)
- [실행 소스 해시](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ValidationArtifacts/MovementFeel/PlatformRider/Final/source-snapshot.json>)
- [운반 회귀 테스트](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Validation/Movement/MovingPlatformRiderTests.cs>)

현재 준비된 임시 프로젝트에서 다시 검증하려면 프로젝트 루트에서 실행한다.

```powershell
& './Validation/Movement/Run-IsolatedRegression.ps1' -TestFilter 'Ember' -ArtifactSubdirectory 'PlatformRider/Final'
```

이 명령의 52개 전체 검증은 기존 방 편집 테스트·Editor 파일과 타일맵 fixture가 준비된 임시 프로젝트를 사용한다. 새 임시 프로젝트를 만들 때는 기존 LevelWorkflow·Movement 검증 준비 절차도 먼저 수행한다.

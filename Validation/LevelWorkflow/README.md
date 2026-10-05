# Level Workflow 검증

Unity 6000.5.2f1의 별도 임시 프로젝트에서 실제 Runtime 소스 26개와 새 Editor 소스 4개를 컴파일하고 EditMode 테스트를 실행한다. 테스트 코드는 `Assets` 밖에 보관하며 실제 게임 프로젝트에 테스트 패키지나 asmdef를 추가하지 않는다.

## 실행

프로젝트 루트에서 PowerShell로 실행한다.

```powershell
& './Validation/LevelWorkflow/Run-IsolatedValidation.ps1'
```

스크립트는 숨겨진 Unity 프로세스를 시작하고 PID를 반환한다. 기본 QA 프로젝트는 `C:/Users/admin/AppData/Local/Temp/EmberMovementQA-20261004`다. 기존 게임의 Unity 창을 종료하거나 실제 씬을 저장하지 않는다. 같은 QA 프로젝트를 이미 열어 둔 상태에서는 중복 실행하지 않는다.

결과는 `ValidationArtifacts/LevelWorkflow/UnityResults.xml`, 로그는 `isolated-unity.log`에 기록된다. `source-snapshot.json`은 실행 시 실제 소스와 복사본의 SHA-256, 2씬의 SHA-256을 기록한다. XML이 생성되고 모든 테스트의 result가 Passed인지 확인한다. 실행 직전에 이전 XML을 제거하므로 이전 성공 결과가 새 결과처럼 남지 않는다.

Fixture는 LocalApplicationData/Temp 아래의 Batch Mode 프로젝트에서만 실행된다. 격리 프로젝트의 초기 미저장 씬을 빈 테스트 씬으로 교체하므로 게임 프로젝트에서 직접 실행하지 않는다.

## 확인 범위

- 방 Hierarchy, 0.5 U 타일 크기, Terrain의 Static/Composite/Merge, Spikes의 Tilemap Trigger, Decorations의 충돌 없음.
- 그린 지형의 자동 Composite 생성, 가시가 없는 원점에 위험 박스가 생기지 않음, 기존 Box 위험물의 자동 Collider 유지.
- 방 생성 Undo/Redo와 복구된 Collider 설정, 카메라 화면 크기 맞춤, 실제 채워진 셀의 월드 범위 맞춤과 Undo.
- 카메라 연결 시 기존 Target과 Transform 보존, Starting Room 연결과 Undo.
- 기존 Linear/Smooth enum과 열린 경로, 정확한 원의 반지름·방향, 닫힌 Smooth 연결, 잘못된 점 처리.
- Once/자동 출발/Reset, PingPong 끝점 대기, Circle Loop와 남은 시간, 열린 Loop 거부.
- 실제 `Physics2D.Simulate`에서 끝점 정지·대기 중 위치 안정성, 도착 이벤트에서 Reset의 우선권.
- 경유점 추가·삭제 Undo/Redo, 고스트 미리보기 계산이 실제 블록을 이동시키지 않음.

## 검증 한계

EditMode에서 실제 컴포넌트의 Awake/FixedUpdate를 호출하고 네이티브 Physics2D를 진행한다. Scene 뷰 핸들을 마우스로 드래그하는 UI 동작, Game 화면 렌더링, 모든 기존 씬을 직접 플레이한 결과는 포함하지 않는다. 실행 파일 빌드도 수행하지 않는다.

플레이어 운반·발판 점프 관성·압사 처리는 이번 기능에 추가하지 않았다. 가시 그림/Tile 에셋도 별도로 준비해야 한다. 사용 순서는 [방 제작 가이드](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Docs/LevelEditingWorkflow.md>)를 참고한다.

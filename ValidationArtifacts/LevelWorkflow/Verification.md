# Ember Level Workflow 검증 결과

2026-10-05 / Unity 6000.5.2f1 / Windows / 격리 임시 프로젝트의 Batch EditMode 실행.

최종 결과: **21 passed, 0 failed, 0 skipped**. 새 편집 도구의 컴파일과 실제 네이티브 Physics2D·Tilemap·Undo 검증을 통과했다. 원본 Runtime 26개, Editor 4개, 테스트 소스 2개의 실행 복사본과 최종 작업 파일 SHA-256이 일치한다.

## 확인한 결과

- `Tools > Ember > Level Workflow`에서 생성하는 방에 CameraBounds, Grid/Terrain·Spikes·Decorations, Mechanics, Paths, Spawn이 포함된다. 기본 셀 크기는 0.5 U다.
- Terrain에 그린 타일이 Static Rigidbody2D + Composite Polygons/Synchronous + Merge로 자동 결합된다. 생성 Undo/Redo 후에도 충돌 설정과 셀 크기가 복구된다.
- Spikes는 실제 Tilemap Trigger를 사용한다. 빈 원점에 별도 위험 Box가 생기지 않으며, 기존 Box 기반 위험물도 Collider를 유지한다.
- 카메라 화면 크기 맞춤은 중심을 유지한다. 타일맵 맞춤은 실제 채워진 셀의 월드 범위를 사용하며 Undo가 작동한다. 카메라 연결은 기존 Target과 위치를 보존하며 시작 방을 연결한다.
- 기존 Linear/Smooth·Once, Circle, 닫힌 Smooth, PingPong·Loop, 대기, 자동 출발, Reset, 도착 이벤트를 검증했다. 짧은 경로에서도 정확한 도착 프레임을 사용한다.
- 네이티브 물리에서 Once 종료·반복 대기 중 위치가 안정적이며, 도착 이벤트의 Reset이 같은 프레임 이동보다 우선한다. 경유점 Undo/Redo와 고스트 미리보기의 실제 위치 보존을 확인했다.

## 보존

기존 Runtime 직렬화 필드명과 Linear=0/Smooth=1 enum 값을 유지했다. 기존 Runtime `.meta` GUID는 보존했고 새 Editor GUID 4개는 Assets 안에서 중복되지 않는다.

기존 `Assets/Scenes/2.unity`의 해시는 작업 전후 동일하다.

```text
0019647EFA1F015738232A69497F67195C8D04D99D2D300A34963D0CDA8363EB
```

이번 작업에서 기존 씬의 배치·타일·플레이어를 편집하거나 저장하지 않았다. 실제 Unity 프로젝트의 패키지와 ProjectSettings도 변경하지 않았다. 원본 코드 백업은 `Docs/AI/Backups/2026-10-05-level-workflow-before`에 보관했다.

## 한계

핸들의 마우스 조작과 Game 화면 렌더링은 직접 검증하지 않았다. EditMode 테스트는 실제 컴포넌트 메서드와 네이티브 Physics2D를 사용하며 전체 게임 PlayMode 통과·실행 파일 빌드 결과는 아니다. 가시 그림/Tile 에셋, 플레이어 운반·발판 점프 관성·압사 처리는 포함하지 않는다.

최초 두 실행은 임시 씬 생성 fixture에서 실패했고, 이후 네이티브 검증에서 타일 범위 계산과 짧은 경로의 도착 지연을 수정했다. 테스트의 EditMode 호출·Undo 그룹 구분도 교정했다. 최종 성공 XML과 로그는 상위 폴더에, 앞선 실패 기록은 `Attempts`에 보존했다.

- [최종 테스트 XML](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ValidationArtifacts/LevelWorkflow/UnityResults.xml>)
- [실행 소스 해시](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ValidationArtifacts/LevelWorkflow/source-snapshot.json>)
- [재실행 안내](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Validation/LevelWorkflow/README.md>)
- [방 제작 가이드](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Docs/LevelEditingWorkflow.md>)

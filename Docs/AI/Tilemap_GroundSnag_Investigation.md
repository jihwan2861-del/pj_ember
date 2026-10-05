# 2씬 평평한 바닥 걸림 수정

최초 검증: 2026-10-04. 최신 저장본에 재적용: 2026-10-05.

현재 `Assets/Scenes/2.unity`는 최신 751개 셀을 보존한 수정본이다. 최초 검증 도중 753개 셀 기준으로 추가한 컴포넌트가 이후 새 저장본에서 빠졌고, 셀 (-6, 3), (-5, 3)이 삭제된 것을 확인했다. 해당 타일 편집과 인접 Sprite 갱신, 플레이어 설정을 모두 유지하면서 같은 두 컴포넌트만 다시 추가했다. 아래 최초 검증 기록은 753개 셀 기준이며 `ValidationArtifacts/MovementFeel/TilemapSticking-753Cells`에 별도로 보존했다. 최신 결과는 기존 `TilemapSticking` 폴더에 기록한다.

## 확인된 원인

오른쪽 방향키를 유지해 평평한 바닥을 걸을 때 타일 경계에서 수평 속도가 0이 되는 현상을 실제 2씬 지형으로 재현했다. TilemapCollider2D에는 Composite Operation = Merge가 설정되어 있었지만, 연결할 Rigidbody2D와 CompositeCollider2D가 없었다. Inspector의 노란 경고와 같은 조건이다. 따라서 753개 셀은 독립적인 사각 충돌 도형으로 남아 있었고, 바닥 내부의 타일 경계가 플레이어 BoxCollider에 수평 접촉을 만들었다.

테스트에서 정지 순간의 접촉 법선은 (-1, 0), 접촉점은 약 (0.03136, -2.98), 플레이어 위치는 약 (-0.182855, -2.69918)이었다. 이동 입력은 계속 오른쪽이었으며 동일한 컨트롤러·Collider·마찰 설정에서 지형 합치기만 적용하자 이 접촉과 정지가 사라졌다.

타일은 모두 Mossy Terrain RuleTile의 Grid Collider를 사용한다. Cell Gap은 0이고 0.5배 Tilemap 변환도 동일하다. 따라서 이번 재현은 스프라이트의 불규칙한 외곽이나 셀 배치 간격이 원인이 아니다.

## 적용한 수정

`Assets/Scenes/2.unity`의 Tilemap GameObject에 다음 두 컴포넌트를 추가했다.

- Rigidbody2D: Body Type = Static.
- CompositeCollider2D: Geometry Type = Polygons, Generation Type = Synchronous.

기존 TilemapCollider2D의 Merge와 Extrusion Factor = 0을 유지했다. 0.01 extrusion 비교도 같은 결과를 냈으므로 외곽을 추가로 넓히지 않았다. 최초 수정에서는 753개 셀, 최신 재적용에서는 751개 셀을 각각 보존했다. RuleTile GUID, Grid 위치 (0.04, 0, 0), Tilemap scale (0.5, 0.5, 1), 플레이어 설정도 보존했다. 이번 수정에서 이동 코드·플레이어 Collider·물리 재질은 변경하지 않았다.

Unity가 수정된 native 씬 문서를 실제로 Import/OpenScene한 결과, 지형은 5개 Composite 경로와 78개 점으로 합쳐졌다. 이는 물리 Collider 5개라는 뜻은 아니며 Composite 내부의 물리 shapeCount는 35였다. TilemapCollider의 composite와 attachedRigidbody 연결도 확인했다. 마지막 검증에서는 Composite.GenerateGeometry() 강제 호출을 제거했다. OpenScene 직후 이미 5개 경로가 존재했고 ProcessTilemapChanges 뒤에도 5개였으므로 Synchronous 설정에 따른 자동 생성을 확인했다.

## 최신 751개 셀 검증

최신 저장본으로 native 지형 테스트 4개를 다시 실행했고 **4개 통과·0개 실패**, 종료 코드 0을 확인했다. 수정 전후 751개 셀을 모두 보존했으며 최신 Composite는 5개 경로·80개 점·37개 내부 물리 shape다. OpenScene 직후와 ProcessTilemapChanges 후 모두 5개 경로가 있어 강제 GenerateGeometry 호출 없이 자동 생성된다.

동일한 평지에서 수정 전 이동 거리 1.017145 U·정지 83 tick·수평 접촉 83 tick, 수정 후 6.260648 U·정지 0 tick·수평 접촉 0 tick으로 확인했다. 착지 후 걷기도 수정 전 정지 43 tick에서 수정 후 0 tick으로 개선됐고, 실제 높은 벽에서 대시 종료와 본체 Collider 복귀도 통과했다. 걷기 접촉 기록 420행을 simulation.csv에 보관했다. 현재 런타임 소스 26개와 QA 복사본의 SHA256은 모두 일치했다.

최신 결과는 `ValidationArtifacts/MovementFeel/TilemapSticking/UnityResults.xml`, `summary.json`, `after-geometry.json`과 `terrain-snapshot.json`이다. 최신 재검증 중 기존 753개 셀의 결과와 재실행 소스는 별도 폴더로 보존했다.

## 최초 753개 셀 수정 전후 검증

Unity 6000.5.2f1의 격리 EditMode 테스트에서 실제 KeyboardState 입력과 Physics2D.Simulate(0.02초)를 사용했다. 원본과 수정본 씬에서 Grid·Tilemap·Collider의 native YAML 문서, 원본 RuleTile, atlas PNG/meta를 추출해 Unity가 직접 불러왔다. 플레이어는 실제 런타임 코드를 사용하고 2씬의 이동 값과 BoxCollider 크기·offset을 적용했다.

| 확인 항목 | 수정 전 | 수정 후 |
|---|---:|---:|
| 실제 긴 평지에서 100 physics tick 이동 거리 | 1.017145 U | 6.260648 U |
| 위 걷기 중 정지 tick | 83 | 0 |
| 위 걷기 중 수평 접촉 tick | 83 | 0 |
| 착지 후 60 tick 이동 거리 | 1.017145 U | 3.733659 U |
| 착지 후 걷기 중 정지 tick | 43 | 0 |
| 원래 시작 위치에서 실제 높은 벽으로 대시 | 정상 정지·본체 복귀 | 정상 정지·본체 복귀 |

평지 구간은 cell y=-7, x=-3..13이며 바로 위 y=-6/-5에는 장애물이 없다. 테스트 시작 위치는 (-1.2, -2.5), 착지 비교 시작 위치는 (-1.2, -1.8)이다. 각 테스트는 먼저 30 tick 동안 바닥에 안정시킨 뒤 오른쪽을 유지했다. 대시 벽 비교는 원래 플레이어 시작 위치 (-8.091217, 0.10589218)와 실제 대시 CircleCollider 크기·offset을 사용했다. 수정 전후 모두 4 tick 후 x≈-7.3872에서 멈추고 본체 Collider가 정상 복귀했다.

새 지형 회귀 테스트 4개가 모두 통과했다. 이전 이동 개선 테스트 17개 결과는 별도 기록이며, 당시에는 실제 Tilemap 바닥을 검증하지 못했다. 이번 테스트는 해당 검증 공백을 보완한다.

## 기록과 재실행

- 테스트: `Validation/Movement/Scene2TilemapTests.cs`.
- 실행 준비: `Validation/Movement/Prepare-TilemapRegression.ps1`.
- 결과: `ValidationArtifacts/MovementFeel/TilemapSticking/UnityResults.xml`.
- 접촉·위치·속도: 같은 폴더의 `walk-*.json`, `landing-*.json`, `dash-wall-*.json`.
- 지형 연결: `before-geometry.json`, `after-geometry.json`.
- 원본·수정 씬과 소스 SHA256: `terrain-snapshot.json`, `source-snapshot.json`.
- 최초 원본 백업: `Docs/AI/Backups/2026-10-04-tilemap-merge-before/2.unity`.
- 최신 751개 셀 원본 백업: `Docs/AI/Backups/2026-10-05-tilemap-merge-before/2.unity`.

원본 씬 SHA256: `47345F898C22DED8B7B82672D885CC3DD2D8AFD50C88C00B162B4945991A2300`.

최초 753개 셀 수정 씬 SHA256: `5879BF81E9264CC455DC40D3E2F3B63A11E9D2C32077F8311876508C15B8630F`.

최신 751개 셀 원본 SHA256: `2A126F3A412EF82C5C4B3B5EEFF2ECB8DD0771655A03639B3AB37A6289C892DE`.

최신 751개 셀 수정 씬 SHA256: `0019647EFA1F015738232A69497F67195C8D04D99D2D300A34963D0CDA8363EB`.

최신 수정은 추가한 두 문서와 GameObject의 두 컴포넌트 참조를 제거하면 최신 원본과 바이트 단위로 완전히 일치한다. 원본의 37개 native 문서 중 변경된 기존 문서는 Tilemap GameObject의 컴포넌트 목록뿐이며, 두 컴포넌트 문서가 추가되어 39개가 됐다.

## 적용 확인 범위

원본 Unity Editor를 자동으로 종료하거나 조작하지 않았다. 최초 검증 때는 열린 씬에 미저장 표시가 있어 재로드하지 않았으며, 최신 재적용 시점에는 원본 Unity 프로세스가 실행 중이지 않았다. Unity에서 최신 2씬을 열어 적용본을 확인할 수 있다. Inspector에서 Tilemap에 Static Rigidbody2D와 CompositeCollider2D가 있는지, 기존 노란 Composite 경고가 사라졌는지 확인한다.

지형과 입력·물리 동작은 위 자동 재현으로 확인했다. 전체 2씬의 손 플레이·렌더링·별도 플레이어 빌드는 이번 조사 범위에 포함하지 않았다.

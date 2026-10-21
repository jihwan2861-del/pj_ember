# Unity 툴 정리 기록

정리일: 2026-10-17. 사용자가 실제 쓰는 레벨 워크플로우를 중심으로 생성 메뉴와 자동 설정 스크립트를 정리했다. 기존 씬·프리팹·애니메이션과 게임 기능 코드는 삭제하지 않았다.

## Unity에 유지한 편집기

| 스크립트 | 용도 |
| --- | --- |
| EmberLevelWorkflowWindow | 사용하는 Level Workflow 창 |
| CameraRoomEditor | 카메라 경계 Inspector와 Scene 크기 조절 핸들 |
| MovementPathEditor | 이동 경로의 점·곡선·원 편집 |
| PathMovingBlockEditor | 이동 발판 Inspector와 경로 미리보기 |

나머지 편집기 3개는 별도 생성 메뉴가 아니라 방과 기믹을 직접 편집하는 보조 기능이다.

## 삭제한 생성 및 자동 설정 스크립트

| 스크립트 | 하던 일과 삭제 이유 |
| --- | --- |
| EmberTutorialBuilder | 튜토리얼 씬 자동 생성. 씬은 사용자가 직접 제작한다. |
| EmberPrototypeSceneBuilder | 옛 프로토타입 씬·카메라 생성. 현재 레벨 도구와 중복된다. |
| FireFlowerDevelopmentBuilder | 화염꽃 이미지 분할·프리팹 생성·현재 씬 배치. 애셋 제작이 끝났다. |
| PollutionSpikeBuilder | 가시 띠 프리팹 생성·현재 씬 배치. 개별 타일 팔레트를 사용한다. |
| BurrTileSetBuilder | 밤송이 3종 Tile·팔레트 생성. 기존 Tile과 팔레트로 직접 그린다. |
| MossyAutotileSetup | 이전 픽셀 지형 RuleTile·팔레트 생성과 검사. 기존 애셋을 유지하고 생성 툴은 제거한다. |
| PlayerJumpAnimationSetup | 에디터 시작 시 이전 점프 애니메이션 컨트롤러 자동 수정. 컨트롤러는 직접 편집한다. |
| DeathParticlePrefabSetup | 에디터 시작 시 플레이어 사망 파티클 자동 생성·연결. 기존 프리팹을 사용한다. |
| LaunchRingParticlePrefabSetup | 에디터 시작 시 점화 링 파티클 자동 생성·연결. 기존 프리팹을 사용한다. |

## 검사 폴더로 분리한 코드

다음 8개는 과거 검사 코드가 직접 호출하므로 `Validation/EditorSupport/`에 소스와 메타데이터를 보관했다. 실제 프로젝트의 `Assets` 밖에 있어 메뉴에 나타나거나 에디터 시작 시 실행되지 않는다.

- EmberMainMenuSceneBuilder
- FireFlowerValidationSceneBuilder
- RopeValidationSceneBuilder
- WaterSpiritPrefabBuilder
- WaterSpiritAISetup
- WaterWavePrefabBuilder
- StrawberryPickupBuilder
- WeightPlatformBuilder

기존 격리 검사 실행 스크립트는 필요한 생성 코드를 새 위치에서 QA 프로젝트로 복사한다. 검사는 격리 프로젝트에서 실행하며 실제 작업 씬을 생성하는 용도로 사용하지 않는다.

## 외부 애니메이션 가져오기

`UnityWebRigImporter`는 외부 Rig 데이터를 프리팹·애니메이션으로 가져오는 코드다. `Assets/WebRigs/`에 가져온 결과물이 있어 원본을 `Webanim/UnityIntegration/`에 보관했다. Unity 메뉴에서는 제거했으며 기존 Rig 애셋의 실행에는 이 가져오기 코드가 필요하지 않다.

## 게임 기능 스크립트 조사

직접 붙어 있는 씬·프리팹이 없다는 이유만으로 사용하지 않는 코드라고 단정하지 않는다. `DeathScreenWipe`는 사망·재시작 코드에서 생성되고, `WaterSpiritAttack`·`WaterSpiritBehavior`는 보스 동작에 쓰인다. `MapReset`과 입력 보조 클래스도 실행 중 호출된다.

`AllTorchesDoor`, `Spring`, `WaterAmbientParticles`는 조사한 직렬화 애셋에 직접 연결이 없지만 기능·검사 코드가 있어 유지했다. `CameraPositionGrid`와 `CameraShiftTrigger`는 이전 카메라 방식의 추가 정리 후보이며, 이번에는 게임 기능 코드 삭제에 포함하지 않았다.

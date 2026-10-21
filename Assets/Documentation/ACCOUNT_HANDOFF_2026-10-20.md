# Project Ember — 다른 계정용 전체 인수인계

작성 기준: **2026-10-21, 한국 시간**. 이전 채팅을 읽지 않아도 현재 작업을 이어갈 수 있도록 정리한 문서다. 이후 사용자 지시와 이 문서보다 최신인 실제 구현·검증 기록이 있으면 그 내용을 우선한다.

## 2026-10-21 최신 방향 — 어두운 길과 정령 구출 오프닝

사용자는 원형을 도는 대신 Global Light를 끄고 작은 불꽃이 걸어가다 오염된 정령을 구하고, 세계가 변한 이유를 들은 뒤 산으로 향하는 흐름을 선택했다. STORY02는 세 컷 만화 → 어두운 길 걷기 → 오염된 빛의 정령 구출/대화 → 강의 과거 장면 → C 태양꽃/에너지 전달 → TutorialScene이다. 앰버는 태양이 되겠다는 목표부터 가진 것이 아니라 자기 정체와 세계 변화의 이유를 찾으려 한다.

`Assets/Scenes/Storyscene.unity`에서 Global Light 2D를 비활성화하고 `OpeningComicSequence`가 플레이어를 따라다니는 Point Light를 런타임에 생성한다. 조절값은 intensity1.25, 주황색(1,.48,.16), inner1.2/out6.5U다. 만화 종료 후 `StorySectionCamera`가 x0부터 따라오며, `RotatingFlatGroundEntrance.beginAtJoinPose=true`가 평지를 시작부터 연결한다. 출발 플랫폼은 scaleX 1.2366, JoinPose Z 회전 9.9897도로 조정해 world (0,-1.33)부터 강둑 경사 시작점 (34.1,4.68)까지 이으며, 원형 회전은 필요하지 않다. 원형 이동 큐 `아무거나`는 비활성화했고 `CircularOpeningWorld`는 오프닝에 연결하지 않는다. 화면을 나가면 NextStoryStart로 페이드 워프하던 `ScreenExitSceneLoader`도 비활성화했다.

### 스토리 블록 재조립 기준

사용자는 새 기능을 더 만들기보다 기존 기능을 장면 순서에 맞게 조립하고, 강가 같은 기존 배치는 가져와 재사용하기로 했다. 순서는 만화 → 어두운 길 → 오염된 빛의 정령 구출/대화 → 강가 과거 기억 → 시든 태양꽃 에너지 전달 → TutorialScene이다. 현재 `Storyscene.unity`에는 강가 전체가 `StoryShoreSection` 아래 이미 모여 있으므로 이를 통째로 유지하고, 물·발판·반사·기억 이벤트·꽃은 재복제하지 않는다. 위치·충돌·카메라 제한·직렬화된 참조를 화면 확인 전 임의 이동하지 않는다. 상세 배치 지도는 `Docs/StoryAssembly_Rebuild.md`다. 이 턴에는 Unity 편집기/Game View 캡처를 사용할 수 없었으므로 실제 연속 플레이와 화면 구도는 미확인이다.

`Guide_NPC`는 x44.55 부근의 빛의 정령으로 바꿨다. 새 이미지 `Assets/Ember/Art/Storybook/Characters/StorySpirit_Doodle_v1.png`와 기존 `Assets/Ember/Art/Storybook/StageExtracted/07_CorruptionVines.png`를 쓰며 `Corruption Vines`는 대화가 실제 시작될 때 꺼진다. 대사는 태양 오염이 세상과 정령의 빛을 끊었으며 산 정상의 오래된 빛 근원에서 이유를 찾을 수 있다고 안내한다. 기존 Stone_3 착지→RiverMemorySequence→시든 꽃 C→에너지 이동→TutorialScene 전환은 그대로 연결했다. 백업은 `Assets/Scenes/Backups/Storyscene_BeforeSpiritRescueOpening_20261020.unity`다. 출발 평지 경사 연결은 씬 직렬화에 반영했으나 Unity Play/Game View 화면은 아직 확인하지 않았다. 현재 기준 문서는 `Docs/StoryFlow_Setup.md`, `TASKS.md`, `AGENTS.md`다.

## 최신 변경 — NPC 대화 UI

`StoryNarrationCue.Presentation=Dialogue`가 Celeste풍 하단 패널, 화자 이름/초상화, 글자 출력, 수동 Z 진행과 닫기를 사용하도록 구현했다. 안내자 큐는 첫 줄을 앰버, 두 번째·세 번째 줄을 안내자로 표시하고 안내자 그림을 초상화로 연결했다. 기존 조작 잠금과 NPC 진입 트리거, 상단 자동 나레이션은 유지했다. 설정은 `Docs/StorybookNarration_Setup.md`, 씬 배치는 `Docs/StoryFlow_Setup.md`다. 코드와 직렬화 참조는 반영했지만 Unity Game View에서 실제 크기·레이아웃·입력 동작은 아직 확인하지 않았다.

## 이전 변경 — 오프닝 태양 궤도 숨김 (정령 구출 오프닝으로 대체됨)

사용자 요청으로 Storyscene 원형 구간에서 플레이어 주변을 돌던 정상·오염 태양을 숨겼다. `OpeningSunTransition.showOrbitingSunsDuringOpening=false`이며 씬 직렬화에서도 `NormalSun`을 비활성 상태로 시작한다. 기존 페이드 지점의 `PinCorruptedSunToCamera`는 유지되어 오염 태양이 다시 나타나 우측 상단에 고정된다. 원형 지형/플레이어 이동은 현재 임시로 남아 있고, 태양 안에서 오염이 추격해 막다른 순간 C 또는 X 입력을 유도하는 새 오프닝은 다음 작업이다. 런타임 확인은 아직 하지 않았다. 설정: `Docs/OpeningSunTransition_Setup.md`, `Docs/StoryFlow_Setup.md`.

## 이전 배치 기록 — Storyscene 오프닝·NPC·강가 흐름

사용자가 지정한 Storyscene 순서대로 씬/코드를 저장했다. 세 컷 만화가 Screen Space Canvas에 3.5초 표시되고 0.85초 페이드아웃한 뒤 원형 이동이 시작된다. Circle의 `beginOnStart`를 꺼서 만화보다 먼저 회전하지 않게 했다. 원형 이동 시작에는 “아무거나” 나레이션 큐를 연결했다. **최신 변경에서 이 원형 구간 중 정상·오염 태양 궤도는 숨겼다.**

평지에는 사용자가 요청한 임의 제작 안내자 이미지 `Assets/Ember/Art/Storybook/Characters/Guide_Elder_Doodle_v1.png`를 배치했다. 안내자 대사는 “저 꽃은 뭐야?”에서 시작해 태양꽃 설명, 강물을 살피라는 말로 이어진다. 해당 대화는 나레이션별 조작 잠금이 켜져 있다. `Stone_3` 위 `MemoryOnMiddleStone`은 착지/접지 감지 후 기존 RiverMemorySequence를 시작한다. 완료 이벤트에서 TutorialScene으로 바로 전환하던 연결을 제거했다.

강 건너 첫 평탄부의 시든 꽃은 C로 활성화된다. 이야기용 꽃은 `fireProjectileOnActivation=false`여서 발사체 없이 꽃이 피고, 에너지가 플레이어에게 이동한 뒤 기존 브리지로 페이드 전환한다. 브리지는 RiverMemorySequence 완료를 기다린다. 씬 배치, 노출된 조절값, 다음 확인 순서는 `Docs/StoryFlow_Setup.md`다.

**현재 기록은 파일/직렬화 연결 확인이며 Unity Game View의 실제 Play 확인은 아직 하지 않았다.** 단계별 화면 캡처와 입력 완주 상태로 읽지 않는다. 다음 작업은 Unity에서 Storyscene을 열어 컴파일·Play하고 만화부터 TutorialScene 전환까지 캡처해, NPC/발판/꽃의 화면상 위치와 잠금 타이밍을 조정하는 것이다. 이전 실행 중이던 Unity 편집기는 저장 파일과 동기화되지 않은 상태로 알려져 있으니, 파일 변경을 가져올 때 현재 편집기의 사용자 수정본을 덮어쓰지 않도록 먼저 저장 여부를 확인한다.

## 최신 추가 — 태양 아트 통일

정상 태양은 `Sun_Doodle.png`, 오염 상태는 `CorruptedSun_Doodle_v1.png`를 표준으로 사용한다. Storyscene의 `NormalSun`과 두 과거 반사 원본을 정상 낙서 태양으로 바꾸고, 화면 크기도 교체 전 월드 크기와 오염 태양에 맞게 보정했다. 물가 반사·회전 연결과 나머지 배치는 유지했다. SkinScene 및 `sun.prefab`은 이미 같은 정상 그림을 사용하고 있었다. 태양꽃과 불꽃은 별도 기믹이어서 변경하지 않았다.

현재 어떤 씬·프리팹도 참조하지 않는 구형 오염 태양 그림과 생성 프롬프트를 삭제했다. `03_Sun.png`, `PPT_HealthySun.png`, `PPT_CorruptedSun.png`는 복구용 백업 씬이 참조하므로 남겼다. 일반 씬9개와 LevelDesign 프리팹 참조 검사에서 구 정상 태양 참조가 없음을 확인했다. 최종 캡처는 `Assets/ArtReference/SunCleanup/01_SunUnified.png`, 설정은 `Docs/OpeningSunTransition_Setup.md`와 `Docs/CorruptedSunDoodle_Setup.md`다. 작업 중 Unity가 TutorialScene Play 모드였으므로 화면을 캡처한 뒤 저장을 위해 Play를 종료했고, 에디터는 Storyscene 비재생·clean 상태다.

## 최신 추가 — 개발 태스크와 앰버 탄생

사용자가 개발 태스크 문서 작성을 요청하고 첫 개발을 **앰버 탄생→능력 튜토리얼**로 선택했다. 루트 `TASKS.md`를 최신 실행표로 갱신했으며 옛 TASKS는 `Docs/Archive/Technical/TASKS_Before_2026-10-20.md`에 보관했다. 현재 DEV01은 구현·적용·실제 Play 확인 완료다.

TutorialScene 진입의 최신 흐름은 **작은 불꽃/잠금 → 입장 페이드 완료 대기 → 시각 전용 꽃 반응 → 빛 이동 → 앰버 전환 → 카메라 복귀/잠금 해제 → 첫 X 교육**이다. 기존 낙서 꽃 Head/Body와 StoryPlayerAppearance 불꽃을 재사용했다. EmberBirth/BirthFlowerVisual에는 FireFlower/Collider가 없어 발사·정화가 발생하지 않는다. 두 번째 교육의 실제 꽃은 유지한다. 탄생 단계 총3.75게임초이며 설정은 `Docs/EmberBirth_Setup.md`, 새 코드 `Assets/EmberPrototype/Runtime/EmberBirthSequence.cs`, 백업은 `Assets/Scenes/Backups/TutorialScene_BeforeEmberBirth_20261020.unity`다.

직접 Play/Storyscene 경유, 들어오는 페이드 대기, 연출 중 Right/X/C 차단, Pause/R 재시작, 명시적 Skip 연결, 비활성화 정리와 종료 뒤 실제 X→Z 착지를 확인했다. 최종 카메라(105.51,2)/Size5.0625·잠금false·첫 교육EnterFlammable이다. 최종 화면은 `ArtReference/EmberBirth/03~07·09~10`, 01~02는 흰 여백이 있는 수정 전이다. 기존 지형/Collider와 Storyscene2+3+2초는 유지했고 새 PNG는 없다. 아래 이전 탄생 미완료·진입 즉시 앰버 표시는 이 추가 전 기록이다. 일반 사망은 탄생을 다시 재생하지 않으며 R 씬 재시작 때 재생한다. 이번 변경 뒤 2/3교육 재완주·원형 시작부터 완주·빌드는 수행하지 않았다.

## 1. 가장 먼저 알아둘 상태

- 프로젝트: `C:/Users/kyk04/Desktop/지스타 게임`
- 현재 브랜치/확인한 HEAD: **main / 6396fcf**.
- Unity Editor 기준: **6000.6.5f1**. 현재 설치·프로젝트 버전을 작업 시작 전에 다시 확인한다.
- 현재 콘셉트는 **불의 정령 앰버**다. 과거 바람 버전의 글라이더·자동 기류·깃털 비행을 복구하지 않는다.
- 사용자 최신 요청으로 **Storyscene과 TutorialScene을 분리했다**. 과거의 “모든 스토리·튜토리얼을 Storyscene 하나에서 연결” 기록보다 이번 결정을 우선한다.
- 사용자가 직접 수정한 오브젝트 배치와 삭제한 발판을 보존한다. 그림을 옛 Collider 경계에 맞추려고 임의로 되돌리지 않는다.
- 새 이미지 애셋은 **3개마다 실제 씬 적용 화면을 보여주고 사용자 확인 후 다음 묶음**으로 진행한다.
- 새 기본 지형은 **그레이박스 Tilemap → 충돌 없는 아트 스킨** 순서다. 외형 교체 때문에 이동·충돌을 임의로 바꾸지 않는다.

### Git과 계정 변경 시 주의

이 프로젝트는 큰 미커밋 작업 상태다. 작성 전 조사에서 `git status --short` 항목은 삭제1,382·수정64·미추적110개였으며 미추적 폴더 내부 파일까지 센 숫자는 아니다. 삭제 목록에는 기존 폴더 정리·이동 결과도 섞여 있다.

**HEAD만 checkout하거나 원격 저장소만 받으면 최신 결과가 전달되지 않는다.** Storyscene, 새 아트 폴더, 새 Runtime 코드와 설정 문서 상당수가 미추적 상태다. `Assets`의 `.meta`까지 포함해 실제 작업 파일을 보존해야 한다. `.meta`가 빠지면 GUID 연결이 깨질 수 있다. `Packages`, `ProjectSettings`, 문서와 필요한 `ArtReference`도 함께 보존한다.

`git reset --hard`, `git clean`, 대량 삭제 복구, 브랜치 변경, 원격 이력 덮어쓰기를 정리 목적으로 실행하지 않는다. 커밋·푸시는 사용자 요청 없이 하지 않는다. 에디터 안의 **미저장 변경은 파일 복사나 Git에 자동 포함되지 않으므로**, 계정 전환 전에 현재 에디터의 저장·백업 상태를 별도로 확인한다. 이전 작업은 사용자 미저장 편집을 보호하려고 일부 씬을 dirty로 유지했다.

## 2. 문서 읽는 순서

1. 루트 `AGENTS.md`의 **최상단 최신 항목**.
2. 이 문서와 루트 `pj_ember_구현_인수인계.md`의 최신 항목.
3. `Docs/AI/FireProject_Handoff.md`, `Docs/README.md`.
4. 요청에 해당하는 설정 문서와 실제 씬·Runtime 코드.
5. `TASKS.md`, OpenSpec, 보관 문서는 시점을 구분해서 참고한다.

문서에 “최신”이라는 제목이 여러 번 나오거나 서로 다른 과거 상태가 누적돼 있다. **뒤의 과거 복원 기록을 읽고 현재 채택 아트를 다시 되돌리지 않는다.** 옛 TASKS의14방 표·F06 미구현 표시는 보관본으로 옮겼다. 현재 TASKS.md는 최신 실행표다. 발표용 최신 범위는 **튜토리얼 별도 + 일반 방8개 + 선택 진입하는 우미 보스 방**이다. 모든 방의 역할과 본편 배치가 구현 완료된 것은 아니다.

## 3. 이번 씬 분리 — 가장 최근 작업

| 씬 | 현재 역할 |
| --- | --- |
| `Assets/Scenes/Storyscene.unity` | 어두운 길 → 정령 구출 → 평지·강가·과거 반사·나레이션 |
| `Assets/Scenes/TutorialScene.unity` | 기존 GameplayTutorial_01/02/03과 튜토리얼용 player·카메라·Global Light |
| `Assets/Scenes/Backups/Storyscene_BeforeSceneSplit_20261020.unity` | 분리 전 live 배치를 보존한 백업. 평상시 플레이 씬으로 사용하지 않는다 |

두 씬은 Build Scene List에 등록했다. 기존 `story2`는 이번 연결 대상이 아니다. 옛 파일을 발견했다는 이유로 자동 연결하거나 삭제하지 않는다.

### Storyscene → TutorialScene 연결

- 현재 저장된 Storyscene 흐름은 **3컷 만화 → Global Light를 끈 길과 불꽃 조명 → 빛의 정령 구출/대화 → 가운데 Stone_3의 강 기억 → 건너편 시든 태양꽃 C 활성화 → 앰버로 에너지 이동 → TutorialScene**이다. 예전의 원형과 “아무거나” 단계는 비활성화했다.
- 이전 `PastNarration.onFinished` 직접 전환 연결은 제거했다. `StoryTutorialSceneTransition`은 이제 꽃 에너지 도착 뒤 호출되며, 기억 연출 완료/사망/일시정지/다른 페이드 상태를 기다린다.
- 기존 `SceneFadeTransition`으로 **페이드아웃0.65초 → TutorialScene을 Single로 로드 → 페이드인0.4초**를 사용한다. 강가 공통 `PastNarration` 문구는 남고, 이야기 큐가 대사를 표시할 때만 각 큐의 임시 문구로 바뀐다.
- 시작 만화: `Assets/Ember/Art/Storybook/OpeningComic_ThreePanels_v1.png`, Inspector 표시3.5초/페이드0.85초. 안내자 그림: `Assets/Ember/Art/Storybook/Characters/Guide_Elder_Doodle_v1.png`.
- 설정·하이어라키: `Docs/StoryFlow_Setup.md`. 실제 런타임 캡처/전체 입력 완주는 아직 없다.
- 새 브리지 코드: `Assets/EmberPrototype/Runtime/StoryTutorialSceneTransition.cs`.

### TutorialScene 진입

- 새 시작 코드: `Assets/EmberPrototype/Runtime/TutorialSceneEntry.cs`.
- player를 원형 이동 모드에서 해제하고 EntryCheckpoint로 배치하며 같은 위치를 리스폰으로 등록한다.
- 시작점: **EntryCheckpoint (102.11,.35)**.
- 첫 방: **PracticeRoom 중심 (105.51,2)**.
- 화면: **Orthographic Size5.0625 / 18×10.125U / 36×20.25칸 / 16:9**.
- 앰버 모습으로 표시하고 CelesteRoomCamera의 방·타깃·카메라 위치를 초기화한다.
- 첫 `FlammableEntryTutorial.BeginTutorial`을 호출한다. 입장 페이드가 입력을 막는 동안에는 해당 튜토리얼 자체의 대기 처리를 사용한다.
- 과거의 `QueueBegin → River 시듦 대기`에만 의존하면 새 씬에서 진행되지 않는다. 새 씬에는 Storyscene의 River를 참조하지 않는다.

각 튜토리얼 루트에 지형·위험·꽃·오염·체크포인트·안내 Canvas·CameraRoom이 포함돼 있다. 새 씬에 StorySectionCamera의 강가 추적이나 Circle 참조를 남기지 않는다. player·CameraRoom·UnityEvent·체크포인트는 같은 씬 안의 복제본을 연결해야 한다.

`MapReset`은 별도 씬 오브젝트가 아닌 static 코드이며 player가 속한 씬의 기믹을 초기화한다. `GameSessionControls`는 player의 Awake에서 자동 추가되므로 별도 중복 매니저를 만들 필요가 없다. 두 씬을 동시에 Play하면 자동 player 탐색·카메라·Global Light가 충돌할 수 있으므로 실제 전환은 Single 로드를 사용한다.

### 이번 분리에서 확인한 범위

실제 방향키+Z로 오른 강둑 앞 낮은 돌에 착지 → 태양/물 진행1 → 나레이션 자동 종료 → 페이드 → TutorialScene 로드까지 확인했다. 새 씬에서는 player가 접지하고 조작 잠금이 풀렸으며 첫 교육의 `EnterFlammable` 상태와 카메라1개를 확인했다. 좌우 이동도 확인했다.

분리 후 세 교육을 실제 방향키/Z/X/C 입력으로 연속 완료했다. 초기 물가 접근만 캡처용으로 Stone_4에 배치한 뒤, 낮은 돌 착지부터 새 씬 전환과 세 교육 완료까지는 위치를 쓰거나 완료 상태를 강제하지 않았다. 전체 원형 오프닝부터의 입력 완주·빌드를 수행한 것은 아니다. 실제 Game View 스크린샷은 루트 `ArtReference/SceneSplit`에 보관했다.

#### 분리 후 최종 확인 기록

| 항목 | 현재 기록 |
| --- | --- |
| 01 X 점화·진입 → 방향키+Z 이탈·착지 | 목표(104.51,2) 진입, FireLaunchCount1, (110.24,.81) 착지/Completed. 직전 좌우 이동 모두 확인 |
| 02 C 발사 → X 탑승 → Z 하차 → 정화·착지 | 01 완료 뒤 걸어서 진입. C 후 BoardFlame/0.2배, X 후 탑승/1배, Z 하차, IsPassageClear=true/Completed |
| 03 C → 방향키+Z 대시·착지 | 02 완료 뒤 (138.95,.84)까지 걸어서 진입, IgnitionDashCount1, (141.51,.84) 접지. 세 교육 모두 Completed=true |
| 사망·체크포인트 복귀 | 03 완료 후 실제 왼쪽 입력으로 틈에 떨어져 사망, (145.50,.85)로 복귀/접지/잠금false. 새 씬 체크포인트 참조 정상. 이번에는 모든 꽃·오염 복구 조합을 다시 확인한 것은 아님 |
| 독립 TutorialScene 시작과 Storyscene 경유 시작 비교 | 둘 다 (102.11,.34) 접지/잠금false/EnterFlammable/카메라1. 기존 GameSessionControls.RestartScene 호출 후 같은 씬 첫 구간으로 정상 재시작 |
| 저장·참조·Console | 두 씬 저장, 각 player/camera/audioListener1, missing script/ref0. Play 종료, 일시정지false, runInBackground=false, 원래 InputSettings의 background/routing0 복원. 최종 직접 시작·재시작 중 컴파일 실패false·Console 오류/경고0 |

검증 캡처: `01_Story_Opening_Play.png`, `02_River_Narration_Play.png`, `03_Fade_Out_Play.png`, `04_Tutorial_Start_Play.png`, `05_X_Enter_Play.png`, `06_Flower_Slow_Cue_Play.png`, `07_Third_Room_Play.png`, `08_Three_Tutorials_Completed_Play.png`, `09_Tutorial_Direct_Start_Play.png`.

당시 기록된 에디터 상태이며 현재 Unity 창의 열린 씬/재생 상태를 나타내지 않는다. 현재 Storyscene은 원형 오프닝 대신 어두운 길과 정령 구출로 시작한다. 새 씬을 직접 Play하면 스토리 없이 능력 교육부터 시작한다. 씬 전환을 시험할 때 두 씬을 Additive로 함께 재생하지 않는다.

## 4. 이전 낮은 돌 도달점 설정 — STORY02에서 대체됨

아래는 2026-10-20의 이전 배치/검증 기록이다. 최신 저장 씬에서는 낮은 돌 착지가 바로 강 연출을 시작하지 않는다. 현재는 안내자 뒤에 `Stone_3/MemoryOnMiddleStone` 착지 트리거가 과거 반사를 시작하며, C 태양꽃 에너지 연출이 끝난 뒤 TutorialScene으로 전환한다. 이 섹션의 낮은 돌→물 시듦→즉시 전환 경로는 현재 재생 플로우가 아니다.

사용자는 “기존에 올라가면 스토리가 나오던 **시작 지점 자체를 오른 강둑 앞 낮은 돌로 옮기자**”는 뜻이었다. 연출 순서를 다시 설계하거나 모든 변화가 동시에 시작되게 바꾸라는 요청은 아니다.

기존 오른 강둑 그림의 낮은 부분은 X74.7~79.1 부근인데 FarShore Polygon은 X79.62부터 시작했다. 사용자가 그 돌에 못 올라간 이유는 해당 그림 아래에 충돌이 없었기 때문이다. 삭제한 Stone_5를 되살리거나 강둑 그림을 원래 위치로 옮기지 않고 필요한 착지만 추가했다.

- `StoryShoreSection/FarShore/Art/ArrivalPlatform`: solid BoxCollider2D, **X74.6~79 / topY-1.5 / bottomY-3.15**.
- 자식 `WaterChangeOnLanding`: trigger 중심(76.8,-.8), 크기4.15×1.5.
- `PlayerEventTrigger.Require Grounded=true`, `Landing Surface=ArrivalPlatform`.
- 착지 후 `River.BeginTreeWithering`을 한 번 호출한다.
- 공중 진입 시에는 이벤트를 소모하지 않고 `OnTriggerStay2D`에서 착지를 재검사한다. 새 착지 옵션의 기본값은 false이므로 기존 진입 트리거는 그대로 작동한다.
- River의 **Wither Trees By Player Position=false**다. 옛 X80 자동 시작은 꺼져 있다.

### 착지 뒤 순서

1. **0~2게임초**: 물속 정상 태양 반사만 오염된 태양으로 전환한다.
2. **2~5게임초**: 변화된 태양을 유지한다.
3. **5~7게임초**: 나레이션과 함께 푸른 나무·풀·꽃 반사가 마른 모습으로 바뀌고 물속 하늘·물빛이 어두운 청회색이 된다.
4. 나레이션 종료 뒤 새 브리지가 물 변화 완료를 보장하고 TutorialScene으로 전환한다.

위쪽 실제 하늘은 유지하며, 태양을 단순히 어둡게 칠하는 방식이 아니다. 처음부터 물에는 푸른 과거 풍경이 비친다. 앰버의 밝은 불꽃 반사는 보존한다. 원형 구간 페이드 이후 화면 위 태양은 viewport **(.86,.84)**에 고정하는 기존 연출을 사용한다.

실제 방향키+Z로 수정 전 낙수·사망과 수정 후 착지를 비교했다. 착지 뒤 키를 해제하고 약7게임초 기다리는 동안 발판에 안정적으로 서 있고 물/태양 진행1이 되는 것을 확인했다. 별도 공중 진입에서는 미발동, 자연 착지 후 발동도 확인했다. 기록은 `ArtReference/OpeningWater/ArrivalLedgeFix`, 상세는 `Docs/WaterSequence_PPT4-7_Setup.md`다.

## 5. 현재 채택 아트와 교체하지 말아야 할 상태

전체 방향은 **메인 화면의 산처럼 단순한 동화풍 낙서**다. 굵고 불규칙한 검정 선, 성긴 마커·수채화 채색, 크림색 돌·연보라 그림자·황금 풀이 기준이다. 기존 픽셀풍8×8 기준은 새 아트에 적용하지 않는다.

### 원형 오프닝과 평지

- 원 그림 `GroundDisk_Doodle_v1.png`는 유지한다.
- 원 안 나무·풀·돌의 낙서 교체 시안은 사용자가 선호하지 않았다. **원본 Foreground/Background 시트 장식이 현재 채택본**이다.
- 사용자가 좋다고 한 평지4곳·경사면은 `FlatGround_Doodle_v1.png`, `PPT_Descent_Doodle_v1.png`로 복구했다.
- 원→평지 세로 경계는 `StoryGroundJoinBlend`와 전용 재질로 완화한 상태다. 원 내부와 겹치는 부분만 혼합하고 윗면·원 밖 알파를 유지한다.
- 원에 산 원호를 추가하던 `MountainArc_Doodle` 실험은 취소했다. 다시 배치하지 않는다.

### 강가 아트 묶음

| 묶음 | 파일 / 현재 적용 |
| --- | --- |
| 첫3개 | `Shore_Left_Doodle_v1`, `Tree_Dry_Doodle_v1`, `Tree_Healthy_Doodle_v1`: 강둑·큰 나무·푸른 반사 나무 |
| 두 번째3개 | `Stone_Low_Doodle_v1`, `Stone_Low_Healthy_Doodle_v1`, `Shore_Right_Doodle_v1`: 낮은 발판 짝·오른 강둑 |
| 기믹3개 | `SunFlower_Head_Doodle_v1`, `SunFlower_Body_Doodle_v1`, `Pollution_Thorns_Doodle_v1`: 사용자 “좋다 다음가자” 승인 완료 |
| 세 번째 강가3개 | `Stone_Mid_Doodle_v1`, `Stone_Mid_Healthy_Doodle_v1`, `Mountains_Doodle_v1`: 중간 발판 짝·산 배경, **적용 화면 사용자 확인 대기** |

강가 PNG는 `Assets/Ember/Art/Storybook/WaterDoodle_v1`, 기믹 PNG는 `Assets/Ember/Art/Storybook/DoodleMechanics_v1`에 있다. 첫 마른 큰 나무는 사용자가 좋다고 평가했다. 높은 발판 Stone_3·그 푸른 반사·숲·오염 태양·물 텍스처는 기존 그림이 남아 있다.

마른/건강한 짝은 같은 캔버스·pivot·돌/줄기 위치를 사용한다. 반사 원본은 EmberPastReflection 레이어31로 Main Camera에서 제외하고 WaterSurface2D에 합성한다. Single/Full Rect가 필요하다. 태양·반사 배열·진행 이벤트를 그림 교체 때문에 끊지 않는다.

산은 가로 반복하는 ParallaxLayer다. 새 산1915×821은 원본1916×821보다 가로1픽셀 작지만 Transform은 보정하지 않았다. 반복 배경과 물 반사의 연결을 실제 화면에서 확인했다. 산을 한 장짜리 전체 풍경으로 확대해 강가를 덮지 않는다.

### 사용자의 수동 배치

다음 위치는 과거 배치로 되돌리지 않는다. 추가 사용자 편집이 있을 수 있으므로 수정 직전 live 씬을 다시 읽는다.

- FarShore/Art: world **(83.04,.33)**.
- 오른 마른 나무: **(80.63,2.83)**.
- 오른 건강한 반사 나무: **(80.69,2.77)**. 작은 오프셋도 임의 정렬하지 않는다.
- 왼쪽 건강한 반사 나무: **(50.7,3)**로 옮긴 기록이 있다.
- 사용자가 삭제한 중간 `Stone_5`는 복구하지 않는다.

이미지 생성은 내장 `image_gen`을 사용했다. 프롬프트와 실제 적용 캡처는 `ArtReference/OpeningWater` 각 묶음 폴더에 있다. 생성 원본을 덮어쓰거나 미채택 시안을 현재 확정본으로 취급하지 않는다.

## 6. 게임 조작과 튜토리얼 계약

실제 기준 코드는 `Assets/EmberPrototype/Runtime/PlayerAbilityInputRouter.cs`와 `FlamePlayerController.cs`다. 기본 이동 수치를 새 아트 작업 때문에 바꾸지 않는다.

| 입력 | 기능 |
| --- | --- |
| 방향키 | 이동·능력 조준 |
| Z | 점프. 불/로프 안에서는 방향키로 조준해 이탈. 점화 링에서는 방향 대시 |
| X | 유효한 불/가연물에 점화·진입/이동. 대상이 없을 때 대시. 점화 링에서는 불씨 조준 사격 |
| C | 주변 점화 폭발·링 진입, 태양꽃 활성화 |
| `]` / `[` | 재생 속도를 각각2배/절반, 구현 범위1/32~32배 |
| Esc / R | 일시정지 / 재시작 |

마우스 입력은 사용하지 않는다. 모든 능력으로 모든 오염을 없애는 설계가 아니다. 꽃 불꽃 전용 정화 조건 등을 유지한다.

### 세 교육의 순서

1. **GameplayTutorial_01**: X로 고정된 미점화 가연물을 태우고 안으로 들어간 뒤 방향키+Z로 이탈·착지한다.
2. **GameplayTutorial_02**: C로 태양꽃 발사 → 발사 직후 슬로모션·X 안내 → 실제 X 입력으로 불꽃 탑승 → 조종·Z 하차 → 꽃 불꽃으로 오염 정화·착지.
3. **GameplayTutorial_03**: C → 방향키+Z 링 대시로 틈을 건넌다. 일반 X 대시만으로 교육 완료 처리하지 않는다.

이전 Storyscene 안에서 각 교육의 개별 입력·완료·일부 실패 재시도를 확인한 기록이 있다. **이번 씬 분리 뒤 같은 결과를 모두 다시 확인했다는 뜻은 아니다.** 위 분리 후 검증 표를 기준으로 구분한다.

### 태양꽃

- 정식 프리팹: `Assets/Ember/Prefabs/Tutorial/SunFlower.prefab`.
- 소형 꽃은 사용하지 않는다. 이전 대형 꽃을 ‘태양꽃’으로 통일했고 GUID를 유지했다.
- `Body`와 `Head`가 분리돼 있다. **Head 로컬 +X가 최초 발사 방향**이며 플레이어를 조준하지 않는다.
- C뿐 아니라 직접 접촉 활성화 기능도 있다. 현재 교육 인스턴스는 C 교육을 위해 접촉 활성화를 꺼 둔다.
- 탑승 가능한 꽃 불꽃에 X로 타고 방향키로 **최대2게임초** 조종한다.
- Z 또는 시간 만료로 내린 뒤 불꽃은 **마지막 방향으로1게임초** 직진한다. 재탑승하지 못한다.
- 오염·장애물에 먼저 닿으면 정화/충돌 종료와 탑승자 안전 이탈이 우선이다.
- 꽃 불꽃은 플레이어에게 피해를 주는 탄환이 아니다. 일반 꽃 탄환의 rideable 기본값은 false다.
- 낙서 머리 참조는 `wiltedSprite`/`bloomingSprite`에도 연결돼 있어 Awake·발사·리스폰 뒤 옛 그림으로 돌아가지 않게 했다.

두 번째 교육의 오염은 기존 solid Tilemap12셀을 유지하고 표시를 DoodleArt로 바꿨다. 별도 `ContactHazard` trigger/RoomHazard로 접촉 사망을 추가했고 기존 PollutionObstacle가 정화와 리스폰 복구를 함께 담당한다. 기존 solid를 trigger로 바꾸지 않는다.

## 7. 그레이박스·스킨·타일셋

- 기존 메뉴: **Tools > Ember > Level Workflow**.
- Grid는 Transform Scale(.5,.5,1), Cell Size(1,1,0)이며 실제 한 칸은.5U다.
- Terrain은 충돌용 Tilemap+Static Rigidbody2D+Composite다. Background·Art는 충돌 없는 표시용 Tilemap이다.
- 일반 화면 작업 기준은36×20.25칸·16:9다. 사용자가 제시한40×22.5를 그대로 확정한 것이 아니다.
- 기본 이동과 충돌을 먼저 고정하고 이후 AI 아트 스킨을 얹는다. 태양꽃·가연물처럼 기능이 있는 핵심 오브젝트는 별도 프리팹이다.
- 새 맵/씬/생성 툴은 사용자 요청 범위에서만 만든다. 이번 TutorialScene 생성은 명시 승인된 예외다.
- 유지한 편집기는 Level Workflow·CameraRoom·MovementPath·PathMovingBlock 편집기다. 삭제된 일회성 생성 툴을 자동 복구하지 않는다.

### skin scene 실험의 최신 상태

`Assets/Scenes/skin scene.unity`는 기존 씬 안의 SkinRoom_01 실험이다. Terrain329셀에서 DoodleTerrainSkin이 연속 돌·둥근 외곽·펜선·윗면 풀을 만들고, 끄면 같은 충돌의 그레이박스로 돌아간다. 타일 편집·삭제·Undo/Redo·Grid/Transform 변경을 편집 모드에서 자동 반영한다.

안쪽 음영 최신은 **밝은 돌 외곽 → 성기고 연한 해칭 → 촘촘하고 진한 해칭 → 검정 중앙**이다. 기본/04 재질은 거친 회흑색 사선 텍스처를 사용하며 Gradient Width1.4U, Solid Core Start.84다. 이전 매끈한 검정 막대·단색 중앙·03만 사용하는 복원 기록보다 이 그라데이션 수정이 최신이다.

최신 실제 Play 캡처는 `ArtReference/SkinScene/HatchingGradient/03_Gradient_Final_Play.png`. 상세는 `Docs/SkinScene_Setup.md`다. 이전 실제 점프4회·가시 사망·체크포인트 복귀 기록은 있지만 모든 능력 경로와 본편 전체를 확인한 결과는 아니다.

### 새 타일셋 계획의 미완료 범위

사용자는 Storyscene을 바로 타일로 다시 꾸미기보다 **현재 새 스토리 아트와 어울리는 타일셋**을 먼저 보고 싶다고 정정했다. 첫 스타일 시안은 `ArtReference/TilesetStyle/01_Story_Tileset_Concept_v1.png`다. 아직 실제 Sprite slicing·RuleTile·Tile Palette·씬 적용은 하지 않았다. 시안에 보이는 부품 개수를 실제 제작 수로 확정하지 않는다. GPT 연동 자동 스킨 툴도 아직 구현 완료된 기능이 아니다.

## 8. 나레이션·이야기 미완료 범위

- 나레이션은 이미지에 글자를 굽지 않고 **편집 가능한 uGUI Text + GamjaFlower-Regular**를 사용한다.
- StorybookNarration의 Lines에서 대사를 바꾼다.
- 기본 타이핑24자/실제초, 글자 완료 뒤2초 대기, 자동 다음 문장/닫기다. 일시정지 중 타이핑은 멈춘다.
- Lock Player Controls는 인스턴스별 선택이다. **모든 나레이션에서 무조건 입력을 잠그지 않는다.** 현재 강가 나레이션은 false다.
- 작은 불꽃과 태양꽃의 반응으로 앰버가 탄생하는 별도 연출은 **DEV01에 구현**했다. 첫 X 교육을 거쳐 기존 두 번째 꽃 탑승/정화 교육으로 이어진다. 상세·이번 확인 범위는 최상단 추가와 `Docs/EmberBirth_Setup.md`를 따른다.
- 전체 스토리/능력 교육 통합 재설계와 본편8개 방 배치·보스 구현 완료를 주장하지 않는다.
- 앰버가 태어난 이유를 찾는 여정이며 “힘이 있으니 희생해야 한다”를 세계관의 정답으로 취급하지 않는다. 우미 선택과 세부 발표 범위는 루트 인수인계를 따른다.

## 9. 확인 작업의 원칙

1. 화면 문제가 있으면 **수정 전 실제 Game View 캡처 → 수정 → 같은 구도 캡처 → 확인**을 반복한다.
2. 문제가 남으면 다시 수정하고 다시 캡처한다. 위치 지정으로 만든 시각 미리보기와 실제 방향키/Z/X/C 입력 진행을 구분한다.
3. Play 모드를 켰다는 것만으로 검증 완료라고 하지 않는다. 실제 프레임 진행·화면·필요한 상태를 확인한다.
4. 현재 씬과 dirty 여부를 먼저 읽고 사용자의 편집을 보존한다. 새 작업 때문에 기존 미저장 상태를 덮어쓰지 않는다.
5. 캡처용 코드가 실제 Edit 모드에서 위치·회전을 바꾸지 않도록 Play 여부를 검사한다.
6. 임시 키보드·입력 라우팅·runInBackground·Rigidbody.simulated·pause 상태는 작업 뒤 복구한다.
7. 실제 Unity 장면·프리팹·재질·Sprite 설정 변경은 Unity API/AssetDatabase를 사용한다. 씬/프리팹 YAML과 `.meta`를 직접 편집하지 않는다.
8. 일반적인 아트 수정에서 자동 테스트를 임의 추가하지 않는다. 수행한 입력 확인·Console·시각 확인과 하지 않은 빌드·전체 완주를 정확하게 구분한다.

## 10. 빠르게 찾을 파일

| 주제 | 문서/경로 |
| --- | --- |
| 최신 전체 기준 | `AGENTS.md`, `pj_ember_구현_인수인계.md` |
| 문서 목차 | `Docs/README.md` |
| 강가 아트·반사·착지 시작점 | `Docs/WaterSequence_PPT4-7_Setup.md` |
| 물 렌더·VFX | `Docs/StoryWater_Setup.md`, `Assets/Ember/Water/StoryWater.mat` |
| 원형 오프닝 | `Docs/CircularOpeningWorld_Setup.md` |
| 원→평지 연결 | `Docs/FlatGround_Setup.md` |
| 첫 가연물 교육 | `Docs/GameTutorial_Setup.md` |
| 태양꽃·탑승·접촉 오염 | `Docs/FlowerRide_Setup.md` |
| C+방향키+Z 교육 | `Docs/IgnitionDash_Setup.md` |
| 나레이션 | `Docs/StorybookNarration_Setup.md` |
| 타일 스킨·해칭 | `Docs/SkinScene_Setup.md` |
| 최신 생성 이미지/프롬프트 | `Assets/Ember/Art/Storybook`, `ArtReference/OpeningWater` |
| 분리 전 안전 백업 | `Assets/Scenes/Backups/Storyscene_BeforeSceneSplit_20261020.unity` |
| 씬 전환·새 씬 시작 코드 | `Assets/EmberPrototype/Runtime/StoryTutorialSceneTransition.cs`, `TutorialSceneEntry.cs` |

## 11. 새 계정에서 이어갈 순서

1. AGENTS 최신 항목과 이 문서를 읽고 현재 Git branch/HEAD·Unity 버전·열린 씬·미저장 상태를 확인한다.
2. 이번 씬 분리의 최종 검증 표와 `ArtReference/SceneSplit`을 읽는다. 비어 있는 확인 결과를 완료로 추정하지 않는다.
3. 실제 현재 Storyscene/TutorialScene과 이벤트 참조를 읽고 사용자 배치·최신 아트가 유지되는지 확인한다.
4. 사용자가 다음 작업을 요청한 범위만 진행한다. 아트는 마지막3개 사용자 확인 여부부터 확인한다.
5. 다음 수정 결과는 실제 화면으로 보여주고, 문서에는 새로 확인한 결과와 남은 범위를 나눠 기록한다.

### 새 채팅 시작 문장 예시

> 이 폴더의 Project Ember 작업을 이어가줘. 먼저 AGENTS.md와 Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md를 읽고, 현재 Unity 씬·미저장 변경·Git 상태를 확인해. 최신 Storyscene/TutorialScene 분리와 사용자가 옮긴 강가 배치를 보존하고, 인수인계에서 아직 확인하지 않았다고 적힌 작업을 완료로 가정하지 마.

## 12. Storyscene 나무 Hierarchy 정리 (2026-10-20)

강가 왼쪽/오른쪽 나무의 현재 모습, 푸른 과거 모습, 물 반사 원본을 각각 `Storyscene/RiverMemory/Trees/Tree_Left` 및 `Tree_Right` 아래에 묶었다. 쌍 그룹을 옮기면 관련 시각 변형이 함께 따라간다. 반사 연출 중 현실의 푸른 나무는 시퀀스가 `Past_Reflection_Source`에 정렬한다. 기존 렌더러 참조·시퀀스 연결과 여섯 오브젝트의 월드 위치는 유지했다. 설정은 `Docs/WaterSequence_PPT4-7_Setup.md`를 따른다. 변경 전 씬 백업: `Assets/Scenes/Backups/Storyscene_BeforeTreeHierarchy_20261020.unity`.

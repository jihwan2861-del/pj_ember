# Project Ember 개발 태스크

최신 결정: **2026-10-21**. **STORY02 — 어두운 길과 정령 구출을 기존 강가 배치에 조립한다.** 기능을 새로 만들지 않고 기존 강가·반사·발판·기억 이벤트·꽃 전달을 하나의 재사용 블록으로 유지한다. 진행은 만화 → 어두운 길 → 정령 구출/대화 → 강가의 과거 기억 → 시든 태양꽃 → TutorialScene이다. 기존 Storyscene에는 이미 이 블록들이 함께 저장돼 있다. 조립 지도를 [Storyscene 조립 지도](Docs/StoryAssembly_Rebuild.md)에 정리했다. 실제 Play/Game View 연속 확인은 남아 있다.

현재 구현·배치의 기준은 [AGENTS.md](AGENTS.md), [전체 인수인계](Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md), 실제 씬·코드다. 이 문서는 실행할 작업과 완료 조건을 관리한다. 이전 목록은 [2026-10-20 갱신 전 원본](Docs/Archive/Technical/TASKS_Before_2026-10-20.md)에 보관했다.

## 현재 확정 기준

- 불의 정령 앰버가 주인공이며 Unity 기준은 **6000.6.5f1**이다.
- 발표용 범위는 **별도 튜토리얼 + 일반 방 8개 + 선택 진입하는 우미 보스 방**이다. 본편 방별 역할과 배치는 아직 완료되지 않았다.
- `Storyscene`은 만화·어두운 길 걷기·정령 구출·강가 반사·나레이션을 맡고, `TutorialScene`은 세 능력 교육을 맡는다. Storyscene 내부는 이어서 걷고, 씬 전환은 페이드와 Single 로드를 사용한다.
- 지형은 **0.5U 그레이박스 Tilemap → 충돌 없는 아트 스킨** 순서로 만든다. 일반 화면 기준은 **36×20.25칸 / 16:9 / Orthographic Size 5.0625**다.
- 사용자의 최신 배치와 삭제한 발판을 보존한다. 기존 이동·지형 충돌 수치를 외형 작업 때문에 변경하지 않는다.
- 아트는 메인 화면 산과 어울리는 단순한 동화풍 낙서다. 새 이미지 **3개마다 실제 씬 적용 화면을 보여주고 확인 후 다음 묶음**으로 진행한다.
- 기본 움직임은 이미 구현된 기반이다. 태스크의 완료 표시는 아래에 적힌 확인 범위에만 해당한다.

## 최신 작업 — STORY02 어두운 길과 빛의 정령

**상태: 코드·씬·이미지 저장 완료 / Unity 런타임 화면 확인 대기.**

오프닝은 세 컷 만화 → Global Light 끄기와 불꽃 조명 → 회전 없이 평지 경사를 따라 걷기 → 오염된 빛의 정령 구출/대화 → 가운데 발판의 강 과거 연출 → 시든 태양꽃 C 반응 → 에너지 전달 → TutorialScene 페이드 전환 순서다. 산 정상의 빛의 근원은 정령이 알려주는 다음 목적지로 설정했다. 새 정령 그림 1개와 기존 보라색 오염 그림을 사용한다.

- [x] 만화 입력 잠금, 3.5초 표시, 0.85초 페이드.
- [x] Global Light 비활성화와 플레이어 추적 Point Light(색상·범위 Inspector 조절).
- [x] 원형 회전 없이 출발 평지를 FlatGroundJoin에 배치하고, 9.99도 경사로 강둑 시작점까지 연결.
- [x] `ScreenExitSceneLoader` 비활성화로 페이드/NextStoryStart 워프 제거. 같은 씬에서 카메라가 시작점부터 따라감.
- [x] 평지 Guide_NPC를 `빛의 정령`으로 교체하고 보라색 오염 그림을 자식으로 배치.
- [x] 정령 대화가 실제 시작될 때 오염 시각을 해제하고, 태양 오염의 결과·산 정상으로 갈 동기를 설명. 대화 중 조작 잠금.
- [x] 기존 `Stone_3` 착지→RiverMemorySequence, 시든 꽃 C→에너지 이동→TutorialScene 전환 연결 보존.
- [ ] Storyscene 전체 실제 입력 진행과 Game View 단계별 캡처: 어두운 시야·평지 접지·정령 구출·대화·물 과거·꽃·전환.

자세한 하이어라키, 배치, 조절 필드는 [Storyscene 설정 문서](Docs/StoryFlow_Setup.md)를 본다. 화면 완주를 아직 실행하지 않았으므로 코드/직렬화 상태와 런타임 검증 상태를 구분한다.

강가 구간의 구성요소와 월드 좌표, 연결 관계는 [Storyscene 조립 지도](Docs/StoryAssembly_Rebuild.md)를 따른다. 사용자 배치와 충돌/이벤트 참조를 보존하고, 화면 확인 뒤 필요한 연결만 손본다.

## 현재 개발 — DEV01 앰버 탄생 → 능력 교육

**상태: 구현·씬 적용·실제 Play 확인 완료. 연출 외형은 첫 적용본이다.**

물가 연출 뒤 TutorialScene에 들어온 작은 불꽃이 태양꽃과 반응해 앰버로 태어나고, 카메라와 조작이 돌아온 뒤 기존 첫 X 교육을 시작한다. 현재 진입 즉시 `ShowEmber()`와 첫 교육을 호출하던 부분에 탄생 과정을 연결한다.

### 실행 체크

- [x] 현재 씬 분리 구조·진입 코드·태양꽃·외형 전환 기능과 미완료 범위 조사.
- [x] 최신 개발 태스크와 탄생 연출의 완료 조건 문서화.
- [x] 코드: 진입 시 작은 불꽃 → 입장 페이드 완료 대기 → 꽃 반응·빛 이동 → `ShowEmber()` → 카메라 복귀·조작 잠금 해제 → 첫 X 교육 순서를 연결.
- [x] 씬: 기존 태양꽃 아트를 재사용해 TutorialScene의 연출 참조와 시작 조건 연결. 새 PNG는 생성하지 않음. 기존 Transform/Collider 값 일치.
- [x] 실제 Play: 직접 시작·Storyscene 경유 모두 확인. 연출 중 Right/X/C 차단, Pause, R 재시작, 명시적 Skip, 비활성화 정리 확인. 종료 후 실제 X→방향키Z 착지/Completed 확인.
- [x] 후속 문서: [탄생 설정과 관찰](Docs/EmberBirth_Setup.md), 인수인계, `ArtReference/EmberBirth` 단계별 Game View 기록.

### 완료 조건

1. TutorialScene 진입 직후에는 작은 불꽃 모습으로 보이고, 입장 페이드가 끝난 뒤 꽃 반응이 시작된다.
2. 태양꽃 반응과 빛 이동이 보인 다음 앰버 외형으로 전환된다. 연출 종료 뒤 카메라가 플레이 구도로 돌아오고 연출이 획득한 조작 잠금이 해제된다.
3. 탄생이 끝나기 전에 첫 X 교육이 시작되지 않는다. 종료 뒤 기존 X 점화·진입 → 방향키+Z 이탈 교육으로 이어진다.
4. 기존 꽃 아트를 재사용하며 새 PNG를 만들지 않는다. 지형·충돌·기본 이동 수치와 Storyscene의 **태양 2초 → 여운 3초 → 물 2초** 연출을 유지한다.
5. 실제 적용 Game View와 확인 결과를 남긴다. 기존 세 교육 완료 기록을 이번 변경 후 전체 재확인으로 간주하지 않는다.

참조: [첫 교육 설정](Docs/GameTutorial_Setup.md), [태양꽃 설정](Docs/FlowerRide_Setup.md), [전체 인수인계](Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md), [현재 씬 진입 코드](Assets/EmberPrototype/Runtime/TutorialSceneEntry.cs).

## 후속 작업

### 최신 연출 계획 — DEV02 강에 남은 과거

**계획 작성 완료 / 구현 전.** 현재 반사→푸른 과거 발견→물 밖에도 잠시 화사한 과거→메마른 현실 복귀의4단계로 바꾸는 방향이다. VFX Graph는 수면 잔빛, Particle System은 꽃가루/잎, 라이팅과 시각 레이어는 현실에 번지는 잔상을 담당한다. 기존 물 셰이더·반사 레이어를 재사용하고, 옛 시듦 완료에 의존하는 다음 씬 연결도 새 연출 완료 기준으로 바꾸도록 계획했다. 마지막 물속 반사의 상태와 NPC 문구/배치는 미정이다. 상세는 [VFX·파티클·라이팅 제작 계획](Docs/RiverMemory_Reveal_Plan.md)을 따른다. 현재 씬/코드는 아직 이전 연출이다.

아래는 DEV01 이후의 작업 순서다. 확인 대기·구상·미확인을 구현 완료로 표시하지 않는다. 새 씬과 맵 배치는 사용자 요청 범위에서 진행한다.

| ID | 상태·순서 | 작업 | 완료 조건·참조 |
| --- | --- | --- | --- |
| STORY02 | 현재 · 런타임 확인 대기 | Storyscene 오프닝부터 에너지 전달까지 실제 진행 캡처 | 만화 → 어두운 길 → 정령 → 중간 돌 기억 → C 꽃 → 페이드 순으로 입력 진행, 필요한 위치에서 캡처·수정. [스토리 플로우](Docs/StoryFlow_Setup.md) |
| ART01 | 다음 · 사용자 확인 대기 | 중간 발판 마른/푸른 짝과 산 배경 3종 검토 | 기존 적용 화면을 사용자에게 보여주고 수정 또는 승인을 기록. [WaterDoodle_Batch3](ArtReference/OpeningWater/WaterDoodle_Batch3), [강가 설정](Docs/WaterSequence_PPT4-7_Setup.md) |
| ART02 | ART01 뒤 | 남은 강가 애셋 교체 | 높은 발판과 푸른 반사 짝·숲·오염 태양·물 중 작업할 묶음을 정해 교체. 반사·시듦 연결과 사용자 배치 유지, 3개마다 실적용 캡처·확인. [강가 설정](Docs/WaterSequence_PPT4-7_Setup.md) |
| TILE01 | 스타일 시안만 존재 | 새 스토리 아트와 어울리는 타일셋 제작 | 역할·규격 확정 후 이미지 제작·Sprite 분할·RuleTile·팔레트 구성과 스킨 실적용 비교. 기존 충돌 보존. [타일셋 시안](ArtReference/TilesetStyle/01_Story_Tileset_Concept_v1.png), [스킨 설정](Docs/SkinScene_Setup.md) |
| STORY01 | 미완료 | 능력 교육 뒤 “앰버는 산을 오릅니다” 연결 | 세 번째 교육 완료 뒤 나레이션과 등반 시작 흐름을 확정하고 연결. [구현 인수인계](pj_ember_구현_인수인계.md) |
| LEVEL01 | 미완료 | 일반 방 8개 설계와 플레이 연결 | 방별 역할·기믹을 정하고 사용자가 제작한 그레이박스에 체크포인트·기믹·출구 연결. [레벨 제작 가이드](Docs/LevelEditingWorkflow.md) |
| TOOL01 | 구상 | Room 데이터 기반 AI 스킨 워크플로우 | 점유셀·좌표·외곽 등 룸 배치 데이터를 받는 범위부터 확정. GPT 연동이나 자동 스킨 생성은 현재 구현된 기능으로 취급하지 않음. [스킨 설정](Docs/SkinScene_Setup.md) |
| CHECK01 | 미확인 | 어두운 Storyscene 오프닝부터 세 능력 교육까지 전체 입력 진행 확인 | 위치·단계 강제 없이 연결 흐름을 실제 입력으로 진행하고 Game View·막힌 지점·완료 범위를 기록. [기존 부분 확인](Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md) |
| CHECK02 | 사용자 요청 시 | 타깃 빌드 확인 | 대상 플랫폼과 빌드 범위를 정한 뒤 수행. 빌드를 하지 않은 현재 상태와 구분해 결과 기록 |
| BOSS01 | 후순위 | 우미 선택과 기존 보스 코드 검토 | 기본 스테이지 이후 현재 우미 선택·기존 물의 정령/파도 구현을 대조하고 후속 범위 확정. 옛 통합 보스 계획 자동 복원 금지. [구현 인수인계](pj_ember_구현_인수인계.md) |

## 완료된 기반과 확인 범위

| 항목 | 현재 완료 범위 | 상세 |
| --- | --- | --- |
| 씬 분리·페이드 | Storyscene → 물 변화 완료 대기 → .65초 페이드아웃 → TutorialScene Single 로드 → .4초 페이드인. 두 씬 저장·Build 목록 등록·분리 전 live 백업 보존 | [전체 인수인계](Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md), [실제 캡처](ArtReference/SceneSplit) |
| 능력 교육 01 → 02 → 03 | 분리 후 X 점화·진입/Z 이탈 → C 꽃 발사/X 탑승/Z 하차·정화 → C+방향키+Z 대시를 실제 입력으로 연속 완료. 낙하 체크포인트 복귀·독립 시작·기존 재시작 확인 | [첫 교육](Docs/GameTutorial_Setup.md), [꽃 탑승](Docs/FlowerRide_Setup.md), [링 대시](Docs/IgnitionDash_Setup.md) |
| 낮은 돌 착지·강가 기억 시작 | 오른 강둑 앞 낮은 돌에 착지 충돌을 추가했고, 새 배치에서는 가운데 Stone_3 착지 이벤트가 RiverMemorySequence를 시작한다. 낮은 돌 착지·새 순서의 런타임 재확인은 대기 | [강가 설정](Docs/WaterSequence_PPT4-7_Setup.md), [스토리 플로우](Docs/StoryFlow_Setup.md) |
| 나레이션 | 편집 가능한 Text/GamjaFlower, 타이핑, 출력 완료 뒤 2초 자동 진행, 나레이션별 선택 조작 잠금. 강가 잠금 false 유지 | [나레이션 설정](Docs/StorybookNarration_Setup.md) |
| 태양꽃·오염 기믹 | Head/Body 분리 낙서 아트 승인. 꽃 불꽃 탑승 최대 2게임초·하차 후 마지막 방향 직진 1게임초. 오염 정화·접촉 사망·복구 확인 기록 | [태양꽃 설정](Docs/FlowerRide_Setup.md) |
| 원형·평지와 강가의 부분 아트 교체 | 원 그림·평지·경사면 채택, 원 안 장식은 기존 시트 유지. 강가 나무·낮은 발판·강둑 등 부분 교체. 중간 발판 짝·산 3종은 ART01 확인 대기 | [전체 인수인계의 아트 기준](Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md) |
| 그레이박스 스킨 실험 | skin scene의 Terrain 329셀에서 연속 돌·둥근 외곽·펜선·풀·해칭 그라데이션과 타일 편집 반영 구현. 본편 전체 스킨 적용·모든 능력 경로 확인 완료는 아님 | [스킨 설정](Docs/SkinScene_Setup.md) |

새 어두운 Storyscene 오프닝부터 에너지 전달까지의 실제 입력 완주, 단계별 캡처, 타깃 빌드는 아직 확인하지 않았다. DEV01 변경 후에는 첫 X/Z 교육까지 확인했으며, 두 번째·세 번째 교육의 이번 연속 재완주는 수행하지 않았다.

## 기록 원칙

- 코드·씬·그림 생성·실제 적용·사용자 승인·입력 확인을 구분해서 상태를 갱신한다.
- 화면 수정은 실제 Game View의 수정 전후를 비교한다. 문제가 남으면 수정하고 다시 캡처한다.
- 현재 씬과 미저장 상태를 먼저 확인하고 사용자의 편집을 보존한다. Git 정리 목적으로 미커밋 파일을 되돌리지 않는다.
- 기존 OpenSpec의 완료 체크는 당시 구현 이력이다. 이번 일반 태스크 목록 갱신을 새 OpenSpec 변경 생성으로 확대하지 않는다.

## 과거 구현 이력

- [이전 TASKS 원본](Docs/Archive/Technical/TASKS_Before_2026-10-20.md)
- [로프 구현](openspec/changes/add-burning-rope-swing/tasks.md)
- [물 접촉 소화](openspec/changes/add-water-contact-extinguishing/tasks.md)
- [물의 정령 AI](openspec/changes/add-water-spirit-chase-ai/tasks.md)
- [옛 14방·통합 보스 계획과 제거 기록](openspec/changes/add-fourteen-rooms-water-spirit-escape/tasks.md)

과거 바람 콘셉트·14방 목록·픽셀 8×8 기준을 현재 작업으로 자동 복원하지 않는다.

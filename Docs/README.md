# Project Ember 문서 안내

정리일: 2026-10-17. 현재 작업 기준, 기능 배치 설명, 과거 기획을 찾는 시작점이다.

## 먼저 읽을 문서

| 문서 | 읽는 목적 |
| --- | --- |
| [다른 계정용 전체 인수인계](../Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md) | 2026-10-20 실제 Storyscene/TutorialScene 분리, 최신 아트·배치·검증·남은 작업. 새 계정의 첫 시작점. |
| [AGENTS.md](../AGENTS.md) | 최신 결정과 작업 방식. 씬·맵은 사용자가 제작하며 한 번에 확인받은 작업 하나를 진행한다. |
| [구현 인수인계](../pj_ember_구현_인수인계.md) | 이야기·발표 범위·튜토리얼 동선 참고. 과거의 씬 자동 생성 계획보다 최신 사용자 지시를 우선한다. |
| [불 버전 인수인계](AI/FireProject_Handoff.md) | 기존 기능과 작업 이력, 실제 코드와 대조할 기준. |
| [세계관과 아트 방향](WorldAndStageDirection.md) | 동화풍 아트와 세계관 참고. 문서 안 이전 시점의 내용은 최신 결정과 구분한다. |
| [타이틀 다음 작업](../다음%20작업.md) | 타이틀 화면과 메뉴 연출 계획. 구현 완료 목록이 아니다. |
| [작업 목록](../TASKS.md) | 기존 작업 후보와 진행 기록. 이전 14방 목록은 최신 범위 확정으로 해석하지 않는다. |

기본 움직임은 기존 구현을 사용한다. 사용자가 타일맵으로 동선을 만든 뒤 확정한 충돌 위에 동화풍 이미지를 겹친다. 상단 나레이션은 편집 가능한 폰트·타이핑·자동 진행·나레이션별 선택 조작 잠금을 지원한다.

## 제작할 때 찾을 설명서

| 작업 | 문서 |
| --- | --- |
| 앰버 탄생·태양꽃 반응·능력 교육 연결 | [탄생 연출 설정](EmberBirth_Setup.md), [현재 태스크](../TASKS.md) |
| 방·타일맵·카메라 배치 | [레벨 제작 가이드](LevelEditingWorkflow.md) |
| skin scene·낙서 지형·자동 외곽·그레이박스 비교 | [낙서 스킨 실험 설정](SkinScene_Setup.md) |
| 강가0.5U 그레이박스·자동 돌 스킨·분리 오브젝트 | [강가 키트 사용법](ShoreKit_Setup.md) |
| 상단 나레이션·대사 편집·타이핑·자동 진행·선택 잠금 | [나레이션 설정](StorybookNarration_Setup.md) |
| 첫 게임 튜토리얼·그레이박스 타일맵 | [X 점화·진입과 Z 이탈 설정](GameTutorial_Setup.md) |
| 두 번째 튜토리얼·태양꽃 불꽃 탑승 | [태양꽃 발사·탑승·조향 설정](FlowerRide_Setup.md) |
| 세 번째 튜토리얼·C 링과 방향키+Z 대시 | [점화 대시·그레이박스 편집 설정](IgnitionDash_Setup.md) |
| 화염꽃·태양초와 오염 정화 | [화염꽃 배치](FireFlower_Setup.md) |
| 불붙은 로프 | [로프 설정](Rope_Setup.md) |
| 물 접촉과 소화 | [물 접촉 설정](WaterContact_Setup.md) |
| PPT 4~7 물가·발판·밝은 과거 반사 | [최신 설정과 캡처](WaterSequence_PPT4-7_Setup.md), [물 패키지 조사](WaterPackageAssets_Audit.md) |
| 움직임·재시작·일시정지 | [세션 조작](GameSessionControls_Setup.md), [이동 분석](AI/PlayerMovementAnalysis.md) |
| 움직이는 발판 | [무게 발판](WeightPlatform_Setup.md), [플레이어 운반 수정 기록](AI/MovingPlatform_Rider_Fix.md) |
| 물의 정령과 파도 | [물의 정령](WaterSpirit_Setup.md), [파도](WaterWave_Easy_Setup.md) |

스테이지 추출 아트는 `Assets/Ember/Art/Storybook/StageExtracted/`에 있다. 이미지 모음이며 기믹과 충돌이 연결된 프리팹은 아니다.

## 과거 기록과 참고자료

[Archive 목록](Archive/README.md)에 현재 기준에서 벗어난 기획·일정·상태 문서 5개를 모았다. 파일은 삭제하지 않았다.

`Design/Ember_20Minute_Design_v1.md`, `Ember_Reverse_Design_Document.md`, 기존 `MainMenu.md`도 작성 당시 기획·구현 참고로 읽는다. 이번 보관 이동 대상에는 포함하지 않았다.

루트 `ArtReference/`는 이미지와 제작 프롬프트, `DriveImport/`는 전달받은 인수인계 원본이다. 드라이브의 독립 인수인계 문서와 압축 해제 폴더 안 문서는 내용이 동일하지만 원본을 유지한다. `openspec/`과 `Validation/`, `ValidationArtifacts/`는 설계·검사 이력으로 보존한다.

툴 정리 내역과 각 스크립트의 용도는 [Unity 툴 정리 기록](Archive/Technical/EditorToolCleanup_2026-10-17.md)을 참고한다. 현재 일반 편집 메뉴는 Level Workflow를 사용하며, 씬·프리팹 생성 및 자동 설정 메뉴는 정리했다.

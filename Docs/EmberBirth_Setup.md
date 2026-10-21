# 앰버 탄생 연출 — TutorialScene 진입

작성 기준: **2026-10-20**. Storyscene의 물가 연출 뒤 작은 불꽃과 태양꽃이 반응해 앰버가 태어나고, 기존 첫 X 능력 교육으로 이어지는 연출이다.

사용자는 이번 개발을 앰버 탄생 → 능력 튜토리얼 연결로 선택했다. 기존 태양꽃 그림을 재사용했으며 **새 PNG는 만들지 않았다**. 지형·충돌·기본 이동 수치와 Storyscene의 태양 2초 → 여운 3초 → 물 2초 흐름은 유지한다.

## 1. 씬과 코드

| 대상 | 경로·역할 |
| --- | --- |
| 적용 씬 | `Assets/Scenes/TutorialScene.unity` |
| 씬 진입 | `Assets/EmberPrototype/Runtime/TutorialSceneEntry.cs` |
| 탄생 진행 | `Assets/EmberPrototype/Runtime/EmberBirthSequence.cs` |
| 외형 전환 | `Assets/EmberPrototype/Runtime/StoryPlayerAppearance.cs`의 `ShowFire()` / `ShowEmber()` |
| 연출 전 백업 | `Assets/Scenes/Backups/TutorialScene_BeforeEmberBirth_20261020.unity` |

`TutorialSceneEntry`가 기존 EntryCheckpoint **(102.11,.35)**와 PracticeRoom **(105.51,2)**, 카메라 Size **5.0625**를 초기화한다. 새 선택 참조 `Birth Sequence`가 연결돼 있으면 탄생 완료 뒤 `BeginAbilities()`를 호출한다. 참조가 없거나 연출을 시작할 수 없는 경우에는 기존처럼 앰버 외형과 첫 교육으로 바로 연결한다.

탄생 진행 중 첫 `FlammableEntryTutorial`은 **Idle**을 유지한다. `ShowEmber()`로 모습이 바뀌었다는 것만으로 첫 교육을 시작하지 않는다. 연출의 카메라 복귀와 조작 잠금 해제까지 끝나야 `BeginTutorial()`이 호출된다.

## 2. 배치와 재사용 아트

- 연출 루트: **EmberBirth**, 월드 위치 **(100.35,0)**.
- 자식 **BirthFlowerVisual**: 정식 `SunFlower.prefab`의 Head/Body 그림을 재사용한 시각 전용 꽃, 축척 **.65**.
- 시각 전용 꽃에는 **FireFlower와 Collider가 없다**. 이 꽃은 능력 발사·접촉 활성화·충돌 대상이 아니다. 뒤의 기존 능력 교육용 꽃은 그대로 유지한다.
- 카메라 포커스: 월드 **(102,.95)**. 이 씬의 `Camera Zoom`은 **.6**이다. 코드의 새 컴포넌트 기본값 **.7**과 구분한다.
- 플레이어는 `StoryPlayerAppearance.startAsFire=true`로 시작하고, 저장된 Sprite 정렬은 **EmberWater / Order 10**이다.

Head/Body의 회전축과 기존 낙서 그림을 재사용한다. 공명 중 Head의 크기·회전과 꽃잎 색을 잠시 바꾸고, 연출 종료 또는 취소 때 시작값으로 돌린다. 기존 지형·Collider 직렬화 값은 적용 전후 일치함을 확인했다.

## 3. 연출 순서와 시간 설정

시간 단위는 **게임 시간**이다. 입장 페이드 대기는 아래 3.75초에 포함되지 않으며, 프레임 단위 진행 때문에 실제 관찰 시간에는 작은 오차가 생길 수 있다.

| BirthPhase | 기본 시간 | 화면·진행 |
| --- | --- | --- |
| WaitingForFade | 입장 페이드·재시작 와이프·일시정지 종료까지 | 작은 불꽃으로 표시하고 조작 잠금 획득. 첫 교육은 Idle 유지 |
| Resonating | .9초 | 꽃 Head가 미세하게 커지고 기울며 꽃잎이 따뜻한 색으로 밝아짐. 꽃에서 작은 빛이 맺힘 |
| ReceivingLight | .85초 | 꽃의 빛이 곡선을 따라 플레이어 쪽으로 이동 |
| Transforming | .55초 | 진행의 **35%** 시점에 `ShowEmber()` 호출. 플레이어 주위 빛·링·작은 불씨가 퍼짐 |
| Revealing | .9초 | 빛 효과를 숨기고 새 앰버 모습을 보여줌. 첫 교육은 아직 Idle |
| Returning | .55초 | CelesteRoomCamera의 연출 모드를 끝내고 기존 플레이 구도로 복귀 |
| Completed | 종료 처리 | 꽃·효과·카메라 정리, 소유한 조작 잠금 해제, 첫 X 교육 시작 |

총 설정 시간은 **.9 + .85 + .55 + .9 + .55 = 3.75게임초 + 입장 페이드 대기**다. Inspector의 `Timing — Game Seconds`에서 단계별 시간을 조정한다. `Camera Zoom`으로 연출 카메라 확대 비율을 조정한다.

카메라는 `CelesteRoomCamera.BeginCinematic()` / `EndCinematic()`으로 기존 방 카메라의 연출 기능을 사용한다. 종료 뒤 PracticeRoom 중심 **(105.51,2)**, Size **5.0625**의 플레이 화면으로 돌아간다.

### 빛 효과

`Light`의 색은 황금빛 **RGBA (1,.68,.1,1)**이며 기본 정렬은 **EmberWater / Order 30**이다. 런타임에 `Birth Light`와 LineRenderer **6개**를 만든다.

- Flower Light 1개: 꽃에서 맺혀 플레이어에게 이동하는 빛.
- Birth Ring 1개: 외형 전환 때 확장되는 링.
- Spark 4개: 이동하는 빛과 외형 전환 주변의 작은 불씨.

새 이미지 애셋이나 별도 VFX Graph를 생성하지 않는다. 효과 오브젝트와 임시 Material은 완료·스킵·취소 때 제거한다.

## 4. 조작 잠금·취소·다시 재생

- 연출이 자신의 조작 잠금을 획득하고, 종료 때 그 잠금만 해제한다. 다른 시스템이 이미 플레이어를 잠근 경우에는 탄생 연출을 시작하지 않는다.
- 일시정지 중에는 `Time.deltaTime`이 진행하지 않아 단계 시간과 빛 이동이 멈춘다. 입장 페이드·재시작 와이프가 진행 중일 때에도 연출 시작을 기다린다.
- **`SkipBirth()`**를 명시적으로 호출하면 연출을 정리하고 앰버로 전환한 뒤 첫 교육으로 연결한다. 별도 스킵 키를 새로 지정한 기능은 아니다.
- 실행 도중 컴포넌트나 오브젝트가 **비활성화**되면 완료 콜백을 버린다. 꽃·효과·카메라·소유 잠금은 정리하지만 **첫 교육을 자동 시작하지 않는다**.
- 한 씬 실행에서 한 번 진행한다. 일반 사망·체크포인트 복귀 때 처음부터 재생하지 않는다. **R로 씬 재시작**하면 새 씬 인스턴스에서 다시 탄생을 진행한다.

## 5. 실제 Play 확인 기록

아래는 이번 변경에서 확인한 결과다. 위치를 지정한 시각 미리보기와 실제 입력 진행을 구분한다.

| 항목 | 관찰 결과 |
| --- | --- |
| TutorialScene 직접 시작 | 작은 불꽃 / 조작 잠금 true / 첫 교육 Idle에서 시작 |
| 빛을 받는 동안의 입력 | Right/X/C를 입력해도 플레이어 위치 **(102.11,.35)**를 유지. 첫 교육이 먼저 시작되지 않음 |
| 앰버 외형 전환 | `ShowEmber()` 이후에도 연출 종료 전에는 첫 교육 Idle 유지 |
| 정상 종료 | Completed, 조작 잠금 false, 첫 교육 EnterFlammable. 카메라 **(105.51,2)** / Size **5.0625** 복귀 |
| 종료 뒤 첫 X 교육 | 실제 X 입력으로 목표 **(104.51,2)**에 진입. Right+Up+Z 이탈 뒤 **(110.23,.81)** 착지, Completed / FireLaunchCount 1 |
| 일시정지 | ReceivingLight 중 일시정지 전후 게임 시간 **1.34419525**와 단계가 유지됨 |
| R 재시작 | 씬을 다시 시작한 뒤 새 Resonating 단계 진입 확인 |
| 실행 도중 비활성화 | 효과 0개 / 카메라 연출 false / 조작 잠금 false / 첫 교육 Idle. 취소가 첫 교육을 호출하지 않음 |
| 지형·아트 보존 | 기존 지형·Collider 값 일치. 새 PNG 없음 |

### 추가 확인

- **SkipBirth 호출 뒤 첫 교육 연결:** Skipped / 앰버 외형 / 잠금 false / 첫 교육 EnterFlammable / 효과 없음 / 카메라 연출 false를 확인했다.
- **Storyscene 경유 입장 페이드 → 탄생 → 첫 교육:** 확인을 위해 플레이어를 낮은 돌 위에 배치한 뒤 자연 착지로 기존 물/나레이션을 시작했다. 새 씬 로드 직후 WaitingForFade / 작은 불꽃 / 잠금 true / 첫 교육 Idle / 페이드 true였으며, 페이드 종료 뒤 Resonating으로 진행했다. 최종 Completed / 잠금 false / 첫 교육 EnterFlammable / 효과 없음 / 카메라 연출 false를 확인했다. 전체 원형 시작부터 입력 완주한 결과는 아니다.

최종 적용 캡처 경로는 **`ArtReference/EmberBirth`의 03~07·09~10**이다. 08은 스토리에서 새 씬으로 들어오는 페이드 화면이다. **01~02는 연출 구도 왼쪽에 흰 여백이 보이던 수정 전 화면**이므로 최종 결과로 사용하지 않는다. 관찰값은 같은 폴더 `PlayObservations.txt`다.

이번 변경 후 두 번째 꽃 탑승·세 번째 C+Z 교육을 새로 연속 완주한 것은 아니다. 기존 세 교육의 분리 후 연속 완료 기록은 [계정 인수인계](../Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md)를 따른다. 전체 원형 오프닝부터의 입력 완주와 빌드는 이번 확인 범위에 포함하지 않는다.

## 6. 관련 문서

- [현재 개발 태스크](../TASKS.md)
- [첫 X 점화·진입 교육](GameTutorial_Setup.md)
- [태양꽃 발사·불꽃 탑승 교육](FlowerRide_Setup.md)
- [세 번째 C+Z 대시 교육](IgnitionDash_Setup.md)
- [Storyscene 물가 연출](WaterSequence_PPT4-7_Setup.md)
- [계정 인수인계](../Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md)

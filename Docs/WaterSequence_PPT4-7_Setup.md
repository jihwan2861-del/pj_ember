# PPT 4~7 물가와 과거 반사 연출 — 2026-10-18

## 2026-10-20 최신 — 강가 연출 뒤 TutorialScene으로 전환

사용자 승인으로 능력 교육3개를 `Assets/Scenes/TutorialScene.unity`로 분리했다. Storyscene의 낮은 돌 착지→태양2초/여운3초/물2초·나레이션은 유지한다. PastNarration.onFinished는 이제 StoryTutorialSceneTransition.BeginTransition이며 물 변화가 끝나면 기존 페이드로 TutorialScene을 Single 로드한다(.65초 아웃/.4초 인). 과거 01.QueueBegin 연결과 강가 오른쪽 회색 튜토리얼 직접 연결 설명은 이전 상태다. 실제 착지부터 새 씬 로드·세 교육 완료를 확인했다. 상세·백업·계정 이전 방법은 [종합 인수인계](../Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md), 캡처는 `ArtReference/SceneSplit`이다.

## 오른 강둑 앞 낮은 돌 착지와 바다 변화 시작점 — 2026-10-20

사용자 후속 정정: 이곳은 **기존 스토리 전체의 시작 위치를 옮긴 포인트**다. 현재 착지→기존 태양 반사 전환/여운→물 변화와 기존 나레이션→종료 후 첫 능력 튜토리얼 대기 연결을 사용한다. 옛 위치의 별도 발동은 없다. 나레이션은 기존 순서대로 착지 약5게임초 뒤 물 변화 시작과 함께 나온다. 시작점 이동 외에 타이밍을 추가 변경한 것은 아니다.

사용자가 오른 강둑 앞 낮은 돌에 올라갈 수 없으며 그곳에 도착하면 바다가 변하길 요청했다. 사용자는 삭제한 물체와 관련이 있을 수 있다고 설명했다. 현재 그림의 낮은 돌은 X74.7~79.1 부근이지만 기존 FarShore Polygon은 X79.62부터 시작해, 해당 그림 아래에 착지할 충돌이 없었다. 삭제한 물체를 되살리거나 현재 아트·맵을 이동하지 않고 필요한 착지 판정만 추가했다. 새 이미지는 생성하지 않았다.

- `StoryShoreSection/FarShore/Art/ArrivalPlatform`에 solid BoxCollider2D를 추가했다. 월드 범위는 X74.6~79, 윗면Y-1.5, 밑면Y-3.15다.
- 그 아래 `WaterChangeOnLanding` 자식의 trigger는 월드 중심(76.8,-.8), 크기4.15×1.5이며, `PlayerEventTrigger`의 `Require Grounded=true`, `Landing Surface=ArrivalPlatform`을 사용한다. 착지 이벤트는 `River.BeginTreeWithering`을 호출한다.
- 기존 FarShore Polygon·비활성 BoxCollider·Art Transform과 사용자가 수정한 배치를 유지했다.
- River의 `Wither Trees By Player Position=false`로 기존 X80 자동 시작을 해제했다. 새 낮은 돌에 착지하면 정상 태양 반사→오염 반사2게임초, 여운3게임초, 나무·발판·물빛 변화2게임초 순서로 진행한다. 기존 나레이션 이벤트도 유지했다.

`PlayerEventTrigger`에 착지 조건을 선택하는 옵션을 추가했으며 기본값은 false라 기존 트리거는 이전처럼 작동한다. 지정한 발판에 실제 접지해야 통과하며, `OnTriggerStay2D`에서도 재검사한다. 공중에서 trigger에 먼저 들어와 조건이 실패해도 일회 이벤트를 소비하지 않으므로, 같은 영역 안에 착지한 뒤 정상 호출할 수 있다.

실제 오른쪽·Z 입력으로 수정 전후를 비교했다. 출발(71.12,-.66)에서 수정 전에는(74.88,-1.93)으로 떨어져 사망하는 현상을 재현했다. 수정 후에는(74.61,-1.26)에서 grounded=true, IsDying=false, HasTriggered=true와 태양 진행값 약.00387을 확인했다. 이는 낮은 돌까지의 점프·착지와 연출 시작 확인이며 전체 오프닝 입력 완주는 아니다. 캡처와 관찰 기록은 `ArtReference/OpeningWater/ArrivalLedgeFix`에 보관한다. 기존 미저장 변경이 있는 씬은 저장하거나 되돌리지 않고 유지했다.

최종 재확인에서는 오른쪽·Z로(74.60,-1.26)에 착지한 뒤 키를 해제하고6.992096게임초 더 기다렸다. 플레이어가(74.77,-1.16), grounded=true로 새 발판에 안정적으로 머문 상태에서 나무·물 변화와 태양 진행이 모두1에 도달했으며 `04_After_WaterChanged.png`로 캡처했다. 별도 공중 관찰에서는(76.8,-.37)에서 grounded=false, HasTriggered=false, 태양/나무 진행0으로 착지 전 연출이 시작되지 않음을 확인했다. 입력 라우팅이 준비되지 않아 입력이 남았던 중간 캡처 시도는 완료 근거로 사용하지 않는다.

최종 재확인에서는 같은 점프 뒤 키를 해제하고 추가 6.992096게임초를 기다렸다. 플레이어는(74.77,-1.16)에서 grounded=true로 안정적으로 서 있었고 Sun/Water 진행값이 모두1이 됐다(`04_After_WaterChanged.png`). 별도 공중 진입(76.8,-.37)은 grounded=false/HasTriggered=false/Sun0/Water0이며, 자연 낙하로 착지(76.8,-1.18)한 뒤 HasTriggered=true였다(`05_Airborne_NoTrigger.png`, `06_Landed_Center_Triggered.png`). 프리뷰의 임시 키보드·입력 라우팅은 원래 설정으로 복구하고 Play를 종료했다.

## 중간 발판 마른/푸른 짝과 산 배경 3종 — 2026-10-20, 사용자 확인 대기

사용자가 태양꽃 머리·몸통과 접촉 위험 오염 3종의 적용 화면에 “좋다 다음가자”라고 답해 다음 묶음으로 진행했다. 해당 승인과 기능 확인은 [태양꽃 설정](FlowerRide_Setup.md)의 최신 항목을 따른다. 이번에는 내장 image_gen으로 기존 그림을 참조 편집해 다음 PNG3개를 만들었다. 경로는 모두 `Assets/Ember/Art/Storybook/WaterDoodle_v1`이다.

| PNG | 현재 Storyscene 적용 Renderer |
| --- | --- |
| Stone_Mid_Doodle_v1.png | StoryShoreSection/SteppingStones/Stone_2/Art |
| Stone_Mid_Healthy_Doodle_v1.png | 위 Art의 HealthyReflection |
| Mountains_Doodle_v1.png | StoryShoreSection/MountainBackdrop |

중간 발판 두 그림은1254×1254·중앙pivot·PPU100으로 원본과 규격이 같다. 마른 그림과 푸른 그림의 돌 배치는 같고 풀·꽃만 다르게 해 반사의 시듦을 유지한다. 건강한 반사 그림은 기존 Art의 자식 local identity와 EmberPastReflection 레이어31을 유지한다. 산은1915×821·중앙pivot·PPU100이며 원본1916×821보다 가로1픽셀(약0.0522%) 작다. 이번에는 Transform을 보정하지 않았다. Single/Full Rect로 가져와 반사와 가로 반복에 사용한다.

현재 씬의 Sprite 참조3개만 교체했다. 모든 Transform·Collider·WaterSurface2D·ParallaxLayer의 직렬화 값이 적용 전후 같음을 확인했다. 사용자가 수정한 배치와 삭제한 Stone_5를 유지하며 복구·재생성하지 않았다. 원본 PNG와 프리팹은 보존했고, 높은 Stone_3와 그 푸른 반사는 아직 기존 수채화 그림이다. 이번 적용 전부터 씬이 dirty였으므로 씬을 저장하거나 사용자의 미저장 변경을 되돌리지 않았다.

전체 프롬프트와 실제 Play 비교 캡처는 `ArtReference/OpeningWater/WaterDoodle_Batch3`에 보관한다. 넓은 비교 구도는 camera(66,0,-10)·Orthographic Size8, 중간 발판 확대 비교는 camera(61.5,.2,-10)·Orthographic Size5다. 수정 전후 푸른 반사 상태를 각각 같은 구도로 캡처했다. 플레이어를 비교 위치에 고정한 시각 확인이며 전체 입력 이동·발판 완주 확인은 아니다. 이번3개 적용 화면을 사용자에게 확인받은 뒤 다음 이미지 묶음으로 진행한다.

`05_After_Close_Withered_Play.png`는 기존 `BeginTreeWithering`을 직접 호출한 뒤7.003263게임초에 TreeWitherProgress/SunTransitionProgress가 모두1인 실제 Play 화면이다. 새 중간 발판의 푸른 풀이 마른 풀로 바뀌고 꽃이 사라지며, 어두워진 물빛과 산 반사가 이어지는 것을 확인했다. 원래 X80까지 실제 입력으로 이동해 도달한 검증은 아니다.

## 두 번째 낙서3종·사용자 최신 배치 유지 — 2026-10-20

사용자가 첫 묶음 화면 후 다음 차례를 요청했다. 이번 PNG3개도 내장 image_gen으로 만들었으며 Assets/Ember/Art/Storybook/WaterDoodle_v1에 있다.

| PNG | 적용 대상 |
| --- | --- |
| Stone_Low_Doodle_v1.png | SteppingStones/Stone_1/Art, Stone_4/Art |
| Stone_Low_Healthy_Doodle_v1.png | 위 두 Art의 HealthyReflection |
| Shore_Right_Doodle_v1.png | FarShore/Art |

발판 둘은1254×1254, 오른 강둑은1774×887이며 원본과 같은 캔버스·중앙pivot·PPU100이다. Single/Full Rect·Bilinear·Uncompressed·mipmap 꺼짐으로 가져왔다. 돌 그림을 다시 이동·확대하지 않고 Sprite 참조5개만 교체했으므로 전체 씬 Transform 직렬화 값, Collider 값, WaterSurface2D 설정이 적용 전후 일치한다. 원본 PNG·프리팹은 보존했다. 낮은 발판의 마른/푸른 그림은 돌 위치가 같고 풀·꽃만 달라진다. 중간/높은 발판은 이번 묶음에서 바꾸지 않았다.

사용자가 작업 도중 현재 오브젝트 배치를 바꿨다고 알려 적용 직전 live씬을 다시 읽었다. FarShore/Art는 기존보다 왼쪽4.75U/아래.919U인 world(83.04,.33)이고 오른 마른 나무는(80.63,2.83), 건강한 반사 나무는(80.69,2.77)이었다. 이 미세 오프셋과 왼쪽 반사 나무 이동도 유지했다. 그림을 이전 Collider 경계로 자동 재배치하지 않았다. 적용 직전 사용자가 씬을 저장한 clean 상태였고, 그 배치 위에 그림만 교체해 저장했다.

실제 Play 비교와 전체 프롬프트는 ArtReference/OpeningWater/WaterDoodle_Batch2에 있다.01_Before_Play/02_After_Healthy_Play는 같은 카메라(74.5,.5,-10),Orthographic Size9이며 현재 배치의 오른 강둑과 낮은 발판4를 담았다. 플레이어는 발판4 기준 위치 지정·물리 정지로 비교 구도만 고정했다. 이동 난이도/착지 완주 확인은 아니다. 이번3개 화면을 사용자에게 확인받은 뒤 다음 묶음을 만든다.

03_After_Withered_Play는 같은 구도에서 기존 BeginTreeWithering을 직접 호출한 뒤7.002079게임초에 태양/나무 진행값1이 된 화면이다. 새 발판의 푸른 풀이 마른 반사로 바뀌고 물빛이 어두워지는 것을 확인했다. 원래 X80까지 이동한 플레이 검증은 아니다. Play 종료 후 FarShore/Art(83.04,.33)를 다시 확인했고 현재 컴파일 실패·Console 오류0이다. PNG참조 적용 시점에는 clean씬 위에 저장을 완료했으나, 캡처 후 현재 씬은 dirty여서 미저장 상태를 추가 저장/되돌리기하지 않고 유지했다.

이후 태양꽃 머리·몸통2개와 접촉 위험 오염물1개의 낙서 PNG를 생성·적용했고, 사용자가 적용 화면을 승인했다. 최신 애셋 경로와 기존 기능·충돌을 확인한 범위는 [태양꽃 설정](FlowerRide_Setup.md)의 2026-10-20 항목을 따른다.

## 최신 강가 아트3종 적용 — 2026-10-20, 사용자 확인 대기

사용자는 메인 화면의 산처럼 단순한 낙서풍으로 강가 새 애셋을 만들도록 선택했다. 첫 마른 나무를 좋다고 평가했고, 원 안 장식만 원래대로 두며 마음에 들었던 평지·경사면을 복구한 뒤 애셋 작업을 계속 요청했다. 적용한3개 PNG는 모두 Assets/Ember/Art/Storybook/WaterDoodle_v1에 있다.

| PNG | 적용 Renderer |
| --- | --- |
| Shore_Left_Doodle_v1.png | StoryShoreSection/ShoreBankOverlay |
| Tree_Dry_Doodle_v1.png | StoryShoreSection/DeadTree 및 FarShore/DeadTree |
| Tree_Healthy_Doodle_v1.png | PastReflectionSources/HealthyLeftTree 및 HealthyRightTree |

내장 image_gen을 사용했고, 원본 PPT 그림은 형태 참고·메인 화면 산과 제목 화면은 스타일 참고로 썼다. 건강한 나무는 새 마른 나무에 푸른 잎과 흰 꽃을 더한 짝이다. 두 그림은1536×1024의 같은 캔버스·중앙pivot이며 건강한 그림의 월드Transform/flip을 실제 마른 나무와 일치시켜 반사 뿌리와 줄기를 맞췄다. 강둑과 실제 나무는 원본 불투명 외곽(alpha128)에 맞춰 표시 크기·접점을 보정했다. Single Sprite·PPU100·Full Rect·Bilinear·Uncompressed·mipmap 꺼짐이며 원본 PNG는 보존했다.

Collider와 WaterSurface2D의 직렬화 설정을 적용 전후 비교해 같음을 확인했다. 기존 반사 배열8개·레이어31·태양 참조·2초 태양/3초 여운/2초 시듦 연출은 유지했다. 발판·물·오염·배경 산·숲은 이번3개 묶음에서 교체하지 않았다. 기존 프리팹 원본을 덮어쓰지 않고 현재 Storyscene의 표시 참조만 바꿨다.

프롬프트와 실제 Play 비교 캡처는 ArtReference/OpeningWater/WaterDoodle_v1에 있다.01_Before_Play와02_After_HealthyReflection_Play는 같은 카메라(53,-1,-10),Orthographic Size9다. 플레이어는 비교 구도에 위치 지정하고 Rigidbody2D.simulated를 잠시 꺼 고정했다. 이동·충돌 난이도 완주 확인은 아니다. 시각 확인 뒤 Play 종료로 임시 상태를 복원한다. 이번3개 적용 화면을 사용자에게 보여주고 확인받은 뒤 다음 이미지 묶음을 진행한다.

03_After_WitheredReflection_Play는 같은 구도에서 BeginTreeWithering을 직접 호출하고 기존 연출이 끝난 화면이다. TreeWitherProgress/SunTransitionProgress가 모두1이 됐으며 실제 게임 시간7.000394초였다(WitherCaptureState.txt). 푸른 잎·흰 꽃이 사라진 뒤 마른 가지 반사와 어두운 물빛을 확인했다. X80까지 실제 입력으로 걸어 도달한 검증은 아니다. 마지막 Storyscene은 저장된 시작0도·clean이고 재생은 종료했다. 현재 컴파일 실패·Console 오류는0이었다.

## 최신 반사 흐름 — 2026-10-19

강이 처음 보일 때부터 푸른 풍경이 비친다. 건너편 X80에 도착하면 다음 순서로 진행한다.

1. 0~2게임초: 물속 정상 태양만 오염된 태양 반사로 바뀐다. 물빛과 나무·풀은 푸른 상태다.
2. 2~5게임초: 태양 변화가 끝난 상태를 3초 유지한다.
3. 5~7게임초: ‘세상은 조금씩 빛을 잃어갔습니다.’ 나레이션을 시작하고 나무2개·발판5개의 풀·꽃이 마른 모습으로 전환된다. 물속 하늘과 물빛도 어두운 청회색으로 바뀐다.

앰버 반사의 밝기는 보존한다. 나레이션은 기존 24자/실제초·완성 후2실제초 자동 닫기이며 조작 잠금은 꺼져 있다.
위쪽 실제 풍경은 그대로다.
이 설정이 아래의 과거 위치 기반 등장 기록보다 우선한다.

StoryShoreSection/River의 **Water Surface 2D**에서 조절한다.

| Inspector | 현재 값 / 역할 |
| --- | --- |
| Start With Past Reflection | 켜짐. 처음부터 푸른 풍경 반사 |
| Reveal By Player Position | 꺼짐. 접근하면서 과거가 처음 나타나던 흐름을 중단 |
| Wither Trees By Player Position | 켜짐 |
| Tree Wither Start X | 80. 플레이어가 도달하면 한 번 시작 |
| Tree Wither Seconds | 2. 게임 시간 기준이며 [ / ] 배속 적용 |
| Separate Sun Before Withering | 켜짐. 태양과 물의 변화를 분리 |
| Past Sun Reflection Source | PastSunSource |
| Sun Transition Seconds | 2. 태양 반사 전환 시간 |
| Hold After Sun Seconds | 3. 태양 완료 후 물 변화까지의 간격 |
| Withering Reflection Layers | 나무2개 + 발판5개 + 정상 태양1개 |
| On Tree Withering Started | PastNarration.BeginNarration |
| On Past Revealed | 나레이션 연결 제거 |
| Preview Past Blend / Preview Tree Withering / Preview Sun Transition | 편집 미리보기: 1 / 0 / 0. 태양과 나무·물을 따로 미리보기 |

### 분리 구현 확인

실제 Play의 Game View에서 수정 전·태양만 완료·물 변화 중간·최종 상태를 캡처했다. 태양 완료 2.002게임초에 Sun=1/Water=0/나레이션 꺼짐, 그 뒤4.000게임초에 Sun=1/Water=0.501/나레이션 켜짐, 최종 Sun=1/Water=1을 확인했다. 전체 오프닝 이동 완주를 확인한 것은 아니다.

캡처: `ArtReference/OpeningWater/BeatSplit/BeatSplit_Before.png`, `BeatSplit_01_SunOnly.png`, `BeatSplit_02_WaterChanging.png`, `BeatSplit_03_WaterFinished.png`.

`BeginTreeWithering`은 분리 옵션이 켜졌을 때 전체 순서를 시작한다. `On Sun Transition Started`는 즉시, 기존 `On Tree Withering Started`는 물 변화 시작에 한 번 호출한다. `TreeWitherProgress`는 마지막 물 변화의 진행값이므로 첫 튜토리얼의 완료 대기 조건을 유지한다. `ResetPastReflection`은 두 진행과 대기를 모두 초기화한다. 일시정지·화면 페이드 동안 진행은 정지한다.
| Darken Sky With Withering | 켜짐. 시듦과 동시에 반사 하늘·물빛을 어둡게 함 |
| Withered Sky Brightness | 0.24. 종료 시 하늘 밝기 배율. 낮을수록 어두움 |
| Withered Sky Saturation | 0.3. 종료 시 하늘 채도 |
| Restore Present Sun With Withering | 켜짐. 정상 태양이 사라지며 오염된 태양의 실제 반사를 복원 |

시작 후 뒤로 돌아가도 연출을 반복하지 않는다. Z로 나레이션을 닫아도 변화는 끝까지 진행한다.
나레이션의 조작 잠금과 타이머를 분리했으며, 게임 일시정지·페이드·재시작 중에는 멈춘다.
`ResetPastReflection()` 또는 씬 재시작으로 초기의 푸른 모습과 도착 이벤트를 복원한다.
일반 사망의 맵 복구는 이 스토리 진행을 되돌리지 않는다.

### 푸른 발판 애셋

내장 image_gen으로 기존 발판의 회색 바위 배열을 보존하고, 윗부분을 초록·청록 풀과
작은 흰꽃으로 바꾼 투명 PNG3종이다. 원본 그림과 캔버스 크기·피벗을 맞췄다.

- Assets/Ember/Art/Storybook/WaterPPT/Platforms/PPT_Stone_Low_Healthy.png
- Assets/Ember/Art/Storybook/WaterPPT/Platforms/PPT_Stone_Mid_Healthy.png
- Assets/Ember/Art/Storybook/WaterPPT/Platforms/PPT_Stone_High_Healthy.png
- 최종 생성 프롬프트는 같은 이름의 .prompt.txt에 있다.

각 Stone_1~5/Art/HealthyReflection은 원본 Art를 따라 움직이는 반사 전용 원본이다.
EmberPastReflection 레이어31에 두어 Main Camera에서는 숨기고 수면 셰이더에만 합성한다.
위쪽 발판과 기존 충돌체·착지 높이는 유지한다. 반사 레이어는 총8개다.
나무·발판7개는 시들고 정상 태양은 오염된 반사로 전환된다. 새 일반 생성 툴이나 씬은 추가하지 않았다.

### 이번 확인

- 실제 Play에서 플레이어를 X79.9→80으로 옮겨 나레이션 시작 및 진행0을 확인했다.
- Game View 캡처: ArtReference/OpeningWater/Captures/ReflectionWither_Healthy.png,
  ReflectionWither_0s.png, ReflectionWither_1s.png, ReflectionWither_2s.png.
- 중간 진행0.5008(약1.002초), 완료1, 과거 반사1, 정상 태양 시듦0, 발판·나무 시듦1을 확인했다.
- 나레이션이 입력을 잠근 상태에서도 중간 진행에 도달하고, 완료 후 재진입해도 반복되지 않음을 확인했다.
- C# 컴파일 오류 없음, 물 셰이더 메시지0개. 지정 위치의 시각 확인이며 발판 전 구간의 입력 완주·빌드·자동 테스트는 수행하지 않았다.
- 추가 사용자 요청으로 반사 하늘의 밝기·채도도 같은 진행에 연결했다. 물 붓질·흰 파도까지 합성한 마지막 색상을 조절하여 밝은 물결이 남지 않게 했다. 하늘을 가장 어둡게 하고 지형 반사는 가독성을 위해 덜 어둡게 하며, 앰버 반사는 보존한다.
- 추가 실제 Game View 캡처: ReflectionSky_0s.png / ReflectionSky_1s.png / ReflectionSky_2s.png. 진행0.5004에서 하늘 어두움0.5004, 종료1에서1, 셰이더 메시지0개를 확인했다. ReflectionSky_Before.png는 직전의 밝은 종료 반사 캡처다.

### 태양 전환과 페이드 이후 위치 고정 — 추가 결정

- 물속 태양은 어두운 정상 태양으로 남는 대신 오염된 태양으로 전환한다.
  PastSunSource가 사라지는 진행에 맞춰 오염 태양 반사를 지우던 마스크를1→0으로 풀어준다.
  별도 태양 색상 어둡게 로직은 추가하지 않았다.
- ScreenExitSceneLoader의 On Continued 마지막 이벤트가 OpeningSuns의
  PinCorruptedSunToCamera를 호출한다. 카메라와 플레이어 이동 후 암전 중에 고정되며,
  그 이후 OpeningSunTransition은 궤도 대신 화면 좌표를 매 프레임 적용한다.
- 위치: OpeningSuns / Opening Sun Transition / Upper Right Viewport = (0.86,0.84).
  왼쪽에서86%, 아래에서84% 위치다. 기존 원형 구간의 태양 궤도는 유지한다.
- 실행 순서는 카메라 → 패럴렉스1000 → 태양1050 → 물1100이다.
  ResetTransition은 화면 고정을 해제하고 원형 시작 상태로 돌아간다.
- 실제 Play에서 이어가기 이벤트를 실행한 뒤 카메라(44.5,0), 크기5 /
  (52.5,0.5), 크기9 / (66.5,-2.3), 크기8.5에서 모두 태양 뷰포트(0.8600,0.8400)를 확인했다.
  암전 전체를 걸어서 재생한 검증은 아니며, 이어가기 콜백과 지정 위치·줌의 시각 확인이다.
- SunTransition_0s.png / SunTransition_1s.png / SunTransition_2s.png 및
  SunPin_Shore.png / SunPin_ContinuationStart.png를 Captures에 저장했다.
  종료 시 정상 태양 전환1, 현재 태양 제거 마스크0, 셰이더 메시지0개를 확인했다.

## 현재 흐름

Storyscene 하나에서 원형 구간 → 기존 페이드 → StoryShoreSection으로 이어진다.

| PPT | 게임 적용 | 캡처 |
| --- | --- | --- |
| 4 | x46~54 내리막에 회색 바위·황금빛 풀 오버레이, 마른 나무·오염 장식, 물가가 드러남 | Slide4_Shore.png |
| 5 | 첫 물 건너기와 Z 안내, 낮은 첫 발판 | Slide5_FirstJump.png |
| 6 | 높이가 다른 돌 발판 5개와 반사, 오른쪽 진행 카메라 | Slide6_Stones.png |
| 7 | 현재의 마른 풍경 아래에 초록 나무·꽃·밝은 태양이 비치는 과거 반사, 상단 나레이션 | Slide7_PastReflection.png |

PPT는 이미지로만 구성되어 있어 4~7번의 원본 삽입 이미지를 손실 없이 추출했다.
원본: ArtReference/OpeningWater/PPT/Slide4_1.png ~ Slide7_1.png.
나란히 비교: ArtReference/OpeningWater/PPT4-7_Comparison.png.

PPT와 똑같은 배치가 아니라 기존 Storyscene 동선에 맞춘 게임용 구성이다.
오염 덩굴은 현재 시각 장식이며 위험/정화 충돌을 추가하지 않았다.
화염꽃 정화 기믹을 새로 연결하거나 능력·입력을 변경하지 않았다.

## 생성 이미지

Assets/Backgrounds/Storybook/WaterSequence/:
- PresentLandscape_v1.png: 마른 나무와 차가운 숲, 현재 배경
- PastLandscape_v2.png: 초록 나무·꽃·밝은 태양, 현재 사용하는 과거 반사
- PastLandscape_v1.png: 초기 생성 원본. v2는 수면 화면 안에 태양이 전부 보이도록 태양 위치만 낮춘 수정본
- SteppingStone_v1.png: 투명 배경 돌 발판, 높이·너비를 다르게 사용
- ShoreBank_v1.png: 투명 배경 물가 경사 지형

모두 built-in image_gen으로 제작했다. 각 PNG 옆 .prompt.txt에 최종 프롬프트와 참조를 보관했다.
PPT 전체 그림을 플레이 화면 위에 붙이지 않고 배경·발판·물가·반사를 각각 분리했다.

기존 07_CorruptionVines.png와 RotatingWorld 나무/바닥 이미지도 재사용한다.
Gamja Flower 글꼴은 Assets/Ember/UI/Fonts/에 OFL과 함께 보관한다.
출처: https://github.com/google/fonts/tree/main/ofl/gamjaflower

## Hierarchy / 조절

StoryShoreSection:
- ShoreBankOverlay: 내리막 그림. 기존 ShoreCollision은 유지
- SteppingStones/Stone_1~5: 중심 X 57 / 61.5 / 66 / 70.5 / 75
  - 각 오브젝트의 Y가 충돌체 윗면이다
  - Art만 늘려 물속까지 바위가 이어지도록 했다
  - 잔풀 끝이 아니라 단단한 바위 윗면을 밟는 설정
- FarShore: x79부터. PolygonCollider2D와 그림, Foundation
- ShoreVines_0~3: 오염 장식만 담당
- JumpCue: 첫 점프 Z 안내. EmberWater 정렬 레이어라 반사에 글자가 들어가지 않음
- PastNarration: 기존 StorybookNarration을 사용. Z 진행/닫기
- River: 물 표현·기존 물 접촉 판정

재사용 프리팹:
- Assets/Ember/Prefabs/Story/SteppingStone.prefab
- Assets/Ember/Prefabs/Story/PastReflectionNarration.prefab

### Main Camera / Story Section Camera
- Max X: 77.5
- Reframe For Water: 켜짐
- Water Framing Start X / End X: 48 / 56
- Water Camera Y: -2
- Smooth Time: 기존 0.3

물가 접근에 따라 카메라를 낮춰 수면을 화면 중간에 둔다.
MountainBackdrop의 기존 ParallaxLayer가 배경 이동을 담당한다. 이때 카메라 스크립트는 배경을 중복 이동시키지 않는다.
기존 패럴렉스 비율과 반복 설정은 유지했다.

### River / Water Surface 2D
- Past Landscape: PastLandscape_v2
- Landscape Reference: MountainBackdrop의 SpriteRenderer
- Memory Reveal Start X / End X: 68 / 78
- Fade Seconds: 2
- Preview Past Blend: 편집 화면에서 0=현재 / 1=과거 반사
- On Past Revealed: PastNarration.BeginNarration

Landscape Reference가 있으면 그 SpriteRenderer의 월드 영역을 기준으로 현재·과거 그림을 정렬한다.
비어 있으면 Past World Origin / Size 수동 설정으로 돌아간다.
ParallaxLayer 이동 이후 실행 순서 1100에서 반사 좌표를 갱신한다.

실행 중 SetPastBlend(float)는 수동으로 반사 비율을 고정한다.
BeginPastReflection은 수동 페이드, ResetPastReflection은 위치 기반 진행을 다시 활성화한다.
반사가 95% 드러나면 나레이션을 한 번 시작한다.
문장: “세상은 조금씩 빛을 잃어갔습니다.”
조작 잠금은 꺼 두어 이동/점프 중 갑자기 멈추지 않는다. Z를 누르면 닫힌다.
뒤로 돌아가면 반사 비율은 위치를 따라 돌아가지만 같은 나레이션은 반복하지 않는다.

## 물 표현

반사/왜곡은 StoryWater2D.shader, 잔빛은 SurfaceRipples.vfx가 담당한다.
설치 버전: VFX Graph 17.6.0 / URP 17.6.0 / Unity 6000.6.5f1.

- 실제 화면 반사: Renderer2D Camera Sorting Layer Texture, Default까지 캡처
- 물: EmberWater 정렬 레이어로 캡처 뒤에 그려 자기 자신을 반복 반사하지 않음
- 밝은 과거: 현재 반사를 별도 과거 그림으로 서서히 전환
- 앰버의 불꽃 반사는 플레이어 주변에서 보존. 오염 태양의 주황색을 잘못 보존하지 않도록 위치 제한
- 가로 왜곡, 얇은 수면선, 끊어진 물빛 선, 깊이에 따른 청록색 변화
- SCPE CausticsRGB 텍스처를 낮은 강도로 두 방향 스크롤
- VFX: 초당 22개, 수명 1.6초, 작고 가로로 긴 잔빛이 나타났다 사라짐
- VFX 위치 범위: 중심 69 부근, 가로 31 / 수면 아래 2.2 유닛
- VFX Bounds: 34 × 5 × 2

상세 재사용 패키지 조사: WaterPackageAssets_Audit.md.
SCPE의 호환되지 않는 스크립트/렌더 패스는 가져오지 않았다.
물빛 텍스처만 기존 제공 패키지에서 복사해 사용했다.

물 Material: Assets/Ember/Water/StoryWater.mat
- Reflection Strength: 현재 반사 강도
- Horizontal Distortion: 일렁임 크기
- Wave Speed / Frequency: 물결 속도와 촘촘함
- Shallow / Deep Water: 물색
- Water Light Strength: 재사용한 물빛 텍스처 강도

현재 화면 반사는 화면 밖 물체를 새로 그려주는 3D 반사가 아니다.
과거 반사는 별도 일러스트이며 완전한 과거 월드/플레이 공간이 아니다.
꽃 기믹과 새로운 물리 시뮬레이션은 이 작업에 포함하지 않았다.

## 확인 기록과 한계

- 4~7번에 해당하는 에디터 카메라 렌더를 반복 캡처해 비교했다.
- 발판 그림과 땅 높이의 차이, 건너편 그림 아래 빈틈, 반사 좌표 지연, 반사된 Z 안내를 수정했다.
- Play 모드에서 실제 Fire Mode 애니메이션, 움직이는 수면, VFX 입자 35~36개 유지 및 컬링 해제 상태를 확인했다.
- 플레이어를 건너편 위치로 옮기자 과거 반사가 자동으로 나타나고 나레이션 IsPlaying=true가 되는 것을 확인했다.
- 런타임 캡처는 플레이어를 지정 위치로 옮겨 정지한 시각 확인이다. 처음부터 걸어서 발판을 전부 통과한 난이도/입력 검증은 아니다.
- 빌드·자동 테스트는 실행하지 않았다.
- 확인 후 Play 모드를 종료했고, 원형 오프닝 시작 상태로 저장했다.

## 2026-10-19 반복 배경 경계 수정
패럴렉스 배경은 가로로 반복되지만 과거 반사는 한 장의 끝에서 현재 물색으로 빠져 오른쪽에 짙은 사각 경계가 드러났다. WaterSurface2D가 연결된 ParallaxLayer.RepeatsHorizontally를 읽어 셰이더의 _PastRepeatX를 전달하도록 수정했다. 가로 반복은 같은 타일 주기의 frac 좌표를 사용하며, 좌우 끝 5.5%는 서로 부드럽게 섞는다. 원본 배경이 한 타일 이동해도 반사가 좌우 반전되지 않는다. 가로 반복이 꺼졌거나 ParallaxLayer가 없으면 기존 한 장 범위를 사용한다. 세로 범위와 연출 진행은 유지했다.
- 같은 카메라/배경 위치에서 이전 경계 처리와 새 처리로 렌더 캡처를 비교했다.
- 비교: ArtReference/OpeningWater/Captures/ReflectionEdge_Before.png 및 ReflectionEdge_After.png.
- 셰이더 메시지 0개 및 반복 플래그 자동 연결을 확인했다. 자동 테스트는 실행하지 않았다.

## 2026-10-19 MountainBackdrop 설정
- Parallax Layer를 켜고 Camera Transform에 Main Camera를 연결한다. Follow X는 0.95, Follow Y는 0, Repeat X는 켠다.
- 컴포넌트를 끄면 가로 복제도 사라져 한 장의 끝에서 파란 카메라 배경색이 드러난다. 높이만 고정하려면 컴포넌트를 켠 채 Follow Y를 0으로 둔다.
- StorySectionCamera는 ParallaxLayer가 있는 배경을 직접 이동시키지 않는다. 비활성 상태도 존중한다.
- 실제 Game View의 수정 전후 캡처로 오른쪽 빈 공간과 반사 복구를 확인했다: ArtReference/OpeningWater/Captures/MountainBackdrop_Before.png 및 MountainBackdrop_After.png.

## 2026-10-19 PPT 원본을 기준으로 분리 재구성

사용자 요청으로 기존 물가 구간의 외형과 카메라를 다시 맞췄다. PPT4~7은 각각
1672×941 PNG 한 장으로 평탄화되어 있다. 원본 PNG는 ArtReference/OpeningWater/PPT에
손실 없이 추출되어 있으며, 아래 그림은 내장 image_gen으로 원본을 참조해 분리·복원했다.
원본 편집 파일의 숨겨진 레이어를 복구한 자료는 아니다.

### 애셋과 프리팹

- 이미지: Assets/Ember/Art/Storybook/WaterPPT/
- Backgrounds: 산, 숲, 물 붓질, 긴 비탈, 깊이가 있는 비탈, 오염 태양, 정상 하늘.
- Platforms: 낮음·중간·높음 돌 3종. 씬에는 낮음→중간→높음→낮음→중간 순서로 5개 배치.
- Scenery: 좌우 강둑, 마른 나무, 오염 덩굴, 건강한 나무·흰 꽃, 정상 태양, 수중 바위·수초.
- 프롬프트: 각 이미지 옆 prompt.txt 및 Backgrounds/IMAGEGEN_PROMPTS.txt.
- 프리팹: Assets/Ember/Prefabs/Story/WaterPPT/의 9종. 돌은 기존 충돌체를 포함하며,
  이름에 Visual이 붙은 강둑·오염은 외형용이다. 마른 나무와 수중 바위도 장식이다.
- 비탈 프리팹도 그림용이다. 현재 씬의 비탈 충돌은 별도 ShoreCollision이 담당한다.
- 미채택 생성본은 ArtReference/OpeningWater/GeneratedVariants에 보관했다.

### 현재 씬 설정

- 카메라는 물가 X48~58에서 Y0.5·Orthographic Size9로 전환한다.
- X74~80에서는 X66.5·Y-2.3·크기8.5로 물길을 넓게 보여준다.
- 원형 구간의 최초 카메라 크기는5다. 위 설정은 StorySectionCamera의 물가 진행 중에만 적용한다.
- 물가 전환의 On Continued 마지막 이벤트는 기존 Square 종이 바탕을 끈다. 새 PaperSky와 겹쳐
  이전 종이 그림의 사각 경계가 남는 것을 방지한다. 원형 시작 시 Square는 켜져 있다.
- 산과 숲은 분리된 패럴렉스이며 가로 추적0.08/0.15, 세로 추적0이다. 나무가 포함된
  전체 풍경 한 장을 확대하는 이전 MountainBackdrop 구성은 교체했다.
- 화면에 보였던 검정 직사각형 CharcoalFoundation은 제거했다. 현재 CharcoalDescent는
  아래까지 재구성한 PPT_Descent_Deep.png 한 장이며 고유한 투명 외곽으로 물가를 가린다.
  바닥 그림을 세로로 늘이거나 별도 사각 바탕을 붙이는 방식은 사용하지 않는다.
- 비탈은 EmberWater 정렬 레이어 Order1로 물보다 앞에 그려 자연스러운 물가 외곽을 유지한다.
  수중 바위는 같은 레이어 Order2, 반투명 장식이며 충돌체가 없다.
- ShoreCollision과 FarShore의 지면 경계를 새 바위/비탈 외곽에 맞췄다. 돌5개의 위치·착지 높이는 유지했다.
- Z 안내는 StoryJumpCue로 플레이어를 따라가며 X52~76.5에서 표시한다.
- 과거 하늘은 밝은 종이 부분만 대체한다. 산·돌과 플레이어의 현재 반사는 유지한다.
- 건강한 나무2개와 정상 태양은 EmberPastReflection 레이어31의 숨김 원본을 수면에만 합성한다.
  Main Camera는 이 레이어를 제외한다. 정상 태양은 오염 태양의 자식으로 위치를 함께 움직인다.
- 오염 태양의 반사는 지정 Renderer 범위를 과거 하늘로 지우고 정상 태양으로 바꾼다.
- 나레이션은 기존 문장 ‘세상은 조금씩 빛을 잃어갔습니다.’와 Z닫기를 유지한다.

### 확인한 범위

- 에디터 카메라 렌더를 여러 차례 비교해 강둑 반전, 지나치게 큰 돌, 수면 높이,
  비탈 아래의 직사각 바탕과 지면 연결부를 수정했다.
- 실제 Play에서 지정 위치에 플레이어를 고정하고 카메라 크기·위치와 반사 진행을 확인했다.
- Game View 캡처: ArtReference/OpeningWater/Captures/PPT6_Runtime.png, PPT7_Runtime.png.
- 검정박스 수정 최종 재생 캡처: PPT4_Runtime.png. CharcoalFoundation과 임시 보정용 메시·재질은 제거했다.
- 플레이어를 x66에 고정했을 때 카메라 x67.5/Y0.5/크기9, x80으로 옮겼을 때 x66.5/Y-2.3/크기8.5,
  과거반사1 및 나레이션 재생 상태가 되는 것을 확인했다. 셰이더 메시지와 C# 컴파일 오류는0이다.
- PPT4~7 외형 미리보기: 같은 폴더 PPT4_Rebuild.png~PPT7_Rebuild.png.
- 실제 재생 비교: [PPT4 원본](../ArtReference/OpeningWater/PPT/Slide4_1.png) /
  [현재 물가](../ArtReference/OpeningWater/Captures/PPT4_Runtime.png),
  [PPT6 원본](../ArtReference/OpeningWater/PPT/Slide6_1.png) /
  [현재 돌다리](../ArtReference/OpeningWater/Captures/PPT6_Runtime.png),
  [PPT7 원본](../ArtReference/OpeningWater/PPT/Slide7_1.png) /
  [현재 과거 반사](../ArtReference/OpeningWater/Captures/PPT7_Runtime.png).
- 처음부터 모든 발판을 직접 건너는 입력/난이도 확인과 빌드는 수행하지 않았다.
- PPT7의 돌이 없는 중앙 물길 구도와 현재 플레이 지형은 차이가 있다. 기존 착지 발판을
  임의로 숨기지 않고 물가의 색·외형·반사 대비를 우선 맞췄다.
- PPT4~7에는 화염꽃 발사가 직접 그려져 있지 않다. 이번에 새 꽃 발사 퍼즐을 추가한 것으로 보지 않는다.

## Storyscene 나무 Hierarchy (2026-10-20)

`Storyscene/RiverMemory/Trees` 아래에 `Tree_Left`, `Tree_Right`를 두었다. 각 그룹은 `Present_Dry`(현재의 마른 나무), `Past_Reality_Healthy`(현실에 나타나는 푸른 과거 나무), `Past_Reflection_Source`(물 반사 원본)를 포함한다. `Tree_Left` 또는 `Tree_Right`를 옮기면 세 시각 변형이 함께 이동한다. 과거 연출 중 `Past_Reality_Healthy`는 시퀀스가 반사 원본 위치에 맞추므로, 푸른 과거 나무를 이동할 때는 `Past_Reflection_Source`를 조정한다. 기존 RiverMemorySequence 참조와 화면상 월드 위치는 유지했다.
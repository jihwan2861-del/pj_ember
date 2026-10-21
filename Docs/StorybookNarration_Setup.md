# 상단 동화 나레이션

프리팹: `Assets/Ember/Prefabs/Story/PastReflectionNarration.prefab`

Storyscene의 기존 물가 나레이션: `StoryShoreSection/PastNarration`

## NPC 대화 모드

`StoryNarrationCue`의 `Presentation`을 `Dialogue`로 바꾸면 같은 출력·Z 입력 잠금을 사용하는 NPC 대화창이 열린다. 화면 아래쪽의 짙은 대화 패널에 화자 이름, 초상화, 글자 출력, `Z 계속`/`Z 닫기` 안내를 표시한다. 대화는 자동으로 넘어가지 않는다. 타이핑 중 Z는 현재 문장을 완성하고, 완성 뒤 Z는 다음 줄로 이동한다.

| 항목 | 역할 |
| --- | --- |
| `Speaker Name` / `Speaker Portrait` | 줄별 설정이 비었을 때 사용할 기본 화자와 초상화 |
| `Line Speakers` | `Lines`와 같은 순서로 화자를 지정. 빈 칸은 기본 화자를 사용 |
| `Line Portraits` | `Lines`와 같은 순서로 초상화를 지정. 명시적 빈 항목은 해당 줄 초상화를 숨김 |
| `Lock Player Controls` | 대화 중 이동 잠금. 현재 Guide_NPC 대화는 켜짐 |

대화창은 기존 `StorybookNarration`의 Screen Space Canvas를 사용해 실행 중 생성된다. 기존 상단 나레이션 UI는 그대로 유지되며, 기본 `Presentation`은 `Narration`이다. `Guide_NPC`는 현재 영역 진입 트리거로 시작하고, 1행은 앰버, 2·3행은 안내자 초상화와 이름을 표시한다. 다른 NPC는 각자의 `StoryNarrationCue`에서 Presentation과 화자 데이터를 지정하고 이벤트의 `BeginNarration()`에 연결한다.

Unity Game View에서 실제 배치와 글자 크기·초상화 구도를 확인하는 단계는 아직 남아 있다. 코드를 씬에 연결한 상태와 런타임 화면 검증을 구분한다.

## 문장과 재생 설정

`Storybook Narration` 컴포넌트에서 나레이션마다 설정한다. 문장은 이미지가 아닌 편집 가능한 uGUI Text이며, `Lines`를 바꾸면 대사를 교체할 수 있다.

| 항목 | 기본 설정과 역할 |
| --- | --- |
| `Lines` | 표시할 문장을 순서대로 입력한다. |
| `Typewriter` | 켜짐. 문장을 글자 순서대로 표시한다. |
| `Characters Per Second` | 24. 생성된 글자 쿼드 기준 초당 출력 수다. |
| `Auto Advance` | 켜짐. 출력 완료 후 다음 문장으로 넘어가고 마지막 문장은 닫는다. |
| `Line Hold Seconds` | 2. 글자가 전부 나온 뒤 기다리는 실제 초이며 게임 배속에 영향받지 않는다. |
| `Lock Player Controls` | 새 컴포넌트의 기본값은 꺼짐. 켜진 나레이션만 플레이어를 멈춘다. |
| `Player` | 비우면 같은 씬의 활성 플레이어를 찾는다. 여러 플레이어가 있으면 직접 연결한다. |

**모든 나레이션을 잠그지 않는다.** 기존 인스턴스의 개별 잠금 설정을 유지하며, 물가의 `PastNarration`은 잠금이 꺼져 있다. 잠금을 켰다면 타이핑 중과 출력 완료 후 대기 동안 모두 유지하고, 마지막 문장이 자동으로 닫힐 때 해제한다.

## 배치와 연결

1. 프리팹을 원하는 씬에 드래그한다. 자체 Canvas가 있으므로 기존 Canvas 아래에 넣지 않아도 된다.
2. 루트 `Storybook Narration` 컴포넌트의 `Lines`에 문장을 순서대로 입력하고, 해당 나레이션의 잠금 여부와 출력·대기 시간을 설정한다.
3. 이벤트 지점의 `Player Event Trigger > On Player Entered`에 씬에 배치한 나레이션 오브젝트를 넣는다.
4. 함수로 `StorybookNarration > BeginNarration()`을 고른다.

프리팹 자체를 이벤트 대상으로 넣지 말고 **씬의 인스턴스**를 연결한다. 트리거와 플레이어의 Collider2D가 겹치고 플레이어의 Rigidbody2D가 활성화되어 있어야 한다.

## 동작

- 처음에는 숨김. 시작하면 상단에 첫 문장을 타이핑한다. 출력 속도와 자동 대기는 실제 시간을 사용한다.
- 타이핑 중 Z를 새로 누르면 현재 문장을 한 번에 완성한다. 완성된 상태에서 다시 Z를 누르면 다음 문장으로 진행하며, 마지막 문장에서는 닫힌다.
- 시작한 프레임에는 다음 문장으로 넘어가지 않는다. 나레이션에 사용한 Z는 점프 등 게임 조작에 전달하지 않는다.
- `Auto Advance`가 켜져 있으면 Z 안내를 숨긴다. 잠금이 꺼진 나레이션의 자동 시작·자동 진행은 게임 입력을 소비하지 않아 이동하면서 읽을 수 있다.
- Esc 일시정지, 재시작 전환, 씬 페이드 중에는 타이핑과 자동 대기가 진행되지 않는다.
- 한 번에 하나의 나레이션만 재생한다. 다른 나레이션이 재생 중인 요청은 경고 후 무시한다. 자동 대기열은 없다.
- `On Started` / `On Finished`: 시작/완료 후 연결할 이벤트. 자동 종료도 `On Finished`를 호출하므로 기존 후속 튜토리얼 연결이 유지된다.
- `CancelNarration()`이나 오브젝트 비활성화는 완료 이벤트 없이 닫고 타이핑·대기 상태와 이 나레이션이 얻은 플레이어 잠금을 정리한다. 시작 전에 다른 시스템이 이미 잠근 플레이어의 잠금은 해제하지 않는다.
- 기존 `Player Event Trigger`의 `Trigger Once`가 켜져 있으면 사망 후에도 다시 시작하지 않는다. 씬을 다시 불러오면 초기화된다.

## 외형 수정

- `TopPanel`: 위치·높이·배경색. CanvasGroup으로 숨기므로 루트와 패널 오브젝트는 활성 상태로 둔다.
- `NarrationText`: 글꼴·크기·색상·정렬.
- 글꼴: `Assets/Ember/UI/Fonts/GamjaFlower-Regular.ttf`. 기존 동화풍 화면에 맞는 GamjaFlower를 사용하며 대사는 `Lines`로 계속 편집한다.
- `ContinueHint` 또는 `AdvanceHint`: 수동 진행 때 사용할 Z 안내 문구. 자동 진행 중에는 숨긴다.
- `NarrationTextReveal`은 전체 문장 배치를 유지한 채 아직 나오지 않은 글자의 메시 알파만 숨긴다. 타이핑할 때 중앙 정렬 위치나 줄바꿈이 글자마다 바뀌지 않는다. 현재 나레이션 Text에는 Shadow/Outline 효과가 없다.

## 확인한 범위 — 2026-10-19

- 잠금 켜짐: 실제 오른쪽 입력을 유지해도 X 이동량이 0이었고, 글자가 완성된 뒤 대기하는 동안에도 잠금이 유지됐다. 연속 재생에서 대기 약 2.002초 후 자동 종료와 잠금 해제를 확인했다.
- 잠금 꺼짐: 실제 오른쪽 입력을 유지했을 때 타이핑 중 약 2.021U 이동했다.
- 실제 나레이션 Text의 GamjaFlower 적용과 Game View 캡처를 확인했다.
- 두 문장이 자동으로 이어지고 마지막 완료 이벤트가1회 실행됐다. 타이핑 도중1실제초 일시정지했을 때 출력 진행은 그대로 유지됐다. 취소·비활성화도 잠금을 해제하고 완료 이벤트를 추가 호출하지 않았다.

캡처는 `ArtReference/OpeningWater/Narration/Captures`의 `01_TypingLocked.png`, `02_FullHold.png`, `03_UnlockedMovement.png`다. 확인용 입력 장치와 런타임 설정을 복원하고 Edit 모드에서 씬을 저장했다.

이번 작업은 기존 나레이션의 글꼴·출력·자동 진행·선택 잠금 기능 수정이다. 대사 작성이나 새 씬 생성은 포함하지 않는다.

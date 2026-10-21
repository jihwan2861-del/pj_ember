# 오프닝 진행 제어

`Assets/EmberPrototype/Runtime/EmberOpeningSequence.cs`

빈 오브젝트에 `Ember / Opening Sequence`를 추가한다. `Begin On Start`가 켜져 있으면 플레이 시작 시 1단계로 들어간다. 끄면 이벤트에서 `BeginSequence()`를 호출한다.

각 단계의 `On Enter`에 해당 단계 시작 연출, `On Exit`에 안내 숨기기 등 정리 작업을 연결한다. 해당 단계가 완료된 곳에서는 아래 함수를 호출한다.

| 단계 | 완료 함수 | 연결할 신호 예시 |
| --- | --- | --- |
| 1 따뜻했던 태양 | CompleteWarmSun | 이동 지점 도달 |
| 2 오염된 태양 | CompletePollutedSun | 꽃 안내 지점 도달 |
| 3 꽃 정화 | CompleteFlowerPurification | PollutionObstacle의 On Purified |
| 4 평지 전환 | CompleteRoundToFlat | 첫 물가 앞 도달 |
| 5 첫 물 건너기 | CompleteFirstWaterJump | 첫 발판 착지 |
| 6 연속 점프 | CompleteConsecutiveJumps | 마지막 안전 육지 착지 확인 |
| 7 과거 반사 | CompletePastReflection | 과거 풍경·서술 연출 완료 |
| 8 앰버 탄생 | CompleteEmberBirth | 변신·문장·감상 시간 완료 |
| 9 산 공개 | CompleteMountainReveal | 카메라 게임 구도 복귀 완료 |

예: `Player Event Trigger > On Player Entered`에 씬의 진행 관리자 오브젝트를 넣고 `EmberOpeningSequence.CompleteWarmSun()`을 선택한다. 같은 신호를 다시 받거나 다른 단계의 완료 함수가 호출되면 무시한다.

## 범위와 주의

- 진행 순서만 관리한다. 카메라 이동, 타이머, 나레이션 내용 변경, 조작 잠금, 안전 착지 판정, 반사·변신 효과는 아직 구현하거나 자동 연결하지 않는다.
- 6단계 완료에 일반 접촉 트리거만 연결하면 공중에서도 진행할 수 있다. 이후 안전 착지 확인 기능에서 CompleteConsecutiveJumps를 호출해야 한다.
- 현재 StorybookNarration은 Z 진행 방식이다. 이동·점프 중 자동으로 유지되는 서술은 다음 작업에서 확장한다. 하나의 나레이션 완료 이벤트에 여러 단계 완료 함수를 나열하지 않는다.
- Enter/Exit 이벤트 안에서 진행 함수를 다시 호출하는 재귀 전환은 무시한다. 실제 도달·연출 완료 신호에서 호출한다.
- 전체 완료 시 On Completed를 호출한다. 8→9는 같은 씬에서 카메라 연출로 연결하며 이 스크립트는 씬 이동이나 플레이어 순간이동을 하지 않는다.
- ResetSequence는 현재 Exit를 호출하고 미시작 상태로 돌아간다. 다시 시작하려면 BeginSequence를 호출한다. 오염·플레이어·카메라·트리거 등은 자동 초기화하지 않는다.
- 사망으로 오프닝 전체를 다시 시작하지 않는다. 씬 재로드 시에는 처음부터 시작한다. 체크포인트별 연출 복구 정책은 이후 연결 작업이다.
- 진행 관리자 오브젝트는 계속 활성 상태로 유지한다. 비활성화는 연출 취소·조작 복귀 용도로 사용하지 않는다.

씬 생성·배치나 플레이 모드 테스트는 수행하지 않았다.

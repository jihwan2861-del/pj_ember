# 스토리 플레이어 외형 전환

Storyscene의 player에 StoryPlayerAppearance를 연결했다.

- 시작 외형: Assets/player/Fire Mode.anim의 불꽃 8프레임 루프.
- 1~7단계: 불꽃 모습 유지. 이동·점프 때도 같은 불꽃 루프를 사용한다.
- 8단계 EmberBirth 진입: 기존 플레이어 Animator Controller로 복귀한다.
- 9단계 이후: 기존 Idle/Run/Jump 애니메이션을 사용한다.
- 플레이어 오브젝트·위치·충돌체·능력은 교체하지 않는다. 프리팹 원본의 Animator도 수정하지 않는다.
- ShowFire / ShowEmber를 UnityEvent에 연결하면 변신 효과의 특정 시점에 외형을 수동 전환할 수도 있다.
- 연결된 Opening Sequence의 단계가 바뀔 때만 자동 전환한다. 씬을 다시 불러오면 불꽃으로 시작한다.
- Storyscene에는 진행 관리자가 없어 Circle에 EmberOpeningSequence를 추가했다. 단계 진행은 기존 계획대로 도달/정화/연출 완료 이벤트로 연결해야 한다. 이 작업에서 임의의 이동 거리나 타이머로 1~9단계를 자동 진행시키지는 않았다.
- 탄생의 불씨·빛 효과 및 카메라 연출은 포함하지 않는다.

플레이 모드 전환 테스트는 이번 작업에서 수행하지 않았다.

2026-10-18 방향 수정: 불꽃은 런타임 StoryFireVisual 자식에서 별도로 재생하며 Fire Rotation Degrees 기본값 +90도로 둥근 부분이 아래, 꼬리가 위를 향한다. 플레이어 루트·충돌체·앰버 애니메이션은 회전하지 않는다. 불꽃 표시 동안 기존 SpriteRenderer의 화면 출력만 숨긴다.

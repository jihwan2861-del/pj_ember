# 치킨 수집 아이템

`Assets/EmberPrototype/Prefabs/ChickenPickup.prefab`을 씬에 배치한다. 생성 메뉴는 `Ember Prototype > Create Chicken Pickup Prefab`이다.

플레이어가 닿으면 치킨가 따라오며, 여러 개는 줄지어 따라온다. 공중에서는 확정되지 않고, 일반 이동 상태로 안전한 바닥에 0.3초 동안 서 있으면 수집이 확정된다. 점프·불 속 이동·사망·조작 잠금 중에는 확정되지 않는다. 발판은 안전한 바닥으로 인정하되 RoomHazard 또는 WaterContact가 붙은 바닥은 제외한다.

확정 전 사망하거나 리스폰·맵 리셋하면 원래 자리로 돌아온다. 확정 후에는 사망해도 유지된다. 이 기록은 현재 플레이 세션과 플레이어 오브젝트의 수명 동안만 유지되며, 씬 재로드·게임 재실행 시에는 초기화된다.

- **Confirm Delay**: 바닥에 서 있어야 하는 시간. 기본 0.3초.
- **Follow Speed**: 따라오는 반응 속도. 기본 8.
- **Follow Offset**: 플레이어 뒤쪽 거리와 높이. 기본 (-0.65, 0.6).
- **Bob Height**: Visual 자식의 위아래 떠 있는 움직임. 기본 0.08.
- **On Collected**: 수집 확정 이벤트. 효과음·이펙트 등을 Inspector에서 연결할 수 있다.

플레이어의 StrawberryCollection 컴포넌트가 없으면 최초 획득 시 자동 추가한다. HUD 등에서는 `CollectedCount`, `PendingCount`, `OnCountChanged`를 사용한다. Inspector에서 이벤트를 미리 연결하려면 StrawberryCollection을 플레이어에 추가한다. 확정된 아이템은 삭제하지 않고 그림과 충돌만 끈다.

Visual 자식의 SpriteRenderer를 원하는 아이템 그림으로 교체하면 된다. 기본은 픽셀 치킨 아이콘이다. 기존 플레이어 프리팹과 게임 씬에는 자동 배치하지 않는다.

기존 배치 참조를 유지하기 위해 프리팹 GUID와 런타임 StrawberryPickup/StrawberryCollection 타입은 유지한다. 표시 이름과 기본 그림은 치킨이다.

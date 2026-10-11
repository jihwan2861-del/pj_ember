# 물의 정령 프리팹

Move 애니메이션 추적·돌진 AI 연결과 외부 모드 전환은 `Docs/WaterSpirit_AI_Setup.md`를 따른다. 아래의 자동·수동 공격 설명은 AI를 사용하지 않는 경우에 해당한다.

`Assets/EmberPrototype/Prefabs/WaterSpirit.prefab`을 씬에 드래그한다. 생성 메뉴는 `Ember Prototype > Create Water Spirit Prefab`이며, 이미 만든 프리팹과 애니메이션을 덮어쓰지 않는다.

- Idle: 원본 시트 0~4, 반복.
- Move: 5~8, 반복.
- Attack: 9~12, 한 번 재생. 기본 재생 속도는 초당 8프레임.
- Auto Attack: 기본 활성화. 대기 후 파도를 발사한다.
- Direction: 오른쪽/왼쪽 발사 방향. 정령 원본은 왼쪽, 파도 원본은 오른쪽을 바라보므로 각각 방향에 맞게 반전된다.
- Attack Interval: 공격 종료 후 다음 공격까지의 대기 시간. 이전 파도가 이동 중이면 끝날 때까지 기다린다.
- Wave Release Time: 공격 시작 후 파도를 내보내는 시간. 기본 0.25초로 세 번째 공격 프레임에 맞춘다.
- Attack Duration: 기본 0.5초. 애니메이션 속도를 편집했다면 발사 시간과 함께 조절한다.
- Wave Spawn: 파도의 바닥 생성 위치. 기본은 정령 중심에서 진행 방향으로 0.8유닛, 아래로 0.5유닛. 정령 위치와 지형에 맞춰 조절한다.

Wave Prefab에는 기존 `WaterWave.prefab`이 연결돼 있다. 정령이 발사하는 파도는 자동 반복을 끄고 한 번만 이동한다. 정령마다 파도 한 개를 재사용하며, 위험 접촉은 기존 RoomHazard의 플레이어 사망 처리를 따른다. 기존 파도 프리팹의 높이·속도·이동 거리·Deadly 설정을 사용한다.

이동을 연결하려면 `WaterSpirit.SetMoveVelocity(Vector2)`를 호출한다. 이동 중 Move를 재생하고, 정지하면 Idle로 돌아온다. 수동 공격은 Auto Attack을 끄고 `Attack()`을 호출한다. 공격 중 이동은 멈추고 발사 방향은 공격 시작 시 고정한다. 정령 자체에는 추적 AI나 접촉 피해가 없다.

기존 MapReset 경로에서 정령 위치·공격·파도를 초기화한다. 사망 연출 중에는 공격을 보류하고 리스폰 완료 후 다시 시작한다. 프리팹을 비활성화해도 공격과 파도가 중단된다.



# 무게로 내려가는 플랫폼

`Assets/EmberPrototype/Prefabs/WeightPlatform.prefab`을 씬에 드래그한다. 기존 프리팹을 직접 배치하며 이전 생성 메뉴는 정리했다.

플레이어나 Collider2D가 있는 물체가 플랫폼 위에 있으면 천천히 내려가고, 아무것도 없으면 원래 높이로 돌아온다. 여러 물체가 있으면 마지막 물체가 떠날 때 복귀한다. 물체 수와 질량에 관계없이 설정한 속도를 사용한다.

- **Descend Distance**: 최대로 내려가는 거리. 기본 2유닛.
- **Descend Speed**: 내려가는 속도. 기본 초당 0.5유닛.
- **Return Speed**: 복귀 속도. 기본 초당 0.5유닛. 0으로 설정하면 내려간 위치에 머문다.
- **Surface Tolerance**: 위쪽 접촉을 감지하는 여유 거리. 기본 0.1유닛.
- **Load Mask**: 하중으로 취급할 물체의 레이어. 기본 모든 레이어.

원하는 그림을 사용하려면 프리팹의 PrototypeSprite와 NonFlammableTile 컴포넌트를 제거한 뒤 SpriteRenderer의 Sprite를 교체하고 BoxCollider2D 크기를 발판의 윗면에 맞춘다. 플랫폼 Collider는 트리거로 만들지 않는다. 위쪽 물체의 Collider도 트리거가 아니어야 하며, 옆이나 아래만 닿는 물체는 플랫폼을 누르지 않는다. Rigidbody2D는 Kinematic으로 동작하므로 중력으로 직접 떨어지는 방식이 아니다.

기존 오브젝트에 기능을 붙이려면 BoxCollider2D와 WeightPlatform을 추가한다. 같은 오브젝트에 PathMovingBlock을 함께 붙이지 않는다. 플랫폼의 위치와 회전은 고정된 수평 발판을 기준으로 설정한다.

플레이어는 발판의 이동 속도를 함께 적용해 내려가는 동안 탑승 상태를 유지한다. MapReset 시 처음 배치한 높이로 돌아온다. 본편 씬에는 자동으로 배치하지 않는다.

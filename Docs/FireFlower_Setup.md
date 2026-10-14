# 불꽃꽃 — 2026-10-14

불꽃꽃은 반응 범위 안에서 성공한 C 점화 폭발에 반응한다. 반응 순간의 플레이어 위치와 발사 방향을 저장한다. 점선으로 그 위치를 예고한 뒤 불꽃을 발사하며 플레이어가 이동해도 재조준하지 않는다. 불꽃은 저장된 위치를 지나 사거리까지 진행한다.

## 배치

- 꽃 오브젝트에 FireFlower를 추가한다. Collider2D는 없어도 된다.
- Reaction Radius는 C 폭발 반경과 독립된 반응 거리다. 꽃 위치와 플레이어 중심 간 거리로 판정한다.
- Warning Time으로 발사 예고 시간, Projectile Speed/Range/Radius로 속도·사거리·충돌 크기를 조절한다.
- Muzzle을 지정하면 자식 발사 지점에서 쏜다. 비어 있으면 꽃 위치를 사용한다.
- Wilted Sprite/Blooming Sprite에 픽셀 애셋을 연결한다. 비어 있으면 임시 16×16 픽셀 꽃을 표시한다. 지형·장애물의 8×8 타일 기준과 꽃 전체 이미지 크기는 별개다.
- 기본 One Shot은 활성화 후 한 번만 발사한다. 재사용하려면 끄고 Cooldown을 설정한다. 발사 준비 중 연속 C는 기존 조준을 바꾸지 않는다.

## 오염

정화할 오염 전용 루트에 PollutionObstacle을 추가하고, 해당 루트 또는 자식에 Collider2D를 배치한다. 위험한 오염이면 RoomHazard도 추가한다. 꽃 불꽃이 닿으면 오염의 렌더러와 충돌을 끄고 길을 연다.

Accepts Flower Fire가 켜진 오염만 꽃 불꽃으로 정화된다. 일반 C 폭발이나 기존 EmberProjectile은 이 조건을 충족하지 않는다. 다른 불 능력으로 모든 오염을 지우는 규칙은 추가하지 않았다.

Purified Visual에 정화 후 식물·금빛 장식을 연결할 수 있다. On Purified 이벤트는 문 열기 같은 추가 기믹 연결에 사용한다. 꽃 불꽃은 일반 벽에서 멈추고 플레이어에게 닿으면 사망/재시작을 요청한다.

## 테스트 씬

Assets/EmberPrototype/FireFlowerValidation.unity를 열어 Play한다. 중앙 발판에서 C → 점선 예고 → Z 또는 방향키로 비켜나기 → 오른쪽 오염 정화를 확인한다. R은 씬을 다시 시작한다.

Tools > Ember > Create Fire Flower Test Scene으로 테스트 씬을 다시 만들 수 있다. 이 메뉴는 현재 씬의 저장 여부를 먼저 확인한다. 기존 제작 씬은 자동으로 수정하지 않는다.

사망/맵 초기화 시 발사 준비, 꽃 활성 상태, 날아가는 불꽃, 정화된 오염은 초기 상태로 돌아간다. 체크포인트·스토리 진행과 별개의 임시 기믹이다.

## 검증

Unity 6000.5.2f1의 격리 프로젝트에서 관련 검사 6개가 통과했다. 실제 Play Mode의 C 입력 → 위치 고정 → 플레이어를 발사 경로 밖으로 이동 → 오염 정화를 확인했다. 추가로 고정 방향 발사, 범위 밖/중복 반응 차단, 벽 충돌, C 직접 정화 방지, 맵 초기화를 검사했다. 결과는 ValidationArtifacts/FireFlower/UnityResults.xml과 source-snapshot.json에 있다.

꽃·발사체·테스트 지형은 동작 검증용 임시 표현이다. 시각 참고 이미지는 ArtReference/FireFlower_FixedAim_Reference.png에 보관했다. 최종 8×8 타일 아트와 오프닝 연출은 이번 기믹 구현에 포함되지 않는다.

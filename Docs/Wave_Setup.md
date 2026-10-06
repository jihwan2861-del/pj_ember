# 파도 하나 배치하기

`Assets/EmberPrototype/Prefabs/WaterWave.prefab`을 씬에 드래그한다. 프리팹이 없다면 `Ember Prototype > Create Water Wave Prefab` 메뉴로 생성한다. 위치는 **파도의 바닥 가운데**이고, 크기는 루트 Scale 대신 Height와 Width로 조절한다. 기본 높이는 기존 플레이어 이단 점프 실측에 가까운 2.65유닛이다.

- Direction: 좌→우 또는 우→좌.
- Height / Width: 파도 높이와 폭.
- Speed: 초당 이동 속도.
- Travel Distance: 시작 위치에서 이동할 거리.
- Repeat / Repeat Delay: 끝에 도달한 후 시작점에서 다시 나올지와 간격.
- Deadly: 켜면 기존 위험물처럼 플레이어가 닿을 때 리스폰한다. 움직임만 볼 때는 끈다.
- Play On Enable: 실행 시 자동으로 출발한다. 꺼두면 나중에 Play() 또는 PlayAt(시작점)으로 출발시킬 수 있다.

끝에 도달하면 파도 표현과 위험 판정을 끈다. Stop()은 즉시 정지하며 반복 예약도 취소한다. 파도는 지형을 통과하므로 높은 발판 위에서 피하도록 배치한다. 물결 모양의 Polygon Collider가 시각 표현을 따른다. 현재 접촉 처리는 기존 RoomHazard의 트리거 방식이며, 충돌체가 꺼지는 능력에 대한 별도 판정은 추가하지 않았다.

이 단계에는 보스, 공격 예고, 상승하는 물, 방 진행 기능이 없다. 기존 씬과 플레이어 코드는 수정하지 않는다.

Unity에서 실제 물리 업데이트로 양방향 이동·정지·재출발·시각/충돌 높이와 기존 방 리스폰을 확인했다. 검사 3개가 통과했으며 결과는 `ValidationArtifacts/Wave/UnityResults.xml`에 있다. 재실행은 `Validation/Wave/Run-Validation.ps1`을 사용한다.

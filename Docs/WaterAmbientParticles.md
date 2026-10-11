# 직접 배치하는 파란 파티클

자동 카메라 연결은 제거했다. Assets/EmberPrototype/Prefabs/BlueAmbientParticles.prefab을 Hierarchy 또는 Scene 뷰에 드래그해 원하는 구역에 직접 배치한다. 기본 Z는 0이다. 카메라에 붙이지 않아도 된다.

선택 후 Inspector의 Particle System에서 다음 값을 조절한다.

- Shape > Scale: 입자가 생길 구역의 가로·세로 크기. 기본 약 17.78 × 10.
- Emission > Rate over Time: 초당 생성 개수. 기본 8.
- Main > Start Size: 입자 크기. 기본 0.025~0.07.
- Main > Start Color: 파란색과 투명도.
- Main > Start Lifetime: 유지 시간. 기본 3~6초.
- Velocity over Lifetime > Y: 위로 떠오르는 속도. 기본 0.04~0.16.
- Renderer > Order in Layer: 그리는 순서. 기본 2.

Scene 뷰에서 오브젝트를 선택하고 Particle Effect 패널의 Play를 눌러 미리 보며 편집한다. 플레이 모드에서만 바꾼 값은 종료하면 돌아가므로, 저장할 설정은 플레이 종료 상태에서 변경한다. 프리팹 인스턴스별로 다른 구역 크기를 설정할 수 있다.

이 프리팹은 기본 Particle System만 포함한다. 색상과 밀도를 덮어쓰는 런타임 스크립트가 없다. 조명과 상관없이 보이는 소프트 파란 점용 머티리얼·텍스처가 함께 연결되어 있다.

이전에 카메라에 WaterAmbientParticles를 직접 추가했다면 그 컴포넌트를 제거해 중복 효과를 피한다.

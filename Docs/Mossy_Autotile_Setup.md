# Mossy 타일맵

- 원본 `Assets/tile/Mossy Tileset/Mossy - TileSet.png`: 512×512 픽셀, 7×7 격자 = 49개.
- 자동 연결: 47가지 이웃 패턴을 가진 `Mossy Terrain RuleTile`.
- 남은 2개는 내부 타일 변형이며, 수동 배치용으로 보관한다.
- 원본 PNG, 기존 씬, 기존 테스트 팔레트는 변경하지 않는다.

## 그리는 법

1. `Window > 2D > Tile Palette`에서 `Mossy Terrain Palette`를 선택한다.
2. 팔레트 **위쪽 3×3 샘플 또는 그 오른쪽의 단독 타일**을 선택해 그린다. 이곳이 자동 연결 브러시다.
3. 아래쪽 7×7은 수동 타일이다. 자동 연결을 원하면 위쪽 브러시만 사용한다.
4. 씬의 `Grid > Cell Size`는 X=1, Y=1. 원본 PPU=512이므로 타일 하나가 1 Unity 단위다.
5. 지형용 Tilemap에 `Tilemap Collider 2D`를 추가한다. 이어서 Static `Rigidbody 2D`와 `Composite Collider 2D`를 추가하고, Tilemap Collider의 `Composite Operation`을 `Merge`로 설정하면 타일 사이의 충돌 경계를 합칠 수 있다.

## 초기 생성

기존 RuleTile과 팔레트를 Tile Palette에서 사용한다. 생성·자동 초기화 스크립트와 Setup Mossy Autotile 메뉴는 정리했다.

이전 Validate Mossy Autotile 메뉴는 제거했다. 아래 규칙 설명은 기존 애셋의 제작 기록이다.

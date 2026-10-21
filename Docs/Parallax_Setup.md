# 패럴렉스 설정

## 어디에 붙이나

배경 이미지 오브젝트 각각의 기존 `Parallax Layer`를 사용한다. `Main Camera`에는 붙이지 않는다. 배경 부모는 카메라 자식으로 두지 않는다. 실제 씬 배치는 자동으로 변경하지 않았다.

## 01 화면 채움용 배경

- Sprite: `Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_01_Atmosphere_v2.png`
- Camera Transform: Main Camera
- Follow X / Y: 1 / 1
- Repeat X: 끔
- Play를 끈 상태에서 카메라 중심에 배치하고, 가장 큰 줌아웃 화면보다 넉넉하게 키운다. 자동 화면 맞춤/스케일 변경 기능은 없다.

## 02 좌우 반복 동굴

- Sprite: `Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_02_CavernRepeatX_v2.png`
- Camera Transform: Main Camera
- Follow X / Y: 우선 0.8 / 0.8
- Repeat X: 켬
- Sprite Renderer의 Draw Mode: Simple
- 배경과 부모의 Rotation: 0
- 원본 한 장만 배치한다. 수동으로 여러 장 복제할 필요가 없다.
- 콜라이더나 Rigidbody를 배경에 추가하지 않는다.

## 반복 방식

Play 중 좌우 복사본을 자동으로 만든다. 보통 원본 포함 세 장이며, 카메라 줌아웃/종횡비에 따라 필요한 장수를 자동으로 늘린다. 넓어졌다 좁아지는 경우 만들어둔 복사본을 재사용한다.

이동 거리가 커지거나 리스폰으로 카메라가 순간이동해도 카메라 근처에 타일들을 다시 배치한다. 매 프레임 이동량을 누적하지 않아 반복 위치가 계속 밀리지 않는다.

간격은 `Sprite.rect.width / Pixels Per Unit × 가로 스케일`을 사용한다. 렌더링된 모양의 Bounds가 아니라 전체 스프라이트 사각형을 기준으로 하므로, 이미지 양끝에 만든 투명 연결 여백이 보존된다.

복사본에는 Transform과 SpriteRenderer만 있다. 원본의 스크립트·콜라이더·자식 오브젝트는 복제하지 않는다. 스프라이트, 색, 공유 머티리얼, MaterialPropertyBlock, Flip, Sorting 설정을 반영한다. Repeat X를 끄거나 컴포넌트를 비활성화하면 자동 생성한 복사본만 정리한다.

## 제한과 주의

- X 반복만 지원한다. 현재 02 이미지는 Y 반복용이 아니다.
- 수평 패럴렉스는 `Follow X = 1`일 때 화면상 고정, 0일 때 월드 고정이다. 셀레스트의 Scroll 값과 의미가 반대이므로 숫자를 그대로 옮기지 않는다.
- 회전된 배경, Sliced/Tiled Draw Mode, 비정사영 카메라는 반복하지 않고 경고한다. 일반 패럴렉스 이동은 유지한다.
- 한쪽당 최대 32개의 복사본을 허용한다. 배경이 지나치게 작아 더 많이 필요하면 경고한다. 이 경우 Scale/PPU를 조정한다.
- 기존 03·04는 반복 이음새가 준비되지 않았으므로 Repeat X를 켜지 않는다.
- 이 변경은 이미지, `.meta`, 카메라 코드, 씬, 프리팹, 패키지 설정을 바꾸지 않는다.

## 확인 순서

1. 03·04를 잠깐 끄고 01·02만 확인한다.
2. 카메라가 움직이는 구간에서 좌우로 그림 한 장 너비 이상 이동한다.
3. 끝이 드러나거나 이미지가 갑자기 건너뛰는지 확인한다.
4. 카메라 줌아웃과 리스폰 후에도 복사본이 화면을 덮는지 확인한다.
5. 위아래 빈 화면이 보이면 01 위치/크기를 확인한다. Repeat X가 Y 화면 채움까지 담당하지 않는다.

## 검증 도구

`Validation/Parallax/Run-Validation.ps1`은 원본 씬과 패키지를 변경하지 않고 별도의 작은 Unity QA 프로젝트에서 EditMode 테스트를 실행한다. 테스트 결과는 `ValidationArtifacts/Parallax/ParallaxResults.xml`, 로그는 같은 폴더의 `parallax.log`에 저장한다.

테스트는 반복 간격, 투명 여백, 양방향/긴 거리 이동, 확대된 화면의 커버 범위, 복사본 재사용, 부모 스케일·Flip·Pivot, 렌더 설정 동기화, 복사본 정리 및 잘못된 설정 처리를 검사한다. 실제 아트의 미적 자연스러움과 사용자 씬의 플레이 화면은 별도 확인해야 한다.

검증 결과: 프로젝트 C# 컴파일 오류 0개(기존 경고 32개), Unity 6000.5.2f1의 별도 QA 프로젝트에서 EditMode 테스트 11개 모두 통과. 테스트는 컴포넌트 콜백과 생성된 복사본을 검사하며, 사용자 씬의 실제 Play 화면 검증을 대신하지 않는다.

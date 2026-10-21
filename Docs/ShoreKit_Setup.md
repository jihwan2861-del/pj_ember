# 강가 그레이박스·낙서풍 스킨 키트 — 2026-10-19

## 최신 사용자 피드백

기존 Storyscene 강둑·첫 두 발판에 키트를 실적용해 Game View로 비교한 뒤 사용자가 기존 강가가 더 좋다고 밝혔다. 실적용 변경은 적용 직전 씬으로 모두 되돌렸다. 이 키트는 별도 실험용 애셋이며 현재 강가의 확정 외형이 아니다. 기존 강가 실루엣·충돌·카메라·반사 연결을 유지한다.

## 이번 제작 범위

강가를 타일맵으로 설계하고 분리한 그림을 얹기 위한 1차 재사용 키트다. Storyscene 기존 배치를 교체하거나 새 씬을 저장하지 않았다. 원형·물가·능력 교육을 하나의 이야기로 합치는 레벨 재설계는 이후 작업이다.

## 시작할 프리팹

폴더: `Assets/Ember/Prefabs/LevelDesign/ShoreKit`

| 파일 | 용도 |
| --- | --- |
| ShoreRoom_Empty.prefab | 비어 있는 0.5U Grid, Terrain 충돌, Art 자동 스킨, CameraRoom과 Spawn |
| ShoreRoom_GrayboxExample.prefab | 양쪽 강둑과 징검다리3개의 그레이박스 예제 |
| ShoreRoom_Example.prefab | 같은 지형에 돌 스킨·나무·풀·배경·물 반사 연결한 조립 예제 |

기존 Level Workflow의 CreateRoom으로 뼈대를 만들고 프리팹으로 저장했다. 새 생성 메뉴나 편집 창은 추가하지 않았다. 사용자가 원하는 씬에 프리팹을 배치하고 카메라·플레이어·기믹을 연결한다.

## 규격과 편집

- Grid Scale=(0.5,0.5,1), Cell Size=(1,1,0), 직사각형. 한 칸=0.5U.
- CameraRoom=18×10.125U, 일반16:9 화면36×20.25칸, 카메라 Size5.0625 기준.
- 팔레트: `Assets/Ember/LevelDesign/ShoreKit/Palettes/Ember Shore.prefab`, Tile Palette 창에서 **Ember Shore** 선택.
- `Grid/Terrain`: Shore_Terrain을 칠하는 충돌 원본. Static Rigidbody2D + TilemapCollider2D Merge + Composite Polygons.
- `Grid/Art`: TilemapVisualSkin이 생성하는 그림 전용 출력. 충돌 없음. 직접 칠한 내용은 다음 갱신 때 덮어쓴다.
- `Grid/Decorations`: 수동 장식 타일용. `Objects`는 개별 나무·돌·풀 프리팹용.
- `Grid/Background`: 배경 타일용. `Backdrop`은 산·숲·구름 스프라이트용.
- 그레이박스만 보려면 Art와 장식·배경·Water를 끈다. GrayboxExample에는 이미 이 상태가 저장돼 있다.

`TilemapVisualSkin`은 Terrain의 타일 변경을 받아 같은 셀에 스킨을 다시 채운다. Terrain/Visual은 같은 Grid·같은 Transform이어야 한다. Fill Tile은 단일 그림용이고 Pattern Tiles는 큰 그림을 분할해 연결하는 선택 설정이다. 이번 키트는 돌 그림을4×4칸에 나눈16개 타일로 연결한다. 음수 셀 좌표에서도 같은 패턴을 유지한다.

현재는 직사각형 셀의 외곽을 그대로 보존하는 돌 채움 스킨이다. 전용 모서리·외곽 펜선 자동 생성, 경사면, 한 방 전체를 새 그림으로 생성하는 GPT 도구는 포함하지 않는다. 잔디는 별도 장식 프리팹으로 배치한다.

## 개별 그림과 프리팹

원본 PNG: `Assets/Ember/Art/Storybook/ShoreKit`

| 이름 | 용도 / 캔버스의 월드 너비 |
| --- | --- |
| Tree_Dry_A / Tree_Healthy_A | 같은 나무의 마른·푸른 버전 /6U |
| Grass_Dry_A / Grass_Healthy_A | 같은 풀의 마른·푸른 버전 /2U |
| Rock_A | 둥근 작은 돌 /1.5U |
| Rock_B | 세로로 긴 돌 /1U |
| Bush_Healthy_A | 꽃이 조금 섞인 푸른 관목 /2U |
| Mountains_A | 산 레이어 /18U |
| Forest_A | 먼 숲 레이어 /18U |
| Cloud_A | 구름 한 개 /4U |
| Stone_Fill_A | 돌 채움 원본 /1U |
| Stone_Pattern_4x4 | 같은 돌 원본의 Unity 분할 애셋 /각 조각1로컬U=0.5월드U |

11개 그림을 기본 image_gen으로 개별 생성했다. Stone_Pattern_4x4는 원본을 복사해 Sprite Editor API로16개 SpriteRect를 설정한 추가 PNG다. 생성 프롬프트와 원본 위치는 `ArtReference/ShoreKit/GenerationManifest_v1.json`에 있다.

나무·풀의 푸른 버전은 마른 원본을 편집해 만들었다. 같은 기준점과 캔버스 너비로 가져와 원점·축척을 맞췄다. 장식 스프라이트는 Full Rect·Bilinear·mipmap off, 그림 전용 Unlit 재질이며 충돌체가 없다. 지면 오브젝트의 pivot은 마른 원본의 불투명 하단을 기준으로 잡았고 짝에도 같은 값을 사용한다.

## 물과 반사 연결

Example의 Water는 기존 StoryWater.mat와 WaterSurface2D를 재사용한다. 기존 수면 재질·셰이더는 수정하지 않았다.

- 수면 높이: 예제 로컬Y=-0.5, 깊이5U.
- 실제 나무·풀은 마른 버전이다. `PastReflectionSources`에는 대응하는 푸른 버전을 같은 좌표·축척으로 두었다.
- 푸른 원본은 **EmberPastReflection(31)** 레이어이며 일반 카메라에서 제외해야 한다. 현재 Storyscene 카메라는 이미 제외한다.
- Past Reflection Layers / Withering Reflection Layers에는 나무2개+풀6개=8개를 연결했다. 현재 WaterSurface2D의 최대8개 제한을 따른다.
- Start With Past Reflection=true, Preview Past Blend=1. 예제의 자동 위치 시작은 꺼져 있다.
- 편집 미리보기: Preview Tree Withering=0은 푸른 반사,1은 마른 반사·어두운 물빛이다.
- 재생 때 시듦을 시작하려면 기존 이벤트에서 BeginTreeWithering을 호출한다. 플레이어 위치 시작을 사용할 때는 Player와 Tree Wither Start X를 실제 배치에 맞게 연결한다.
- 새 씬에서 반사를 쓰려면 URP Renderer2D의 Camera Sorting Layer Texture 설정을 기존 StoryWater_Setup.md와 맞춘다.

예제는 수면 그림·반사 연결용이다. 물 접촉 소화, 낙하 사망, 체크포인트, 태양 전환, 나레이션, 꽃 교육 이벤트는 기존 기능을 해당 레벨에 별도로 연결한다. 기존 Storyscene의 연결은 유지했다.

## 확인 범위

실제 Editor Game View에서 임시 예제를 같은 카메라로 캡처했다.

- Terrain218셀 / Art218셀 / 셀 불일치0 / Art 충돌체0 / 실제 셀 간격0.5U.
- 첫 스킨 캡처에서 과도하게 작던 돌 무늬를4×4 연결로 수정하고 재캡처했다.
- 푸른 나무·풀 반사와 시듦 완료·어두운 물빛을 각각 확인했다. 이는 편집 미리보기 확인이며 플레이어가 이 예제를 완주한 확인은 아니다.
- 확인용 인스턴스를 제거하고 원래 카메라를 복원했다. 기존 Storyscene 저장 내용과 바이트가 동일함을 확인해 씬을 원래의 clean 상태로 정리했다.

캡처: `ArtReference/ShoreKit/Captures/01_Graybox.png`, `02_Skin_First.png`(수정 전), `03_Skin_Past.png`, `04_Skin_Withered.png`.

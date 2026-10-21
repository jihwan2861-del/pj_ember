# WALLCOEUR 불꽃·연기 VFX 적용 — 2026-10-21

## 2026-10-21 최신 — 애니메이션 투사체 덩어리·가연물 연소 효과

- 고정된 AI풍 `FireOrb_Outlined_Doodle_v1.png`는 사용자 피드백으로 채택을 취소하고 `Assets/Ember/Art/VFX`에서 PNG와 메타 파일을 제거했다. `RideableFlowerFlame.prefab`에는 해당 스프라이트 참조가 없다.
- 가져온 `a_VFX_flame.png` 3×3 플립북을 세 겹으로 겹친 `Assets/Ember/VFX/Wallcoeur/Prefabs/ProjectileCore_Animated_Outlined.prefab`를 만들었다. 같은 파티클 배치를 짙은 외곽, 주황 불꽃 덩어리, 노란 중심 순서로 그리며, 세 레이어는 로컬 시뮬레이션·초당38개·수명0.44초로 움직인다. `RideableFlowerFlame.prefab`의 `Animated Projectile Core`가 이를 사용하고, 기존 `FlowerFlight_Outlined`는 짧은 잔불 꼬리로 뒤에 남는다. 프로젝트 원본 VFX 팩은 수정하지 않았다.
- 발사·탑승·조향·속도·충돌 반경은 변경하지 않았다. 루트 SpriteRenderer는 코드의 정렬 기준으로 남기되 sprite=null/렌더 비활성으로 뒀다. 꼬리 연기 렌더는 비활성이다.
- `HangingKindling.prefab`은 `burningSprite` 참조를 비웠다. 태울 때 기존 `HangingWood_Unlit` 그림은 유지되고 `FlammableTile`이 `FireGlow`를 켜서 효과를 재생한다. 기존 Light2D와 `AmbientFire_Outlined`를 유지하면서 불꽃을 1.25배 키우고, 밑면에 `GroundFire_Outlined`를 0.45배로 추가했다. 소화/리스폰은 원래 `FlammableTile` 흐름을 따른다.
- `ArtReference/WallcoeurVFX/11_AnimatedProjectile_Frame_A.png`부터 `13_AnimatedProjectile_Frame_C.png`까지는 서로 다른 플립북 시점, `14_AnimatedProjectile_InRoom.png`는 그레이박스 방 크기에서 본 Unity PreviewScene 렌더다. 실제 Play 입력이나 런타임 이동은 이번에 실행하지 않았다.
- 현재 편집 씬은 `skin scene.unity`로 그대로 두었고 dirty 상태도 보존했다. TutorialScene이나 Storyscene을 다시 저장하지 않았다.

사용자가 Asset Store에서 가져온 `Assets/VFXPACK_FIRE_WALLCOEUR`를 탑승 불꽃, 환경, 연출에 함께 사용하도록 승인했다. 프로젝트용 복사본은 `Assets/Ember/VFX/Wallcoeur`에 있다. 원본 패키지와 텍스처는 수정하지 않았다.

## 프리팹

| Prefabs 폴더의 이름 | 용도 | 현재 연결 |
| --- | --- | --- |
| FlowerFlight_Outlined | 이동하는 불꽃의 짧은 잔불·외곽선 연기 | RideableFlowerFlame의 Wallcoeur VFX 자식, Storyscene 꽃 에너지 이동 |
| FlameBurst_Outlined | 한 번 퍼지고 사라지는 불·연기 | 꽃 탄환 소멸, Storyscene 에너지 도착, TutorialScene 앰버 변신 |
| AmbientFire_Outlined | 반복되는 불·연기 | HangingKindling/FireGlow의 Wallcoeur VFX 자식 |
| AmbientSmoke_Outlined | 배치용 외곽선 연기 | 별도 환경용 프리팹 |
| GroundFire_Outlined | 배치용 바닥 불 | 별도 환경용 프리팹, 시각 효과 전용 |

환경 효과는 Hierarchy의 원하는 방 `Mechanics`/장식 부모 아래에 프리팹을 놓고 크기를 조절한다. 불과 연기 양은 Particle System의 Emission, 크기는 Main/Start Size, 색은 Main/Start Color에서 조절한다. 가연물에 연결할 때에는 해당 `FlammableTile.fireEffect`가 활성화하는 오브젝트 아래에 두면 점화/소화/리스폰에 따른다. 현재 장작의 기존 FireGlow와 Light2D는 유지했다.

## 렌더링과 외형

- 원본 3×3 플립북을 유지했다. 불꽃은 `a_VFX_flame.png`, 외곽선 연기는 `A_Smoke_2.png`를 참조한다.
- 원본 Mobile/Particles 셰이더 대신 URP/Particles/Unlit 재질2개를 프로젝트 폴더에 만들었다. 밝은 종이 배경에서도 색과 검은 선이 보이도록 Alpha 혼합을 사용한다.
- Mesh/Horizontal Billboard가 섞인 원본은 횡스크롤용 Billboard와 XY 이동으로 맞췄다. Collider·피해 판정은 추가하지 않았다.
- 탑승 불꽃의 현재 외형은 `ProjectileCore_Animated_Outlined`의 외곽·주황 덩어리·노란 중심 플립북 레이어와 뒤쪽 Wallcoeur 잔불이다. 기본 발사 속도, 탑승 가속, 조향, 최대2초/하차후1초, C 발사/X 탑승/Z 하차는 변경하지 않았다.
- 파티클은 게임 시간으로 움직이고 수를 제한한다. 일회성 Burst는 반복하지 않고 루트가 자동 삭제된다. 탄환 Retire에서만 소멸 효과가 나오며 맵 리셋에는 터지지 않는다.

## 연출 연결

꽃 탄환의 정상 소멸에는 약한 화면 흔들림도 함께 호출한다. `RideableFlowerFlame.prefab`의 **Fire Flower Projectile → Despawn Screen Shake**에서 켜기/끄기, 지속 시간0.12게임초, 강도0.05U를 조절한다. 화면 안·같은 씬의 활성 CelesteRoomCamera에만 적용하고 페이드/일시정지/카메라 이동 중단 및 리셋에는 호출하지 않는다. 환경 불·연기와 스토리 탄생 Burst에는 자동 적용하지 않는다. 세부 기준은 `Docs/FlowerRide_Setup.md`의 최신 항목을 따른다.

`StoryFlowerEnergySequence`의 Energy Particles Prefab은 기존 베지어 경로를 따라간다. 도착 시 방출을 멈추고 Arrival Particles Prefab을 재생한다. 기존 .85초 이동/.25초 여운/페이드 타이밍은 유지하며, 도착 파티클은 페이드 시작 때 기존 효과 루트와 함께 정리된다.

`EmberBirthSequence.birthParticlesPrefab`은 앰버 모습으로 바뀌는 순간 한 번 재생한다. 기존 빛 선이 꺼진 뒤에도 입자가 잠시 남을 수 있도록 별도 소유하고, 정상 완료/Skip/비활성화 때 정리한다. 중복 재생을 막고 기존 잠금·카메라·연출 시간은 유지했다.

TutorialScene과 Storyscene의 새 프리팹 참조는 저장했다. 연결 중 Storyscene의 `FireFlower.head` 및 `StoryFlowerEnergySequence.energyOrigin`이 GameObject를 가리켜 Transform 형식 불일치를 내던 기존 문제를 발견해 같은 Head의 Transform으로 재연결했다. Head/Petals/Muzzle의 실제 위치·회전·크기는 유지했다. 변경 전 씬 파일은 `Library/EmberTutorial/BeforeWallcoeurVFX`에 있다.

## 확인 범위

- Unity 컴파일·정적 생명주기 검토 및 프리팹/씬 참조 확인.
- `ArtReference/WallcoeurVFX/01~06_*_EditorPreview.png`: 저장된 TutorialScene과 임시 배치 화면의 Unity 렌더 미리보기. 실제 Play/입력 완주 캡처는 아니다.
- 캡처 도구에서 재생 중 randomSeed를 다시 지정하던 오류는 초기화 시에만 지정하도록 수정하고 재캡처했다. 이는 런타임 코드와 분리된 Library 임시 도구다.
- 현재 skin 씬의 미저장 편집과 배치는 유지했다. 전체 Storyscene→TutorialScene 입력 완주·빌드·기기 성능 측정은 이번에 하지 않았다.

# 태양꽃 — 배치와 오염 정화

## 현재 기준 — 2026-10-20

소형 꽃은 사용하지 않고, 불꽃에 탑승할 수 있던 기존 대형 꽃을 **태양꽃**으로 통일한다. 정식 프리팹은 `Assets/Ember/Prefabs/Tutorial/SunFlower.prefab`이며 이전 SunFlower_Giant의 GUID를 유지한다. Body/Head 분리와 Head 로컬+X 발사 방향은 같다.

C 또는 다른 태양꽃 탄환 명중으로 활성화하고 미리 정한 방향으로 발사한다. 플레이어 접촉으로 발사하지 않는다. 탑승 불꽃에 X로 탄 뒤 최대2게임초 조종하고 Z/시간 만료로 하차한다. 하차 뒤 불꽃은 마지막 방향으로1게임초 직진한다. X 탑승에 C 점화 링은 필요하지 않다. 세부 설정은 [태양꽃 탑승 설정](FlowerRide_Setup.md)을 따른다.

과거 작은 픽셀 꽃 프리팹·아트는 이미 삭제되어 있었으며 FireFlowerValidation 씬의 작은 임시 꽃 오브젝트도 제거했다. 해당 옛 테스트 씬을 현재 태양꽃 테스트 장소로 안내하지 않는다. `FireFlower`와 `FireFlowerProjectile` 코드는 정식 태양꽃도 사용하므로 유지한다.

## 2026-10-20 낙서 애셋 3종 적용 — 사용자 승인 완료

메인 화면 산처럼 단순한 낙서풍으로 다음 3개 PNG를 만들고 기존 태양꽃·튜토리얼 오염에 연결했다. 실제 Play에서 새 머리가 Awake·발사·리스폰 후에도 유지되고, 오염 직접 접촉 시 플레이어 `IsDying=true`를 확인했다. 사용자가 적용 화면을 보고 “좋다 다음가자”라고 답해 이번 태양꽃 머리·몸통·접촉 오염 3종은 승인 완료했다. 다음 중간 발판 마른/푸른 짝과 산 배경 묶음은 별도 확인 대상이다.

| 용도 | 최신 경로 |
| --- | --- |
| 태양꽃 머리 | `Assets/Ember/Art/Storybook/DoodleMechanics_v1/SunFlower_Head_Doodle_v1.png` |
| 태양꽃 줄기·잎 | `Assets/Ember/Art/Storybook/DoodleMechanics_v1/SunFlower_Body_Doodle_v1.png` |
| 접촉 위험 오염 | `Assets/Ember/Art/Storybook/DoodleMechanics_v1/Pollution_Thorns_Doodle_v1.png` |

`SunFlower.prefab`의 `Body`·`Head/Petals` 그림과 `FireFlower.wiltedSprite`·`bloomingSprite`를 교체했다. 머리는 1254×1254, PPU 500, pivot (0.17,0.5), 몸통은 1024×1536, PPU 600, pivot (0.52,0.025)로 기존 규격을 유지했다. 기존 Transform·Head·Muzzle과 꽃 Collider 수치는 보존했다.

`FlowerRideTutorial.prefab`의 `Pollution` Tilemap 12셀과 기존 solid Collider는 유지하고 `TilemapRenderer`만 숨겼다. 그 아래 `DoodleArt`와 `ContactHazard`를 추가했으며, 접촉 위험은 BoxCollider2D trigger와 `RoomHazard`로 처리한다. 로컬 중심은 (35,4), 위험 영역의 로컬 크기는 (2.08,6.08)이다. 기존 `PollutionObstacle`이 정화 및 리스폰 복구를 계속 담당한다.

실제 꽃 탄환이 오염 벽에 맞으면 `IsPurified=true`와 함께 TilemapCollider2D·위험 BoxCollider2D·그림 SpriteRenderer가 모두 꺼졌다. `ResetForRespawn` 후 기존 solid Collider·위험 트리거·그림은 복구되고 TilemapRenderer는 숨김을 유지했다. solid는 `isTrigger=false`, 접촉 위험 자식은 `true`다. 두 꽃 프리팹의 Body·Petals·wiltedSprite·bloomingSprite 4개 참조도 새 PNG임을 읽기 전용으로 확인했다. 자동 테스트나 전체 튜토리얼의 실제 입력 완주는 수행하지 않았다. 적용 캡처는 `ArtReference/OpeningWater/DoodleMechanics_v1`, 내장 image_gen 생성 프롬프트는 같은 폴더의 `GenerationPrompts.txt`에 기록한다. 새 이미지 3개마다 적용 화면을 확인받는 규칙을 유지한다.

## 아래는 이전 구현 기록

아래의 소형/대형 미결정·옛 프리팹 경로·비탑승 탄환·픽셀 폴백·테스트 씬 안내는 당시 기록이며 현재 제작 지시가 아니다.

## 2026-10-19 대형 태양꽃 튜토리얼 설계

불꽃과 태양꽃의 반응으로 앰버가 탄생한 직후 정화 튜토리얼을 진행한다.
대형 태양꽃 발사 → 슬로모션·X 키 안내 → 실제 X 입력으로 해당 불꽃에 탑승하는 흐름이다.
탄생 반응과 튜토리얼 발사는 구분해 플레이어가 조작과 결과를 볼 수 있게 한다.
대형만 제작하거나 대형·소형별 기능을 나누는 결정은 아직 하지 않았다.

현재 FireFlowerProjectile는 오염 정화용이며 FlammableTile 탑승 대상이 아니다.
기존 X 이동은 이동하는 표적을 추적할 수 있지만 탑승 후 위치를 계속 따라가는 처리는 없다.
따라서 탑승용 앵커·이동 추종·소멸 전 하차와 슬로모션/X 안내 연결은 다음 구현 범위다.
이 절은 기획 기록이며 새 기능이나 씬 배치가 완료됐다는 뜻이 아니다.

불꽃꽃은 반응 범위 안에서 성공한 C 점화 폭발에 반응한다. C는 꽃을 활성화하는 역할이다. 꽃마다 미리 지정한 Head 각도 방향으로 즉시 발사한다. 플레이어 위치는 거리 판정에만 사용한다. 총알은 플레이어를 통과하며 오염을 정화한다. 방향 변경 장치는 추후 검토한다.

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

Purified Visual에 정화 후 식물·금빛 장식을 연결할 수 있다. On Purified 이벤트는 문 열기 같은 추가 기믹 연결에 사용한다. 꽃 불꽃은 오염을 파괴하는 정화 수단이다. 플레이어와 그 자식 충돌체를 통과하며 피해나 사망을 일으키지 않는다. 일반 벽에서는 멈춘다.

## 테스트 씬

Assets/EmberPrototype/FireFlowerValidation.unity를 열어 Play한다. 중앙 발판에서 C → 설정 방향 발사 → 오른쪽 오염 정화를 확인한다. 이전 테스트 씬에는 Warning Time이 설정되어 있을 수 있어 즉시 발사를 확인하려면 0으로 설정한다. R은 씬을 다시 시작한다.

이전 테스트 씬 생성 메뉴는 정리했다. 기존 테스트 씬은 참고용으로 보존하고 실제 게임 씬은 사용자가 직접 구성한다. 검사 코드는 Validation/EditorSupport에 있다.

사망/맵 초기화 시 발사 준비, 꽃 활성 상태, 날아가는 불꽃, 정화된 오염은 초기 상태로 돌아간다. 체크포인트·스토리 진행과 별개의 임시 기믹이다.

## 이전 버전 검증 기록 (플레이어 위치 조준 방식)

Unity 6000.5.2f1의 격리 프로젝트에서 관련 검사 6개가 통과했다. 실제 Play Mode의 C 입력 → 위치 고정 → 플레이어를 발사 경로 밖으로 이동 → 오염 정화를 확인했다. 추가로 고정 방향 발사, 범위 밖/중복 반응 차단, 벽 충돌, C 직접 정화 방지, 맵 초기화를 검사했다. 결과는 ValidationArtifacts/FireFlower/UnityResults.xml과 source-snapshot.json에 있다.

꽃·발사체·테스트 지형은 동작 검증용 임시 표현이다. 시각 참고 이미지는 ArtReference/FireFlower_FixedAim_Reference.png에 보관했다. 최종 8×8 타일 아트와 오프닝 연출은 이번 기믹 구현에 포함되지 않는다.

## 2026-10-14 화염꽃 디벨롭

새 프리팹은 Assets/EmberPrototype/Prefabs/FireFlower.prefab과 FireFlowerProjectile.prefab이다. FireFlowerSheet.png의 꽃 4종 및 총알 4프레임을 사용한다. C 반응 즉시 설정 방향으로 발사하며 꽃 개화 애니메이션을 동시에 재생한다. Warning Time 0은 즉시 발사, 양수는 기존 예고 방식이다. 새 프리팹은 반복 사용 가능하며 기본 쿨다운은 1.5초다.

Assets/Scenes/New Scene.unity의 Mechanics 아래 Spawn 왼쪽에 배치했다. 해당 씬에는 현재 플레이어가 없어 실제 입력 플레이 검증은 남아 있다. 발사체는 일반 벽에서 멈추고 꽃 전용 오염을 정화하며 플레이어는 통과한다. 생성 스프라이트는 8×8 지형 타일과 별개 크기의 시안이다.



- 머리/몸 분리 프리팹은 Head의 로컬 +X(오른쪽 축)를 발사 방향으로 사용한다. 기존 미분리 꽃은 이전 저장된 방향을 유지한다.

## 머리와 몸 분리

FireFlower 아래 Body는 줄기와 잎을 표시하고, Head는 꽃 머리만 표시한다. Head/Muzzle은 머리 중심의 발사 지점이다. 개화 애니메이션과 리스폰 초기화는 Head의 스프라이트에만 적용한다.

Hierarchy에서 FireFlower > Head를 선택하고 Transform의 Rotation Z를 설정한다. 0도는 오른쪽, 90도는 위쪽, 180도는 왼쪽, -90도는 아래쪽이다. Body와 루트를 회전하지 않고 Head만 회전하면 줄기는 그대로 유지된다. 꽃 루트를 선택하면 Scene의 화살표로 최종 방향을 확인할 수 있다. 플레이 중 C는 활성화만 하며 머리는 플레이어를 추적하지 않는다.

분리 이미지: Assets/EmberPrototype/Art/FireFlower/FireFlowerParts.png. 몸 1종과 시든 머리/개화 중/개화 완료 3종을 포함한다. 총알은 기존 FireFlowerSheet.png의 4프레임을 사용한다.


## 플레이어 접촉 활성화

C 외에 플레이어가 꽃의 BoxCollider2D 트리거에 직접 닿아도 활성화된다. Activate On Player Contact로 접촉 반응을 설정한다. 머리 방향으로 즉시 발사하며 총알은 플레이어를 통과한다. 다른 물체나 총알은 접촉 활성화 대상으로 취급하지 않는다. 플레이어의 자식 충돌체도 인식한다.

계속 닿아 있는 동안 자동으로 반복 발사하지 않는다. 반복 사용 꽃은 쿨다운이 끝난 뒤 떨어졌다가 다시 닿거나 C를 눌러 재활성화한다. 리스폰으로 초기화된 꽃은 겹쳐 있는 플레이어에도 반응한다. 접촉 영역은 C Reaction Radius와 독립적이다.

## 임시 붉은 가시 오염 장애물

Assets/EmberPrototype/Prefabs/PollutionSpikes.prefab을 배치한다. 붉은 가시 띠 이미지 PollutionSpikes.png를 사용하는 약 2유닛 너비의 임시 장애물이다. 화염꽃 총알이 닿으면 해당 프리팹 한 덩어리의 렌더러와 충돌체가 꺼져 길이 열린다. 독립적으로 제거하려면 프리팹을 여러 개 배치한다. 바닥용은 기본 각도, 벽/천장용은 루트를 회전해 배치한다.

RoomHazard로 플레이어 접촉 시 사망하고, PollutionObstacle로 꽃 총알에 정화된다. 정화 뒤에는 위험 충돌도 꺼진다. 기존 맵 초기화에서는 복구된다. 새 씬에는 화염꽃 발사 방향 앞 3유닛에 한 덩어리를 배치했다. 배치용 접촉 박스는 그림 안쪽에 설정되어 있으며 최종 아트가 아닌 임시 시안이다.

이전 가시 생성 메뉴는 정리했다. 기존 프리팹과 Burr Pollution 팔레트를 직접 사용한다.

### 2026-10-15 가시 형태 수정

PollutionSpikes.png는 작은 둥근 붉은 중심 덩어리와 주변의 짧고 촘촘한 가시가 이어지는 밤송이 군집으로 교체했다. 긴 수정 기둥 형태는 채택하지 않는다. 같은 애셋 경로와 GUID를 사용하므로 기존 씬/프리팹에서도 새 모양이 표시된다.

## 2026-10-15 개별 밤송이 타일 3종

최신 요청은 타일맵에 찍는 개별 밤송이 3종이다. Assets/tile/PollutionBurrs에 Burr_A/B/C.png와 각 Tile .asset이 있다. 각 이미지는 밤송이 한 개이며, Unity에서는 한 칸 크기로 가져온다. 고해상도 픽셀풍 임시 이미지로 실제 8×8 원본은 아니다.

Window > 2D > Tile Palette에서 Burr Pollution 팔레트를 선택하고 Active Tilemap을 PollutionTilemap으로 설정해 그린다. New Scene의 기존 Grid 아래 비어 있는 PollutionTilemap을 추가했다. 다른 씬에서는 PollutionTilemap.prefab을 사용할 수 있다. 꽃 총알에 맞은 타일 한 칸만 제거되며 주변 타일은 남고 맵 초기화 시 복구된다. 오염 전용 맵에 지형 타일을 섞지 않는다. PollutionObstacle을 타일맵 전체에 추가하지 않는다.

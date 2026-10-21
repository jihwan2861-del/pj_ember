# pj_ember 구현 인수인계 — 첫 튜토리얼 씬

## 2026-10-21 최신 — 공용 Grid 제작과 방 진입 체크포인트

Level Workflow의 **공용 Grid에 방 추가**가 기본true다. skin scene은 `WorldGeometry/Grid/Terrain·Spikes·Decorations` 한 벌로 지형을 이어 그리고, `Rooms/SkinRoom_01·02`의 CameraBounds가 화면 범위를 나눈다. `LevelTilemaps`가 각 방과 공용 타일맵을 연결한다. 기존 Mechanics/Paths/DoodleBackdrop과 사용자 오브젝트는 방 아래에 유지했다. 새 공용 방은 CameraBounds와 Spawn만 생성하며 기존 공용맵을 재사용한다. 다른 씬·소스 프리팹의 기존 개별 Grid는 유지한다.

제작은 **Tools > Ember > Level Workflow → 공용 Grid에 방 추가 켜기 → 새 방 생성 → WorldGeometry/Grid/Terrain 선택 후 연속 칠하기** 순서다. 스킨은 자동 반영된다. 가시는 같은 Grid의 Spikes를 사용하고 카메라 방 경계는 별도로 정한다. 공용 지형 전체로 방 경계가 늘어나는 일을 막기 위해 공용 방의 전체 타일맵 범위 맞춤을 비활성화했다. Spawn은 안전한 바닥 위에 놓는다.

CameraRoom.SpawnPoint가 연결된 방에 정상 플레이 중 진입하면 그 Spawn을 자동 저장한다. 같은 방의 중간 체크포인트를 카메라 갱신만으로 덮어쓰지 않으며 연출·조작 잠금·일시정지 중에는 저장을 유예한다. 연결되지 않은 기존 방은 이전 동작을 유지한다. `SkinRoom_Example.prefab`의 예제 Checkpoint_Center/Checkpoint_Finish 두 개만 제거하고 Spawn을 연결했으며 실제 씬은 사용자가 이미 제거한 추가 체크포인트 상태를 유지했다.

공용 Grid 병합은 승인된 Room_02 전체 **(-0.05,-0.12)U 이동**을 포함한다. Terrain575칸·Spikes32칸(총607칸), Composite60shape를 확인했다. 사용자 오브젝트32개는 승인된 이동 외 월드 좌표 변경0개다. Room_01의 map.tint는 셀 색에 곱해 보존했고 두 방의 활성 스킨에 맞춰 Terrain 렌더 표시와 빈 Room_02 Spikes 표시를 통일했다. 전후 화면·정량 결과는 `ArtReference/SkinScene/SharedGrid/01_Before_*`, `02_After_*`, `MigrationResult.json`에 있다.

에디터 확인용 구성에서 생성·재사용·병합·Undo/Redo·셀 색/변환 보존을 확인했고 실제 런타임 컴포넌트를 분리한 동기 체크포인트15항목이 통과했다. 전체 경로 직접 입력 완주는 하지 않았다. 활성 skin scene은 **dirty를 유지**하며 저장 사본만 `Assets/Scenes/Backups/skin_scene_BeforeSharedGrid_20261021.unity`와 `skin_scene_AfterSharedGrid_20261021.unity`에 남겼다. 아래의 방별 Grid·111칸 충돌 수정 수치는 이전 단계 기록이다. 상세 사용법은 `Docs/LevelEditingWorkflow.md`, 현재 구조·검증은 `Docs/SkinScene_Setup.md` 최상단을 따른다.

## 2026-10-21 최신 — Room_02 밝은 타일 충돌 수정

Room_02/Terrain111칸이 충돌 없는 GrayboxBackground 타일이 원인이었다. 사용자가 이 밝은 모양을 선호해 동일 모양의 GrayboxLightSolid(Grid)를 만들고111칸만 교체했으며 공유 Background는 보존했다. Ember Graybox/Ember Doodle 팔레트(0,-2)에 추가했다. 전후 모습은 캡처 픽셀까지 같고 충돌은0→15shape, Play의 분리된 물리 공간 낙하에서 관통→정상접촉/착지를 확인했다. 씬/새타일/팔레트 저장, Edit모드 복귀. 직접 입력 이동 완주는 이번에 하지 않았다. 상세 Docs/SkinScene_Setup.md·ArtReference/SkinScene/Room02CollisionFix/Verification.md.

## 2026-10-21 최신 — 초기 굵은 검정 사선 스킨 복원

사용자 첨부 캡처의 지형 스킨은 `ArtReference/SkinScene/BoldDiagonal/02_Bold_Dense_Full.png` 시기였다. 작업 기록에서 수치를 찾아 Doodle_Stone/04_BoldDiagonal을 Style4·UseHatchMap0·Spacing.105·Width.075·CleanEdge.22·Gradient.94·SolidCore.68·Variation.07로 복원했다. 채색 텍스처 방식보다 이번 선택이 우선한다. 현재 저장된 skin scene420셀과 사용자 배치는 보존했다. 셰이더 오류0, Unity PreviewScene 렌더 확인이며 Play 입력은 미실행. 캡처 `ArtReference/SkinScene/DenseHatchRestored`, 상세 `Docs/SkinScene_Setup.md`.

## 2026-10-21 최신 — 애니메이션 플립북 투사체·가연물 VFX

사용자가 고정 AI풍 불씨 그림을 거부해 `FireOrb_Outlined_Doodle_v1.png`와 `.meta`를 프로젝트에서 삭제했다. `RideableFlowerFlame.prefab`은 이제 `ProjectileCore_Animated_Outlined.prefab`를 자식으로 사용한다. 가져온 Wallcoeur 3×3 불꽃 플립북을 어두운 외곽·주황 덩어리·노란 중심으로 겹쳐 계속 움직이는 단일 불꽃 덩어리처럼 보이게 했다. Wallcoeur 이동 잔불은 뒤쪽에 남았다. 충돌/발사/탑승/가속 수치는 유지했다. 루트 SpriteRenderer는 sorting 참조 때문에 유지하되 sprite=null/비활성이다.

`HangingKindling.prefab`의 burningSprite는 비우고 기존 장작 그림을 유지한다. `FireGlow`에 기존 AmbientFire를 키우고 GroundFire를 덧대어 점화될 때만 VFX가 보인다. 캡처 `ArtReference/WallcoeurVFX/11~14_*`는 프리팹과 실제 TutorialScene을 PreviewScene에서 렌더한 것이다. 애니메이션 시점별 외형을 비교했으며 실제 Play 입력 확인은 아니다. `skin scene.unity`의 미저장 편집/dirty는 그대로 보존했다. 설정은 `Docs/WallcoeurVFX_Setup.md`, `Docs/FlowerRide_Setup.md`.

## 2026-10-21 최신 — 불꽃 폭발 쉐이크

RideableFlowerFlame.prefab의 Fire Flower Projectile → Despawn Screen Shake에서 지속0.12게임초/강도0.05U를 조절한다. 정상 소멸의 기존 Retire에서만 화면 안·같은 씬의 활성 CelesteRoomCamera.Shake를 호출한다. 더 강한 기존 쉐이크와 페이드/일시정지/카메라 중단은 보호하고 맵 리셋에는 호출하지 않는다. 컴파일·프리팹 연결 확인 범위이며 실제 Play 체감 확인은 별도다. 현재 skin 씬의 dirty는 보존했다. 상세 Docs/FlowerRide_Setup.md.

## 2026-10-21 최신 — WALLCOEUR 효과 공용 적용

사용자가 팩을 탑승 불꽃·환경·스토리 연출에 사용하도록 승인했다. `Assets/Ember/VFX/Wallcoeur`의 프로젝트용 URP 재질2개·프리팹5개를 사용한다. 탄환 잔불/소멸, 장작 FireGlow, StoryFlowerEnergySequence의 이동/도착, EmberBirthSequence의 변신 Burst를 연결했다. 원본팩/지형/충돌/조작/가속과 연출 시간은 유지했다. Storyscene과 TutorialScene의 참조는 저장했고, 기존 스토리 꽃 Head/energyOrigin의 잘못된 GameObject 참조를 Head.transform으로 복구했다. 현재 skin씬 미저장 편집은 보존했다. 컴파일·정적 검토·에디터 렌더 미리보기 범위이며 실제 입력 완주는 아니다. 설정·캡처·씬 백업 구분은 `Docs/WallcoeurVFX_Setup.md`를 따른다.

## 2026-10-21 최신 — 꽃 불꽃 가속과 소멸 파티클

탑승 전 속도는 그대로, 탑승 순간0.6배에서1.2배까지4.5U/게임초²로 가속한다. 기본 발사6이면3.6→7.2/.8게임초다. RideableFlowerFlame.prefab의 FireFlowerProjectile/Ride Acceleration에서 조절한다. 이동 거리 적분과 Velocity가 같은 현재 속도를 사용하고 하차 시 그 속도를 그대로 이어받으며 최대2초/하차후1초·조향은 유지한다. 정상Retire에서 FlowerFlameDespawn.prefab을 분리 생성하며 같은 씬/렌더정렬을 따라간다. 기존 Epic Toon FX 텍스처+전용Alpha재질로 불씨28개가 줄어들며 사라지고 StopAction.Destroy로 정리된다. 리셋·씬 종료 OnDisable에는 효과를 만들지 않는다. 코드/불꽃 프리팹/새파티클·재질만 저장했고 씬 배치와 현재 skin 씬 dirty는 유지했다. 컴파일과 에디터 파티클 렌더 캡처를 확인했으나 실제 입력 Play 재완주는 하지 않았다. 설정/캡처 구분은 Docs/FlowerRide_Setup.md를 따른다.

## 2026-10-21 최신 — C+X 사격 입력 제거, X 불 진입·탑승

TutorialScene의 플레이어에 옛 C 링→X 조준/놓기 사격 입력이 남아 있었다. 공통 PlayerAbilityInputRouter에서 사격 입력 연결을 제거하고 링 안에서도 X가 불 진입을 요청하도록 수정했다. C 사용 직후에는 목표 미리보기가 비어 있으므로 RequestFireAbsorb에서 링 상태의 현재 방향 목표를 갱신한다. 기존 X 꽃 탄환 탑승 우선 처리와 가연물 X 점화·진입, Z 이탈을 유지한다. 꽃 발사 C와 탑승 X 안내를 분리했고 C+Z 대시 교육은 유지한다. 이전 사격 컴포넌트/직렬화 필드는 보존하며 씬·프리팹을 다시 저장하지 않았다. 이번 확인은 컴파일·정적 검토이며 실제 Play 재완주는 하지 않았다.

## 2026-10-21 최신 — 태양꽃은 C 또는 꽃 탄환 명중으로 활성화

사용자 정정: 태양꽃은 C 폭발 또는 다른 태양꽃의 탄환이 맞을 때만 활성화하며, 플레이어가 닿거나 계속 붙어 있는 것으로 발사하지 않는다. FireFlower의 접촉 콜백·직렬화 옵션을 제거해 옛 프리팹의true 값으로도 접촉 활성화되지 않게 했다. FireFlowerProjectile.CircleCast는 꽃의 trigger/자식 Collider를 명중 대상으로 포함하고 TryActivateFromFlowerProjectile→기존TryActivateCore→Fire 경로를 사용한다. 발사한 자기 꽃은 제외하며, 이미 사용했거나 준비/쿨다운 중인 꽃에 맞아도 탄환은 기존Retire로 종료해 나중에 겹침으로 재시도하지 않는다. 탑승 중 명중은 기존 안전 이탈을 유지한다. C반응거리·Head방향·쿨다운·oneShot·ShotCount/LastProjectile·정화/리스폰 계약은 유지했다. Storyscene의 시든 꽃은 기존 fireProjectileOnActivation=false와 onActivated 에너지 연출을 유지한다. 씬·프리팹·외형/Collider 배치는 변경하지 않았다. 컴파일/정적 확인을 수행하며 실제 C/연쇄탄환 Play 재확인은 이번 범위가 아니다.

## 2026-10-20 최신 — 태스크 갱신과 탄생 연출 DEV01

사용자가 첫 개발로 탄생→능력 교육을 선택했다. TASKS.md를 최신 실행표로 갱신하고 TutorialSceneEntry에 EmberBirthSequence를 연결했다. 작은 불꽃으로 페이드를 기다린 뒤 기존 꽃 그림과 공명·빛 이동·변신·여운·카메라 복귀(총3.75게임초) 후 잠금을 풀고 첫 X 교육을 시작한다. 시각 전용 꽃이라 발사/충돌은 없고 기존 두 번째 교육 꽃은 유지한다. 직접/Storyscene 경유, 입력 차단/Pause/R/Skip/비활성화, 종료 뒤 실제 X→Z 완료를 확인했다. 설정은 Docs/EmberBirth_Setup.md, 최종 화면은 ArtReference/EmberBirth/03~07·09~10, 변경 전 TutorialScene 백업도 Assets/Scenes/Backups에 보존했다. 새 PNG와 기존 지형/Collider 변경은 없다. 이번2/3교육 재완주·전체 원형부터 완주·빌드는 미확인이다. 아래 탄생 미완료 표기는 과거 기록이다.

## 2026-10-20 최신 — 실제 씬 분리와 계정 인수인계 완료

종합 인수인계는 [Assets 안의 계정 인수인계](Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md)를 먼저 읽는다. 사용자 최신 승인으로 Storyscene(원형·평지·강가 연출)과 TutorialScene(기존 세 능력 교육)을 실제 분리했다. PastNarration 종료→물 완료 대기→.65초 페이드아웃→TutorialScene Single 로드→.4초 페이드인으로 연결하고 Build 목록에 등록했다. TutorialSceneEntry는 (102.11,.35)의 독립 체크포인트·앰버 외형·CelesteRoomCamera를 초기화한다. 01→02→03은 새 씬 안에서 걸어서 연결된다. 최근 사용자 배치와 애셋을 유지했고 분리 전 live 전체는 Assets/Scenes/Backups/Storyscene_BeforeSceneSplit_20261020.unity에 보존했다.

실제 낮은 돌 착지→반사 연출/자동 나레이션→페이드/로드, 새 씬 좌우 이동·X 점화/진입/Z 이탈→C 꽃 발사/X 탑승/Z 하차/정화→C+방향키Z 세 교육 연속 완료를 확인했다. 낙하 사망 후 새 체크포인트 복귀, TutorialScene 독립 Play와 기존 재시작도 정상이다. 각 씬 카메라/플레이어1·누락 참조0, 마지막 직접 Play/재시작 Console 오류/경고0. 캡처와 관찰은 ArtReference/SceneSplit이다. 전체 원형 오프닝부터 입력 완주·빌드는 하지 않았다. 두 씬 저장·Play 종료·일시정지 해제·임시 입력 복구를 마쳤고 Storyscene을 저장된 clean 상태로 열어 두었다. 아래 과거 단일 씬/dirty 유지 기록보다 이 결과가 최신이다.

## 2026-10-20 최신 — 마지막 낮은 돌 충돌과 착지 기반 바다 연출

사용자가 오른 강둑 앞 낮은 돌을 못 밟는다고 보고했고, 맵을 정리하며 물체를 삭제한 영향 같다고 설명했다. 현재 그림의 해당 부분X74.7~79.1에는 충돌이 없고 기존FarShore Polygon은X79.62부터다. 그림·기존강둑Polygon·현재삭제Stone_5를보존하고 FarShore/Art/ArrivalPlatform에solid BoxCollider2D를추가했다. 월드X74.6~79,윗면Y-1.5,아랫면-3.15다. 자식WaterChangeOnLanding은worldcenter(76.8,-.8),trigger4.15×1.5이며PlayerEventTrigger의RequireGrounded=true/LandingSurface=해당solid다. OnPlayerEntered→River.BeginTreeWithering 한 번이며,기존WaterSurface2D의X80자동조건은false로바꿨다. 태양2초→대기3초→물/반사시듦2초와나레이션연결은유지했다.

PlayerEventTrigger는기본false인선택착지조건과OnTriggerStay재평가를추가했다. 접지조건실패때HasTriggered를소모하지않고,현재방문중중복호출을막는다. 기존일반진입트리거는이옵션을사용하지않는다. 실제RightArrow+Z로마지막Stone4(71.12,-.66)에서기존낙수사망을재현했고,수정후74.60,-1.26에서착지/트리거시작을관찰했다. 입력을해제하고6.992096게임초더기다린최종(74.77,-1.16)에서도grounded=true,water/sun1로안정유지와변화완료를확인했다. 별도공중지역진입(76.8,-.37)은grounded=false/trigger=false/sun0/tree0이고,내려착지(76.8,-1.18)후trigger=true였다.

실제GameView01~06과원본관찰은ArtReference/OpeningWater/ArrivalLedgeFix에있다. 최초프리뷰입력라우팅오류때이동입력이남아촬영구간을벗어난관찰도로그에보존했고,라우팅수정뒤의안정착지/물완료캡처로교체했다. 전체오프닝입력완주/자동테스트실행은아니다. Play종료·임시키보드제거·입력설정복원·body.simulated=true·isPaused=false·runInBackground=false를확인했다. 기존미저장씬편집을저장하거나되돌리지않아Storyscene은dirty다. 기존X80시작기록보다이번착지지점연결이최신이다.

## 2026-10-20 최신 — 중간 발판 짝·산 배경 낙서3종

사용자 ‘좋다 다음가자’로 직전 태양꽃 머리/몸통/접촉 오염3종이 승인됐다. 이번 묶음은 WaterDoodle_v1/Stone_Mid_Doodle_v1.png, Stone_Mid_Healthy_Doodle_v1.png, Mountains_Doodle_v1.png다. 현재 live Storyscene의 Stone_2/Art 및 HealthyReflection, MountainBackdrop의 Sprite3개만 교체했다. 원본 PNG는 유지한다. 현재 삭제된 Stone_5는 복원하지 않았다. Stone_3 높은 발판·숲·오염태양·물 텍스처 등은 이번에 바꾸지 않았다.

적용 전후 기존 모든 Transform, Collider, WaterSurface2D, ParallaxLayer 직렬화 값이 일치한다. 중간발판 두 PNG는1254², 산은1915×821(원본1916×821보다 가로1px짧음)이며 center pivot/PPU100을 사용한다. 산의0.0522% 폭차 때문에 사용자 Transform을 조정하지 않았다. 실제 Game View Wide(66,0/ortho8), Close(61.5,.2/ortho5)를 같은 구도로 전후 비교했고, BeginTreeWithering 직접 호출 뒤7.003263게임초에 tree/sun1 및 새 중간발판 푸른 풀→마른 풀·어두운 반사를 확인했다. 오른쪽(80,0)으로 카메라를 옮긴 추가 캡처에서 산을 확인했다. 우측 끝의 회색 배경은 기존 연결 튜토리얼이다. X80 실제 입력 도달/착지 완주 검증은 아니다.

새3PNG와 생성 프롬프트·01~06 실제 화면은 ArtReference/OpeningWater/WaterDoodle_Batch3에 기록했다. 현재3종 화면 사용자 확인 대기다. 작업 전부터 있던 사용자 미저장 변경을 보존하려고 Storyscene은 저장하지 않고 dirty를 유지했다. Play종료·isPaused=false·body.simulated=true·runInBackground=false를 복구했다. 현재 컴파일 실패·Console groundTruth오류는0이다. 자세한 설정은 Docs/WaterSequence_PPT4-7_Setup.md 최상단을 따른다.

## 2026-10-20 최신 — 교체 중인 스토리 애셋에 맞는 타일셋 시안

사용자는 Storyscene 지형을 즉시 꾸미는 요청을 정정해, 교체 중인 새 스토리 애셋에 어울리는 타일셋을 만들고 싶다고 했다. 실제 live Sprite 참조와 현재 Game View, 새 강둑/낮은 발판/평지/태양꽃 PNG를 확인했다. 큰 크림색 돌·연보라색 넓은 사선 채색·굵은 검정 윤곽·황금 풀의 첫 조립 시안은 ArtReference/TilesetStyle/01_Story_Tileset_Concept_v1.png이며 입력과 내장 imagegen 프롬프트는 같은 폴더 GenerationPrompt.txt다. 그림체 검토용으로, 아직 분할된 타일 애셋·RuleTile·팔레트 완성이나 씬 적용을 뜻하지 않는다. Storyscene의 사용자 미저장 편집과 재생 상태를 바꾸지 않았다. 다음 실제 제작 때에는 기존0.5U Grid 규격과 스킨/충돌 분리를 유지한다.

## 2026-10-20 최신 — 태양꽃 머리·몸통과 접촉 사망 오염3종

새 PNG는 Assets/Ember/Art/Storybook/DoodleMechanics_v1에 있다. SunFlower.prefab의 Body/Petals/wiltedSprite/bloomingSprite를 새 머리·몸통으로 연결했다. Head1254²/PPU500/pivot(.17,.5), Body1024×1536/PPU600/pivot(.52,.025)로 기존 회전축과 위치를 유지한다. 새 오염 그림은 붉은 둥근 가시 덩어리3개이며 두 번째 교육의 기존 Pollution12셀/solid벽에 맞춰 표시한다. 기존 벽에는 접촉 사망 기능이 없어 자식 ContactHazard의 별도 Box trigger+RoomHazard를 추가했고 원래 TilemapCollider는 nontrigger 그대로다. 기존 PollutionObstacle 하나가 새 그림·기존 벽·위험 트리거를 함께 정화/초기화한다.

실제 Play에서 접촉으로 IsDying=true, 실제 꽃 탄환 충돌로 IsPurified=true 및 두 Collider/그림 비활성, ResetForRespawn 뒤 새 그림·벽·위험 복구를 확인했다. Awake/발사/리스폰 뒤 새 Head 참조도 유지됐다. 새 PNG 생성은 정확히3개이며 현재 사용자 적용 화면 확인 대기다. 전체 오프닝 입력 완주/새 자동 테스트/빌드를 수행한 결과는 아니다. 적용 전후 기존 씬의 모든 Transform과 Collider 값은 같다(신규 자식2개만 추가). 현재 Storyscene의 미저장 사용자 편집 때문에 씬은 저장하지 않고 dirty 유지했다. PNG/기존 프리팹 변경은 저장했다. Play 종료·isPaused=false·player Rigidbody.simulated=true·runInBackground=false로 복구했다.

실제 Game View 비교/발사/정화/확대 캡처는 ArtReference/OpeningWater/DoodleMechanics_v1의01~06, 전체 프롬프트는 GenerationPrompts.txt, 적용값과 관찰은 MechanicDoodleApply.txt/MechanicObservations.txt다. 상세는 Docs/FireFlower_Setup.md 및 Docs/FlowerRide_Setup.md 최신 항목을 따른다. 기존 강가·평지·사용자 최신 오브젝트 배치는 보존했다.

## 2026-10-20 최신 — 낮은 발판·푸른 짝·오른 강둑, 사용자 배치 유지

사용자의 다음 차례 요청으로 낮은 발판/푸른 반사 버전/오른 강둑3PNG를 WaterDoodle_v1 폴더에 추가했다. Stone_1·Stone_4의 Art/HealthyReflection4곳, FarShore/Art1곳의 그림만 바꿨다. 원본과 같은 캔버스·pivot·PPU로 만들었으므로 표시 Transform도 바꾸지 않았다. 작업 중 사용자가 오브젝트 배치를 바꿨다고 알려 최신 live씬을 다시 읽었고 FarShore/Art(83.04,.33), 오른 나무(80.63,2.83)/반사(80.69,2.77) 등의 사용자 위치를 그대로 유지했다. 전체 씬 Transform·Collider·WaterSurface2D 설정이 교체 전후 같음을 확인했다. 실제 Play 같은 구도 비교와 내장 image_gen 프롬프트는 ArtReference/OpeningWater/WaterDoodle_Batch2, 상세는 Docs/WaterSequence_PPT4-7_Setup.md다.

이 묶음은 사용자 ‘다음 ㄱㄱ’로 다음 작업이 승인됐다. 태양꽃 머리·몸통 및 접촉하면 죽는 오염물3개는 위 최신 항목에서 생성·연결했다. 중간/높은 발판은 기존 PNG이고 현재 전체 강가 아트 교체 완료는 아니다.

기존 시듦 흐름 직접 호출 후7.002079게임초에 새 저발판의 푸른 풀→마른 반사·어두운 물빛과 진행값1을 확인했다(03_After_Withered_Play). PNG연결은 clean상태에서 저장했으나 캡처 이후 현재씬은 dirty이므로 추가 저장·되돌리기하지 않았다. 현재 컴파일 실패·Console 오류는0이다.

## 2026-10-20 최신 — 원 안 장식만 원본 유지·평지 복구·강가 낙서3종

사용자는 원 안 오브젝트가 별로였고 평지·강가는 좋았다고 정정했다. 직전 원 그림 외 전부 복원은 범위 오해였다. 원 안 나무·풀·돌은 원본 시트로 유지하되 평지4곳·경사면1곳은 마음에 들었던 낙서 PNG로 재적용하고 FlatGroundArt의 혼합 컴포넌트·재질도 복구했다. 실제 Play270도 전후는 ArtReference/OpeningWorld/FlatRepair다. Collider 값은 유지했다.

이후 강가 새 스타일은 메인 화면 산 같은 단순한 낙서풍으로 선택했다. 사용자가 첫 마른 나무를 좋다고 평가하고 애셋 작업 계속을 요청해 강둑·마른 나무·푸른 반사용 나무3개를 만들었다. Assets/Ember/Art/Storybook/WaterDoodle_v1의3PNG를 강둑1·나무2·반사원본2곳에 연결했다. 마른/건강 나무는 같은 캔버스와pivot을 쓰며 월드Transform/flip을 일치시켰다. Collider와 WaterSurface2D 설정은 적용 전후 같다. 실제 Play 동일 구도 비교와 프롬프트는 ArtReference/OpeningWater/WaterDoodle_v1, 설정은 Docs/WaterSequence_PPT4-7_Setup.md를 따른다. 다음 이미지 묶음은 이번3개 적용 화면을 확인받은 뒤 진행한다. 발판·물·오염·배경 산과 숲은 아직 기존 그림이다.

실제 Play에서 기존 시듦 연출을 직접 호출해7.000394게임초 후 태양/나무 진행값1·마른 반사·어두운 물빛을 확인했다. X80 실제 이동 검증은 아니다. 마지막 Storyscene 시작0도·clean·재생 종료이며 현재 컴파일 실패와Console 오류는0이다.02_After_HealthyReflection_Play와03_After_WitheredReflection_Play가 이번 검토 화면이다.

## 2026-10-20 최신 — 외곽부터 중앙까지 해칭 밀도·농도 그라데이션

외곽 가까운 곳은 성기고 연하게, 안쪽만 진하게 하라는 사용자 수정 요청을 반영했다. 기존 Painted 분기의 좁은 전환 띠에서 회흑색 면 전체가 켜지던 방식을 제거하고, 질감 밝기 순서로 어두운 획부터 점차 드러낸다. 농도도 경계 거리 전체에서 별도로 올린다. 텍스처의 실제 선형 밝기10/90백분위(.0117/.0730)를 확인해 기준을 잡았고, 그림 자체는 새로 만들지 않았다. Doodle_Stone/04_BoldDiagonal의 Gradient Width1.4U·Solid Core Start.84이며 최종 실제 Play 화면은 ArtReference/SkinScene/HatchingGradient/03_Gradient_Final_Play.png다. 셰이더 오류 없음과 화면을 확인했고 지형·충돌·기본 이동은 바꾸지 않았다. 세부는 Docs/SkinScene_Setup.md 최신 항목.

## 2026-10-20 최신 — 원 그림만 유지·다른 교체분 복원

사용자가 구 형태 외에는 이전 아트가 더 좋다고 피드백했다. Storyscene에서 GroundDisk_Doodle_v1과 GroundArt Transform은 유지하고 나무·풀·돌24곳, 평지4곳, CharcoalDescent1곳을 원본 그림으로 복구했다. 장식의 위치·축척은 적용 전 기록에서 복구하고 임시 DoodleTree 자식12개를 제거했다. FlatGroundArt 혼합 컴포넌트는 제거하고 StorybookUnlit 재질로 복원했다. Collider 직렬화 값은 복원 전후 같다. 장식 Order9는 플레이어 가림 방지를 위해 유지했다. 미채택 PNG·프롬프트·혼합 코드와 재질은 실험 자료로 보존한다. 이후 사용자가 강가 애셋 변경을 요청했고 스타일 질문은 답변 대기다. 반사와 시듦을 유지하려면 강둑·마른 나무·동일 나무의 푸른 반사 버전3개부터 진행하고 발판은 다음 묶음에서 마른/건강한 짝으로 만든다.

복원 후 실제 Play 시작0도·평지 연결270도·물가 기준 화면을 ArtReference/OpeningWorld/PreviousArtRestored에 저장했다. 회전 Drive와 위치 지정 시각 확인이며 전체 입력 완주는 아니다. 마지막 Storyscene은 시작0도·clean·재생 종료다. 강가 새 이미지는 아직 생성하지 않았다.

## 2026-10-20 최신 — 거친 검정 사선 채색·검정 중앙 적용

사용자가 imagegen의 굵은 사선 시안과 추가 첨부한 회흑색 낙서 면 질감을 실제 스킨에 적용하길 요청했다. 첫 procedural capsule 획은 밝은 돌 위에 검정 막대가 떠 보여 수정했다. 현재는 생성한 Hatching_Charcoal_Diagonal_v1.png를 연속 월드 UV로 입혀 회흑색 바탕과 거친 사선을 함께 올리고, 지형 깊은 곳은 검정으로 채운다. 활성 기본 Doodle_Stone.mat과04_BoldDiagonal.mat은 Style4/Use Painted Hatching=true, Texture World Size3.6U, Clean Stone Edge.26U, Gradient Width1.1U, Solid Core Start.78이다. 스킨과 예제 프리팹만 기본 재질을 참조했음을 확인하고 적용했다. 원래 재질은00_Original, 기존 펜·연필·낙서01~03도 보존했다.

실제 skin scene Play의 Game View 원본 화면 전후는 ArtReference/SkinScene/BoldDiagonal/06_Before_Original_Play.png와07_After_Textured_Play.png다. 01/02는 매끈한 절차적 사선 실험,03/04는 임시 Preview Scene 카메라 렌더이고 최종 실제 Play 결과는05/07이다. 재질·셰이더 오류 없음을 확인했고 Terrain329셀, 이번 native Play 충돌20shape이며 지형·충돌 코드는 수정하지 않았다. 전체 이동 경로를 새로 완주하거나 기기별 성능을 측정한 것은 아니다. 기존 Storyscene의 재생을 중단하지 않고 소재 작업을 진행했으며, 사용자가 재생을 끝낸 뒤 clean 상태에서 skin scene을 잠시 열어 캡처하고 원래 Storyscene clean 편집 상태로 복원했다. 세부는 Docs/SkinScene_Setup.md.

## 2026-10-20 최신 — 원형 장식 교체·앞뒤 나무2종

원·평지·경사면 적용 화면을 보여준 뒤 사용자가 다른 애셋 교체를 요청해 나무·풀·돌 묶음을 만들었다. 이어 앞쪽은 작은 덜 자란 나무로 나눠 달라는 수정 요청을 받아 Tree_Sapling_Doodle_v1.png를 추가했다. 앞쪽4곳은 가지가 적은 어린나무(이전 높이55%), 뒤쪽8곳은 옅은 다 자란 Tree_Doodle_v1.png다. 풀6곳과 돌6곳도 낙서풍 새 PNG로 바꿨다. 원본 시트는 보존했고 기존 불투명 외곽·뿌리 접점에 맞춰 그림 Transform을 보정했다. 장식 Order를9로 내려 플레이어10이 가려지지 않도록 했다. Collider 값·회전 구조·이동 수치는 유지했다.

새 아트는 Assets/Backgrounds/Storybook/RotatingWorld, 비교 화면과 내장 image_gen 프롬프트는 ArtReference/OpeningWorld/DoodleDecor_v1에 있다. 실제 Play 시작 구도와 AngleRotator.Drive35도 구도 확인이며 전체 오프닝 입력 완주 기록은 아니다. 배경 산·언덕, 물가 나무·오염·물 반사는 이번에 바꾸지 않았다. 다음 새 이미지 묶음은 수정된 적용 화면을 사용자에게 보여주고 확인받은 뒤 진행한다. 상세는 Docs/CircularOpeningWorld_Setup.md를 따른다.

## 2026-10-20 최신 — 낙서 평지·경사면 적용, 3개 단위 확인

사용자는 새 이미지 애셋3개마다 씬 적용 스크린샷을 보고 확인한 후 다음 작업으로 넘어가도록 요청했다. 현재 원·평지·경사면3개를 첫 검토 묶음으로 취급하며 사용자 확인 대기다.

원의 낙서풍을 기준으로 FlatGround_Doodle_v1.png와 PPT_Descent_Doodle_v1.png를 만들었다. Storyscene의 FlatGroundArt, LeadInGround/Art, DescendingShore/Art, FarShore/Foundation, CharcoalDescent에 연결했고 원본 PNG는 보존했다. 실제 불투명 외곽의 크기와 위치에 맞춰 새 Sprite의 표시 Transform을 보정했다. Collider 직렬화 값은 적용 전후 일치했다. 원→평지의 세로선은 StoryGroundJoinBlend 컴포넌트와 전용 Sprite 셰이더로 원 내부에 겹치는 좌측1.4U에서만 혼합해 완화했다. 윗면·원 바깥의 불투명도는 유지하므로 단순 전체 페이드에서 생길 수 있는 하늘 틈을 방지한다.

ArtReference/OpeningWorld/DoodleFlat_v1/02_Join_After_Play.png와04_Shore_After_Play.png가 이번 씬 적용 검토 화면이다. 연결부는 Play 중270도 도착 구도로, 경사면은 Play 중 해당 위치에 카메라와 캐릭터를 놓은 시각 확인이다. 전체 스토리를 입력으로 완주한 검증이 아니다. 경사면 비교 전 사진은 사용자 첨부 캡처이며 카메라 구도가 완전히 같지는 않다. 마지막 저장 씬은 시작 회전0도·정상 카메라·원형 시작 상태이고 재생은 종료했다. 현재 컴파일/전용 셰이더/Console 오류는 없다. 상세와 생성 프롬프트는 Docs/FlatGround_Setup.md 및 ArtReference/OpeningWorld/DoodleFlat_v1/GenerationPrompts.txt를 따른다.

## 2026-10-20 최신 — 원래 낙서 해칭 유지

사용자가 유성펜 획 변경을 취소했다. 03_LooseScribble의 기존 곡선·필압·끊김과 간격0.065U/굵기0.013U/잉크색(.008,.006,.012)을 복원했다. 현재 선택 방향은 이 원래 낙서 해칭이다. 셰이더 오류 없음을 확인하고 저장된 skin scene의 같은 구도를 Unity Preview Scene 카메라로 재렌더했다(ArtReference/SkinScene/ScribbleRestored). 활성 Storyscene과 미저장 상태는 유지했다. 유성펜 실험 이미지는 채택한 결과가 아니다.

## 2026-10-20 최신 — 해칭 품질 비교3종

해칭을 펜 교차선·거친 연필·곡선 낙서3종으로 만들었다. 각각의 획 함수와 재질은 Assets/Ember/LevelDesign/SkinScene/Hatching에 있으며 DoodleTerrainSkin.Fill Material로 교체한다. 기존 Doodle_Stone은 Original(0)으로 남아 있고 사용자 선택은 미정이다. ArtReference/SkinScene/HatchStyles에 전체1920×1080/동일 부분900×900 비교를 저장했다. Unity Preview Scene 카메라 렌더로 확인했고 현재 사용자가 편집·재생 중인 씬이나 미저장 상태를 바꾸지 않았다. 실제 새 입력 완주나 기기별 성능 확인 기록은 아니다. 세부는 Docs/SkinScene_Setup.md.

## 2026-10-20 최신 — 원형 지형부터 낙서풍 교체

사용자가 직전의 산 원호 추가를 취소하고 전체 스토리 아트를 메인 화면·Mountains_Doodle의 성긴 마커/펜선 스타일에 맞춰 하나씩 교체하길 요청했다. MountainArc_Doodle 배치와 실험 PNG를 제거했다. 이번에는 Storyscene의 WorldPivot/GroundArt만 GroundDisk_Doodle_v1.png로 교체했다. 사진 같은 바위 질감을 큰 회색·회보라색 붓자국과 검은 펜 윤곽으로 바꿨다. 새 그림은 원본과 같은1254×1254이고, 불투명 경계의 지름과 중심 차이는 GroundArt의 표시 Transform에서 보정했다. 원형 충돌 반지름 약6.67U·회전 구조·플레이어 설정은 유지한다. 저장 시작 회전은0도다.

실제 Play에서 교체 전후0도와 AngleRotator.Drive를 통한35도 회전을 캡처해 비교했다. 현재 컴파일 오류·Console 오류는0이었다. 캡처는 ArtReference/OpeningWorld/DoodleDisk_v1, 원본 보존 위치와 새 애셋·프롬프트는 Docs/CircularOpeningWorld_Setup.md를 따른다. 이 기록은 원 그림 한 개의 교체이며 나무·풀·배경·평지·전체 스토리 아트 교체나 전체 오프닝 입력 완주를 완료했다는 뜻이 아니다.

## 2026-10-20 최신 — 태양꽃 명칭·타일 편집 자동 반영

소형 태양꽃은 없애고, 기존 탑승 가능한 대형 꽃을 ‘태양꽃’으로 통일했다. SunFlower_Giant.prefab을 AssetDatabase로 SunFlower.prefab으로 옮겨 GUID와 Storyscene/튜토리얼/예제 연결을 유지했다. 정식 불꽃의 rideable=true를 확인했고 FireFlowerValidation의 옛 임시 작은 꽃1개는 제거했다. 공용 FireFlower/FireFlowerProjectile 코드는 정식 태양꽃 기능이므로 유지한다.

DoodleTerrainSkin이 편집 모드에서 LateUpdate/delayCall만 기다리다 갱신되지 않던 경우를 보완했다. EditorApplication.update에서 최대20회/초로 변경을 반영하고 Undo/Redo·Grid 규격·Transform 변경도 감지한다. 같은 Terrain의 중앙 공중 기둥 타일을 아래 지형에 붙이는 편집에서 수동 Rebuild 없이 외곽8→7개·접합면 선/풀0개·실제 충돌 연결, Undo8/Redo7개 복귀를 확인했다. 예시 캡처는 ArtReference/SkinScene/TileEdit/03_Connected_Automatic.png이며 확인 후 배치는 원래329셀로 되돌렸다. 사용자는 SkinRoom_01/Grid/Terrain을 편집한다. 서로 다른 Tilemap을 겹치거나 임시 생성 그림 자식을 옮기는 방식은 연결 판정 대상이 아니다.

## 2026-10-20 최신 — 중앙 해칭 그라데이션

사용자가 셀레스트 타일셋 분석 글(https://aran.ink/posts/celeste-tilesets)을 참고해 중앙을 검게 채우고 해칭 그라데이션으로 표현해 달라고 요청했다. DoodleTerrainSkin이 실제 노출 경계까지의 거리를 메시UV1에 기록하고 DoodleTerrainInfill.shader가 밝은 돌→흔들리는 사선→교차선→짙은 중앙으로 바꾼다. 셀 단위 분류 없이 연속 거리로 전환한다. Doodle_Stone.mat과 SkinRoom_Example 프리팹·skin scene을 갱신했다. 최종 Play 캡처는 ArtReference/SkinScene/Infill/03_Hatching_Final_Play.png이며329셀·18충돌shape, 셰이더 오류 없음을 확인했다. 이번 변경은 음영 표시이며 이전의 실제 점프 완주를 새로 수행한 기록으로 취급하지 않는다. 설정은 Docs/SkinScene_Setup.md.

## 2026-10-20 최신 — 기존 skin scene에 낙서 스킨 실험

사용자가 셀레스트 한 테마의 지상·공중·벽·천장 배치를 참고해 첨부 낙서 그림체로 실제 화면과 타일 배치를 요청했다. Forsaken City A의5개 방 지도 브라우저 캡처를 참고하고, 기존 skin scene에 기존 Level Workflow.CreateRoom으로 SkinRoom_01을 만들었다. 타일0.5U·36×20.25칸, Terrain329셀, 삼각 가시38셀이다. DoodleTerrainSkin이 원본 점유셀에서 연속 메시·둥근 외곽·윗면 풀을 만들며 끄면 같은 충돌의 그레이박스로 돌아간다. Ember Doodle 팔레트와 분리 PNG6개·배경 프리팹3개·SkinRoom_Example을 저장했다. 기존 동화풍 Animator와 태양꽃·가연물 기능을 사용하고 기본 이동 수치는 바꾸지 않았다. 최종 배치에서 위치를 쓰지 않고 실제 방향키/Z로4번 점프·도착, 가시 사망 후 마지막 체크포인트(16,6.35)복귀, 접촉 꽃 발사1회를 확인했다. Storyscene은 변경하지 않았다. 전체 스토리/능력 교육 재설계와 모든 X/C 경로·오염 정화·빌드는 후속이다. Docs/SkinScene_Setup.md와 ArtReference/SkinScene의 비교 캡처를 따른다.

## 2026-10-19 최신 — 강가 외형 실적용 복원

강둑과 첫 두 발판을0.5U 타일맵·ShoreKit 스킨으로 교체해 수정 전후 실제 Game View를 비교했다. 사용자는 기존 강가가 훨씬 좋다고 피드백했다. 실적용 시도의 지형·그림·카메라·반사 변경은 모두 적용 직전 저장본으로 복원했고 Storyscene GUID를 유지했다. 키트와 예제 프리팹은 실험 자료로 남긴다. 현재 강가를 새 키트로 다시 교체하는 것은 확정되지 않았다.

## 2026-10-19 최신 — 강가 타일맵·분리 낙서풍 키트

사용자는 연출과 능력 교육을 하나의 여정으로 연결하고, 강가도0.5U 그레이박스 위에 스킨을 얹으며 오브젝트는 개별 애셋으로 만들기로 했다. 첫 제작물은 Assets/Ember/Prefabs/LevelDesign/ShoreKit의 빈 방·그레이박스 예제·스킨 예제와 오브젝트10개다. 원본PNG11개와 돌 분할용복사1개, Ember Shore 팔레트·18개타일을 포함한다. TilemapVisualSkin이 Terrain과 같은218셀에 충돌 없는Art를 생성하는 것과 푸른/마른 반사 끝점을 Game View로 확인했다. 기존 Storyscene은 교체하지 않았고 내용이 원래 저장본과 동일한 것을 확인했다. 실제 사용자 레벨 배치·튜토리얼 통합·새 맵 완주는 이후 작업이다. 사용법·제한은 Docs/ShoreKit_Setup.md.

## 2026-10-19 최신 — 물가의 태양·물 변화 분리

사용자가 초반 점프 구간의 연출이 몰린다고 요청해 물속 태양 전환2게임초 → 여운3게임초 → 나레이션과 물·나무·발판 변화2게임초로 나눴다. River의 WaterSurface2D에서 Sun Transition Seconds / Hold After Sun Seconds / Tree Wither Seconds를 조절한다. 현재 태양만 전환된 동안 물은 밝고 푸른 상태를 유지한다. 나레이션은 두 번째 변화 시작에 재생하고 기존 선택 잠금false를 유지한다. 실제 Game View의 태양 완료·물 변화 중간·완료를 캡처 확인했으며 전체 오프닝 완주는 이번 확인 범위가 아니다. 상세는 Docs/WaterSequence_PPT4-7_Setup.md.

## 2026-10-19 최신 — 전체 애셋 교체 예정

사용자는 향후 전체 아트의 일관성을 맞추기 위해 현재 애셋을 대대적으로 교체할 예정이라고 밝혔다. 현재 외형·색감은 최종 확정 기준이 아니다. 기능·충돌과 외형을 분리해 교체할 수 있는 기존 그레이박스·프리팹 제작 방식을 유지한다. 최종 스타일과 교체 시점은 추후 결정한다.

## 2026-10-19 최신 — 폰트 나레이션·자동 진행·선택 잠금

- 사용자는 대사를 앞으로 교체할 예정이며 편집 가능한 폰트 사용을 요청했다. 기존 GamjaFlower-Regular로 씬의 PastNarration과 PastReflectionNarration 프리팹을 연결하고 Lines 편집을 유지했다.
- 타이핑은 기본 초당24자, 글자 완성 뒤2실제초 대기 후 자동 다음 문장/마지막 종료다. 모든 나레이션을 잠그지 않으며 Lock Player Controls로 각자 선택한다. 현재 물가 인스턴스·프리팹의 잠금false는 유지했다. 잠금on일 때는 자동으로 닫힐 때 해제한다.
- 실제 입력으로 잠금on X이동0/잠금off 타이핑 중2.021U이동, 완성 후2.002초 자동 종료, 두 문장 자동 진행·완료 이벤트1회, 일시정지 중 타이핑 정지 및 Cancel/비활성화 잠금 해제를 확인했다. 상단 배치가 흔들리지 않게 전체 문장 메시를 유지하며 글자만 공개한다. 상세와 캡처는 Docs/StorybookNarration_Setup.md. 대사 새 작성·새 씬 배치는 하지 않았다.

## 2026-10-19 최신 — 세 번째 점화 대시 교육 연결

- 사용자 요청으로 기존 Level Workflow의 `CreateRoom`을 사용해 `GameplayTutorial_03`을 만들었다. 같은 Storyscene에서 첫 X 가연물 구간→두 번째 꽃 탑승→세 번째 C·방향키+Z까지 오른쪽으로 연결하며 중간 워프·추가 페이드는 없다.
- 화면은36×20.25칸, 타일0.5U다. 이전 끝135부터 바닥을 잇고 X139.5~141에3칸 틈을 두었다. 지형·위험·배경·출발 표시는 Tilemap, Art는 이후 스킨용으로 비워 두었다. 새 사용자용 생성 툴은 추가하지 않았다.
- C링 상태에서 실제 대시 시작 성공·출발영역·방향을 기록하고 목적지에 접지해야 완료한다. 실제 두 번째 C→X→Z 정화·착지 후 위치를 쓰지 않고 세 번째 출발까지 걸었다. 순차 C→오른쪽+Z와 동시 C+Z로 완료하고, 일반 X 대시는 완료되지 않는 것과 낙하 후 출발 체크포인트(135.7,0.85) 재시도를 확인했다.
- Game View 캡처에서 가려진 링을 발견해 입자·폭발선 정렬을 수정하고 재캡처했다. 재사용 프리팹은 `Assets/Ember/Prefabs/Tutorial/IgnitionDashTutorial.prefab`, 설정·검증·캡처 안내는 `Docs/IgnitionDash_Setup.md`다. 전체 오프닝 처음부터 입력 완주·빌드·별도 앰버 탄생 연출은 확인 완료 범위가 아니다. 아래 과거의 C+Z 후속 기록보다 이 항목을 우선한다.

## 2026-10-18 물가 구간 연결

사용자 요청으로 같은 Storyscene의 StoryShoreSection에 짧은 평지→내리막→강과 산 배경을 배치했다. 원형 구간 이후 화면 밖 페이드로 이 구간 시작점에 이동하고, 걷는 동안 StorySectionCamera가 PPT 4번 같은 물가 구도를 드러낸다. 강 건너기 발판·태양꽃 배치·과거 반사·탄생 연출은 아직이다. 배치 설명은 Docs/StoryShoreSection_Setup.md. 플레이 검증은 아직 수행하지 않았다.

## 2026-10-18 최신 태양꽃 배치 결정

사용자는 원본 PPT 3번 슬라이드의 태양꽃 기믹을 평지가 나온 뒤 강을 건너는 구간에서 소개하기로 변경했다. 원형 구간의 C 꽃 튜토리얼은 후순위 구간으로 이동한다. 최신 큰 흐름은 정상 태양 → 오염된 태양과 평지 진입 → 평지에서 강 건너기와 태양꽃 기믹 → 과거 반사 → 앰버 탄생 → 산 공개다. 강 건너기 구간 안에서 꽃과 점프를 소개하는 세부 순서는 아직 확정하지 않았다. 기존 FireFlower의 C/접촉 활성화·고정 방향 발사·꽃 불꽃 전용 오염 정화 규칙은 유지한다. 이번 변경은 기획 기록이며 씬 배치나 코드 진행 순서를 변경한 것은 아니다.

## 2026-10-18 최신 스토리 연결

사용자는 스토리씬을 하나로 유지하기로 결정했다. 오른쪽 화면 밖 이동 → 페이드아웃 → 같은 Storyscene의 NextStoryStart/NextCameraPose로 이동 → 페이드인으로 이어간다. story2 씬 로드는 사용하지 않는다. 두 마커는 StoryContinuation 아래에 있으며 사용자 배치로 조정한다. 오염된 태양은 등장 이후 화면 오른쪽 위를 유지한다. 코드·씬 연결은 추가했으며 실제 플레이 전환 확인은 아직이다.

작성일: 2026-10-16 (한국 시간)

사용자는 이 문서를 `pj_ember` 구현 세션에서 사용하려 한다. 이 문서는 구현 요청과 최신 기획을 전달하는 자료이며, 아래 기능이나 씬이 이미 구현되었다는 뜻이 아니다.

## 1. 이번에 만들 것

**첫 번째 플레이 씬인 튜토리얼 방부터 만든다.**

첫 방의 기본 흐름:

**시작 지점 → 이동·점프 → 불꽃 이동 → 태양초로 오염 제거 → 열린 길 → 출구**

기존 레벨 워크플로우와 실제 플레이어·기믹을 먼저 확인하고 재사용한다. 지형과 판정으로 끝까지 플레이 가능한 방을 먼저 완성한다. 동화풍 지형 이미지는 구조 확정 후 그 위에 입힌다. 오프닝 삽화는 이후 이 씬 앞에 연결한다.

## 2. 발표용 1차 범위

사용자는 다음 주 발표를 목표로 한다. 정확한 발표일은 전달되지 않았다.

- 튜토리얼: 별도 시작 구간.
- 일반 방 1~7: 기믹을 활용하며 산을 오른다.
- 일반 방 8: 길을 열다가 물의 신하를 우연히 구출한다. 신하에게 우미의 사정을 듣는다.
- 9번째 방: 우미 보스전. 플레이어가 이쪽으로 향할지 선택한다.

이전에 제안했던 일반 방 2개로 축소하는 안은 사용자가 채택하지 않았다. **튜토리얼 별도 + 일반 방 8개 + 우미 보스 방 1개**를 1차 목표로 이해한다. 이번 구현은 그중 첫 튜토리얼 씬이다. 1~7번 방의 개별 역할과 구체적인 지형은 아직 확정하지 않았다.

## 3. 아트와 지형 제작 방식

- 전체 분위기는 **동화풍 낙서 이미지**다.
- 불규칙한 손그림 윤곽선, 밝고 단순한 형태, 종이 위 수채화·마커 채색 느낌을 참고한다.
- 캐릭터는 최근 첨부 참고에 맞춘 **붉은 윤곽선·밝은 노란 얼굴·주황색 작은 몸** 시안을 사용한다. 최종 게임 애니메이션이 완성된 것은 아니다.
- 기존 픽셀풍·8×8 제작 기준은 과거 기록이다.
- **기존 레벨 워크플로우로 방·발판·충돌·카메라 경계를 만들고, AI 지형 이미지를 같은 위치와 크기로 겹치는 방식**으로 제작한다.
- 충돌용 지형은 유지하고, 외형 적용 후 기존 지형 렌더러만 숨기는 방식을 검토한다. 게임오브젝트 전체를 꺼서 충돌까지 없애면 안 된다.
- 태양꽃/태양초, 불·로프, 가시, 정화되는 오염, 체크포인트, 출구, 움직이는 발판은 기능별 프리팹으로 재사용한다.
- 사라지거나 움직이거나 상호작용하는 오브젝트를 AI 지형 이미지에 합쳐 넣지 않는다.
- AI 이미지가 발판 폭·틈·착지면을 바꾸면 실제 충돌 구조에 맞게 보정한다.

### 준비된 자료

| 위치 | 용도 |
| --- | --- |
| `Assets/Ember/Art/Storybook/StageExtracted/` | 손그림 스테이지에서 추출·복원한 PNG 9종과 원본·설명서 |
| `ArtReference/Character/Ember_WatercolorRedOutline_Concept_v2.png` | 최신 붉은 윤곽선·수채화 캐릭터 시안 |
| `ArtReference/Character/Ember_Storybook_Concept_v1.png` | 이전 탐색 시안 |
| `ArtReference/Water/Ember_WaterReflection_Concept_v1.png` | 지형·캐릭터가 또렷이 비치고 가로 잔물결로 흔들리는 반사 예시 |
| `DriveImport/Handoff/Ember_Session_Handoff_2026-10-14/` | 이전 기획·키 아트·타이틀 구도 자료 |
| `다음 작업.md` | 이전에 작성한 인터랙티브 타이틀 작업 설명서 |

추출 PNG는 원본과 픽셀 단위로 동일한 분리가 아니고 일부 복원·재해석이 포함돼 있다. 지형은 전체 레이어 한 장이며 자동 연결 타일셋이 아니다. 구름·가시는 모음 이미지다. 불과 로프는 한 정적 이미지다. 참고용 그림을 실제 기믹·충돌·애니메이션이 구현된 프리팹으로 오해하지 않는다.

물 반사는 사용자가 원하는 이후 시각 효과다. 현재는 예시 이미지만 만들었으며 실제 Shader Graph/VFX Graph 효과는 구현하지 않았다. 첫 방이 플레이 가능해지는 일을 먼저 진행한다.

## 4. 이동 조작: 확정한 요구와 미정 사항

현재 코드는 실제 `PlayerAbilityInputRouter`, `FlamePlayerController`와 프리팹을 확인한다. 방향키·Z/X/C를 기준으로 하며 마우스 입력은 사용하지 않는다.

### 사용자가 원하는 추가 이동

- **X로 불꽃을 향해 이동하는 도중 Z로 이동을 끊고 이탈할 수 있게 한다.**
- 이탈 순간의 진행 방향과 속도를 받아, 중력으로 포물선을 그리며 특정 블록에 착지하게 한다.
- 예: 위쪽 불꽃을 향해 이동하다 중간에 Z로 빠져나와 옆 발판에 착지한다.
- 이미 불·로프 안에 도착한 뒤의 Z 탈출과, 불꽃으로 이동 중인 상태에서의 Z 이탈은 구분한다. 기존 코드가 어느 범위까지 지원하는지 확인한다.

### 아직 결정하지 않은 것

- 사용자는 벽점프 추가를 제안했다. 이번 첫 방에 필수로 넣을지, 이후 방에서 소개할지는 미정이다. 첫 방을 벽점프가 없으면 진행 불가능한 구조로 바로 확정하지 않는다.
- 일반 대시 제거와 C+Z 제거를 논의했지만, 마지막에 **C+Z가 있어도 될 것 같다**고 재고했다. 제거가 확정된 것이 아니다.
- 대시/C+Z를 임의 삭제하지 않는다. 기존 동작을 확인하고 유지한 상태에서 첫 방의 필수 동선을 설계하는 것을 구현 기본안으로 삼는다.
- 이탈 속도, 공중 방향 제어량, 속도 상한, 벽점프 반발값 등은 구현 중 조정할 값이다.

이번 첫 방은 기본 이동·점프, 불꽃 이동, 정화가 중심이다. X→Z 이탈 요구는 기존 지원 여부를 먼저 조사하고, 미지원이면 필요한 최소 구현과 짧고 안전한 연습 동선을 연결한다. 벽점프·물 반사 등 다른 신규 기능을 모두 첫 방에 몰아넣지 않는다.

## 5. 튜토리얼 구체 구성

### A. 시작 지점

- 안전한 바닥에 플레이어를 배치한다.
- 산을 오르는 진행 방향이 화면에서 읽히게 한다.
- 리스폰 기준점을 둔다.
- 첫 입력 전에는 적이나 보스를 배치하지 않는다.

### B. 이동·점프

- 낮은 단차와 짧은 틈으로 좌우 이동과 점프를 소개한다.
- 텍스트는 방향키·Z 같은 짧은 키 표시를 기본안으로 삼는다.
- 실패해도 멀리 되돌아가지 않게 구성한다.

### C. 불꽃 이동

- 진행 방향에 유효한 불 목표를 눈에 띄게 배치한다.
- 목표와 경로가 읽히도록 불빛과 기존 타깃 피드백을 사용한다.
- X로 이동해 다음 안전 지점으로 갈 수 있게 만든다.
- X→Z 중간 이탈이 가능해졌다면 별도의 짧은 동선에서 포물선 착지를 익히게 한다. 실제 플레이어 수치를 기준으로 간격을 정한다.

### D. 태양초로 첫 정화

- 길을 막는 작은 오염과, 오염 쪽을 향한 태양초를 함께 보여준다.
- 꽃 근처에서 C를 사용하면 꽃이 활성화되고 빛/불꽃이 오염을 제거하는 결과를 보여준다.
- 이 과정을 통해 **태양초를 이용해 정해진 오염을 제거할 수 있음**을 플레이로 이해하게 한다.
- 모든 불 능력이 모든 오염을 제거한다는 규칙으로 확대하지 않는다.
- 태양초·태양꽃·화염꽃·불꽃꽃이라는 호칭이 대화와 문서에 혼재한다. 이번 정화 튜토리얼은 기존 `FireFlower` 기믹을 우선 확인하며, 호칭 때문에 동일 기능을 중복 구현하지 않는다.

### E. 출구

- 정화로 열린 길이 출구로 이어진다.
- 출구는 실제 다음 방 연결 또는 명확한 튜토리얼 완료 상태를 만든다.
- 후속 씬이 없으면 임의의 본편 씬을 덮어쓰지 말고 완료 상태로 끝나게 하고 연결 대상을 명시한다.

## 6. 첫 씬 Hierarchy 기본안

아래 이름은 제안이며 기존 레벨 워크플로우의 방 구조와 컴포넌트 구성을 우선한다.

```text
TutorialScene
├─ Main Camera / CameraRig
├─ LevelRoot
│  └─ TutorialRoom
│     ├─ Geometry              기존 타일맵/블록과 충돌
│     ├─ ArtOverlay            이후 동화풍 지형 이미지
│     ├─ Background            산·하늘·구름
│     ├─ GameplayObjects
│     │  ├─ FireTargets
│     │  ├─ FireFlower
│     │  ├─ PurifiablePollution
│     │  ├─ Checkpoint
│     │  └─ Exit
│     └─ CameraBounds
├─ Player
├─ RespawnPoint
└─ TutorialUI                  필요한 최소 키 안내
```

씬 경로 예시는 `Assets/Scenes/EmberTutorial.unity`다. 실제 작업 전 같은 목적의 씬·방이 있는지 확인하고, 사용자 제작 지형을 덮어쓰지 않는 이름을 선택한다.

## 7. 이후 연결할 오프닝

오프닝은 다음 문장과 이미지를 차례로 보여준다.

1. 태양은 ‘오염’이라 불리는 물질에 서서히 물들었습니다.
2. 세상은 조금씩 빛을 잃어갔습니다.
3. 그 혼란 속에서, 작은 불꽃 앰버가 태어났습니다.
4. 앰버는 산을 오르기 시작했습니다.

그 뒤 동일 장소의 밝았던 과거와 오염된 현재가 겹쳐 보이다가 현재 플레이 화면으로 이어진다. 그 장면이 실제 앰버의 기억·능력인지 단정한 설정은 없다. 이어서 튜토리얼에서 태양초의 정화를 보여준다.

초반 텍스트는 최소화한다. 앰버가 세상을 구하거나 새 태양이 되어야 한다는 사명을 내레이션으로 확정하지 않는다. 플레이어가 오염된 태양과 작은 불꽃을 보고 스스로 목적을 추측하게 한다.

## 8. 이야기의 중심과 우미 선택

### 이야기의 중심

- 중심은 **앰버가 태어난 이유와 살아갈 의미를 찾는 여정**이다.
- 앰버는 자신의 생명도 소중하고 죽음이 두렵다. 능력이 있다는 이유만으로 희생할 의무가 생긴다는 주장을 세계관의 정답으로 취급하지 않는다.
- 물의 신하는 우미를 구하고 싶어서 앰버를 몰아붙인다. 절박함과 강요가 함께 있는 인물이다.
- 앰버가 신하를 구한 행동은 우선 길을 열려던 행동이며, 플레이어가 처음부터 영웅이 되려고 선택한 구조가 아니다.
- 우미 정화 후 앰버가 “나는 세상을 구하기 위해 태어났구나”라고 본능적으로 느낄 수 있다. 그것은 앰버가 찾은 의미이며 보편적인 희생 의무를 선언하는 결론이 아니다.

### 물의 신하와 우미

우미는 자신이 오염된 것을 알고 물의 신하들을 이끌어 산 곳곳에 정화초를 배치하고, 자신을 쓰러뜨릴 존재를 기다리고 있다. 구출된 신하는 앰버에게 우미를 쓰러뜨려 달라고 요구한다.

대사의 내용 방향:

- 신하: 왜 자신을 구했는지 묻고, 힘을 가진 자에게 그에 맞는 희생을 요구한다.
- 앰버: 여기까지 오는 동안에도 여러 번 죽을 뻔했고 우미를 상대하는 것이 두렵다고 말한다.
- 신하: 두려움을 알면서도 우미를 위해 가 달라고 강요한다.

위는 내용 방향이며 최종 대사 원고가 아니다.

### 우미에게 가는 길과 구역 탈출

기존 보스 구역에는 무한 스크롤 패럴렉스 배경을 사용한 공간이 있다. 실제 씬과 기믹 구성을 먼저 확인한다. 그 공간의 특정 지점에서 다음 연출을 넣을 계획이다.

1. 지점 도달 → 잠시 앰버 조작 정지.
2. 카메라가 아래쪽으로 이동해 우미의 방 입구를 보여줌.
3. 앰버: “저기가… 아까 말했던 우미님이 계신 곳인가?”와 같은 짧은 대사.
4. 카메라가 돌아오고 조작 재개.
5. 아래로 내려가면 우미의 방으로 진입. 계속 전진하면 이 구역을 탈출하고 산을 계속 오른다.

최신 분기 방식은 **직접 길을 선택하는 방식**이다. 이전에 논의한 Z/X 선택창은 이 흐름보다 우선하지 않는다. 아래 길은 실수로 빠지는 함정처럼 보이지 않게 입구를 인지시킨다. 무한 스크롤 배경의 반복과 실제 플레이 구역·분기 트리거 위치는 별개로 관리한다.

장기 보스 흐름은 상위 물 정령 우미 → 땅 보스 → 바람 보스 → 오염된 태양 보스를 정화하는 방향을 논의했다. 현재 1차 구현은 물 파트다. 나머지 파트와 전체 결말의 분기 규칙은 아직 구체화하지 않았다.

## 9. 첫 씬 구현 순서와 완료 기준

### 구현 순서

1. 실제 프로젝트 경로·브랜치·HEAD·Unity 버전·열린 씬을 확인한다.
2. 기존 레벨 워크플로우와 플레이어·꽃·오염·체크포인트·출구 프리팹을 조사한다.
3. 사용자 제작 씬을 보존하고 튜토리얼용 방을 준비한다.
4. 이동·점프·불꽃 이동 구간의 지형과 충돌을 만든다.
5. X 이동 중 Z 이탈의 기존 지원 여부를 확인하고 필요한 범위를 구현한다.
6. 꽃과 오염을 배치해 C 활성화 → 정화 → 길 개방을 연결한다.
7. 리스폰·카메라·최소 안내·출구를 연결한다.
8. 실제 플레이로 처음부터 끝까지 진행 가능한지 확인한다.
9. 구조가 확정되면 그림 제작용 지형 캡처와 AI 외형 적용으로 넘어간다.

### 완료 확인

- [ ] 시작 지점에서 출구까지 진행 가능
- [ ] 방향키 이동·Z 점프 안내와 동작 연결
- [ ] X 불꽃 이동과 목표 인지 가능
- [ ] X→Z 이탈 요구의 구현 상태와 미완료 범위 명시
- [ ] 꽃 C 활성화와 지정 오염 제거 동작
- [ ] 실패 시 재시도 가능하고 맵 초기화 후 상태 정상 복구
- [ ] 카메라 경계와 출구 연결 정상
- [ ] 기존 사용자 제작 지형·다른 씬의 의도치 않은 변경 없음
- [ ] 새로 확인한 플레이 결과와 남은 작업 기록

오프닝·타이틀 개편·물 반사·신하 대화·우미 갈림길은 이후 연결 작업이다. 구현 완료로 체크하지 않는다.

## 10. 다른 구현 세션에서 주의할 점

- 이 문서를 쓴 작업 폴더는 `C:\Users\kyk04\.codex\worktrees\2c95\지스타 게임`이며 확인한 HEAD는 `25fd612` detached HEAD다. 수정·미추적 파일이 있다.
- 기존 작업 중 실제 Unity가 열린 경로는 `C:\Users\kyk04\Desktop\지스타 게임`이었다. 두 폴더의 이름이 같으므로 대상 경로를 명시한다.
- GitHub를 마지막으로 확인했을 때 `origin/main`은 `260f28d`였고 화염꽃·오염·발표 자료가 추가돼 있었다. 이 값을 최신 상태로 가정하지 말고 다시 확인한다. 이 작업 폴더에 원격 변경을 병합한 것은 아니다.
- 원격 문서에는 Unity 6000.6.5f1, 꽃의 고정 방향 즉시 발사·접촉 활성화·플레이어 무피해·꽃 머리 회전으로 방향 편집·오염 타일 한 칸 정화가 기록돼 있었다. 이 작업 폴더의 오래된 조준/예고 설명과 차이가 있으므로 실제 대상 코드와 최신 문서를 대조한다.
- 기존 사용자 변경을 reset/checkout으로 덮어쓰지 않는다. 버전·패키지 변경을 임의로 되돌리지 않는다.
- `AGENTS.md` → `Docs/AI/FireProject_Handoff.md` → 이 문서 → 실제 씬·코드를 읽는다. 최신 사용자 결정과 실제 구현을 구분한다.
- 이 문서는 다른 채팅에 전달할 인수인계다. 다른 채팅에 메시지를 보내거나 GitHub에 업로드한 상태는 아니다.

## 2026-10-18 수면 표현
- Storyscene의 기존 River에 실시간 화면 반사·왜곡 셰이더와 SurfaceRipples VFX Graph를 연결했다.
- VFX Graph 17.6.0 설치, EmberWater 정렬 레이어와 Renderer2D 캡처 설정을 추가했다.
- 설정과 한계는 Docs/StoryWater_Setup.md를 따른다. 기존 물 접촉 판정은 유지했다.

## 2026-10-18 PPT 4~7 물가/과거 반사
- 기존 Storyscene에 내리막 오버레이, 충돌이 있는 돌 발판5개, 건너편 땅, Z 안내와 오염 장식을 추가했다.
- 현재·밝은 과거 배경을 짝으로 생성하고, River의 과거 반사는 플레이어 x68~78에서 점차 나타난다. 95%에서 기존 상단 나레이션 문장과 Z 닫기를 연결했다.
- SCPE에서는 CausticsRGB 텍스처만 재사용했다. VFX잔빛·수면왜곡은 현URP기반이다.
- 캡처 비교·설정·확인한 범위: Docs/WaterSequence_PPT4-7_Setup.md. 발판 전 구간 직접 플레이/빌드는 아직 확인하지 않았다.

## 2026-10-19 Level Workflow 복구
- 사용자 문의로 확인하니 유지 대상인 Assets/EmberPrototype/Editor 폴더의 편집기4개가 파일 시스템에서 삭제되어 메뉴가 사라진 상태였다. 삭제 경위는 확인하지 못했다.
- HEAD 6396fcf에 있던 EmberLevelWorkflowWindow, CameraRoomEditor, MovementPathEditor, PathMovingBlockEditor와 각 메타를 복구했다. 다른 생성 툴은 복구하지 않았다.
- Tools > Ember > Level Workflow 메뉴 실행 성공(Opened=True)을 확인하고 창을 열었다. 방 생성·씬 배치·카메라 연결 버튼은 실행하지 않았다.
- 이 도구는 맵/CameraRoom 제작용이며 수면 셰이더의 반사 경계 문제를 자동으로 수정하지는 않는다.

- 2026-10-19 복구 편집기 경고 수정: CameraRoomEditor.OnSceneGUI의 targets 반복과 MovementPathEditor.OnSceneGUI의 targets.Length 접근을 단일 target 처리로 변경했다. Inspector 일괄 편집은 유지한다. 배속2/4/8배 로그는 정상 안내다. 자동 테스트는 실행하지 않았다.

## 2026-10-19 Celeste 카메라와 오프닝 페이드 충돌 수정
- 사용자 추가 CelesteRoomCamera가 시작부터 플레이어를 따라 화면 밖 전환 조건을 막고 StorySectionCamera와 위치 제어가 겹치는 문제를 수정했다.
- ScreenExitSceneLoader는 전환까지 카메라 이동을 보류한다. 암전/물가 활성화 후 예약을 해제한다. StorySectionCamera는 유효 룸 안에서 셀레스트 카메라로 넘기고 룸 밖에서는 기존 물가 구도를 유지한다.
- 사용자 배치 룸/씬 지형은 수정하지 않았다. 세부 흐름은 Docs/ScreenExitSceneLoader_Setup.md 참고. 자동/플레이 테스트는 새로 실행하지 않았다.

## 2026-10-19 MountainBackdrop 높이와 가로 반복
- 아래로 이동하는 배경은 Follow Y를 0으로 고정했다. 비활성 ParallaxLayer를 StorySectionCamera가 대신 이동시키던 분기도 수정했다.
- 이후 배경의 오른쪽이 비는 현상은 ParallaxLayer 비활성 상태로 가로 반복까지 멈춘 것이 원인이었다. 컴포넌트를 활성화하고 Main Camera 참조, Follow X 0.95, Follow Y 0, Repeat X 켜기를 Storyscene에 저장했다.
- 현재 Play 위치에서 Game View를 수정 전후 직접 캡처해 오른쪽 배경과 산의 수면 반사가 채워지는 것을 확인했다. 캡처는 ArtReference/OpeningWater/Captures/MountainBackdrop_Before.png 및 MountainBackdrop_After.png. 전체 진행/빌드 검증은 수행하지 않았다.
- 사용자 요청: 앞으로 화면 문제는 수정 전후 캡처로 확인하고, 남은 문제가 있으면 수정과 캡처를 반복한다. 이 기준을 AGENTS.md에도 기록했다.

## 2026-10-19 PPT4~7 애셋 분리·물가 재구성
- PPT는 한 장씩 합쳐진 PNG다. 원본을 참조한 image_gen 분리·복원으로 WaterPPT 애셋을 준비하고 기존 Storyscene 물가에 적용했다. 산/숲/강둑/나무/오염/발판/수중바위/과거 나무·꽃/태양을 분리했다.
- 물가 카메라 확대와 수면 높이를 바꾸고 기존 돌5개에 낮음·중간·높음 그림을 각각 적용했다. 강둑 충돌 경계와 시작 지점은 새 그림에 맞췄다.
- 사용자가 지적한 검정박스 CharcoalFoundation은 제거했다. 비탈을 아래까지 복원한 PPT_Descent_Deep의 알파 외곽으로 물가를 자연스럽게 겹친다.
- 수중 바위·수초는 장식 프리팹이며 수면 반사와 별도로 배치한다. 프리팹9종은 Assets/Ember/Prefabs/Story/WaterPPT.
- 과거의 건강한 나무·흰꽃·정상태양은 수면 전용 숨김 레이어31에서 합성한다. 카메라/과거반사 진행을 실제 Play 지정위치 캡처로 확인했다.
- 실제 설정과 확인 범위, PPT7의 중앙 물길 구도 차이는 Docs/WaterSequence_PPT4-7_Setup.md를 따른다. 발판 전체 입력 완주/빌드/새 화염꽃 퍼즐 완료로 처리하지 않는다.

## 2026-10-19 푸른 수면 반사와 2초 시듦

- 사용자 정정으로 강은 처음부터 푸른 풍경을 비춘다. 위치에 따라 과거 그림이 처음 드러나던 설정은 Storyscene에서 끄고 Start With Past Reflection을 켰다.
- 건너편 X80에서 기존 나레이션이 시작되고 반사 속 나무2개·발판5개의 푸른 풀과 꽃이 2초 동안 현재의 마른 모습으로 바뀐다. 추가 요청으로 물에 비친 하늘·흰 물결도 같은 2초 동안 어두운 청회색이 된다. 물속 정상 태양도 오염 태양으로 바뀐다. 위쪽 실제 풍경·앰버 반사의 밝기는 유지한다.
- 발판 Low/Mid/High의 Healthy 투명 PNG3종을 WaterPPT/Platforms에 생성했다. 각 기존 Art의 HealthyReflection 자식이 반사 원본이며 충돌체는 추가하지 않는다.
- River/WaterSurface2D의 Tree Wither Start X/Seconds로 지점·시간을 조절한다. 입력 잠금과 무관하게 진행하고 pause/fade/restart에서는 멈춘다. 완료 후 뒤로 가도 반복하지 않는다.
- River/WaterSurface2D의 Darken Sky With Withering을 켰다. 종료 하늘 밝기 배율 Withered Sky Brightness는0.24, 채도 Withered Sky Saturation은0.3이다. 실제 Game View 0/1/2초 추가 캡처로 어두워지는 흐름과 셰이더 메시지0개를 확인했다.
- 실제 Play 지정위치에서 0/1/2초 Game View 캡처, 입력 잠금 중 진행, 완료 후 재진입의 1회 동작을 확인했다. 전체 입력 완주/빌드/자동 테스트 결과는 아니다. 최신 상세 설정·생성 프롬프트·확인 범위는 Docs/WaterSequence_PPT4-7_Setup.md 맨 위를 따른다.
- 추가 태양 결정: 색상만 어둡게 하는 대신 물속 정상 태양을 오염 태양의 실제 반사로2초 전환한다. Restore Present Sun With Withering=true, 전환 레이어8개다.
- 원형 구간의 페이드 이후 On Continued→PinCorruptedSunToCamera로 위쪽 태양 중심을 화면(0.86,0.84)에 고정한다. 카메라3위치와 줌5/9/8.5에서 뷰포트좌표 유지, 태양 전환0/1/2초 실제캡처와 셰이더 메시지0개를 확인했다. 전체 암전·입력 완주 검증은 아니다.

## 2026-10-19 태양꽃 반응으로 탄생·발사 불꽃 탑승 튜토리얼 — 설계

- 물 반사 연출 이후 작은 불꽃이 태양꽃과 반응해 앰버로 변하고, 바로 오염을 제거하는 튜토리얼로 연결하는 최신 논의다. 원형 구간에서 꽃을 먼저 소개하던 과거 순서보다 우선한다.
- 사용자는 대형 태양꽃 발사 직후 슬로모션과 X 키보드 안내를 보여주고 X 입력으로 그 불꽃에 자연스럽게 탑승하는 흐름을 요청했다. 탄생 접촉 반응과 이후 튜토리얼 발사는 구분하는 방향이다.
- 예시 그림은 ArtReference/OpeningWater/Concepts/EmberBirth_SunFlower_Concept_v1.png이며 PPT8의 낙서·수채화 느낌으로 생성한 3컷 시안이다. 실제 씬이나 변신 구현을 뜻하지 않는다.
- 현재 꽃탄환은 정화용이다. 기존 X의 이동 표적 추적은 있지만 꽃탄환의 탑승 대상 등록·탑승 후 이동 추종·탄환 소멸 전 안전 하차·슬로모션/X 안내는 미구현이다. 기존 배속·일시정지·사망 복구와 함께 연결해야 한다.
- 대형 태양꽃만 만든다는 확정 기록이나 대형/소형별 기능 구분은 없다. 크기별 종류를 임의로 추가하지 않는다.

## 2026-10-19 게임 튜토리얼과 일반 플레이 화면 기준 — 설계

- 시안 단계의 사용자 보완: X로 미점화 가연물을 태우면서 그 안으로 들어가는 단계가 필요했다. 아래의 꽃탄환 첫 X 교육보다 앞에 고정 가연물 X 점화·진입→Z 이탈을 넣었다. 전체 순서는 가연물 X 진입 → 대형 태양꽃 C 발사·불꽃 X 탑승·정화 → C+방향키+Z 대시다. 최신3장 시안·프롬프트·설계 메모는 ArtReference/OpeningWater/Concepts/GameplayTutorial_v2에 있다. 시안 작성 당시 코드는 이미 타는 대상만 X로 선택했고 미점화 대상 점화·진입은 구현/복원 항목이었다. 당시 작업은 이미지 설계였으며, 이후 실제 구현 상태는 아래의 첫 구간 구현 기록을 따른다.
- 사용자는 스토리 연출 뒤 실제 게임과 같은 화면 구도로 능력을 배우는 튜토리얼을 요청했다. 방향키·Z는 이미 소개한 것으로 보고 X와 C+Z를 교육한다. 마지막에 중복으로 적은X는 오타라는 답을 받았다. 불씨 조준 사격 단계를 임의로 추가하지 않는다.
- 고정 가연물 X 점화·진입을 먼저 익힌 뒤, 대형 태양꽃이 발사한 움직이는 불꽃에 X로 탑승하는 방식으로 응용한다. 발사→슬로모션·X 안내→실제X입력→탑승 확인→정상 속도 복귀 흐름을 따른다. 이후 C로 점화 링을 만들고 방향키+Z로 대시하는 조작을 가르친다.
- 사용자 제시40×22.5칸(16:9)을 검토했으며, 현재 캐릭터와 기믹의 가독성을 위해 일반 플레이 작업 기준을36×20.25칸으로 정했다. 한 칸0.5U라면18×10.125U, Orthographic Size5.0625다. 1080p에서 한 칸 약53.3px이며40칸 화면 대비 그림이 약11.1% 크게 보인다.
- 현재 씬의 이동속도5U/s는10칸/s, X범위4U는8칸이다. C+Z 대시 이동량은 코드 수치 기준 약4칸이며 이탈 관성·실제 장애물 통과는 플레이 검증 전이다. 이 화면에서 목표 불꽃과 다음 착지면을 함께 보여주는 구성을 우선한다.
- 현재 Game View는1920×1080·16:9다. 원형 오프닝·물가 감상 구도의 줌은 별도로 유지하고, 탄생 후 일반 플레이 구도로 전환하는 방식으로 연결한다. 이번 기록만으로 카메라 전환 코드·튜토리얼 지형·꽃 발사체 탑승을 구현하거나 씬을 배치한 것으로 취급하지 않는다.

## 2026-10-19 첫 게임 튜토리얼 구현과 그레이박스 전환

- 최신 사용자 결정은 플레이어·가연물 등 핵심 오브젝트를 제외한 지형·배경을 먼저 그레이박스 타일맵으로 만드는 것이다. 위의 이미지 설계 단계 기록보다 최신인 실제 작업 범위는 첫 X 점화·진입 → 방향키+Z 이탈 → 오른쪽 발판 착지까지다. 대형 태양꽃의 발사체 X 탑승·슬로모션·C+Z 교육은 후속 구간이며 아직 구현 완료로 처리하지 않는다.
- `FlamePlayerController`는 마른 미점화 가연물도 X 표적으로 선택하고, 실제 X 입력에서 거리·방향·활성·젖음·경로를 확인한 뒤 점화하고 진입한다. 미리보기만으로 점화하지 않는다. 기존 불 사이 이동과 로프·대시 fallback을 유지하며, 일반 벽이 null source 비교로 무시되던 경로 판정도 수정했다.
- `FlammableEntryTutorial`은 수면 시듦 완료 대기 후 첫 연습을 시작한다. 실제 대상 내부 진입과 성공한 Z 이탈 횟수(`FireLaunchCount`), 지정 착지 구역과 지상 상태를 확인해 완료한다. Pause·Restart·Fade 중에는 진행하지 않고 사망하면 X 단계부터 다시 시도한다. 최초 프레이밍 때 자신이 건 잠금만 해제한다.
- 첫 구간은 `Storyscene/GameplayTutorial_01`에서 이어간다. 루트 (96.5,0), 일반 카메라 Size5.0625·36×20.25칸 기준이다. 수면 시듦 나레이션 종료 후 On Began에서 기존 `StoryShoreSection`을 숨기고 회색 평지 접근 구간을 활성화한다. 카메라 중심은 접근방(87.5,2)에서 연습방(105.5,2)으로 이어진다. 전체 프리팹은 `Assets/Ember/Prefabs/Tutorial/FlammableEntryTutorial.prefab`, 핵심 장작은 `HangingKindling.prefab`이다. 교육 시작 시 기존 `ShowEmber()`로 외형을 전환하지만 태양꽃과 반응하는 별도의 탄생 연출 완성을 뜻하지 않는다.
- 지형은 `Exercise/Grid`(Scale0.5, Cell Size1)의 `Terrain` Tilemap에 그린다. 같은 오브젝트의 Static Rigidbody2D·TilemapCollider2D(Merge)·CompositeCollider2D(Polygons/Synchronous)를 사용한다. `Background`와 이후 스킨용 빈 `Art` Tilemap도 Grid 자식이며 충돌이 없다. EmberWater 정렬 순서는 Background3/Terrain4/Art5/장작9/플레이어10이고 기존 Global Light 2D에도 EmberWater를 포함해 검정 표시를 수정했다. 편집 모드에서는 Exercise를 켜둬 Terrain을 바로 칠하고 재생 Awake에서 Hide Exercise Until Begin으로 숨긴 뒤 Begin에서 켠다. 초기 AI 돌 이미지2종과 전용 미사용 RopeLine.mat는 정리했다.
- 타일·팔레트 경로는 `Assets/Ember/LevelDesign/Graybox`, 팔레트 이름은 `Ember Graybox`다. 기존 Level Workflow와 Tile Palette로 편집하며 새 생성 툴은 만들지 않는다. 시작 바닥 셀 X-36~12/상단Y-1, 오른쪽 X21~39/Y0, 아래 받침 X13~20/Y-4 기준이며 월드 윗면은 각각0/0.5/-1.5다.
- 확인한 범위: 새 그레이박스 타일맵에서 런타임 가상 키보드가 정상 입력 라우터와 물리 이동을 거쳐 X 점화·진입을 처리했다. 대상 내부(104.5,2)에서 ExitFlammable, 방향키+Z 후 오른쪽 착지(109.97,0.83)에서 Completed/FireLaunchCount1을 확인했고 36×20.25칸 카메라를 유지했다. 실제 Game View `Graybox_01_Unlit.png`, `Graybox_02_Inside.png`, `Graybox_03_Landing.png`가 캡처 폴더에 있다. 프로젝트 입력 코드를 도구에 맞춰 바꾸지는 않았다. 이 결과는 첫 X→Z 연습 구간에 한정하며 전체 오프닝 키 입력 완주·빌드·자동 테스트를 새로 완료한 것은 아니다.
- 상세 설정·확인 범위는 `Docs/GameTutorial_Setup.md`, 비교 캡처는 `ArtReference/OpeningWater/GameplayTutorial/Captures`를 따른다. 이전 문서 기록은 각 작업 당시의 상태로 보존한다.

## 2026-10-19 두 번째 대형 태양꽃 탑승 교육 — 구현

- 첫 구간 뒤 `FlowerRideTutorial.ArmTutorial()`로 대기하고 Start Point(117.5,0.85), 거리4.5U 근처에서 교육을 시작한다. 첫 착지 직후 즉시 화면을 넘기지 않는다. 두 번째 루트는(114.5,0), 방 카메라 중심은(123.5,2), Size5.0625·36×20.25칸이다. 지형은 .5U Grid의 Terrain/Background/Art/FallHazard/Pollution 독립 타일맵으로 편집한다.
- 최초 꽃 발사 방향은 Head 로컬 +X로 고정한다. 탑승 가능 탄환은 실제 X 입력으로 탑승한 뒤 방향키로 최대2게임초 조종하고 Z 또는 시간 만료로 하차한다. 하차 후에는 마지막 방향으로1게임초 직진한다. 장애물·오염과 조기 충돌하면 남은 시간보다 정화/충돌 종료·안전 이탈이 우선한다. 같은 탄환 재탑승은 불가하다. 기존 꽃 탄환은 rideable=false를 유지해 정화 전용 역할이 자동 변경되지 않게 했다.
- 새 프리팹은 `Assets/Ember/Prefabs/Tutorial/SunFlower_Giant.prefab`, `RideableFlowerFlame.prefab`, `FlowerRideTutorial.prefab`이다. Body와 Head를 분리했고 Head local(0,2.4), Head 아래 Muzzle local(1.5,0)로 배치한다. 꽃 프리팹의 Activate On Player Contact는 true, 이번 C 교육용 씬 인스턴스만 false다. 반복 시도용 One Shot=false/Cooldown 약2.5, 속도6U/게임초, 조향300도/게임초·Smooth Time0.12를 사용한다. 2초/1초는 FireFlowerProjectile의 상수다.
- 동화풍 image_gen 아트3종은 `Assets/Ember/Art/Storybook/Tutorial/SunFlower/SunFlower_Head_v1.png`, `SunFlower_Body_v1.png`, `FlowerRide_Flame_v1.png`다. 생성 원본은 `ArtReference/OpeningWater/GameplayTutorial/FlowerRide`, 프롬프트는 그 아래 Prompts에 보관한다. 핵심 꽃·탄환 외 지형과 배경은 그레이박스를 유지한다.
- 튜토리얼 발사 직후 .2×현재배속 슬로와 X 안내를 기본 최대2실제초 제공한다. 실제 탑승 카운터와 CurrentFlowerRide를 확인하면 정상 배속으로 복귀하고 남은 탑승 시간을 안내한다. GameSessionControls의 pause/배속/재시작과 FlameShotAbility 조준 시작 전 EndSessionSlowMotion으로 시간 소유권을 정리한다. 직접 자동 탑승시키지 않는다.
- 구현된 완료 판정은 실제 탑승→이탈→지정 Landing Area 지상 착지 및 PollutionObstacle.IsPurified다. 실패/완료 전 사망은 C부터 재시도하고, 학습 완료 후 MapReset으로 오염이 돌아오면 근처 C 복구 안내를 사용한다. 완료 이벤트와 현재 길의 정화 상태는 분리한다.
- 실제 정상 입력 라우터와 물리 이동으로 C 발사→0.2배 슬로→방향키+X 탑승→정상 배속→Z 하차→오염 제거 및 착지(130.12,0.84)/Completed·IsPassageClear=true를 확인했다. Z 하차 직후 잔여1초,0.12게임초 뒤0.88초였다. 열린 공간에서는 자동하차1.999999게임초/총소멸2.999999게임초, 약44.1도 조향과 하차 뒤 반대 입력에도 추가회전0도를 확인했다. float 잔여값 때문에 한 물리 틱 늦던 경계는 보정했다. 슬로 중 Pause/Resume은 timeScale0.2→0→1로 복귀했다. 실제 캡처5장은 ArtReference/OpeningWater/GameplayTutorial/FlowerRide/Captures에 보관한다. 전체 오프닝 입력 완주·빌드·자동 테스트 및 C+Z 교육·별도 탄생 연출을 완료한 것은 아니다.
- 상세 설정과 후속 확인 결과는 `Docs/FlowerRide_Setup.md`를 따른다.

## 2026-10-19 물가 바로 옆에 튜토리얼 연결

- 사용자가 수면 나레이션 화면 뒤 다른 곳으로 워프하지 않고 옆으로 이어지는 구성을 요청했다. 기존 StoryShoreSection을 끄던 On Began 이벤트와 고정 GameplayCameraPose 이동을 제거했다. 나레이션을 닫아도 물가와 반사는 남고 현재 카메라 구도가 유지된다.
- X81.5~96.5의 ApproachRoom에 걸어 들어갈 때 StorySectionCamera가 CelesteRoomCamera에 제어를 넘긴다. 기존 EndCinematic 복귀 보간으로 위치·Size5.0625 줌을0.8초에 이어받으며 고정 위치 지정이나 플레이어 ResetAt을 쓰지 않는다. 첫 연습 방은 기존 X96.5~114.5, 다음 꽃 방은114.5~132.5로 이어진다.
- FarShore의 그림 외곽을 따라 생성했던 충돌선은 작은 홈에서 걸리는 문제가 있어 평면과 걸을 수 있는 경사의9점으로 단순화했다. 강둑 끝(96.82,3) 옆 X96.5부터 첫 타일맵6칸이3→0.5높이로 내려가고 X99.5에서 높이0의 첫 X 연습 바닥으로 연결된다. 타일맵 배경의 시작도96.5로 줄였다. 핵심 오브젝트와 다음 꽃 구간은 기존 위치를 유지한다.
- Camera Blend Seconds는 현재0이라 대사 뒤 조작을 잠그지 않는다. 카메라 전환 시간은 StorySectionCamera.Gameplay Camera Blend Seconds=.8이다. 원형→물가 첫 페이드는 이 변경과 별도로 유지한다.
- 실제 확인: 물가 프레임에서 Z로 나레이션을 닫아도 카메라X 변화0, 물가 활성·조작 잠금 없음·페이드 없음이었다. 이후 오른쪽 입력만으로 X79.72→98.25(약3.98게임초, 최대 관찰step0.10U), 이어 X102.15/Y0.34의 첫 X 안내/PracticeRoom까지 이동했다. 이동 중 ResetAt·Transform 위치 설정은 쓰지 않았다. 전체 오프닝 완주와는 구분한다. 비교 캡처는 ArtReference/OpeningWater/GameplayTutorial/ContinuousShore/Captures에 보관한다.

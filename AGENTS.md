# Project Ember 작업 기준

- 2026-10-21 최신 공용 Grid·방 진입 체크포인트: Level Workflow의 `공용 Grid에 방 추가` 기본true이며 `LevelTilemaps`를 통해 WorldGeometry/Grid/Terrain·Spikes·Decorations 한 벌에 이어 그리고 Rooms/SkinRoom_01·02 아래 CameraBounds/Spawn과 기존Mechanics/Paths/DoodleBackdrop은 유지한다. 새 공용 방은 CameraBounds+Spawn만 생성하고 공용맵 전체 범위맞춤은 비활성이다. 승인된 Room_02 전체(-.05,-.12)U 이동으로 정렬해 Terrain575+Spikes32=607셀/Composite60shape 보존, 사용자오브젝트32개는 승인shift외 월드좌표변경0이다. Room_01 map.tint는셀색에곱해보존했고 활성스킨두방의Terrain표시/빈Room_02Spikes표시를통일했다. 예제SkinRoom_Example.prefab의Checkpoint_Center/Finish2개만삭제하고Spawn연결;실제씬추가체크포인트는사용자가이미삭제한상태유지다. CameraRoom.SpawnPoint가연결된방만정상플레이진입시자동저장하며동일방중간checkpoint유지/연출·잠금·일시정지중유예/기존unlinked방변경없음이다. 에디터확인용생성·재사용·병합·UndoRedo·셀색/변환보존,분리실제컴포넌트동기checkpoint15항목통과;전체직접입력완주는미실행이다. 기존소스프리팹의개별Grid는유지한다. 실제skin씬dirty유지,saveAsCopy전후백업은Assets/Scenes/Backups/skin_scene_BeforeSharedGrid_20261021.unity 및skin_scene_AfterSharedGrid_20261021.unity. 전후캡처/MigrationResult.json은ArtReference/SkinScene/SharedGrid,설정Docs/LevelEditingWorkflow.md·Docs/SkinScene_Setup.md. 앞으로공용on새방→WorldGeometry/Grid/Terrain에서연속paint→스킨자동반영,방경계별도·Spawn안전위치배치가최신제작방식이다.

- 2026-10-21 Room_02 충돌: skin scene의 Room_02/Terrain111칸이 GrayboxBackground(None)여서 통과했다. 사용자가 밝은 타일 외형을 선호하므로 동일 Sprite/색/Transform/Flags의 GrayboxLightSolid(Grid) asset을 만들고 해당111칸만 바꿨다. 공유 Background는 그대로 None이다. 두 팔레트 Ember Graybox/Ember Doodle의(0,-2)에 새 타일을 추가했다. 색으로 충돌을 판단하지 않는다. 지형·셀별색/변환/플래그 보존/씬 저장, Composite0→15shape. Play의 분리된 PhysicsScene2D에서 플레이어 BoxCollider크기/offset·중력으로1초낙하 전후 관통→접촉/착지 확인(직접입력완주는아님). 캡처 전후PNG는 픽셀동일. Edit상태로복귀, 상세 Docs/SkinScene_Setup.md·ArtReference/SkinScene/Room02CollisionFix/Verification.md.

- 2026-10-21 최신 스킨 선택: 사용자가 “사선을 약3배 굵게 하고 짧은 검정 획이 겹치는” 과거 캡처를 다시 선택했다. 당시 기록의 `02_Bold_Dense` 설정으로 Doodle_Stone/04_BoldDiagonal 재질을 복원했다. Style4/UseHatchMap0/Spacing.105U/Width.075U/CleanEdge.22U/Gradient.94U/SolidCore.68/Variation.07U이며 짧고 굵은 사선→검정 중앙이다. 아래의 채색 텍스처·Gradient1.4/SolidCore.84 결정은 이전 기록으로 이번 선택이 우선한다. 기존 저장된 skin scene420셀·사용자 배치·충돌은 변경하지 않았고 씬도 저장하지 않았다. Unity 셰이더 오류0/PreviewScene 렌더 직접 비교, 실제 Play 입력은 미실행. 캡처 ArtReference/SkinScene/DenseHatchRestored, 설정 Docs/SkinScene_Setup.md.

- 2026-10-21 최신 애니메이션 투사체/연소 VFX: 고정 AI풍 `FireOrb_Outlined_Doodle_v1.png`와 `.meta`를 프로젝트에서 제거했다. `RideableFlowerFlame.prefab`은 `Assets/Ember/VFX/Wallcoeur/Prefabs/ProjectileCore_Animated_Outlined.prefab`를 사용한다. 가져온 3×3 플립북을 짙은 외곽(Order38)·주황 덩어리(Order39)·노란 중심(Order40)으로 겹쳐 로컬 시뮬레이션한다. `FlowerFlight_Outlined` 꼬리는 Order35의 불꽃 잔불이며 연기는 꺼져 있다. 충돌·발사·탑승 수치는 그대로다. `HangingKindling`은 burningSprite를 비워 `HangingWood_Unlit` 그림을 유지하고 FireGlow의 기존 Light2D/AmbientFire_Outlined에 1.25배 크기와 0.45배 GroundFire_Outlined 밑면 효과를 더했다. 캡처 `ArtReference/WallcoeurVFX/11~14_*`는 PreviewScene 렌더이며 실제 Play 확인은 하지 않았다. 미저장 skin 씬은 dirty 유지. 상세 `Docs/WallcoeurVFX_Setup.md`, `Docs/FlowerRide_Setup.md`.

- 2026-10-21 불꽃 폭발 쉐이크: RideableFlowerFlame.prefab의 FireFlowerProjectile/Despawn Screen Shake를 켜고 지속0.12게임초·강도0.05U로 설정했다. 정상 Retire에서 기존 CelesteRoomCamera.Shake를 한 번 호출하며 같은 씬·화면 안의 활성 룸 카메라만 선택한다. 페이드·일시정지·카메라 중단·맵 리셋/씬 종료는 제외하고 더 강한 기존 쉐이크를 덮지 않는다. 컴파일/프리팹 직렬화 확인 범위이며 실제 Play 체감 확인은 하지 않았다. skin 씬 dirty 유지, 설정은 Docs/FlowerRide_Setup.md.

- 2026-10-21 WALLCOEUR VFX 적용: 사용자가 새VFX팩을 탑승 불꽃·환경·연출에 모두 쓰도록 승인했다. Assets/Ember/VFX/Wallcoeur에 URP Alpha 재질2개와 FlowerFlight_Outlined/AmbientFire_Outlined/AmbientSmoke_Outlined/FlameBurst_Outlined/GroundFire_Outlined 프리팹5개를 만들었다. 원본팩·3×3텍스처는 유지한다. 기존RideableFlowerFlame에 시각 잔불 자식과 Burst 소멸참조, HangingKindling의 기존FireGlow/Light2D 아래 환경효과를 연결했다. StoryFlowerEnergySequence는 에너지 이동/도착, EmberBirthSequence는 변신 순간 한 번Burst를 재생하며 기존 타이밍/입력/물리/가속/잠금은 유지한다. 두씬참조 저장 중 발견한 기존Head/energyOrigin의GameObject→Transform형식불일치는 같은Head.transform으로 고쳤고 실제Transform수치는 유지했다. 컴파일·정적검토·에디터파티클렌더캡처는 ArtReference/WallcoeurVFX이며 실제Play완주는 아니다. skin씬dirty유지, 상세 Docs/WallcoeurVFX_Setup.md.

- 2026-10-21 Level Workflow 스킨룸 통합: 기존 EmberLevelWorkflowWindow에 낙서 스킨룸 생성·선택 방의 스킨 연결/복구·그레이박스 표시 전환·Terrain/Spikes+팔레트 바로 선택을 추가했다. UI 기본18×10.125U/0.5U=36×20.25칸이며 SkinRoom_Example의 최신 스킨 스타일을 신규 컴포넌트에만 복사한다. 기존4인자CreateRoom은 회색 방, CreateSkinRoom은 빈 지형에 스킨을 연결한다. 기존 사용자 재질/해칭·타일/충돌/기믹 배치를 유지하며, 소스는 유일한Terrain 우선·가시는 필요할 때만 같은레이어Terrain+5 이상으로 정렬한다. OpenBottomY는 새 방 하단 기준으로 초기화하고 생성/연결/표시변경Undo를 기록한다. 기존SkinRoom_01은 이미 올바른Source/스타일을 사용하므로 씬을 재배치/저장하지 않았고 dirty를 유지했다. 컴파일·정적 검토·제작창캡처 범위이며 새 방 생성/UndoRedo/브러시 입력 실행은 이번에 하지 않았다. 상세 Docs/LevelEditingWorkflow.md·Docs/SkinScene_Setup.md.

- 2026-10-21 태양꽃 불꽃 탑승 가속·소멸 파티클: FireFlowerProjectile은 탑승 전 발사 속도를 유지하며 X 탑승 후 시작0.6배→최대1.2배/가속4.5U·초⁻²를 적용한다. 꽃의 Projectile Speed6 기준3.6→7.2U/게임초, 약.8초다. RideableFlowerFlame.prefab의 Ride Acceleration에서 켜기·시작/최대 배율·가속을 조절한다. 하차 후 그때 속도/방향으로 기존1초 직진, 최대탑승2초·C 발사/X 탑승·조향은 유지한다. 충돌/사거리/수명 종료 Retire에서만 분리된 FlowerFlameDespawn 파티클28개를 같은 씬/정렬에 생성하고 자동 삭제한다. MapReset/OnDisable/씬 종료에는 생성하지 않는다. 전용 프리팹/Alpha 재질은 기존 Epic Toon FX 불씨 텍스처를 재사용하며 새 PNG는 없다. 컴파일·정적 검토와 TutorialScene 편집기 렌더 미리보기는 ArtReference/FlowerRide/AccelerationDespawn에 있고 실제 입력 Play 재완주는 하지 않았다. 현재 skin 씬 dirty 상태를 보존했다. 상세 Docs/FlowerRide_Setup.md.

- 2026-10-21 C+X 사격 제거 정정: 사용자는 TutorialScene에서 예전 C+X 대신 X로 불에 타는 조작을 원한다고 재확인했다. X 탑승 우선 처리는 있었지만 공통 PlayerAbilityInputRouter에 C 점화 링→X 누르기 조준/놓기 사격이 남아 있어 해당 입력 연결을 제거했다. 링 안 X도 RequestFireAbsorb를 사용하며 C가 비운 목표 미리보기를 현재 방향으로 다시 구한다. 첫 교육은 X 점화·진입/Z 이탈, 두 번째는 C 꽃 발사→X 탑승→Z 하차, 세 번째는 C+Z 대시다. 꽃은 C 또는 다른 꽃 탄환으로만 발사하며 자동 접촉 발사로 바꾸지 않는다. 기존 FlameShot 직렬화 컴포넌트·필드·내부 API는 참조 보존을 위해 남겨두지만 플레이어 입력에서 호출하지 않는다. 씬/프리팹 배치 변경 없이 안내 문구도 구분했다. 이번 확인은 컴파일·정적 검토이며 실제 Play 재완주는 별도다.

- 2026-10-21 태양꽃 발사 조건 최신 정정: 사용자는 C 입력 또는 다른 태양꽃 탄환 명중으로만 꽃이 발사하도록 명시했다. 플레이어 직접 접촉·계속 겹침은 발사 조건이 아니다. FireFlower의 접촉 콜백/activateOnPlayerContact 옵션을 제거하고, FireFlowerProjectile의 기존 CircleCast가 꽃 트리거도 명중 처리해 기존 TryActivateCore로 연쇄 활성화한다. 발사한 자기 꽃은 제외하며 명중 탄환은 쿨다운 등으로 활성화가 거절돼도 종료해 접촉 재시도 발사를 막는다. Head 로컬+X·쿨다운·OneShot·탑승2초/하차후1초·스토리 에너지 전용 꽃의 fireProjectileOnActivation=false/onActivated는 유지한다. 아래 접촉 활성화true 기록보다 이 결정이 최신이다. 이번 변경은 컴파일/정적 검토 범위이며 실제 연쇄 Play 확인은 별도다. 설정은 Docs/FlowerRide_Setup.md.

- 2026-10-21 Storyscene 재조립: 기존 기능을 새로 만들지 않고 만화→어두운 길→정령 구출/대화→강가 기억→시든 태양꽃 에너지 전달→TutorialScene 순으로 연결한다. `Storyscene.unity`의 `StoryShoreSection`에는 강가·발판·반사·기억 이벤트·꽃이 이미 모여 있으므로 이를 한 블록으로 재사용한다. 배치 좌표와 충돌/카메라/이벤트 참조를 보존한다. Unity Game View 연속 진행 캡처는 아직 없고, 지도는 `Docs/StoryAssembly_Rebuild.md`를 따른다.

- 2026-10-21 **최신 Storyscene 오프닝 방향**: 사용자는 원형을 도는 대신 Global Light를 끄고 작은 불꽃이 길을 걸어 오염에 붙잡힌 정령을 구한 뒤, 세계가 변한 이유를 듣고 산으로 향하도록 바꾸기로 했다. STORY02 최신 흐름은 만화→어두운 길→정령 구출/대화→기존 강 과거 장면→시든 태양꽃 C/에너지 전달→TutorialScene이다. 앰버의 첫 목표는 태양이 되는 것이 아니라 자기 정체와 세계 변화의 답을 찾는 것이다. Storyscene에서 Global Light 2D를 비활성화하고 `OpeningComicSequence`가 Point Light를 플레이어에 붙여 런타임 생성한다(강도1.25, 색상(1,.48,.16), inner1.2/out6.5). 만화 뒤 `StorySectionCamera`가 시작점부터 따라온다. 회전 없이 시작하도록 `RotatingFlatGroundEntrance.beginAtJoinPose=true`; 출발 평지는 scaleX1.2366, `FlatGroundJoin` Z 회전9.9897도로 월드 (0,-1.33)에서 기존 강둑 경사 (34.1,4.68)까지 잇는다. `CircularOpeningWorld`는 OpeningComic과 연결하지 않고 “아무거나” Circle 큐도 비활성화했다. 화면 이탈 시 NextStoryStart로 워프하던 `ScreenExitSceneLoader`도 비활성화해 같은 씬 안에서 걷는다. `StorySpirit_Doodle_v1.png`를 Guide_NPC에 연결하고 기존 `07_CorruptionVines.png`를 자식 `Corruption Vines`로 배치했다. 대화가 실제 시작하면 `StoryNarrationCue.rescueVisuals`가 오염 그림을 끈다. 정령 대사는 태양 오염으로 정령과 세상이 빛을 잃었다는 사실과 산 정상 빛의 근원에 답이 있을 수 있다는 목적을 전한다. Stone_3/RiverMemorySequence, C 태양꽃, 꽃 에너지, TutorialScene 페이드/로드는 유지했다. 변경 전 씬 백업은 `Assets/Scenes/Backups/Storyscene_BeforeSpiritRescueOpening_20261020.unity`. 코드와 씬 직렬화 연결은 저장했지만 실제 Unity Play·Game View는 아직 확인하지 않았다. 설정은 `Docs/StoryFlow_Setup.md`, 최신 체크리스트는 `TASKS.md`.

- 2026-10-20 이전 NPC 대화 UI 배치(정령 구출 대화가 대체): `StoryNarrationCue.Presentation=Dialogue`는 Celeste풍 하단 패널·이름/초상화·글자 출력·수동 Z 진행을 사용한다. 현재 `Storyscene/Guide_NPC`는 여섯 줄 정령 구조 대화이며, 상세 설정은 `Docs/StoryFlow_Setup.md` 및 `Docs/StorybookNarration_Setup.md`를 따른다.

- 2026-10-20 오프닝 태양 궤도 숨김 기록(이후 정령 구출 오프닝으로 대체됨): 사용자가 플레이어 주위를 돌던 오프닝 태양을 끄기로 했다. `OpeningSunTransition.showOrbitingSunsDuringOpening=false`이며 Storyscene 시작 시 정상·오염 태양을 모두 숨긴다. 기존 페이드 지점의 `PinCorruptedSunToCamera`는 유지되어 오염 태양을 화면 우측 상단에 고정한다. 최신 오프닝 방향과 구현 상태는 위 2026-10-21 항목을 따른다.

- 2026-10-20 이전 Storyscene 플로우 배치 기록(정령 구출 오프닝으로 대체됨): 당시 STORY02는 오프닝 3컷 만화 → 원형 이동/“아무거나” → 평지 안내자 → Stone_3 강 기억 → 건너편 C 태양꽃/에너지 이동 → TutorialScene 전환 순서였다. 이전 배치의 상세 기록은 아래 항목에 남기며, 현재 재생 흐름은 위 2026-10-21 방향을 따른다.

- 2026-10-20 최신 태양 아트 통일: 모든 정상 태양은 `Assets/Ember/Art/Storybook/SkinScene/Backdrop/Sun_Doodle.png`, 오염 상태는 `Assets/Ember/Art/Storybook/WaterDoodle_v1/CorruptedSun_Doodle_v1.png`로 통일했다. Storyscene의 NormalSun과 두 과거 반사 소스만 바꿨다. Sun_Doodle의 760 PPU를 보정해 NormalSun 월드 지름을 CorruptedSun과 맞추고, 반사 소스는 이전 월드 크기를 유지했다. SkinScene과 `sun.prefab`은 이미 같은 스프라이트를 사용하고 있었고 태양꽃/불꽃은 기믹 애셋이므로 변경하지 않았다. 미사용 `Assets/Backgrounds/Storybook/04_CorruptedSun_v1.png`와 생성 프롬프트는 2,085개 에셋 참조 확인 후 삭제했다. `03_Sun.png`, `PPT_HealthySun.png`, `PPT_CorruptedSun.png`는 백업 씬이 참조해 보존했다. 9개 일반 씬·LevelDesign 프리팹 검사에서 구 정상 태양 참조0, Storyscene 깨끗함, 1,280×720 에디터 카메라 캡처 `Assets/ArtReference/SunCleanup/01_SunUnified.png`. Play 중이던 TutorialScene은 저장을 위해 종료되어 Editor가 Storyscene 비재생 상태로 돌아왔다. 상세는 `Docs/OpeningSunTransition_Setup.md`·`Docs/CorruptedSunDoodle_Setup.md`.

- 2026-10-20 최신 개발 태스크·탄생 연출: 사용자 요청으로 TASKS.md를 최신 실행표로 갱신하고 첫 작업 DEV01(앰버 탄생→능력 교육)을 구현했다. 이전 TASKS는 Docs/Archive/Technical/TASKS_Before_2026-10-20.md에 보관했다. TutorialSceneEntry의 선택 참조 EmberBirthSequence가 작은 불꽃으로 들어오는 페이드를 기다리고 꽃 공명.9초→빛 이동.85초→변신.55초→앰버 여운.9초→카메라 복귀.55초 후 첫 X 교육을 시작한다. 기존 꽃 그림을 재사용한 EmberBirth/BirthFlowerVisual은 FireFlower·Collider 없는 시각 전용이며 기존 두 번째 교육용 꽃의 발사와 분리된다. 일반 사망에는 재생하지 않고 R 씬 재시작 때 재생한다. SkipBirth는 정리 후 교육을 시작하며 OnDisable은 콜백 없이 잠금/카메라/효과만 정리한다. 직접 시작과 Storyscene 물가/나레이션/페이드 경유, 연출 중 입력 차단/Pause/R/Skip/비활성화, 종료 뒤 실제 X→Z 착지를 확인했다. 기존 지형·충돌 값·Storyscene2+3+2초는 유지했고 새 PNG는 없다. 백업은 Assets/Scenes/Backups/TutorialScene_BeforeEmberBirth_20261020.unity, 설정은 Docs/EmberBirth_Setup.md, 최종 캡처는 ArtReference/EmberBirth/03~07·09~10이다. 전체 원형부터 입력 완주·빌드·이번2/3교육 재완주는 미확인이다. 아래 탄생 미완료 기록보다 이번 결과를 우선한다.

- 2026-10-20 최신 씬 분리·계정 인수인계: 사용자가 씬 분리의 실제 구현과 인수인계를 승인했다. `Storyscene`에는 원형→평지→강가/반사/나레이션을 유지하고 기존 세 능력 교육과 전용 player/camera/light는 `Assets/Scenes/TutorialScene.unity`로 옮겼다. PastNarration.onFinished→StoryTutorialSceneTransition.BeginTransition은 물 변화 완료를 기다려 기존 SceneFadeTransition으로 .65초 아웃→Single 로드→.4초 인한다. 새 씬은 TutorialSceneEntry로 EntryCheckpoint(102.11,.35), PracticeRoom(105.51,2), ortho5.0625에서 시작하며 01→02→03은 같은 씬 안에서 직접 이어진다. Build 목록 등록·참조 분리·저장 완료. 이번 명시적 승인과 결과는 아래 과거의 단일 Storyscene/강가 직접 연결 기준보다 우선한다. 분리 전 미저장 live 배치도 `Assets/Scenes/Backups/Storyscene_BeforeSceneSplit_20261020.unity`에 저장했다. 실제 낮은 돌 착지→연출/자동 나레이션→페이드/로드, 새 씬 좌우 입력·X→꽃 C/X/Z 정화→C+Z 세 구간 연속 완료, 낙하 사망/새 체크포인트 복귀, 독립 씬 시작/기존 재시작을 확인했다. 전체 원형부터 입력 완주·빌드는 미확인이다. 캡처는 `ArtReference/SceneSplit`, 다음 계정이 가장 먼저 읽을 종합 문서는 `Assets/Documentation/ACCOUNT_HANDOFF_2026-10-20.md`다. 최신 아트 채택/미채택·사용자 배치·남은 작업·Git 미커밋 보존도 이 문서를 따른다.

- 2026-10-20 스토리 포인트 의미 정정: 사용자는 기존에 올라가면 스토리가 나오던 시작 지점 자체를 오른 강둑 앞 낮은 돌로 옮기려는 뜻이라고 명확히 했다. 기존 태양 반사→물/시듦·나레이션→다음 튜토리얼 연결을 그대로 옮기는 범위다. 현재 ArrivalPlatform 착지→River.BeginTreeWithering→기존 onTreeWitheringStarted/PastNarration→onFinished/GameplayTutorial_01.QueueBegin 연결을 확인했고, 옛X80자동조건은false이며 별도옛스토리트리거는없다. 이 정정만으로 기존2+3+2초 연출이나 나레이션 타이밍을 임의 변경하지 않는다.

- 2026-10-20 마지막 낮은 돌 착지·바다 변화 시작점 수정: 사용자가 오른 강둑 그림의 왼쪽 낮은 돌에 못 올라간다고 했고 맵 정리 중 물체 삭제가 원인 같다고 설명했다. 실제로 그 그림은X74.7~79.1에 있으나 기존 FarShore Polygon은X79.62부터여서 충돌이 없었다. 기존 그림/삭제한Stone_5/강둑Polygon을 유지하고 FarShore/Art 아래 ArrivalPlatform solidBox(X74.6~79,topY-1.5,bottom-3.15)를 추가했다. 자식 WaterChangeOnLanding의 PlayerEventTrigger는 Require Grounded=true, Landing Surface=해당Box이며 River.BeginTreeWithering을 한 번 호출한다. PlayerEventTrigger의 새 착지옵션 기본은false라 기존 진입 트리거는 유지된다. Water.witherTreesByPlayerPosition=false로X80 자동시작을 끄고 기존태양2초/대기3초/물2초와나레이션연결은유지했다. 실제방향키+Z로Stone4에서수정전낙수/사망,수정후낮은돌착지와약7초간안정접지·물변화끝을확인했다. 공중에서지역진입시HasTriggered=false/sun0/tree0이며착지후true도확인했다. 캡처/관찰은ArtReference/OpeningWater/ArrivalLedgeFix,상세Docs/WaterSequence_PPT4-7_Setup.md최신항목이다. 씬기존미저장편집은dirty그대로이며 Play·임시입력장치·입력라우팅·runInBackground를복구했다. 전체오프닝완주는아니다.

- 2026-10-20 강가 낙서 세 번째 묶음: 사용자 ‘좋다 다음가자’로 직전 태양꽃 Head/Body·접촉 오염3종이 승인됐다. 이어 WaterDoodle_v1의 Stone_Mid_Doodle_v1.png, Stone_Mid_Healthy_Doodle_v1.png, Mountains_Doodle_v1.png를 내장 image_gen으로 만들어 현재 Stone_2/Art·HealthyReflection 및 MountainBackdrop의 Sprite3곳만 바꿨다. 기존 모든 씬 Transform/Collider/WaterSurface2D/ParallaxLayer 직렬화 값은 그대로이며 현재 삭제된 Stone_5를 복구하지 않았다. 중간1254²/산1915×821(원본1916보다 가로1px짧음), center pivot/PPU100이다. 산 폭차0.0522%로 Transform은 보정하지 않았다. 실제 Play 같은 Wide/Close구도 비교·푸른 반사·7.003263게임초 뒤 마른 풀/어두운 물빛 전환을 확인했고 오른쪽 카메라 이동도 캡처했다. 우측 끝 회색 타일은 기존 연결 튜토리얼이다. X80 입력 도달/전체 이동 완주 검증은 아니다. 현재 high발판·숲·오염태양·물 텍스처는 기존이다. 이전 미저장 씬 편집을 보존해 dirty를 유지했다. 캡처·프롬프트는 ArtReference/OpeningWater/WaterDoodle_Batch3, 상세 Docs/WaterSequence_PPT4-7_Setup.md. 이번3종 적용 화면은 사용자 확인 대기이며 추가 새 이미지는 확인 뒤 만든다.

- 2026-10-20 타일셋 방향 추가: 사용자는 Storyscene 맵을 바로 꾸미는 대신 현재 교체 중인 새 스토리 애셋을 보고 어울리는 타일셋을 만들고 싶다고 정정했다. 새 강둑·낮은 발판·평지·태양꽃 그림을 참조한 큰 크림색 돌/연보라 넓은 채색/굵은 검정 선/황금 풀의 첫 검토 시안은 ArtReference/TilesetStyle/01_Story_Tileset_Concept_v1.png다. 아직 실제 Sprite slicing·RuleTile·팔레트나 씬 적용은 하지 않았으며 시안 부품 개수를 실제 제작 수로 확정하지 않는다. Storyscene 미저장 편집·재생 상태는 유지했다.

- 2026-10-20 태양꽃·접촉 오염 낙서3종: 사용자 ‘다음 ㄱㄱ’에 따라 DoodleMechanics_v1의 SunFlower_Head_Doodle_v1.png, SunFlower_Body_Doodle_v1.png, Pollution_Thorns_Doodle_v1.png를 생성했다. SunFlower.prefab Body/Petals 및 wiltedSprite/bloomingSprite에 새 그림을 연결하고 원본과 같은 Head/Body 캔버스·PPU·pivot을 유지했다. Storyscene 최신 live 배치의 기존 모든 Transform/Collider 값은 적용 전후 같으며 기존 Head/Muzzle도 유지했다. 두 번째 교육의 Pollution은 기존12셀·solid TilemapCollider를 보존하고 표시만 DoodleArt로 바꿨다. 해당 벽에 접촉 사망 기능이 없었으므로 별도 ContactHazard 자식(Box trigger/RoomHazard, 로컬 중심35,4/크기2.08,6.08)을 추가했다. 기존 PollutionObstacle가 그림·벽·트리거를 함께 정화/복구한다. 실제 Play에서 물리 접촉→IsDying, 실제 꽃 탄환→정화 후 렌더러/충돌 비활성, 초기화 복구, 발사/리스폰 후 새 머리 그림 유지를 확인했다. 전체 입력 튜토리얼 완주를 재검증한 것은 아니다. 프리팹/PNG는 저장했으나 기존 미저장 사용자 편집을 보존하려고 Storyscene은 dirty 상태로 남겼다. 캡처·프롬프트는 ArtReference/OpeningWater/DoodleMechanics_v1, 상세는 Docs/FlowerRide_Setup.md다. 이번3종 적용 화면 사용자 확인 대기이며 다음 이미지 묶음은 확인 뒤 진행한다.

- 2026-10-20 강가 낙서 두 번째 묶음: 사용자가 이전3개 적용 후 다음 차례를 요청해 Stone_Low_Doodle_v1.png, Stone_Low_Healthy_Doodle_v1.png, Shore_Right_Doodle_v1.png를 같은 WaterDoodle_v1 폴더에 만들었다. 기존 Stone_1/Stone_4의 Art와 HealthyReflection4곳, FarShore/Art1곳의 그림만 교체했다. 새 PNG의 캔버스·pivot·PPU가 원본과 같아 Transform을 보정하지 않았고, 전체 씬 Transform·Collider·WaterSurface2D 설정은 적용 전후 모두 일치했다. 사용자 최신 배치에서 FarShore/Art가(83.04,.33)으로, 오른 나무/반사도 이동한 것을 다시 읽어 그 위치와 미세 오프셋을 유지했다. 중간·높은 발판과 이전 나무·강둑·평지는 그대로다. 캡처/프롬프트는 ArtReference/OpeningWater/WaterDoodle_Batch2, 설정은 Docs/WaterSequence_PPT4-7_Setup.md다. 현재3개 적용 화면 확인 단계이며 다음 묶음은 사용자 요청한 태양꽃 Head/Body2개와 접촉 즉사 오염물1개다. 새 기능으로 추가하는 것이 아니라 기존 기능에 낙서 그림을 연결하며,3개마다 씬 적용 확인 규칙을 유지한다.

- 2026-10-20 최신 범위 정정·강가 낙서3종: 사용자는 별로였던 것이 원 형태 안의 나무·풀·돌이며, 평지·강가는 좋았다고 명확히 정정했다. 직전 모든 교체분 복원은 범위를 잘못 이해한 중간 작업이다. 원 안 장식은 원본 시트로 유지하고, 평지4곳·CharcoalDescent는 FlatGround_Doodle_v1/PPT_Descent_Doodle_v1로 다시 연결했다. FlatGroundArt의 StoryGroundJoinBlend와 전용 재질도 복구해270도 실제 Play 캡처로 연결부를 확인했다(ArtReference/OpeningWorld/FlatRepair). 이후 사용자 요청대로 메인 화면 산처럼 단순한 낙서풍의 강둑·마른 큰 나무·동일 나무의 건강한 반사용 그림3개를 만들었다. 첫 마른 나무는 사용자가 좋다고 평가했다. Assets/Ember/Art/Storybook/WaterDoodle_v1의 Shore_Left_Doodle_v1,Tree_Dry_Doodle_v1,Tree_Healthy_Doodle_v1을 기존 강둑1곳·나무2곳·반사원본2곳에 적용했다. 건강한 나무는 동일 캔버스·pivot·마른 나무의 월드Transform/flip으로 정렬했다. Collider와 WaterSurface2D 직렬화 설정은 그대로다. 이번3개 씬 적용 화면 확인 뒤 다음 이미지 묶음으로 진행한다. 발판·물·배경·오염 그림은 이번 교체 대상이 아니다. 상세/내장 image_gen 프롬프트는 Docs/WaterSequence_PPT4-7_Setup.md와 ArtReference/OpeningWater/WaterDoodle_v1을 따른다.

- 2026-10-20 해칭 그라데이션 최신 수정: 사용자는 외곽은 해칭이 적고 연하며 가운데로 갈수록 촘촘하고 진해지는 전환을 요청했다. Painted 분기의 급한 면 덮기를 없애고 텍스처의 어두운 획부터 경계 거리 전체에 걸쳐 드러내며 밀도·농도를 따로 증가시킨다. 기본/04 재질의 Gradient Width는1.4U, Solid Core Start는.84로 변경했다. 최종 실제 Play 화면은 ArtReference/SkinScene/HatchingGradient/03_Gradient_Final_Play.png이며 이전 BoldDiagonal07보다 이 결과가 최신이다. 지형/충돌은 변경하지 않았다.

- 2026-10-20 최신 스토리 아트 선택: 사용자는 구 형태를 제외하면 이전 아트가 더 좋다고 밝혔다. GroundDisk_Doodle_v1 원 그림만 유지하고, 최근 나무·풀·돌24개 표시 참조와 평지4곳·CharcoalDescent1곳은 기존 PNG로 복원했다. 원 위 장식 위치·축척은 적용 전 기록에서 복구하고 임시 DoodleTree 자식12개를 제거했다. FlatGroundArt는 StorybookUnlit 재질로 복구하고 StoryGroundJoinBlend 컴포넌트를 제거했다. Collider 값·원 그림과 원 Transform은 복원 전후 같았다. 플레이어 가림 방지를 위한 앞 장식 Order9는 유지한다. 새 시안 PNG와 혼합 코드·재질은 미채택 실험 자료이며 현재 씬 채택본으로 취급하지 않는다. 사용자는 이어 강가 애셋 교체를 요청했다. 반사 연출을 유지하려면 첫3개를 강둑·마른 큰 나무·동일 나무의 건강한 반사용 그림으로 만들고, 발판은 별도 묶음에서 마른/푸른 짝으로 교체한다. 강가 새 스타일 질문은 답변 대기다.

- 2026-10-20 해칭 최신 적용: 사용자가 굵은 검정 사선 시안을 채택하고, 이후 첨부한 거친 회흑색 채색 참조에 맞춰 실제 skin 지형에 적용하길 요청했다. 매끈한 capsule 사선1·2차는 최종 결과가 아니다. Hatching_Charcoal_Diagonal_v1.png를 월드좌표3.6U로 연속 샘플하여 밝은 돌 외곽→거친 사선 채색→완전 검정 중앙으로 잇는다. Doodle_Stone.mat과04_BoldDiagonal.mat은 Style4/Use Painted Hatching=true, Clean Edge.26U/Gradient1.1U/Solid Core Start.78이다. 이전 Original은00_Original, 기존01~03도 보존했다. 실제 Play 화면 전후는 ArtReference/SkinScene/BoldDiagonal/06_Before_Original_Play.png와07_After_Textured_Play.png다. 이번에는 재질만 바꿨고329셀·현재 Play 충돌20shape를 확인했다. Storyscene 강가·이동 수치는 변경하지 않았다. 상세는 Docs/SkinScene_Setup.md 최신 항목.

- 2026-10-20 애셋 확인 주기: 새 이미지 애셋3개를 만들 때마다 실제 씬에 적용된 스크린샷을 사용자에게 보여주고 확인받은 뒤 다음 묶음으로 진행한다. 원·평지·경사면 화면을 보여준 뒤 사용자가 다른 애셋 교체를 요청해 다음 묶음으로 진행했다. 나무·풀·돌 묶음 적용 중 사용자가 나무를2종으로 나누고 앞은 작은 덜 자란 나무로 바꾸도록 직접 수정 요청했다. 이 요청에 따라 어린나무 PNG를 추가했고, 다음 이미지 묶음은 수정된 씬 화면 확인 후 진행한다.

- 2026-10-20 원형 장식 낙서풍 교체: Storyscene의 SurfaceDecorations16곳을 새 나무·풀·돌로 바꾸고 DistantScenery의 나무8곳에 다 자란 나무를 옅게 재사용했다. 앞쪽 나무4곳은 Tree_Sapling_Doodle_v1.png이며 이전 나무 높이의55%로, 가지가 적은 어린나무다. 뒤쪽 Tree_Doodle_v1.png는 큰 나무이고 군집4곳은 같은 그림3개를 조합했다. 풀은 Grass_Doodle_v1.png, 돌은 Rocks_Doodle_v1.png다. 모든 PNG는 Assets/Backgrounds/Storybook/RotatingWorld에 있다. 원 위 뿌리 접점·회전 구조·Collider 값은 유지하고, 장식 정렬을 Default9로 바꿔 플레이어10을 가리지 않게 했다. 배경 산·언덕·물가 대형 나무·오염·물 반사는 이번 교체 범위가 아니다. 설정·적용 캡처·생성 프롬프트는 Docs/CircularOpeningWorld_Setup.md 및 ArtReference/OpeningWorld/DoodleDecor_v1을 따른다. 사용자 검토 전 최종 스타일 확정으로 취급하지 않는다.

- 2026-10-20 원→평지 낙서풍 연결: FlatGround_Doodle_v1.png와 PPT_Descent_Doodle_v1.png를 생성해 Storyscene의 기존 평지4곳과 CharcoalDescent1곳에 연결했다. 불투명 외곽 범위를 맞춰 그림 Transform만 보정했고 모든 Collider 직렬화 값은 적용 전후 같았다. FlatGroundArt의 StoryGroundJoinBlend는 CircleCollider2D 내부와 겹치는 좌측1.4U에서만 그림을 혼합해 세로 절단선을 완화한다. 원 밖·윗면은 알파를 유지한다. 새로운 편집 툴은 추가하지 않았다. 캡처는 ArtReference/OpeningWorld/DoodleFlat_v1이며 현재 원·평지·경사면3개 묶음은 사용자 검토 대기다. 물가 나무·오염·배경 산 등 다른 아트의 교체 완료로 취급하지 않는다. Docs/FlatGround_Setup.md를 따른다.

- 2026-10-20 해칭 최신 선택: 사용자는 유성펜처럼 굵게 바꾼 낙서 해칭을 취소하고 기존 03_LooseScribble 표현을 사용하기로 했다. 획 함수와 간격0.065U·굵기0.013U·잉크색(.008,.006,.012)을 원래대로 복원했다. 복원 미리보기는 ArtReference/SkinScene/ScribbleRestored다. 유성펜 시안은 채택하지 않는다.

- 2026-10-20 해칭 스타일3종: 사용자 요청으로 Assets/Ember/LevelDesign/SkinScene/Hatching에01_FinePen/02_RoughPencil/03_LooseScribble 재질과 각각의 HLSL 획 함수를 만들었다. DoodleTerrainSkin의 Fill Material에 넣어 교체한다. 기존 Doodle_Stone은 Hatching Style=Original(0)이며 선택 스타일을 확정하지 않았다. 전체/확대 비교는 ArtReference/SkinScene/HatchStyles에 있다. 저장된 skin scene의 Unity 카메라 미리보기 렌더이며 Play 입력 캡처와 구분한다. 실제 편집/재생 중 씬과 미저장 상태를 유지했다. 설정은 Docs/SkinScene_Setup.md.

- 2026-10-20 원형 오프닝 아트 최신 결정: 사용자는 직전의 산 원호 추가를 취소하고, 메인 화면·Mountains_Doodle처럼 굵고 불규칙한 펜선과 성긴 마커 채색이 보이는 낙서풍으로 애셋을 하나씩 교체하기로 했다. 첫 교체는 Storyscene의 WorldPivot/GroundArt에 연결한 GroundDisk_Doodle_v1.png다. 기존 실사 바위 원본은 보존하고 새 PNG의 불투명 지름·중심을 기존 그림에 맞춰 정렬했다. 회전 중심·물리 원·플레이어 높이는 유지한다. MountainArc_Doodle 오브젝트와 해당 실험 PNG는 제거했다. 시작0도와 AngleRotator.Drive로35도 진행한 Play 화면을 확인했으며 캡처는 ArtReference/OpeningWorld/DoodleDisk_v1이다. 나무·풀·돌·배경·평지의 낙서풍 교체는 아직 진행하지 않았다. 설정과 범위는 Docs/CircularOpeningWorld_Setup.md의 최신 항목을 따른다.

- 2026-10-20 태양꽃 통일·타일 편집 반영: 사용자는 소형 꽃을 없애고 불꽃 탑승 가능한 기존 대형 꽃을 ‘태양꽃’으로 통일했다. 정식 프리팹은 Assets/Ember/Prefabs/Tutorial/SunFlower.prefab이며 이전 SunFlower_Giant GUID(d2859712e7ad8ab4e9bc188513672a1a)를 유지한다. 소형 프리팹·그림은 이미 삭제되어 있었고 FireFlowerValidation 씬의 남은 임시 작은 꽃1개를 제거했다. 현재 FireFlower/FireFlowerProjectile 코드는 정식 꽃도 사용하므로 유지한다. DoodleTerrainSkin은 편집기 update에서0.05초 간격으로 변경을 반영하며 타일 이동·삭제·Undo/Redo·Grid/Transform 변경 시 스킨·해칭·풀·외곽선이 자동 재계산된다. 같은 Terrain 안에서 중앙 기둥 타일을 아래와 붙여 외곽8→7개, 접합면 선/풀0개, 충돌 연결을 확인했고 Undo/Redo도 확인했다. 캡처는 ArtReference/SkinScene/TileEdit. 확인용 타일 이동은 되돌렸고 최종 씬은329셀이다.

- 2026-10-20 skin scene 안쪽 음영 최신 결정: 사용자는 https://aran.ink/posts/celeste-tilesets 를 참고해 지형 중앙을 검게 표현하되 그라데이션·해칭 느낌을 요청했다. DoodleTerrainSkin의 경계 거리와 DoodleTerrainInfill.shader로 밝은 돌 외곽→사선→교차 해칭→짙은 중앙을 연속 전환한다. Doodle_Stone.mat은 Clean Stone Edge0.35U, Hatching Gradient Width1.1U, Spacing0.085U, Line Width0.014U다. 하단 화면 밖으로 이어지는 바닥은 Open Bottom Infill/Y0을 사용한다. 기존329셀·18물리shape를 유지하며 실제 Play 화면 비교와 셰이더 오류 없음을 확인했다. 최신 캡처는 ArtReference/SkinScene/Infill/03_Hatching_Final_Play.png이며 이전 단색 중앙 캡처보다 이 결과가 최신이다.

- 2026-10-20 낙서 스킨 실험: 사용자 요청으로 기존 Assets/Scenes/skin scene.unity에 기존 Level Workflow.CreateRoom으로 SkinRoom_01을 배치했다. 0.5U·36×20.25칸이며 Terrain329셀과 같은 점유셀에서 DoodleTerrainSkin이 충돌 없는 연속 돌 메시·0.08U 모서리·펜선·윗면 풀을 만든다. 컴포넌트를 끄면 그레이박스로 돌아간다. Ember Doodle 팔레트(지형·4방향 삼각 가시), 분리 배경3종, SkinRoom_Example 프리팹을 추가했다. 기존 동화풍 플레이어·태양꽃·장작 프리팹을 연결했으며 이동 수치는 유지했다. 실제 방향키/Z로4번 점프 완주·가시 사망 뒤(16,6.35)복귀·접촉 꽃 발사1회를 확인했다. Storyscene 강가는 수정하지 않았다. 셀레스트 Forsaken City A의5개 참고는 Berry Camp 방 지도 브라우저 캡처이며 원본 타일 전종 추출·직접 게임 플레이 촬영은 아니다. 새 툴 메뉴는 추가하지 않았고 전체 튜토리얼 재설계·모든 능력 경로 검증 완료는 아니다. 설정·캡처는 Docs/SkinScene_Setup.md.

- 2026-10-19 강가 외형 최신 피드백: 사용자는 새 타일맵 스킨을 적용한 강가보다 적용 전 강가가 훨씬 좋다고 밝혔다. 강둑·첫 두 발판을 ShoreKit으로 바꾼 실적용 시도는 전부 되돌려 Storyscene을 적용 직전 저장본으로 복원했다. 기존 자연스러운 강가 실루엣·그림·충돌·카메라·반사 연결이 현재 기준이다. ShoreKit 애셋과 예제 프리팹은 별도 실험 자료로 유지하며, 이를 현재 강가의 확정 외형으로 취급하지 않는다.

- 2026-10-19 강가 1차 키트: 강가도0.5U Terrain 타일맵으로 설계하고 Art에 스킨을 얹기로 했다. Assets/Ember/Art/Storybook/ShoreKit에 개별 원본PNG11개+돌 분할용복사1개, Assets/Ember/Prefabs/LevelDesign/ShoreKit에 오브젝트10개와 빈 방·그레이박스 예제·스킨 예제3개를 만들었다. 팔레트는 Ember Shore다. TilemapVisualSkin은 Terrain 점유셀에만 그림을 복사하며 충돌은 변경하지 않는다. 돌은4×4패턴, 나무·풀은 마른/푸른 짝과 동일pivot을 사용한다. 예제의 Terrain/Art218셀 일치와 Game View 푸른 반사/시듦 끝점을 확인했다. 기존 Storyscene 내용은 교체하지 않았으며 원래 카메라·씬 clean 상태로 복원했다. 튜토리얼과 스토리를 통합하는 전체 레벨 재설계는 후속이다. 상세 Docs/ShoreKit_Setup.md.

- 2026-10-19 최신 물가 연출 분리: X80에서 물속 태양 반사만2게임초 동안 정상→오염으로 전환하고,3게임초 여운 뒤 나레이션과 물·나무·발판 시듦을2게임초 진행한다. 이전의 모든 변화 동시2초 기록보다 우선한다. River/WaterSurface2D의 Separate Sun Before Withering=true, Past Sun Reflection Source=PastSunSource, Sun Transition Seconds=2, Hold After Sun Seconds=3, Tree Wither Seconds=2다. 기존 나레이션은 물 변화 시작에 호출하며 잠금false를 유지했다. 실제 Game View 단계별 캡처와 진행값 확인은 Docs/WaterSequence_PPT4-7_Setup.md. 원형 구간 위쪽 태양 연출은 이번 수정 대상이 아니다.

- 2026-10-19 최신 애셋 방향: 사용자는 전체 아트의 일관성을 맞추기 위해 현재 애셋을 추후 대대적으로 교체할 예정이다. 현재 개별 애셋의 외형·색감은 최종 확정으로 취급하지 않는다. 기존 그레이박스·프리팹의 기능과 충돌을 외형 교체와 분리하는 제작 방식을 이어간다. 최종 스타일과 교체 시점은 추후 결정한다.

- 2026-10-19 나레이션 최신 결정: 이미지에 대사를 굽지 않고 편집 가능한 uGUI Text와 기존 GamjaFlower-Regular 폰트를 사용한다. StorybookNarration의 Lines에서 대사를 교체한다. 기본 Typewriter 24자/실제초, Auto Advance 켜짐, Line Hold Seconds 2초이며 글자 출력 완료 뒤 다음 문장/마지막 닫기를 자동 진행한다. Lock Player Controls는 나레이션별 선택(새 기본 꺼짐)이며 현재 물가 PastNarration과 프리팹의 false를 유지했다. 켜진 나레이션은 타이핑·대기 동안 잠그고 종료/취소/비활성화 때 자신이 얻은 잠금만 해제한다. 실제 잠금 on/off 입력, 2.002초 자동 닫기, 두 문장 자동 진행, 일시정지 중 타이핑 정지와 완료 이벤트1회를 확인했다. 설정은 Docs/StorybookNarration_Setup.md. 아래 과거 Z 필수 진행 기록보다 이 결정이 최신이다.

- 2026-10-19 세 번째 C→방향키+Z 교육 구현: 사용자 요청으로 기존 Level Workflow의 CreateRoom을 사용해 GameplayTutorial_03을 중심(141.5,2),36×20.25칸으로 생성했다. 두 번째 완료→Arm 후(134,0.85)의1.5U 안으로 접근하면 시작한다. 이전 바닥 끝135부터 직접 연결하고 139.5~141의3칸 틈을 수평 대시로 건넌다. 일반 X 대시로는 완료하지 않고 출발영역에서 성공한 C링 대시 기록과 목적지 접지가 필요하다. 실제 두 번째 꽃 탑승부터 걸어서 연결, 순차/동시 C+Z 완료, X 오인 완료 방지, 낙하 후135.7체크포인트 재시도를 확인했다. 링·액션 입자·폭발선이 배경 뒤에 가리던 정렬을 플레이어 레이어+10으로 수정하고 재캡처했다. 지형은 Exercise/Grid의 Terrain/Spikes/Background/Art/LaunchMark Tilemap이며 프리팹은 Assets/Ember/Prefabs/Tutorial/IgnitionDashTutorial.prefab이다. 상세는 Docs/IgnitionDash_Setup.md. 아래 이전 기록의 C+Z 후속 표기보다 이 결과가 최신이며 별도 탄생 연출·전체 오프닝 입력 완주는 미완료다.

- 2026-10-19 물가→튜토리얼 최신 연결: 수면 시듦 나레이션 뒤 StoryShoreSection을 숨기거나 플레이어를 워프시키지 않는다. 물가 구도에서 그대로 오른쪽으로 이동해 X81.5의 ApproachRoom에 들어갈 때 CelesteRoomCamera가 위치·줌을 약0.8초로 이어받는다. 강둑 끝(96.82,3) 옆 X96.5부터 회색 타일맵 계단으로 X99.5/높이0 첫 연습 바닥에 연결했다. 회색 배경도 X96.5부터 시작하며 고정 GameplayCameraPose와1초 조작 잠금은 제거했다. FarShore 충돌선은 걸을 수 있는 경사·평면9점으로 단순화했다. 원형→물가의 기존 페이드는 별도다. 상세는 Docs/GameTutorial_Setup.md를 따른다.
- 2026-10-19 대형 태양꽃 탑승 최신 정책: 최초 발사는 꽃 Head 로컬 +X 방향으로 고정하며 플레이어를 조준하지 않는다. 탑승 가능한 탄환에 실제 X 입력으로 탄 뒤 방향키로 최대2게임초 조종하고 Z 또는 시간 만료로 하차한다. 하차 후 탄환은 마지막 방향으로1게임초 직진하되 장애물·오염에 먼저 닿으면 정화/충돌 종료와 탑승자 안전 이탈이 우선한다. 재탑승은 불가하며 기존 꽃 탄환의 rideable 기본값은 false를 유지한다. 꽃 프리팹 접촉 활성화는 true, 이번 C 교육 씬 인스턴스만 false다. 설정은 `Docs/FlowerRide_Setup.md`를 따른다.
- 2026-10-19 두 번째 꽃 탑승 교육 구현: 첫 X 구간 완료→FlowerRideTutorial.ArmTutorial 후 (117.5,0.85)에서4.5U 안으로 접근하면 시작한다. 두 번째 그레이박스 방의 카메라 중심은(123.5,2), 36×20.25칸이다. 실제 Play에서 C→0.2배 슬로→방향키+X 탑승→정상 배속→Z 하차→오염 제거·착지(130.12,0.84)/Completed를 확인했다. 열린 공간의 자동 하차는 약2.00게임초, 소멸은 총3.00게임초이며 하차 뒤 반대 입력에도 직진했다. Pause/Resume은0.2→0→1로 복귀했다. Body/Head 분리 프리팹은 SunFlower_Giant.prefab이며 상세·캡처는 Docs/FlowerRide_Setup.md를 따른다. C+Z 대시 교육·별도 탄생 연출·전체 오프닝 완주는 완료로 처리하지 않는다.
- 2026-10-19 최신 게임 튜토리얼 제작 결정: 플레이어·가연물 등 핵심 오브젝트를 제외한 지형과 배경은 먼저 그레이박스 타일맵으로 만든다. `Exercise/Grid`는 Scale (0.5,0.5,1), Cell Size (1,1,0)이며 자식 `Terrain`은 충돌용 Tilemap+Static Rigidbody2D+Composite, `Background`와 `Art`는 충돌 없는 Tilemap이다. `Art`는 이후 스킨용으로 비워둔다. 같은 EmberWater 레이어에서 Background3/Terrain4/Art5/장작9/플레이어10이며 Global Light 2D에도 이 레이어를 포함한다. 타일·팔레트는 `Assets/Ember/LevelDesign/Graybox`, 팔레트 이름은 `Ember Graybox`다. 편집 중 Exercise는 활성 상태로 칠하며 재생 Awake에서 Hide Exercise Until Begin으로 숨긴 뒤 Begin에서 활성화한다. 새 생성 툴은 추가하지 않고 기존 Level Workflow와 Tile Palette로 편집한다.
- 2026-10-19 첫 게임 튜토리얼 구현 범위: 고정된 마른 가연물을 X로 점화·진입하고 방향키+Z로 이탈해 오른쪽 발판에 착지하는 구간이다. 최신 연결은 StoryShoreSection을 유지하고 강둑→회색 계단→첫 연습으로 직접 이동한다. 실제 오른쪽 입력만으로 X79.72→98.25, 이후102.15까지 통과해 PracticeRoom/EnterFlammable을 확인했다. 이전 첫 X→Z 확인에서는 대상(104.5,2) 진입, 오른쪽(109.97,0.83) 착지/Completed/FireLaunchCount1을 확인했다. 캡처·확인 범위는 Docs/GameTutorial_Setup.md를 따른다. 전체 오프닝 입력 완주는 확인하지 않았다. 두 번째 꽃 탑승은 위 최신 구현을 따르며 C+Z·별도 탄생 연출은 후속이다.
- 2026-10-19 게임 튜토리얼 누락 수정 — 시안 단계 기록: 고정된 미점화 가연물을 X로 태우면서 그 안으로 진입하는 교육을 먼저 넣었다. 순서는 가연물 X 점화·진입→Z 이탈 → 대형 태양꽃 C 발사·불꽃 X 탑승·정화 → C+방향키+Z 대시다. 시안은 ArtReference/OpeningWater/Concepts/GameplayTutorial_v2에 있다. 당시 코드는 이미 타는 대상만 X로 선택했고 미점화 대상 점화·진입은 후속 항목이었다. 이후 첫 구간에 구현한 최신 결과는 위 항목을 따른다.
- 2026-10-19 게임 튜토리얼 화면 기준: 사용자 제시40×22.5칸을 현재 이동 수치와 비교해 일반 플레이는36×20.25칸·16:9를 작업 기준으로 정했다. 타일 한 칸0.5U 기준 화면18×10.125U, Orthographic Size5.0625다. 기존 Grid는 cellSize뿐 아니라 Transform 축척까지 확인한다. 방향키·Z는 이미 소개했고, 중복으로 적은 마지막X는 오타다. 다음 교육은 가연물 X 점화·진입 → 대형 태양꽃 발사체 X 탑승 → C+Z 대시 순서이며 새 불씨 사격 튜토리얼을 임의 추가하지 않는다. 이 항목은 설계 기준으로, 카메라 전환·튜토리얼 맵·발사체 탑승이 구현됐다는 뜻은 아니다.
- 2026-10-19 최신 탄생·튜토리얼 설계: 수면 과거가 시드는 연출 뒤 작은 불꽃과 태양꽃이 반응해 앰버가 탄생하고, 변신 직후 오염 제거 튜토리얼로 잇는다. 대형 태양꽃이 불꽃을 발사하면 슬로모션과 X 키 안내를 보여주고 플레이어의 X 입력으로 그 불꽃에 탑승하도록 한다. 대형만 제작하거나 대형/소형 기능을 분리한다는 결정은 아직 없다. 탄생 예시는 ArtReference/OpeningWater/Concepts/EmberBirth_SunFlower_Concept_v1.png. 이 항목은 최신 설계이며 탄생·꽃탄환 탑승·슬로모션 튜토리얼의 구현 완료를 뜻하지 않는다.
- 2026-10-18 최신 연출 변경: 원본 `앰버 연출 .pptx` 3번 슬라이드의 태양꽃(C 활성화·오염 정화) 튜토리얼은 원형 구간에서 소개하지 않는다. 평지 진입 뒤 강을 건너는 구간으로 옮긴다. 원형 구간은 정상 태양→오염 태양·평지 등장 흐름으로 진행한다. 꽃의 기존 능력/정화 조건은 유지한다.

- 2026-10-18 사용자 결정: 스토리는 Storyscene 하나에서 이어간다. 오른쪽 화면 밖 이동 후 페이드아웃하고 StoryContinuation/NextStoryStart 및 NextCameraPose로 이동한 뒤 페이드인한다. story2로 씬을 불러오는 흐름은 사용하지 않는다. 오염된 태양은 등장 후 카메라 오른쪽 위를 유지한다.

## 최신 작업 방식 — 2026-10-17

- 2026-10-19 사용자 확정: 강의 반사는 처음부터 푸른 풍경이다. 건너편 X80에 도착하면 나레이션과 함께 반사 속 나무·발판의 푸른 풀이 2초에 걸쳐 현재의 마른 모습으로 전환된다. 같은 진행으로 물에 비친 하늘·물빛도 어두운 청회색이 된다(Darken Sky With Withering, Withered Sky Brightness 0.24 / Saturation 0.3). 위쪽 실제 하늘은 유지한다. WaterSurface2D의 Start With Past Reflection, Tree Wither Start X/Seconds와 On Tree Withering Started를 사용한다. 과거 반사를 이동에 따라 처음 드러내던 설정은 이 씬에서 사용하지 않는다.
- 2026-10-19 태양 추가 결정: 물속의 정상 태양은 같은2초 동안 오염된 태양 반사로 바뀐다. 단순한 태양 색상 어둡게 처리는 사용하지 않는다. Restore Present Sun With Withering을 켜고 PastSunSource도 전환 대상에 포함한다. 원형 구간 페이드 이후 On Continued→OpeningSuns.PinCorruptedSunToCamera를 호출해 화면 위쪽 태양 중심을 뷰포트(0.86,0.84)에 유지한다. 위치·줌 변화 후에도 유지하며 기존 원형 태양 궤도는 페이드 전까지 사용한다.
- 2026-10-19 사용자 요청: 화면 문제는 수정 전후 실제 Game View를 캡처해 비교한다. 문제가 남으면 재수정하고 다시 캡처하며, 확인한 범위만 완료로 보고한다.
- 2026-10-19 사용자 요청으로 PPT4~7을 기준으로 기존 물가 구간을 다시 구성했다. 애셋은 Assets/Ember/Art/Storybook/WaterPPT, 재사용 프리팹은 Assets/Ember/Prefabs/Story/WaterPPT에 있다. 자세한 설정·캡처·남은 차이는 Docs/WaterSequence_PPT4-7_Setup.md 참고. 이번 요청은 기존 구간의 배치 수정 허가이며 앞으로 새 씬을 임의 생성하는 허가는 아니다.
- 씬 생성과 맵 배치는 사용자가 직접 한다. 요청 없이 씬을 생성하거나 배치하지 않는다.
- 한 번에 한 작업을 설명하고 확인받은 범위만 진행한다. 기본 움직임은 구현된 기반으로 취급한다.
- 2026-10-17 사용자 요청으로 기본 상단 나레이션을 구현했다. StorybookNarration 프리팹의 Lines를 등록하고 PlayerEventTrigger 이벤트에 BeginNarration을 연결한다. Z 진행·마지막 닫기·선택 조작 잠금만 포함한다. 씬 배치는 사용자가 하며 추가 동화 연출 툴은 별도 요청 범위다. 설명은 Docs/StorybookNarration_Setup.md를 따른다.
- 첫 폴더 정리로 스테이지 추출 아트를 Assets/Ember/Art/Storybook/StageExtracted/로 이동했다. Unity AssetDatabase로 이동해 GUID를 유지했다. 다른 아트 폴더 이동은 별도 작업이다.

## 최신 구현 인수인계 — 2026-10-16

- 최신 작업 범위와 스토리·방 구성·조작 논의는 루트 `pj_ember_구현_인수인계.md`를 함께 읽는다. 기존 14방 계획보다 최신인 발표용 1차 범위는 튜토리얼 별도 + 일반 방 8개 + 선택 진입하는 우미 보스 방이다.
- 지금 시작할 구현은 첫 튜토리얼 씬이다. 기존 레벨 워크플로우로 지형·판정을 먼저 만들고 동화풍 AI 이미지를 겹친다. 기능 오브젝트는 프리팹으로 재사용한다.
- X 이동 중 Z로 이탈해 속도를 받아 포물선으로 착지하는 요구가 있다. 대시/C+Z 제거는 확정되지 않았으므로 임의 삭제하지 않는다.
- 핵심 이야기는 앰버가 태어난 이유를 찾는 여정이며, 힘이 있다는 이유로 희생해야 한다는 주장을 세계관의 정답으로 취급하지 않는다. 세부 흐름과 우미 선택은 위 인수인계를 따른다.

## 최신 아트 결정 — 2026-10-15

- 사용자가 게임 전체 분위기를 **동화풍 낙서 이미지**로 바꿀 예정이라고 명시했다. 앞으로 만드는 타이틀·배경·지형·캐릭터·기믹·UI 아트는 이 방향을 우선한다.
- 시각 참고는 사용자가 첨부한 손그림 스테이지 이미지다. 불규칙한 손그림 선과 종이 위 채색 질감을 참고하며 세부 팔레트·선 굵기·해상도는 미확정이다.
- 기존 픽셀풍·8×8 제작 기준은 과거 기록이다. 새 아트에 자동으로 적용하지 않는다.
- 분리 자료는 `Assets/Ember/Art/Storybook/StageExtracted/`이다. 추출·복원 PNG이며 타일셋·애니메이션·씬 교체 완료를 뜻하지 않는다.
- 타이틀의 B안 중앙 구도와 메뉴 동작은 `다음 작업.md`를 따른다. 기존 픽셀풍 타이틀 이미지는 새 스타일에 맞춰 교체할 예정이다.

## 최신 사용자 결정 — 2026-10-15

- 현재 게임 콘셉트는 **불의 정령**이다. 사용자가 바람 콘셉트를 중단하고 불 버전 복원을 요청했다.
- 복원 기준은 GitHub 이력의 `1b7a702` (`Save current Ember project progress`)이다. 복원 브랜치 이름은 당시 기록이며 현재 작업 브랜치는 `main`이다.
- 현재 저장된 프로젝트와 에디터 기준은 **6000.6.5f1**이다. 불 버전 복원 당시 기준은 6000.5.2f1이었다. 바람 버전의 Unity/패키지 설정을 자동으로 적용하지 않는다.
- 현재 본편 규모는 일반 방 **14개 + 보스전 1개**다. 방 번호별 역할을 임의로 고정 배정하지 않는다. 사용자가 타일 지형을 제작한다.
- 최신 조작 변경: 방향키 이동·조준, Z 점프/불·로프 탈출(점화 링 안에서 대시), X는 불 이동 또는 대상이 없을 때 대시(점화 링 안에서 불씨 조준 사격), C는 주변 점화 폭발. 마우스 입력은 사용하지 않는다.
- 기본 스테이지를 보스전보다 먼저 완성한다. 콘셉트는 태양이 되기 위해 돌산을 오르는 불꽃 정령이며, 물이 많은 동굴과 자연 가연물을 검토 중이다.
- 세계관·아트·오프닝 기준은 Docs/WorldAndStageDirection.md다. 태양의 쇠퇴로 퍼진 오염을 불 기믹별 정화 조건으로 제거하며 등반한다. 모든 불 능력으로 모든 오염을 제거하지 않는다.
- 화염꽃은 C로 활성화하면 꽃마다 미리 정해둔 방향으로 즉시 발사한다. 플레이어 위치는 반응 거리 판정에만 사용한다. 방향 변경 장치는 추후 검토한다. 꽃 불꽃 전용 오염 정화 조건과 배치는 Docs/FireFlower_Setup.md를 따른다.

## 다른 세션에서 읽을 순서

1. 이 파일.
2. `Docs/AI/FireProject_Handoff.md`.
3. 루트 `TASKS.md`와 실제 현재 씬·코드.
4. 요청과 관련 있는 기존 OpenSpec change 및 설정 문서.

## 스펙·작업 목록 해석

- 바람 단계의 X 글라이더, 자동 기류 탑승, 깃털 비행, 바람 부족 이야기 및 불 기능 취소 분류는 현재 목표가 아니다. 다른 브랜치·stash·과거 문서에서 발견해도 현재 작업에 자동 적용하지 않는다.
- 현재 불 조작은 복원 코드와 사용자 최신 결정인 방향키+Z/X/C를 기준으로 `PlayerAbilityInputRouter`와 `FlamePlayerController`를 확인한다.
- 불 흡수·점화·불 발사·로프·물 접촉 소화 등은 복원된 코드 기준으로 확인한다. 바람 단계에서 취소했었다는 이유로 다시 제거하지 않는다.
- OpenSpec의 완료 체크는 해당 작업을 진행했을 때의 기록이다. 모든 스펙이 현재 채택한 최종 기획이거나 모든 기능이 최신 검증을 통과했다는 뜻은 아니다.
- `add-fourteen-rooms-water-spirit-escape`의 2026-10-06 구현 제거 기록은 유지한다. 그 옛 통합 보스 계획을 자동 재구현하지 않는다. 이후 개별 제작한 물의 정령·파도 코드와 대조한다.
- 과거 20분 기획·5개 스테이지·일정·스토리 상세는 참고 자료다. 불 버전 복원만으로 모두 다시 확정된 것으로 취급하지 않는다.
- 사용자에게 새 결정이 나오면 이 기준과 인수인계를 함께 갱신한다. 구현하지 않은 작업을 완료로 체크하지 않는다.

## 버전 확인

작업 전 현재 브랜치와 HEAD를 확인한다. 이 복원은 로컬에서 수행했으며 원격 `main`은 복원 당시 바람 버전이었다. 다른 컴퓨터에서 원격 기본 브랜치를 가져왔다는 사실만으로 불 복원이 적용됐다고 가정하지 않는다. 브랜치를 바꾸거나 원격 이력을 덮어쓰는 작업은 사용자 요청에 따라 진행한다.

- 불/로프 내부 Z 새 입력으로 탈출·박차기, X 입력으로 다른 불로 이동한다. C는 주변 가연물을 점화하는 불꽃 폭발을 일으킨다.
- 화염꽃은 플레이어 공격용이 아닌 오염 파괴용이다. 꽃 총알은 플레이어 충돌체를 통과하고 피해·사망을 유발하지 않는다.

- 화염꽃 프리팹은 Body와 Head를 분리한다. 배치 시 Head의 Rotation Z로 발사 방향을 정하며 Head 로컬 +X가 발사 방향이다. 줄기·잎은 머리 회전과 개화 애니메이션의 영향을 받지 않는다.
- 화염꽃은 C뿐 아니라 플레이어 직접 접촉으로도 활성화된다. 접촉은 트리거로 감지하고 머리 방향으로 발사한다. 계속 닿아 있는 동안 반복 발사하지 않는다.
- 임시 붉은 가시 오염은 PollutionSpikes.prefab이다. 화염꽃 총알에 맞으면 해당 프리팹 덩어리의 그림·위험 충돌이 사라지고 맵 초기화 시 복구된다.
- 2026-10-15 오염 가시 형태 수정: 긴 수정/산호 기둥이 아니라 작은 둥근 밤송이 덩어리에 짧은 가시가 빽빽하게 돋은 붉은 군집을 사용한다.
- 2026-10-15 최신 오염 아트 요청은 개별 밤송이 타일 3종이다. Assets/tile/PollutionBurrs의 Burr_A/B/C Tile 애셋과 Burr Pollution 팔레트를 사용한다. PurifiableSpikeTilemap은 꽃 총알에 맞은 한 칸만 제거한다.
- 현재 문서 시작점은 Docs/README.md이며 과거 기획·일정·상태 문서 5개는 Docs/Archive/README.md에서 찾는다. 보관 문서를 현재 구현 지시로 자동 적용하지 않는다.
- 2026-10-17 툴 정리: Level Workflow와 카메라 경계/이동 경로/발판 편집기만 Unity에 유지했다. 일회성 생성·자동 설정 9개를 삭제하고 과거 검사에서 호출하는 생성 코드 8개는 Validation/EditorSupport로 분리했다. 삭제한 툴을 요청 없이 재생성하지 않는다.

- 2026-10-17 동화풍 정리: 배경은 Assets/Backgrounds/Storybook 및 Legacy/WaterlitRuins에 둔다. 현재 플레이어 Animator와 Idle은 Assets/player/Animations/Current, game 1 전용은 Animations/Legacy에 유지한다. 과거 미사용 Animator 11개·클립 5개와 CameraPositionGrid/CameraShiftTrigger를 삭제했다. 손그림 HandDraw_Player는 유지한다.

- 2026-10-18 수면 표현: Storyscene의 River는 StoryWater.mat의 화면 반사·왜곡과 SurfaceRipples.vfx를 사용한다. VFX Graph 17.6.0, EmberWater 정렬 레이어, Renderer2D Camera Sorting Layer Texture(Default까지 캡처)를 유지한다. 설정은 Docs/StoryWater_Setup.md를 따른다.

- 2026-10-18 PPT 4~7 후속 작업: 사용자 요청으로 Storyscene 기존 물가 구간에 비교 캡처 기반 현재/과거 배경·발판 5개·물가/건너편 아트·과거 반사와 나레이션을 적용했다. 새 씬은 만들지 않았다. River의 WaterSurface2D는 x68~78 진행에 따라 밝은 과거를 드러내며, ParallaxLayer 뒤 실행 순서1100에서 반사 좌표를 맞춘다. 최신 설정/시각 확인 한계는 Docs/WaterSequence_PPT4-7_Setup.md와 ArtReference/OpeningWater/를 따른다.

- 2026-10-19 Level Workflow 문의 대응: 삭제 상태였던 유지 대상 편집기4개(EmberLevelWorkflowWindow, CameraRoomEditor, MovementPathEditor, PathMovingBlockEditor)를 Git 원본과 메타로 복구했다. 메뉴는 Tools > Ember > Level Workflow. 다른 삭제된 생성 툴은 복원하지 않았다.

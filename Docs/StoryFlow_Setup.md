# Storyscene 스토리 플로우 설정

작성 기준: 2026-10-21. 오프닝 개편과 출발 경사 연결은 코드와 저장 씬에 반영했으며, 실제 Play와 Game View 화면은 아직 확인하지 않았다.

기존 강가 배치를 중심으로 새 흐름을 조립하는 순서와 좌표는 [Storyscene 조립 지도](StoryAssembly_Rebuild.md)를 따른다. 강가를 복제하거나 새로 배치하지 않고 현재 `StoryShoreSection`을 재사용한다.

## 현재 진행 순서

1. `Opening_Comic`이 세 컷 만화를 3.5초 표시하고 0.85초 동안 페이드아웃한다. 이 동안 플레이어 입력을 잠근다.
2. 만화가 사라지면 원형 이동을 시작하지 않는다. Storyscene의 `Global Light 2D`를 끄고 플레이어를 따라다니는 따뜻한 Point Light를 만든다. 오염된 태양은 카메라 우측 상단에 고정되고, `StorySectionCamera`가 시작점부터 플레이어를 따라간다.
3. 원형 회전 대신 `RotatingFlatGroundEntrance.Begin At Join Pose`가 출발 평지를 바로 연결한다. `FlatGroundJoin`은 약 9.99도 기울고 플랫폼 길이를 1.2366배 늘려 월드 (0, -1.33)에서 강둑 경사 시작점 (34.1, 4.68)까지 이어진다. 플레이어는 같은 Storyscene 안에서 걸어 평지와 정령에게 간다. 화면 이탈 페이드/NextStoryStart 워프는 비활성화했다.
4. 평지의 `Guide_NPC`는 오염에 붙잡힌 빛의 정령이다. 플레이어가 진입 영역에 다가오면 `Corruption Vines`가 사라지고 Celeste풍 대화가 시작된다. 대화 중 조작은 잠기며 마지막 줄에서 Z로 닫으면 해제된다. 이 구조는 접근 시 자동으로 일어난다.
5. 정령은 태양이 오염된 뒤 세상과 정령들이 빛을 잃었다고 설명한다. 산 정상의 오래된 빛의 근원을 찾아가면 변화의 이유와 앰버 자신의 정체를 알 수 있다는 동기를 준다. 이 이야기는 앰버가 태어나자마자 태양이 되겠다고 정한 것으로 표현하지 않는다.
6. 이후 기존 길을 따라 `Stone_3/MemoryOnMiddleStone`에 착지하면 `RiverMemorySequence`가 강의 과거 장면을 재생한다. 끝나면 건너편 시든 태양꽃에 다가가 C로 활성화하고 에너지를 전달한다.
7. 에너지 도착 뒤 기존 `StoryTutorialSceneTransition`이 페이드 후 `TutorialScene`을 Single 로드한다. 능력 튜토리얼 이후 산으로 향하는 전체 플레이 구간은 후속 레벨 작업이다.

## 배치와 하이어라키

- `Opening_Comic/Three_Panel_Art`: Screen Space Overlay, sorting order 100. 기존 만화 그림은 탄생과 산을 향한 첫 여정을 보여준다.
- `Global Light 2D`: Storyscene에서 비활성. `OpeningComicSequence`가 플레이어 위치에 Point Light를 런타임 생성한다.
- `WorldPivot/GroundArt`: 원형 그림과 장식은 씬에 남아 있지만 오프닝 진행 중 회전하지 않는다. 물리 반경은 변경하지 않았다.
- `Circle/FlatGround`: 평지 진입 스크립트가 시작 시 기존 `FlatGroundJoin` 위치·경사에 플랫폼을 켠다. 경사 끝이 `StoryShoreSection/ShoreCollision` 시작에 맞는다. `Circle`의 `CircularOpeningWorld`는 시작 이벤트에 연결되어 있지 않다.
- `ScreenExitSceneLoader`: 화면 밖으로 나갔을 때 플레이어와 카메라를 다음 좌표로 옮기던 컴포넌트는 비활성화했다.
- `StoryShoreSection/Guide_NPC/Guide Artwork`: 평지의 빛의 정령 그림.
- `StoryShoreSection/Guide_NPC/Corruption Vines`: 정령을 감싼 기존 보라색 오염 스프라이트. 대화가 실제 시작될 때 꺼진다.
- `Stone_3/MemoryOnMiddleStone`: 가운데 발판의 착지 이벤트. `requireGrounded`와 `landingSurface`는 기존 돌 Collider를 계속 참조한다.
- `StoryShoreSection/Withered Sunflower`: 강 건너 C 반응 꽃. 접촉 활성화와 발사체 발사는 꺼져 있다.

## 조절값

| 대상 | 필드 | 현재 값 |
| --- | --- | ---: |
| OpeningComicSequence | Hold Seconds / Fade Out Seconds | 3.5 / 0.85 real seconds |
| OpeningComicSequence | Player Light Intensity | 1.25 |
| OpeningComicSequence | Player Light Inner / Outer Radius | 1.2 / 6.5 units |
| OpeningComicSequence | Player Light Color | 따뜻한 주황 `#FF7A29` |
| StorySectionCamera | Min X / Max X | 0 / 82 |
| Guide_NPC narration cue | Presentation | Dialogue |
| Guide_NPC narration cue | Lock Player Controls | 켜짐. 마지막 줄에서 Z로 닫으면 해제 |
| StoryNarrationCue | Rescue Visuals | `Corruption Vines` 한 개 |
| StoryFlowerEnergySequence | Travel / Arrival Glow Seconds | 0.85 / 0.25 game seconds |
| StoryTutorialSceneTransition | Fade Out / Fade In | 0.65 / 0.4 seconds |

정령 문구는 `Guide_NPC`의 `StoryNarrationCue.lines`에서 편집한다. 강가 과거 기억 문구는 기존 `PastNarration`에 있다. “아무거나” 큐는 Circle에 남겨둔 비활성 구성요소이며 현재 흐름에는 나타나지 않는다.

## 주요 파일

- 씬: `Assets/Scenes/Storyscene.unity`
- 정령 이미지: `Assets/Ember/Art/Storybook/Characters/StorySpirit_Doodle_v1.png`
- 정령 Idle 시트: `Assets/Ember/Art/Storybook/Characters/StorySpirit_Idle_6Frame_v1.png` (사용자가 선택한 정령 그림 기준으로 갱신, 3×2, 프레임 512×512, 다중 스프라이트 임포트, 500 PPU). `Guide Artwork`에 아직 연결하지 않았고 애니메이션 클립/Animator는 만들지 않았다.
- 오염 그림: `Assets/Ember/Art/Storybook/StageExtracted/07_CorruptionVines.png`
- 시작 만화: `Assets/Ember/Art/Storybook/OpeningComic_ThreePanels_v1.png`
- 시작 시퀀스·불꽃 조명: `Assets/EmberPrototype/Runtime/OpeningComicSequence.cs`
- 구출 시점 처리와 대사 잠금: `Assets/EmberPrototype/Runtime/StoryNarrationCue.cs`
- 평지 시작 위치: `Assets/EmberPrototype/Runtime/RotatingFlatGroundEntrance.cs`
- 꽃 에너지 이동: `Assets/EmberPrototype/Runtime/StoryFlowerEnergySequence.cs`
- 강가에서 튜토리얼 씬 전환: `Assets/EmberPrototype/Runtime/StoryTutorialSceneTransition.cs`
- 씬 변경 전 백업: `Assets/Scenes/Backups/Storyscene_BeforeSpiritRescueOpening_20261020.unity`

## 런타임 확인 순서

Unity에서 Storyscene을 열고 만화 뒤 Global Light가 꺼지는지, 불꽃 주변의 빛과 오염 태양이 보이는지 확인한다. 방향키로 원형 회전 없이 출발 평지의 완만한 경사를 따라 정령에게 이동해 오염 그림이 사라지고 대화가 시작되는지 확인한다. 이어 Stone_3 착지, 강 과거 장면, C 꽃 활성화, 에너지 전달, TutorialScene 페이드/로드 순으로 플레이 화면을 캡처한다. 현재 이 입력 완주와 Game View 캡처는 미확인이다.

# 원형 오프닝 배치

## 최신 범위 정정 — 2026-10-20

사용자가 원 안 오브젝트만 마음에 들지 않았고 평지·강가는 좋았다고 정정했다. 따라서 원 안의 나무·풀·돌은 원본 시트로 두되 GroundDisk_Doodle_v1, FlatGround_Doodle_v1, PPT_Descent_Doodle_v1과 FlatGroundArt의 StoryGroundJoinBlend/전용 재질은 다시 적용했다. 평지4곳·경사면1곳의 Collider 값은 유지했다. 아래의 전체 복원 기록은 범위를 잘못 이해했던 중간 상태이며 현재 채택 상태가 아니다.

실제 Play270도 연결부 전후는 ArtReference/OpeningWorld/FlatRepair/01_Before_Play.png와02_After_Play.png다. 전 사진은 직전 전체 복원 작업의 실제 Play 캡처를 사용했다. AngleRotator.Drive로 해당 구도를 확인했으며 전체 입력 완주 기록은 아니다.

## 최신 선택: 원 그림 유지·다른 교체분 복원 — 2026-10-20

사용자가 구 형태를 제외하면 이전 아트가 더 좋다고 피드백했다. GroundDisk_Doodle_v1.png는 유지하고 SurfaceDecorations16곳과 DistantScenery의 나무8곳은 원본 Foreground_Sheet/Background_Sheet로 복구했다. 원래 표시 위치·크기는 적용 직전 기록에서 복구했고, 군집용 DoodleTree 자식12개를 제거했다. 배경 나무 색은 원래 흰색·알파1이다. 앞 장식 Order9는 플레이어 가림 방지를 위해 유지한다.

평지4곳과 CharcoalDescent1곳도 원본 그림으로 복구했다. FlatGroundArt의 StoryGroundJoinBlend를 제거하고 StorybookUnlit 재질로 돌렸다. 모든 Collider 값과 GroundArt의 낙서 원 그림·Transform은 유지했다. 아래 새 나무·풀·돌·평지 시안은 미채택 실험 기록이며 현재 씬 외형이 아니다. PNG·프롬프트는 비교 자료로 보존했다. 강가 새 그림 교체는 별도 후속 요청이다.

실제 Play에서 시작0도·평지 연결270도·물가 카메라 구도를 확인했다. 캡처는 ArtReference/OpeningWorld/PreviousArtRestored의01_Circle_Kept_Play.png,02_Flat_Restored_Play.png,03_River_Baseline_Play.png다. 원형 회전은 AngleRotator.Drive, 물가는 카메라/플레이어 위치를 지정한 시각 확인이며 전체 입력 완주가 아니다. 마지막 저장 Storyscene은 시작0도·clean, 재생 종료 상태다. 강가 새 PNG는 아직 생성하지 않았으며 스타일 질문 답변 대기다.

## 최신 장식 교체 — 2026-10-20

원·평지·경사면 묶음 이후 사용자가 다른 애셋도 바꾸도록 요청해 나무·풀·돌을 내장 image_gen으로 제작했다. 적용 도중 추가 요청에 따라 나무는 앞쪽 어린나무와 뒤쪽 다 자란 나무2종으로 나눴다. 이번 검토는 이 수정까지 포함하며 다음 이미지 묶음은 사용자 화면 확인 뒤 진행한다.

| 새 PNG (Assets/Backgrounds/Storybook/RotatingWorld) | 씬 용도 |
| --- | --- |
| Tree_Sapling_Doodle_v1.png | SurfaceDecorations/Foreground_3·8·11·16. 가지가 적은 어린나무, 이전 나무의55% 높이. |
| Tree_Doodle_v1.png | DistantScenery의 나무8곳. 알파0.42로 먼 거리 표현, 군집4곳은 자식 DoodleTree_1~3 조합. |
| Grass_Doodle_v1.png | Foreground의 풀6곳 |
| Rocks_Doodle_v1.png | Foreground의 돌6곳 |

투명 PNG·Single Sprite·PPU100·Bottom Center pivot·Full Rect·Bilinear·Uncompressed·mipmap 꺼짐으로 가져왔다. 원본 Foreground_Sheet/Background_Sheet는 보존했다. 그림의 불투명 외곽(alpha128)을 각 원본 Sprite rect 안에서 읽어 표시 크기·바닥 접점을 맞췄고, 어린나무만 요청대로55% 높이로 줄여 뿌리 접점을 유지했다. 원형 장식 전체의 그림 정렬은 Default20에서9로 내려 플레이어 Default10이 풀에 가려지지 않게 했다. Collider 직렬화 값은 교체 전후 같다.

원 뒤 산·언덕8곳, 물가 대형 나무·오염·배경 산·반사는 교체하지 않았다. DistantScenery의 기존 RotationParallax 설정도 유지했다. 기능 프리팹·이동 수치·카메라·회전 중심을 수정하지 않았다.

비교 캡처는 ArtReference/OpeningWorld/DoodleDecor_v1/01_Before_Play.png, 02_After_Play.png, 03_Rotated35_Play.png다. 실제 Play Game View에서 시작 구도와35도 회전 구도를 확인했다.35도는 AngleRotator.Drive를 호출한 시각 확인이며 방향키로 전체 오프닝을 완주한 기록은 아니다. 원본 프롬프트·추가 어린나무 요청은 같은 폴더의 GenerationPrompts.txt에 있다. 생성 모드는 내장 image_gen이다.

## 최신 원형 지형 아트 — 2026-10-20

사용자가 산 원호 추가를 취소하고 원형 지형부터 메인 화면·Mountains_Doodle의 낙서풍으로 교체하도록 요청했다. 직전 MountainArc_Doodle 오브젝트와 해당 PNG는 제거했다. 현재 교체 범위는 Storyscene의 WorldPivot/GroundArt 한 개다.

- 새 Sprite: `Assets/Backgrounds/Storybook/RotatingWorld/GroundDisk_Doodle_v1.png`
- 보존한 원본: 같은 폴더의 `GroundDisk.png`
- 제작: 내장 image_gen. 기존 원은 형태 참고, 사용자가 첨부한 낙서 산은 스타일 참고로 사용했다.
- 프롬프트: `ArtReference/OpeningWorld/DoodleDisk_v1/GenerationPrompt.txt`
- 이미지:1254×1254·Single Sprite·PPU100·중앙 pivot·Full Rect·투명 외부·Bilinear·mipmap 꺼짐.
- 표시: 기존 WorldPivot의 자식·Default/Order0·StorybookUnlit 재질. 사진 같은 바위 텍스처 대신 큰 회색·회보라색 마커 획과 검은 펜 윤곽을 사용한다.
- 원본 불투명 경계(alpha128 기준)는 X47~1206/Y48~1202, 새 그림은 X41~1211/Y48~1201이다. GroundArt의 X/Y 표시 축척과 약0.006U 위치 차이를 보정해 원본 지름과 중심을 맞췄다. 물리 원 반지름 약6.67U와 플레이어 설정은 변경하지 않았다.
- WorldPivot의 저장 시작 회전은0도다. 미리보기 회전은 Play 종료 후 복원했다.

실제 Game View 비교는 `ArtReference/OpeningWorld/DoodleDisk_v1/01_Before_Play.png`, `02_After_Play.png`, `03_Rotated35_Play.png`다.35도 캡처는 Play 중 AngleRotator.Drive를 호출한 시각 확인이며 방향키로 전체 오프닝을 완주한 기록은 아니다. 현재 컴파일 실패와 Console 오류0을 확인했다. 편집 중이던 skin scene의 활성 오브젝트와 기존 미저장 상태를 복원했다.

나무·풀·돌·원 뒤 배경·평지는 이전 아트가 남아 있다. 전체 스토리 아트 교체 완료로 취급하지 않는다.

스크립트: `Assets/EmberPrototype/Runtime/CircularOpeningWorld.cs`

## 기본 구조

```text
OpeningController       CircularOpeningWorld + EmberOpeningSequence + AngleRotator
WorldPivot              원 중심에 위치, Rotation Target으로 연결
  GroundArt             커다란 원형 땅 그림
  BackObjects           뒤쪽 나무·꽃·풍경
  GameplayObjects       화염꽃·오염 등 실제 기믹
  FrontObjects          앞쪽 풀·장식
Player                  기존 FlamePlayerController
StandingGround          원 꼭대기에서 플레이어를 받치는 고정 수평 바닥
Main Camera             회전 대상 밖에 유지
```

씬 배치와 원형 그림 제작은 자동으로 하지 않는다. 각 이미지의 Sorting Layer / Order in Layer는 사용자가 설정한다. 예: 배경 -20, 땅 -10, 플레이어 0, 앞 장식 10. 이 스크립트는 그리기 순서를 변경하지 않는다. 부모 Sorting Group을 사용한다면 서로 다른 앞뒤 그룹이 플레이어를 사이에 두도록 구성한다.

## 연결

1. 빈 오브젝트에 `Circular Opening World`와 `Angle Rotator` 추가.
2. Angle Rotator의 `Rotation Target`에 WorldPivot을 연결하고, Circular Opening World의 `Rotator`에 해당 컴포넌트, `Player`에 씬의 플레이어를 연결.
3. 플레이어를 원 꼭대기의 고정 바닥 위에 둔다. 플레이어와 카메라는 WorldPivot의 자식으로 넣지 않는다.
4. `Degrees Per Second`로 속도, `End Angle`로 총 회전 진행량을 조절한다.
5. 플레이 시 오른쪽 키는 진행각을 늘리고, 왼쪽 키는 줄인다. 기본 회전은 +Z이며 필요하면 Reverse Rotation을 켠다. Allow Backtracking을 끄면 후진하지 않는다.

실제 원형 중력이 아닌 오프닝용 연출이다. 원형 그림 자체의 지형 Collider는 사용하지 않고 플레이어 아래 고정 바닥으로 지지한다. 회전하며 올라오는 장식에 불필요한 고체 Collider를 붙이지 않는다. 꽃 접촉 등 물리 기믹을 자식으로 회전시킬 때는 별도 Kinematic Rigidbody2D를 적절히 구성한다. 오염 총알 경로와 충돌은 실제 배치에 맞춰 조정한다.

## Angle Points (Angle Rotator에서 설정)

각 항목에 Label, Angle, Pause On Arrival, On Reached를 설정한다. Angle은 시작 회전으로부터의 누적 진행각이며 0~End Angle 범위에 둔다. 순서대로 처리하며 되돌아가도 동일 이벤트는 반복 실행하지 않는다.

예시 각도는 실제 배치 후 조정한다.

| 지점 | Pause | On Reached 연결 예시 |
| --- | --- | --- |
| 오염된 태양이 들어오는 각도 | 끔 | OpeningSequence.CompleteWarmSun |
| 꽃 상호작용 구간 | 켬 | OpeningSequence.CompletePollutedSun, C 안내 표시 |
| 평지 전환 시작 | 끔 | OpeningSequence.CompleteRoundToFlat 또는 별도 카메라/지형 연출 |

꽃 구간에서는 PollutionObstacle의 On Purified에 `CompleteFlowerPurification()`과 `ResumeWalking()`을 연결한다. 정화되기 전에는 회전이 멈추지만 C 입력은 가능하다. 꽃은 직접 접촉으로도 활성화되므로 C 학습을 원하면 플레이어 몸에 바로 겹치지 않게 배치한다.

기존 단계 정의상 3단계 완료 후 4단계가 시작된다. 따라서 평지 전환이 실제로 끝난 시점에 CompleteRoundToFlat을 호출한다. 전환 시작과 완료 지점을 같게 두지 않는다.

## 시작·끝·초기화

- Begin On Start 기본 켜짐. 끄면 이벤트에서 BeginWalking을 호출한다. 자유 상태의 플레이어만 시작할 수 있다.
- PauseWalking / ResumeWalking: 원의 회전만 중지·재개한다. 플레이어 조작 전체 잠금은 별도 연출이 담당한다.
- End Angle에 도달하면 일반 이동으로 복귀하고 On Finished를 호출한다. 마지막 지점에서 Pause가 걸렸다면 ResumeWalking 후 종료된다.
- StopWalking: 완료 이벤트 없이 일반 이동으로 복귀한다. 오브젝트 비활성화 시에도 적용된다.
- ResetWorld: 회전·진행각·도달 지점을 초기화하고 정지한다. 재시작은 BeginWalking. 꽃·오염·OpeningSequence는 각각 별도 초기화가 필요하다.
- 사망 시 원 회전은 멈추고, 리스폰 후 이어진다. 원형 구간 체크포인트는 같은 고정 발판 위에 두어야 한다. 원형 진행 전체 되감기는 자동으로 수행하지 않는다.
- 조작 잠금 나레이션·일시정지·재시작 전환 중에는 회전하지 않는다.
- 원형 모드 중에만 가로 이동을 원 회전으로 대체하고 X/Z 능력 입력을 제한한다. C 점화는 기존 해금 조건을 따른다. 종료하면 기존 조작을 사용한다.

## 아직 포함하지 않은 것

곡률이 실제로 평평해지는 이미지 변형, 카메라 전환, 자동 나레이션, 앞뒤 레이어 자동 변경, 탄생 효과는 별도 단계다. On Reached / On Finished에 후속 연출을 연결한다. 플레이 모드 테스트는 수행하지 않았다.

## 범용 회전 분리

- AngleRotator: 회전 대상·속도·방향 반전·각도 제한·Angle Points·회전 정지/재개·완료 이벤트를 담당한다. 플레이어·입력·스토리에 의존하지 않는다.
- CircularOpeningWorld: 방향키를 회전에 전달하고 플레이어의 원형 모드 진입/복귀만 담당한다. 기존 BeginWalking/PauseWalking/ResumeWalking/FinishWalking/ResetWorld 함수는 유지한다.
- 다른 용도에서는 AngleRotator만 추가한다. Automatic을 켜면 자동 회전하며, Direction -1~1로 방향·속도 비율을 정한다. Limit Angle을 끄면 끝 각도 없이 진행한다. 누적 진행각의 최솟값은 0이므로 시작점 이전으로는 돌아가지 않는다. 물리 프레임의 scaled time을 사용한다.
- 외부 스크립트는 Automatic을 끄고 Drive(direction, deltaTime)를 호출한다. 자동 회전과 수동 Drive를 동시에 사용하지 않는다.
- PauseRotation/ResumeRotation/ResetRotation/SetAutomatic/SetDirection을 UnityEvent에서 사용할 수 있다. ResetRotation은 활성화 여부나 Automatic 설정을 바꾸지 않으므로 자동 회전 중 리셋하면 다음 프레임에 회전을 재개한다.
- Angle Points는 기본적으로 전진 중 첫 도달에만 호출하며 ResetRotation 후 다시 호출할 수 있다.
- 오프닝에서는 BeginWalking이 Automatic을 꺼서 방향키로만 제어한다. 범용 AngleRotator 자체는 일시정지 메뉴를 알지 못하며 외부 게임에서는 PauseRotation 또는 timeScale로 제어한다.
- 이전 CircularOpeningWorld의 회전 설정은 AngleRotator로 이동했다. 기존 씬/프리팹에서 해당 스크립트를 참조하는 파일은 없어 이전 배치 데이터의 자동 이관은 추가하지 않았다. 저장하지 않은 인스펙터 설정이 있었다면 새 컴포넌트에 다시 입력한다.

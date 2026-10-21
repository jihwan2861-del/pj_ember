# 원형→평지 이미지

## 최신 정정: 마음에 들었던 평지·경사면 재적용 — 2026-10-20

사용자는 원 안의 나무·풀·돌만 이전 것이 더 좋고 평지·강가는 좋았다고 정정했다. FlatGround_Doodle_v1.png4곳과 PPT_Descent_Doodle_v1.png1곳을 다시 연결하고, FlatGroundArt의 StoryGroundJoinBlend·FlatGround_Doodle_Join.mat을 복구했다. 설정은 아래 최초 낙서풍 적용과 같고 Collider는 유지했다.270도 연결부 실제 Play 전후 캡처는 ArtReference/OpeningWorld/FlatRepair/01_Before_Play.png 및02_After_Play.png다. 아래 원본 평지 복원 항목은 범위 오해로 생긴 중간 기록이며 현재 설정이 아니다.

## 최신 선택: 원본 평지·경사면 복원 — 2026-10-20

사용자는 구 형태 외에는 이전 아트가 더 좋다고 피드백했다. Storyscene의 평지4곳은 FlatGround_v1.png, CharcoalDescent는 PPT_Descent_Deep.png로 복구했다. 알파 외곽 보정식을 역방향 적용해 원래 표시 범위를 맞췄고 Collider 값은 그대로다. FlatGroundArt의 StoryGroundJoinBlend는 제거하고 원래 StorybookUnlit.mat으로 복원했다. GroundDisk_Doodle_v1 원 그림은 유지한다. 아래 낙서 평지·경사면 PNG, 코드·재질과 캡처는 미채택 실험 자료다.

## 최신 낙서풍 적용 — 2026-10-20, 사용자 확인 대기

원 그림과 같은 성긴 회색·회보라색 마커 획, 짙은 목탄색 바탕, 굵은 펜 윤곽으로 다음 두 PNG를 만들었다. 내장 image_gen을 사용했으며 원본은 보존했다.

- `Assets/Backgrounds/Storybook/RotatingWorld/FlatGround_Doodle_v1.png`
- `Assets/Ember/Art/Storybook/WaterPPT/Backgrounds/PPT_Descent_Doodle_v1.png`
- 전체 프롬프트: `ArtReference/OpeningWorld/DoodleFlat_v1/GenerationPrompts.txt`

Storyscene의 FlatGround/FlatGroundArt, StoryShoreSection/LeadInGround/Art, DescendingShore/Art, FarShore/Foundation은 새 평지 그림을 사용한다. CharcoalDescent는 새 경사 그림을 사용한다. 이5개 표시 참조를 바꿨으며 Sprite의 불투명 범위(alpha128)로 크기·중심을 보정했다. 타일·지형 Collider·카메라·회전 이벤트는 변경하지 않았다. 전체 Collider 직렬화 값을 적용 전후 비교해 일치함을 확인했다.

FlatGroundArt에는 `StoryGroundJoinBlend`와 `FlatGround_Doodle_Join.mat`을 적용했다. Sprite 전체를 흐리게 하지 않고, 기존 CircleCollider2D의 안쪽에 실제로 겹치는 왼쪽1.4U만 불규칙하게 혼합한다. 원 바깥은 평지의 알파를 유지하므로 접선 윗면에 하늘 틈이 생기는 것을 피한다. `Surface Feather`는0.08U다. 이 처리는 그림 전용이며 Collider나 이동 조작을 변경하지 않는다. Runtime 스크립트는 `Assets/EmberPrototype/Runtime/StoryGroundJoinBlend.cs`, 셰이더는 RotatingWorld/StoryGroundJoinBlend.shader다. 새 편집기 메뉴는 만들지 않았다.

비교 캡처는 `ArtReference/OpeningWorld/DoodleFlat_v1`에 있다.

- `01_Join_Before_Play.png`, `02_Join_After_Play.png`: 원형 진행270도 도착 구도의 Play Game View.
- `03_Shore_Before_User.png`: 사용자가 첨부한 변경 전 경사면 캡처.
- `04_Shore_After_Play.png`: 새 경사면이 적용된 Play Game View. 해당 구간에 카메라·플레이어를 놓은 시각 확인이며 전체 입력 경로 검증은 아니다. 변경 전 사진과 카메라 위치가 완전히 같지는 않다.

저장된 Storyscene은 WorldPivot0도·원형 시작 상태다. 시각 확인용 카메라·플레이어 변경은 Play 종료로 복원했다. 새 그림은 현재 검토안이며 원·평지·경사면3개 묶음에 대한 사용자 확인을 받은 뒤 다음 이미지 묶음을 만든다. 나무·오염·배경·물의 그림체 통일 완료로 취급하지 않는다.

## 이전 설정 — 2026-10-18

- 이미지: Assets/Backgrounds/Storybook/RotatingWorld/FlatGround_v1.png
- 내장 image_gen으로 GroundDisk.png와 같은 짙은 남색/목탄·수채화 질감의 수평 직사각형 지형을 제작했다. 프롬프트는 같은 폴더 FlatGround_v1.prompt.txt에 저장했다.
- Storyscene 루트의 FlatGround는 WorldPivot 중심 X에서 오른쪽으로 28 유닛 이어진다. 기존 CircleCollider2D의 꼭대기 높이에 맞췄다.
- FlatGroundArt의 투명 여백을 계산해 그림의 윗면과 BoxCollider2D를 맞췄다. 원과 같은 머티리얼을 사용한다.
- 회전 대상 밖의 고정 지형이다. 원형 지형과 기존 Collider는 유지했다.
- 2026-10-18 변경: 평지는 처음에는 비활성화된다. Circle의 Rotating Flat Ground Entrance가 Completed Turns(기본 2)만큼 회전한 뒤 평지를 화면 밖에서 활성화하고, 원과 같은 중심을 기준으로 90도 더 회전시켜 수평 연결한다.
- 카메라 이미지로 연결부 외형을 확인했다. 플레이 모드 테스트는 수행하지 않았다.

- 총 진행각 기본값은 810도(2바퀴 720도 + 진입 90도)다. 도착 시 AngleRotator가 끝나 기존 CircularOpeningWorld가 일반 이동으로 복귀한다. Start에서 인스펙터의 바퀴 수로 끝 각도를 계산한다.
- FlatGroundJoin은 최종 수평 위치/방향을 지정하는 빈 오브젝트다. 처음에 숨겨진 FlatGround를 직접 옮기기보다 이 마커를 조절한다.
- 초기에는 평지 그림과 충돌체가 함께 비활성화된다. 진입 시 별도의 수평 슬라이드가 아니라 평지 덩어리가 원 중심을 기준으로 회전한다. 되돌아 걸으면 등장 위치도 되감긴다. 카메라 추적은 별도 작업이다.

- 최신 동기화: Rotating Flat Ground Entrance의 Synchronize With Sun을 연결하면 Completed Turns 대신 Opening Sun Transition의 Corruption Start Angle을 공통 등장 기준으로 사용한다. Entrance Degrees는 그 뒤 평지가 수평으로 연결될 때까지의 추가 회전량이다. 현재 기본은 180도 동시 진입, 270도 연결 완료이며 실제 값은 태양 설정을 따른다. 공통 시작 시점은 오브젝트가 화면 밖에서 활성화되는 시점이며 카메라에 보이기 시작하는 시점은 궤도 배치에 따라 다를 수 있다.

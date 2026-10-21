# PPT 4번 물가 구간 — 2026-10-18

사용자 요청: 평지를 조금 진행한 뒤 원본 앰버 연출 PPT 4번처럼 내리막·산 배경·물가가 보이게 구성한다.

- Storyscene 안에 StoryShoreSection을 배치했다. 원형 구간에서는 비활성화되어 보이지 않는다.
- 기존 화면 밖 페이드의 On Continued에 Main Camera의 StorySectionCamera.BeginFollowing을 연결했다. 검은 화면에서 물가 구간이 활성화된다.
- NextStoryStart는 (43, -0.91), NextCameraPose는 (44.5, 0, -10)으로 변경했다.
- x34~46: 안전한 평지, x46~54: 완만한 내리막, x54부터 강이다. 원본 스케치는 최종 충돌 설계가 아니므로 현재는 간단한 플레이 지형으로 구성했다.
- 물은 지형 그림과 별도 Sprite이며 WaterContact + RoomHazard로 기존 소화/사망 판정을 재사용한다. 물에 닿아 사망하면 다음 구간 시작점에서 다시 시도한다.
- 산 배경은 기존 01_MountainBackground.png를 사용한다. 태양은 기존 OpeningSunTransition이 화면 오른쪽 위에 유지한다.
- StorySectionCamera는 이 구간 진입 후에만 작동한다. Horizontal Offset, Min X, Max X로 구도를 조절한다. 배경도 함께 이동해 빈 화면이 드러나지 않게 한다.
- 물 이미지: Assets/Backgrounds/Storybook/StoryRiver_v1.png. 내장 image_gen 제작이며 프롬프트는 같은 폴더 StoryRiver_v1.prompt.txt에 저장했다. 실제 물 반사 셰이더나 움직이는 잔물결 애니메이션은 아니다.
- 원/평지에 사용한 목탄 지형 이미지를 재사용해 톤을 유지했다. PPT의 노란 풀과 세부 절벽 미술을 그대로 복제한 상태는 아니다.
- 강 건너기 발판·태양꽃·오염은 이번에 배치하지 않았다. 다음 물가 튜토리얼 단계에서 구성한다.
- 에디터 카메라 미리보기로 구도를 확인했다. 처음부터 물가까지의 실제 플레이 검증은 수행하지 않았다.

## 2026-10-18 후속 적용
PPT 4~7 비교 제작으로 물가 오버레이·발판 5개·건너편 땅·현재/과거 배경·반사·나레이션을 추가했다. 위 최초 구성의 고정 카메라 한계, 정적 물, 발판 미배치 기록은 최초 상태 설명이다. 최신은 WaterSequence_PPT4-7_Setup.md를 따른다.

# 강가 낙서 3종 — 2026-10-20

내장 image_gen으로 기존 원본을 참고해 각각 편집했다. 전체 프롬프트는 GenerationPrompts.txt이며 새 PNG는 정확히3개다. 사용자 적용 화면 확인 대기다.

## 저장한 애셋

- Assets/Ember/Art/Storybook/WaterDoodle_v1/Stone_Mid_Doodle_v1.png
- Assets/Ember/Art/Storybook/WaterDoodle_v1/Stone_Mid_Healthy_Doodle_v1.png
- Assets/Ember/Art/Storybook/WaterDoodle_v1/Mountains_Doodle_v1.png

현재 Stone_2의 마른/푸른 Sprite와 MountainBackdrop의 Sprite만 교체했다. 기존 모든 Transform/Collider/WaterSurface2D/ParallaxLayer 설정은 일치한다. 삭제된 Stone_5는 재생성하지 않았다. 높은 Stone_3와 숲·오염태양·물 텍스처는 아직 이전 그림이다.

## 실제 Game View 캡처

| 파일 | 내용 |
| --- | --- |
| 01_Before_Wide_Play.png | 적용 전 카메라(66,0), ortho8 |
| 02_Before_Close_Play.png | 적용 전 카메라(61.5,.2), ortho5 |
| 03_After_Wide_Healthy_Play.png | 적용 후 같은 Wide 구도의 푸른 반사 |
| 04_After_Close_Healthy_Play.png | 적용 후 같은 Close 구도의 발판·산 |
| 05_After_Close_Withered_Play.png | 기존 시듦 전체 흐름 완료 후 마른 풀·어두운 물빛 |
| 06_After_Right_Pan_Play.png | 카메라를(80,0)으로 옮겨 오른 구간 산 확인; 우측 회색은 기존 튜토리얼 |

플레이어 위치와 Rigidbody.simulated=false로 비교 구도를 고정하고 실제 게임 프레임이 진행된 뒤 캡처했다. 기존 BeginTreeWithering 직접 호출 후7.003263게임초에 TreeWitherProgress/SunTransitionProgress=1이 됐다. X80까지 실제 입력으로 이동하거나 전체 구간을 완주한 검증은 아니다. 자동 테스트/빌드를 추가 실행하지 않았다.

중간발판은 원본과 같은1254²이고 산은1915×821로 원본1916×821보다 가로1px 짧다(0.0522%). 모두 center pivot/PPU100이며 사용자 Transform은 보정하지 않았다. 산은 기존 ParallaxLayer를 통해 표시되며 현재 촬영한 구도에서 배경 공백은 보이지 않았다.

변경 전부터 사용자 미저장 씬 편집이 있어 현재 Storyscene은 dirty를 유지했다. 새 PNG/Importer설정은 저장했다. Play 종료 후 paused=false, Rigidbody.simulated=true, runInBackground=false로 복구했다. 현재 Console groundTruth오류0·컴파일 실패false를 확인했다. 적용 보고는 WaterDoodleBatch3Report.txt, 시듦 진행값은 WaterDoodleWitherState.txt에 있다.

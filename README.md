# Project Ember

불꽃이 하늘로 올라가 태양이 되는 정밀 이동 퍼즐 플랫포머의 Unity 프로토타입입니다. 픽셀 표현과 방향키·Z/X/C 조작을 사용합니다.

## 열기

1. Unity Hub에 이 저장소 폴더를 프로젝트로 추가합니다.
2. Unity **6000.5.2f1**로 열고 패키지 설치·컴파일을 기다립니다.
3. `Assets/Scenes/2.unity`를 열어 Play로 확인합니다.

Unity의 Library·Temp·빌드 결과는 저장소에 포함하지 않습니다.

## 현재 구현

- 이동·점프·대시와 점화·흡수·불 발사.
- 지형 RuleTile과 합성 충돌, 플레이어 애니메이션 도구.
- 방별 카메라 경계와 체크포인트.
- `Tools > Ember > Level Workflow`: 방 구조 생성, 카메라 경계 편집, 타일맵 구성 검사.
- 직선·곡선·원형 경로와 한 번 이동·왕복·순환 블록.
- 움직이는 발판 위에서 서 있기·걷기·점프하는 플레이어 운반 처리.
- 위에서 닿으면 튕기고 공중 점프·링 사용권을 회복하는 스프링 스크립트.
- 불붙은 로프 끝 X 흡수, 진입 기세를 받는 스윙, Z 박차기와 연속 연결 테스트 씬.
- `WebDemo/index.html`: 별도 브라우저 게임 데모.

발판 자체가 기울어 회전하는 기능, 발판 점프 관성과 압사 처리는 아직 구현하지 않았습니다.

## 기획·제작 가이드

- [20분 플레이 상세기획](Docs/Design/Ember_20Minute_Design_v1.md)
- [방 제작 워크플로우: Hierarchy·카메라·타일맵·경로](Docs/LevelEditingWorkflow.md)
- [Mossy 자동 타일 설정](Docs/Mossy_Autotile_Setup.md)
- [스프링 배치](Docs/Spring_Setup.md)
- [로프 설정과 연속 연결 테스트](Docs/Rope_Setup.md)
- [플레이어 이동 분석](Docs/AI/PlayerMovementAnalysis.md)
- [이동 감각 수정](Docs/AI/MovementFeel_Implementation.md)
- [바닥 타일 경계 걸림 조사](Docs/AI/Tilemap_GroundSnag_Investigation.md)
- [움직이는 발판 운반 수정](Docs/AI/MovingPlatform_Rider_Fix.md)

일부 문서의 로컬 파일 링크·검증 실행 경로는 개발 당시 Windows 작업 공간을 기준으로 작성되어 있습니다.

## 검증 기록

로프 추가 검증은 **53개 통과, 실패·건너뜀 0개**다. 기존 이동 35개와 로프 관련 18개이며 실제 Play Mode의 횃불→두 로프→착지→재시작을 포함한다. [로프 검증 결과](ValidationArtifacts/MovementFeel/Rope/VerifiedVisual/UnityResults.xml)와 [설정·검증 안내](Docs/Rope_Setup.md)를 참고한다.

움직이는 발판 검증 당시 격리 Unity에서 **52개 통과, 실패·건너뜀 0개**를 확인했습니다. 플레이어 운반 10개, 기존 입력·이동·타일맵 21개, 방 편집·카메라·경로 21개입니다.

- [최종 테스트 결과](ValidationArtifacts/MovementFeel/PlatformRider/Final/UnityResults.xml)
- [이동 검증 실행 안내](Validation/Movement/README.md)
- [방 도구 검증 실행 안내](Validation/LevelWorkflow/README.md)

EditMode에서 실제 컴포넌트와 네이티브 Physics2D를 사용한 결과이며, 전체 게임 플레이 완료나 실행 파일 빌드 검증을 뜻하지 않습니다.

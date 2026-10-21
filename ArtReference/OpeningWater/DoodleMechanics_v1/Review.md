# 태양꽃·접촉 오염 낙서 3종 — 2026-10-20

내장 image_gen으로 새 PNG 3개를 만들었다. Head/Body는 기존 분리 이미지 편집, 오염은 참고 이미지 기반 신규 생성이다. 프롬프트 전체는 GenerationPrompts.txt에 있다. 사용자가 적용 화면을 보고 “좋다 다음가자”라고 답해 이번 태양꽃 머리·몸통·접촉 오염 3종은 승인 완료했다.

## 저장 위치

- Assets/Ember/Art/Storybook/DoodleMechanics_v1/SunFlower_Head_Doodle_v1.png
- Assets/Ember/Art/Storybook/DoodleMechanics_v1/SunFlower_Body_Doodle_v1.png
- Assets/Ember/Art/Storybook/DoodleMechanics_v1/Pollution_Thorns_Doodle_v1.png
- 기존 SunFlower.prefab / FlowerRideTutorial.prefab에 연결했다.

## 실제 화면

- 01_Before.png: 변경 전, 실제 Play의 현재 배치를 카메라(125.5,2), ortho5.0625로 임시 미리보기.
- 02_After.png: 변경 후 같은 카메라. 플레이어는 첫 튜토리얼에서 사용하는 EmberWater 표시 레이어를 임시 적용했다. 이전 캡처는 직접 미리보기로 건너뛰어 플레이어가 배경 뒤에 가려져 있다.
- 03_Contact.png: 오염벽 바로 앞 물리 접촉으로 IsDying=true인 순간. 직접 KillAndRespawn을 호출하지 않았다.
- 04_Fired.png: 기존 FireFlower.TryActivate로 실제 발사, 탄환(121.56,2.9), 새 머리 그림 유지.
- 05_Purified.png: 같은 탄환이 기존 오염벽에 닿아 정화된 실제 화면.
- 06_Flower_Detail.png: Play에서 카메라만(119.2,2), ortho3으로 변경한 꽃 확대.

## 관찰 결과와 한계

기존 모든 씬 Transform/Collider 직렬화 값은 유지했다. 기존 Pollution12셀의 nontrigger벽을 보존하고 별도의 Box trigger/RoomHazard 자식을 추가했다. 실제 접촉에서 IsDying=true, 꽃 탄환 충돌에서 IsPurified=true 및 기존벽/새트리거/그림 비활성화를 관찰했다. ResetForRespawn 시 solid/art/hazard=true, 원래 회색 TilemapRenderer=false로 복구됐다. Awake/발사/리스폰 후 새 Head 참조가 유지됐다.

접촉 미리보기 첫 시도는 Rigidbody 위치와 Transform 동기화가 맞지 않아 출발점에 머물렀다. 미리보기 좌표 동기화를 고친 두 번째 시도에서 실제 접촉 사망을 관찰했다. 런타임 게임 코드 변경은 없었다. 원본 관찰을 MechanicObservations.txt에 함께 보존한다.

전체 튜토리얼 입력 완주, 빌드 또는 자동 테스트를 실행하지 않았다. 프리뷰 동안 카메라/플레이어 위치·일부 진행 컨트롤러를 임시 조정했고 Play 종료로 제거했다. 현재 Editor 컴파일 실패false/Console groundTruth오류0이다. Pipeline 누적 counts는 이전 오류까지 포함하므로 현재 오류 수로 읽지 않는다.

미저장 사용자 편집을 보존하기 위해 Storyscene은 dirty 상태를 유지했다. 새 PNG와 기존 프리팹 변경은 저장했다. 이번 승인 뒤 다음 묶음인 중간 발판 마른/푸른 짝과 산 배경 작업을 진행하며, 그 묶음의 적용 화면은 별도로 확인받는다. 다음 묶음은 현재 검증 중으로 이번 3종 승인에 포함하지 않는다.

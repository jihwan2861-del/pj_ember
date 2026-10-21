# 낮은 돌 착지와 바다 변화 — 2026-10-20

사용자 표시 부분은 강둑 그림의 왼쪽 낮은 돌이다. 원래 그림X74.7~79.1에 충돌이 없고 기존 FarShore Polygon은X79.62부터라 실제 점프 시 물로 떨어졌다. 현재 그림 배치와 기존 강둑 충돌을 유지하며 새 지지 충돌을 추가했다. 새 이미지는 만들지 않았다.

## 씬 설정

- `StoryShoreSection/FarShore/Art/ArrivalPlatform`: solid BoxCollider2D. worldX74.6~79,topY-1.5,bottomY-3.15.
- 자식 `WaterChangeOnLanding`: trigger worldcenter(76.8,-.8),size(4.15,1.5).
- PlayerEventTrigger: Trigger Once=true, Require Grounded=true, Landing Surface=ArrivalPlatform.
- On Player Entered→River.WaterSurface2D.BeginTreeWithering.
- River: Wither Trees By Player Position=false. 이전X80자동시작을대체한다.
- 기존 태양2초→여운3초→물/반사2초와나레이션이벤트유지.

## 실제 Play 확인

1. 01_Before_Approach: 앞Stone4에서출발(71.12,-.66),접지true.
2. 02_Before_FallThrough: 실제RightArrow+Z입력으로기존돌을통과해(74.88,-1.93)에서물접촉사망.
3. 03_After_Landing: 같은입력으로(74.60,-1.26)착지,HasTriggered=true,Sun진행시작.
4. 04_After_WaterChanged: 키를해제하고추가6.992096게임초후(74.77,-1.16)에서안정접지true,Sun/Water진행값1. 수동Begin호출없이착지이벤트로진행했다.
5. 05_Airborne_NoTrigger: 별도공중진입(76.8,-.37),접지false/트리거false/Sun0/Water0.
6. 06_Landed_Center_Triggered: 그위치에서자연낙하해(76.8,-1.18)착지,접지true/트리거true.

비교카메라(74.8,-.3,-10),ortho5.0625. 실제점프는초기출발위치만스테이징하고이후플레이어좌표를쓰지않았다. 공중조건확인은별도위치스테이징을했다. 자동테스트/전체오프닝완주결과는아니다.

원본관찰로그ArrivalLedgePlay.txt에는입력라우팅을설정하지않은첫프리뷰의실패도남겼다. 당시정지입력이게임으로전달되지않아재생확인중플레이어가구간을벗어났다. 프리뷰용입력설정복제본으로라우팅을맞춘후안정접지와물변화를다시확인했고04캡처를그결과로교체했다. 게임이동수치는변경하지않았다.

현재씬의미저장편집을유지했고,Play종료후임시키보드/프리뷰입력설정/카메라/플레이어좌표를복원했다. 소스변경은PlayerEventTrigger의선택착지조건이며기본값false로기존진입동작을유지한다. 현재컴파일실패false/Console groundTruth오류0이다.

# 물의 정령 추격 AI

기존 WaterSpirit 프리팹을 씬에 배치하고 Water Spirit AI의 **Player Body**에 플레이어 Collider2D, **Game Camera**에 게임 직교 Camera를 연결한다. Initial Behavior와 Warning은 프리팹에 연결되어 있다. 씬 참조가 누락되면 경고를 출력하고 공격을 중단한다.

## 추적과 공격

정령은 Move 애니메이션으로 플레이어를 계속 추적하며 **일반 파도 발사와 돌진을 번갈아** 실행한다.

- 일반 파도: 위험 영역 표시 없이 Attack 애니메이션으로 발사한다. 발사 중에도 추적하며 애니메이션 종료 후 바로 추적 상태로 돌아간다.
- 돌진: 수평 위험 영역을 표시하는 동안 잠시 멈춘다. 플레이어가 범위에 없어도 예고 시점의 방향과 높이를 고정한 뒤 Move 애니메이션으로 수평 돌진한다. 별도의 탑승 파도는 생성하지 않는다. 종료 후 즉시 추적한다.
- 회복 대기는 없다. 일반 파도가 아직 이동 중이면 다음 일반 발사를 기다리는 동안에도 계속 추적한다.

Side Chase Behavior의 **Follow Distance**는 플레이어 왼쪽 추적 거리(기본 3), **Ride Height Offset**은 상대 높이(기본 0.9), **Move Speed**는 추적 속도(8), **Tracking Time**은 공격 사이 추적 시간(2초), **Warning Time**은 돌진 예고 시간(0.7초)다. Use Riding Rush를 끄면 일반 파도만 발사한다. 기존 Recovery Time, Screen Margin, Arrival Tolerance 필드는 이전 데이터 호환용이며 현재 동작에 사용하지 않는다.

Water Spirit AI의 **Rush Speed**(16), **Rush Distance**(10), **Screen Padding**(0.15)으로 돌진과 화면 여백을 조절한다. 일반 파도 크기·속도·거리는 Water Wave 프리팹을 사용한다. 돌진 예고와 접촉 피해 영역은 정령의 그림 크기를 기준으로 설정한다. 경로는 지형을 통과한다.

정령의 보이는 영역을 게임 카메라 안으로 제한한다. 화면 경계에 닿으면 돌진을 마치고 추적한다. 카메라 이동으로 예고 위치를 보정해야 하면 표시 경로도 함께 보정하되 돌진은 취소하지 않는다. 플레이어가 예고 범위를 벗어나도 그대로 돌진한다. 실제 돌진 중 가로 화면 경계에 닿으면 종료하고 추적을 재개한다. 세로 카메라 보정은 수평 돌진을 취소하지 않는다. 예고 종료 프레임부터 Move로 돌진 이동을 시작한다. 카메라는 정령의 전체 그림이 들어갈 수 있는 크기로 설정한다.

추적과 돌진은 기존 Move 프레임만 사용하며 추가 파도 비주얼을 생성하지 않는다. 돌진 동안만 정령 몸체의 접촉 피해를 활성화한다. 피해 여부는 Wave Prefab의 Deadly 설정을 따른다. 일반 발사 파도 한 개를 재사용한다.

## 외부 조건과 모드 전환

같은 정령에 연결된 활성 WaterSpiritBehavior를 `spiritAI.RequestBehavior(nextBehavior)`로 요청한다. 반환값 true는 접수이며 즉시 전환 완료를 뜻하지 않는다. 추적 상태에서 이전 일반 파도까지 종료되면 적용한다. 대기 중에도 추적한다. 마지막 유효 요청이 우선이며 현재 모드 재요청은 보류된 요청만 취소한다.

후속 행동은 GetMoveTarget, IsReady, SelectAttack을 구현하고 필요하면 CanRush를 true로 재정의한다. Enter와 Exit에서 모드 소유 상태를 정리한다. 상승 추격 패턴은 후속 모드로 추가할 수 있다.

MapReset은 파도·예고·보류 전환을 정리하고 시작 위치를 복구한다. 사망 연출 동안 정지하고 리스폰 후 초기 모드로 재개한다. AI를 비활성화하면 기존 Auto Attack과 수동 Attack을 사용할 수 있다.

## 검증

`Validation/WaterSpiritAI/Run-Validation.ps1`로 별도 복제 프로젝트에서 Unity 테스트를 실행한다. 결과와 추적·돌진 이미지는 `ValidationArtifacts/WaterSpiritLighting`에 기록한다. 본편 지형에서의 난이도 조절은 별도로 플레이하며 확인한다.


파도가 실제 발사되는 순간 Game Camera의 CelesteRoomCamera에 약한 쉐이킹을 요청한다. Water Spirit의 **Wave Shake Duration** 기본값은 0.1초, **Wave Shake Magnitude** 기본값은 0.06 월드 유닛이다. 둘 중 하나를 0으로 설정하면 끈다. 카메라의 기존 대시·피격 쉐이킹을 함께 사용하며 더 강한 효과가 진행 중이면 기존 효과를 유지한다.



돌진 중에는 플레이어와 같은 PlayerAfterimageEffect 풀을 사용해 파란 잔상을 남긴다. 생성 간격은 0.045초, 잔상 수명은 0.2초이며 돌진 종료 후 자연스럽게 사라진다. 사망·리스폰·비활성화 시에는 즉시 정리한다. Water Spirit의 Rush Afterimage Color에서 색상과 투명도를 조정한다.

정령에는 Spirit Light라는 자식 2D Point Light가 실행 시 연결된다. 기본 푸른색 RGB=(0.15,0.55,1), Intensity=0.8, Radius=2.5이며 Water Spirit의 Spirit Light Color/Intensity/Radius로 조절한다. 주변의 Sprite-Lit 머티리얼을 사용하는 타일·스프라이트에 빛을 비춘다. 정령 이동에 따라 따라오고 정령 컴포넌트 비활성화 시 조명도 꺼진다.

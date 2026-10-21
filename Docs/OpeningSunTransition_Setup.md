# 정상·오염 태양 전환 — 2026-10-18

## 최신 상태 — 2026-10-20

Storyscene 인스턴스의 `showOrbitingSunsDuringOpening`을 껐다. 오프닝 원형 구간에서는 정상 태양과 오염 태양을 모두 숨기고 플레이어 주변 궤도로 이동시키지 않는다. `PinCorruptedSunToCamera` 호출은 유지했으므로 기존 전환 지점에서 오염 태양을 다시 켜고 화면 우측 상단 (0.86, 0.84)에 고정한다. 원형 지형/플레이어 이동 자체는 아직 남아 있으며, 태양 안에서 오염이 쫓아오는 새 오프닝 연출은 후속 작업이다.

아래의 궤도 수치와 구간은 Inspector의 `showOrbitingSunsDuringOpening`을 다시 켰을 때의 기존 설정이다.

Storyscene의 OpeningSuns에 OpeningSunTransition을 연결했다.

- 0~180도: 정상 태양이 하늘 오른쪽에서 왼쪽 화면 밖으로 천천히 이동한다.
- 180도부터: 오염된 태양이 오른쪽 화면 밖에서 들어온다.
- 270도: 오염 태양이 하늘 구도에 자리 잡고 유지된다.
- 순환하는 Transform.eulerAngles 대신 AngleRotator.ProgressAngle 누적값을 사용하므로 360도 이후 정상 태양이 반복 등장하지 않는다.
- 뒤로 걸으면 시각 이동 경로도 되감긴다. On Corruption Started는 최초 도달에만 호출한다. ResetTransition으로 이벤트를 초기화할 수 있다.
- 태양은 WorldPivot 밖의 별도 하늘 그룹이다. 지형의 자식으로 회전하지 않는다.
- 태양은 SunOrbitCenter 중심의 원 궤도를 따른다. Orbit Radius로 반지름, SunOrbitCenter 위치로 중심을 조절한다. Normal/Corrupted Start Degrees와 Sweep Degrees로 시작각과 회전량을 정한다. 0도는 오른쪽, 90도는 위다.
- AngleRotator의 끝 각도는 적어도 270도 이상이어야 한다. 기존 Storyscene 설정 720도는 유지했다.
- 기존에 비어 있던 원형 회전 대상과 플레이어 연결도 다시 연결했다.
- 지형의 원형→평지 전환, 꽃 배치, 단계별 나레이션은 이번 작업에 포함하지 않았다. On Corruption Started에서 다음 스토리 이벤트를 연결할 수 있다.

## 이미지 — 2026-10-20 통일

- 정상 태양과 과거 반사 원본은 모두 `Assets/Ember/Art/Storybook/SkinScene/Backdrop/Sun_Doodle.png`를 사용한다. `Storyscene/OpeningSuns/NormalSun`, `OpeningSuns/CorruptedSun/PastSunSource`, `StoryShoreSection/RiverMemory/PastReality_PastSunSource`를 연결했다. `Sun_Doodle`은 760 PPU로 임포트돼 있어, 정상 태양은 오염 태양과 같은 월드 지름이 되도록 균일 확대했고 반사 원본은 교체 전 월드 크기를 유지하도록 스케일을 보정했다. 위치·회전·정렬·색상과 연출 연결은 그대로다.
- 오염 상태는 `Assets/Ember/Art/Storybook/WaterDoodle_v1/CorruptedSun_Doodle_v1.png`를 계속 사용한다. 낙서 마커 선과 검보라 오염이 정상 태양의 붓결과 맞고, 기존 이글거림 재질은 오염 레이어에만 적용된다.
- 예전 `Assets/Backgrounds/Storybook/03_Sun.png`와 `WaterPPT/Scenery/PPT_HealthySun.png`는 보관 씬 백업이 참조하므로 삭제하지 않았다. 새 플레이 씬과 프리팹의 정상 태양에는 연결되지 않는다.
- 참조가 남지 않은 `Assets/Backgrounds/Storybook/04_CorruptedSun_v1.png` 및 생성 프롬프트는 현재 오염 태양으로 교체되어 정리했다.

최종 씬 화면: `Assets/ArtReference/SunCleanup/01_SunUnified.png`.

에디터 카메라 미리보기로 0도와 270도 모습을 확인하고 시작 구도로 저장했다. 플레이 모드 테스트는 수행하지 않았다.

2026-10-18 변경: 화면 좌표 보간 대신 실제 원 궤도로 변경했다. OpeningSuns/SunOrbitCenter는 그림·충돌체가 없는 빈 오브젝트이며 중심 초깃값은 WorldPivot과 같은 (0, -8), 반지름은 11.5다. OpeningSuns를 선택하면 Scene 뷰에 원 가이드가 보인다. 정상 태양은 궤도각 60→240도, 오염 태양은 진행각 180도부터 궤도각 0→90도로 이동한다. 두 태양 그림의 자체 회전은 유지한다.

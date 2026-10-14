# 시작 화면

- 씬: `Assets/Scenes/EmberMainMenu.unity`.
- 제목: **프로젝트 앰버**. 왼쪽 텍스트 메뉴 / 오른쪽 픽셀 유적·태양 배경.
- 게임 시작: 기본 목적지는 `Assets/Scenes/game 1.unity`; 짧은 암전 후 씬을 불러온다.
- 설정: 전체 음량, 전체화면, 해상도. 음량은 즉시 반영·저장되고 화면 설정은 **적용**을 눌러 저장한다.
- 나가기: 실행 파일을 종료한다. Unity 에디터에서는 Play 모드를 종료한다.
- 조작: 마우스 / 방향키 / Enter·Space·Z / 게임패드. Esc·패드 B는 설정 창을 닫는다.

## Unity에서 확인·수정

1. Project 창에서 `Assets/Scenes/EmberMainMenu.unity`를 열고 Play.
2. Hierarchy의 **Main Menu Controller → Game Scene Name**에서 시작할 씬 이름을 바꾼다. 해당 씬도 Build Profiles의 Scene List에 활성화해야 한다.
3. **Main Menu Canvas → Title and Main Menu**에서 제목, 글꼴 크기, 위치와 버튼 배치를 직접 수정한다.
4. 배경은 **Background - Generated Pixel Ruins → Raw Image → Texture**. 화면 비율이 달라지면 비율을 유지한 채 가운데를 기준으로 잘라 채운다.
5. **Settings Overlay**는 처음에는 비활성화되어 있다. 편집할 때만 켜고, 저장 전 다시 끈다.
6. 화면의 전체화면/해상도 전환은 빌드한 게임에서 확인한다. 에디터에서는 값을 저장하지만 에디터 창 크기는 바꾸지 않는다.

## 빌드 목록

시작 씬을 첫 번째로 넣고 `game 1`도 활성화한다. 기존 `EmberPrototype`, `SampleScene` 항목은 보존한다. 별도 Build Profile에 Scene List Override를 사용 중이라면 그 목록에도 시작 씬을 첫 번째로 추가해야 한다.

## 애셋

- 생성 배경: `Assets/image/ProjectEmber_TitleBackground.png`; [사용한 프롬프트](../ArtReference/ProjectEmber_TitleBackground_Prompt.md). 내장 이미지 생성 사용.
- 케니 장식: 기존 `Assets/kenney`의 divider-003 / panel-transparent-center-013을 복사해 사용. 원본·원본 임포트 설정은 변경하지 않음. CC0 고지는 `Assets/EmberPrototype/UI/MainMenu/Kenney-LICENSE.txt`.
- 한글 글꼴: [Noto Serif CJK KR](https://github.com/notofonts/noto-cjk), SIL OFL 1.1. 원본 OTF 및 `OFL.txt` 동봉.
- 새 UI는 [Input System UI 모듈](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/UISupport.html)을 사용한다. 게임 플레이 입력 애셋은 변경하지 않는다.

## 검증

`Validation/MainMenu/Run-Validation.ps1`이 임시 독립 Unity 프로젝트에서 씬 생성·렌더링을 검사한다. 반환된 QAProject를 사용해 `-Mode Test -QAProject <경로>`로 PlayMode 회귀 테스트를 실행할 수 있다. 작업 중인 게임 씬을 열거나 저장하지 않는다. 실제 게임 씬 전체 플레이, 실제 해상도 전환 및 실행 파일 종료는 빌드에서 별도로 확인해야 한다.

2026-10-08: 메뉴 PlayMode 테스트 **9/9 통과**, 한글 글리프 및 16:9·4:3·설정 창 렌더 확인 완료. 씬 로드 검사는 QA 전용 빈 씬으로 수행했다.

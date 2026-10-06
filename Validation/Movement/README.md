# Movement regression harness

This harness runs the current Ember runtime sources in an isolated Unity 6000.5.2f1 project. It does not add tests, packages, assembly definitions, or project settings to the production Assets folder. The original project's generated solution and Library are not modified by the runner.

Run from PowerShell:

```powershell
& './Validation/Movement/Run-IsolatedRegression.ps1'
```

The runner returns immediately after starting a hidden Unity process. Its PID and output paths are in `ValidationArtifacts/MovementFeel/run.json`. Wait for that process to finish, then read `UnityResults.xml`. Unity Test Runner exits on completion; the user's original Unity editor remains open.

The QA project and its caches live in `%LOCALAPPDATA%/Temp/EmberMovementQA-20261004`. It contains unchanged copies of the 26 runtime C# sources and their metadata, the existing package manifest/lock, four existing ProjectSettings files, and the test fixture. It deliberately excludes production scenes and asset-generation Editor scripts. Runtime and production Editor C# compilation is verified separately using the existing generated Editor project.

The tests inject real KeyboardState events and assert their held, press, and release states before invoking the real controller/router Update methods. Input System 1.19's test option `runPlayerUpdatesInEditMode` is enabled only during each fixture and restored afterward so player press edges are available in the EditMode test context. The tests manually invoke FixedUpdate and use native Physics2D.Simulate at 0.02 seconds; the movement implementation is not duplicated in the tests.

Fixture movement and body settings match scene 2's current move 3.3, jump 6.09, gravity 1.8, air deceleration 28, apex threshold 1, apex gravity .65, fall gravity 1.6, launch control lock .12, dash exit control lock .02, and box dimensions/offset. The small test dash circle has radius .15 to construct reproducible gap cases. These geometry tests check the collider restoration contract rather than claim that scene 2's authored geometry contains that exact gap.

Coverage includes diagonal walking, simultaneous C+Z, blocked-dash chord consumption, jump input near dash end, jump release during the ring, actual X travel to an anchor followed by Z launch, first-tick movement after shot release, boost braking for forward/released/opposite input, ground-prioritized landing jump buffering, immediate midair jump, preserved dash direction, and safe full-body collider restoration.

This proves isolated scripted input/physics behavior. It does not prove scene 2 visual quality, its whole route, a standalone player build, human movement feel, or performance. Source hashes in `source-snapshot.json` establish exactly which production runtime revision was copied.

## Scene 2 flat-floor sticking regression

The original 17 tests used generic box floors and did not cover TilemapCollider tile boundaries. Four separate tests now import extracted native scene 2 terrain before and after the Static Rigidbody/Composite fix. The current exports retain the original Grid/Tilemap documents, all 751 cells, tile GUIDs, collider settings and transforms. They load the original RuleTile asset and atlas PNG/meta. Other gameplay objects are excluded. The earlier 753-cell validation and its exact fixture sources are preserved separately at `ValidationArtifacts/MovementFeel/TilemapSticking-753Cells`.

```powershell
& './Validation/Movement/Prepare-TilemapRegression.ps1'
# After the QA process recorded in run.json exits:
& './Validation/Movement/Collect-TilemapEvidence.ps1'
```

This runs only the four methods whose names begin with `Tilemap`, leaving the original 17-test result intact. The preserved pre-fix scene must exist at `ValidationArtifacts/MovementFeel/TilemapSticking/scene2-before.unity.txt`. Results and per-tick telemetry are stored in that same TilemapSticking directory. The active production editor and scene remain untouched.

The tests check native scene import/composite linkage and geometry, reproduce the old snag while walking across the actual long flat floor at cell row -7, verify walking after landing, and confirm that the authored raised wall at cell -14 still stops a dash and restores the body collider. The baseline assertions require a short blocked displacement, stalled physics ticks and horizontal contacts; the corrected terrain must have no horizontal contacts or stalled ticks on the flat path. Extrusion 0 and .01 are compared without changing friction, materials, player shape or movement code.

The separate simulation covers the actual terrain and collision setup, not a full visual playthrough, dynamic obstacles, every route, or a player build. The missing-Composite inspector warning condition is recorded through native collider/composite linkage; the headless runner does not render the original Inspector warning UI.

## 로프 회귀와 실제 Play Mode 검증

`RopeTests.cs`는 기존 입력/물리 검사와 함께 진입 속도 전달, 이동 목표 추격, 박차기, 공중 기세, 벽 충돌, 초기화, 줄 표현의 끝 고정과 휘어짐 상한을 검사한다. `RopeScenePlayTests.cs`는 별도 검증 씬을 생성하고 실제 Play Mode에 진입해 정상 Update/FixedUpdate 및 물리를 사용한다. 입력만 합성하며 컨트롤러 메서드를 직접 호출해서 로프 연결을 대신하지 않는다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ./Validation/Movement/Run-IsolatedRegression.ps1 -UnityEditor 'C:/Program Files/Unity/Hub/Editor/6000.5.3f1/Editor/Unity.exe' -QaProject "$env:LOCALAPPDATA/Temp/EmberRopeFinalQA-20261006" -TestFilter 'EmberMovementRegression.InputAndPhysicsTests.(?!Tilemap)|EmberMovementRegression.RopeScenePlayTests' -ArtifactSubdirectory 'Rope/VerifiedVisual' -EnableGraphics
```

`EnableGraphics`는 QA 복사본에 그래픽/품질 설정과 렌더링 설정 에셋을 복사하고 화면 캡처를 허용한다. Play Mode 검사는 숨겨진 QA 창에서 키보드 입력이 게임에 전달되도록 QA 프로세스의 입력 포커스와 프레임 시간을 일시 조정한다. 캡처 중 GPU 부하와 관계없이 프레임 시간이 1/60초가 되도록 설정하고 종료 시 복원한다. 원본 프로젝트 설정은 변경하지 않는다. 캡처는 QA 루트의 `RopeEvidence`에 생성된다. 검증 씬 생성 메뉴는 기존 파일을 덮어쓰지 않는다. 씬 생성기 변경 후 검증하려면 새로운 QA 폴더를 지정한다.

최종 결과는 `ValidationArtifacts/MovementFeel/Rope/VerifiedVisual`에 있다. QA에서 생성한 `RopeValidationVisual.unity`를 같은 내용과 GUID로 원본의 `RopeValidation.unity`에 복사했다. 렌더링 캡처도 해당 결과 폴더의 `Captures`에 보존했다.

타일맵 네 개는 이 실행에서 제외하며 기존 별도 검증을 사용한다. 이 결과는 연속 로프 예제와 관련 회귀의 확인이며 전체 게임 실행 파일 빌드나 모든 방의 플레이 완료를 의미하지 않는다.

The tests never force Composite.GenerateGeometry. Native OpenScene and ProcessTilemapChanges geometry counts are recorded separately to verify automatic Synchronous generation. Composite paths are geometric contours, not collider components or physics shapes; consult the latest `after-geometry.json` for the current 751-cell counts. The archived 753-cell run had five paths, 78 points and 35 physics shapes.

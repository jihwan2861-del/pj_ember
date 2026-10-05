# 앰버 방 제작 가이드

작성일: 2026-10-05

대상: `project Ember`의 Unity 2D 씬. 기존 `2.unity`에 방을 추가하고, 카메라 범위와 타일맵을 정한 뒤 이동 경로를 붙이는 작업 순서다.

처음에는 **카메라 범위 → 바닥 → 출발점 → 가시 → 기믹** 순서로 한 방만 만든다. 새 제작 도구는 빈 방의 구조를 만들어 준다. 기존 플레이어와 Main Camera를 바꾸거나 이동시키지 않으며, 씬을 자동 저장하지 않는다.

## 새 방의 Hierarchy

`Tools > Ember > Level Workflow`에서 방을 만들면 아래 구조를 사용한다.

```text
Room_01                         방 전체를 이동할 부모 / Scale 1
├─ CameraBounds                 CameraRoom / 카메라가 보여줄 플레이 공간
├─ Grid                         Grid / Scale X=0.5, Y=0.5
│  ├─ Terrain                   바닥·벽 / Tilemap + 합성된 고체 충돌
│  ├─ Spikes                    가시 / Tilemap + Trigger + RoomHazard
│  └─ Decorations               배경·풀·장식 / Tilemap, 충돌 없음
├─ Mechanics                    횃불·문·움직이는 블록
├─ Paths                        MovementPath와 경로 점
└─ Spawn                        RoomSpawnPoint / 출발·체크포인트

player                          기존 플레이어를 계속 사용
Main Camera                     기존 카메라를 계속 사용
```

`Mechanics`와 `Paths`는 정리용 부모다. 그 아래에 실제 기믹이나 경로를 추가해야 한다. `Spawn`은 해당 위치에 들어온 플레이어의 체크포인트를 바꾸는 영역이며, 생성만으로 플레이어가 그곳에서 시작하는 것은 아니다.

## 첫 방을 만드는 10단계

### 1. 편집할 씬을 연다

Project에서 `Assets/Scenes/2.unity`를 연다. Play 버튼이 꺼져 있는지 확인한다. Play Mode에서 한 편집은 플레이를 종료하면 사라질 수 있다.

Scene 창은 `2D`와 `Gizmos`를 켠다. 기존 방과 플레이어는 우선 그대로 두고 옆의 빈 공간에 새 방을 만든다.

### 2. 제작 창으로 빈 방을 만든다

`Tools > Ember > Level Workflow`를 열면 **Ember 방 만들기** 창이 나온다. `1. 빈 방 만들기`에서 다음 값으로 시작한다.

| 제작 창 필드 | 첫 테스트 예시 |
|---|---|
| 방 이름 | Room_01 |
| 방 중심 (월드) | X=26, Y=0 — 현재 2씬 맵 오른쪽의 빈 공간 |
| 사용할 카메라 | 기존 Main Camera |
| 카메라 경계 크기 (월드) | **카메라 한 화면 크기를 생성값에 사용** 버튼으로 입력 |
| 타일 한 칸 (월드) | 0.5 |

**새 방 생성 (Undo 가능)**을 누른다. 이름은 나중에 Hierarchy에서 바꿀 수 있다. 기존 Scene 맵을 더 확장한 상태라면 방 중심을 그 맵과 겹치지 않는 빈 공간으로 정한다.

생성 후 `Room_01`의 자식이 위 Hierarchy와 같은지 확인한다. 생성 작업은 Undo로 되돌릴 수 있으며, 씬 저장은 나중에 직접 한다.

### 3. 카메라가 보여줄 방 범위를 먼저 정한다

Hierarchy에서 `Room_01 > CameraBounds`를 선택한다. Scene 창의 사각 범위를 가장자리 핸들로 늘이거나 줄인다. 사각형은 **카메라 중심이 이동하는 영역이 아니라 방의 플레이 공간**이다. 바닥, 천장, 출구를 그 사각형 안에 설계한다.

처음에는 Inspector의 **카메라 한 화면 크기로 맞추기** 기능으로 시작한다. 이 기능은 선택한 범위의 중심을 유지하며 Main Camera 한 화면 크기로 맞춘다. 한 화면짜리 방에서는 카메라가 거의 고정되고, 방을 더 넓히면 플레이어를 따라 스크롤한다.

`Main Camera > Camera > Size`가 5이고 Game 화면이 16:9라면 한 화면은 약 **17.78 U × 10 U**다. 0.5 U 타일 기준으로 약 36칸 × 20칸을 덮는다. Game 창의 종횡비는 가능하면 제작 내내 16:9로 고정한다.

### 4. Terrain을 선택하고 바닥을 그린다

1. Hierarchy에서 **새 방의** `Room_01 > Grid > Terrain`을 선택한다.
2. 제작 창의 **Tile Palette 열기** 또는 `Window > 2D > Tile Palette`로 팔레트를 연다.
3. **Active Palette**에서 `Mossy Terrain Palette`를 선택한다.
4. Tile Palette의 **Active Target**을 방금 선택한 `Terrain`으로 지정한다. 같은 이름의 Terrain이 여러 개면 Hierarchy의 방 이름까지 확인한다.
5. 팔레트 자체를 편집하는 **Edit Tile Palette** 상태를 끄고 브러시를 선택한다.
6. 팔레트 위쪽 3×3 샘플 또는 그 옆의 단독 자동 연결 타일을 골라 **Scene 창**에 그린다. 아래 7×7은 수동 타일 모음이다.

먼저 길게 평평한 바닥 하나와 출구용 발판 하나를 만든다. 그리기 시작할 때 한 칸만 찍고 Terrain의 타일 수가 늘어나는지 확인하면 다른 방이나 가시 레이어에 잘못 그리는 일을 줄일 수 있다.

### 5. 출발점을 놓고 기존 플레이어를 옮긴다

`Room_01 > Spawn`을 새 바닥 위로 옮긴다. Spawn의 중심은 재시작 때 플레이어의 중심이 놓일 위치다. 플레이어의 본문 BoxCollider가 바닥이나 벽과 겹치지 않게 충분히 띄운다.

첫 테스트를 위해 Hierarchy의 기존 `player`를 선택해 새 Spawn 위치로 직접 이동한다. 기존 플레이어를 복제할 필요는 없다. 그 플레이어의 `Flame Player Controller > Respawn Point`에 새 `Spawn`을 드래그하면 위험물에 닿았을 때 그곳으로 돌아온다.

플레이어에 `PrototypeRoom`도 붙어 있다면 그 컴포넌트의 `Spawn Point` 역시 새 Spawn으로 맞춘다. `Death Height`는 새 방보다 낮게 정한다. 새 방을 높은 곳이나 깊은 곳에 만들 때 이전 Death Height를 그대로 쓰면 시작하자마자 재시작될 수 있다.

### 6. 카메라와 새 방을 연결해 확인한다

제작 창의 `카메라 경계`에 새 CameraBounds가 선택됐는지 확인하고 **카메라를 이 방에 연결**을 누른다. 기존 Main Camera의 CelesteRoomCamera에 시작 방과 플레이어 참조를 연결한다. 이 버튼도 플레이어를 Spawn으로 이동시키지는 않는다.

Inspector에서 직접 연결하려면 `Main Camera > Celeste Room Camera`의 `Target`에 기존 `player`, `Starting Room`에 새 `CameraBounds`를 드래그한다. 이미 Target이 올바르면 그대로 사용한다. 플레이어가 여러 개라면 원하는 플레이어를 Target으로 직접 지정한다.

`CameraBounds`의 사각형이 플레이어의 시작 위치를 포함해야 한다. Play를 눌러 바닥을 왕복하며 방 가장자리에서 카메라가 멈추는지 확인한 뒤 Play를 종료한다. Main Camera의 Transform을 매번 직접 이동할 필요는 없다.

### 7. Spikes에만 위험물을 그린다

Hierarchy에서 `Room_01 > Grid > Spikes`를 선택하고 Tile Palette의 **Active Target**도 `Spikes`로 바꾼다. Terrain과 같은 팔레트에 있는 타일이라고 해서 같은 레이어에 그릴 필요는 없다.

현재 프로젝트에는 이 워크플로 전용 가시 Sprite/Tile 에셋이 준비되어 있지 않다. 가시 그림을 준비한 뒤, Sprite를 Tile Palette에 넣어 가시 Tile을 만들고 `Collider Type`을 **Sprite**로 설정하는 흐름을 사용한다. Sprite Editor의 **Custom Physics Shape**에서 눈에 보이는 가시 부분보다 조금 안쪽에 위험 판정을 그린다. 셀 전체를 죽음 판정으로 만들 필요가 없다.

가시 준비 전에는 [기존 Hazard 프리팹](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/EmberPrototype/Prefabs/Hazard.prefab>)을 `Mechanics` 아래에 놓고 작은 위험 영역을 테스트할 수 있다.

**Mossy Terrain 타일을 Spikes에 그리면 해당 타일은 Grid 충돌이므로 셀 전체가 죽음 판정이 된다.** 가시 모양으로 자동 변환되지 않는다. Terrain 위에 가시를 얹을 때는 Terrain이 고체 바닥, Spikes가 별도 Trigger라는 구분을 유지한다.

### 8. 횃불과 문을 Mechanics 아래에 놓는다

점프만으로 방을 건널 수 있는지 확인한 뒤 횃불을 추가한다. 기존 [FlammableTile 프리팹](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/EmberPrototype/Prefabs/FlammableTile.prefab>)을 필요에 맞춰 `Mechanics` 아래에 놓는다. 이미 만든 횃불 오브젝트를 복제해 시각적 스타일을 맞춰도 된다.

플레이어가 어디에서 점화하고, 어디로 흡수되고, 어느 방향으로 발사해야 하는지 순서대로 테스트한다. 문과 버튼 등은 Inspector에서 요구하는 참조를 각각 연결한다. 부모 이름이 Mechanics라는 이유만으로 기믹들이 자동 연결되는 것은 아니다.

### 9. 필요한 경우 경로와 움직이는 블록을 만든다

`Paths` 아래에 빈 `Path_01`을 만들고 `MovementPath` 컴포넌트를 추가한다. **경로 모양 (Shape) = Linear**로 두고 **끝에 점 추가**를 두 번 누른다. 생성된 `Point 1`, `Point 2`를 Scene 창의 핸들로 옮긴다.

`Mechanics` 아래에 블록 그림을 가진 오브젝트를 놓고 `BoxCollider2D`와 `PathMovingBlock`을 추가한다. PathMovingBlock이 필요한 Rigidbody2D를 추가하고 Play 때 Kinematic으로 설정한다. 블록 Inspector의 **이동 경로**에 방금 만든 `Path_01`을 드래그한다. 경로의 자식으로 블록을 넣지 않는다.

첫 테스트는 **플레이 시작 시 자동 출발**을 켜고 **이동 방식 = Once**로 시작한다. 이 방식은 플레이를 시작하면 경로의 끝까지 한 번 움직이고 멈춘다. 점 순서나 속도를 확인한 뒤 **PingPong**으로 왕복시키거나, 닫힌 경로를 만들고 **Loop**로 바꾼다. 각 옵션과 필드는 아래 경로 설명을 참고한다.

### 10. 짧게 플레이하고 편집 모드에서 저장한다

다음 순서로 확인한다.

- 바닥을 천천히 좌우로 걸어도 타일 경계에 걸리지 않는가?
- 점프의 출발점과 착지 지점이 화면 안에서 보이는가?
- 가시의 그림 바깥을 스쳤는데 죽거나, 그림을 통과해도 살지 않는가?
- 죽은 뒤 새 Spawn에서 정상 크기와 정상 충돌로 돌아오는가?
- 움직이는 블록이 지형을 뚫거나 플레이어를 벽에 끼우지 않는가?
- 출구로 나가면 다음 방의 범위로 카메라가 넘어가는가?

제작 창의 **방 구성 검사**도 눌러 컴포넌트와 Spawn 위치를 확인한다. 이 검사는 기본 연결 상태를 점검하며 방을 실제로 통과할 수 있는지까지 플레이하지는 않는다.

Play를 종료하고 `Ctrl+S`로 씬을 저장한다. 생성 도구는 방을 만든 시점이나 플레이를 시작한 시점에 씬을 자동 저장하지 않는다.

## 카메라 범위와 Inspector 필드

### CameraBounds의 Camera Room

| Inspector 필드 | 의미 | 처음 설정할 때 |
|---|---|---|
| Size | 방의 가로·세로 크기, 월드 단위 | 한 화면 맞추기 후 필요한 방향으로 늘린다 |
| Center Offset | CameraBounds Transform에서 방 중심까지의 월드 XY 오프셋 | 0, 0으로 시작 |
| Camera Offset | 이 방 안에서 플레이어를 따라갈 때 더하는 시선 오프셋 | 0, 0으로 시작 |
| Preview Color | Scene 창 사각형의 색 | 방별로 구분할 때 사용 |

예를 들어 방의 X 범위가 0~20이고 화면 폭이 16이라면 카메라 중심은 X=8~12 안에서만 움직인다. 화면 가장자리가 방 밖으로 나가지 않도록 화면의 절반 크기만큼 안쪽에서 중심을 제한하는 것이다.

한 방향의 방 크기가 화면보다 작으면 그 방향의 카메라 중심은 방의 중심에 고정된다. 카메라가 자동으로 확대되는 기능은 아니므로 작은 방 주변의 배경도 준비해야 한다.

`Camera Room`의 Size와 Center Offset은 **월드 단위**다. CameraBounds나 부모의 Scale을 바꿔 사각형 크기를 조절하지 말고 Size 또는 Scene 핸들을 사용한다. Rotation으로 사각형을 회전시키는 기능도 아니다. 부모 Scale을 바꾸면 자식의 월드 위치는 달라질 수 있지만 Size가 함께 확대되지는 않는다. 방 부모 `Room_XX`의 Scale은 1로 유지하는 편이 안전하다.

**타일맵 범위로 맞추기**는 그려진 타일의 전체 직사각 범위에 맞춘다. 자동으로 점프할 빈 공간이나 출구 여백을 추가하지 않는다. 바닥 한 줄만 그린 상태에서 누르면 범위가 지나치게 낮아질 수 있으므로, 완성된 지형을 바탕으로 맞춘 뒤 핸들로 여백을 보충한다. 기본 동작은 해당 방 아래의 그려진 Tilemap들을 합친 범위이며 장식도 포함한다. Terrain만 기준으로 삼으려면 CameraBounds Inspector의 **범위 기준 타일맵 (선택)**에 Terrain을 지정한다.

제작 창의 **경계 선택**은 CameraBounds를 선택하고, **방 화면으로 보기**는 Scene 창을 그 방에 맞춰 보여준다. Game 카메라를 이동시키는 버튼은 아니다. **사용할 카메라 선택**을 누르면 Main Camera의 Inspector를 바로 확인할 수 있다.

범위를 기계적으로 확대해서 다른 방과 크게 겹치게 만들지 않는다. 카메라는 현재 방 안에 있는 동안 그 방을 계속 사용한다. 현재 방을 벗어난 뒤 다른 범위를 찾으며, 여러 범위가 겹치면 가장 작은 범위를 우선한다. 방 경계와 출구가 이어지게 설계하면 전환 위치를 예측하기 쉽다.

### Main Camera의 Celeste Room Camera

| Inspector 필드 | 의미 |
|---|---|
| Target | 따라갈 기존 플레이어 Transform |
| Starting Room | 플레이 시작 시 사용할 CameraRoom |
| Global Camera Offset | 모든 방에서 플레이어에 더하는 시선 오프셋 |
| Remaining Distance After One Second | 따라가기의 부드러움. 작은 값일수록 더 빠르게 목표에 가까워짐 |
| Snap To Target On Start | 시작 시 목표 카메라 위치로 바로 배치 |

한 화면의 높이는 `Camera.Size × 2`, 폭은 `높이 × Game 화면 종횡비`다. 예를 들어 Size=5, 16:9에서 높이 10 U와 폭 약 17.78 U가 된다. 화면을 더 넓게 보려면 Main Camera의 Size를 늘린다. 방의 경계를 넓히는 것은 스크롤할 수 있는 공간을 늘리는 동작이다.

기존 `CameraPositionGrid`는 카메라 위치 후보를 시각화하는 별도 도구이며 새 `CameraRoom`의 경계를 대신하지 않는다. 현재 `CameraShiftTrigger`가 호출하는 화면 단위 Shift 함수는 기존 직렬화 호환용으로 남아 있어 새 방 카메라 이동 방식으로 사용하지 않는다. 새 방은 **CameraRoom의 범위와 플레이어 위치**를 기준으로 전환한다.

## 타일 크기와 레이어 충돌

### 새 방과 기존 2씬의 Scale 차이

| 구조 | Grid Scale | Tilemap Scale | 월드에서 한 셀 |
|---|---:|---:|---:|
| 기존 2씬의 Grid > Tilemap | 1 | 0.5 | 0.5 U |
| 새 Room_XX > Grid > Terrain 등 | 0.5 | 1 | 0.5 U |

둘 다 Grid Cell Size는 1×1이다. 새 템플릿은 Grid가 세 Tilemap의 공통 크기를 맡는다. 기존 Tilemap을 새 Grid 아래로 옮길 때 자식 Scale도 0.5로 남겨 두면 한 셀이 **0.25 U**로 줄어든다. 복사할 때는 자식 Tilemap의 Local Scale을 1,1,1로 맞춘다.

현재 Mossy atlas는 3584×3584 이미지이고, 49개 Sprite가 각각 512×512 픽셀, PPU=512로 나뉘어 있다. Sprite 하나의 기본 그림 크기는 1 U이며 새 Grid Scale 0.5를 받아 0.5 U로 보인다. `Grid` 타입 타일의 충돌 크기는 Sprite의 불투명한 외곽이 아니라 셀을 기준으로 한다.

플레이어 이미지의 PPU와 타일 이미지의 PPU는 별도 설정이다. 플레이어 PPU를 보고 Grid 크기를 다시 바꾸지 않는다. 다른 타일셋을 추가할 때는 `Sprite 픽셀 크기 ÷ PPU`로 그림 크기를 확인하고 1셀 크기에 맞춘다.

### 레이어별 확인할 컴포넌트

| Tilemap | 필요한 구성 | 충돌 목적 |
|---|---|---|
| Terrain | Tilemap Collider 2D + Static Rigidbody 2D + Composite Collider 2D | 바닥과 벽, 플레이어를 받치는 고체 |
| Spikes | Tilemap Collider 2D의 Is Trigger + RoomHazard | 닿으면 체크포인트로 재시작 |
| Decorations | Tilemap + Tilemap Renderer | 충돌 없음 |

Terrain에서는 `Tilemap Collider 2D > Composite Operation = Merge`, `Rigidbody 2D > Body Type = Static`, `Composite Collider 2D > Geometry Type = Polygons`, `Generation Type = Synchronous`를 사용한다. `Composite Operation`을 Merge로 바꾸기만 하고 Composite Collider를 추가하지 않으면 결합되지 않는다.

Spikes는 Terrain과 같은 Composite에 합치지 않는다. 가시의 `Tile Collider Type = Sprite`와 실제 Sprite Physics Shape를 별도로 준비한다. 현재 RoomHazard는 TilemapCollider2D를 직접 사용할 수 있으며, 원점의 보조 BoxCollider를 함께 추가할 필요가 없다.

기본 RuleTile의 이웃 판정은 **같은 Tilemap 안의 타일**을 확인한다. Terrain을 그렸다고 Spikes나 Decorations가 그 지형을 보고 자동으로 가시 방향·풀 위치를 정하지는 않는다. 현재는 Terrain의 자동 연결을 사용하고, 가시 방향과 장식 위치는 별도로 배치한다. 서로 다른 Tilemap을 참조하는 자동 배치는 추가 편집 도구가 필요한 기능이다.

Decorations에 Terrain Tile을 그려도 TilemapCollider가 없으면 충돌은 생기지 않는다. 바닥으로 쓰려는 타일을 장식 레이어에 잘못 그렸다면 플레이어가 통과하므로 Active Target부터 확인한다.

Unity 6의 대상 드롭다운 이름은 **Active Target**이다. 오래된 설명에서 보이는 Active Tilemap은 같은 대상을 가리키던 이전 명칭이다. [Unity 공식 Tile Palette 환경 설정 설명](https://docs.unity3d.com/kr/6000.0/Manual/tilemaps/tile-palettes/tile-palette-preferences-reference.html)

## 이동 경로와 움직이는 블록

MovementPath는 경로를 정의하고 PathMovingBlock은 그 경로를 따라 실제 물체를 움직인다. 경로 오브젝트에 Sprite나 Collider를 붙일 필요는 없다. 움직이는 블록은 SpriteRenderer와 고체 Collider가 필요하다.

Linear와 Smooth는 `Paths > Path_01 > Point 1, Point 2…`의 점을 순서대로 연결한다. **직계 자식 점 자동 수집**을 켜면 경로 바로 아래의 자식 순서가 이동 순서가 된다. 다른 장식이나 블록을 경로 점의 자식 목록에 섞지 않는다. Scene 핸들을 움직이면 점 위치가 변하고, Hierarchy 순서를 바꾸면 방문 순서가 변한다. `끝에 점 추가`, `마지막 점 삭제`, `자식 점 다시 수집`으로 편집하며 위치 변경과 점 편집은 Ctrl+Z로 되돌릴 수 있다.

| 경로 모양 | 사용할 때 | 주의할 점 |
|---|---|---|
| Linear | 두 지점 사이를 오가는 발판, 꺾인 경로 | 코너에서 방향이 바로 바뀜 |
| Smooth | 부드러운 곡선 이동 | 곡선이 점 사이의 직선보다 바깥으로 휘어 벽을 통과할 수 있음 |
| Circle | 한 중심 주변을 도는 원형 경로 | 자식 점 없이 중심·반지름·시작 각도 사용 |

### 경로 Inspector 필드

새 편집기는 한글로 표시하므로 기존 설명의 영문 필드와 아래처럼 대응한다.

| Inspector 표시 | 영문 대응 | 의미 |
|---|---|---|
| 경로 모양 (Shape) | Shape | Linear / Smooth / Circle |
| 닫힌 경로 | Closed | Linear·Smooth의 마지막 점과 첫 점을 경로로 연결 |
| 직계 자식 점 자동 수집 | Auto Collect Child Points | 바로 아래 자식들을 Hierarchy 순서로 수집 |
| 경유점 (순서대로) | Points | 방문할 점의 목록. 자동 수집을 끄면 직접 지정 |
| 곡선 구간 샘플 수 | Smooth Samples Per Segment | Smooth 곡선을 계산하는 정밀도, 기본 12 |
| 경로 선 색상 | Path Color | Scene 창 경로 선의 표시 색 |
| 중심 오프셋 (로컬) | Circle Center | Circle의 중심. Path Transform 기준 로컬 오프셋 |
| 반지름 (월드 단위) | Circle Radius | Circle 반지름. 부모 Scale에 따라 커지지 않음 |
| 시작 각도 (도) | Circle Start Angle | Circle의 시작 위치. 0도=오른쪽, 90도=위 |
| 시계 방향 | Circle Clockwise | 체크 시 시계 방향, 해제 시 반시계 방향 |

Circle은 자식 점이나 Smooth 샘플 수를 사용하지 않는다. Scene 창의 **원 중심** 핸들로 중심을 이동하고 **시작점** 핸들을 움직여 반지름과 시작 각도를 함께 바꾼다. 원의 반지름은 월드 단위지만 중심 오프셋은 로컬 좌표이므로 부모 위치·Scale을 바꾸면 중심 위치는 달라질 수 있다.

Smooth는 경유점을 통과하는 곡선이다. 점이 벽 바로 옆에 있으면 곡선이 코너 밖으로 휘어 벽을 통과할 수 있다. 점 위치뿐 아니라 표시되는 경로 전체가 안전한지 확인한다. Smooth에서 `닫힌 경로`를 켜면 마지막 점에서 첫 점으로도 곡선이 이어진다.

### 블록의 이동 방식

| 이동 방식 | 동작 |
|---|---|
| Once | 시작점에서 끝점까지 이동한 뒤 멈춤 |
| PingPong | 양 끝에서 방향을 바꾸고 돌아오는 동작을 반복 |
| Loop | Circle 또는 Closed 경로를 같은 방향으로 계속 순환 |

열린 Linear·Smooth 경로에 Loop를 선택하면 Inspector에 경고가 나오고 이동을 시작하지 않는다. 두 점 사이를 오가려면 PingPong을 사용한다. 순환 경로가 필요하면 MovementPath의 `닫힌 경로`를 켜거나 Circle을 선택한다. Circle + Once는 한 바퀴 돈 뒤 시작 위치에서 멈추는 조합이다.

| Inspector 표시 | 영문 대응 | 의미 |
|---|---|---|
| 이동 경로 | Path | 사용할 MovementPath |
| 이동 속도 (월드 단위/초) | Move Speed | 실제 이동 속도. 처음에는 2.5로 확인 |
| 이동 방식 | Travel Mode | Once / PingPong / Loop |
| 끝점 대기 시간 (초) | End Wait Time | PingPong 양 끝 또는 Loop 한 바퀴 끝의 대기 시간 |
| 시작 시 경로 첫 점에 배치 | Snap To Path Start | Play 시작 시 경로의 시작 위치에 배치 |
| 플레이 시작 시 자동 출발 | Activate On Start | Play 시작 때 곧바로 움직임 |
| 끝점 도착 이벤트 | On Arrived | 끝점 도착 시 연결한 이벤트 실행 |

Once는 끝에서 바로 멈추므로 끝점 대기 시간을 별도로 사용하지 않는다. On Arrived는 Once에서 한 번, 반복 모드에서는 각 끝점·각 바퀴 도착 때 실행된다. 버튼·문을 연결할 때 반복 실행을 원하는지 확인한다.

`플레이 시작 시 자동 출발`이 꺼져 있으면 문·버튼 등의 UnityEvent에서 블록 오브젝트를 지정하고 `PathMovingBlock > Activate()`를 선택해 연결해야 움직인다. Once로 끝까지 간 블록을 다시 출발시키려면 먼저 `ResetBlock()`으로 초기화한다.

`경로 미리보기 (고스트)`와 `블록 미리보기 (고스트)` 슬라이더를 움직이면 Scene 창에 예상 위치만 표시한다. 실제 블록이나 플레이어를 움직이지 않는다. 블록 Inspector의 `경로 선택` 버튼으로 연결된 Path의 점을 바로 편집할 수 있다.

현재 움직이는 블록은 Kinematic Rigidbody2D의 이동을 사용한다. `PathMovingBlock`이 매 물리 프레임의 이동 속도를 제공하고, `FlamePlayerController`가 발판 위에서 플레이어를 함께 운반한다. 별도 운반 컴포넌트나 부모 연결은 필요 없다. 가만히 서 있기, 발판 위에서 걷기, 원형 이동과 끝점 전환을 지원하며 점프하면 운반이 해제된다. 발판 속도를 점프에 추가하는 관성 전달과 압사 처리는 아직 추가하지 않았다. 발판과 주변 벽·천장 사이의 간격은 직접 플레이하며 확인한다.

`PrototypeRoom.Restart()`는 현재 씬의 불과 투사체를 초기화하지만 모든 움직이는 블록을 자동으로 `ResetBlock()` 하지는 않는다. 방 재시작 때 경로 기믹도 초기화해야 한다면 그 연결을 추가해야 한다.

## 자주 막히는 부분

| 증상 | 먼저 볼 곳 |
|---|---|
| 새 방을 만들었는데 예전 방에서 플레이됨 | 기존 player를 새 Spawn으로 옮겼는지, Respawn Point가 새 Spawn인지 |
| 카메라가 자유롭게 따라가며 방 밖까지 보임 | player가 CameraBounds 사각형 안에 있는지, Gizmos가 켜졌는지 |
| CameraBounds Scale을 바꿔도 범위가 같음 | Size 또는 Scene 사각 핸들로 수정 |
| 새 방이 너무 작게 그려짐 | Grid 0.5와 Tilemap 0.5가 동시에 적용되지 않았는지 |
| Terrain 대신 다른 방에 타일이 찍힘 | Hierarchy의 선택과 Tile Palette Active Target이 같은 방인지 |
| 바닥이 보이는데 통과함 | Terrain에 그렸는지, Collider와 Static Rigidbody가 있는지 |
| Merge를 켰는데 경고가 남음 | 같은 Terrain에 Composite Collider 2D가 실제로 붙어 있는지 |
| 가시보다 멀리서 죽음 | Spike Tile이 Grid 타입인지, Sprite Physics Shape가 넓은지 |
| 경로가 보이는데 블록은 정지함 | 이동 경로 참조, 최소 점 개수 또는 Circle 반지름, 플레이 시작 시 자동 출발, 열린 Loop 경고 |
| Play를 끄면 배치가 원래대로 돌아감 | 편집 모드에서 배치했는지, 편집 종료 후 Ctrl+S했는지 |

## 관련 파일

- [20분 상세기획](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Docs/Design/Ember_20Minute_Design_v1.md>)
- [방 제작 창](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/EmberPrototype/Editor/EmberLevelWorkflowWindow.cs>) / [카메라 범위 편집기](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/EmberPrototype/Editor/CameraRoomEditor.cs>)
- [CameraRoom](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/EmberPrototype/Runtime/CameraRoom.cs>) / [CelesteRoomCamera](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/EmberPrototype/Runtime/CelesteRoomCamera.cs>)
- [MovementPath](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/EmberPrototype/Runtime/MovementPath.cs>) / [PathMovingBlock](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/EmberPrototype/Runtime/PathMovingBlock.cs>)
- [주 플레이어 프리팹](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/prefab/player.prefab>) — `Assets/EmberPrototype/Prefabs/Player.prefab`은 별도 예전 프로토타입 프리팹이다.
- [Mossy Terrain RuleTile](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/tile/Mossy Tileset/Autotile/Mossy Terrain RuleTile.asset>) / [Mossy Terrain Palette](<C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/Assets/tile/Mossy Tileset/Autotile/Mossy Terrain Palette.prefab>)

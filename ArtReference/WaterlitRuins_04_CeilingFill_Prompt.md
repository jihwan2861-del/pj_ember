# Waterlit Ruins — 평평한 천장 채움용 암벽

- 내장 image_gen 사용. 기존 04 전경 바위는 색감과 픽셀 질감 참고용이며 변경하지 않았다.
- 저장: `Assets/image/WaterlitRuins_04_CeilingFill.png`.
- 종유석 윤곽 이미지가 아니라, 카메라가 올라갔을 때 천장 위 빈 공간을 가리는 불투명 직사각형 암벽 면이다.
- 출력: 1672×941, RGB PNG(알파 채널 없음). 전체가 불투명하며 네 모서리와 중앙의 알파 읽기 값은 255.
- 생성된 PNG 그대로 복사했다. 기존 이미지·씬·스크립트·임포트 설정은 수정하지 않았다.
- 무봉제 가로/세로 반복 타일로 검증한 이미지는 아니다.

## 배치

1. 새 PNG를 Sprite (2D and UI), Single로 임포트한다. Point 필터, Compression None, 기존 전경과 동일 PPU를 사용한다.
2. 별도의 천장 채움용 SpriteRenderer에 넣는다. 기존 종유석/전경 바위보다 뒤에 그리되, 먼 배경보다 앞에 그린다.
3. 이미지의 아래 끝만 기존 천장의 불투명한 윗부분과 겹치게 하고, 나머지는 위쪽으로 길게 배치한다. 전체 이미지를 기존 천장과 같은 중심에 놓으면 플레이 공간까지 가리므로 주의한다.
4. 카메라가 가장 높이 올라갔을 때도 채움 이미지의 상단이 화면 안에 드러나지 않을 만큼 위쪽 영역을 충분히 덮는다. 이미지 하나만으로 무한 높이의 화면 커버를 보장하지 않는다.
5. 패럴렉스를 쓴다면 기존 천장과 같은 Follow 값을 사용해 연결 부위가 벌어지지 않게 한다. 우선 Repeat X는 끄고 배치를 확인한다.

## 최종 프롬프트

Use case: stylized-concept
Asset type: flat, fully opaque cave-rock FILL texture for a pixel-art platformer's ceiling extension.

Input image: reference for COLOR, ROCK FACET SHAPES, and COARSE PIXEL SIZE ONLY. Do not copy its enclosing frame or silhouettes. Keep the original asset unchanged.

Primary request: ONE completely filled rectangular surface of dark cave rock, to place BEHIND and ABOVE the existing decorative foreground ceiling. When the camera rises during a high jump, this solid rock fill hides the empty background above the ceiling.
This is a flat wall/material texture, NOT a hanging ceiling illustration, NOT a cave opening, NOT a landscape.

Output: one landscape image, target 1672x941 pixels. The ENTIRE rectangle must be opaque solid rock from edge to edge and corner to corner. Zero transparent pixels, zero empty air, no visible outer rock silhouette, no protruding outlines. All four outer boundaries are straight rectangular cuts filled with rock.

Style: match the reference's dark near-black navy, deep blue, and restrained blue-teal rock facets. Unmistakable coarse pixel art as if roughly 320x180 graphics enlarged using nearest-neighbor: crisp approximately 5-pixel square clusters, blocky stepped internal facets, limited discrete colors. Most of the surface is very dark navy around #07121d; use sparse, low-contrast angular planes around #102332 and #1c3b47. Broad facets like the reference, not fine grain, bricks, or realistic stone. Quiet and subdued so it can blend behind the existing foreground rock edges.

Composition: even, flat rock coverage across the entire canvas, without a horizon, central focal point, perspective depth, brighter center, vignetting, border, or framing. Keep the outer edges visually quiet and consistent for adjacent placement; no brightly lit plane crossing an edge.

Absolutely NO stalactites, stalagmites, hanging points, floor, arch, opening, holes, transparent background, black cutout air, sky, water, blue light shafts, fog, plants, lamps, fire, characters, text, labels, checkerboard, watermark, antialiasing, blur, smooth gradients, or painterly brushwork.
Only a FULLY OPAQUE, FLAT RECTANGULAR DARK PIXEL-ROCK SURFACE.


# Waterlit Ruins — 바닥 아래 채움용 암벽

- 내장 image_gen으로 새 이미지 제작. 기존 전경 바위와 CeilingFill은 색감·픽셀 질감 참고용이며 변경하지 않았다.
- 저장: `Assets/image/WaterlitRuins_04_GroundFill.png`.
- 바닥 아래 빈 화면을 가리는 불투명 직사각형 암벽이다. 밟을 수 있는 바닥 타일셋이나 콜라이더가 아니다.
- 실제 출력: 1672×941, RGB PNG(알파 채널 없음). 전체 불투명. 네 모서리와 중앙 샘플 알파는 255.
- 생성 원본을 픽셀 수정 없이 복사했다. 원본과 복사본의 SHA-256이 일치한다.
- 무봉제 반복과 엄격하게 균일한 픽셀 격자는 검증하지 않았다. 기존 이미지·씬·프리팹·스크립트·임포트 설정은 변경하지 않았다.

## 배치

1. 새 PNG를 Sprite (2D and UI), Single로 임포트한다. 기존 전경과 동일 PPU, Point 필터, Compression None을 사용한다.
2. 별도의 SpriteRenderer에 넣고, 기존 하단 바위보다 뒤에 그리되 먼 배경보다 앞에 그린다.
3. 이미지의 위 끝만 기존 하단 바위의 불투명한 부분과 겹치게 놓고, 나머지는 아래쪽으로 내려 배치한다. 전체를 플레이 공간 중앙에 깔면 배경을 가리므로 주의한다.
4. 카메라가 가장 낮게 내려갈 때도 이미지의 아래 끝이 화면 안에 드러나지 않도록 충분한 영역을 덮는다.
5. 패럴렉스를 사용하면 기존 하단 전경과 같은 Follow 값을 사용한다. 우선 Repeat X는 끄고 배치를 확인한다.
6. 이 이미지는 시각적 채움용이다. 실제 이동·충돌 바닥은 기존 타일맵/콜라이더를 그대로 사용한다.

## 최종 프롬프트

Use case: stylized-concept
Asset type: a flat, fully opaque underground ROCK FILL image for the bottom of a side-scrolling pixel-art cavern.

Input roles:
Image 1 is a reference for the existing foreground boulders' palette and chunky rock facets. Do not modify it or reproduce its side-wall frame.
Image 2 is the companion ceiling-fill texture. Match its coarse pixel size, darkness, color palette, and simple material rendering so both belong to the same cave. Make a NEW rock arrangement, not a copy or a mirrored version.

Primary request:
A full rectangular block of solid dark rock texture used BEHIND and BELOW the existing bottom foreground rocks. It fills the area exposed when the camera looks down. This is a flat, front-view material surface, NOT a floor seen in perspective, NOT a row of loose rocks, and NOT a transparent edge decoration.

Canvas:
One wide landscape PNG, target 1672x941. Fully opaque edge to edge and corner to corner. Straight rectangular top, bottom, left and right boundaries. No transparency, empty air, cave holes, irregular outer silhouette, or protrusions.

Style and material:
Unmistakable coarse pixel art matching the references: approximately 5-pixel square clusters, crisp stepped internal contours, large angular rock facets, and a small discrete dark navy / deep blue / muted blue-teal palette.
Use mostly near-black navy around #07121d and deep blue around #102332, with restrained facet highlights around #1c3b47. Broad packed stone shapes with subtle horizontal layering suggest deep ground beneath a cave, without looking like bricks. Consistent contrast throughout; no center spotlight, bright upper rim, or depth gradient. A quiet solid fill that can sit behind the existing rocks without competing with the flame player.

Constraints:
Flat 2D material, no perspective or horizon. Do not draw a top soil line, grassy surface, playable ledge, floating boulder groups, stalagmites, stalactites, cavern scenery, water, ruins, bricks, crystals, lava, plants, fire, characters, labels, border, or watermark.
No fine grain, realistic stone texture, blur, antialiasing, smooth gradients, or painted brushwork.
The ENTIRE rectangular image is solid opaque rock, including all corners. One texture image only, not a sprite sheet or tiled preview.


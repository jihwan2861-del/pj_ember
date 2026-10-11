# 물빛 유적 — 01 먼 동굴 배경

- 생성 방식: 내장 image_gen.
- 파일: Assets/Backgrounds/WaterlitRuins/WaterlitRuins_01_FarCavern.png
- 실제 이미지 크기: 1672×941.
- 용도: 패럴렉스의 가장 뒤에 놓을 불투명 동굴 배경. 청록빛 공간, 먼 바위 실루엣, 빛줄기와 하단의 수면 표현이 포함되어 있다.
- 기둥·아치·가까운 유적은 이후 별도 투명 레이어로 만든다.
- Unity에서 픽셀을 선명하게 보이게 하려면 Filter Mode를 Point, Compression을 None으로 설정한다.
- 좌우 반복을 고려해 생성했지만 완전한 이음새 연결을 검증한 타일 이미지는 아니다. 반복 배치 전에 좌우 경계를 확인해야 한다.
- 이번 작업은 이미지 제작과 저장까지이며 씬·패럴렉스 스크립트·텍스처 Import 설정은 수정하지 않았다.

## 생성 프롬프트

```text
Use case: stylized-concept
Asset type: Layer 01, opaque farthest cavern background for a pixel-art side-scrolling platformer's parallax set.
Reference image: use ONLY the BOTTOM blue/teal waterlit cavern panel of the attached concept board as the palette and pixel-art style reference.

Generate ONE wide landscape background, approximately 16:9, intended as the farthest back layer behind separate architecture and foreground layers.
Show spacious underground air and enormous indistinct remote cavern walls in muted teal and blue. Wide pale turquoise light shafts descend from distant openings, expressed as crisp stepped blocks of flat color. Quiet flat bands of distant haze, simple barely visible distant rock masses, a faint horizontal cyan atmospheric band near the bottom. Keep the central two-thirds open and uncluttered for gameplay and later overlay layers.
Use low contrast, subdued brightness, simplified large shapes and broad horizontal flow. Keep left and right edges similar in palette and overall height so the image is suitable as a candidate for horizontal repetition.
Exclude ALL constructed ruins, arches, pillars, brickwork, doors, lanterns, orange highlights, close rocks, large dark foreground framing, visible floor, detailed water surface, waterfalls, and characters: those belong to other layers.
This is a single backmost environment plate, not the whole finished cavern scene and not a collage.

PIXEL ART IS THE HIGHEST PRIORITY:
Very clear coarse pixel art, like a native 320 by 180 pixel background enlarged with nearest-neighbor. Uniform visible square pixels approximately 4–6 screen pixels wide, clean stair-step contours, carefully grouped pixel clusters, limited palette of approximately 12–16 blue/teal colors. Use flat discrete color bands for lighting and haze; no smooth gradients, antialiasing, blur, painterly brushwork, photographic details, smooth glow, or finely textured noise.
Match the bottom reference panel's crisp blocky appearance while reducing detail and contrast for a faraway layer.
Fully filled opaque background. No text, labels, UI, watermark, borders, diagrams, or extra panels.
```


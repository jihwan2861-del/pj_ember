# 자연스러운 천장 v4

- 결과: `Assets/image/WaterlitRuins_04_CeilingOrganic_v4.png`
- 방식: 내장 이미지 생성. 원본 전경은 수정하지 않음.
- 역할: 불규칙한 바위 아래 경계, 아래쪽 투명 공간을 가진 천장 전경.
- 배치: 기존 평평한 천장 대신 사용. 같은 PPU/패럴렉스 설정으로 맞추고 기존 상단 바위 뒤에서 겹치도록 위치 조절. 실제 씬 연결과 반복 이음새는 검증하지 않음.

## 생성 프롬프트

```text
Use case: stylized-concept.
Asset type: standalone irregular cave ceiling foreground sprite for a 2D pixel-art platformer.
Input image 1 is the EXACT STYLE AND PALETTE REFERENCE, not a composition to copy. Create only a ceiling extension, NOT the reference's full four-sided frame.
Primary request: replace the visually conspicuous flat ceiling with natural irregular dark blue rock that belongs to this same cave. Wide landscape canvas approximately 1672 by 941.
Composition: solid rock completely fills the top 55-65% of the canvas, continuing to the top and left/right edges. Its bottom silhouette descends unevenly to approximately 65-80% in varied broad angular boulders and a few short blunt hanging rock projections, leaving genuinely transparent empty space below. No straight horizontal lower cutoff, no rectangular bottom, no flat dark fade band. This is a thick ceiling mass with a jagged exposed underside, not a thin row of icicles, not side walls or floor. Keep the hanging projections modest so the play area remains open.
Style: match the reference's chunky crisp stair-stepped pixel edges and sparse angular blocky rock facets, with approximately the same visible pixel-block scale. Restricted deep nearly black navy (#010F1B) shadows, subdued dark teal-blue and muted blue facets like the reference. Large shadow-connected rock masses, not a grid of small regular stones. Lighting only subdued blue along selected exposed undersides. No bright cyan, glow, smoothing, painterly blur, fine noisy texture, gradients, outlines, objects, water, text, border or watermark.
Use dark rock along the overlap region so this new sprite can sit behind and slightly overlap the original foreground's upper rock corners. Make the hanging silhouette organically uneven across the entire width. Output ONE isolated ceiling asset on genuine transparent alpha background below its rock silhouette. Do not render a checkerboard or black background in transparent space.
```


# 물 패키지 애셋 확인 — 2026-10-18

## 적용한 텍스처

- 프로젝트 경로: `Assets/Ember/Water/Textures/CausticsRGB.png`
- 원본: `C:/Users/kyk04/Desktop/Ember_DisabledAssets/2026-10-18/SC Post Effects/Runtime/Caustics/CausticsRGB.png`
- RGB 채널로 된 물빛 격자 무늬다. 수면 셰이더의 흐르는 빛 무늬에 활용한다. 바다 풍경이나 완성된 수면 이미지가 아니다.
- 원본 패키지는 보관 위치에 유지한다. Unity 6000.6/URP 17.6과 호환되지 않았던 SC Post Effects 스크립트는 복원하지 않았다.

## 확인했지만 수면에 사용하지 않은 자료

- SCPE `Install/Samples.unitypackage` 안의 `WaterDropsRefraction.png`, `RainDropsRefraction.png`: 화면이나 유리 위 물방울의 굴절용이다.
- `Assets/Epic Toon FX/Textures/water_3x3.png`, `water_soft_3x3.png`, `water_trsp_3x3.png`: 물방울·물튀김·고리 파문용 3×3 아틀라스다. 연속 수면 텍스처로 사용하지 않았다.
- 보관된 SCPE 폴더 및 포함된 `.unitypackage`에서 별도의 바다/해수면 색상 텍스처는 찾지 못했다.

## SCPE 효과의 용도

- **Ripples**: 화면 전체 UV를 사인파나 노이즈로 흔든다.
- **Refraction**: 입력 노멀맵을 이용해 화면을 왜곡한다.
- **Caustics**: 깊이에서 복원한 월드 위치에 물빛 무늬를 투영한다.

이 효과들은 수면의 실시간 반사를 직접 제공하지 않는다. 기존 SCPE 2.4.0 코드는 `Execute` 기반 호환 모드 렌더 패스를 사용하므로, 현재 프로젝트에는 텍스처만 가져와 별도 물 셰이더에서 활용한다. 현재 수면 구성은 `Docs/StoryWater_Setup.md`를 따른다.

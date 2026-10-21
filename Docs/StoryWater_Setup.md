# Storyscene 물 표현

최신 설정과 캡처는 [PPT 4~7 물가와 과거 반사](WaterSequence_PPT4-7_Setup.md)를 따른다.

- 물 셰이더: Assets/Ember/Water/StoryWater2D.shader
- Material: Assets/Ember/Water/StoryWater.mat
- VFX Graph: Assets/Ember/Water/SurfaceRipples.vfx
- 조절 컴포넌트: StoryShoreSection / River / Water Surface 2D
- 반사/왜곡/수면선은 셰이더, 잔빛은 VFX Graph가 담당한다.
- 현재는 실제 화면 반사와 별도 밝은 과거 그림의 전환을 모두 지원한다.
- 패키지 텍스처 재사용 내역은 [패키지 조사](WaterPackageAssets_Audit.md)를 참고한다.

Renderer2D Camera Sorting Layer Texture는 Default까지 캡처한다.
물과 첫 점프 Z 안내는 EmberWater 정렬 레이어를 사용한다.
현재 화면 반사는 화면 밖 물체를 그리지 않는 2D 방식이며, 기존 물 접촉 판정을 사용한다.

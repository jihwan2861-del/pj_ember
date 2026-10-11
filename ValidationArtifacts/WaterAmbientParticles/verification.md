# 맵 파란 분위기 파티클 검증

2026-10-07 Unity 6000.5.2f1 격리 QA 프로젝트: 관련 검사 13개 모두 통과.

새 검사에서 실제 Particle System 생성·입자 방출·월드 시뮬레이션·최대 64개 제한·파란 색상·URP 셰이더 지원·카메라 방 이동 시 정리·비활성화 시 정지를 확인했다. 기존 정령/돌진/파도/잔상/라이팅/쉐이킹 검사도 통과했다.

05-blue-ambient-particles.png를 렌더링하고 파란 소프트 점이 화면 전체에 낮은 밀도로 분포하는 것을 확인했다. UnityResults.xml과 unity.log가 실행 결과다.

CelesteRoomCamera에서 자동 연결하므로 본편 씬의 지형과 씬 직렬화 파일은 수정하지 않았다. 설정을 저장할 때는 카메라에 WaterAmbientParticles 컴포넌트를 미리 연결하면 된다.

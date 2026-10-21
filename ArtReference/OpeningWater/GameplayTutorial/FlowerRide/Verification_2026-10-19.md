# 태양꽃 탑승 구간 확인

- Unity 6000.6.5f1, Storyscene, 36×20.25칸 카메라, 0.5U 그레이박스 Tilemap.
- 정상 PlayerAbilityInputRouter에 런타임 가상 키보드를 입력해 C 발사 → X 탑승 → 오른쪽 이동 → Z 하차를 확인했다.
- C 발사 직후 timeScale0.2, 실제 탑승 후1.0. Z 하차는 (127.44,2.9), 하차 직후 남은 직진1초,0.12게임초 후0.88초. 플레이어는 수평속도6U/초를 받았다.
- 불꽃이 별도 Pollution 타일맵을 정화하고 플레이어가 (130.12,0.84)에 착지해 Completed/IsPassageClear=true.
- 열린 공간에서 선회 입력을 바꾸며 관찰: 최고 약44.07도, 자동하차1.999999게임초, 소멸2.999999게임초. 하차 뒤 반대 입력을 줘도 추가 회전0도.
- 0.02초 물리 틱에서 float 잔여값으로 자동하차가 한 틱 늦는 문제를 발견해 경계 허용오차를 보정하고 다시 관찰했다.
- 튜토리얼 slow 중 Pause/Resume: timeScale0.2→0→1, slow 소유권 해제.
- Captures/01_Ready.png,02_Fired.png,03_Boarded.png,04_Dismount.png,05_Completed.png는 실제 Game View 캡처다.
- 전체 오프닝을 처음부터 키 입력으로 완주하거나 빌드한 결과는 아니다. 자동 테스트 파일은 추가하지 않았다.

PNG 3종은 built-in image_gen으로 생성했다. 원본 PNG와 최종 프롬프트 3개는 이 폴더와 Prompts/에 보관한다. 게임용 사본은 Assets/Ember/Art/Storybook/Tutorial/SunFlower에 있다.

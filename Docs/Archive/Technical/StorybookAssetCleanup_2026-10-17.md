# 동화풍 전환 파일 정리 — 2026-10-17

## 삭제
사용처가 없는 과거 플레이어 Animator 11개·클립 5개와 구형 카메라 스크립트 2개를 삭제했다.
씬·프리팹·애셋·Animator·클립의 GUID 참조와 코드 사용처를 대조했다.

```
Assets/player/new_Player/animaition/ember_Idle_0.controller
Assets/player/new_Player/animaition/Jump.anim
Assets/player/new_Player/animaition/Player_Idle_4Frame_Sheet_0.controller
Assets/player/new_Player/animaition/Player_Idle_anim.anim
Assets/player/new_Player/animaition/Player_Jump_Reference_11Frame_Sheet_Proportions_v2_0.controller
Assets/player/new_Player/animaition/Player_Run 1_0 (1).controller
Assets/player/new_Player/animaition/Player_Run 1_0.controller
Assets/player/new_Player/animaition/Player_Run_1.2_ainm.anim
Assets/player/new_Player/animaition/Player_Run_4Frame_Sheet_0.controller
Assets/player/new_Player/animaition/Player_Run_anim.anim
Assets/player/new_Player/animaition/Player_Walk_4Frame_Sheet_0.controller
Assets/player/new_Player/animaition/Walk.anim
Assets/player/Palyer_No2/Player_Fall_no2_0.controller
Assets/player/Palyer_No2/Player_Jump_No2_0.controller
Assets/player/Palyer_No2/Player_No_2-sheet_0.controller
Assets/player/Palyer_No2/Player_Run_No2_0.controller
Assets/EmberPrototype/Runtime/CameraPositionGrid.cs
Assets/EmberPrototype/Runtime/CameraShiftTrigger.cs
```

## 이동
Unity AssetDatabase.MoveAsset으로 24개 애셋을 이동하고 각 GUID 일치를 확인했다.

```
Assets/player/new_Player/animaition/New_Player_Animator.controller -> Assets/player/Animations/Current/New_Player_Animator.controller
Assets/player/new_Player/animaition/Player_Idle_2.0.anim -> Assets/player/Animations/Current/Player_Idle_2.0.anim
Assets/player/new_Player/animaition/Player_No2_animator.controller -> Assets/player/Animations/Legacy/Player_No2_animator.controller
Assets/player/Palyer_No2/Player_No2_Fall.anim -> Assets/player/Animations/Legacy/Player_No2_Fall.anim
Assets/player/Palyer_No2/Player_no2_Idle.anim -> Assets/player/Animations/Legacy/Player_no2_Idle.anim
Assets/player/Palyer_No2/Player_No2_Jump.anim -> Assets/player/Animations/Legacy/Player_No2_Jump.anim
Assets/player/Palyer_No2/Player_No2_Run.anim -> Assets/player/Animations/Legacy/Player_No2_Run.anim
Assets/Ember/Art/Storybook/StageExtracted/01_MountainBackground.png -> Assets/Backgrounds/Storybook/01_MountainBackground.png
Assets/Ember/Art/Storybook/StageExtracted/02_Clouds.png -> Assets/Backgrounds/Storybook/02_Clouds.png
Assets/Ember/Art/Storybook/StageExtracted/03_Sun.png -> Assets/Backgrounds/Storybook/03_Sun.png
Assets/image/WaterlitRuins_01_Atmosphere_v2.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_01_Atmosphere_v2.png
Assets/image/WaterlitRuins_01_FarCavern.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_01_FarCavern.png
Assets/image/WaterlitRuins_02_CavernRepeatX_v2.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_02_CavernRepeatX_v2.png
Assets/image/WaterlitRuins_02_DistantRuins.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_02_DistantRuins.png
Assets/image/WaterlitRuins_03_NearRuins.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_03_NearRuins.png
Assets/image/WaterlitRuins_04_Ceiling.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_04_Ceiling.png
Assets/image/WaterlitRuins_04_CeilingFill_Joined_v2.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_04_CeilingFill_Joined_v2.png
Assets/image/WaterlitRuins_04_CeilingFill_Joined_v3.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_04_CeilingFill_Joined_v3.png
Assets/image/WaterlitRuins_04_CeilingFill.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_04_CeilingFill.png
Assets/image/WaterlitRuins_04_CeilingOrganic_v4.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_04_CeilingOrganic_v4.png
Assets/image/WaterlitRuins_04_ForegroundRepeatX_v2.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_04_ForegroundRepeatX_v2.png
Assets/image/WaterlitRuins_04_ForegroundRocks.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_04_ForegroundRocks.png
Assets/image/WaterlitRuins_04_GroundFill.png -> Assets/Backgrounds/Legacy/WaterlitRuins/WaterlitRuins_04_GroundFill.png
Assets/Backgrounds/WaterlitRuins/WaterlitRuins_01_FarCavern.png -> Assets/Backgrounds/Legacy/WaterlitRuins/Original/WaterlitRuins_01_FarCavern.png
```

## 유지
현재 플레이어의 손그림 달리기/점프, 기존 Idle 및 수정된 Animator를 보존했다.
game 1 씬이 사용하는 옛 No2 애니메이션은 Animations/Legacy에 유지했다.
아직 Git에 없는 손그림 소스·애니메이션은 삭제하지 않았다.
배경은 Backgrounds/Storybook과 Backgrounds/Legacy/WaterlitRuins로 분리했다.
FarCavern 중복 이미지 두 개는 서로 다른 GUID 연결을 보존하기 위해 Original 하위 폴더로 구분했다.
씬 생성·배치·저장은 하지 않았다.

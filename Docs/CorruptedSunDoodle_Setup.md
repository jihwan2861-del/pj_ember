# Corrupted Sun Doodle

Updated 2026-10-20. Storyscene uses `Assets/Ember/Art/Storybook/WaterDoodle_v1/CorruptedSun_Doodle_v1.png` on the existing `OpeningSuns/CorruptedSun` SpriteRenderer. The original image remains in `WaterPPT/Backgrounds/PPT_CorruptedSun.png`. The healthy `NormalSun`, `OpeningSuns/CorruptedSun/PastSunSource`, and RiverMemory past sun all use the shared `Assets/Ember/Art/Storybook/SkinScene/Backdrop/Sun_Doodle.png` sprite, matching the SkinScene sun prefab. NormalSun scale is uniform and matched to the corrupted sun's world size; the two hidden/reflection sources were rescaled to preserve their previous world bounds after the 760 PPU sprite replacement. Position, rotation, and reflection/material wiring remain unchanged.

The replacement keeps the existing sprite pivot and world width (the scale adjustment is only the 1254→1280 canvas-width ratio). The existing camera pin and circular orbit remain connected. The assigned material is `CorruptedSunWaver.mat`, using `CorruptedSunWaver.shader`.

## Waver material controls

- **Waver Strength**: UV displacement amplitude; default `0.006`. Keep small so the ink shape softly shimmers.
- **Waver Speed**: animation rate; default `1.4`.
- **Waver Scale**: wave frequency; default `13`.
- **Corruption Starts At U** and **Mask Softness**: select dark ink around the right-hand corruption. The purple-ink mask also follows the hand-painted silhouette, so the warm sun strokes stay steady.

Material property changes in the Inspector affect the existing sun renderer without changing orbit movement or water reflection setup. The material isn't automatically applied to `PastSunSource`; that object stays healthy for RiverMemory.

## Visual check

`ArtReference/OpeningSun/05_CorruptedSun_Doodle_Final_Render.png` is a Unity camera render of Storyscene with the existing corrupted sun temporarily shown at upper right to inspect the new art/material. It is an editor render preview, not a recorded full-playthrough capture. `CorruptedSunWaver.shader` imported as supported with no shader compilation failure. Two renders about a second apart showed pixel changes in the sun area. The sun unification capture is `ArtReference/SunCleanup/01_SunUnified.png`.

## Image prompt

See `Assets/Ember/Art/Storybook/WaterDoodle_v1/CorruptedSun_Doodle_v1.prompt.txt`.

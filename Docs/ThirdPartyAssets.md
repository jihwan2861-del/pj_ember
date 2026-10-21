# Third party Unity assets

The GitHub repository is public. To avoid redistributing purchased Asset Store source packages, these imported vendor folders and package archives are excluded from Git:

- `Assets/Epic Toon FX/`
- `Assets/VFXPACK_FIRE_WALLCOEUR/`
- `자료/Epic Toon FX.unitypackage`
- `자료/SC Post Effects Pack.unitypackage`

Install the corresponding packages through the Unity Asset Store before opening scenes that use their source assets. Keep each package's original folder and `.meta` GUIDs so the project's prefabs and materials can resolve their references. Project-specific prefabs, materials, and integrations under `Assets/Ember/` remain in this repository.

Unity may show missing references until both source packages are imported. This is expected for a fresh clone without the licensed packages.

# Customization asset organization repair

Verified in the open Unity editor on 2026-10-19.

- Eye material: `Assets/Characters/Customization/Materials/CharacterEyes.mat`.
- Skin shader: `Assets/Characters/Customization/Shaders/CustomizableSkin.shadergraph`.
- Both original asset GUIDs and their metadata were preserved by Unity AssetDatabase.MoveAsset. Serialized scene, prefab and material references remain connected.
- Updated the female-material preparation script to load the eye material from Materials.
- Updated the Markdown/HTML project guide and its generator.
- No C# methods or variables contained the misspelled skin name; runtime property names were already correct and remain compatible.
- Validation checked shader imports, both skin/eye material pairs, loaded eye renderer assignment, and scene/prefab dependencies. A full gameplay test was not performed for this path-only change.

```text
PASS: moved assets with original GUIDs.
Skin shader: Shader Graphs/CustomizableSkin
CharacterSkin -> Shader Graphs/CustomizableSkin
FemaleSkin -> Shader Graphs/CustomizableSkin
CharacterEyes -> Shader Graphs/CustomizableEyes
FemaleEyes -> Shader Graphs/CustomizableEyes
Loaded renderer eye references: 1
Dependent asset: Assets/Prefabs/FemaleCharacter.prefab
Dependent asset: Assets/Scenes/CharacterLab.unity
Dependent asset: Assets/Scenes/FemaleCharacterReview.unity
```

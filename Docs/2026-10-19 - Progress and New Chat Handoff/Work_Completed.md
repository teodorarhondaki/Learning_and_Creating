# Project progress — character customization milestone

Date: 2026-10-19. Baseline: commit `212225d`, **Add customizable skin, lips, hair, eyebrows and eyelashes** (2026-10-13). This report covers the working-tree changes after that commit. The new commit has not been created by this documentation task.

## What was already in the previous commit

The learning project already contained CharacterLab, the male character, customizable skin/lips/eyes/hair/facial-hair materials and shader graphs. This milestone extends that foundation; it does not claim that all those original assets were newly created here.

## Work completed since the baseline

### In-game color customization

CharacterLab now has six visible color targets: Skin, Lips, Eyes, Hair, Eyebrows and Eyelashes. Each button opens a shared picker and displays its selected color. RGB sliders, a six-digit hex field and a visual HSV square/hue strip edit the same color. Invalid hex input produces an error rather than applying a bad value. A separate brightness multiplier is available when supported by the material; eyes have no brightness property.

The hair shader gained a Texture Exponent control to lighten its neutral detail texture before tinting. The male hair material uses 0.15. This reduces the overly dark result from multiplying the selected color by the original dark texture. Lighting, texture detail and shading still mean the rendered surface is not a flat UI swatch.

### Female character support

The existing female FBX was inspected and its body, eyes, hair, brows, lashes and clothing matched to source assets. Added separate FemaleSkin, FemaleEyes, FemaleHair, FemaleEyebrows, FemaleEyelashes and FemaleClothing materials, plus female neutral skin, lip-mask and hair textures. Shared URP shader graphs are reused. Added a FemaleCharacter prefab and FemaleCharacterReview scene, then integrated the female into CharacterLab with matching facing/alignment.

Male and female colors remain independent during the scene session. Runtime copies protect the saved source materials. Skin and lips intentionally share one copied skin material with separate properties.

### Character tab and hairstyle gallery

The menu now has Character and Colors tabs. Character contains a name field, Male/Female choices, portrait placeholders and a scrolling hair gallery. The name field supports Unicode and has a 40-character scene limit. Tab/profile changes close the picker so its previous target is not accidentally edited.

There are twelve configured hair entries: six for each character. Only the original male short hair and female ponytail currently have models. The other ten are disabled placeholders. Each future choice is one complete preset (one model and picture), not separate length/wave/curl switches. The gallery uses a clipped viewport and vertical scrolling. Selection is retained separately per character during the session.

The production brief gives the twelve finished preset names and designs. The current scene still uses generic slot labels such as Short, Wavy and Curly. Those labels must not be mistaken for completed assets or combinable settings.

### Asset organization repairs

Moved CharacterEyes.mat from Shaders into Materials. Renamed the skin graph to CustomizableSkin.shadergraph. Unity moved both assets with their original GUIDs; existing scene, prefab and material assignments stayed connected. Updated SetupFemaleMaterials, the project guide and its generator. No C# methods or variables had the misspelling.

### Documentation and friend handoff

Added a detailed project guide in Markdown/HTML with nine dependency diagrams and code explanations, a hairstyle production brief, and an asset-repair report. Organized documents into dated theme folders with a browser index. Created the external hairstyle handoff ZIP containing the brief in Markdown/HTML, six reference views and three menu screenshots. It contains no Blender/FBX models and no finished new hair assets.

## Runtime connections

```text
DevMenu -> SceneChanger.ChangeScene() -> CharacterLab
Character tab -> CharacterSetupMenu -> name / profile / complete hair preset
Colors tab -> six buttons -> CharacterColorPicker.Open(index)
  RGB sliders ---------------------> ApplyColor()
  Hex field -> ApplyHex() ----------> ApplyPickedColor() -> ApplyColor()
  Pointer/drag -> ColorPickerSurface -> HsvColorPicker -> ApplyPickedColor()
ApplyColor() -> runtime material property -> shader -> character renderer
             -> button swatch / preview / hex / HSV markers
Brightness -> ApplyBrightness() -> separate material float
Scene teardown -> restore original renderer materials / destroy runtime copies
```

`SetValueWithoutNotify`, `SetTextWithoutNotify` and the HSV applying guard avoid recursive updates when one control refreshes the others. Buttons are registered in Awake/Start. Both character material profiles are initialized in Awake, not lazily when the menu opens. The HSV textures are created when first needed. Closing a panel hides it; it does not unload the character or destroy every menu object.

## Files and responsibilities

| File/group | Responsibility |
|---|---|
| Assets/Scripts/CharacterColorPicker.cs | Button bindings, RGB/hex synchronization, brightness, profiles and material ownership |
| Assets/Scripts/HsvColorPicker.cs | Hue/saturation/value state, generated picker textures and markers |
| Assets/Scripts/ColorPickerSurface.cs | Pointer/drag coordinates converted into picker values |
| Assets/Scripts/CharacterSetupMenu.cs | Tabs, name, profile cards and complete hairstyle selections |
| Assets/Scenes/CharacterLab.unity | UI hierarchy, serialized references, both character instances and gallery configuration |
| Assets/Prefabs/FemaleCharacter.prefab | Female model with URP material assignments |
| Assets/Scenes/FemaleCharacterReview.unity | Isolated female review scene |
| Assets/Characters/Customization/Materials | Male/female customization materials, including relocated CharacterEyes |
| Assets/Characters/Customization/Shaders | CustomizableSkin, CustomizableEyes and CustomizableHair graphs |
| Assets/Editor/Setup*.cs, CreateCustomizationRows.cs, ExpandHairGallery.cs | One-time/editor construction and migration helpers, not runtime menu code |
| Assets/Editor/FemaleMenuRuntimeCheck.cs | Automated editor Play Mode checks; explicit invocation can exit Unity |
| Assets/Editor/CaptureHairHandoff.cs | Reference-image capture helper; opens scene and changes it in memory, uses a local output path |

The TextMesh Pro fallback font asset also changed during this work. Its generated font/glyph data is included in the working-tree changes; it is not a new gameplay feature.

## Verification and limits

- Existing recorded Play Mode result: PASS for tabs, Unicode name, filtered gallery, disabled placeholders, closing picker on tab switch, selection retention, female color targets through RGB/hex/HSV/brightness, swatch synchronization, repeated profile switching, male edits, independent color retention and unchanged source materials.
- The runtime check source also exercises scrolling using a PointerEventData event and the scrollbar. This is not a physical mouse test.
- On 2026-10-19, Unity verified the relocated material and renamed shader, both male/female skin and eye material shader references, a loaded eye renderer reference, and scene/prefab dependency resolution.
- Documentation reorganization: forty local Markdown links and Python syntax checked; browser guide regenerated with nine embedded diagrams and valid section anchors.
- Handoff ZIP: twelve entries, archive integrity checked and nine image links resolved.
- No new full Play Mode regression or standalone build was run for this documentation task. Earlier evidence is preserved separately below.
- No persistence across scene reloads/application restarts. No animation/rig certification, finished ten new hairstyles, finished portrait/thumbnail art, or validation on every screen size.
- Different future hair UVs/materials will require appropriate textures and extending material-color binding. Simply attaching a mesh is not proof that its tinting works.
- Historical Unity GPUResidentDrawer/InstanceCuller assertions were reported earlier; this milestone is not a verified fix for that engine issue.

## Evidence locations

A copy of the latest available Play Mode report and today's asset repair report is stored in this folder under `Evidence`. Earlier logs and screenshots are outside the Git repository at `H:\ChatGPT - IT\CharacterSource\FemaleCustomization`. These reports describe the runs that produced them, not a new run today.

## Commit preparation

Use the title and description in Commit_Message.md. Include new assets with their .meta files and the old-path deletions so Git records the moves correctly. Do not add Library, Temp, Logs or external source archives. The external hairstyle ZIP is not part of this repository. Review the generated TMP fallback change alongside the other changes. No commit or staging was performed by this documentation task.

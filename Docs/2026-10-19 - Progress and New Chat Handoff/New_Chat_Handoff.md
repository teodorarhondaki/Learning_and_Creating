# New chat handoff — Learning_and_Creating

Snapshot: 2026-10-19. Read this first, then inspect the current checkout before changing anything. This is project context, not authorization to perform every suggested future task.

## Project identity and user workflow

- Active Unity project: `H:\ChatGPT - IT\Unity-Project\Learning_and_Creating`.
- Unity: 6000.3.24f1, URP, Windows. Editor: `H:\ChatGPT_Bible_Game\Unity\Editors\6000.3.24f1\Editor\Unity.exe`.
- The chat may start in `H:\ChatGPT_Bible_Game`; that is NOT this learning project's repository. Do not mix it with the older CreationGame project.
- User is learning development and wants small inspectable steps, explanations of ownership/connections and concrete verification. Avoid large unsolicited feature batches. For editor guidance, use short steps and screenshots. Preserve their tuning and unsaved editor work.
- User wants a commit title/description and normally performs the commit. This handoff task does not create a commit. At snapshot time HEAD was `212225d` (2026-10-13), Add customizable skin, lips, hair, eyebrows and eyelashes. Run git status/log now: the user may have committed since this handoff.

## Read next

- [Work completed since previous commit](Work_Completed.md)
- [Commit message](Commit_Message.md)
- [Documentation index](../README.md)
- [Detailed earlier project guide](<../2026-10-15 - Project Guide/Project_Guide.md>) — useful architecture/code examples, but predates female/tab/gallery additions. Current source and this handoff take precedence.
- [Hairstyle production brief](<../2026-10-16 - Character and Hairstyle Asset Brief/Character_Menu_Asset_Brief.md>)
- [Asset repair report](<../2026-10-19 - Customization Asset Repair/Customization_Asset_Repair.md>)

## Current behavior

DevMenu navigation uses SceneChanger.ChangeScene with the destination assigned in the Inspector. The user previously confirmed CharacterLab -> DevMenu -> CharacterLab navigation works; do not repeat old claims that the button is broken without testing.

CharacterLab contains male and female models, one shared customization UI and an EventSystem. Character tab has name, Male/Female cards and six hair cards per profile inside a scrolling viewport. Colors tab shows all six target buttons at once: Skin, Lips, Eyes, Hair, Eyebrows, Eyelashes. Clicking one opens a shared RGB + hex + HSV picker with preview and supported brightness. Buttons display selected color.

Both character color profiles are independent during a scene session. Runtime material copies avoid editing assets. Nothing is persisted across scene reloads. The name is a single scene-session value. Hair selection is stored per profile.

## Source ownership and data flow

`Assets/Scripts/CharacterColorPicker.cs` owns Part arrays (source material, color property, optional brightness property, button, runtime instance). Awake copies source materials, reassigns matching renderer slots including inactive children, and wires UI events. Skin/lips share one skin copy. SelectFemale switches active profile/model and clears editing. Open loads the selected values. ApplyColor writes RGB and refreshes preview/button/hex/HSV. ApplyHex validates exactly six hex digits with optional # and calls ApplyPickedColor. Brightness is independent of the hex RGB. OnDestroy restores renderer arrays and destroys copies.

`HsvColorPicker.cs` generates hue and saturation/value textures, maintains markers and calls ApplyPickedColor. `ColorPickerSurface.cs` translates left-button pointer/drag events through the Canvas camera into normalized coordinates. No-notify setters and an applying guard prevent update loops.

`CharacterSetupMenu.cs` owns tab visibility, name input, portrait placeholders and HairOption entries (id/title/female/thumbnail/hairObject). It filters slots by profile and toggles complete hair objects. Null models are disabled even if a thumbnail is supplied. Six UI slots are configured; adding array entries alone does not create more UI slots. Selected current style is also non-interactable. ShowColors closes the picker before switching panels.

Important lifecycle detail: hiding the picker does not free all its state. Both profiles are initialized in Awake. HSV textures are created on first use and destroyed on teardown. The Done listener currently hides pickerPanel directly; ClosePicker additionally clears the selected index and is used on tab changes.

## Materials and shaders — canonical paths

All customization materials are in `Assets/Characters/Customization/Materials`, including CharacterEyes.mat. SetupFemaleMaterials now loads Materials/CharacterEyes.mat. CharacterEyes GUID remains `87979590f41fc4f499383b8d9056b4e1`.

Graphs are in `Assets/Characters/Customization/Shaders`: CustomizableSkin.shadergraph, CustomizableEyes.shadergraph and CustomizableHair.shadergraph. The skin spelling was corrected on October 19, with GUID `a6e54598955fe3b4eb4701ba08d83412` preserved. Do not recreate the old misspelled asset or regenerate .meta files.

| Target | Color property | Brightness property |
|---|---|---|
| Skin | _Skin_Color | _Skin_Brightness |
| Lips | _Lips_Color | _Lip_Brightness |
| Eyes | _Iris_Color | none |
| Hair / brows / lashes | _Hair_Color | _Hair_Brightness |

Hair neutral detail is adjusted by _Texture_Exponent before tint; male hair uses 0.15. Alpha is independent of color/brightness. Eyebrow cutoff 0.25 was accepted by user. Avoid making brows sparse again or restoring dark source textures that cannot visibly tint white. Physical lighting prevents an exact flat-swatch/render match.

Female has separate six materials and FemaleSkin_TintBase, FemaleSkin_LipMask and FemaleHair_Neutral textures. Existing female FBX was reused. FemaleCharacter.prefab and FemaleCharacterReview.unity are available. CharacterLab female instance faces the camera through scene alignment/rotation; do not assume the review prefab orientation is identical. Rig/animation readiness is unverified.

## Hair art requirements

Each choice is ONE finished hairstyle asset, not combinations of length and curl settings. Twelve slots exist; only male_short and female_ponytail have models. Current generic scene labels remain; the brief gives final names/designs for ten future styles. New styles need fitted models/UVs and their own appropriate materials/textures, plus color-binding integration. A PNG alone does not add usable 3D hair.

Friend pack: `H:\ChatGPT - IT\CharacterSource\Hairstyle_Asset_Handoff_2026-10-19.zip`. Contains brief Markdown/HTML, six character views, three menu screenshots and README. No FBX/.blend files. Requested thumbnails/portraits are 512x512 transparent sRGB PNG, optional 1024 master. Full details and GPT instructions are in the brief. Asset path repairs did not require changing that ZIP.

## Validation, tools and safety of continuation

Read Evidence reports here. Earlier Play Mode checks passed; they were not rerun for documentation. Today's Unity asset repair check passed. Physical pointer interaction, standalone build, animation and save/load remain unverified/unimplemented as applicable. Source FemaleMenuRuntimeCheck exercises gallery scrolling through event calls. The historical GPU-driven culling assertion is not a known fixed feature of this work.

Editor setup helpers are migration tools, not runtime dependencies. Some create/overwrite UI or open/save scenes; inspect each before running. FemaleMenuRuntimeCheck can enter Play Mode and exit Unity. CaptureHairHandoff opens CharacterLab and alters its in-memory state for rendering. Do not run these against unsaved work without considering it. Several helpers and document builders use machine-specific source/tool paths.

External evidence/source material: `H:\ChatGPT - IT\CharacterSource\FemaleCustomization`. Not included in Git. Docs use date-theme folders; keep images/generators alongside their document and fix relative links if moved. Create a new dated theme/version folder for a new concept revision.

## Suggested continuation — only when requested

1. Review and commit the present milestone with all necessary .meta files.
2. Import friend-supplied portraits/thumbnails and completed hair models in small batches; map each whole preset and validate UV/tint/alpha/selection.
3. Add persistence only as a separately scoped feature, with explicit data ownership and restoration tests.
4. Test UI at target resolutions and actual pointer interaction, then build/animation checks when relevant.

Do not claim ten new hair models, persistence, or all testing are complete. Ask the user what they want next rather than automatically starting this backlog.

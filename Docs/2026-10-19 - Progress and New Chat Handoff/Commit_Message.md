# Commit title

Add male and female character setup with synchronized color controls

# Commit description

- Add Character/Colors tabs, a name field, profile selection and a scrolling gallery of complete hairstyle presets with disabled placeholders for missing models.
- Add shared RGB, hex and visual HSV color controls for skin, lips, eyes, hair, eyebrows and eyelashes, with swatch feedback and supported brightness controls.
- Keep male/female colors independent using runtime material copies; restore source assignments on teardown.
- Add female URP materials, neutral textures, prefab and review scene; improve hair tint response with a texture exponent.
- Move CharacterEyes into Materials and correct CustomizableSkin naming while preserving Unity GUIDs and updating loaders/documentation.
- Add editor setup/check tools, generated TMP fallback data and dated documentation with dependency diagrams, hairstyle specifications and a new-chat handoff.

Validation: recorded Play Mode checks passed for tabs, name, profile switching, color synchronization, placeholders, scrolling and material isolation. Unity verified shader/material references after the asset move/rename. Documentation links and browser diagrams checked. No standalone build or fresh full gameplay regression was run for this handoff.

Limits: choices are scene-session only. Ten hairstyle entries remain placeholders; new hair models, final portrait art, persistence and animation validation are not included.

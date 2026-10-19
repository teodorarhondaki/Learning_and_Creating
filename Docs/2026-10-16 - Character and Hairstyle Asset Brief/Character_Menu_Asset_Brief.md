# Character menu — complete hairstyle preset brief

## The design

Each card is **one complete hairstyle**: one named preset, one thumbnail and one matching 3D hair asset. The player clicks the card to equip it. Length, waves and curls are descriptions of that finished style, not separate controls or settings.

Prepare the twelve presets below: six fitted to the male character and six fitted to the female character. These groupings describe the current model fitting and menu organization. Keep the two existing hairstyles exactly as they are. The other ten are new designs to produce.

## Male presets

| Current entry ID | Finished preset name | Exact design | File stem |
|---|---|---|---|
| male_short | Original Short | Match the existing male short hair from the supplied model/screenshots. Preserve its silhouette and parting; do not reinterpret it as a bob. | hair_male_original_short |
| male_medium | Swept-Back Waves | Medium-length, loose S-shaped waves brushed away from the forehead, with volume on top and ends reaching the nape. Ears mostly visible. | hair_male_swept_back_waves |
| male_long | Shoulder-Length Straight | Straight loose hair reaching the shoulders, center part, tucked slightly behind the ears. No ponytail or bun. | hair_male_shoulder_straight |
| male_wavy | Short Tousled Waves | Short wavy cut with a textured, slightly messy top, shorter sides, and a few soft waves falling toward the forehead. | hair_male_tousled_waves |
| male_curly | Curly Crop | Compact rounded crop with clearly defined tight curls, short sides and visible ears. Keep individual curl groups readable. | hair_male_curly_crop |
| male_straight | Straight Fringe | Short straight cut with a soft, slightly broken fringe above the eyebrows and neat sides. Distinct from the original short style. | hair_male_straight_fringe |

## Female presets

| Current entry ID | Finished preset name | Exact design | File stem |
|---|---|---|---|
| female_ponytail | Original Ponytail | Match the existing female ponytail, including its attachment height, swept-back scalp hair and tail length. | hair_female_original_ponytail |
| female_bob | Chin-Length Bob | Straight bob ending at the jaw/chin, subtle inward-curving ends, slight side part and no fringe. | hair_female_chin_bob |
| female_long | Long Loose Curls | Loose hair reaching the upper back, with large defined curls and a center part. Keep curl groups distinct from soft waves. | hair_female_long_curls |
| female_wavy | Shoulder-Length Waves | Loose shoulder-length hair with broad S-shaped waves and a side part; soft volume without ringlets. | hair_female_shoulder_waves |
| female_curly | Curly Pixie | Short curly cut with small defined curls on top, close sides, exposed ears and an uncovered neck. | hair_female_curly_pixie |
| female_straight | Long Straight | Smooth loose hair reaching the upper back, center part, no fringe, no tied sections. | hair_female_long_straight |

The IDs in the first column identify the current Unity slots. They are not additional player choices. The finished preset names are proposed labels for those slots. **This document updates the production brief; the scene still has its earlier generic placeholder labels.**

## Deliverable sizes and formats

- **Menu thumbnails:** one separate **512 × 512 sRGB RGBA PNG** per preset. Keep an optional **1024 × 1024 master**. Use the file stem above plus `.png`.
- **Gender portraits:** `gender_male.png` and `gender_female.png`, also **512 × 512**, optional 1024 masters. Adult head-and-shoulders portraits matching our actual characters.
- **Total menu pictures:** **14** — twelve hairstyles and two portraits.
- **Background:** true transparency. No baked checkerboard, text, border, button background or selection outline. The game supplies labels and button styling.
- **Framing:** leave about 10% margin; do not crop hair ends. Use a consistent camera and head scale. Frame the entire hairstyle even for long hair.
- **Lighting/style:** grounded, moderately realistic game art, soft neutral lighting and readable strand groups. Use neutral dark-gray hair for hairstyle pictures, on a neutral gray mannequin matching the relevant head proportions. Keep male and female sets internally consistent.
- **Small-size review:** check legibility at approximately 48–72 pixels; the actual UI can display thumbnails much smaller than their delivered resolution.
- These are our delivery specifications. If generation produces a larger image, export the requested size while preserving transparency.

## Ready-to-copy request for GPT Pro

Copy this section **together with both preset tables above**, and attach reference screenshots of the current male and female characters.

> I am commissioning menu artwork for a Unity character customization system. Use the attached reference screenshots and the twelve-preset tables in this brief as the specification.
>
> Every hairstyle is one complete, fixed preset. Do not create separate length, straight/wavy/curly, fringe or ponytail controls. Do not make a matrix of every possible combination. Produce exactly the named designs in the tables.
>
> Work in small batches. First list the deliverables and identify any missing references. Then produce one male and one female sample for approval. After I approve the visual style, continue with the remaining presets in batches of two or three. Keep framing, head scale, lighting and rendering style consistent.
>
> For Original Short and Original Ponytail, reproduce our existing hairstyle silhouettes from the references; do not redesign them. For the other ten designs, follow each table description. Show the hair on a neutral gray mannequin head fitted to the male or female proportions. Use neutral dark-gray hair with readable strand detail. Use a consistent three-quarter front view. Where the ponytail or rear shape is unclear, also provide a separate side/rear reference, not an extra selectable preset.
>
> Deliver one separate transparent 512 × 512 sRGB PNG per hairstyle, using the exact file stem in the table. Keep a 1024 × 1024 master when available. Leave about 10% clear margin and include every hair tip. No labels, lettering, borders, button artwork or baked checkerboard. Do not deliver only a combined contact sheet. A contact sheet may accompany the individual files for review.
>
> Also create gender_male.png and gender_female.png: adult head-and-shoulders portraits matching our actual character references, with neutral expressions, plain clothing, the same camera angle and soft neutral lighting. Use the same PNG dimensions and transparency rules.
>
> Check the fourteen menu pictures at small thumbnail size. Report which files are complete. Treat newly designed pictures as concepts until matching 3D hair assets have been created. Do not claim that an image file is a playable 3D hairstyle.

## Playable 3D hair assets

The thumbnail is the picture on the card. The actual hairstyle equipped by the player is a separate **3D asset**. For the two existing presets, reuse the current hair geometry; only their menu pictures are needed. The ten new presets need matching geometry in addition to thumbnails.

Provide the artist with the corresponding character mesh and head/rig reference before modeling. For each new preset deliver:

1. Editable Blender source: `<file_stem>.blend`.
2. A game-ready export: `<file_stem>.fbx`, fitted to the exact target head and using documented scale, axes and attachment/binding.
3. A UV-mapped neutral grayscale hair-detail texture, normally **2048 × 2048 RGBA PNG**, named `<file_stem>_detail.png`. Preserve hair-card transparency. This is a texture atlas matching the model's UVs, not the menu thumbnail.
4. Front, side and rear review images showing head fit, parting, hairline, ears and the whole silhouette. Include animation/attachment checks when supplying rigged assets.
5. A short note listing triangle count, texture dimensions, material slots, head/rig used, source/licensing information and any known clipping issues. Agree the geometry budget against the existing hair assets before final delivery.

Avoid strong baked color or lighting that prevents tinting. Our existing URP shader controls hair color and brightness and uses alpha clipping. Other maps should only be supplied as optional extras until shader support is agreed. Long styles need special attention to neck/shoulder intersections; a good thumbnail does not prove the model fits in motion.

The final menu thumbnail should match the final approved model. If the model differs from an early generated concept, regenerate or render its thumbnail from the finished model.

## Handoff and Unity integration

Deliver folders `UI/`, `Models/`, `Textures/` and `Reference/`, plus a manifest mapping each current entry ID to the final preset name and filenames. Missing models must be clearly labelled as pending; never substitute a PNG for an FBX.

The developer will import UI pictures as single sprites and connect them to the matching Hair Options entries. The current scrollable gallery has six entries per gender. It also has two portrait slots. Adding a thumbnail alone does not enable a pending hairstyle.

Each actual hair object belongs under its matching character and must be assigned to the corresponding entry. Different styles can need different textures, so each may need a distinct material using the shared hair shader. The color-picker binding must be extended to apply the chosen hair color and brightness to each selectable style's material. Do not assign the current ponytail or short-hair texture blindly to new UV layouts. This multi-style material integration is developer work when the new models arrive; it is not already implemented for arbitrary future materials.

## Current project status

CharacterLab contains Character and Colors tabs, a session name field, gender buttons, and a vertically scrollable hair gallery. Only Original Short and Original Ponytail have actual hair objects. Ten other entries are placeholders. Choices are retained during the current scene session; persistent saving is not implemented. This brief specifies the finished assets to replace those placeholders.

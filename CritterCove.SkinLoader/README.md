Skin Loader for Critter Cove
============================

This mod lets you load custom materials to make available in the character
creater in Critter Cove.

Tested with MelonLoader v0.5.7, may also work on later versions.

Usage
-----

1. Navigate to the game's `Critter Cove_Data\StreamingAssets` folder
2. Create a folder inside called `skins`
3. Create a new folder to store your skin resources in, and enter it
4. Create a `skinDefinition.json` file (see below)
5. Copy your textures into the folder; textures can be in PNG or JPEG

Skin Definitions
----------------

A skin definition file is a JSON file containing an array of material
overrides. The core of the skin loader's approach is to load new textures
and colors on top of a material the closest approaches your character,
and as such requires a base material to reference from.

Before we get into the material overrides, let's look at a few concepts.

### What is a material?

A material represents the properties of the "skin" on a model. In Critter Cove,
each body part has an associated material, and by swapping out this material,
you can change the appearance of the character. This is also what this skin
loader is based on, cloning base materials and changing its properties to
modify its appearance.

### What is a texture/mask?

A texture/mask provides the visual patterning of the material. Basically, it is
the bulk of the details that you see on the character model. A texture can be
assigned to a material.

In Critter Cove, to support color customization, colored masks are used, and if
you look at the original textures, you'll see that it has regions of solid
red, green, and blue. Each of these channels are mapped to a particular color,
so when you modify the color in the character builder, each of these regions
take on that color. There are two masks and up to six colors that you can
assign.

Although the character shader is intended to color characters using masks, by
setting the first color to only red, second color to only green, and third
color to only blue, you can map the colors back to what they're supposed to
be on a full-color texture and bypass the limitation on the number of colors.
Note that it is unknown whether Critter Cove's devs will use the chosen colors
for something other than appearance customization.

Note that textures should be 1024x1024px in size.

### Character materials

The selectable materials in the character builder are defined by character
materials in the game database. These group materials for multiple body parts
into a single set, and specifies the number of colors that can be changed.
Body character materials specify the body, feet, and hands materials. Head
character materials specify the head and ear materials. Tail character materials
specify the tail materials. Beak character materials specify the tail materials.

### Character sub-materials

Sub-materials are customizable materials when certain items are worn. Currently
they mostly apply to certain types of hands, feet, and ears.

Now let's look at a couple of constructs used by the skin loader.

### Material parameters

Material parameters specify some character shader variables that can be
modified for a given material.

```json
{
    "Mask1Texture": "texture.png",
    "Mask2Texture": "texture2.png",
    "EnableMask2": false,
    "InheritMask1Texture": false,
    "InheritMask2Texture": false,
    "InheritEnableMask2": false
}
```

- `Mask1Texture`: Path of texture to use for first mask; path is relative to the
  folder that the skin definition is in. Leave as `null` if you do not want to
  use it.
- `Mask2Texture`: Path of texture to use for first mask. Leave as `null` if you
 do not want to use it.
- `EnableMask2`: Specifies whether the second mask is enabled. Set to `false` if
  you are not using the second mask.
- `InheritMask1Texture`: Whether to use mask 1 from the original material.
  Overrides `Mask1Texture` if set to `true`.
- `InheritMask2Texture`: Whether to use mask 1 from the original material.
  Overrides `Mask2Texture` if set to `true`.
- `InheritEnableMask2` whether to use enable mask 2 setting from the orignal
  material. Overrides `EnableMask2` if set to `true`.

Default values are `null` and `false`. If you aren't setting something, you can
omit it from the JSON.

### Serializable color

These are ARGB color components represented with floating point numbers.

```json
{
    "a": 1.0,
    "r": 1.0,
    "g": 0.0,
    "b": 0.0
}
```

If the whole object is set to `null`, then the corresponding color in the
material will not be overridden. It is useful to set colors so you can have the
exact color you want when you select the material in the character creator.

### Material overrides

As the main element of the skin definitions file, it defines overrides for
character materials. Each material override corresponds to a character material
that you want to be based off of.

```json
{
    "Id": "body_custom_name",
    "BaseId": "body_critter_name",
    "Name": "Name",
    "Colors": [
        {
            "a": 1.0,
            "r": 1.0,
            "g": 0.0,
            "b": 0.0
        },
        {
            "a": 1.0,
            "r": 0.0,
            "g": 1.0,
            "b": 0.0
        },
        {
            "a": 1.0,
            "r": 0.0,
            "g": 0.0,
            "b": 1.0
        }
    ],
    "InheritColors": false,
    "Icon": "solid_mask",
    "InheritIcon": false,
    "BodyMaterial": <MaterialParams>,
    "FeetMaterial": <MaterialParams>,
    "HandsMaterial": <MaterialParams>,
    "HeadMaterial": <MaterialParams>,
    "TailMaterial": <MaterialParams>,
    "EarMaterial": <MaterialParams>,
    "BeakMaterial": <MaterialParams>,
    "InheritBodyMaterial": false,
    "InheritFeetMaterial": false,
    "InheritHandsMaterial": false,
    "InheritHeadMaterial": false,
    "InheritTailMaterial": false,
    "InheritEarMaterial": false,
    "InheritBeakMaterial": false,
    "SubMaterials": [
        <SubMaterialOverrideDefinition>
    ],
    "AutoInheritAllSubMaterials": true
}
```

- `Id`: ID of the character material to generate. This will be saved in your
  save game if you choose to use this material. Cannot be `null`.
- `BaseId`: ID of character material to base off of. Cannot be `null`.
- `Name`: Name of the new character material
- `Colors`: Default colors to apply when the material is selected
- `InheritColors`: Whether to use original set of colors from base character
  material. Overrides `Colors` if set to `true`.
- `Icon`: Name of icon that represents this material. Choices to be documented
  later.
- `InheritIcon`: Whether to use original icon from base character material.
  Overrides `Icon` if set to `true`.
- `BodyMaterial`: Material parameters for body. Can be left `null` if unused.
- `FeetMaterial`: Material parameters for feet. Can be left `null` if unused.
- `HandsMaterial`: Material parameters for hands. Can be left `null` if unused.
- `HeadMaterial`: Material parameters for head. Can be left `null` if unused.
- `TailMaterial`: Material parameters for tail. Can be left `null` if unused.
- `EarMaterial`: Material parameters for ears. Can be left `null` if unused.
- `BeakMaterial`: Material parameters for beak. Can be left `null` if unused.
- `InheritBodyMaterial`: Whether to use original body material from base
  character model. Overrides `BodyMaterial` if set to `true`.
- `InheritFeetMaterial`: Whether to use original feet material from base
  character model. Overrides `FeetMaterial` if set to `true`.
- `InheritHandsMaterial`: Whether to use original gabds material from base
  character model. Overrides `HandsMaterial` if set to `true`.
- `InheritHeadMaterial`: Whether to use original gead material from base
  character model. Overrides `HeadMaterial` if set to `true`.
- `InheritTailMaterial`: Whether to use original tail material from base
  character model. Overrides `TailMaterial` if set to `true`.
- `InheritEarMaterial`: Whether to use original ear material from base
  character model. Overrides `EarMaterial` if set to `true`.
- `InheritBeakMaterial`: Whether to use original beak material from base
  character model. Overrides `BeakMaterial` if set to `true`.
- `SubMaterials`: List of sub-material overrides
- `AutoInheritAllSubMaterials`: Inherit all sub-materials from base character
  material. If set to `true`, `SubMaterials` will not be used.

Default values are `null` and `false`. If you aren't setting something, you can
omit it from the JSON, aside from `Id`, `BaseId`, and `Name`.

### Sub-material overrides

You can specify your own character sub-materials. Note that if you choose to
specify sub-material overrides, you should specify all the ones that you'll
need, because base sub-materials will not be otherwise inherited.

```json
{
    "SubId": "feet_robot_01",
    "SubMaterial": <MaterialParams>,
    "InheritSubMaterial": false
}
```

- `SubId`: The ID of the wearable item that this affects
- `SubMaterial`: Material parameters for the item
- `InheritSubMaterial`: Whether to use original material from character
  sub-material for this item. Overrides `SubMaterial` if set to `true`.

Getting character material IDs
------------------------------

You can find the character material names you are currently using by inspecting
your save file. Go to `%USERPROFILE%\AppData\LocalLow\Gentleman Rat Studios\Critter Cove\GameSaves\{64BitSteamID}`
and use 7-Zip to decompress your latest `.save` file. It is a JSON file. Inside,
locate the `ActorData` object with the `Id` property set to `player`. You can
find your current character material IDs in the values of the `BodyMask`,
`HeadMask`, `TailMask`, and `BeakMask` properties. You can also grab currently
configured colors from the this object.

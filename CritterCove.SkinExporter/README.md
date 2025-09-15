Skin Exporter for Critter Cove
==============================

This mod exports your current player model as FBX and generates a skin template
that you can load using the [Skin Loader](/CritterCove.SkinLoader).

Usage
-----

After installing MelonLoader, put `CritterCove.SkinExporter.dll` and
`CritterCove.SkinLoader.dll` in the `Mods` folder, and the files
`Autodesk.Fbx.dll`, `Unity.Formats.Fbx.Editor.dll`, and `UnityFbxSdkNative.dll`
in the `UserLibs` folder.

Within the game, when you enter the character creation screen, pick your desired
body parts and base materials, then select "Export" (see bottom right of screen).
The files will then be generated in the
`Critter Cove_Data\StreamingAssets\skins\export` folder under the game folder.

Note: to enable exporting meshes, the mod patches the engine so it reads
`isReadable` as true when loading mesh data. This uses extra memory, so to
reduce extraneous memory usage, remove the mod once you are finished doing
exports.

Exported contents
-----------------

- `export.fbx`: FBX file with the exported model and embedded textures. Use this
  while editing your textures to verify that they look OK. Some 3D modelling
  programs may not support FBX files completely, so you may need to convert it
  into another format such as Wavefront OBJ before you can import into your
  3D modelling program. You may also have to reassign textures if your 3D
  modelling program does not support embedded textures, but you should be able
  to locate the textures by name.
- `fbx_textures`: folder of exported textures, in case you need to manually
  select them for your model. It contains both diffuse and normal maps.
- `skinDefinition.json`: this is a template for your skin. The majority of the
  data has been set up for you. You should change the following:
  - `Id` and `Name` of each material definition. Make them unique to your skin.
  - `Colors`: the default mask colors. This will be touched upon later.
  - `Icon`: the icon to represent the material. Currently the default is a blank
    icon. List of available icons TBD.
  See [Skin Loader readme](/CritterCove.SkinLoader/README.md) for more info on
  the properties.
- `*.png`: these are your replacement textures. You can either link them to the
  FBX model, or copy the completed textures over them. You can also drop your
  textures and change the file names in `skinDefinition.json`. You can have up
  to two maps per material.

Once you have finished creating your skin, you can delete `export.fbx` and the
`fbx_textures` folder.

Full color textures vs masks
----------------------------

The game uses RGB masks to allow color customization. Essentially, each
character material can have up to two mask textures, which represent sets of
patterns as 100% red, 100% green, and 100% blue pixels. A character material can
have up to 6 colors, the first three representing the RGB masks of the first
mask texture, and the last three representing the RGB masks of the second mask
texture. If you use non-pure-RGB colors, it allows blending the different
user-selectable colors by the proportion of the mask colors. You can specify
the default user-selectable colors these mask colors correspond to by setting
them in the `Colors` array of the material definition.

Taking advantage of the color mixing behavior, you can set the user-selectable
colors to 100% red, 100% green, and 100% blue in that order to render the colors
of the mask texture as-is (you only need one mask texture for this). This allows
you to use any color on your texture, and it will be reflected in-game. However,
it is unknown if the devs will use the user-selectable colors for anything other
than material coloring.

Making a full-color texture is easier and more flexible than using mask colors,
and is a good choice if you need more than 6 colors per body part or if you
want to create colorfully-complex patterns. However, due to the aforementioned
point, you may way to stick to drawing patterns with the mask colors instead.

Other notes
-----------

Human models may be missing their groin area because they always wear pants,
so due to the way the character structure works and culling of inactive
wearables, there may be nothing there in the exported model.

The version of the FBX Exporter package has been modified to remove dependencies
from the Unity Editor, and additionally modified to export shader textures
introduced by Critter Cove's shaders specifically. The source code for this
version is available [here](https://github.com/GMMan/com.unity.formats.fbx/tree/custom).

using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CritterCove.SkinLoader
{
    internal class SkinLoaderV2
    {
        public const string SKINS_DIR_NAME = "skins";
        public const string DEFINITION_FILE_NAME = "skinDefinition.json";

        static readonly Lazy<SkinLoaderV2> lazy = new Lazy<SkinLoaderV2>(() => new SkinLoaderV2());
        public static SkinLoaderV2 Instance => lazy.Value;

        Dictionary<string, Material> cachedMaterials = new Dictionary<string, Material>();
        List<Texture2D> createdTextures = new List<Texture2D>();
        Dictionary<string, (SkinMaterialDefinition SkinDef, string OrigMaterialId, MaterialParams Params)> generatedMaterialsMap =
            new Dictionary<string, (SkinMaterialDefinition SkinDef, string origMaterialId, MaterialParams Params)>();
        Dictionary<string, string> pathMap = new Dictionary<string, string>();

        public void Initialize(GameDataCollections gameData)
        {
            Reset();

            // Create map of materials, submaterials, and filters
            CreateOrigMaterialMaps(gameData, out var materialsMap, out var subMaterialsMap, out var filtersMap);

            // Scan StreamingAssets/skins for all definition files
            foreach (var defPath in Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, SKINS_DIR_NAME),
                DEFINITION_FILE_NAME, SearchOption.AllDirectories))
            {
                try
                {
                    string json = File.ReadAllText(defPath);
                    var definitions = JsonConvert.DeserializeObject<List<SkinMaterialDefinition>>(json);
                    string defDir = Path.GetDirectoryName(defPath);

                    foreach (var definition in definitions)
                    {
                        // Add material
                        gameData.character_materials.Add(GenerateMaterialDefinition(materialsMap[definition.BaseId], definition));

                        // Add submaterials
                        if (definition.AutoInheritAllSubMaterials)
                        {
                            if (subMaterialsMap.TryGetValue(definition.BaseId, out var origSubMaterials))
                            {
                                var defaultSubDefinition = new SubMaterialOverrideDefinition { InheritSubMaterial = true };
                                foreach (var subMaterial in origSubMaterials.Values)
                                {
                                    gameData.character_materials_sub.Add(GenerateSubmaterial(subMaterial, definition, defaultSubDefinition));
                                }
                            }
                        }
                        else if (definition.SubMaterials != null)
                        {
                            foreach (var subMaterial in definition.SubMaterials)
                            {
                                gameData.character_materials_sub.Add(GenerateSubmaterial(subMaterialsMap[definition.BaseId][subMaterial.SubId], definition, subMaterial));
                            }
                        }

                        // Add filters
                        foreach (var filter in filtersMap[definition.BaseId])
                        {
                            gameData.character_material_filter.Add(GenerateMaterialFilter(filter, definition));
                        }

                        pathMap.Add(definition.Id, defDir);
                    }

                    Melon<Mod>.Logger.Msg($"Registered skin at {defPath}.");
                }
                catch (IOException ex)
                {
                    Melon<Mod>.Logger.Error($"Failed to read while processing definition at {defPath}.", ex);
                }
                catch (JsonException ex)
                {
                    Melon<Mod>.Logger.Error($"Failed to deserialize definitions {defPath}.", ex);
                }
                catch (KeyNotFoundException ex)
                {
                    Melon<Mod>.Logger.Error($"Failed to find an ID during processing definition at {defPath}.", ex);
                }
            }
        }

        public void Reset()
        {
            foreach (var material in cachedMaterials.Values)
            {
                UnityEngine.Object.Destroy(material);
            }
            foreach (var texture in createdTextures)
            {
                UnityEngine.Object.Destroy(texture);
            }
            cachedMaterials.Clear();
            createdTextures.Clear();
            generatedMaterialsMap.Clear();
            pathMap.Clear();
        }

        public Material? GetMaterial(string id)
        {
            //Melon<Mod>.Logger.Msg($"Game requesting material {id}.");

            if (cachedMaterials.TryGetValue(id, out var material))
            {
                //Melon<Mod>.Logger.Msg($"Found cached.");
                return material;
            }

            if (!generatedMaterialsMap.TryGetValue(id, out var tup))
            {
                //Melon<Mod>.Logger.Msg($"Doesn't seem to be generated, aborting.");
                return null;
            }

            if (string.IsNullOrEmpty(tup.OrigMaterialId))
            {
                Melon<Mod>.Logger.Msg($"Original material does not exist, aborting.");
                return null;
            }

            material = LoadWearableMaterialPatch.LoadWearableMaterialOrig("customization/materials/" + tup.OrigMaterialId);
            if (material == null)
            {
                Melon<Mod>.Logger.Error($"Failed to load original material {"customization/materials/" + tup.OrigMaterialId}.");
                return null;
            }

            string dir = pathMap[tup.SkinDef.Id];

            if (!tup.Params.InheritMask1Texture)
            {
                if (tup.Params.Mask1Texture != null)
                {
                    material.SetTexture("_MaskTex", LoadCustomTexture(dir, tup.Params.Mask1Texture));
                }
                else
                {
                    material.SetTexture("_MaskTex", null);
                }
            }

            if (!tup.Params.InheritMask2Texture)
            {
                if (tup.Params.Mask2Texture != null)
                {
                    material.SetTexture("_MaskTex2", LoadCustomTexture(dir, tup.Params.Mask2Texture));
                }
                else
                {
                    material.SetTexture("_MaskTex2", null);
                }
            }

            if (!tup.Params.InheritEnableMask2)
            {
                material.SetFloat("USE_MASK2", tup.Params.EnableMask2 ? 1.0f : 0.0f);
            }

            if (!tup.SkinDef.InheritColors && tup.SkinDef.Colors != null)
            {
                for (int i = 0; i < tup.SkinDef.Colors.Count; ++i)
                {
                    int idAdd = i >= 3 ? 1 : 0;
                    var color = tup.SkinDef.Colors[i];
                    if (color != null)
                    {
                        material.SetColor($"_Color{i + 1 + idAdd}", color.GetColor());
                    }
                }
            }

            cachedMaterials.Add(id, material);
            return material;
        }

        character_materials GenerateMaterialDefinition(character_materials baseMaterial, SkinMaterialDefinition definition)
        {
            character_materials material = new character_materials();
            material.id = definition.Id;
            material.name = definition.Name;
            if (definition.InheritColors)
            {
                material.colors = baseMaterial.colors;
            }
            else
            {
                material.colors = definition.Colors?.Count ?? 0;
            }
            if (definition.InheritIcon)
            {
                material.icon = baseMaterial.icon;
            }
            else
            {
                material.icon = definition.Icon;
            }

            if (definition.InheritBodyMaterial)
            {
                material.body_material = baseMaterial.body_material;
            }
            else
            {
                material.body_material = GenerateNewMaterial(definition, baseMaterial.body_material, definition.BodyMaterial);
            }

            if (definition.InheritFeetMaterial)
            {
                material.feet_material = baseMaterial.feet_material;
            }
            else
            {
                material.feet_material = GenerateNewMaterial(definition, baseMaterial.feet_material, definition.FeetMaterial);
            }

            if (definition.InheritHandsMaterial)
            {
                material.hands_material = baseMaterial.hands_material;
            }
            else
            {
                material.hands_material = GenerateNewMaterial(definition, baseMaterial.hands_material, definition.HandsMaterial);
            }

            if (definition.InheritHeadMaterial)
            {
                material.head_material = baseMaterial.head_material;
            }
            else
            {
                material.head_material = GenerateNewMaterial(definition, baseMaterial.head_material, definition.HeadMaterial);
            }

            if (definition.InheritTailMaterial)
            {
                material.tail_material = baseMaterial.tail_material;
            }
            else
            {
                material.tail_material = GenerateNewMaterial(definition, baseMaterial.tail_material, definition.TailMaterial);
            }

            if (definition.InheritEarMaterial)
            {
                material.ear_material = baseMaterial.ear_material;
            }
            else
            {
                material.ear_material = GenerateNewMaterial(definition, baseMaterial.ear_material, definition.EarMaterial);
            }

            if (definition.InheritBeakMaterial)
            {
                material.beak_material = baseMaterial.beak_material;
            }
            else
            {
                material.beak_material = GenerateNewMaterial(definition, baseMaterial.beak_material, definition.BeakMaterial);
            }

            material.new_test = baseMaterial.new_test;
            material.exclude_in_demo = baseMaterial.exclude_in_demo;

            return material;
        }

        character_materials_sub GenerateSubmaterial(character_materials_sub baseMaterial, SkinMaterialDefinition skinDefinition, SubMaterialOverrideDefinition overrideDefinition)
        {
            character_materials_sub material = new character_materials_sub();
            material.body_material_id = skinDefinition.Id;
            material.sub_id = baseMaterial.sub_id;
            if (overrideDefinition.InheritSubMaterial)
            {
                material.sub_material = baseMaterial.sub_material;
            }
            else
            {
                material.sub_material = GenerateNewMaterial(skinDefinition, baseMaterial.sub_material, overrideDefinition.SubMaterial);
            }
            material.new_test = baseMaterial.new_test;
            return material;
        }

        character_material_filter GenerateMaterialFilter(character_material_filter baseFilter, SkinMaterialDefinition definition)
        {
            character_material_filter filter = new character_material_filter();
            filter.body_type = baseFilter.body_type;
            filter.material = definition.Id;
            filter.sub = baseFilter.sub;
            filter.new_test = baseFilter.new_test;
            filter.pos = baseFilter.pos;
            return filter;
        }

        string GenerateNewMaterial(SkinMaterialDefinition skinDef, string originalId, MaterialParams? materialParams)
        {
            if (materialParams == null) return null;

            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            generatedMaterialsMap.Add("customization/materials/" + guidString, (skinDef, originalId, materialParams));
            return guidString;
        }

        static void CreateOrigMaterialMaps(GameDataCollections gameData, out Dictionary<string, character_materials> materialsMap,
            out Dictionary<string, Dictionary<string, character_materials_sub>> subMaterialsMap, out Dictionary<string, List<character_material_filter>> filtersMap)
        {
            materialsMap = new Dictionary<string, character_materials>();
            subMaterialsMap = new Dictionary<string, Dictionary<string, character_materials_sub>>();
            filtersMap = new Dictionary<string, List<character_material_filter>>();

            foreach (var material in gameData.character_materials)
            {
                materialsMap.Add(material.id, material);
            }
            foreach (var subMaterial in gameData.character_materials_sub)
            {
                GetOrCreateInDictionary(subMaterialsMap, subMaterial.body_material_id).Add(subMaterial.sub_id, subMaterial);
            }
            foreach (var filter in gameData.character_material_filter)
            {
                GetOrCreateInDictionary(filtersMap, filter.material).Add(filter);
            }
        }

        static T GetOrCreateInDictionary<T>(Dictionary<string, T> dict, string key) where T : new()
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = new T();
                dict.Add(key, value);
            }
            return value;
        }

        Texture2D? LoadCustomTexture(string dir, string path)
        {
            string fullPath = Path.Combine(dir, path);
            //Melon<Mod>.Logger.Msg($"Texture requested for {fullPath}");
            try
            {
                byte[] imageData = File.ReadAllBytes(fullPath);

                Texture2D texture = new Texture2D(1, 1);
                createdTextures.Add(texture);
                if (ImageConversion.LoadImage(texture, imageData, true))
                {
                    return texture;
                }
                else
                {
                    Melon<Mod>.Logger.Error($"Failed to load image at {fullPath} into texture.");
                    return null;
                }
            }
            catch (IOException ex)
            {
                Melon<Mod>.Logger.Error($"Failed to read image at {fullPath}", ex);
                return null;
            }
        }
    }
}

using Animancer;
using CritterCove.SkinLoader;
using GRS;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

namespace CritterCove.SkinExporter
{
    internal class Exporter
    {
        const string EXPORT_PATH = "export";

        static readonly FieldInfo CCSUI_clothing = AccessTools.Field(typeof(CharacterCreationSheetUI), "_clothing");
        static readonly FieldInfo CCSUI_doll = AccessTools.Field(typeof(CharacterCreationSheetUI), "_doll");
        static readonly PropertyInfo AnimationManager_Animancer = AccessTools.Property(typeof(AnimationManager), "Animancer");
        static readonly MethodInfo SerializableColor_ConvertColors = AccessTools.Method(typeof(SerializableColor), "ConvertColors");

        public static void Export()
        {
            MelonCoroutines.Start(ExportCoroutine());
        }

        static System.Collections.IEnumerator ExportCoroutine()
        {
            CharacterCreationSheetUI ccUi = UnityEngine.Object.FindAnyObjectByType<CharacterCreationSheetUI>(FindObjectsInactive.Include);

            // Remove clothing
            bool origClothing = (bool)CCSUI_clothing.GetValue(ccUi);
            if (origClothing)
            {
                ccUi.ToggleClothing();
            }

            var doll = (Doll)CCSUI_doll.GetValue(ccUi);
            var originalPos = doll.Owner.transform.localPosition;
            doll.Owner.transform.localPosition = Vector3.zero;

            // Wait for next frame so wearables can update
            yield return null;

            // Move inactive wearables out so they are not captured in the export
            GameObject holderGo = new GameObject();
            List<(GameObject Go, Transform OrigParent)> inactiveList = new List<(GameObject Go, Transform OrigParent)>();

            foreach (var transform in doll.Owner.GetComponentsInChildren<Transform>(true))
            {
                var go = transform.gameObject;
                if (go.TryGetComponent<Wearable>(out _) && (!go.activeInHierarchy || transform.parent == doll.Owner.transform))
                {
                    inactiveList.Add((go, transform.parent));
                    transform.SetParent(holderGo.transform);
                }
            }

            // Move BigSplash as well
            var bigSplash = doll.Owner.transform.Find("BigSplash");
            inactiveList.Add((bigSplash.gameObject, bigSplash.parent));
            bigSplash.SetParent(holderGo.transform);

            // Reset animation
            var animancer = (AnimancerComponent)AnimationManager_Animancer.GetValue(doll.Owner.AnimationManager);
            animancer.Stop();

            // Wait for things to settle
            yield return new WaitForEndOfFrame();

            string exportBase = Path.Combine(Application.streamingAssetsPath, SkinLoaderV2.SKINS_DIR_NAME, EXPORT_PATH);
            Directory.CreateDirectory(exportBase);

            // Export to FBX
            ExportModelOptions exportOptions = new ExportModelOptions
            {
                ExportFormat = ExportFormat.Binary,
                EmbedTextures = true,
                AnimateSkinnedMesh = false,
                ModelAnimIncludeOption = Include.Model,
            };

            bool success = true;

            try
            {
                success = ModelExporter.ExportObject(Path.Combine(exportBase, "export.fbx"), doll.Owner.gameObject, exportOptions) != null;

                if (success)
                {
                    // Export skin definition and textures
                    var def = CreateSkinDef(ccUi, exportBase);
                    string json = JsonConvert.SerializeObject(def, Formatting.Indented);
                    File.WriteAllText(Path.Combine(exportBase, SkinLoaderV2.DEFINITION_FILE_NAME), json);
                }
            }
            catch (Exception ex)
            {
                success = false;
                Melon<Mod>.Logger.Error("Failed to export model.", ex);
            }

            // Restore position
            doll.Owner.transform.localPosition = originalPos;

            // Restore inactive wearables to their original location
            foreach (var tup in inactiveList)
            {
                tup.Go.transform.SetParent(tup.OrigParent);
            }
            inactiveList.Clear();
            UnityEngine.Object.Destroy(holderGo);

            if (origClothing)
            {
                ccUi.ToggleClothing();
            }

            if (success)
            {
                AcceptConfirmUI.Open("Export successful.", null);
            }
            else
            {
                AcceptConfirmUI.Open("Export failed. MelonLoader console may have more info.", null);
            }
        }

        static List<SkinMaterialDefinition> CreateSkinDef(CharacterCreationSheetUI ccUi, string exportPath)
        {
            List<SkinMaterialDefinition> defs = new List<SkinMaterialDefinition>();

            if (ccUi._currentBodyMask != null)
            {
                defs.Add(CreateSkinMaterialDef(ccUi._currentBodyMask, ccUi._bodyMaskColor, exportPath, "body"));
            }
            if (ccUi._currentHeadMask != null)
            {
                defs.Add(CreateSkinMaterialDef(ccUi._currentHeadMask, ccUi._bodyMaskColor, exportPath, "head"));
            }
            if (ccUi._currentTailMask != null)
            {
                defs.Add(CreateSkinMaterialDef(ccUi._currentTailMask, ccUi._bodyMaskColor, exportPath, "tail"));
            }
            if (ccUi._currentBeakMask != null)
            {
                defs.Add(CreateSkinMaterialDef(ccUi._currentBeakMask, ccUi._bodyMaskColor, exportPath, "beak"));
            }

            return defs;
        }

        static SkinMaterialDefinition CreateSkinMaterialDef(character_materials baseMaterial, Color[] currentColors,
            string exportPath, string maskType)
        {
            SkinMaterialDefinition newMat = new SkinMaterialDefinition();
            newMat.Id = maskType + "_custom_new";
            newMat.BaseId = baseMaterial.id;
            newMat.Name = "New " + maskType;
            newMat.Colors = new List<SerializableColor?>((SerializableColor[])SerializableColor_ConvertColors.Invoke(null, new object[] { currentColors }));
            while (newMat.Colors.Count > baseMaterial.colors)
            {
                newMat.Colors.RemoveAt(newMat.Colors.Count - 1);
            }
            newMat.Icon = "solid_mask";

            if (!string.IsNullOrEmpty(baseMaterial.body_material))
            {
                newMat.BodyMaterial = ExportMaterial("body", baseMaterial.body_material, exportPath);
            }
            if (!string.IsNullOrEmpty(baseMaterial.feet_material))
            {
                newMat.FeetMaterial = ExportMaterial("feet", baseMaterial.feet_material, exportPath);
            }
            if (!string.IsNullOrEmpty(baseMaterial.hands_material))
            {
                newMat.HandsMaterial = ExportMaterial("hands", baseMaterial.hands_material, exportPath);
            }
            if (!string.IsNullOrEmpty(baseMaterial.head_material))
            {
                newMat.HeadMaterial = ExportMaterial("head", baseMaterial.head_material, exportPath);
            }
            if (!string.IsNullOrEmpty(baseMaterial.tail_material))
            {
                newMat.TailMaterial = ExportMaterial("tail", baseMaterial.tail_material, exportPath);
            }
            if (!string.IsNullOrEmpty(baseMaterial.ear_material))
            {
                newMat.EarMaterial = ExportMaterial("ear", baseMaterial.ear_material, exportPath);
            }
            if (!string.IsNullOrEmpty(baseMaterial.beak_material))
            {
                newMat.BeakMaterial = ExportMaterial("beak", baseMaterial.beak_material, exportPath);
            }

            newMat.AutoInheritAllSubMaterials = true;

            return newMat;
        }

        static MaterialParams? ExportMaterial(string maskType, string materialName, string exportPath)
        {
            MaterialParams? materialParams = null;
            var material = AddressableHelper.LoadWearableMaterial("customization/materials/" + materialName);
            if (material != null)
            {
                materialParams = new MaterialParams();
                var mask = material.GetTexture("_MaskTex");
                if (mask != null)
                {
                    string name = $"{maskType}_mask1.png";
                    ExportTexture((Texture2D)mask, Path.Combine(exportPath, name));
                    materialParams.Mask1Texture = name;
                }

                mask = material.GetTexture("_MaskTex2");
                if (mask != null)
                {
                    string name = $"{maskType}_mask2.png";
                    ExportTexture((Texture2D)mask, Path.Combine(exportPath, name));
                    materialParams.Mask1Texture = name;
                }
            }

            return materialParams;
        }

        static void ExportTexture(Texture2D texture, string path)
        {
            Texture2D newTexture = ModelExporter.DeCompressTexture(texture, false);
            var texturePng = newTexture.EncodeToPNG();
            File.WriteAllBytes(path, texturePng);
            UnityEngine.Object.Destroy(newTexture);
        }
    }
}

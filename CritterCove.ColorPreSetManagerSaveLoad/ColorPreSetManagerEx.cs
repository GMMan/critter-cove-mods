using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CritterCove.ColorPreSetManagerSaveLoad
{
    internal static class ColorPreSetManagerEx
    {
        const string PRESET_SAVE_NAME = "color_presets.json";

        static readonly FieldInfo ColorPreSetManager_colorPreSets = AccessTools.Field(typeof(ColorPreSetManager), "_colorPreSets");

        public static bool LoadPrefix()
        {
            Load();
            return false;
        }

        public static bool SavePrefix()
        {
            Save();
            return false;
        }

        public static void Load()
        {
            string presetSavePath = Path.Combine(GameSaveManager.DataRootSave, PRESET_SAVE_NAME);
            if (File.Exists(presetSavePath))
            {
                string json = File.ReadAllText(presetSavePath);
                Dictionary<string, string?[]> deserialized = JsonConvert.DeserializeObject< Dictionary<string, string?[]>>(json);

                Dictionary<string, Color?[]> newPresets = new Dictionary<string, Color?[]>();
                foreach (var p in deserialized)
                {
                    if (p.Value == null) continue;
                    Color?[] colors = new Color?[p.Value.Length];
                    for (int i = 0; i < colors.Length; ++i)
                    {
                        if (p.Value[i] == null)
                        {
                            continue;
                        }

                        try
                        {
                            colors[i] = HexToColor(p.Value[i]!);
                        }
                        catch (FormatException)
                        {
                            Debug.Log($"Failed to convert \"{p.Value[i]}\" to Color");
                            colors[i] = null;
                        }
                    }
                    newPresets.Add(p.Key, colors);
                }

                ColorPreSetManager_colorPreSets.SetValue(null, newPresets);
            }
            else
            {
                ColorPreSetManager_colorPreSets.SetValue(null, new Dictionary<string, Color?[]>());
            }
        }

        public static void Save()
        {
            Dictionary<string, string?[]> toSerialize = new Dictionary<string, string?[]>();
            Dictionary<string, Color?[]> currentPresets = (Dictionary<string, Color?[]>)ColorPreSetManager_colorPreSets.GetValue(null);

            foreach (var p in currentPresets)
            {
                string?[] convertedColors = new string?[p.Value.Length];
                for (int i = 0; i < convertedColors.Length; ++i)
                {
                    if (p.Value[i] == null)
                    {
                        continue;
                    }

                    convertedColors[i] = ColorUtility.ToHtmlStringRGB(p.Value[i]!.Value);
                }
                toSerialize.Add(p.Key, convertedColors);
            }

            string presetSavePath = Path.Combine(GameSaveManager.DataRootSave, PRESET_SAVE_NAME);
            File.WriteAllText(presetSavePath, JsonConvert.SerializeObject(toSerialize));
        }

        public static Color HexToColor(string hex)
        {
            // Wrapper compatible with previous implementation
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                color.a = 1.0f;
                return color;
            }
            else
            {
                throw new FormatException("Not a valid hex color.");
            }
        }
    }
}

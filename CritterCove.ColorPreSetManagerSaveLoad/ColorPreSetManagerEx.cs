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

                    convertedColors[i] = ColorToHex(p.Value[i]!.Value);
                }
                toSerialize.Add(p.Key, convertedColors);
            }

            string presetSavePath = Path.Combine(GameSaveManager.DataRootSave, PRESET_SAVE_NAME);
            File.WriteAllText(presetSavePath, JsonConvert.SerializeObject(toSerialize));
        }

        public static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 3)
            {
                return new Color(ParseHexToFloat($"{hex[0]}{hex[0]}"), ParseHexToFloat($"{hex[1]}{hex[1]}"), ParseHexToFloat($"{hex[2]}{hex[2]}"));
            }
            else if (hex.Length == 6)
            {
                return new Color(ParseHexToFloat(hex.Substring(0, 2)), ParseHexToFloat(hex.Substring(2, 2)), ParseHexToFloat(hex.Substring(4, 2)));
            }
            else
            {
                throw new FormatException("Not a valid hex color.");
            }
            
        }

        static float ParseHexToFloat(string hex)
        {
            return int.Parse(hex, System.Globalization.NumberStyles.HexNumber) / 255f;
        }

        public static string ColorToHex(Color color)
        {
            return $"{FloatToHex(color.r)}{FloatToHex(color.g)}{FloatToHex(color.b)}";
        }

        static string FloatToHex(float value)
        {
            int intValue = Mathf.RoundToInt(value * 255);
            if (intValue > 255) intValue = 255; // Could this even happen?
            return intValue.ToString("x2");
        }
    }
}

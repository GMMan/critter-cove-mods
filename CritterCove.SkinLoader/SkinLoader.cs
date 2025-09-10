using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CritterCove.SkinLoader
{
    internal static class SkinLoader
    {
        public static CharacterCustomizations LoadedCustomizations { get; } = new CharacterCustomizations();

        public static IEnumerator LoadAddressables()
        {
            yield return Addressables.InitializeAsync(true);
            var handle = Addressables.LoadContentCatalogAsync(Path.Combine(Application.dataPath, "StreamingAssets", "aa", "skins", "skins.json"));
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                yield return LoadCustomizationsFromAddressables(handle.Result);
            }
            else
            {
                Melon<Mod>.Logger.Msg($"Failed to load skins catalog: {handle.Status}");
            }
            handle.Release();

            Melon<Mod>.Logger.Msg(ConsoleColor.Green, "Loading complete.");
        }

        public static void AddCustomizationsToGameData(GameDataCollections gameData)
        {
            gameData.character_materials.AddRange(LoadedCustomizations.Materials);
            gameData.character_materials_sub.AddRange(LoadedCustomizations.MaterialsSubs);
            gameData.character_material_filter.AddRange(LoadedCustomizations.MaterialFilters);
        }

        static IEnumerator LoadCustomizationsFromAddressables(IResourceLocator locator)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };

            foreach (var key in locator.Keys)
            {
                //Melon<Mod>.Logger.Msg($"Found key in locator: {key}");
                if (key is string k && k.Contains("skin_additions") && Path.GetFileNameWithoutExtension(k) == "skin_additions")
                {
                    if (locator.Locate(key, typeof(TextAsset), out var locations))
                    {
                        foreach (var location in locations)
                        {
                            var handle = Addressables.LoadAssetAsync<TextAsset>(location);
                            yield return handle;
                            if (handle.Status == AsyncOperationStatus.Succeeded)
                            {
                                try
                                {
                                    var customizations = JsonConvert.DeserializeObject<CharacterCustomizations>(handle.Result.text);
                                    LoadedCustomizations.Merge(customizations);
                                }
                                catch (JsonException ex)
                                {
                                    Melon<Mod>.Logger.Error($"Failed to deserialize skin_additions asset {location.PrimaryKey}: {ex.Message}");
                                }
                            }
                            else
                            {
                                Melon<Mod>.Logger.Msg($"Failed to load skin_additions asset {location.PrimaryKey}: {handle.Status}, {handle.OperationException}");
                            }
                            handle.Release();
                        }
                    }
                }
            }
        }
    }
}

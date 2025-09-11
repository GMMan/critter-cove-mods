using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CritterCove.SkinLoader
{
    [HarmonyPatch(typeof(GameSaveManager), nameof(GameSaveManager.LoadGameDataCollection))]
    internal static class PostLoadGameDataCollectionPatch
    {
        static void Postfix(GameDataCollections __result)
        {
            SkinLoaderV2.Instance.Initialize(__result);
        }
    }
}

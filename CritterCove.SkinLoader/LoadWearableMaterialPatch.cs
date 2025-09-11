using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CritterCove.SkinLoader
{
    [HarmonyPatch(typeof(AddressableHelper), nameof(AddressableHelper.LoadWearableMaterial))]
    internal static class LoadWearableMaterialPatch
    {
        public static Material LoadWearableMaterialOrig(string id)
        {
            throw new NotImplementedException("This is a stub.");
        }

        static bool Prefix(ref Material __result, string id)
        {
            __result = SkinLoaderV2.Instance.GetMaterial(id);
            return __result == null;
        }
    }
}

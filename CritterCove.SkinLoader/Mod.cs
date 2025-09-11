using HarmonyLib;
using MelonLoader;
using System;

namespace CritterCove.SkinLoader
{
    internal class Mod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            // Get rid of buggy MelonLoader patch for ReversePatch function
            HarmonyInstance.Unpatch(AccessTools.Method("HarmonyLib.PatchFunctions:ReversePatch"), HarmonyPatchType.Prefix, BuildInfo.Name);
            
            // Install reverse patches
            HarmonyInstance.CreateReversePatcher(AccessTools.Method($"AddressableHelper:LoadWearableMaterial"),
                AccessTools.Method(typeof(LoadWearableMaterialPatch), nameof(LoadWearableMaterialPatch.LoadWearableMaterialOrig)).ToNewHarmonyMethod())
                .Patch(HarmonyReversePatchType.Original);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Build") SkinLoaderV2.Instance.Reset();
        }
    }
}

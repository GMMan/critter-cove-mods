using HarmonyLib;
using MelonLoader;
using System;

namespace CritterCove.ColorPreSetManagerSaveLoad
{
    internal class Mod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            HarmonyInstance.Patch(AccessTools.Method(typeof(ColorPreSetManager), nameof(ColorPreSetManager.Load)),
                AccessTools.Method(typeof(ColorPreSetManagerEx), nameof(ColorPreSetManagerEx.LoadPrefix)).ToNewHarmonyMethod());
            HarmonyInstance.Patch(AccessTools.Method(typeof(ColorPreSetManager), nameof(ColorPreSetManager.Save)),
                AccessTools.Method(typeof(ColorPreSetManagerEx), nameof(ColorPreSetManagerEx.SavePrefix)).ToNewHarmonyMethod());
            HarmonyInstance.Patch(AccessTools.Method(typeof(GameSaveManager), nameof(GameSaveManager.Load)),
                postfix: AccessTools.Method(typeof(GameSaveManagerPatches), nameof(GameSaveManagerPatches.LoadPostfix)).ToNewHarmonyMethod());
            HarmonyInstance.Patch(AccessTools.Method(typeof(GameSaveManager), nameof(GameSaveManager.New)),
                postfix: AccessTools.Method(typeof(GameSaveManagerPatches), nameof(GameSaveManagerPatches.NewPostfix)).ToNewHarmonyMethod());
            HarmonyInstance.Patch(AccessTools.Method(typeof(GameSaveManager), nameof(GameSaveManager.Save), new Type[] { typeof(bool), typeof(string) }),
                postfix: AccessTools.Method(typeof(GameSaveManagerPatches), nameof(GameSaveManagerPatches.SavePostfix)).ToNewHarmonyMethod());
        }
    }
}

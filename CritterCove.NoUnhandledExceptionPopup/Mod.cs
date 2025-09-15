using GRS;
using HarmonyLib;
using MelonLoader;
using System;

namespace CritterCove.NoUnhandledExceptionPopup
{
    internal class Mod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            HarmonyInstance.Patch(AccessTools.Method(typeof(MasterPopUpController), nameof(MasterPopUpController.CloseActiveWindow)),
                AccessTools.Method(typeof(MasterPopUpControllerPatch), nameof(MasterPopUpControllerPatch.CloseActiveWindowPrefix)).ToNewHarmonyMethod());
            HarmonyInstance.Patch(AccessTools.Method(typeof(MasterPopUpController), nameof(MasterPopUpController.OpenWindow)),
                AccessTools.Method(typeof(MasterPopUpControllerPatch), nameof(MasterPopUpControllerPatch.OpenWindowPrefix)).ToNewHarmonyMethod());
        }
    }
}

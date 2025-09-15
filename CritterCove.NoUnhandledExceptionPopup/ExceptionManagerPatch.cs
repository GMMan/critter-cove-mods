using GRS;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CritterCove.NoUnhandledExceptionPopup
{
    [HarmonyPatch(typeof(ExceptionManager), "HandleLogCallback")]
    internal static class ExceptionManagerPatch
    {
        public static bool IsInsideHandleLogCallback { get; set; }

        // Transpilers unfortunately don't work when the runtime is using .NET Standard, so just shim all the things

        static void Prefix()
        {
            IsInsideHandleLogCallback = true;
        }

        static void Postfix()
        {
            IsInsideHandleLogCallback = false;
        }
    }
}

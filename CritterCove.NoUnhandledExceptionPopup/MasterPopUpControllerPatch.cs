using GRS;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CritterCove.NoUnhandledExceptionPopup
{
    internal static class MasterPopUpControllerPatch
    {
        public static bool CloseActiveWindowPrefix()
        {
            return !ExceptionManagerPatch.IsInsideHandleLogCallback;
        }

        public static bool OpenWindowPrefix(string id)
        {
            return !(ExceptionManagerPatch.IsInsideHandleLogCallback && id == "UnhandleException");
        }
    }
}

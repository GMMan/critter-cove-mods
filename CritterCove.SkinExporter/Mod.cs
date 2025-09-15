using GRS;
using HarmonyLib;
using MelonLoader;
using MelonLoader.NativeUtils.PEParser;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CritterCove.SkinExporter
{
    internal class Mod : MelonMod
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        static readonly MethodInfo PEUtils_ImageFirstSection = AccessTools.Method(typeof(PEUtils), "ImageFirstSection");

        bool engineModified;

        public override void OnInitializeMelon()
        {
            engineModified = ModifyMeshTransferIsReadable();
            if (!engineModified)
            {
                LoggerInstance.Error("Failed to modify engine so Mesh.isReadable is always loaded as true. Mod will not continue to install.");
            }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (!engineModified) return;

            if (sceneName == "Build")
            {
                CreateExportButton();
            }
        }

        static unsafe bool ModifyMeshTransferIsReadable()
        {
            IntPtr hModuleUnityPlayer = GetModuleHandle("UnityPlayer.dll");
            if (hModuleUnityPlayer == IntPtr.Zero) return false;
            ImageNtHeaders* ntHeaders = PEUtils.AnalyseModuleWin(hModuleUnityPlayer);
            object boxedTextSection = PEUtils_ImageFirstSection.Invoke(null, new object[] { (IntPtr)ntHeaders });
            ImageSectionHeader* textSection = (ImageSectionHeader*)Pointer.Unbox(boxedTextSection);

            IntPtr textSectionPtr = IntPtr.Add(hModuleUnityPlayer, textSection->virtualAddress);
            IntPtr patchPtr = IntPtr.Add(textSectionPtr, 0x1084f10);
            Span<byte> patchSpan = new Span<byte>(patchPtr.ToPointer(), 3);
            if (!patchSpan.SequenceEqual(new byte[] { 0x0f, 0xb6, 0x01 })) return false;

            if (!VirtualProtect(textSectionPtr, (UIntPtr)textSection->virtualSize, 0x40, out var oldProtect)) return false; // change protection to RWX
            new byte[] { 0xb0, 0x01, 0x90 }.CopyTo(patchSpan);
            VirtualProtect(textSectionPtr, (UIntPtr)textSection->virtualSize, oldProtect, out _);
            return true;
        }

        static void CreateExportButton()
        {
            // Get toggle_clothes button from character creation sheet
            var ccUi = UnityEngine.Object.FindAnyObjectByType<CharacterCreationSheetUI>(FindObjectsInactive.Include);
            var toggleClothesButton = ccUi.transform.Find("Container/BottomBar/toggle_clothes").gameObject;

            // Clone button and setup
            var exportButton = UnityEngine.Object.Instantiate(toggleClothesButton, toggleClothesButton.transform.parent);
            // Input and label
            var glyphUi = exportButton.GetComponent<ControllerGlyphUI>();
            glyphUi.Action = BindAction.map;
            var label = exportButton.transform.Find("clothes").GetComponent<UILabel>();
            label.LocalizationId = null;
            label.text = "Export";
            // Anchor
            var exportButtonWidget = exportButton.GetComponent<UIWidget>();
            exportButtonWidget.rightAnchor.absolute = -165;
            exportButtonWidget.ResetAndUpdateAnchors();
            // On click action
            var actionTween = exportButton.GetComponent<TweenScale>();
            actionTween.RemoveOnFinished(actionTween.onFinished[0]);
            actionTween.AddOnFinished(Exporter.Export);
        }
    }
}

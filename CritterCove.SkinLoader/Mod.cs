using MelonLoader;
using System;

namespace CritterCove.SkinLoader
{
    internal class Mod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonCoroutines.Start(SkinLoader.LoadAddressables());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CritterCove.ColorPreSetManagerSaveLoad
{
    internal static class GameSaveManagerPatches
    {
        public static void LoadPostfix()
        {
            ColorPreSetManager.Load();
        }

        public static void NewPostfix()
        {
            ColorPreSetManager.Load();
        }

        public static void SavePostfix()
        {
            ColorPreSetManager.Save();
        }
    }
}

using GRS;
using System;
using System.Collections.Generic;
using System.Text;

namespace CritterCove.SkinLoader
{
    public class MaterialParams
    {
        public string? Mask1Texture { get; set; }
        public string? Mask2Texture { get; set; }
        public bool EnableMask2 { get; set; }
        public bool InheritMask1Texture { get; set; }
        public bool InheritMask2Texture { get; set; }
        public bool InheritEnableMask2 { get; set; }
    }
}

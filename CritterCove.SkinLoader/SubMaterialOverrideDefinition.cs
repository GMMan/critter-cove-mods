using System;
using System.Collections.Generic;
using System.Text;

namespace CritterCove.SkinLoader
{
    public class SubMaterialOverrideDefinition
    {
        public string SubId { get; set; }
        public MaterialParams? SubMaterial { get; set; }
        public bool InheritSubMaterial { get; set; }
    }
}

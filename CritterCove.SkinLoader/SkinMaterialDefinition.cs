using GRS;
using System;
using System.Collections.Generic;
using System.Text;

namespace CritterCove.SkinLoader
{
    public class SkinMaterialDefinition
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        public string Name { get; set; }
        public List<SerializableColor?>? Colors { get; set; } = new List<SerializableColor?>();
        public bool InheritColors { get; set; }
        public string? Icon { get; set; }
        public bool InheritIcon { get; set; }
        public MaterialParams? BodyMaterial { get; set; }
        public MaterialParams? FeetMaterial { get; set; }
        public MaterialParams? HandsMaterial { get; set; }
        public MaterialParams? HeadMaterial { get; set; }
        public MaterialParams? TailMaterial { get; set; }
        public MaterialParams? EarMaterial { get; set; }
        public MaterialParams? BeakMaterial { get; set; }
        public bool InheritBodyMaterial { get; set; }
        public bool InheritFeetMaterial { get; set; }
        public bool InheritHandsMaterial { get; set; }
        public bool InheritHeadMaterial { get; set; }
        public bool InheritTailMaterial { get; set; }
        public bool InheritEarMaterial { get; set; }
        public bool InheritBeakMaterial { get; set; }
        public List<SubMaterialOverrideDefinition>? SubMaterials { get; set; }
        public bool AutoInheritAllSubMaterials { get; set; }
    }
}

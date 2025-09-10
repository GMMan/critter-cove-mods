using System;
using System.Collections.Generic;
using System.Text;

namespace CritterCove.SkinLoader
{
    public class CharacterCustomizations
    {
        public List<character_materials> Materials { get; set; } = new List<character_materials>();
        public List<character_materials_sub> MaterialsSubs { get; set; } = new List<character_materials_sub>();
        public List<character_material_filter> MaterialFilters { get; set; } = new List<character_material_filter>();

        public void Merge(CharacterCustomizations? other)
        {
            if (other == null) return;
            Materials.AddRange(other.Materials);
            MaterialsSubs.AddRange(other.MaterialsSubs);
            MaterialFilters.AddRange(other.MaterialFilters);
        }
    }
}

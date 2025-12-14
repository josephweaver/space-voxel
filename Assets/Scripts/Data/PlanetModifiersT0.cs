// File: PlanetModifiersT0.cs
// Unity 2022/2023 compatible

using UnityEngine;

namespace SpaceVoxel.Materials
{
    /// <summary>
    /// Tier 0 planet modifiers in additive score space.
    /// Keep magnitudes small so they bias without dominating category templates.
    /// </summary>
    public static class PlanetModifiersT0
    {
        public static MaterialPropertyVector Compute(PlanetProfileSO planet, MaterialCategory category, MaterialSubtype subtype)
        {
            var v = MaterialPropertyVector.CreateZero();
            v.EnsureInit();

            // Temperature band
            switch (planet.TempBand)
            {
                case PlanetTempBand.Cryo:
                    v[MaterialProperty.MaxTemperature] -= 0.03f;
                    v[MaterialProperty.Manufacturability] -= 0.02f;
                    break;

                case PlanetTempBand.Temperate:
                    break;

                case PlanetTempBand.Hot:
                    v[MaterialProperty.MaxTemperature] += 0.08f;
                    v[MaterialProperty.Manufacturability] -= 0.03f;
                    break;

                case PlanetTempBand.Extreme:
                    v[MaterialProperty.MaxTemperature] += 0.10f;
                    v[MaterialProperty.Manufacturability] -= 0.05f;
                    v[MaterialProperty.ErosionResistance] += 0.03f;
                    break;
            }

            // Pressure band
            switch (planet.PressureBand)
            {
                case PlanetPressureBand.Low:
                    v[MaterialProperty.Density] -= 0.02f;
                    break;
                case PlanetPressureBand.Normal:
                    break;
                case PlanetPressureBand.High:
                    v[MaterialProperty.Strength] += 0.05f;
                    v[MaterialProperty.Density] += 0.03f;
                    break;
            }

            // Atmosphere chemistry
            switch (planet.AtmoChem)
            {
                case PlanetAtmoChem.Reducing:
                    v[MaterialProperty.CorrosionResistance] += 0.02f;
                    break;

                case PlanetAtmoChem.Neutral:
                    break;

                case PlanetAtmoChem.Oxidizing:
                    // Metals suffer more than silicates in oxidizing environments.
                    if (category == MaterialCategory.Metal || subtype == MaterialSubtype.SulfideOre || subtype == MaterialSubtype.NativeMetal)
                        v[MaterialProperty.CorrosionResistance] -= 0.08f;
                    else
                        v[MaterialProperty.CorrosionResistance] -= 0.03f;

                    break;
            }

            // Hydrosphere
            switch (planet.Hydrosphere)
            {
                case HydrosphereType.None:
                    break;

                case HydrosphereType.Water:
                    if (category == MaterialCategory.Metal)
                        v[MaterialProperty.CorrosionResistance] -= 0.08f;
                    if (category == MaterialCategory.StoneSilicate)
                        v[MaterialProperty.CorrosionResistance] += 0.05f;
                    break;

                case HydrosphereType.Acidic:
                    if (category == MaterialCategory.Metal)
                        v[MaterialProperty.CorrosionResistance] -= 0.12f;
                    else
                        v[MaterialProperty.CorrosionResistance] -= 0.06f;
                    break;

                case HydrosphereType.Hydrocarbon:
                    if (category == MaterialCategory.Metal)
                        v[MaterialProperty.CorrosionResistance] += 0.02f;
                    break;
            }

            return v;
        }
    }
}

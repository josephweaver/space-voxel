// File: MaterialVisualGeneratorT0.cs
// Deterministically derives a visual profile from a MaterialInstance, a planet profile, and a requested state.

using UnityEngine;

namespace SpaceVoxel.Materials
{
    public static class MaterialVisualGeneratorT0
    {
        /// <summary>
        /// Generate a Tier 0 visual profile. Deterministic from planet seed + node seed + state.
        /// This does not allocate Unity Materials, it just outputs parameters.
        /// </summary>
        public static MaterialVisualProfile Generate(
            PlanetProfileSO planet,
            MaterialInstance mat,
            MaterialVisualState state,
            bool paintedOverride,
            Color paintColor,
            float paintStrength01)
        {
            uint seed = DeterministicRng.HashCombine(mat.PlanetSeed, mat.NodeSeed);
            seed = DeterministicRng.HashCombine(seed, (uint)state + 1u);
            var rng = new DeterministicRng(seed);

            mat.Properties.EnsureInit();

            float strength = mat.Properties[MaterialProperty.Strength];
            float maxTemp = mat.Properties[MaterialProperty.MaxTemperature];
            float thermK = mat.Properties[MaterialProperty.ThermalConductivity];
            float elecK = mat.Properties[MaterialProperty.ElectricalConductivity];
            float erosion = mat.Properties[MaterialProperty.ErosionResistance];
            float corrosion = mat.Properties[MaterialProperty.CorrosionResistance];
            float density = mat.Properties[MaterialProperty.Density];
            float manuf = mat.Properties[MaterialProperty.Manufacturability];

            // Base color from category palette, then bias by environment a bit.
            Color baseColor = MaterialVisualPalettesT0.PickBase(mat.Category, mat.Subtype, ref rng);

            // Oxidizing atmospheres tint metals toward rust tones a bit.
            if (planet != null && planet.AtmoChem == PlanetAtmoChem.Oxidizing && mat.Category == MaterialCategory.Metal)
            {
                baseColor = Color.Lerp(baseColor, new Color(0.45f, 0.22f, 0.18f), 0.15f * (1f - corrosion));
            }

            // Dirt, noise, roughness, metallic are mainly state-driven, with property influence.
            var profile = new MaterialVisualProfile
            {
                VisualState = state,
                BaseColor = baseColor,

                Metallic = ComputeMetallic(mat.Category, mat.Subtype, elecK),
                Smoothness = ComputeSmoothness(state, manuf, corrosion, mat.Category, mat.Subtype),

                NormalStrength = Mathf.Lerp(1.2f, 0.6f, manuf), // easier to manufacture tends to look smoother
                NoiseScale = Mathf.Lerp(0.9f, 2.8f, rng.Next01()),
                NoiseStrength = ComputeNoiseStrength(state, manuf, mat.Category),
                DirtStrength = ComputeDirtStrength(state, mat.Category),
                OxidationStrength = ComputeOxidationStrength(state, mat.Category, corrosion),

                HeatTintStrength = ComputeHeatTintStrength(state, maxTemp, erosion, mat.Category),
                HeatTintColor = new Color(0.95f, 0.55f, 0.20f),

                UsePaint = false,
                PaintColor = paintColor,
                PaintStrength = Mathf.Clamp01(paintStrength01)
            };

            // Refined and manufactured look is more uniform.
            // Slightly reduce color variation with increased manufacturability.
            profile.BaseColor = Color.Lerp(profile.BaseColor, new Color(profile.BaseColor.r, profile.BaseColor.g, profile.BaseColor.b), 1f);
            profile.BaseColor = Color.Lerp(profile.BaseColor, profile.BaseColor * Mathf.Lerp(0.85f, 1.05f, density), 0.10f);

            if (paintedOverride)
            {
                profile.VisualState = MaterialVisualState.PaintedPart;
                profile.UsePaint = true;
                profile.PaintStrength = Mathf.Clamp01(paintStrength01);
                profile.PaintColor = paintColor;

                // Paint should not fully erase metallic response, clamp metallic down a bit if paint is strong.
                float paint = profile.PaintStrength;
                profile.Metallic = Mathf.Lerp(profile.Metallic, Mathf.Min(profile.Metallic, 0.35f), 0.65f * paint);
                profile.Smoothness = Mathf.Lerp(profile.Smoothness, Mathf.Clamp01(profile.Smoothness + 0.15f), 0.30f * paint);
            }

            return profile;
        }

        private static float ComputeMetallic(MaterialCategory cat, MaterialSubtype subtype, float elecK)
        {
            if (cat == MaterialCategory.Metal)
            {
                // Oxides should read less metallic.
                float baseM = (subtype == MaterialSubtype.MetalOxide) ? 0.10f : 0.80f;
                // Electrical conductivity nudges metallic feel a bit.
                return Mathf.Clamp01(baseM + 0.15f * (elecK - 0.5f));
            }

            // Stone and sediments are mostly non-metallic.
            if (cat == MaterialCategory.StoneSilicate) return 0.02f;
            if (cat == MaterialCategory.SedimentaryCarbon) return 0.05f;

            // Liquids and gases are not rendered as metallic surfaces typically.
            return 0.00f;
        }

        private static float ComputeSmoothness(MaterialVisualState state, float manuf, float corrosion, MaterialCategory cat, MaterialSubtype subtype)
        {
            // Smoothness is mostly about processing state and manufacturability.
            float baseSm = state switch
            {
                MaterialVisualState.RawOre => 0.10f,
                MaterialVisualState.ConcentratedOre => 0.18f,
                MaterialVisualState.RefinedStock => 0.35f,
                MaterialVisualState.ManufacturedPart => 0.55f,
                MaterialVisualState.PaintedPart => 0.60f,
                _ => 0.35f
            };

            // Metals can polish, stone stays rougher.
            float catBias = cat switch
            {
                MaterialCategory.Metal => 0.10f,
                MaterialCategory.StoneSilicate => -0.08f,
                MaterialCategory.SedimentaryCarbon => -0.05f,
                _ => 0f
            };

            // Poor corrosion resistance yields a slightly rougher look over time.
            float corrosionPenalty = 0.10f * (1f - corrosion);

            return Mathf.Clamp01(baseSm + catBias + 0.20f * (manuf - 0.5f) - corrosionPenalty);
        }

        private static float ComputeNoiseStrength(MaterialVisualState state, float manuf, MaterialCategory cat)
        {
            float baseNoise = state switch
            {
                MaterialVisualState.RawOre => 0.85f,
                MaterialVisualState.ConcentratedOre => 0.65f,
                MaterialVisualState.RefinedStock => 0.35f,
                MaterialVisualState.ManufacturedPart => 0.20f,
                MaterialVisualState.PaintedPart => 0.10f,
                _ => 0.35f
            };

            // Easier manufacturing implies cleaner surfaces.
            baseNoise *= Mathf.Lerp(1.15f, 0.75f, manuf);

            // Stone should keep some texture even when refined.
            if (cat == MaterialCategory.StoneSilicate && state >= MaterialVisualState.RefinedStock)
                baseNoise = Mathf.Max(baseNoise, 0.22f);

            return Mathf.Clamp01(baseNoise);
        }

        private static float ComputeDirtStrength(MaterialVisualState state, MaterialCategory cat)
        {
            float baseDirt = state switch
            {
                MaterialVisualState.RawOre => 0.70f,
                MaterialVisualState.ConcentratedOre => 0.45f,
                MaterialVisualState.RefinedStock => 0.15f,
                MaterialVisualState.ManufacturedPart => 0.05f,
                MaterialVisualState.PaintedPart => 0.03f,
                _ => 0.15f
            };

            // Sedimentary and stone tend to hold grime.
            if (cat == MaterialCategory.SedimentaryCarbon || cat == MaterialCategory.StoneSilicate)
                baseDirt = Mathf.Clamp01(baseDirt + 0.10f);

            return baseDirt;
        }

        private static float ComputeOxidationStrength(MaterialVisualState state, MaterialCategory cat, float corrosionResistance)
        {
            if (cat != MaterialCategory.Metal) return 0f;

            float baseOx = state switch
            {
                MaterialVisualState.RawOre => 0.60f,
                MaterialVisualState.ConcentratedOre => 0.45f,
                MaterialVisualState.RefinedStock => 0.20f,
                MaterialVisualState.ManufacturedPart => 0.10f,
                MaterialVisualState.PaintedPart => 0.05f,
                _ => 0.20f
            };

            // If corrosion resistance is low, oxidation is more visible.
            baseOx *= Mathf.Lerp(1.20f, 0.60f, corrosionResistance);

            return Mathf.Clamp01(baseOx);
        }

        private static float ComputeHeatTintStrength(MaterialVisualState state, float maxTemp, float erosion, MaterialCategory cat)
        {
            // Heat tint is most visible on manufactured parts, especially metals.
            if (state < MaterialVisualState.ManufacturedPart) return 0f;
            if (cat != MaterialCategory.Metal) return 0.05f * (1f - maxTemp);

            // Lower maxTemp and lower erosion resistance means more heat damage tint.
            float heatVuln = 0.60f * (1f - maxTemp) + 0.40f * (1f - erosion);
            return Mathf.Clamp01(0.15f + 0.50f * heatVuln);
        }
    }
}

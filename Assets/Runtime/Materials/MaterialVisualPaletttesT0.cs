// File: MaterialVisualPalettesT0.cs
// Tier 0 deterministic palettes for category and subtype

using UnityEngine;

namespace SpaceVoxel.Materials
{
    public static class MaterialVisualPalettesT0
    {
        // Small curated palettes, the seed picks within these and then perturbs slightly.
        private static readonly Color[] MetalPalette =
        {
            new Color(0.60f, 0.60f, 0.62f), // steel gray
            new Color(0.55f, 0.58f, 0.62f), // cool gray
            new Color(0.55f, 0.52f, 0.48f), // bronze-ish
            new Color(0.45f, 0.48f, 0.50f), // dark gray
        };

        private static readonly Color[] MetalOxidePalette =
        {
            new Color(0.52f, 0.25f, 0.18f), // rust
            new Color(0.45f, 0.30f, 0.15f), // ochre brown
            new Color(0.20f, 0.20f, 0.20f), // black oxide
            new Color(0.35f, 0.22f, 0.18f), // dark rust
        };

        private static readonly Color[] StonePalette =
        {
            new Color(0.55f, 0.52f, 0.48f), // granite tan
            new Color(0.42f, 0.42f, 0.44f), // gray
            new Color(0.18f, 0.18f, 0.20f), // basalt
            new Color(0.60f, 0.58f, 0.54f), // light stone
        };

        private static readonly Color[] CarbonPalette =
        {
            new Color(0.12f, 0.12f, 0.12f), // charcoal
            new Color(0.20f, 0.17f, 0.14f), // dark brown
            new Color(0.08f, 0.08f, 0.09f), // near black
            new Color(0.18f, 0.18f, 0.20f), // graphite-ish
        };

        private static readonly Color[] LiquidPalette =
        {
            new Color(0.20f, 0.35f, 0.45f), // bluish
            new Color(0.30f, 0.25f, 0.18f), // amber
            new Color(0.20f, 0.30f, 0.22f), // greenish
            new Color(0.35f, 0.35f, 0.35f), // murky
        };

        private static readonly Color[] GasPalette =
        {
            new Color(0.60f, 0.70f, 0.80f), // pale blue
            new Color(0.75f, 0.75f, 0.75f), // light gray
            new Color(0.80f, 0.65f, 0.55f), // pale orange
            new Color(0.70f, 0.80f, 0.70f), // pale green
        };

        public static Color PickBase(MaterialCategory cat, MaterialSubtype subtype, ref DeterministicRng rng)
        {
            Color[] palette = cat switch
            {
                MaterialCategory.Metal => (subtype == MaterialSubtype.MetalOxide) ? MetalOxidePalette : MetalPalette,
                MaterialCategory.StoneSilicate => StonePalette,
                MaterialCategory.SedimentaryCarbon => CarbonPalette,
                MaterialCategory.Liquid => LiquidPalette,
                MaterialCategory.Gas => GasPalette,
                _ => StonePalette
            };

            var c = palette[rng.NextInt(0, palette.Length)];

            // Small deterministic perturbation to avoid identical repeats.
            float dh = (rng.Next01() - 0.5f) * 0.04f;
            float ds = (rng.Next01() - 0.5f) * 0.06f;
            float dv = (rng.Next01() - 0.5f) * 0.06f;

            Color.RGBToHSV(c, out float h, out float s, out float v);
            h = Mathf.Repeat(h + dh, 1f);
            s = Mathf.Clamp01(s + ds);
            v = Mathf.Clamp01(v + dv);
            return Color.HSVToRGB(h, s, v);
        }
    }
}

// File: PlanetProfileSO.cs
// Unity 2022/2023 compatible

using UnityEngine;

namespace SpaceVoxel.Materials
{
    [CreateAssetMenu(menuName = "SpaceVoxel/Materials/Planet Profile", fileName = "PlanetProfile")]
    public class PlanetProfileSO : ScriptableObject
    {
        [Header("Planet Bands")]
        public PlanetTempBand TempBand = PlanetTempBand.Temperate;
        public PlanetPressureBand PressureBand = PlanetPressureBand.Normal;
        public PlanetAtmoChem AtmoChem = PlanetAtmoChem.Neutral;
        public HydrosphereType Hydrosphere = HydrosphereType.Water;

        [Header("Category Weights (Tier 0)")]
        [Tooltip("Relative weights for choosing a material category on this planet at Tier 0.")]
        public float WeightMetal = 1.0f;
        public float WeightStoneSilicate = 2.0f;
        public float WeightSedimentaryCarbon = 1.0f;
        public float WeightLiquid = 1.0f;
        public float WeightGas = 1.0f;

        [Header("Seed")]
        [Tooltip("Stable seed for this planet profile. Keep constant for deterministic generation.")]
        public uint PlanetSeed = 12345u;
    }
}

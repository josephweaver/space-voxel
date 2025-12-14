// File: MaterialEnums.cs
// Unity 2022/2023 compatible

using System;

namespace SpaceVoxel.Materials
{
    public enum MaterialCategory
    {
        Metal,
        StoneSilicate,
        SedimentaryCarbon,
        Liquid,
        Gas
    }

    public enum MaterialSubtype
    {
        // Metal family
        NativeMetal,
        MetalOxide,
        SulfideOre,

        // Stone family
        SilicateRock,
        VolcanicGlass,
        CeramicMineral,

        // Sedimentary family
        CarbonRich,
        OrganicSediment,

        // Liquid family
        WaterBrine,
        HydrocarbonLiquid,
        ReactiveSolvent,

        // Gas family
        InertGas,
        ReactiveGas
    }

    public enum PlanetTempBand { Cryo, Temperate, Hot, Extreme }
    public enum PlanetPressureBand { Low, Normal, High }
    public enum PlanetAtmoChem { Reducing, Neutral, Oxidizing }
    public enum HydrosphereType { None, Water, Acidic, Hydrocarbon }

    public enum MaterialProperty
    {
        Strength,             // S
        MaxTemperature,       // Tm
        ThermalConductivity,  // Kt
        ElectricalConductivity,// Ke
        EnergyPotential,       //Ep
        ErosionResistance,    // Er
        CorrosionResistance,  // Cr
        Density,              // D
        Manufacturability     // Mf
    }

    public static class MaterialPropertyCount
    {
        public const int Count = 8;
    }
}

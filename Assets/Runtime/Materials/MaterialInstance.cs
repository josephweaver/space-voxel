// File: MaterialInstance.cs
// Unity 2022/2023 compatible

using System;
using UnityEngine;

namespace SpaceVoxel.Materials
{
    [Serializable]
    public class MaterialInstance
    {
        [Header("Identity")]
        public string DisplayName;
        public MaterialCategory Category;
        public MaterialSubtype Subtype;

        [Header("World Context")]
        public uint PlanetSeed;
        public uint NodeSeed;

        [Header("Gameplay")]
        [Range(0f, 1f)] public float EstimatedPlanetaryAbundance; // abstracted, can be bucketed for UI
        public MaterialProperty DominantProperty;
        public MaterialProperty WeaknessProperty;

        [Header("Properties (Tier 0 normalized)")]
        public MaterialPropertyVector Properties;

        public override string ToString()
        {
            return $"{DisplayName} [{Category}/{Subtype}] Dom={DominantProperty} Weak={WeaknessProperty} Props=({Properties})";
        }
    }
}

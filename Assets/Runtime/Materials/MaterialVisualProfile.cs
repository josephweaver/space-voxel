// File: MaterialVisualProfile.cs
// Unity 2022/2023, URP compatible

using System;
using UnityEngine;

namespace SpaceVoxel.Materials
{
    public enum MaterialVisualState
    {
        RawOre,
        ConcentratedOre,
        RefinedStock,      // ingot, powder, pellet
        ManufacturedPart,  // machined, assembled
        PaintedPart        // override coat
    }

    [Serializable]
    public struct MaterialVisualProfile
    {
        public MaterialVisualState VisualState;

        public Color BaseColor;
        [Range(0f, 1f)] public float Metallic;
        [Range(0f, 1f)] public float Smoothness;

        [Range(0f, 2f)] public float NormalStrength;

        [Range(0f, 5f)] public float NoiseScale;
        [Range(0f, 1f)] public float NoiseStrength;

        [Range(0f, 1f)] public float DirtStrength;
        [Range(0f, 1f)] public float OxidationStrength;

        [Range(0f, 1f)] public float HeatTintStrength;
        public Color HeatTintColor;

        // Optional paint overlay, use only when VisualState == PaintedPart
        public bool UsePaint;
        public Color PaintColor;
        [Range(0f, 1f)] public float PaintStrength;

        public override string ToString()
        {
            return $"{VisualState} Base={BaseColor} M={Metallic:0.00} S={Smoothness:0.00} Noise={NoiseStrength:0.00} Dirt={DirtStrength:0.00} Ox={OxidationStrength:0.00} Heat={HeatTintStrength:0.00} Paint={UsePaint}";
        }
    }
}

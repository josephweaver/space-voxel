// File: MaterialCategoryTemplateSO.cs
// Unity 2022/2023 compatible

using System;
using UnityEngine;

namespace SpaceVoxel.Materials
{
    [Serializable]
    public struct PropertyTemplate
    {
        [Range(0f, 1f)] public float Mean;
        [Range(0f, 1f)] public float Spread;
    }

    [Serializable]
    public struct SubtypeAdjustment
    {
        public MaterialSubtype Subtype;

        [Header("Additive Adjustments (in score space)")]
        public float Strength;
        public float MaxTemperature;
        public float ThermalConductivity;
        public float ElectricalConductivity;
        public float EnergyPotential;
        public float ErosionResistance;
        public float CorrosionResistance;
        public float Density;
        public float Manufacturability;

        public MaterialPropertyVector ToVector()
        {
            var v = MaterialPropertyVector.CreateZero();
            v.EnsureInit();
            v[MaterialProperty.Strength] = Strength;
            v[MaterialProperty.MaxTemperature] = MaxTemperature;
            v[MaterialProperty.ThermalConductivity] = ThermalConductivity;
            v[MaterialProperty.ElectricalConductivity] = ElectricalConductivity;
            v[MaterialProperty.EnergyPotential]= EnergyPotential;
            v[MaterialProperty.ErosionResistance] = ErosionResistance;
            v[MaterialProperty.CorrosionResistance] = CorrosionResistance;
            v[MaterialProperty.Density] = Density;
            v[MaterialProperty.Manufacturability] = Manufacturability;
            return v;
        }
    }

    [CreateAssetMenu(menuName = "SpaceVoxel/Materials/Category Template (Tier 0)", fileName = "MaterialCategoryTemplate_T0")]
    public class MaterialCategoryTemplateSO : ScriptableObject
    {
        public MaterialCategory Category;

        [Header("Base Means and Spreads (Tier 0, normalized [0,1])")]
        public PropertyTemplate Strength;
        public PropertyTemplate MaxTemperature;
        public PropertyTemplate ThermalConductivity;
        public PropertyTemplate ElectricalConductivity;
        public PropertyTemplate EnergyPotential;
        public PropertyTemplate ErosionResistance;
        public PropertyTemplate CorrosionResistance;
        public PropertyTemplate Density;
        public PropertyTemplate Manufacturability;

        [Header("Dominant and Weakness Sets")]
        public MaterialProperty[] DominantCandidates;
        public MaterialProperty[] WeaknessCandidates;

        [Header("Subtype Adjustments")]
        public SubtypeAdjustment[] SubtypeAdjustments;

        public MaterialPropertyVector GetMeanVector()
        {
            var v = MaterialPropertyVector.CreateZero();
            v[MaterialProperty.Strength] = Strength.Mean;
            v[MaterialProperty.MaxTemperature] = MaxTemperature.Mean;
            v[MaterialProperty.ThermalConductivity] = ThermalConductivity.Mean;
            v[MaterialProperty.ElectricalConductivity] = ElectricalConductivity.Mean;
            v[MaterialProperty.EnergyPotential]= EnergyPotential.Mean;
            v[MaterialProperty.ErosionResistance] = ErosionResistance.Mean;
            v[MaterialProperty.CorrosionResistance] = CorrosionResistance.Mean;
            v[MaterialProperty.Density] = Density.Mean;
            v[MaterialProperty.Manufacturability] = Manufacturability.Mean;
            return v;
        }

        public MaterialPropertyVector GetSpreadVector()
        {
            var v = MaterialPropertyVector.CreateZero();
            v[MaterialProperty.Strength] = Strength.Spread;
            v[MaterialProperty.MaxTemperature] = MaxTemperature.Spread;
            v[MaterialProperty.ThermalConductivity] = ThermalConductivity.Spread;
            v[MaterialProperty.ElectricalConductivity] = ElectricalConductivity.Spread;
            v[MaterialProperty.EnergyPotential] = EnergyPotential.Spread;
            v[MaterialProperty.ErosionResistance] = ErosionResistance.Spread;
            v[MaterialProperty.CorrosionResistance] = CorrosionResistance.Spread;
            v[MaterialProperty.Density] = Density.Spread;
            v[MaterialProperty.Manufacturability] = Manufacturability.Spread;
            return v;
        }

        public bool TryGetSubtypeAdjustment(MaterialSubtype subtype, out MaterialPropertyVector adj)
        {
            if (SubtypeAdjustments != null)
            {
                for (int i = 0; i < SubtypeAdjustments.Length; i++)
                {
                    if (SubtypeAdjustments[i].Subtype == subtype)
                    {
                        adj = SubtypeAdjustments[i].ToVector();
                        return true;
                    }
                }
            }

            adj = MaterialPropertyVector.CreateZero();
            return false;
        }
    }
}

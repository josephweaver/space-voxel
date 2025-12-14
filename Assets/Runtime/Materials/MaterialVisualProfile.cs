// File: MaterialGeneratorT0.cs
// Unity 2022/2023 compatible

using System;
using UnityEngine;

namespace SpaceVoxel.Materials
{
    /// <summary>
    /// Tier 0 material generator. Deterministic from planet seed and node seed.
    /// Expects you to provide category templates via ScriptableObjects.
    /// </summary>
    public class MaterialGeneratorT0
    {
        private readonly MaterialCategoryTemplateSO _metal;
        private readonly MaterialCategoryTemplateSO _stone;
        private readonly MaterialCategoryTemplateSO _sed;
        private readonly MaterialCategoryTemplateSO _liquid;
        private readonly MaterialCategoryTemplateSO _gas;

        public MaterialGeneratorT0(
            MaterialCategoryTemplateSO metal,
            MaterialCategoryTemplateSO stone,
            MaterialCategoryTemplateSO sedimentary,
            MaterialCategoryTemplateSO liquid,
            MaterialCategoryTemplateSO gas)
        {
            _metal = metal;
            _stone = stone;
            _sed = sedimentary;
            _liquid = liquid;
            _gas = gas;
        }

        public MaterialInstance Generate(PlanetProfileSO planet, uint nodeSeed, string baseName)
        {
            if (planet == null) throw new ArgumentNullException(nameof(planet));

            uint seed = DeterministicRng.HashCombine(planet.PlanetSeed, nodeSeed);
            var rng = new DeterministicRng(seed);

            // 1) Choose category
            MaterialCategory cat = ChooseCategory(planet, ref rng);

            // 2) Choose subtype compatible with category
            MaterialSubtype subtype = ChooseSubtype(cat, ref rng);

            // 3) Get category template
            var tpl = GetTemplate(cat);
            if (tpl == null) throw new InvalidOperationException($"Missing template for category {cat}");

            // 4) Sample base properties
            MaterialPropertyVector props = SampleFromTemplate(tpl, ref rng);

            // 5) Apply subtype adjustments
            if (tpl.TryGetSubtypeAdjustment(subtype, out var adj))
                props.Add(adj);

            // 6) Apply planet modifiers
            props.Add(PlanetModifiersT0.Compute(planet, cat, subtype));

            // 7) Choose dominant and weakness (before caps, then apply boosts/penalties)
            var dominant = ChooseDominant(tpl, props);
            var weakness = ChooseWeakness(tpl, props);

            props[dominant] += TierRulesT0.DominantBoost;
            props[weakness] -= TierRulesT0.WeaknessPenalty;

            // 8) Clamp and enforce Tier 0 rules
            props.Clamp01();
            TierRulesT0.ApplyHardCaps(ref props);
            TierRulesT0.EnforceOneDominant(ref props);
            TierRulesT0.EnsureAWeakness(ref props);

            // 9) Abundance (simple deterministic roll biased by category weight)
            float abundance = EstimateAbundance(cat, ref rng);

            // 10) Build instance
            var inst = new MaterialInstance
            {
                DisplayName = MakeDisplayName(baseName, cat, subtype, nodeSeed),
                Category = cat,
                Subtype = subtype,
                PlanetSeed = planet.PlanetSeed,
                NodeSeed = nodeSeed,
                EstimatedPlanetaryAbundance = abundance,
                DominantProperty = dominant,
                WeaknessProperty = weakness,
                Properties = props
            };

            return inst;
        }

        private static MaterialCategory ChooseCategory(PlanetProfileSO planet, ref DeterministicRng rng)
        {
            float wM = Mathf.Max(0f, planet.WeightMetal);
            float wS = Mathf.Max(0f, planet.WeightStoneSilicate);
            float wC = Mathf.Max(0f, planet.WeightSedimentaryCarbon);
            float wL = Mathf.Max(0f, planet.WeightLiquid);
            float wG = Mathf.Max(0f, planet.WeightGas);

            float sum = wM + wS + wC + wL + wG;
            if (sum <= 0f) return MaterialCategory.StoneSilicate;

            float r = rng.Next01() * sum;
            if ((r -= wM) < 0f) return MaterialCategory.Metal;
            if ((r -= wS) < 0f) return MaterialCategory.StoneSilicate;
            if ((r -= wC) < 0f) return MaterialCategory.SedimentaryCarbon;
            if ((r -= wL) < 0f) return MaterialCategory.Liquid;
            return MaterialCategory.Gas;
        }

        private static MaterialSubtype ChooseSubtype(MaterialCategory cat, ref DeterministicRng rng)
        {
            switch (cat)
            {
                case MaterialCategory.Metal:
                {
                    MaterialSubtype[] opts = { MaterialSubtype.NativeMetal, MaterialSubtype.MetalOxide, MaterialSubtype.SulfideOre };
                    return opts[rng.NextInt(0, opts.Length)];
                }
                case MaterialCategory.StoneSilicate:
                {
                    MaterialSubtype[] opts = { MaterialSubtype.SilicateRock, MaterialSubtype.VolcanicGlass, MaterialSubtype.CeramicMineral };
                    return opts[rng.NextInt(0, opts.Length)];
                }
                case MaterialCategory.SedimentaryCarbon:
                {
                    MaterialSubtype[] opts = { MaterialSubtype.CarbonRich, MaterialSubtype.OrganicSediment };
                    return opts[rng.NextInt(0, opts.Length)];
                }
                case MaterialCategory.Liquid:
                {
                    MaterialSubtype[] opts = { MaterialSubtype.WaterBrine, MaterialSubtype.HydrocarbonLiquid, MaterialSubtype.ReactiveSolvent };
                    return opts[rng.NextInt(0, opts.Length)];
                }
                case MaterialCategory.Gas:
                default:
                {
                    MaterialSubtype[] opts = { MaterialSubtype.InertGas, MaterialSubtype.ReactiveGas };
                    return opts[rng.NextInt(0, opts.Length)];
                }
            }
        }

        private MaterialCategoryTemplateSO GetTemplate(MaterialCategory cat)
        {
            return cat switch
            {
                MaterialCategory.Metal => _metal,
                MaterialCategory.StoneSilicate => _stone,
                MaterialCategory.SedimentaryCarbon => _sed,
                MaterialCategory.Liquid => _liquid,
                MaterialCategory.Gas => _gas,
                _ => null
            };
        }

        private static MaterialPropertyVector SampleFromTemplate(MaterialCategoryTemplateSO tpl, ref DeterministicRng rng)
        {
            MaterialPropertyVector mu = tpl.GetMeanVector();
            MaterialPropertyVector sig = tpl.GetSpreadVector();
            mu.EnsureInit();
            sig.EnsureInit();

            var v = MaterialPropertyVector.CreateZero();
            v.EnsureInit();

            for (int i = 0; i < MaterialPropertyCount.Count; i++)
            {
                float tri = rng.Tri01();          // [0,1]
                float centered = (2f * tri) - 1f; // [-1,1]
                float x = mu.Get(i) + sig.Get(i) * centered;
                v.Set(i, x);
            }

            v.Clamp01();
            return v;
        }

        private static MaterialProperty ChooseDominant(MaterialCategoryTemplateSO tpl, MaterialPropertyVector props)
        {
            props.EnsureInit();

            if (tpl.DominantCandidates == null || tpl.DominantCandidates.Length == 0)
                return MaterialProperty.Strength;

            MaterialProperty best = tpl.DominantCandidates[0];
            float bestVal = props[best];

            for (int i = 1; i < tpl.DominantCandidates.Length; i++)
            {
                var p = tpl.DominantCandidates[i];
                float v = props[p];
                if (v > bestVal) { bestVal = v; best = p; }
            }

            return best;
        }

        private static MaterialProperty ChooseWeakness(MaterialCategoryTemplateSO tpl, MaterialPropertyVector props)
        {
            props.EnsureInit();

            if (tpl.WeaknessCandidates == null || tpl.WeaknessCandidates.Length == 0)
                return MaterialProperty.Manufacturability;

            MaterialProperty worst = tpl.WeaknessCandidates[0];
            float worstVal = props[worst];

            for (int i = 1; i < tpl.WeaknessCandidates.Length; i++)
            {
                var p = tpl.WeaknessCandidates[i];
                float v = props[p];
                if (v < worstVal) { worstVal = v; worst = p; }
            }

            return worst;
        }

        private static float EstimateAbundance(MaterialCategory cat, ref DeterministicRng rng)
        {
            // Tier 0: just provide a deterministic "commonness" hint.
            // Metals slightly less common on average than stone/silicate.
            float baseMean = cat switch
            {
                MaterialCategory.Metal => 0.35f,
                MaterialCategory.StoneSilicate => 0.65f,
                MaterialCategory.SedimentaryCarbon => 0.45f,
                MaterialCategory.Liquid => 0.50f,
                MaterialCategory.Gas => 0.55f,
                _ => 0.50f
            };

            float tri = rng.Tri01();
            float x = Mathf.Clamp01(baseMean + 0.25f * ((2f * tri) - 1f));
            return x;
        }

        private static string MakeDisplayName(string baseName, MaterialCategory cat, MaterialSubtype subtype, uint nodeSeed)
        {
            // Keep names deterministic but simple.
            // You can later add a proper name generator.
            string catStr = cat.ToString();
            string subStr = subtype.ToString();
            string suffix = (nodeSeed % 10000u).ToString("0000");
            return $"{baseName} ({catStr}:{subStr}) #{suffix}";
        }
    }
}

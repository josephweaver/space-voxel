// File: TierRulesT0.cs
// Unity 2022/2023 compatible

using UnityEngine;

namespace SpaceVoxel.Materials
{
    /// <summary>
    /// Tier 0 scoring constants and enforcement rules.
    /// Keep this small and explicit so you can version it later per tier.
    /// </summary>
    public static class TierRulesT0
    {
        public const float HardCap = 0.80f;

        public const float DominantBoost = 0.10f;
        public const float WeaknessPenalty = 0.10f;

        public const float DominantThreshold = 0.85f; // used for one-dominant constraint
        public const float WeaknessRequiredMax = 0.35f;

        public static float CapDensityMin = 0.10f;
        public static float CapDensityMax = 0.90f;

        public static void ApplyHardCaps(ref MaterialPropertyVector v)
        {
            v.EnsureInit();

            for (int i = 0; i < MaterialPropertyCount.Count; i++)
            {
                var p = (MaterialProperty)i;
                float x = v.Get(i);

                if (p == MaterialProperty.Density)
                    x = Mathf.Clamp(x, CapDensityMin, CapDensityMax);
                else
                    x = Mathf.Min(x, HardCap);

                v.Set(i, Mathf.Clamp01(x));
            }
        }

        public static void EnforceOneDominant(ref MaterialPropertyVector v)
        {
            v.EnsureInit();

            // If multiple properties exceed DominantThreshold, squash all but the max.
            int maxIdx = 0;
            float maxVal = v.Get(0);
            for (int i = 1; i < MaterialPropertyCount.Count; i++)
            {
                float x = v.Get(i);
                if (x > maxVal) { maxVal = x; maxIdx = i; }
            }

            int countAbove = 0;
            for (int i = 0; i < MaterialPropertyCount.Count; i++)
                if (v.Get(i) > DominantThreshold) countAbove++;

            if (countAbove <= 1) return;

            for (int i = 0; i < MaterialPropertyCount.Count; i++)
            {
                if (i == maxIdx) continue;
                float x = v.Get(i);
                if (x > DominantThreshold)
                    v.Set(i, DominantThreshold);
            }
        }

        public static void EnsureAWeakness(ref MaterialPropertyVector v)
        {
            v.EnsureInit();

            float minVal = v.Get(0);
            for (int i = 1; i < MaterialPropertyCount.Count; i++)
                minVal = Mathf.Min(minVal, v.Get(i));

            if (minVal <= WeaknessRequiredMax) return;

            // If nothing is weak enough, gently lower the global minimum property.
            int minIdx = 0;
            float mv = v.Get(0);
            for (int i = 1; i < MaterialPropertyCount.Count; i++)
            {
                float x = v.Get(i);
                if (x < mv) { mv = x; minIdx = i; }
            }

            v.Set(minIdx, Mathf.Max(0f, WeaknessRequiredMax));
        }
    }
}

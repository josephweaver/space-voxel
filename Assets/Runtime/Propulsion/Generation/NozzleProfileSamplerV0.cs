using System;
using System.Collections.Generic;
using UnityEngine;

namespace IR.Propulsion.Generation
{
    /// <summary>
    /// Produces a 2D axial profile for a nozzle: list of (z, r) points.
    /// V0: piecewise profile with a throat region and a conical diverging section,
    /// plus optional deterministic jitter on the exit flare.
    /// </summary>
    public static class NozzleProfileSamplerV0
    {
        [Serializable]
        public struct Profile2D
        {
            // Each point is (z, r)
            public List<Vector2> zr;

            public float Length => (zr == null || zr.Count == 0) ? 0f : zr[zr.Count - 1].x;
            public float MinRadius()
            {
                if (zr == null || zr.Count == 0) return 0f;
                float m = float.PositiveInfinity;
                for (int i = 0; i < zr.Count; i++) m = Mathf.Min(m, zr[i].y);
                return (float.IsPositiveInfinity(m)) ? 0f : m;
            }
            public float MaxRadius()
            {
                if (zr == null || zr.Count == 0) return 0f;
                float m = 0f;
                for (int i = 0; i < zr.Count; i++) m = Mathf.Max(m, zr[i].y);
                return m;
            }
        }

        /// <summary>
        /// Generate a simple nozzle profile.
        /// Inputs are geometric, not physics-correct, by design for Tier 0.
        /// </summary>
        /// <param name="seed">Deterministic seed.</param>
        /// <param name="length">Nozzle length along +Z.</param>
        /// <param name="throatRadius">Radius at throat.</param>
        /// <param name="exitRadius">Radius at exit plane.</param>
        /// <param name="axialSamples">Number of samples along z (minimum 2).</param>
        /// <param name="throatCurvatureFactor">0..1 controls how rounded the throat region is.</param>
        /// <param name="flareJitter">0..1 small variation near exit.</param>
        public static Profile2D Sample(
            int seed,
            float length,
            float throatRadius,
            float exitRadius,
            int axialSamples,
            float throatCurvatureFactor,
            float flareJitter
        )
        {
            // Defensive clamps
            length = Mathf.Max(0.01f, length);
            throatRadius = Mathf.Max(0.001f, throatRadius);
            exitRadius = Mathf.Max(throatRadius, exitRadius);
            axialSamples = Mathf.Max(2, axialSamples);

            throatCurvatureFactor = Mathf.Clamp01(throatCurvatureFactor);
            flareJitter = Mathf.Clamp01(flareJitter);

            // Define a small throat "blend" region as a fraction of length.
            // This is not physically rigorous, it is a controllable shape primitive.
            float blendFrac = Mathf.Lerp(0.05f, 0.25f, throatCurvatureFactor);
            float zBlend = length * blendFrac;

            var rng = new System.Random(seed);

            var points = new List<Vector2>(axialSamples);

            for (int i = 0; i < axialSamples; i++)
            {
                float t = (axialSamples == 1) ? 0f : (float)i / (axialSamples - 1);
                float z = t * length;

                float r;
                if (z <= zBlend)
                {
                    // Rounded throat region: ease from throatRadius to a slightly larger radius.
                    // This prevents the profile from being a hard corner at z=0.
                    float u = z / zBlend; // 0..1
                    float eased = SmoothStep(u);

                    // Let the radius expand a little within the throat region before diverging.
                    float preDivergeTarget = Mathf.Lerp(throatRadius, Mathf.Min(exitRadius, throatRadius * 1.25f), throatCurvatureFactor);
                    r = Mathf.Lerp(throatRadius, preDivergeTarget, eased);
                }
                else
                {
                    // Conical diverging region from radius at zBlend to exitRadius at length.
                    float u = (z - zBlend) / (length - zBlend); // 0..1

                    float rAtBlend = RadiusAtBlend(throatRadius, exitRadius, throatCurvatureFactor);
                    r = Mathf.Lerp(rAtBlend, exitRadius, u);

                    // Optional deterministic flare jitter near the exit (last ~20%).
                    // This gives you "minor flare random variations" without breaking determinism.
                    float exitBandStart = 0.8f;
                    if (flareJitter > 0f && u >= exitBandStart)
                    {
                        float v = (u - exitBandStart) / (1f - exitBandStart); // 0..1
                        float amp = flareJitter * 0.03f * exitRadius; // 3% max at flareJitter=1

                        // Signed noise in [-1, 1]
                        float noise = (float)(rng.NextDouble() * 2.0 - 1.0);

                        // Fade in jitter smoothly toward the exit.
                        r += amp * SmoothStep(v) * noise;
                        r = Mathf.Max(0.0001f, r);
                    }
                }

                points.Add(new Vector2(z, r));
            }

            // Ensure monotone z and non-negative radii (already ensured, but keep invariant explicit)
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].x < points[i - 1].x)
                    points[i] = new Vector2(points[i - 1].x, points[i].y);
                if (points[i].y < 0f)
                    points[i] = new Vector2(points[i].x, 0f);
            }

            return new Profile2D { zr = points };
        }

        private static float SmoothStep(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        private static float RadiusAtBlend(float throatRadius, float exitRadius, float throatCurvatureFactor)
        {
            // Same expression used in the throat blend section so the profile connects smoothly.
            return Mathf.Lerp(throatRadius, Mathf.Min(exitRadius, throatRadius * 1.25f), Mathf.Clamp01(throatCurvatureFactor));
        }
    }
}

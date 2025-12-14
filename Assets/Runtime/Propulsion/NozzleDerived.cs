// File: NozzleDerived.cs
// Unity 2022/2023, C#
// Derived quantities computed from NozzleSpecSO.
// This is intentionally "simple v0": consistent and deterministic, not a full rocket solver.

using System;
using UnityEngine;

namespace SpaceVoxel.Propulsion
{
    [Serializable]
    public struct NozzleDerived
    {
        [Header("Derived areas (m^2)")]
        public float ThroatArea_At_m2;
        public float ExitArea_Ae_m2;

        [Header("Derived radii (m)")]
        public float ThroatRadius_rt_m;
        public float ExitRadius_re_m;

        [Header("Derived lengths (m)")]
        public float NozzleLength_L_m;

        [Header("Derived coefficients (dimensionless, v0 approximations)")]
        public float ThrustCoefficient_Cf;
        public float EffectiveIsp_sl_s;
        public float EffectiveIsp_vac_s;

        public override string ToString()
        {
            return $"At={ThroatArea_At_m2:0.####} m^2, Ae={ExitArea_Ae_m2:0.####} m^2, rt={ThroatRadius_rt_m:0.###} m, re={ExitRadius_re_m:0.###} m, L={NozzleLength_L_m:0.###} m, Cf={ThrustCoefficient_Cf:0.###}";
        }
    }

    public static class NozzleDeriverV0
    {
        // v0 constants: these are "good enough" for shape + gameplay consistency.
        // You can later replace these with a real Cf/Isp solver.
        private const float kPaToPa = 1000f;
        private const float MPaToPa = 1_000_000f;

        // Rough baseline Cf by propellant class.
        // These are not meant to be accurate, just stable.
        private static float BaseCf(PropellantClass propellant)
        {
            return propellant switch
            {
                PropellantClass.Solid => 1.35f,
                PropellantClass.LoxRp1 => 1.45f,
                PropellantClass.LoxLh2 => 1.55f,
                PropellantClass.Methalox => 1.50f,
                PropellantClass.Hypergolic => 1.42f,
                PropellantClass.Exotic => 1.65f,
                _ => 1.45f
            };
        }

        // Very simple ambient influence: as Pa increases, effective Cf decreases slightly for a fixed nozzle.
        private static float AmbientCfFactor(float designAmbientPressure_kPa)
        {
            // 101.3 kPa -> ~0.90..0.95 range, vacuum -> 1.0
            float pa = Mathf.Clamp(designAmbientPressure_kPa, 0f, 200f);
            return Mathf.Lerp(1.00f, 0.92f, pa / 101.3f);
        }

        // Very simple epsilon influence: higher expansion ratio provides modest Cf gains in vacuum.
        private static float EpsilonCfFactor(float epsilon)
        {
            // epsilon in [1, 200], log growth
            float e = Mathf.Clamp(epsilon, 1.01f, 200f);
            float t = Mathf.Log(e) / Mathf.Log(200f); // 0..1
            return Mathf.Lerp(0.92f, 1.06f, t);
        }

        public static NozzleDerived Compute(NozzleSpecSO spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            spec.ClampForSafety();

            // Inputs
            float F_N = spec.DesignThrust_kN * 1000f;
            float Pc_Pa = spec.ChamberPressure_MPa * MPaToPa;
            float epsilon = spec.ExpansionRatio;

            // v0 approximate Cf model
            float cf = BaseCf(spec.Propellant) * AmbientCfFactor(spec.DesignAmbientPressure_kPa) * EpsilonCfFactor(epsilon);
            cf = Mathf.Clamp(cf, 0.8f, 2.2f);

            // Derive throat area from F = Cf * Pc * At
            float At = F_N / (cf * Pc_Pa);

            // Derive exit area
            float Ae = epsilon * At;

            // Radii from areas
            float rt = Mathf.Sqrt(At / Mathf.PI);
            float re = Mathf.Sqrt(Ae / Mathf.PI);

            // Length model: baseline depends on nozzle type and divergence angle.
            // For conical: L ~ (re - rt) / tan(theta)
            // For bell: shorter, scaled by LengthFactor.
            float thetaRad = Mathf.Deg2Rad * Mathf.Clamp(spec.DivergenceHalfAngle_deg, 5f, 35f);
            float L_conical = (re - rt) / Mathf.Max(0.05f, Mathf.Tan(thetaRad));
            float L_base = spec.Type == NozzleType.Conical ? L_conical : 0.8f * L_conical;
            float L = Mathf.Max(0.02f, spec.LengthFactor * L_base);

            // Placeholder Isp estimates: stable outputs for UI. Not accurate physics.
            // You can replace later with a real expansion + c* model.
            float ispVac = 240f + 120f * Mathf.Clamp01((cf - 1.0f) / 0.8f);
            float ispSl = ispVac * AmbientCfFactor(101.3f);

            return new NozzleDerived
            {
                ThroatArea_At_m2 = At,
                ExitArea_Ae_m2 = Ae,
                ThroatRadius_rt_m = rt,
                ExitRadius_re_m = re,
                NozzleLength_L_m = L,
                ThrustCoefficient_Cf = cf,
                EffectiveIsp_sl_s = ispSl,
                EffectiveIsp_vac_s = ispVac
            };
        }
    }
}

// File: NozzleSpecSO.cs
// Unity 2022/2023, C#
// ScriptableObject definition for a parameterized nozzle spec.
// This is the "authoritative inputs" object. Derived quantities are computed elsewhere.
//
// Design goals:
// - Edit-time friendly
// - Deterministic
// - Minimal fields for v0
// - Supports "derived model": throat + exit sizes derived from Thrust, Pc, epsilon

using UnityEngine;

namespace SpaceVoxel.Propulsion
{
    public enum NozzleType
    {
        Conical = 0,
        BellRao = 1
        // Future: DualBell, AerospikeLinear, AerospikeAnnular, EMNozzle
    }

    public enum PropellantClass
    {
        Solid = 0,
        LoxRp1 = 1,
        LoxLh2 = 2,
        Methalox = 3,
        Hypergolic = 4,
        Exotic = 5
    }

    [CreateAssetMenu(menuName = "SpaceVoxel/Propulsion/Nozzle Spec", fileName = "NozzleSpec")]
    public class NozzleSpecSO : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName = "Nozzle";

        [Header("Performance-defining inputs (authoritative)")]
        public PropellantClass Propellant = PropellantClass.LoxRp1;

        [Tooltip("Design thrust at the design point (kN). Used to derive throat area given Pc and an approximate Cf.")]
        [Min(0.1f)] public float DesignThrust_kN = 100f;

        [Tooltip("Chamber pressure Pc in MPa.")]
        [Min(0.1f)] public float ChamberPressure_MPa = 5f;

        [Tooltip("Expansion ratio epsilon = Ae/At.")]
        [Min(1.01f)] public float ExpansionRatio = 20f;

        [Tooltip("Design ambient pressure at which this nozzle is optimized (kPa). 101.3 for sea-level, ~0 for vacuum.")]
        [Min(0f)] public float DesignAmbientPressure_kPa = 101.3f;

        [Header("Geometry inputs (authoritative)")]
        public NozzleType Type = NozzleType.Conical;

        [Tooltip("Length scale factor. Interpreted as a multiplier on a baseline length model.")]
        [Range(0.2f, 1.5f)] public float LengthFactor = 0.80f;

        [Tooltip("Conical half-angle in degrees (used primarily for conical nozzles).")]
        [Range(5f, 35f)] public float DivergenceHalfAngle_deg = 15f;

        [Tooltip("Throat curvature factor (dimensionless). Higher makes a smoother throat blend. Used as a shaping parameter.")]
        [Range(0.2f, 3.0f)] public float ThroatRadiusCurvatureFactor = 1.0f;

        [Tooltip("Wall thickness in mm, used to generate outer surface.")]
        [Range(1f, 200f)] public float WallThickness_mm = 8f;

        [Header("Determinism and variation")]
        [Tooltip("Optional seed to create reproducible visual variation (surface noise, minor contour perturbations).")]
        public int DeterministicSeed = 0;

        [Header("Links (optional, v0)")]
        [Tooltip("Optional reference to a material definition. If provided, downstream systems can clamp Pc, thickness, etc.")]
        public ScriptableObject MaterialRef;

        [Header("Mesh generation")]
        [Tooltip("Axial samples for the profile curve. Higher is smoother, but heavier mesh.")]
        [Range(8, 256)] public int ProfileSamples = 64;

        [Tooltip("Radial segments for revolve. Higher is rounder, but heavier mesh.")]
        [Range(8, 256)] public int RadialSegments = 48;

        [Tooltip("Generate inner surface triangles.")]
        public bool GenerateInnerSurface = true;

        [Tooltip("Generate outer surface triangles.")]
        public bool GenerateOuterSurface = true;

        [Tooltip("Leave ends open. If false, generator may cap inlet and exit.")]
        public bool OpenEnds = true;

        /// <summary>
        /// Sanity clamping for edit-time use.
        /// Keep it gentle, do not hide errors, just keep values finite.
        /// </summary>
        public void ClampForSafety()
        {
            if (DesignThrust_kN < 0.1f) DesignThrust_kN = 0.1f;
            if (ChamberPressure_MPa < 0.1f) ChamberPressure_MPa = 0.1f;
            if (ExpansionRatio < 1.01f) ExpansionRatio = 1.01f;

            ProfileSamples = Mathf.Clamp(ProfileSamples, 8, 256);
            RadialSegments = Mathf.Clamp(RadialSegments, 8, 256);

            WallThickness_mm = Mathf.Clamp(WallThickness_mm, 1f, 200f);
            LengthFactor = Mathf.Clamp(LengthFactor, 0.2f, 1.5f);
            DivergenceHalfAngle_deg = Mathf.Clamp(DivergenceHalfAngle_deg, 5f, 35f);
            ThroatRadiusCurvatureFactor = Mathf.Clamp(ThroatRadiusCurvatureFactor, 0.2f, 3.0f);
            DesignAmbientPressure_kPa = Mathf.Max(0f, DesignAmbientPressure_kPa);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ClampForSafety();
        }
#endif
    }
}

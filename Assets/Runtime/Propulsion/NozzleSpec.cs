using UnityEngine;

namespace IR.Propulsion.Specs
{
    [CreateAssetMenu(
        fileName = "NozzleSpec",
        menuName = "Procedural Studio/Specs/Nozzle"
    )]
    public class NozzleSpec : ScriptableObject
    {
        [Header("Identity")]
        public int seed = 0;

        [Header("Performance")]
        [Min(0.0f)]
        public float thrust = 100f;

        [Header("Geometry")]
        [Min(0.01f)]
        public float length = 1.0f;

        [Min(0.01f)]
        public float throatRadius = 0.1f;

        [Min(0.01f)]
        public float exitRadius = 0.3f;

        [Header("Quality")]
        [Range(3, 128)]
        public int radialSegments = 32;

        [Header("Style")]
        [Range(0f, 1f)]
        public float flareJitter = 0.0f;


        [Header("Deterministic Mesh Parameters")]
        [Range(4, 256)]
        public int axialProfileSamples = 48;

        [Range(0f, 1f)]
        public float throatCurvatureFactor = 0.5f;
    }
}

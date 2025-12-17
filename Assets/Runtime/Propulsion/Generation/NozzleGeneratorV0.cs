using UnityEngine;
using IR.Propulsion.Specs;

namespace IR.Propulsion.Generation
{
    /// <summary>
    /// Orchestrates NozzleProfileSamplerV0 + RevolveMeshBuilder into a ProcArtifact.
    /// V0 is geometric-first, physics placeholders allowed.
    /// </summary>
    public static class NozzleGeneratorV0
    {
        // Bump this any time generation meaningfully changes so you can invalidate baked assets.
        public const string GeneratorVersion = "NozzleGeneratorV0_0001";

        public static ProcArtifact Generate(NozzleSpec spec)
        {
            if (spec == null)
            {
                return new ProcArtifact
                {
                    mesh = null,
                    generatorVersion = GeneratorVersion,
                    tags = { "error:null-spec" }
                };
            }

            // 1) Sample 2D profile (z,r)
            var profile = NozzleProfileSamplerV0.Sample(
                seed: spec.seed,
                length: spec.length,
                throatRadius: spec.throatRadius,
                exitRadius: spec.exitRadius,
                axialSamples: spec.axialProfileSamples,
                throatCurvatureFactor: spec.throatCurvatureFactor,
                flareJitter: spec.flareJitter
            );

            // 2) Revolve into mesh
            var settings = new RevolveMeshBuilder.BuildSettings
            {
                radialSegments = spec.radialSegments,
                capStart = false,
                capEnd = false,
                generateNormals = true,
                generateUVs = true
            };

            Mesh mesh = RevolveMeshBuilder.Build(profile.zr, settings);
            mesh.name = $"NozzleMesh_{spec.name}";

            // 3) Fill artifact metadata
            var artifact = new ProcArtifact
            {
                mesh = mesh,
                materials = null, // you can assign master material later
                bounds = mesh.bounds,
                specGuid = null,  // will be filled during baking in Editor (AssetDatabase)
                specHash = ComputeSpecHash(spec),
                generatorVersion = GeneratorVersion,

                // Convention: throat at z=0, exit at z=+L, thrust points -Z (exhaust goes +Z)
                inletPosition = new Vector3(0f, 0f, 0f),
                outletPosition = new Vector3(0f, 0f, profile.Length),
                thrustAxis = Vector3.back
            };

            artifact.tags.Add("nozzle");
            artifact.tags.Add("v0");

            return artifact;
        }

        /// <summary>
        /// Cheap stable hash used to detect stale baked assets.
        /// Not cryptographic, just consistency.
        /// </summary>
        private static string ComputeSpecHash(NozzleSpec s)
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + s.seed;
                h = h * 31 + FloatHash(s.thrust);
                h = h * 31 + FloatHash(s.length);
                h = h * 31 + FloatHash(s.throatRadius);
                h = h * 31 + FloatHash(s.exitRadius);
                h = h * 31 + s.radialSegments;
                h = h * 31 + s.axialProfileSamples;
                h = h * 31 + FloatHash(s.throatCurvatureFactor);
                h = h * 31 + FloatHash(s.flareJitter);
                return h.ToString("X8");
            }
        }

        private static int FloatHash(float x)
        {
            // Quantize slightly to avoid tiny inspector float noise causing hash churn.
            // Change scale if you want finer sensitivity.
            return Mathf.RoundToInt(x * 10000f);
        }
    }
}

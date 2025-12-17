using System;
using System.Collections.Generic;
using UnityEngine;

namespace IR.Propulsion.Generation
{
    /// <summary>
    /// Bundle of outputs produced by a procedural generation pass.
    /// Keep this dumb: data only, no generation logic.
    /// </summary>
    [Serializable]
    public sealed class ProcArtifact
    {
        [Header("Primary Outputs")]
        public Mesh mesh;

        // Optional. If you use shared master materials + MaterialPropertyBlock,
        // you may leave this empty and let the consuming prefab decide.
        public Material[] materials;

        [Header("Metadata")]
        public Bounds bounds;

        // Determinism + cache correctness:
        // - specGuid: ties back to the ScriptableObject asset if desired
        // - specHash: detects changes in spec values
        // - generatorVersion: invalidate old bakes when code changes meaningfully
        public string specGuid;
        public string specHash;
        public string generatorVersion;

        [Header("Attach Points (Local Space)")]
        public Vector3 inletPosition;
        public Vector3 outletPosition;
        public Vector3 thrustAxis; // convention: typically +Z or +Y, choose and stick to it

        [Header("Tags")]
        public List<string> tags = new List<string>();

        public bool IsValidMesh()
        {
            return mesh != null && mesh.vertexCount > 0;
        }
    }
}

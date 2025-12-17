using System;
using System.Collections.Generic;
using UnityEngine;

namespace IR.Propulsion.Generation
{
    /// <summary>
    /// Utility: revolve a 2D axial profile (z, r) around +Z to produce a mesh.
    /// Profile is assumed to be ordered by increasing z.
    /// Coordinates:
    /// - z goes along +Z
    /// - r is radius in XY plane
    /// </summary>
    public static class RevolveMeshBuilder
    {
        public struct BuildSettings
        {
            public int radialSegments;     // >= 3
            public bool capStart;          // add cap at first profile point
            public bool capEnd;            // add cap at last profile point
            public bool generateNormals;   // if false, Unity will recalc later
            public bool generateUVs;       // simple cylindrical UVs
        }

        /// <summary>
        /// Revolves the profile points into a mesh.
        /// Each profile point is (z, r).
        /// </summary>
        public static Mesh Build(IReadOnlyList<Vector2> profileZR, BuildSettings settings)
        {
            if (profileZR == null) throw new ArgumentNullException(nameof(profileZR));
            if (profileZR.Count < 2) throw new ArgumentException("Profile must have at least 2 points.", nameof(profileZR));

            int radial = Mathf.Max(3, settings.radialSegments);
            int rings = profileZR.Count;

            // We duplicate the seam vertex (radial + 1) for clean UV wrapping.
            int ringVerts = radial + 1;

            var vertices = new List<Vector3>(rings * ringVerts + (settings.capStart ? 1 + ringVerts : 0) + (settings.capEnd ? 1 + ringVerts : 0));
            var normals = settings.generateNormals ? new List<Vector3>(rings * ringVerts) : null;
            var uvs = settings.generateUVs ? new List<Vector2>(rings * ringVerts) : null;

            // Precompute z min/max for UV scaling
            float zMin = profileZR[0].x;
            float zMax = profileZR[rings - 1].x;
            float zSpan = Mathf.Max(1e-6f, zMax - zMin);

            // Build side surface rings
            for (int i = 0; i < rings; i++)
            {
                float z = profileZR[i].x;
                float r = Mathf.Max(0f, profileZR[i].y);

                for (int j = 0; j < ringVerts; j++)
                {
                    // angle goes 0..2pi, seam vertex repeats at j==radial
                    float u = (float)j / radial;
                    float theta = u * Mathf.PI * 2f;

                    float x = r * Mathf.Cos(theta);
                    float y = r * Mathf.Sin(theta);

                    vertices.Add(new Vector3(x, y, z));

                    if (settings.generateUVs)
                    {
                        float v = (z - zMin) / zSpan;
                        uvs.Add(new Vector2(u, v));
                    }
                }
            }

            var triangles = new List<int>((rings - 1) * radial * 6);

            // Side triangles
            for (int i = 0; i < rings - 1; i++)
            {
                int baseA = i * ringVerts;
                int baseB = (i + 1) * ringVerts;

                for (int j = 0; j < radial; j++)
                {
                    int a0 = baseA + j;
                    int a1 = baseA + j + 1;
                    int b0 = baseB + j;
                    int b1 = baseB + j + 1;

                    // Winding chosen so outward is +normal (right-hand rule)
                    triangles.Add(a0);
                    triangles.Add(b0);
                    triangles.Add(a1);

                    triangles.Add(a1);
                    triangles.Add(b0);
                    triangles.Add(b1);
                }
            }

            int sideVertexCount = vertices.Count;

            // Caps
            // Cap is a fan: center vertex + ring vertices.
            if (settings.capStart)
            {
                AddCap(
                    profileZR,
                    isStart: true,
                    radial: radial,
                    ringVerts: ringVerts,
                    vertices: vertices,
                    uvs: uvs,
                    triangles: triangles
                );
            }

            if (settings.capEnd)
            {
                AddCap(
                    profileZR,
                    isStart: false,
                    radial: radial,
                    ringVerts: ringVerts,
                    vertices: vertices,
                    uvs: uvs,
                    triangles: triangles
                );
            }

            var mesh = new Mesh();
            mesh.name = "RevolvedMesh_V0";

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            if (settings.generateUVs)
                mesh.SetUVs(0, uvs);

            // Normals: we can either compute ourselves later, or let Unity do it now.
            // For V0, Unity recalculation is reliable and simpler.
            if (settings.generateNormals)
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddCap(
            IReadOnlyList<Vector2> profileZR,
            bool isStart,
            int radial,
            int ringVerts,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles
        )
        {
            int ringIndex = isStart ? 0 : profileZR.Count - 1;
            float z = profileZR[ringIndex].x;
            float r = Mathf.Max(0f, profileZR[ringIndex].y);

            // If radius is basically zero, a cap adds degenerate triangles, skip.
            if (r <= 1e-6f)
                return;

            int centerIndex = vertices.Count;
            vertices.Add(new Vector3(0f, 0f, z));
            if (uvs != null) uvs.Add(new Vector2(0.5f, 0.5f));

            int ringStartIndex = vertices.Count;

            // Build ring vertices (duplicate seam for UV convenience)
            for (int j = 0; j < ringVerts; j++)
            {
                float u = (float)j / radial;
                float theta = u * Mathf.PI * 2f;
                float x = r * Mathf.Cos(theta);
                float y = r * Mathf.Sin(theta);

                vertices.Add(new Vector3(x, y, z));

                if (uvs != null)
                {
                    // Simple polar-ish cap UVs
                    float uu = 0.5f + 0.5f * Mathf.Cos(theta);
                    float vv = 0.5f + 0.5f * Mathf.Sin(theta);
                    uvs.Add(new Vector2(uu, vv));
                }
            }

            // Fan triangles
            for (int j = 0; j < radial; j++)
            {
                int v0 = ringStartIndex + j;
                int v1 = ringStartIndex + j + 1;

                if (isStart)
                {
                    // Start cap faces -Z, so winding is reversed to point outward
                    triangles.Add(centerIndex);
                    triangles.Add(v1);
                    triangles.Add(v0);
                }
                else
                {
                    // End cap faces +Z
                    triangles.Add(centerIndex);
                    triangles.Add(v0);
                    triangles.Add(v1);
                }
            }
        }
    }
}

// File: MaterialGenerationDemoMono.cs
// Unity 2022/2023 compatible
//
// Drop this on a GameObject, assign templates and a planet profile,
// then hit play and it will log one deterministic generated material.
// This is a simple sanity harness.

using UnityEngine;

namespace SpaceVoxel.Materials
{
    public class MaterialGenerationDemoMono : MonoBehaviour
    {
        public PlanetProfileSO Planet;

        [Header("Tier 0 Category Templates")]
        public MaterialCategoryTemplateSO MetalTemplate;
        public MaterialCategoryTemplateSO StoneTemplate;
        public MaterialCategoryTemplateSO SedimentaryTemplate;
        public MaterialCategoryTemplateSO LiquidTemplate;
        public MaterialCategoryTemplateSO GasTemplate;

        [Header("Node Seed")]
        public uint NodeSeed = 42u;

        [Header("Name Seed Base")]
        public string BaseName = "Bro-tonium";

        private void Start()
        {
            var gen = new MaterialGeneratorT0(MetalTemplate, StoneTemplate, SedimentaryTemplate, LiquidTemplate, GasTemplate);
            var inst = gen.Generate(Planet, NodeSeed, BaseName);
            Debug.Log(inst.ToString());
        }
    }
}

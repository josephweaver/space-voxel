// File: MaterialVisualDemoMono.cs
// Simple harness: generate a material instance and apply raw vs refined vs manufactured visuals.
// Attach to a GameObject with a child Renderer, assign planet, assign a generated MaterialInstance in code or inspector.

using UnityEngine;

namespace SpaceVoxel.Materials
{
    public class MaterialVisualDemoMono : MonoBehaviour
    {
        public PlanetProfileSO Planet;

        [Header("Assign a Renderer or leave null to auto-find")]
        public MaterialVisualApplierURP Applier;

        [Header("Material Instance (for demo)")]
        public MaterialInstance Material;

        [Header("Visual State")]
        public MaterialVisualState State = MaterialVisualState.RawOre;

        [Header("Paint Override")]
        public bool UsePaint;
        public Color PaintColor = new Color(0.2f, 0.2f, 0.9f);
        [Range(0f, 1f)] public float PaintStrength = 1.0f;

        private void Awake()
        {
            if (Applier == null) Applier = GetComponentInChildren<MaterialVisualApplierURP>();
        }

        private void Start()
        {
            ApplyNow();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) ApplyNow();
        }

        public void ApplyNow()
        {
            if (Applier == null) return;
            if (Material == null) return;

            var profile = MaterialVisualGeneratorT0.Generate(
                Planet,
                Material,
                State,
                UsePaint,
                PaintColor,
                PaintStrength);

            Applier.Apply(profile);
        }
    }
}

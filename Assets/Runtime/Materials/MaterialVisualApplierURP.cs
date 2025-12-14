// File: MaterialVisualApplierURP.cs
// Applies MaterialVisualProfile to a Renderer using MaterialPropertyBlock.
// This avoids per-instance Material allocations.

using UnityEngine;

namespace SpaceVoxel.Materials
{
    [DisallowMultipleComponent]
    public class MaterialVisualApplierURP : MonoBehaviour
    {
        [Header("Renderer target")]
        public Renderer TargetRenderer;

        [Header("URP Lit property names")]
        [Tooltip("URP Lit uses _BaseColor. If you use a custom shader, change these names.")]
        public string BaseColorProperty = "_BaseColor";
        public string MetallicProperty = "_Metallic";
        public string SmoothnessProperty = "_Smoothness";

        [Header("Optional custom shader properties")]
        public string NoiseScaleProperty = "_NoiseScale";
        public string NoiseStrengthProperty = "_NoiseStrength";
        public string DirtStrengthProperty = "_DirtStrength";
        public string OxidationStrengthProperty = "_OxidationStrength";
        public string HeatTintStrengthProperty = "_HeatTintStrength";
        public string HeatTintColorProperty = "_HeatTintColor";
        public string PaintColorProperty = "_PaintColor";
        public string PaintStrengthProperty = "_PaintStrength";

        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            if (TargetRenderer == null) TargetRenderer = GetComponentInChildren<Renderer>();
            _mpb ??= new MaterialPropertyBlock();
        }

        public void Apply(in MaterialVisualProfile profile)
        {
            if (TargetRenderer == null) return;

            _mpb.Clear();

            _mpb.SetColor(BaseColorProperty, profile.BaseColor);
            _mpb.SetFloat(MetallicProperty, profile.Metallic);
            _mpb.SetFloat(SmoothnessProperty, profile.Smoothness);

            // These only do anything if your shader supports them.
            if (!string.IsNullOrEmpty(NoiseScaleProperty)) _mpb.SetFloat(NoiseScaleProperty, profile.NoiseScale);
            if (!string.IsNullOrEmpty(NoiseStrengthProperty)) _mpb.SetFloat(NoiseStrengthProperty, profile.NoiseStrength);
            if (!string.IsNullOrEmpty(DirtStrengthProperty)) _mpb.SetFloat(DirtStrengthProperty, profile.DirtStrength);
            if (!string.IsNullOrEmpty(OxidationStrengthProperty)) _mpb.SetFloat(OxidationStrengthProperty, profile.OxidationStrength);
            if (!string.IsNullOrEmpty(HeatTintStrengthProperty)) _mpb.SetFloat(HeatTintStrengthProperty, profile.HeatTintStrength);
            if (!string.IsNullOrEmpty(HeatTintColorProperty)) _mpb.SetColor(HeatTintColorProperty, profile.HeatTintColor);

            if (profile.UsePaint)
            {
                if (!string.IsNullOrEmpty(PaintColorProperty)) _mpb.SetColor(PaintColorProperty, profile.PaintColor);
                if (!string.IsNullOrEmpty(PaintStrengthProperty)) _mpb.SetFloat(PaintStrengthProperty, profile.PaintStrength);
            }
            else
            {
                if (!string.IsNullOrEmpty(PaintStrengthProperty)) _mpb.SetFloat(PaintStrengthProperty, 0f);
            }

            TargetRenderer.SetPropertyBlock(_mpb);
        }
    }
}

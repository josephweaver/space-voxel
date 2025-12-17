#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using IR.Propulsion.Generation;
using IR.Propulsion.Specs;

namespace IR.Propulsion.EditorTools
{
    public static class NozzleSpecPreviewMenu
    {
        private const string PreviewRootName = "PROC_PREVIEW_ROOT";

        [MenuItem("Assets/IR/Propulsion/Preview Nozzle From Spec", true)]
        private static bool ValidatePreview()
        {
            return Selection.activeObject is NozzleSpec;
        }

        [MenuItem("Assets/IR/Propulsion/Preview Nozzle From Spec")]
        private static void Preview()
        {
            var spec = Selection.activeObject as NozzleSpec;
            if (spec == null)
                return;

            var artifact = NozzleGeneratorV0.Generate(spec);
            if (artifact == null || artifact.mesh == null || !artifact.IsValidMesh())
            {
                Debug.LogWarning("Nozzle preview failed, artifact mesh invalid.");
                return;
            }

            // Find or create a root for previews so you can delete them in one go.
            GameObject root = GameObject.Find(PreviewRootName);
            if (root == null)
            {
                root = new GameObject(PreviewRootName);
                Undo.RegisterCreatedObjectUndo(root, "Create Proc Preview Root");
            }

            // Create the preview object
            var go = new GameObject($"NozzlePreview_{spec.name}");
            Undo.RegisterCreatedObjectUndo(go, "Create Nozzle Preview");
            go.transform.SetParent(root.transform, worldPositionStays: false);

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();

            mf.sharedMesh = artifact.mesh;

            // Assign a default material so it renders.
            // You can swap this later for your Material Visual System master material.
            var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
            mr.sharedMaterial = mat;

            // Put it somewhere visible
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            Selection.activeGameObject = go;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        [MenuItem("GameObject/IR/Propulsion/Clear Proc Previews", false, 0)]
        private static void ClearPreviews()
        {
            var root = GameObject.Find(PreviewRootName);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
            }
        }
    }
}
#endif

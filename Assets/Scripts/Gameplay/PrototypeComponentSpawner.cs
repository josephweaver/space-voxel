using UnityEngine;

public class PrototypeComponentSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Placement")]
    [SerializeField] private float spawnDistance = 2.0f;

    private GameObject currentAssembly;

    void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    public void Spawn(ComponentDefinitionSO def)
    {
        if (def == null) return;

        if (currentAssembly != null)
            Destroy(currentAssembly);

        Vector3 forwardFlat = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        if (forwardFlat.sqrMagnitude < 0.001f) forwardFlat = transform.forward;

        Quaternion rot = Quaternion.LookRotation(forwardFlat, Vector3.up);
        Vector3 center = cameraTransform.position + forwardFlat * spawnDistance;

        currentAssembly = new GameObject($"Assembly_{def.displayName}");
        currentAssembly.transform.SetPositionAndRotation(center, rot);

        foreach (var part in def.parts)
        {
            if (part == null || part.prefab == null) continue;

            var go = Instantiate(part.prefab, currentAssembly.transform);
            go.name = string.IsNullOrWhiteSpace(part.partName) ? part.prefab.name : part.partName;
            go.transform.localPosition = part.localPosition;
            go.transform.localRotation = Quaternion.Euler(part.localEulerAngles);
        }
    }
}

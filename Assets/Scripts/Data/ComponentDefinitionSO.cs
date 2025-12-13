using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SpaceVoxel/Components/Component Definition")]
public class ComponentDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "New Component";

    [Header("UI")]
    public Sprite icon; // optional, can be null for now

    [System.Serializable]
    public class PartSpawn
    {
        public string partName;          // "Fuel", "Nozzle", etc, optional
        public GameObject prefab;        // FuelBlock prefab, NozzleBlock prefab
        public Vector3 localPosition;    // relative to assembly root
        public Vector3 localEulerAngles; // optional rotation per part
    }

    [Header("Prototype Parts")]
    public List<PartSpawn> parts = new();
}

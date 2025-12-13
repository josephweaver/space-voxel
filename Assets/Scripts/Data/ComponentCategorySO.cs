using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SpaceVoxel/Components/Component Category")]
public class ComponentCategorySO : ScriptableObject
{
    public string displayName = "New Category";
    public Sprite icon; // optional

    public List<ComponentDefinitionSO> components = new();
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComponentSelectorUI_SO : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform content;   // ScrollView/Viewport/Content
    [SerializeField] private Button tilePrefab;   // your tile prefab button

    [Header("Data")]
    [SerializeField] private List<ComponentCategorySO> categories = new();

    [Header("Spawn")]
    [SerializeField] private PrototypeComponentSpawner spawner;

    private ComponentCategorySO currentCategory;
    private readonly List<Button> spawned = new();

    void OnEnable()
    {
        ShowCategories();
    }

    private void Clear()
    {
        foreach (var b in spawned)
            if (b != null) Destroy(b.gameObject);
        spawned.Clear();
    }

    private void BuildTiles(List<(string label, Sprite icon)> tiles, System.Action<int> onClickIndex)
    {
        Clear();

        for (int i = 0; i < tiles.Count; i++)
        {
            int idx = i;
            var btn = Instantiate(tilePrefab, content);
            spawned.Add(btn);

            // Text
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = tiles[i].label;

            // Optional icon support later: if your prefab has an Image child you can wire it here

            btn.onClick.AddListener(() => onClickIndex(idx));
        }
    }

    private void ShowCategories()
    {
        currentCategory = null;

        var tiles = new List<(string, Sprite)>();
        foreach (var c in categories)
            if (c != null) tiles.Add((c.displayName, c.icon));

        BuildTiles(tiles, idx =>
        {
            var cat = categories[idx];
            if (cat != null) ShowComponents(cat);
        });

        // Minimal click philosophy: auto-drill into first category if present
        if (categories.Count > 0 && categories[0] != null)
            ShowComponents(categories[0]);
    }

    private void ShowComponents(ComponentCategorySO cat)
    {
        currentCategory = cat;

        var tiles = new List<(string, Sprite)>();
        foreach (var comp in cat.components)
            if (comp != null) tiles.Add((comp.displayName, comp.icon));

        BuildTiles(tiles, idx =>
        {
            var comp = cat.components[idx];
            if (comp != null && spawner != null)
                spawner.Spawn(comp);
        });

        // Minimal click philosophy: auto-select first component if you want
        // If you prefer "wait for click", delete this block.
        if (cat.components.Count > 0 && cat.components[0] != null && spawner != null)
            spawner.Spawn(cat.components[0]);
    }
}

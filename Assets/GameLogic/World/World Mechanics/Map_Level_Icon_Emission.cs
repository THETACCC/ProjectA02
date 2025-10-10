using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Map_Level_Icon_Emission : MonoBehaviour
{
    [Header("替换用材质（进入触发器时用它替换子物体材质）")]
    public Material highlightMaterial;

    [Header("替换策略")]
    public bool replaceAllSlots = true;     // 替换所有材质槽
    [Min(0)] public int materialSlotIndex = 0; // 只替换某个槽位时使用

    // 记录每个子Renderer的原始 sharedMaterials，方便退出时还原
    private readonly Dictionary<Renderer, Material[]> _originalSharedMats = new();

    // 子物体上的所有 Renderer
    private Renderer[] _childRenderers;

    void Awake()
    {
        if (!TryGetComponent<Collider>(out var col) || !col.isTrigger)
        {
            Debug.LogWarning($"{name}: 需要一个触发器 Collider（isTrigger = true）。");
        }
    }

    void Start()
    {
        // 收集所有子物体的 Renderer（包括 inactive 子物体时可用 true）
        var all = GetComponentsInChildren<Renderer>(includeInactive: true);
        var list = new List<Renderer>();
        foreach (var r in all)
        {
            if (r.transform != transform) list.Add(r); // 排除自己，仅子物体
        }
        _childRenderers = list.ToArray();

        // 记住每个子Renderer的原始 sharedMaterials（引用，不复制）
        _originalSharedMats.Clear();
        foreach (var r in _childRenderers)
        {
            if (r && !_originalSharedMats.ContainsKey(r))
                _originalSharedMats.Add(r, r.sharedMaterials);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || highlightMaterial == null) return;

        foreach (var r in _childRenderers)
        {
            if (!r) continue;

            // 取当前 sharedMaterials 长度来构造替换数组
            var cur = r.sharedMaterials;
            if (cur == null || cur.Length == 0) continue;

            Material[] replaced = (Material[])cur.Clone();

            if (replaceAllSlots)
            {
                for (int i = 0; i < replaced.Length; i++)
                    replaced[i] = highlightMaterial;
            }
            else
            {
                int idx = Mathf.Clamp(materialSlotIndex, 0, replaced.Length - 1);
                replaced[idx] = highlightMaterial;
            }

            // 用 sharedMaterials 设置（不会创建实例材质，避免内存碎片）
            r.sharedMaterials = replaced;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 还原为进入前记录的 sharedMaterials
        foreach (var r in _childRenderers)
        {
            if (!r) continue;
            if (_originalSharedMats.TryGetValue(r, out var original) && original != null)
            {
                r.sharedMaterials = original;
            }
        }
    }
}

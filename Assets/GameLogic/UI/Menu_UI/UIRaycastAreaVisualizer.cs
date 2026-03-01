using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIRaycastAreaVisualizer : MonoBehaviour
{
    [Header("What to draw")]
    public bool drawAllRaycastTargets = true;   // 画所有 raycastTarget 的 Graphic
    public bool drawInGameView = true;          // Play 时在 GameView 也画
    public bool onlyWhenSelected = false;       // SceneView 只在选中时显示

    [Header("Style")]
    public Color sceneColor = new Color(0f, 1f, 1f, 1f);
    public Color gameColor = new Color(0f, 1f, 1f, 0.85f);
    public float gameThickness = 2f;

    static Texture2D _tex;

    void OnDrawGizmos()
    {
        if (onlyWhenSelected) return;
        DrawSceneGizmos();
    }

    void OnDrawGizmosSelected()
    {
        if (!onlyWhenSelected) return;
        DrawSceneGizmos();
    }

    void DrawSceneGizmos()
    {
        Gizmos.color = sceneColor;
        foreach (var rt in GetTargets())
        {
            if (!rt) continue;
            DrawRectGizmos(rt);
        }
    }

    void OnGUI()
    {
        if (!drawInGameView) return;
        if (!Application.isPlaying) return; // 你也可以改成 true，让编辑模式 GameView 也画

        EnsureTex();

        var canvas = GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay) cam = canvas.worldCamera;

        foreach (var rt in GetTargets())
        {
            if (!rt) continue;
            DrawRectOnGUI(rt, cam);
        }
    }

    IEnumerable<RectTransform> GetTargets()
    {
        if (!drawAllRaycastTargets)
        {
            yield return transform as RectTransform;
            yield break;
        }

        var graphics = GetComponentsInChildren<Graphic>(true);
        bool any = false;

        foreach (var g in graphics)
        {
            if (!g) continue;
            if (!g.raycastTarget) continue;

            any = true;
            yield return g.rectTransform;
        }

        // 如果一个都没找到，就画自己
        if (!any) yield return transform as RectTransform;
    }

    void DrawRectGizmos(RectTransform rt)
    {
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
    }

    void DrawRectOnGUI(RectTransform rt, Camera cam)
    {
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector2[] sp = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            var p = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
            sp[i] = new Vector2(p.x, Screen.height - p.y); // GUI 的 y 轴向下
        }

        DrawLine(sp[0], sp[1], gameColor, gameThickness);
        DrawLine(sp[1], sp[2], gameColor, gameThickness);
        DrawLine(sp[2], sp[3], gameColor, gameThickness);
        DrawLine(sp[3], sp[0], gameColor, gameThickness);
    }

    static void EnsureTex()
    {
        if (_tex != null) return;
        _tex = new Texture2D(1, 1);
        _tex.SetPixel(0, 0, Color.white);
        _tex.Apply();
    }

    static void DrawLine(Vector2 a, Vector2 b, Color col, float width)
    {
        var savedColor = GUI.color;
        var savedMatrix = GUI.matrix;

        GUI.color = col;

        var d = b - a;
        float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        float length = d.magnitude;

        GUIUtility.RotateAroundPivot(angle, a);
        GUI.DrawTexture(new Rect(a.x, a.y - width * 0.5f, length, width), _tex);

        GUI.matrix = savedMatrix;
        GUI.color = savedColor;
    }
}
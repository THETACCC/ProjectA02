using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class DiamondCornerDots : MonoBehaviour
{
    [Header("Hooks")]
    public DiamondRewardBar bar;            // 进度来源（建议 bar 暴露 public float CurrentProgress）
    public Image[] dots = new Image[4];     // 顺序：Top, Left, Bottom, Right

    [Header("Corner t along perimeter (0..1 CCW from top)")]
    public float[] cornerT = { 0f, 0.25f, 0.5f, 0.75f };

    [Header("Colors")]
    public Color offColor = Color.white;                      // 初始白色
    public Color onColor = new Color(1f, 0.85f, 0f, 1f);     // 永久黄（改成你想要的黄）
    [Range(0, 10)] public float glowOn = 2.5f;
    [Range(0, 10)] public float glowOff = 0f;

    [Header("Startup safety")]
    [Tooltip("启用后前多少帧强制把进度当作 0，避免旧材质值导致误亮。")]
    [Range(0, 30)] public int safetyStartFrames = 6;
    [Tooltip("启用时是否把进度条材质 _Progress 直接清零（更稳）。")]
    public bool forceBarProgressZeroOnStart = true;

    // —— 内部 —— 
    private Material[] _mats;          // 每个角点的实例材质（UI/GlowSprite）
    private bool[] _lit;               // 是否已“被路过” → 永久点亮
    private float _lastP;              // 上一帧进度
    private int _framesSinceStart;
    private Image _barImg;
    private PropertyInfo _propCurrentProgress; // 反射拿 bar.CurrentProgress（可选）
    private bool _initialized;

    void OnEnable()
    {
        // 不在 OnEnable 里做任何加载/克隆/Shader.Find（避免 domain backup 报错）
        _initialized = false;
        // 先把可见外观置为白+无光（不创建新材质）
        if (dots != null)
        {
            foreach (var img in dots)
            {
                if (!img) continue;
                img.color = offColor;
                var mat = img.material;
                if (mat != null) mat.SetFloat("_GlowIntensity", glowOff);
            }
        }
    }

    void Start()
    {
        if (Application.isPlaying)
            StartCoroutine(InitRoutine());   // 运行时延迟一帧再做初始化（避开 domain backup）
        else
            EditorInit();                    // 编辑器预览时，尽量不做加载/创建
    }

    IEnumerator InitRoutine()
    {
        // 延迟一帧，避开 Domain Backup 阶段
        yield return null;

        // —— 只在运行时初始化 —— //
        EnsureGlowMaterialsRuntime();        // 强制每个点拥有 GlowSprite 实例材质
        PrepareState();

        _initialized = true;
    }

    void EditorInit()
    {
        // 在编辑器非运行态尽量不新建材质；只做状态重置
        PrepareState();
        _initialized = true;
    }

    void PrepareState()
    {
        _framesSinceStart = 0;
        _lastP = 0f;

        if (_lit == null || _lit.Length != dots.Length)
            _lit = new bool[dots.Length];
        for (int i = 0; i < _lit.Length; i++) _lit[i] = false;

        // 角点全部重置为白色 + 无光
        if (dots != null)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                if (!dots[i]) continue;
                dots[i].color = offColor;
                var m = dots[i].material;
                if (m != null) m.SetFloat("_GlowIntensity", glowOff);
            }
        }

        // 进度条材质（用于回退读取/清零）
        _barImg = bar ? bar.GetComponent<Image>() : null;

        // 清零进度条材质 _Progress（运行时才做）
        if (Application.isPlaying && forceBarProgressZeroOnStart && _barImg != null && _barImg.material != null)
        {
            // 使用实例材质，避免影响其它 Image
            _barImg.material = new Material(_barImg.material);
            _barImg.material.SetFloat("_Progress", 0f);
        }

        // 反射缓存（若 DiamondRewardBar 有 CurrentProgress）
        _propCurrentProgress = (bar != null)
            ? bar.GetType().GetProperty("CurrentProgress", BindingFlags.Instance | BindingFlags.Public)
            : null;
    }

    // 运行时：确保每个角点都是 UI/GlowSprite 的实例材质（与进度条完全隔离）
    void EnsureGlowMaterialsRuntime()
    {
        if (dots == null) return;

        if (_mats == null || _mats.Length != dots.Length)
            _mats = new Material[dots.Length];

        for (int i = 0; i < dots.Length; i++)
        {
            var img = dots[i];
            if (!img) continue;

            // 如果角点错误地用了 DiamondProgressBar（或任何非 GlowSprite）→ 强制替换
            if (img.material == null || img.material.shader == null ||
                img.material.shader.name != "UI/GlowSprite")
            {
                var sh = Shader.Find("UI/GlowSprite");
                if (sh != null) img.material = new Material(sh);
            }

            // 克隆实例材质，彻底独立
            _mats[i] = new Material(img.material);
            img.material = _mats[i];

            // 初始无光
            _mats[i].SetFloat("_GlowIntensity", glowOff);
        }
    }

    void LateUpdate()
    {
        if (!_initialized || bar == null || dots == null) return;
        _framesSinceStart++;

        float p = ReadProgressClamped();

        // “跨越阈值 → 永久点亮”
        for (int i = 0; i < dots.Length; i++)
        {
            var img = dots[i];
            if (!img) continue;
            var mat = img.material;

            float tCorner = (i < cornerT.Length) ? cornerT[i] : 0f;

            if (!_lit[i] && CrossedForward(_lastP, p, tCorner))
                _lit[i] = true;

            if (_lit[i])
            {
                img.color = onColor;
                if (mat) mat.SetFloat("_GlowIntensity", glowOn);
            }
            else
            {
                img.color = offColor;
                if (mat) mat.SetFloat("_GlowIntensity", glowOff);
            }
        }

        _lastP = p;
    }

    // 读取进度；启动的前若干帧强制为 0，彻底避免遗留值
    float ReadProgressClamped()
    {
        if (Application.isPlaying && _framesSinceStart <= safetyStartFrames)
        {
            if (_barImg != null && _barImg.material != null)
                _barImg.material.SetFloat("_Progress", 0f); // 再写一次 0
            return 0f;
        }

        // 优先从脚本属性拿（最稳）
        if (_propCurrentProgress != null && _propCurrentProgress.PropertyType == typeof(float))
        {
            try { return Mathf.Clamp01((float)_propCurrentProgress.GetValue(bar)); }
            catch { }
        }

        // 回退从材质读取
        if (_barImg != null && _barImg.material != null)
            return Mathf.Clamp01(_barImg.material.GetFloat("_Progress"));

        return 0f;
    }

    // 判断进度是否在单位圆 0..1 上从 lastP 前进到 p 的过程中跨过了阈值 t（逆时针）
    static bool CrossedForward(float lastP, float p, float t)
    {
        lastP = Mathf.Repeat(lastP, 1f);
        p = Mathf.Repeat(p, 1f);
        t = Mathf.Repeat(t, 1f);

        if (Mathf.Abs(p - lastP) <= 1e-6f) return false; // 未前进

        if (lastP <= p)   // 不跨 1.0
            return t > lastP && t <= p;
        else              // 跨 1.0
            return t > lastP || t <= p;
    }

    // 外部可调用复位（换关卡/重开时）
    public void ResetAll()
    {
        _lastP = 0f;
        if (_lit == null || _lit.Length != dots.Length)
            _lit = new bool[dots.Length];
        for (int i = 0; i < _lit.Length; i++) _lit[i] = false;

        for (int i = 0; i < dots.Length; i++)
        {
            var img = dots[i];
            if (!img) continue;
            img.color = offColor;
            var mat = img.material;
            if (mat != null) mat.SetFloat("_GlowIntensity", glowOff);
        }

        if (_barImg != null && _barImg.material != null)
            _barImg.material.SetFloat("_Progress", 0f);

        _framesSinceStart = 0;
    }
}

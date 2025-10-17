using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class DiamondRewardBar : MonoBehaviour
{
    [Header("Material Source")]
    [Tooltip("If assigned, we use this as a source. If left empty, we create from ShaderPath.")]
    public Material materialOverride;
    [Tooltip("Shader to use when creating an instance material.")]
    public string shaderPath = "UI/DiamondProgressBar";
    [Tooltip("Override the material's properties from this script every frame (and in edit mode).")]
    public bool overrideMaterialProperties = true;

    [Header("Data Source")]
    public RewardManager rewardManager;
    public bool autoFindManager = true;

    [Header("Visuals (driven into shader)")]
    [Range(0f, 1f)] public float thickness = 0.18f;
    [Range(0f, 0.05f)] public float softness = 0.003f;
    public Color fillColor = new Color(1f, 0.85f, 0f, 1f);
    public Color emptyColor = new Color(0.22f, 0.22f, 0.22f, 0.95f);
    public Color background = new Color(0f, 0f, 0f, 0f);

    [Header("Flow")]
    [Range(1, 50)] public float stripeDensity = 12f;
    public float flowSpeed = 0.6f;
    [Range(0f, 1f)] public float stripeAlpha = 0.6f;
    public bool showFlowWhenEmpty = false;
    public bool stopFlowWhenFull = true;

    [Header("Fill Animation")]
    public bool animateFill = true;
    [Tooltip("Seconds for a full 0→1 lap.")]
    public float secondsPerFullLap = 4f;
    public enum TimeScaleMode { ScaledTime, UnscaledTime }
    public TimeScaleMode timeScale = TimeScaleMode.UnscaledTime;

    // --- internals ---
    private Image _img;
    private Material _instanceMat;
    private float _current; // displayed progress
    private float _target;  // target progress

    void Awake() { EnsureImageAndMaterial(); }
    void OnEnable() { EnsureImageAndMaterial(); }
    void OnValidate() { EnsureImageAndMaterial(); ApplyAllParams(editMode: true); }

    void Start()
    {
        if (autoFindManager && rewardManager == null)
            rewardManager = FindObjectOfType<RewardManager>();
    }

    void Update()
    {
        EnsureImageAndMaterial();
        UpdateTargetFromRewards();
        AnimateProgress();
        ApplyAllParams(editMode: false);
    }

    // --- helpers ---
    private void EnsureImageAndMaterial()
    {
        if (!_img) _img = GetComponent<Image>();

        // Ensure we have a unique per-renderer material instance
        if (_img && (_instanceMat == null || _img.material != _instanceMat))
        {
            if (materialOverride != null)
            {
                _instanceMat = new Material(materialOverride);  // clone so we can tweak freely
            }
            else
            {
                var shader = Shader.Find(shaderPath);
                if (shader == null) { /* shader missing: quietly bail */ return; }
                _instanceMat = new Material(shader);
            }
            _img.material = _instanceMat;
        }

        // Make sure the Image itself isn't alpha=0
        if (_img && _img.color.a < 1f)
        {
            var c = _img.color; c.a = 1f; _img.color = c;
        }
    }

    private void UpdateTargetFromRewards()
    {
        _target = 0f;
        if (rewardManager != null && rewardManager.RewardObjects != null)
        {
            int total = Mathf.Max(0, rewardManager.RewardObjects.Length);
            if (total > 0)
                _target = Mathf.Clamp01((float)rewardManager.rewardsReachedCount / total);
        }
    }

    private void AnimateProgress()
    {
        float dt = (timeScale == TimeScaleMode.ScaledTime) ? Time.deltaTime : Time.unscaledDeltaTime;
        if (!Application.isPlaying) dt = 0f; // avoid drift in edit mode

        if (animateFill && secondsPerFullLap > 0f && dt > 0f)
        {
            float unitsPerSecond = 1f / secondsPerFullLap;
            _current = Mathf.MoveTowards(_current, _target, unitsPerSecond * dt);
        }
        else
        {
            _current = _target;
        }
    }

    private void ApplyAllParams(bool editMode)
    {
        if (_instanceMat == null) return;

        // Aspect for non-square rects
        var rt = (RectTransform)transform;
        float w = Mathf.Max(1, rt.rect.width);
        float h = Mathf.Max(1, rt.rect.height);
        _instanceMat.SetFloat("_Aspect", w / h);

        // Always push progress
        _instanceMat.SetFloat("_Progress", Mathf.Clamp01(_current));

        if (!overrideMaterialProperties) return; // leave the material’s own settings

        // Push all visual params
        _instanceMat.SetFloat("_Thickness", thickness);
        _instanceMat.SetFloat("_Softness", softness);
        _instanceMat.SetColor("_FillColor", fillColor);
        _instanceMat.SetColor("_EmptyColor", emptyColor);
        _instanceMat.SetColor("_Background", background);
        _instanceMat.SetFloat("_StripeDensity", stripeDensity);
        _instanceMat.SetFloat("_FlowSpeed", flowSpeed);
        _instanceMat.SetFloat("_StripeAlpha", stripeAlpha);

        bool showFlow = showFlowWhenEmpty || _current > 0f;
        if (stopFlowWhenFull && _current >= 0.999f) showFlow = false;
        _instanceMat.SetFloat("_ShowFlow", showFlow ? 1f : 0f);
    }

    // Call this to drive manually if you ever need to
    public void SetProgress(float value, bool instant = false)
    {
        _target = Mathf.Clamp01(value);
        if (instant) _current = _target;
    }
}

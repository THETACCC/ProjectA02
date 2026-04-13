using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class StrongGuideOverlay : MonoBehaviour
{
    public enum HoleShape
    {
        Rectangle = 0,
        Circle = 1
    }

    [Header("References")]
    [SerializeField] private RectTransform overlayRect;
    [SerializeField] private Image overlayImage;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 12f;

    [Header("Default")]
    [SerializeField] private float defaultPadding = 24f;

    private Material runtimeMat;

    private RectTransform target1;
    private RectTransform target2;

    private HoleShape hole1Shape = HoleShape.Rectangle;
    private HoleShape hole2Shape = HoleShape.Rectangle;

    private float padding1;
    private float padding2;

    private bool useHole1 = false;
    private bool useHole2 = false;

    private Vector2 currentCenter1;
    private Vector2 currentSize1;

    private Vector2 currentCenter2;
    private Vector2 currentSize2;

    private static readonly int UseHole1ID = Shader.PropertyToID("_UseHole1");
    private static readonly int Hole1ShapeID = Shader.PropertyToID("_Hole1Shape");
    private static readonly int Hole1CenterID = Shader.PropertyToID("_Hole1Center");
    private static readonly int Hole1SizeID = Shader.PropertyToID("_Hole1Size");

    private static readonly int UseHole2ID = Shader.PropertyToID("_UseHole2");
    private static readonly int Hole2ShapeID = Shader.PropertyToID("_Hole2Shape");
    private static readonly int Hole2CenterID = Shader.PropertyToID("_Hole2Center");
    private static readonly int Hole2SizeID = Shader.PropertyToID("_Hole2Size");

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Update()
    {
        EnsureInitialized();

        UpdateHole(
            useHole1,
            target1,
            padding1,
            ref currentCenter1,
            ref currentSize1,
            UseHole1ID,
            Hole1ShapeID,
            Hole1CenterID,
            Hole1SizeID,
            hole1Shape
        );

        UpdateHole(
            useHole2,
            target2,
            padding2,
            ref currentCenter2,
            ref currentSize2,
            UseHole2ID,
            Hole2ShapeID,
            Hole2CenterID,
            Hole2SizeID,
            hole2Shape
        );
    }

    private void EnsureInitialized()
    {
        if (overlayRect == null)
            overlayRect = transform as RectTransform;

        if (overlayImage == null)
            overlayImage = GetComponent<Image>();

        if (runtimeMat == null && overlayImage != null && overlayImage.material != null)
        {
            runtimeMat = new Material(overlayImage.material);
            overlayImage.material = runtimeMat;
        }
    }

    private void UpdateHole(
        bool useHole,
        RectTransform target,
        float padding,
        ref Vector2 currentCenter,
        ref Vector2 currentSize,
        int useID,
        int shapeID,
        int centerID,
        int sizeID,
        HoleShape shape
    )
    {
        if (runtimeMat == null)
            return;

        runtimeMat.SetFloat(useID, useHole ? 1f : 0f);
        runtimeMat.SetFloat(shapeID, (float)shape);

        if (!useHole || target == null)
            return;

        Vector2 targetCenterUV;
        Vector2 targetSizeUV;
        GetTargetUVRect(target, padding, out targetCenterUV, out targetSizeUV);

        currentCenter = Vector2.Lerp(currentCenter, targetCenterUV, Time.deltaTime * followSpeed);
        currentSize = Vector2.Lerp(currentSize, targetSizeUV, Time.deltaTime * followSpeed);

        runtimeMat.SetVector(centerID, new Vector4(currentCenter.x, currentCenter.y, 0f, 0f));
        runtimeMat.SetVector(sizeID, new Vector4(currentSize.x, currentSize.y, 0f, 0f));
    }

    public void Show(RectTransform target, HoleShape shape, float padding = -1f, bool snap = true)
    {
        EnsureInitialized();

        target1 = target;
        target2 = null;

        hole1Shape = shape;
        hole2Shape = HoleShape.Rectangle;

        padding1 = padding >= 0f ? padding : defaultPadding;
        padding2 = defaultPadding;

        useHole1 = target != null;
        useHole2 = false;

        if (runtimeMat != null)
        {
            if (snap && target1 != null)
            {
                GetTargetUVRect(target1, padding1, out currentCenter1, out currentSize1);
                runtimeMat.SetVector(Hole1CenterID, new Vector4(currentCenter1.x, currentCenter1.y, 0f, 0f));
                runtimeMat.SetVector(Hole1SizeID, new Vector4(currentSize1.x, currentSize1.y, 0f, 0f));
            }

            runtimeMat.SetFloat(UseHole1ID, useHole1 ? 1f : 0f);
            runtimeMat.SetFloat(UseHole2ID, 0f);
            runtimeMat.SetFloat(Hole1ShapeID, (float)hole1Shape);
        }

        if (overlayImage != null)
            overlayImage.enabled = true;
    }

    public void ShowTwo(
        RectTransform firstTarget,
        HoleShape firstShape,
        RectTransform secondTarget,
        HoleShape secondShape,
        float firstPadding = -1f,
        float secondPadding = -1f,
        bool snap = true)
    {
        EnsureInitialized();

        target1 = firstTarget;
        target2 = secondTarget;

        hole1Shape = firstShape;
        hole2Shape = secondShape;

        padding1 = firstPadding >= 0f ? firstPadding : defaultPadding;
        padding2 = secondPadding >= 0f ? secondPadding : defaultPadding;

        useHole1 = target1 != null;
        useHole2 = target2 != null;

        if (runtimeMat != null)
        {
            if (snap)
            {
                if (target1 != null)
                {
                    GetTargetUVRect(target1, padding1, out currentCenter1, out currentSize1);
                    runtimeMat.SetVector(Hole1CenterID, new Vector4(currentCenter1.x, currentCenter1.y, 0f, 0f));
                    runtimeMat.SetVector(Hole1SizeID, new Vector4(currentSize1.x, currentSize1.y, 0f, 0f));
                }

                if (target2 != null)
                {
                    GetTargetUVRect(target2, padding2, out currentCenter2, out currentSize2);
                    runtimeMat.SetVector(Hole2CenterID, new Vector4(currentCenter2.x, currentCenter2.y, 0f, 0f));
                    runtimeMat.SetVector(Hole2SizeID, new Vector4(currentSize2.x, currentSize2.y, 0f, 0f));
                }
            }

            runtimeMat.SetFloat(UseHole1ID, useHole1 ? 1f : 0f);
            runtimeMat.SetFloat(UseHole2ID, useHole2 ? 1f : 0f);
            runtimeMat.SetFloat(Hole1ShapeID, (float)hole1Shape);
            runtimeMat.SetFloat(Hole2ShapeID, (float)hole2Shape);
        }

        if (overlayImage != null)
            overlayImage.enabled = true;
    }

    public void Hide()
    {
        EnsureInitialized();

        useHole1 = false;
        useHole2 = false;
        target1 = null;
        target2 = null;

        if (runtimeMat != null)
        {
            runtimeMat.SetFloat(UseHole1ID, 0f);
            runtimeMat.SetFloat(UseHole2ID, 0f);
        }

        if (overlayImage != null)
            overlayImage.enabled = false;
    }

    private void GetTargetUVRect(RectTransform target, float padding, out Vector2 centerUV, out Vector2 sizeUV)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector3 worldBL = corners[0];
        Vector3 worldTR = corners[2];
        Vector3 worldCenter = (worldBL + worldTR) * 0.5f;

        Vector2 localCenter = overlayRect.InverseTransformPoint(worldCenter);
        Rect overlayLocalRect = overlayRect.rect;

        float u = Mathf.InverseLerp(overlayLocalRect.xMin, overlayLocalRect.xMax, localCenter.x);
        float v = Mathf.InverseLerp(overlayLocalRect.yMin, overlayLocalRect.yMax, localCenter.y);
        centerUV = new Vector2(u, v);

        Vector2 localBL = overlayRect.InverseTransformPoint(worldBL);
        Vector2 localTR = overlayRect.InverseTransformPoint(worldTR);
        Vector2 localSize = localTR - localBL;
        localSize += Vector2.one * padding * 2f;

        sizeUV = new Vector2(
            localSize.x / overlayLocalRect.width,
            localSize.y / overlayLocalRect.height
        );
    }
}
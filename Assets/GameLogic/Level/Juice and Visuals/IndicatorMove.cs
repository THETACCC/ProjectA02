using UnityEngine;

public class IndicatorMove : MonoBehaviour
{
    // [Header("Move")]
    // public float height = 1.0f;
    // public float speed = 1.0f;

    [Header("Materials")]
    public Material regularIndicatorMaterial; // 蓝色 / default
    public Material freeIndicatorMaterial;    // 绿色

    [Header("Target Mesh")]
    public Renderer targetRenderer; // assign 这个 indicator 自己的 mesh renderer

    // private Vector3 initialPosition;
    private Block parentBlock;

    void Start()
    {
        // initialPosition = transform.position;

        // 如果没手动拖，就自动找自己或子物体上的 Renderer
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<Renderer>();
        }

        // 往父物体一路找 Block
        parentBlock = GetComponentInParent<Block>();

        ApplyIndicatorMaterial();
    }

    void Update()
    {
        // 如果你之后想恢复上下浮动，就把下面两行取消注释
        // float newY = Mathf.Lerp(initialPosition.y - height, initialPosition.y + height, Mathf.PingPong(Time.time * speed, 1));
        // transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 如果你之后想恢复锁定世界旋转，就把下面这行取消注释
        // transform.rotation = Quaternion.identity;
    }

    private void ApplyIndicatorMaterial()
    {
        if (parentBlock == null)
        {
            Debug.LogWarning($"[IndicatorMove] No Block found in parents for {gameObject.name}", this);
            return;
        }

        if (targetRenderer == null)
        {
            Debug.LogWarning($"[IndicatorMove] No Renderer assigned/found on {gameObject.name}", this);
            return;
        }

        if (parentBlock.type == BlockType.Free)
        {
            if (freeIndicatorMaterial != null)
                targetRenderer.material = freeIndicatorMaterial;
        }
        else if (parentBlock.type == BlockType.Regular)
        {
            if (regularIndicatorMaterial != null)
                targetRenderer.material = regularIndicatorMaterial;
        }
    }
}
using MoreMountains.Tools;
using UnityEngine;

public class LevelFail : MonoBehaviour
{
    public SceneInfo sceneInfo;

    [Header("Audio")]
    [SerializeField] private AudioClip[] loseClip;

    // 全局锁：防止 Player1/Player2 同时死，导致重复 Load
    public static bool s_IsFailing = false;

    private Collider _triggerCol;

    private void Awake()
    {
        _triggerCol = GetComponent<Collider>();
        if (_triggerCol != null) _triggerCol.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (s_IsFailing) return;

        // ✅ 用 CompareTag 更安全更快
        if (other.CompareTag("Player1") ||
            other.CompareTag("Player2") ||
            other.CompareTag("Player1Tutorial") ||
            other.CompareTag("Player2Tutorial"))
        {
            Debug.Log("Enter Death Trigger!");
            TriggerFailOnce();
        }
    }

    public void FailLevel()
    {
        if (s_IsFailing) return;
        TriggerFailOnce();
    }

    private void TriggerFailOnce()
    {
        Debug.Log("Failed!");


        s_IsFailing = true;

        // 立刻关掉触发器，避免同一帧/后续 collider 再触发
        if (_triggerCol != null) _triggerCol.enabled = false;

        // Audio（只播一次）
        if (loseClip != null && loseClip.Length > 0 && SoundFXManager.instance != null)
        {
            SoundFXManager.instance.PlayRandomSoundFXClip(loseClip, transform, 1f);
        }

        // Load（只调用一次）
        if (Scenecontroller.instance != null)
        {
            Scenecontroller.instance.LoadSceneAsset(sceneInfo);
        }

        s_IsFailing = false;

    }
}
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using SKCell;

[DisallowMultipleComponent]
public class LevelTrigger : MonoBehaviour
{
    [Header("Scene binding")]
    [Tooltip("Must be like Chapter0_Level1 / Chapter1_Level6 etc. We parse chapter/level from this.")]
    public SceneTitle scenetitle;

    [Header("Per-trigger UI (local)")]
    [Tooltip("Local prompt UI controlled by this trigger only. Default OFF, ON when player enters, OFF on exit.")]
    [SerializeField] private GameObject uiSelectLevel;

    [Header("Optional")]
    public GameObject spaceIndicator;

    private FlowManager flowManager;
    private bool startloading = false;

    private int chapterIndex0 = 0;
    private int levelIndex0 = 0;

    private Collider[] _colliders;
    private Renderer[] _renderers;

    // ✅ NEW: registry key
    private const string PP_SceneRegistry = "JZ_SavedPosScenes";

    private void Awake()
    {
        var s = scenetitle.ToString();
        var m = Regex.Match(s, @"Chapter\s*(\d+)\s*[_\-\s]\s*Level\s*(\d+)", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            int chap = int.Parse(m.Groups[1].Value);
            int lvl1 = int.Parse(m.Groups[2].Value);
            chapterIndex0 = Mathf.Max(0, chap);
            levelIndex0 = Mathf.Max(0, lvl1 - 1);
        }
        else
        {
            Debug.LogError($"[LevelTrigger:{name}] scenetitle '{s}' not in 'Chapter#_Level#' format. Fallback (0,0).", this);
            chapterIndex0 = 0;
            levelIndex0 = 0;
        }

        _colliders = GetComponentsInChildren<Collider>(true);
        _renderers = GetComponentsInChildren<Renderer>(true);

        SetSoftHidden(true);

        if (spaceIndicator) spaceIndicator.SetActive(false);
        if (uiSelectLevel) uiSelectLevel.SetActive(false);

        var fmObj = GameObject.Find("FlowManager");
        if (fmObj) flowManager = fmObj.GetComponent<FlowManager>();
    }

    private void OnEnable()
    {
        StartCoroutine(WaitAndDecideVisibility());
    }

    private IEnumerator WaitAndDecideVisibility()
    {
        int tries = 0;
        while (SaveManager.Instance == null && tries < 10)
        {
            tries++;
            yield return null;
        }

        if (SaveManager.Instance == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        bool unlocked = SaveManager.Instance.IsLevelUnlocked(chapterIndex0, levelIndex0);
        Debug.Log($"[LevelTrigger:{name}] Unlocked? {unlocked} (c={chapterIndex0}, l0={levelIndex0}, scenetitle={scenetitle})", this);

        if (!unlocked)
        {
            gameObject.SetActive(false);
            yield break;
        }

        SetSoftHidden(false);
        CheckInitialPlayerOverlap();
    }

    private void SetSoftHidden(bool hidden)
    {
        if (_colliders != null)
            foreach (var c in _colliders) if (c) c.enabled = !hidden;

        if (_renderers != null)
            foreach (var r in _renderers) if (r) r.enabled = !hidden;
    }

    private void CheckInitialPlayerOverlap()
    {
        var trigger = GetComponent<Collider>();
        if (!trigger || !trigger.enabled) return;

        Collider[] hits = Physics.OverlapBox(
            trigger.bounds.center,
            trigger.bounds.extents,
            trigger.transform.rotation
        );

        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                if (spaceIndicator) spaceIndicator.SetActive(true);
                if (uiSelectLevel) uiSelectLevel.SetActive(true);

                if (WorldLevelUI.Instance) WorldLevelUI.Instance.EnterTrigger(this);
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (spaceIndicator) spaceIndicator.SetActive(true);
        if (uiSelectLevel) uiSelectLevel.SetActive(true);

        if (WorldLevelUI.Instance) WorldLevelUI.Instance.EnterTrigger(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (spaceIndicator) spaceIndicator.SetActive(false);
        if (uiSelectLevel) uiSelectLevel.SetActive(false);

        if (WorldLevelUI.Instance) WorldLevelUI.Instance.ExitTrigger(this);
    }

    private void OnDisable()
    {
        if (spaceIndicator) spaceIndicator.SetActive(false);
        if (uiSelectLevel) uiSelectLevel.SetActive(false);

        if (WorldLevelUI.Instance) WorldLevelUI.Instance.ExitTrigger(this);
    }

    public void LoadNextLevel()
    {
        if (startloading) return;
        startloading = true;

        string currentSceneName = SceneManager.GetActiveScene().name;
        string targetSceneName = scenetitle.ToString();

        // ✅ NEW: register both scenes as “has saved pos”
        RegisterSceneForSavedPos(currentSceneName);
        RegisterSceneForSavedPos(targetSceneName);

        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerZ", transform.position.z);

        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerZ", transform.position.z);

        PlayerPrefs.SetString("LastTriggerScene", targetSceneName);
        PlayerPrefs.Save();

        SKUtils.InvokeAction(0.2f, () =>
        {
            if (flowManager)
                flowManager.LoadScene(new SceneInfo() { index = scenetitle });
            else
                SceneManager.LoadScene(targetSceneName);
        });
    }

    // -------------------- NEW helper --------------------
    private void RegisterSceneForSavedPos(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        // simple ; separated unique set
        string raw = PlayerPrefs.GetString(PP_SceneRegistry, "");
        string token = sceneName + ";";
        if (!raw.Contains(token))
        {
            raw += token;
            PlayerPrefs.SetString(PP_SceneRegistry, raw);
        }
    }
}
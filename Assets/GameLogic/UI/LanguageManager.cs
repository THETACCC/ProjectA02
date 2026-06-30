using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    private const string EnglishCode = "en";
    private const string ChineseCode = "zh-Hans";

    private const string LanguagePrefsKey = "SelectedLanguage";

    private bool isChangingLanguage = false;

    private void Awake()
    {
        // Singleton + DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        // 等 Localization 初始化
        yield return LocalizationSettings.InitializationOperation;

        // 读取之前保存的语言，没有就用当前 Unity 默认语言
        string savedCode = PlayerPrefs.GetString(LanguagePrefsKey, "");

        if (!string.IsNullOrEmpty(savedCode))
        {
            SetLocaleByCode(savedCode);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleLanguage();
        }
    }

    public void ToggleLanguage()
    {
        if (isChangingLanguage) return;

        StartCoroutine(ToggleLanguageRoutine());
    }

    private IEnumerator ToggleLanguageRoutine()
    {
        isChangingLanguage = true;

        yield return LocalizationSettings.InitializationOperation;

        string currentCode = LocalizationSettings.SelectedLocale.Identifier.Code;

        string targetCode;

        if (currentCode == EnglishCode)
        {
            targetCode = ChineseCode;
        }
        else
        {
            targetCode = EnglishCode;
        }

        SetLocaleByCode(targetCode);

        isChangingLanguage = false;
    }

    private void SetLocaleByCode(string localeCode)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;

        foreach (var locale in locales)
        {
            if (locale.Identifier.Code == localeCode)
            {
                LocalizationSettings.SelectedLocale = locale;

                PlayerPrefs.SetString(LanguagePrefsKey, localeCode);
                PlayerPrefs.Save();

                Debug.Log("Language changed to: " + locale.LocaleName + " / " + locale.Identifier.Code);

                return;
            }
        }

        Debug.LogWarning("Locale not found: " + localeCode);
    }
}
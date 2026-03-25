using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    [SerializeField, LocalizationKey] private string localizationKey;
    
    private Text standardText;
    private TMP_Text tmpText;

    private void Awake()
    {
        standardText = GetComponent<Text>();
        if (standardText == null) standardText = GetComponentInChildren<Text>();

        tmpText = GetComponent<TMP_Text>();
        if (tmpText == null) tmpText = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        // Đăng ký sự kiện lần cuối ở Start để chắc chắn Manager đã Awake xong
        // (OnEnable có thể đã miss lần này nếu Manager chưa tồn tại)
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText; // tránh đăng ký 2 lần
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        }
        UpdateText();
    }

    private void OnEnable()
    {
        // Chỉ subscribe nếu Manager đã sẵn sàng lúc này
        // Nếu chưa có, Start() sẽ lo sau
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            // Cập nhật text ngay khi enable lại (vd: khi bật panel)
            UpdateText();
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    /// <summary>
    /// Update the text value based on current language
    /// </summary>
    public void UpdateText()
    {
        if (string.IsNullOrEmpty(localizationKey)) return;
        if (LocalizationManager.Instance == null) return;

        string translatedText = LocalizationManager.Instance.GetText(localizationKey);

        if (tmpText != null)
        {
            tmpText.text = translatedText;
        }
        else if (standardText != null)
        {
            standardText.text = translatedText;
        }
    }

    /// <summary>
    /// Allow setting a new key dynamically at runtime
    /// </summary>
    public void SetKey(string newKey)
    {
        localizationKey = newKey;
        UpdateText();
    }
}

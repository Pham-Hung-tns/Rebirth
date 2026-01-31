using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    [Header("Player UI")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image ArmorBarImage;
    [SerializeField] private Image EnergyBarImage;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI energyText;

    [Header("Fade")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Level and Room Completed")]
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI completedText;

    [Header("UI Weapon")]
    [SerializeField] private GameObject weaponPanel;
    [SerializeField] private TextMeshProUGUI energyConsumptionText;
    [SerializeField] private Image weaponImage;

    [Header("Coin")]
    [SerializeField] private TextMeshProUGUI coinUI;

    [Header("GameOver")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Fire Button")]
    [SerializeField] private GameObject fireButton;

    [Header("Pickup Button")]
    [SerializeField] private GameObject pickupButton;

    [Header("Use Skill Button")]
    public Image useSkillButton;

    [Header("UI Boss")]
    public GameObject HealthBossPanel;
    public Image HealthUIBoss;

    protected override void Awake()
    {
        base.Awake();
    }

    private Coroutine skillCooldownCoroutine;

    // Event-driven updates (no polling in Update)
    private void OnPlayerStatsChanged(PlayerStatsData data)
    {
        if (healthBarImage != null)
            healthBarImage.fillAmount = Mathf.Clamp01((float)data.curHp / Mathf.Max(1f, data.maxHp));
        if (ArmorBarImage != null)
            ArmorBarImage.fillAmount = Mathf.Clamp01((float)data.curArmor / Mathf.Max(1f, data.maxArmor));
        if (EnergyBarImage != null)
            EnergyBarImage.fillAmount = Mathf.Clamp01((float)data.curEnergy / Mathf.Max(1f, data.maxEnergy));

        if (healthText != null) healthText.text = data.curHp + "/" + data.maxHp;
        if (armorText != null) armorText.text = data.curArmor + "/" + data.maxArmor;
        if (energyText != null) energyText.text = data.curEnergy + "/" + data.maxEnergy;
    }

    private void OnCoinChanged(float totalCoins)
    {
        if (coinUI != null)
            coinUI.text = totalCoins.ToString("0.00");
    }

    public void FadeNewDungeon(float value)
    {
        if (canvasGroup != null)
            StartCoroutine(Helper.IEFade(canvasGroup, value, 1.5f));
    }

    public void UpdateLevelText(string currentLevel)
    {
        if (currentLevelText != null)
            currentLevelText.text = currentLevel;
    }

    public void RoomCompleted()
    {
        StartCoroutine(IERoomCompleted());
    }

    private IEnumerator IERoomCompleted()
    {
        if (completedText != null)
        {
            completedText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            completedText.gameObject.SetActive(false);
        }
    }

    private void ShowUIWeapon(Weapon weapon)
    {
        if (weapon == null) return;
        if (weaponPanel != null && !weaponPanel.activeSelf)
            weaponPanel.SetActive(true);
        if (weaponImage != null) weaponImage.sprite = weapon.WeaponData.icon;
        if (energyConsumptionText != null) energyConsumptionText.text = weapon.WeaponData.energy.ToString();
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ReturnHome()
    {
        SceneManager.LoadScene(Settings.homeScene);
    }

    public void ShowPickupButton(bool isActive)
    {
        if (pickupButton != null) pickupButton.SetActive(isActive);
        if (fireButton != null) fireButton.SetActive(!isActive);
    }

    // Start skill cooldown animation (event-driven)
    public void StartSkillCooldown(float cooldown)
    {
        if (useSkillButton == null) return;
        if (skillCooldownCoroutine != null) StopCoroutine(skillCooldownCoroutine);
        skillCooldownCoroutine = StartCoroutine(IESkillCooldown(cooldown));
    }

    private IEnumerator IESkillCooldown(float cooldown)
    {
        if (useSkillButton == null) yield break;
        useSkillButton.fillAmount = 0f;
        float t = 0f;
        while (t < cooldown)
        {
            t += Time.unscaledDeltaTime;
            useSkillButton.fillAmount = Mathf.Clamp01(t / Mathf.Max(0.0001f, cooldown));
            yield return null;
        }
        useSkillButton.fillAmount = 1f;
    }

    private void ShowHealthUIBoss(float amount)
    {
        if (HealthUIBoss != null) HealthUIBoss.fillAmount = amount;
        if (HealthBossPanel != null) HealthBossPanel.SetActive(true);
    }

    private void OnEnable()
    {
        LevelManager.OnRoomCompleted += RoomCompleted;
        LevelManager.OnPlayerInRoomBoss += ShowHealthUIBoss;
        PlayerWeapon.OnShowUIWeaponEvent += ShowUIWeapon;
        PlayerVitality.OnPlayerDeathEvent += ShowGameOverPanel;
        // Subscribe to centralized UI events
        UIEvents.OnPlayerStatsChanged += OnPlayerStatsChanged;
        UIEvents.OnCoinChanged += OnCoinChanged;
        UIEvents.OnShowWeapon += ShowUIWeapon;
        UIEvents.OnStartSkillCooldown += StartSkillCooldown;
        UIEvents.OnPickupToggle += ShowPickupButton;
        UIEvents.OnLevelTextUpdate += UpdateLevelText;
        UIEvents.OnRoomCompleted += RoomCompleted;
        UIEvents.OnBossHealthUpdated += ShowHealthUIBoss;
        UIEvents.OnFadeNewDungeon += FadeNewDungeon;
        UIEvents.OnShowGameOver += ShowGameOverPanel;
    }

    private void OnDisable()
    {
        LevelManager.OnRoomCompleted -= RoomCompleted;
        LevelManager.OnPlayerInRoomBoss -= ShowHealthUIBoss;
        PlayerWeapon.OnShowUIWeaponEvent -= ShowUIWeapon;
        PlayerVitality.OnPlayerDeathEvent -= ShowGameOverPanel;
        UIEvents.OnPlayerStatsChanged -= OnPlayerStatsChanged;
        UIEvents.OnCoinChanged -= OnCoinChanged;
        UIEvents.OnShowWeapon -= ShowUIWeapon;
        UIEvents.OnStartSkillCooldown -= StartSkillCooldown;
        UIEvents.OnPickupToggle -= ShowPickupButton;
        UIEvents.OnLevelTextUpdate -= UpdateLevelText;
        UIEvents.OnRoomCompleted -= RoomCompleted;
        UIEvents.OnBossHealthUpdated -= ShowHealthUIBoss;
        UIEvents.OnFadeNewDungeon -= FadeNewDungeon;
        UIEvents.OnShowGameOver -= ShowGameOverPanel;
    }
}

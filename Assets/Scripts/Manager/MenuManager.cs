using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : Singleton<MenuManager>
{
    [Header("Config")]
    [SerializeField] private PlayerCreate[] playerCreates;

    [Header("UI and Stats")]
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI currentLevel;
    [SerializeField] private TextMeshProUGUI Story;
    [SerializeField] private TextMeshProUGUI maxHealthStat;
    [SerializeField] private TextMeshProUGUI maxEnergyStat;
    [SerializeField] private TextMeshProUGUI maxArmorStat;
    [SerializeField] private GameObject statPanel;
    [SerializeField] private GameObject weaponPanel;
    [SerializeField] private Image weaponImage;
    [SerializeField] private TextMeshProUGUI energyConsumptionText;

    [Header("Coin")]
    [SerializeField] private TextMeshProUGUI coinUI;
    [SerializeField] private TextMeshProUGUI unlockCharacterText;
    [SerializeField] private TextMeshProUGUI upgradeCharacterText;
    private SelectCharacter currentPlayer;
    private bool playerSelected;

    [Header("Button")]
    [SerializeField] Button unlockButton;
    [SerializeField] Button upgradeButton;
    [SerializeField] Button chooseButton;
    [SerializeField] GameObject moveButton;
    [SerializeField] GameObject fireButton;
    //[SerializeField] GameObject pickupButton;
    [SerializeField] GameObject useSkillButton;
    [SerializeField] GameObject changeWeaponButton;

    [Header("Skill Tree UI")]
    [SerializeField] private GameObject skillTreePanel;
    [SerializeField] private Button[] skillTreeButtons = new Button[3];
    [SerializeField] private TextMeshProUGUI skillTreeDetailText;
    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        coinUI.text = CoinManager.Instance.totalCoins.ToString();
        CreateCharactersInScene();
        SetupSkillTreeButtonListeners();
    }

    private void CreateCharactersInScene()
    {
        foreach (PlayerCreate character in playerCreates)
        {
            PlayerController player = Instantiate(character.Character,
                character.initialPosition.position,
                Quaternion.identity,
                character.initialPosition);

            // player cant not move
            player.enabled = false;
            player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
    }


    // Select character
    public void EnableMovement()
    {
        if (playerSelected) return;

        GameManager.Instance.playerPrefab = currentPlayer.PlayerConfig;
        playerPanel.SetActive(false);
        currentPlayer.GetComponent<PlayerController>().enabled = true;
        currentPlayer.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        playerSelected = true;
        moveButton.SetActive(true);
        fireButton.SetActive(true);
        useSkillButton.SetActive(true);
        changeWeaponButton.SetActive(true);
        //pickupButton.SetActive(true);
        statPanel.SetActive(true);
        currentPlayer.GetComponent<ClickHandle>().enabled = false;

        PlayerWeapon.OnShowUIWeaponEvent += ShowUIWeapon;
    }

    private void ShowUIWeapon(Weapon weapon)
    {
        if (weapon == null) return;
        if (weaponPanel != null && !weaponPanel.activeSelf)
            weaponPanel.SetActive(true);
        if (weaponImage != null) weaponImage.sprite = weapon.WeaponData.icon;
        if (energyConsumptionText != null) energyConsumptionText.text = weapon.WeaponData.energy.ToString();
    }
    // show stat of current player
    public void ShowStats(SelectCharacter player)
    {
        currentPlayer = player;

        playerPanel.SetActive(true);
        icon.sprite = currentPlayer.PlayerConfig.Icon;
        playerName.text = currentPlayer.PlayerConfig.Name;
        currentLevel.text = $"Level {currentPlayer.PlayerConfig.Level}";

        if (!currentPlayer.PlayerConfig.unlock)
        {
            upgradeButton.gameObject.SetActive(false);
            chooseButton.interactable = false;
            unlockButton.gameObject.SetActive(true);
            unlockCharacterText.text = $"Unlock\n({currentPlayer.PlayerConfig.unlockCost.ToString()})";
        }
        else
        {
            upgradeButton.gameObject.SetActive(true);
            chooseButton.interactable = true;
            unlockButton.gameObject.SetActive(false);
            upgradeCharacterText.text = $"Upgrade\n({currentPlayer.PlayerConfig.upgradeCost.ToString()})";
        }
        ResetStat();
        ShowSkillTreeUI();
    }


    public void UnLockCharacter()
    {
        if (CoinManager.Instance.totalCoins >= currentPlayer.PlayerConfig.unlockCost)
        {
            unlockButton.gameObject.SetActive(false);
            upgradeButton.gameObject.SetActive(true);
            upgradeCharacterText.text = $"Upgrade\n({currentPlayer.PlayerConfig.upgradeCost.ToString()})";
            chooseButton.interactable = true;
            ResetStat();
            CoinManager.Instance.RemoveCoin(currentPlayer.PlayerConfig.unlockCost);
            //// cap nhat so luong coin
            coinUI.text = CoinManager.Instance.totalCoins.ToString();
            // mo khoa nhan vat
            currentPlayer.PlayerConfig.unlock = true;
        }
    }

    public void UpgradeCharacter()
    {
        if (CoinManager.Instance.totalCoins >= currentPlayer.PlayerConfig.upgradeCost)
        {
            CoinManager.Instance.RemoveCoin(currentPlayer.PlayerConfig.upgradeCost);
            ////cap nhat so luong coin
            coinUI.text = CoinManager.Instance.totalCoins.ToString();
            UpgradeCharacterStats();
        }
    }

    public void UpgradeCharacterStats()
    {
        PlayerConfig config = currentPlayer.PlayerConfig;
        config.Level++;
        config.MaxHealth++;
        config.MaxEnergy += 10;
        config.MaxArmor++;
        config.upgradeCost = Mathf.RoundToInt(config.upgradeCost + config.upgradeCost * (config.upgradeCostPercent / 100f));
        upgradeCharacterText.text = $"Upgrade\n({config.upgradeCost.ToString()})";
        ResetStat();
    }
    public void ResetStat()
    {
        currentLevel.text = $"Level {currentPlayer.PlayerConfig.Level.ToString()}";
        maxHealthStat.text = $"{currentPlayer.PlayerConfig.MaxHealth.ToString()}";
        maxEnergyStat.text = $"{currentPlayer.PlayerConfig.MaxEnergy.ToString()}";
        maxArmorStat.text = $"{currentPlayer.PlayerConfig.MaxArmor.ToString()}";
    }

    public void Back()
    {
        playerPanel.SetActive(false);
    }
    private void Update()
    {
        coinUI.text = CoinManager.Instance.totalCoins.ToString();
    }

    // Skill Tree UI Methods
    private void SetupSkillTreeButtonListeners()
    {
        for (int i = 0; i < skillTreeButtons.Length; i++)
        {
            int index = i;
            skillTreeButtons[i].onClick.AddListener(() => DisplaySkillTreeDetail(index));
        }
    }

    private void DisplaySkillTreeDetail(int buttonIndex)
    {
        if (currentPlayer == null || currentPlayer.PlayerConfig == null) return;

        List<TechNode> skillNodes = currentPlayer.PlayerConfig.GetSkillNodes();
        
        if (buttonIndex >= 0 && buttonIndex < skillNodes.Count)
        {
            TechNode node = skillNodes[buttonIndex];
            string detailText = $"<b>Definition:</b>\n{node.tech.definition}";
            
            skillTreeDetailText.text = detailText;
        }
        else
        {
            skillTreeDetailText.text = "No skill available";
        }
    }

    public void ShowSkillTreeUI()
    {
        if (currentPlayer == null || currentPlayer.PlayerConfig == null) return;

        List<TechNode> skillNodes = currentPlayer.PlayerConfig.GetSkillNodes();
        
        // Setup and display the 3 buttons
        for (int i = 0; i < skillTreeButtons.Length; i++)
        {
            if (i < skillNodes.Count)
            {
                skillTreeButtons[i].gameObject.SetActive(true);
                
                // Display icon
                Image buttonImage = skillTreeButtons[i].GetComponent<Image>();
                if (buttonImage != null && skillNodes[i].tech.icon != null)
                {
                    buttonImage.sprite = skillNodes[i].tech.icon;
                }
                
                // Clear button text since we're showing icon
                TextMeshProUGUI buttonText = skillTreeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "";
                }
            }
            else
            {
                skillTreeButtons[i].gameObject.SetActive(false);
            }
        }

        skillTreePanel.SetActive(true);
        // Display first skill by default
        if (skillNodes.Count > 0)
        {
            DisplaySkillTreeDetail(0);
        }
    }

    public void HideSkillTreeUI()
    {
        skillTreePanel.SetActive(false);
    }
}

[Serializable]
public class PlayerCreate
{
    public PlayerController Character;
    public Transform initialPosition;
}


using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DS.ScriptableObjects;
using DS.Data;
using System.Collections.Generic;

public class DialogueUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    private DSDialogueSO currentDialogue;
    private NPCInteractable currentNPC;

    private void Start()
    {
        // Ẩn bảng hội thoại khi mới bắt đầu
        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// Bắt đầu một đoạn hội thoại mới từ NPCInteractable
    /// </summary>
    public void StartDialogue(NPCInteractable npc)
    {
        if (npc == null || npc.NpcDialogue == null || npc.NpcDialogue.StartingDialogue == null) return;
        
        currentNPC = npc;
        dialoguePanel.SetActive(true);

        // Resume nếu NPC đã lưu lại Node đang dang dở, nếu không thì bắt đầu từ đầu
        DSDialogueSO nodeToShow = currentNPC.CurrentNode != null ? currentNPC.CurrentNode : npc.NpcDialogue.StartingDialogue;
        ShowDialogue(nodeToShow);
    }

    /// <summary>
    /// Hiển thị nội dung của một cụm hội thoại (DSDialogueSO)
    /// </summary>
    private void ShowDialogue(DSDialogueSO dialogue)
    {
        dialoguePanel.SetActive(true);
        currentDialogue = dialogue;
        
        // Lưu lại tiến trình node hiện tại cho NPC
        if (currentNPC != null)
        {
            currentNPC.CurrentNode = dialogue;
        }

        dialogueText.text = dialogue.Text;

        ClearOldChoices();

        // Kiểm tra xem hội thoại này có các lựa chọn nào không
        if (dialogue.Choices != null && dialogue.Choices.Count > 0)
        {
            foreach (DSDialogueChoiceData choice in dialogue.Choices)
            {
                CreateChoiceButton(choice);
            }
        }
        else
        {
            // Nếu không có lựa chọn nào, tạo nút "Exit"
            CreateCloseButton("Exit");
        }
    }

    /// <summary>
    /// Tạo nút bấm cho mỗi lựa chọn
    /// </summary>
    private void CreateChoiceButton(DSDialogueChoiceData choice)
    {
        GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

        buttonText.text = choice.Text;

        // Bắt sự kiện click
        button.onClick.AddListener(() =>
        {
            if (choice.NextDialogue != null)
            {
                // Nếu lựa chọn này dẫn đến câu thoại khác
                ShowDialogue(choice.NextDialogue);
            }
            else
            {
                // Nếu NextDialogue bị trống -> Kết thúc hội thoại
                EndDialogue();
            }
        });
    }

    /// <summary>
    /// Tạo nút đóng khi hết hội thoại
    /// </summary>
    private void CreateCloseButton(string text = "Close")
    {
        GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

        buttonText.text = text;
        button.onClick.AddListener(EndDialogue);
    }

    /// <summary>
    /// Xóa các nút lựa chọn cũ
    /// </summary>
    private void ClearOldChoices()
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Tắt hộp thoại nếu NPC này bị người chơi bỏ chạy xa
    /// </summary>
    public void CloseDialogueIfNPC(NPCInteractable npc)
    {
        if (currentNPC == npc && dialoguePanel.activeSelf)
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Kết thúc và tắt bảng hội thoại
    /// </summary>
    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentDialogue = null;
        currentNPC = null;
    }
}

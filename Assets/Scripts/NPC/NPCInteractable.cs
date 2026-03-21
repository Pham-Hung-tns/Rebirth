using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Bắt buộc phải có Collider2D
[RequireComponent(typeof(DS.DSDialogue))] // Bắt buộc NPC này phải có sẵn cục DSDialogue
public class NPCInteractable : MonoBehaviour
{
    private DS.DSDialogue npcDialogue;

    // Lưu trữ Node đang nói chuyện dở dang
    [HideInInspector] public DS.ScriptableObjects.DSDialogueSO CurrentNode;

    // Cung cấp getter để UI Manager có thể truy cập
    public DS.DSDialogue NpcDialogue => npcDialogue;

    private void Awake()
    {
        // Tự động lấy component DSDialogue đang gắn trên cùng GameObject NPC này
        npcDialogue = GetComponent<DS.DSDialogue>();
    }

    // Hàm này sẽ được PlayerController gọi khi người chơi bấm nút Q
    public void Interact()
    {
        // Tìm ông quản lý UI và tung kịch bản của con NPC này ra
        DialogueUIManager uiManager = FindObjectOfType<DialogueUIManager>();
        
        if (uiManager != null)
        {
            Debug.Log("Player is interacting with NPC");
            uiManager.StartDialogue(this);
        }
    }

    // Khi Player bước vào vùng tương tác
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.PLAYER_TAG))
        {
            // Báo cho Player biết đây là đối tượng có thể tương tác
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetInteractable(this);
                        
            }
        }
    }

    // Khi Player bỏ đi ra khỏi vùng
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.PLAYER_TAG))
        {
            // Xóa đối tượng tương tác khỏi Player
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetInteractable(null);
            }

            // Ép tắt hộp thoại nếu Player bỏ chạy khi đang trò chuyện với NPC này
            DialogueUIManager uiManager = FindObjectOfType<DialogueUIManager>();
            if (uiManager != null)
            {
                uiManager.CloseDialogueIfNPC(this);
            }
        }
    }
}

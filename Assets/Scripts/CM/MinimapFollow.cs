using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // Kéo Player vào đây

    void LateUpdate()
    {
        if (player != null)
        {
            // Chỉ update vị trí X, Y. Giữ nguyên Z (độ cao camera)
            Vector3 newPos = player.position;
            newPos.z = transform.position.z; 
            transform.position = newPos;
            
            // Nếu bạn muốn minimap xoay theo hướng nhìn của player thì thêm dòng này:
            // transform.rotation = Quaternion.Euler(0, 0, player.eulerAngles.z);
        }
    }
}

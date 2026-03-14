using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

/// <summary>
/// Helper tool để tự động setup scene test cho Dungeon Builder trong Editor Mode
/// </summary>
public class SetupTestSceneEditor
{
    [MenuItem("Tools/Setup Test Scene/Auto Setup Test Scene")]
    public static void AutoSetupTestScene()
    {
        // Tạo scene mới hoặc dùng scene hiện tại
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            if (!EditorUtility.DisplayDialog("Save Scene?", 
                "Scene hiện tại chưa được lưu. Bạn có muốn lưu trước khi setup?", 
                "Lưu và tiếp tục", "Hủy"))
            {
                return;
            }
            EditorSceneManager.SaveOpenScenes();
        }

        // Kiểm tra và tạo DungeonBuilder
        DungeonBuilder dungeonBuilder = GameObject.FindObjectOfType<DungeonBuilder>();
        if (dungeonBuilder == null)
        {
            GameObject dbGO = new GameObject("Dungeon Builder");
            dungeonBuilder = dbGO.AddComponent<DungeonBuilder>();
            Undo.RegisterCreatedObjectUndo(dbGO, "Create DungeonBuilder");
            Debug.Log("Đã tạo DungeonBuilder GameObject");
        }

        // Kiểm tra GameResources trong scene hoặc Resources
        GameResources gameResources = Resources.Load<GameResources>("GameResources");
        if (gameResources == null)
        {
            Debug.LogWarning("Không tìm thấy GameResources trong Resources/GameResources. " +
                "Vui lòng tạo GameResources asset và đặt trong thư mục Resources!");
        }
        else
        {
            Debug.Log("Đã tìm thấy GameResources");
        }

        // Tạo Camera nếu chưa có
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCam = camGO.AddComponent<Camera>();
            mainCam.orthographic = true;
            mainCam.orthographicSize = 10f;
            camGO.transform.position = new Vector3(0, 0, -10);
            Undo.RegisterCreatedObjectUndo(camGO, "Create Main Camera");
            Debug.Log("Đã tạo Main Camera");
        }

        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        // Focus vào DungeonBuilder
        Selection.activeGameObject = dungeonBuilder.gameObject;
        EditorGUIUtility.PingObject(dungeonBuilder.gameObject);

        Debug.Log("=== Setup Test Scene hoàn tất! ===");
        Debug.Log("Các bước tiếp theo:");
        Debug.Log("1. Đảm bảo GameResources asset có RoomNodeTypeListSO được gán");
        Debug.Log("2. Tạo hoặc chọn DungeonLevelSO trong Resources (ví dụ: Resources/DungeonLevel_1-2)");
        Debug.Log("3. Sử dụng menu Tools/Test Dungeon Builder để test");
    }

    [MenuItem("Tools/Setup Test Scene/Create Empty Test Scene")]
    public static void CreateEmptyTestScene()
    {
        string scenePath = "Assets/Scenes/TestDungeonBuilder.unity";
        
        // Tạo scene mới
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        Debug.Log($"Đã tạo scene test tại: {scenePath}");
        Debug.Log("Bây giờ chạy Tools/Setup Test Scene/Auto Setup Test Scene để setup các GameObject cần thiết");
    }

    [MenuItem("Tools/Setup Test Scene/Validate Setup")]
    public static void ValidateSetup()
    {
        bool isValid = true;
        System.Text.StringBuilder issues = new System.Text.StringBuilder();

        // Kiểm tra DungeonBuilder
        DungeonBuilder dungeonBuilder = GameObject.FindObjectOfType<DungeonBuilder>();
        if (dungeonBuilder == null)
        {
            isValid = false;
            issues.AppendLine("❌ Không tìm thấy DungeonBuilder trong scene");
        }
        else
        {
            issues.AppendLine("✅ Tìm thấy DungeonBuilder");
        }

        // Kiểm tra GameResources
        GameResources gameResources = Resources.Load<GameResources>("GameResources");
        if (gameResources == null)
        {
            isValid = false;
            issues.AppendLine("❌ Không tìm thấy GameResources trong Resources/GameResources");
        }
        else
        {
            issues.AppendLine("✅ Tìm thấy GameResources");
            
            if (gameResources.roomNodeTypeList == null)
            {
                isValid = false;
                issues.AppendLine("❌ GameResources.roomNodeTypeList chưa được gán");
            }
            else
            {
                issues.AppendLine("✅ GameResources.roomNodeTypeList đã được gán");
            }
        }

        // Kiểm tra DungeonLevelSO
        DungeonLevelSO dungeonLevel = Resources.Load<DungeonLevelSO>("DungeonLevel_1-2");
        if (dungeonLevel == null)
        {
            isValid = false;
            issues.AppendLine("❌ Không tìm thấy DungeonLevelSO trong Resources/DungeonLevel_1-2");
            issues.AppendLine("   Tạo hoặc đổi tên file DungeonLevelSO thành 'DungeonLevel_1-2' và đặt trong Resources");
        }
        else
        {
            issues.AppendLine("✅ Tìm thấy DungeonLevelSO");
            
            if (dungeonLevel.roomTemplateList == null || dungeonLevel.roomTemplateList.Count == 0)
            {
                isValid = false;
                issues.AppendLine("❌ DungeonLevelSO.roomTemplateList trống");
            }
            else
            {
                issues.AppendLine($"✅ DungeonLevelSO có {dungeonLevel.roomTemplateList.Count} room templates");
            }

            if (dungeonLevel.roomNodeGraphList == null || dungeonLevel.roomNodeGraphList.Count == 0)
            {
                isValid = false;
                issues.AppendLine("❌ DungeonLevelSO.roomNodeGraphList trống");
            }
            else
            {
                issues.AppendLine($"✅ DungeonLevelSO có {dungeonLevel.roomNodeGraphList.Count} room node graphs");
            }
        }

        // Kiểm tra Camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            issues.AppendLine("⚠️ Không tìm thấy Main Camera (không bắt buộc)");
        }
        else
        {
            issues.AppendLine("✅ Tìm thấy Main Camera");
        }

        // Hiển thị kết quả
        Debug.Log("=== VALIDATION RESULT ===");
        Debug.Log(issues.ToString());
        
        if (isValid)
        {
            Debug.Log("✅ Setup hợp lệ! Bạn có thể sử dụng Tools/Test Dungeon Builder");
        }
        else
        {
            Debug.LogWarning("❌ Setup chưa hoàn chỉnh. Vui lòng sửa các vấn đề trên.");
        }
    }
}


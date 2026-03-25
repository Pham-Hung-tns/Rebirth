using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(LocalizationKeyAttribute))]
public class LocalizationKeyDrawer : PropertyDrawer
{
    private string[] keys;
    private bool initialized = false;

    private void Initialize()
    {
        if (initialized) return;
        
        string path = Application.dataPath + "/Resources/Localization/en.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            LocalizationData data = JsonUtility.FromJson<LocalizationData>(json);
            List<string> keyList = new List<string>();
            if (data != null && data.items != null)
            {
                foreach (var item in data.items)
                {
                    keyList.Add(item.key);
                }
            }
            keys = keyList.ToArray();
        }
        else
        {
            keys = new string[0];
        }
        initialized = true;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Initialize();

        if (property.propertyType == SerializedPropertyType.String)
        {
            if (keys != null && keys.Length > 0)
            {
                int currentIndex = Mathf.Max(0, System.Array.IndexOf(keys, property.stringValue));
                
                // Cập nhật vị trí cho icon/nút reload (tùy chọn)
                Rect popupRect = new Rect(position.x, position.y, position.width - 20, position.height);
                Rect buttonRect = new Rect(position.x + position.width - 15, position.y, 15, position.height);

                currentIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, keys);
                property.stringValue = keys[currentIndex];

                if (GUI.Button(buttonRect, "↺", EditorStyles.miniButton))
                {
                    initialized = false;
                    Initialize();
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use [LocalizationKey] with strings.");
        }
    }
}

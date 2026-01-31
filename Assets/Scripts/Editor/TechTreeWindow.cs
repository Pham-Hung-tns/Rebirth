using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TechTreeWindow : EditorWindow
{
    Vector2 nodeSize = new Vector2(140f,70f);
    Vector2 scrollPosition = Vector2.zero;
    Vector2 scrollStartPos;

    public TechTree targetTree;
    SerializedObject serializedTree;
    SerializedProperty treeProp;

    TechNode activeNode;
    SerializedProperty activeNodeProp;
    Vector2 mouseSelectionOffset;

    [MenuItem("Window/Tech Tree Editor")]
    public static void OpenWindow()
    {
        TechTreeWindow w = GetWindow<TechTreeWindow>("Tech Tree");
        w.minSize = new Vector2(600, 400);
    }

    void OnEnable()
    {
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        TechTree newTarget = (TechTree)EditorGUILayout.ObjectField("Tech Tree Asset", targetTree, typeof(TechTree), false);
        if (newTarget != targetTree)
        {
            targetTree = newTarget;
            if (targetTree != null)
            {
                serializedTree = new SerializedObject(targetTree);
                treeProp = serializedTree.FindProperty("tree");
                serializedTree.Update();
            }
            else
            {
                serializedTree = null;
                treeProp = null;
            }
        }

        if (serializedTree == null)
        {
            EditorGUILayout.HelpBox("Assign a TechTree asset to edit.", MessageType.Info);
            return;
        }

        Event e = Event.current;

        serializedTree.Update();

        Rect canvas = GUILayoutUtility.GetRect(position.width, position.height - 60);
        // Draw background
        EditorGUI.DrawRect(canvas, new Color(0.12f, 0.12f, 0.12f));

        // Handle scroll panning with middle mouse
        if (e.button == 2)
        {
            if (e.type == EventType.MouseDown)
            {
                scrollStartPos = e.mousePosition + scrollPosition;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag)
            {
                scrollPosition = -(e.mousePosition - scrollStartPos);
                Repaint();
            }
        }

        // Draw connections first
        if (treeProp != null)
        {
            for (int i = 0; i < treeProp.arraySize; i++)
            {
                SerializedProperty nodeProp = treeProp.GetArrayElementAtIndex(i);
                SerializedProperty techProp = nodeProp.FindPropertyRelative("tech");
                SerializedProperty reqsProp = nodeProp.FindPropertyRelative("requirements");
                SerializedProperty uiPosProp = nodeProp.FindPropertyRelative("UIposition");
                if (techProp.objectReferenceValue == null) continue;
            Vector2 uiPos = uiPosProp.vector2Value - scrollPosition + new Vector2(canvas.x, canvas.y);

                if (reqsProp != null)
                {
                    for (int r = 0; r < reqsProp.arraySize; r++)
                    {
                        SerializedProperty req = reqsProp.GetArrayElementAtIndex(r);
                        if (req.objectReferenceValue == null) continue;
                        int reqIdx = FindTechIndex((Tech)req.objectReferenceValue);
                        if (reqIdx == -1) continue;
                        SerializedProperty reqNode = treeProp.GetArrayElementAtIndex(reqIdx);
                        Vector2 reqUi = reqNode.FindPropertyRelative("UIposition").vector2Value - scrollPosition + new Vector2(canvas.x, canvas.y);

                        Vector2 start = uiPos + new Vector2(-12f, 10f);
                        Vector2 end = reqUi + new Vector2(100f, 10f);
                        Handles.DrawBezier(start, end, start + Vector2.left * 100, end + Vector2.right * 100, Color.white, null, 3f);

                        // draw simple arrowhead at end
                        Color prev = Handles.color;
                        Handles.color = Color.white;
                        Vector2 dir = (end - start).normalized;
                        float arrowSize = 8f;
                        Vector2 basePoint = end - dir * arrowSize;
                        Vector2 perp = new Vector2(-dir.y, dir.x) * (arrowSize * 0.5f);
                        Handles.DrawAAConvexPolygon(end, basePoint + perp, basePoint - perp);
                        Handles.color = prev;
                    }
                }
            }
        }

        // Draw nodes
        if (treeProp != null)
        {
            for (int i = 0; i < treeProp.arraySize; i++)
            {
                SerializedProperty nodeProp = treeProp.GetArrayElementAtIndex(i);
                SerializedProperty techProp = nodeProp.FindPropertyRelative("tech");
                SerializedProperty costProp = nodeProp.FindPropertyRelative("researchCost");
                SerializedProperty levelProp = nodeProp.FindPropertyRelative("level");
                SerializedProperty uiPosProp = nodeProp.FindPropertyRelative("UIposition");

                Vector2 uiPos = uiPosProp.vector2Value - scrollPosition + new Vector2(canvas.x, canvas.y);
                Rect nodeRect = new Rect(uiPos, nodeSize);

                string label = techProp.objectReferenceValue != null ? techProp.objectReferenceValue.name : "NULL TECH";
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.LabelField(nodeRect, "");
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginFoldoutHeaderGroup(nodeRect, true, label);

                // draw tech icon (if any)
                Tech techObj = techProp.objectReferenceValue as Tech;
                if (techObj != null && techObj.icon != null)
                {
                    Rect imgRect = new Rect(uiPos + new Vector2(4f, 4f), new Vector2(40f, 40f));
                    GUI.DrawTexture(imgRect, techObj.icon.texture, ScaleMode.ScaleToFit);
                }

                // small fields inside node (moved right of image)
                Rect costRect = new Rect(uiPos + new Vector2(52f, 12f), new Vector2(84f, 16f));
                EditorGUI.LabelField(new Rect(costRect.x, costRect.y, 44f, costRect.height), "Cost:");
                int newCost = EditorGUI.IntField(new Rect(costRect.x + 44f, costRect.y, 36f, costRect.height), costProp.intValue);
                if (newCost != costProp.intValue)
                {
                    Undo.RecordObject(serializedTree.targetObject, "Edit Tech Node Cost");
                    costProp.intValue = newCost;
                    serializedTree.ApplyModifiedProperties();
                }

                Rect levelRect = new Rect(uiPos + new Vector2(52f, 32f), new Vector2(84f, 16f));
                EditorGUI.LabelField(new Rect(levelRect.x, levelRect.y, 52f, levelRect.height), "Level:");
                int newLevel = EditorGUI.IntField(new Rect(levelRect.x + 52f, levelRect.y, 28f, levelRect.height), levelProp.intValue);
                if (newLevel != levelProp.intValue)
                {
                    Undo.RecordObject(serializedTree.targetObject, "Edit Tech Node Level");
                    levelProp.intValue = newLevel;
                    serializedTree.ApplyModifiedProperties();
                }

                // handle mouse events for move/select
                Event evt = Event.current;
                if (nodeRect.Contains(evt.mousePosition))
                {
                    if (evt.type == EventType.MouseDown && evt.button == 0)
                    {
                        activeNodeProp = nodeProp;
                        activeNode = null;
                        mouseSelectionOffset = uiPosProp.vector2Value - evt.mousePosition;
                        evt.Use();
                    }
                    else if (evt.type == EventType.MouseUp && evt.button == 0)
                    {
                        activeNodeProp = null;
                        evt.Use();
                    }
                }

                if (activeNodeProp != null && evt.type == EventType.MouseDrag && evt.button == 0)
                {
                    Undo.RecordObject(serializedTree.targetObject, "Move Tech Node");
                    Vector2 newUi = evt.mousePosition + mouseSelectionOffset;
                    activeNodeProp.FindPropertyRelative("UIposition").vector2Value = newUi;
                    serializedTree.ApplyModifiedProperties();
                    Repaint();
                    evt.Use();
                }
                EditorGUI.EndFoldoutHeaderGroup();
            }
        }

        // Drag & drop add
        if (e.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            e.Use();
        }
        else if (e.type == EventType.DragPerform)
        {
            bool added = false;
            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
            {
                if (DragAndDrop.objectReferences[i] is Tech)
                {
                    Tech t = DragAndDrop.objectReferences[i] as Tech;
                    int insert = treeProp.arraySize;
                    treeProp.InsertArrayElementAtIndex(insert);
                    SerializedProperty newElem = treeProp.GetArrayElementAtIndex(insert);
                    newElem.FindPropertyRelative("tech").objectReferenceValue = t;
                    newElem.FindPropertyRelative("requirements").ClearArray();
                    newElem.FindPropertyRelative("researchCost").intValue = 0;
                    newElem.FindPropertyRelative("researchInvested").intValue = 0;
                    newElem.FindPropertyRelative("UIposition").vector2Value = e.mousePosition + scrollPosition;
                    added = true;
                }
            }
            if (added)
            {
                Undo.RecordObject(serializedTree.targetObject, "Add Tech Node");
                serializedTree.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedTree.targetObject);
            }
            e.Use();
        }

        // Bottom UI: list and delete
        GUILayout.BeginArea(new Rect(4, position.height - 52, position.width - 8, 48));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh"))
        {
            serializedTree = new SerializedObject(targetTree);
            treeProp = serializedTree.FindProperty("tree");
        }
        if (GUILayout.Button("Clear Empty Nodes"))
        {
            Undo.RecordObject(serializedTree.targetObject, "Clear Empty Nodes");
            for (int i = treeProp.arraySize - 1; i >= 0; i--)
            {
                if (treeProp.GetArrayElementAtIndex(i).FindPropertyRelative("tech").objectReferenceValue == null)
                    treeProp.DeleteArrayElementAtIndex(i);
            }
            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedTree.targetObject);
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
        if (GUILayout.Button("Export JSON"))
        {
            ExportToJson();
        }
        if (GUILayout.Button("Import JSON"))
        {
            ImportFromJson();
        }

        serializedTree.ApplyModifiedProperties();
    }

    int FindTechIndex(Tech tech)
    {
        if (treeProp == null) return -1;
        for (int i = 0; i < treeProp.arraySize; i++)
        {
            SerializedProperty p = treeProp.GetArrayElementAtIndex(i).FindPropertyRelative("tech");
            if (p != null && p.objectReferenceValue == tech) return i;
        }
        return -1;
    }

    // JSON DTOs for import/export
    [System.Serializable]
    class NodeDTO
    {
        public string techGUID;
        public int researchCost;
        public int level;
        public Vector2 UIposition;
        public List<string> requirementsGUIDs;
    }
    [System.Serializable]
    class TechTreeDTO
    {
        public List<NodeDTO> nodes = new List<NodeDTO>();
    }

    void ExportToJson()
    {
        if (targetTree == null)
        {
            EditorUtility.DisplayDialog("Export", "No TechTree selected.", "OK");
            return;
        }
        TechTreeDTO dto = new TechTreeDTO();
        for (int i = 0; i < treeProp.arraySize; i++)
        {
            SerializedProperty nodeProp = treeProp.GetArrayElementAtIndex(i);
            SerializedProperty techProp = nodeProp.FindPropertyRelative("tech");
            SerializedProperty reqsProp = nodeProp.FindPropertyRelative("requirements");
            SerializedProperty costProp = nodeProp.FindPropertyRelative("researchCost");
            SerializedProperty levelProp = nodeProp.FindPropertyRelative("level");
            SerializedProperty uiPosProp = nodeProp.FindPropertyRelative("UIposition");

            NodeDTO n = new NodeDTO();
            n.techGUID = techProp.objectReferenceValue != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(techProp.objectReferenceValue)) : "";
            n.researchCost = costProp.intValue;
            n.level = levelProp.intValue;
            n.UIposition = uiPosProp.vector2Value;
            n.requirementsGUIDs = new List<string>();
            if (reqsProp != null)
            {
                for (int r = 0; r < reqsProp.arraySize; r++)
                {
                    var req = reqsProp.GetArrayElementAtIndex(r);
                    string g = req.objectReferenceValue != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(req.objectReferenceValue)) : "";
                    n.requirementsGUIDs.Add(g);
                }
            }
            dto.nodes.Add(n);
        }

        string json = JsonUtility.ToJson(dto, true);
        string path = EditorUtility.SaveFilePanel("Export TechTree to JSON", "", targetTree.name + ".json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            EditorUtility.RevealInFinder(path);
        }
    }

    void ImportFromJson()
    {
        string path = EditorUtility.OpenFilePanel("Import TechTree JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;
        string content = File.ReadAllText(path);
        TechTreeDTO dto = null;
        try { dto = JsonUtility.FromJson<TechTreeDTO>(content); } catch { }
        if (dto == null)
        {
            EditorUtility.DisplayDialog("Import", "Failed to parse JSON.", "OK");
            return;
        }

        Undo.RecordObject(serializedTree.targetObject, "Import TechTree JSON");
        // clear existing
        for (int i = treeProp.arraySize - 1; i >= 0; i--)
            treeProp.DeleteArrayElementAtIndex(i);

        // populate
        for (int i = 0; i < dto.nodes.Count; i++)
        {
            treeProp.InsertArrayElementAtIndex(i);
            SerializedProperty newElem = treeProp.GetArrayElementAtIndex(i);
            NodeDTO n = dto.nodes[i];
            Tech t = null;
            if (!string.IsNullOrEmpty(n.techGUID))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(n.techGUID);
                if (!string.IsNullOrEmpty(assetPath))
                    t = AssetDatabase.LoadAssetAtPath<Tech>(assetPath);
            }
            newElem.FindPropertyRelative("tech").objectReferenceValue = t;
            newElem.FindPropertyRelative("researchCost").intValue = n.researchCost;
            newElem.FindPropertyRelative("level").intValue = n.level;
            newElem.FindPropertyRelative("UIposition").vector2Value = n.UIposition;
            var reqs = newElem.FindPropertyRelative("requirements");
            reqs.ClearArray();
            if (n.requirementsGUIDs != null)
            {
                for (int r = 0; r < n.requirementsGUIDs.Count; r++)
                {
                    reqs.InsertArrayElementAtIndex(reqs.arraySize);
                    string rg = n.requirementsGUIDs[r];
                    Tech rt = null;
                    if (!string.IsNullOrEmpty(rg))
                    {
                        string rp = AssetDatabase.GUIDToAssetPath(rg);
                        if (!string.IsNullOrEmpty(rp)) rt = AssetDatabase.LoadAssetAtPath<Tech>(rp);
                    }
                    reqs.GetArrayElementAtIndex(reqs.arraySize - 1).objectReferenceValue = rt;
                }
            }
        }

        serializedTree.ApplyModifiedProperties();
        EditorUtility.SetDirty(serializedTree.targetObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

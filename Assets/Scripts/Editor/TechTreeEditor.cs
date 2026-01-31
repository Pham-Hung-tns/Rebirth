using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[CustomEditor(typeof(TechTree))]
public class TechTreeEditor : Editor
{
    // positioning
    Vector2 nodeSize = new Vector2(150f,100f);
    float minTreeHeight = 720f;
    float minTreeWidth = 1000f;
    Vector2 incomingEdgVec = new Vector2(100f, 10f);
    Vector2 outgoingEdgVec = new Vector2(-12f, 10f);
    Vector2 upArrowVec = new Vector2(-10f,-10f);
    Vector2 downArrowVec = new Vector2(-10f, 10f);
    Vector2 nextLineVec = new Vector2(0f, 20f);
    Vector2 indentVec = new Vector2(102f, 0f);
    Vector2 nodeContentSize = new Vector2(40f, 20f);
    Vector2 nodeLabelSize = new Vector2(100f, 20f);

    // scrolling and moving
    Vector2 mouseSelectionOffset;
    Vector2 scrollPosition = Vector2.zero;
    Vector2 scrollStartPos;

    TechNode activeNode;
    TechNode selectedNode;

    public override void OnInspectorGUI()
    {
        TechTree targetTree = (TechTree)target;

        // Mouse Events
        Event currentEvent = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        EventType UIEvent = currentEvent.GetTypeForControl(controlID);

        // Node styles
        GUIStyle nodeStyle = new GUIStyle(EditorStyles.helpBox);
        GUIStyle selectedNodeStyle = new GUIStyle(EditorStyles.helpBox);
        selectedNodeStyle.fontStyle = FontStyle.BoldAndItalic;

        // The techtree view
        EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.MinHeight(720));

        if (targetTree.tree != null)
        {
            for(int nodeIdx = 0; nodeIdx < targetTree.tree.Count; nodeIdx++)
            {
                if (targetTree.tree[nodeIdx] == null) continue;
                // Draw node
                Rect nodeRect = new Rect(targetTree.tree[nodeIdx].UIposition - scrollPosition, nodeSize);
                string techName = targetTree.tree[nodeIdx].tech != null ? targetTree.tree[nodeIdx].tech.name : "NULL TECH";
                
                if (targetTree.tree[nodeIdx].tech == null) continue;
                
                // Draw node background
                GUI.Box(nodeRect, "", (selectedNode==targetTree.tree[nodeIdx]? selectedNodeStyle : nodeStyle));
                
                // Draw icon on the left outside the node
                Rect iconRect = new Rect(nodeRect.x - 50f, nodeRect.y, 50f, 50f);
                if (targetTree.tree[nodeIdx].tech != null && targetTree.tree[nodeIdx].tech.icon != null)
                {
                    GUI.DrawTexture(iconRect, targetTree.tree[nodeIdx].tech.icon.texture, ScaleMode.ScaleToFit);
                }
                
                // Draw foldout header
                Rect headerRect = new Rect(nodeRect.x, nodeRect.y, nodeRect.width, 20f);
                bool expanded = EditorGUI.Foldout(headerRect, true, techName, true);
                
                if (expanded)
                {
                    EditorGUI.LabelField(new Rect(targetTree.tree[nodeIdx].UIposition - scrollPosition + nextLineVec, nodeLabelSize), "Research cost: ");

                    int newCost = EditorGUI.IntField(new Rect(targetTree.tree[nodeIdx].UIposition - scrollPosition + nextLineVec + indentVec, nodeContentSize), targetTree.tree[nodeIdx].researchCost);
                    if (newCost != targetTree.tree[nodeIdx].researchCost)
                    {
                        Undo.RecordObject(targetTree, "Edit Tech Node");
                        targetTree.tree[nodeIdx].researchCost = newCost;
                        EditorUtility.SetDirty(targetTree);
                    }

                    EditorGUI.LabelField(new Rect(targetTree.tree[nodeIdx].UIposition - scrollPosition + nextLineVec*2, nodeLabelSize),"Level: ");

                    int newLevel = EditorGUI.IntField(new Rect(targetTree.tree[nodeIdx].UIposition - scrollPosition + nextLineVec * 2 + indentVec, nodeContentSize), targetTree.tree[nodeIdx].level);
                    if (newLevel != targetTree.tree[nodeIdx].level)
                    {
                        Undo.RecordObject(targetTree, "Edit Tech Node");
                        targetTree.tree[nodeIdx].level = newLevel;
                        EditorUtility.SetDirty(targetTree);
                    }
                }

                if (targetTree.tree[nodeIdx].requirements != null)
                {
                    foreach(Tech req in targetTree.tree[nodeIdx].requirements)
                    {
                        int reqIdx = targetTree.FindTechIndex(req);
                        if(reqIdx != -1)
                        {
                            // Draw connecting curve
                            Handles.DrawBezier(targetTree.tree[nodeIdx].UIposition - scrollPosition + outgoingEdgVec,
                                targetTree.tree[reqIdx].UIposition - scrollPosition + incomingEdgVec,
                                targetTree.tree[nodeIdx].UIposition - scrollPosition + outgoingEdgVec + Vector2.left * 100,
                                targetTree.tree[reqIdx].UIposition - scrollPosition + incomingEdgVec + Vector2.right * 100,
                                Color.white
                                , null
                                , 3f);

                            // Draw arrow
                            Handles.DrawLine(targetTree.tree[nodeIdx].UIposition - scrollPosition + outgoingEdgVec, targetTree.tree[nodeIdx].UIposition - scrollPosition + outgoingEdgVec + upArrowVec);
                            Handles.DrawLine(targetTree.tree[nodeIdx].UIposition - scrollPosition + outgoingEdgVec, targetTree.tree[nodeIdx].UIposition - scrollPosition + outgoingEdgVec + downArrowVec);
                        }

                        else Debug.LogWarning("missing tech " + req.name + " in tech tree!");
                    }
                }

                // mouse events
                if (nodeRect.Contains(currentEvent.mousePosition))
                {
                    if(UIEvent == EventType.MouseDown)
                    {
                        if (currentEvent.button == 0)
                        {
                            activeNode = targetTree.tree[nodeIdx];
                            mouseSelectionOffset = activeNode.UIposition - currentEvent.mousePosition;
                            Undo.RecordObject(targetTree, "Move Tech Node");
                        }
                        else if (currentEvent.button == 1)
                        {
                            selectedNode = targetTree.tree[nodeIdx];
                            Repaint();
                        }
                    }
                    else
                    // Create/Destroy connections
                    if (UIEvent == EventType.MouseUp)
                    {
                        if(currentEvent.button == 1 && selectedNode != null && selectedNode != targetTree.tree[nodeIdx])
                        {
                            Undo.RecordObject(targetTree, "Modify TechTree");
                            if(targetTree.tree[nodeIdx].requirements.Contains(selectedNode.tech))
                                targetTree.tree[nodeIdx].requirements.Remove(selectedNode.tech);
                            else if(selectedNode.requirements.Contains(targetTree.tree[nodeIdx].tech))
                                selectedNode.requirements.Remove(targetTree.tree[nodeIdx].tech);
                            else
                            if(targetTree.IsConnectible( targetTree.tree.IndexOf(selectedNode), nodeIdx))
                            {

                                targetTree.tree[nodeIdx].requirements.Add(selectedNode.tech);

                                for(int k=0; k < targetTree.tree.Count; k++)
                                    targetTree.CorrectRequirementsCascades(k);
                            }
                            EditorUtility.SetDirty(targetTree);
                        }
                    }
                }
            }
        }

        // Scroll in the Tech Tree view
        if(currentEvent.button == 2)
        {
            if(currentEvent.type == EventType.MouseDown)
            {
                scrollStartPos = currentEvent.mousePosition + scrollPosition;
            }
            else if(currentEvent.type == EventType.MouseDrag)
            {
                scrollPosition = -(currentEvent.mousePosition - scrollStartPos);
                Repaint();
            }
        }

        if(selectedNode != null && currentEvent.button == 1)
        {
            Handles.DrawBezier(currentEvent.mousePosition,
                selectedNode.UIposition - scrollPosition + incomingEdgVec,
                currentEvent.mousePosition + Vector2.left * 100,
                selectedNode.UIposition - scrollPosition + incomingEdgVec + Vector2.right * 100,
                Color.white,
                null,
                1.5f);
            Repaint();
        }

        // Move nodes with left mouse button
        if(UIEvent == EventType.MouseUp)
        {
            activeNode = null;
        }
        else if (UIEvent == EventType.MouseDrag)
        {
            if( activeNode != null)
            {
                activeNode.UIposition = currentEvent.mousePosition + mouseSelectionOffset;
                EditorUtility.SetDirty(targetTree);
            }
        }

        // Import new Tech
        if(currentEvent.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
        else if (currentEvent.type == EventType.DragPerform)
        {
            for (int i = 0; i< DragAndDrop.objectReferences.Length; i++)
            {
                if( DragAndDrop.objectReferences[i] is Tech)
                {
                    Undo.RecordObject(targetTree, "Add Tech Node");
                    targetTree.AddNode(DragAndDrop.objectReferences[i] as Tech, 1, currentEvent.mousePosition + scrollPosition);
                    EditorUtility.SetDirty(targetTree);
                }
            }
        }
        EditorGUILayout.EndScrollView();

        scrollPosition.x = GUILayout.HorizontalScrollbar(scrollPosition.x, 20f, 0f, minTreeWidth);
        scrollPosition.y = GUI.VerticalScrollbar(new Rect(0,0,20,720),scrollPosition.y, 20f, 0f, minTreeHeight);

        EditorGUILayout.BeginHorizontal();
        if(selectedNode == null || selectedNode.tech == null)
        {
            EditorGUILayout.LabelField("No tech selected");
        }
        else
        {
            EditorGUILayout.LabelField("Selected Tech: " + selectedNode.tech.name);
            if(GUILayout.Button("Delete Tech"))
            {
                Undo.RecordObject(targetTree, "Delete Tech");
                targetTree.DeleteNode(selectedNode.tech);
                if(activeNode == selectedNode) activeNode = null;
                selectedNode = null;
                EditorUtility.SetDirty(targetTree);
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Export JSON"))
        {
            ExportToJson(targetTree);
        }
        if (GUILayout.Button("Import JSON"))
        {
            ImportFromJson(targetTree);
        }
        if (GUILayout.Button("Undo"))
        {
            Undo.PerformUndo();
        }
        if (GUILayout.Button("Redo"))
        {
            Undo.PerformRedo();
        }
        EditorGUILayout.EndHorizontal();
        EditorUtility.SetDirty(targetTree);
    }

    [Serializable]
    class NodeDTO
    {
        public string techGUID;
        public int researchCost;
        public int level;
        public Vector2 UIposition;
        public List<string> requirementsGUIDs;
    }
    [Serializable]
    class TechTreeDTO
    {
        public List<NodeDTO> nodes = new List<NodeDTO>();
    }

    void ExportToJson(TechTree targetTree)
    {
        if (targetTree == null)
        {
            EditorUtility.DisplayDialog("Export", "No TechTree selected.", "OK");
            return;
        }
        TechTreeDTO dto = new TechTreeDTO();
        if (targetTree.tree != null)
        {
            for (int i = 0; i < targetTree.tree.Count; i++)
            {
                TechNode node = targetTree.tree[i];
                NodeDTO n = new NodeDTO();
                n.techGUID = node.tech != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(node.tech)) : "";
                n.researchCost = node.researchCost;
                n.level = node.level;
                n.UIposition = node.UIposition;
                n.requirementsGUIDs = new List<string>();
                if (node.requirements != null)
                {
                    for (int r = 0; r < node.requirements.Count; r++)
                    {
                        Tech req = node.requirements[r];
                        string g = req != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(req)) : "";
                        n.requirementsGUIDs.Add(g);
                    }
                }
                dto.nodes.Add(n);
            }
        }

        string json = JsonUtility.ToJson(dto, true);
        string path = EditorUtility.SaveFilePanel("Export TechTree to JSON", "", "TechTree.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            EditorUtility.RevealInFinder(path);
        }
    }

    void ImportFromJson(TechTree targetTree)
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

        Undo.RecordObject(targetTree, "Import TechTree JSON");
        // clear existing
        if (targetTree.tree != null) targetTree.tree.Clear();
        else targetTree.tree = new List<TechNode>();

        // create nodes
        for (int i = 0; i < dto.nodes.Count; i++)
        {
            NodeDTO n = dto.nodes[i];
            Tech t = null;
            if (!string.IsNullOrEmpty(n.techGUID))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(n.techGUID);
                if (!string.IsNullOrEmpty(assetPath))
                    t = AssetDatabase.LoadAssetAtPath<Tech>(assetPath);
            }
            TechNode newNode = new TechNode(t, new List<Tech>(), n.researchCost, n.level, n.UIposition);
            targetTree.tree.Add(newNode);
        }

        // assign requirements
        for (int i = 0; i < dto.nodes.Count; i++)
        {
            NodeDTO n = dto.nodes[i];
            TechNode node = targetTree.tree[i];
            node.requirements = new List<Tech>();
            if (n.requirementsGUIDs != null)
            {
                for (int r = 0; r < n.requirementsGUIDs.Count; r++)
                {
                    string rg = n.requirementsGUIDs[r];
                    Tech rt = null;
                    if (!string.IsNullOrEmpty(rg))
                    {
                        string rp = AssetDatabase.GUIDToAssetPath(rg);
                        if (!string.IsNullOrEmpty(rp)) rt = AssetDatabase.LoadAssetAtPath<Tech>(rp);
                    }
                    node.requirements.Add(rt);
                }
            }
        }

        EditorUtility.SetDirty(targetTree);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

public class TechTreeAssetPostprocessor : AssetPostprocessor
{
    // [UnityEditor.Callbacks.OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj is TechTree)
        {
            TechTreeWindow w = EditorWindow.GetWindow<TechTreeWindow>("Tech Tree Editor");
            w.targetTree = (TechTree)obj;
            w.minSize = new Vector2(600, 400);
            return true; // Prevent default Inspector from opening
        }
        return false;
    }
}

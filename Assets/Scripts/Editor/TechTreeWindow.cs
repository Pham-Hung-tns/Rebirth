using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TechTreeWindow : EditorWindow
{
    // === Layout ===
    private Vector2 nodeSize = new Vector2(150f, 100f);
    private Vector2 incomingEdgVec = new Vector2(100f, 10f);
    private Vector2 outgoingEdgVec = new Vector2(-12f, 10f);
    private Vector2 upArrowVec = new Vector2(-10f, -10f);
    private Vector2 downArrowVec = new Vector2(-10f, 10f);
    private Vector2 nextLineVec = new Vector2(0f, 20f);
    private Vector2 indentVec = new Vector2(102f, 0f);
    private Vector2 nodeContentSize = new Vector2(40f, 20f);
    private Vector2 nodeLabelSize = new Vector2(100f, 20f);

    // === State ===
    public TechTree targetTree;
    private Vector2 scrollPosition = Vector2.zero;
    private Vector2 scrollStartPos;

    // Node interaction
    private TechNode activeNode;       // Node đang được kéo (left-click drag)
    private TechNode selectedNode;     // Node đã chọn để tạo kết nối (right-click)
    private Vector2 mouseSelectionOffset;

    // Grid
    private const float gridSmall = 25f;
    private const float gridLarge = 100f;

    [MenuItem("Window/Tech Tree Editor")]
    public static void OpenWindow()
    {
        TechTreeWindow w = GetWindow<TechTreeWindow>("Tech Tree Editor");
        w.minSize = new Vector2(600, 400);
    }

    void OnEnable()
    {
        // Subscribe to selection change để auto-load khi click SO trong Project
        Selection.selectionChanged += OnSelectionChanged;
    }

    void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        // Auto-load TechTree khi select trong Project window
        if (Selection.activeObject is TechTree tree)
        {
            targetTree = tree;
            Repaint();
        }
    }

    void OnGUI()
    {
        // === Header: Object field ===
        EditorGUILayout.Space(4);
        TechTree newTarget = (TechTree)EditorGUILayout.ObjectField("Tech Tree Asset", targetTree, typeof(TechTree), false);
        if (newTarget != targetTree)
        {
            targetTree = newTarget;
            selectedNode = null;
            activeNode = null;
        }

        if (targetTree == null)
        {
            EditorGUILayout.HelpBox("Assign a TechTree asset to edit.\nHoặc double-click vào TechTree SO trong Project window.", MessageType.Info);
            return;
        }

        // === Canvas area ===
        Rect canvas = GUILayoutUtility.GetRect(position.width, position.height - 80);
        GUI.BeginClip(canvas);
        {
            // Background
            EditorGUI.DrawRect(new Rect(0, 0, canvas.width, canvas.height), new Color(0.15f, 0.15f, 0.15f));

            // Grid
            DrawGrid(gridSmall, 0.15f, canvas);
            DrawGrid(gridLarge, 0.25f, canvas);

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            EventType uiEvent = e.GetTypeForControl(controlID);

            // Draw connections (bezier curves)
            DrawConnections();

            // Draw live connection line (khi đang kéo từ selectedNode)
            DrawLiveConnectionLine(e);

            // Draw nodes + handle node events
            DrawNodesAndProcessEvents(e, uiEvent);

            // Process canvas-level events (pan, drag-drop, deselect)
            ProcessCanvasEvents(e, uiEvent, canvas);
        }
        GUI.EndClip();

        // === Bottom toolbar ===
        DrawToolbar();

        // Force repaint khi đang kéo node hoặc kéo connection
        if (activeNode != null || selectedNode != null)
            Repaint();
    }

    #region Grid Drawing

    private void DrawGrid(float spacing, float opacity, Rect canvas)
    {
        int cols = Mathf.CeilToInt(canvas.width / spacing) + 1;
        int rows = Mathf.CeilToInt(canvas.height / spacing) + 1;

        Handles.color = new Color(0.5f, 0.5f, 0.5f, opacity);

        float offsetX = -(scrollPosition.x % spacing);
        float offsetY = -(scrollPosition.y % spacing);

        for (int i = 0; i < cols; i++)
        {
            float x = offsetX + i * spacing;
            Handles.DrawLine(new Vector3(x, 0), new Vector3(x, canvas.height));
        }
        for (int j = 0; j < rows; j++)
        {
            float y = offsetY + j * spacing;
            Handles.DrawLine(new Vector3(0, y), new Vector3(canvas.width, y));
        }

        Handles.color = Color.white;
    }

    #endregion

    #region Connection Drawing

    private void DrawConnections()
    {
        if (targetTree?.tree == null) return;

        for (int i = 0; i < targetTree.tree.Count; i++)
        {
            TechNode node = targetTree.tree[i];
            if (node == null || node.tech == null || node.requirements == null) continue;

            Vector2 nodePos = node.UIposition - scrollPosition;

            foreach (Tech req in node.requirements)
            {
                int reqIdx = targetTree.FindTechIndex(req);
                if (reqIdx == -1) continue;

                Vector2 reqPos = targetTree.tree[reqIdx].UIposition - scrollPosition;

                Vector2 start = nodePos + outgoingEdgVec;
                Vector2 end = reqPos + incomingEdgVec;

                // Bezier curve
                Handles.DrawBezier(start, end,
                    start + Vector2.left * 100,
                    end + Vector2.right * 100,
                    Color.white, null, 3f);

                // Arrow head
                Handles.DrawLine(start, start + upArrowVec);
                Handles.DrawLine(start, start + downArrowVec);
            }
        }
    }

    private void DrawLiveConnectionLine(Event e)
    {
        if (selectedNode != null && selectedNode.tech != null && e.button == 1)
        {
            Vector2 start = e.mousePosition;
            Vector2 end = selectedNode.UIposition - scrollPosition + incomingEdgVec;

            Handles.DrawBezier(start, end,
                start + Vector2.left * 100,
                end + Vector2.right * 100,
                new Color(0.5f, 1f, 0.5f, 0.8f), null, 2f);

            Repaint();
        }
    }

    #endregion

    #region Node Drawing & Events

    private void DrawNodesAndProcessEvents(Event e, EventType uiEvent)
    {
        if (targetTree?.tree == null) return;

        GUIStyle nodeStyle = new GUIStyle(EditorStyles.helpBox);
        GUIStyle selectedNodeStyle = new GUIStyle(EditorStyles.helpBox);
        selectedNodeStyle.normal.background = Texture2D.linearGrayTexture;

        for (int i = 0; i < targetTree.tree.Count; i++)
        {
            TechNode node = targetTree.tree[i];
            if (node == null || node.tech == null) continue;

            Vector2 nodePos = node.UIposition - scrollPosition;
            Rect nodeRect = new Rect(nodePos, nodeSize);

            // === Draw node background ===
            bool isSelected = (selectedNode == node);
            GUI.Box(nodeRect, "", isSelected ? selectedNodeStyle : nodeStyle);

            // === Draw icon ===
            if (node.tech.icon != null)
            {
                Rect iconRect = new Rect(nodePos.x - 50f, nodePos.y, 50f, 50f);
                GUI.DrawTexture(iconRect, node.tech.icon.texture, ScaleMode.ScaleToFit);
            }

            // === Draw header ===
            Rect headerRect = new Rect(nodePos.x, nodePos.y, nodeSize.x, 20f);
            EditorGUI.LabelField(headerRect, node.tech.name, EditorStyles.boldLabel);

            // === Draw Cost field ===
            EditorGUI.LabelField(new Rect(nodePos + nextLineVec, nodeLabelSize), "Research cost:");
            int newCost = EditorGUI.IntField(new Rect(nodePos + nextLineVec + indentVec, nodeContentSize), node.researchCost);
            if (newCost != node.researchCost)
            {
                Undo.RecordObject(targetTree, "Edit Tech Node Cost");
                node.researchCost = newCost;
                EditorUtility.SetDirty(targetTree);
            }

            // === Draw Level field ===
            EditorGUI.LabelField(new Rect(nodePos + nextLineVec * 2, nodeLabelSize), "Level:");
            int newLevel = EditorGUI.IntField(new Rect(nodePos + nextLineVec * 2 + indentVec, nodeContentSize), node.level);
            if (newLevel != node.level)
            {
                Undo.RecordObject(targetTree, "Edit Tech Node Level");
                node.level = newLevel;
                EditorUtility.SetDirty(targetTree);
            }

            // === Mouse events on node ===
            if (nodeRect.Contains(e.mousePosition))
            {
                if (uiEvent == EventType.MouseDown)
                {
                    // Left-click: bắt đầu kéo node
                    if (e.button == 0)
                    {
                        activeNode = node;
                        mouseSelectionOffset = node.UIposition - (e.mousePosition + scrollPosition);
                        GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                        e.Use();
                    }
                    // Right-click: chọn node để tạo kết nối
                    else if (e.button == 1)
                    {
                        selectedNode = node;
                        e.Use();
                        Repaint();
                    }
                }
                else if (uiEvent == EventType.MouseUp)
                {
                    // Right-click release trên node khác → tạo/xóa kết nối
                    if (e.button == 1 && selectedNode != null && selectedNode != node)
                    {
                        Undo.RecordObject(targetTree, "Modify TechTree Connection");

                        if (node.requirements.Contains(selectedNode.tech))
                        {
                            // Xóa kết nối đã tồn tại
                            node.requirements.Remove(selectedNode.tech);
                        }
                        else if (selectedNode.requirements.Contains(node.tech))
                        {
                            // Xóa kết nối ngược
                            selectedNode.requirements.Remove(node.tech);
                        }
                        else if (targetTree.IsConnectible(targetTree.tree.IndexOf(selectedNode), i))
                        {
                            // Tạo kết nối mới
                            node.requirements.Add(selectedNode.tech);

                            // Sửa cascade requirements
                            for (int k = 0; k < targetTree.tree.Count; k++)
                                targetTree.CorrectRequirementsCascades(k);
                        }

                        EditorUtility.SetDirty(targetTree);
                        selectedNode = null;
                        e.Use();
                    }
                    // Left-click release: kết thúc kéo
                    else if (e.button == 0)
                    {
                        activeNode = null;
                        e.Use();
                    }
                }
            }
        }

        // === Kéo node (drag) — xử lý ngoài vòng lặp node ===
        if (activeNode != null && uiEvent == EventType.MouseDrag && e.button == 0)
        {
            Undo.RecordObject(targetTree, "Move Tech Node");
            activeNode.UIposition = e.mousePosition + scrollPosition + mouseSelectionOffset;
            EditorUtility.SetDirty(targetTree);
            e.Use();
            Repaint();
        }

        // === Mouse up ngoài node → release drag ===
        if (uiEvent == EventType.MouseUp)
        {
            if (e.button == 0)
            {
                activeNode = null;
            }
            if (e.button == 1)
            {
                selectedNode = null;
            }
        }
    }

    #endregion

    #region Canvas Events (Pan, Drag-Drop, Context Menu)

    private void ProcessCanvasEvents(Event e, EventType uiEvent, Rect canvas)
    {
        // === Middle-click pan ===
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
                e.Use();
            }
        }

        // === Drag & Drop Tech SO vào canvas để thêm node ===
        if (e.type == EventType.DragUpdated)
        {
            bool hasTech = false;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is Tech) { hasTech = true; break; }
            }
            if (hasTech)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                e.Use();
            }
        }
        else if (e.type == EventType.DragPerform)
        {
            bool added = false;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is Tech tech)
                {
                    Undo.RecordObject(targetTree, "Add Tech Node");
                    targetTree.AddNode(tech, 1, e.mousePosition + scrollPosition);
                    added = true;
                }
            }
            if (added)
            {
                EditorUtility.SetDirty(targetTree);
            }
            DragAndDrop.AcceptDrag();
            e.Use();
        }

        // === Right-click context menu trên canvas trống ===
        if (e.type == EventType.ContextClick)
        {
            // Kiểm tra không nằm trên node nào
            bool overNode = false;
            if (targetTree?.tree != null)
            {
                foreach (var node in targetTree.tree)
                {
                    if (node == null || node.tech == null) continue;
                    Rect nodeRect = new Rect(node.UIposition - scrollPosition, nodeSize);
                    if (nodeRect.Contains(e.mousePosition)) { overNode = true; break; }
                }
            }

            if (!overNode)
            {
                ShowCanvasContextMenu(e.mousePosition);
                e.Use();
            }
        }
    }

    private void ShowCanvasContextMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Select All"), false, () =>
        {
            // Highlight tất cả (cho tương lai)
            Repaint();
        });
        menu.AddSeparator("");

        // Delete selected node
        if (selectedNode != null)
        {
            string label = $"Delete '{selectedNode.tech?.name ?? "NULL"}'";
            menu.AddItem(new GUIContent(label), false, () =>
            {
                Undo.RecordObject(targetTree, "Delete Tech Node");
                targetTree.DeleteNode(selectedNode.tech);
                if (activeNode == selectedNode) activeNode = null;
                selectedNode = null;
                EditorUtility.SetDirty(targetTree);
            });
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Delete Node (right-click a node first)"));
        }

        menu.ShowAsContext();
    }

    #endregion

    #region Bottom Toolbar

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();

        // Selected node info
        if (selectedNode != null && selectedNode.tech != null)
        {
            EditorGUILayout.LabelField($"Selected: {selectedNode.tech.name}", EditorStyles.boldLabel);
            if (GUILayout.Button("Delete Selected", GUILayout.Width(120)))
            {
                Undo.RecordObject(targetTree, "Delete Tech");
                targetTree.DeleteNode(selectedNode.tech);
                if (activeNode == selectedNode) activeNode = null;
                selectedNode = null;
                EditorUtility.SetDirty(targetTree);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Right-click node để chọn, right-click node khác để liên kết");
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Export JSON")) ExportToJson();
        if (GUILayout.Button("Import JSON")) ImportFromJson();
        if (GUILayout.Button("Undo")) Undo.PerformUndo();
        if (GUILayout.Button("Redo")) Undo.PerformRedo();
        if (GUILayout.Button("Clear Empty"))
        {
            Undo.RecordObject(targetTree, "Clear Empty Nodes");
            targetTree.tree.RemoveAll(n => n == null || n.tech == null);
            EditorUtility.SetDirty(targetTree);
        }
        EditorGUILayout.EndHorizontal();
    }

    #endregion

    #region JSON Import/Export

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
                    foreach (Tech req in node.requirements)
                    {
                        string g = req != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(req)) : "";
                        n.requirementsGUIDs.Add(g);
                    }
                }
                dto.nodes.Add(n);
            }
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

        Undo.RecordObject(targetTree, "Import TechTree JSON");
        if (targetTree.tree != null) targetTree.tree.Clear();
        else targetTree.tree = new List<TechNode>();

        // Create nodes
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

        // Assign requirements
        for (int i = 0; i < dto.nodes.Count; i++)
        {
            NodeDTO n = dto.nodes[i];
            TechNode node = targetTree.tree[i];
            node.requirements = new List<Tech>();
            if (n.requirementsGUIDs != null)
            {
                foreach (string rg in n.requirementsGUIDs)
                {
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
    }

    #endregion
}

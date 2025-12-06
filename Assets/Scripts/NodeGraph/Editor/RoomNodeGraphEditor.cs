using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.MPE;
using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;

public class RoomNodeGraphEditor : EditorWindow
{

    private GUIStyle roomNodeStyle;
    private GUIStyle selectedRoomNodeStyle;
    
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentSelectedRoomNode;
    private RoomNodeTypeListSO roomNodeTypeList;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    
    private const float connectingLineWidth = 3f;
    private const float connectingArrowSize = 6f;
    
    // Draw the Grid and Background
    Vector2 graphOffset;
    Vector2 dragOffset;
    
    private const float gridSmallSpacing = 25f;
    private const float gridLargeSpacing = 100f;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        Selection.selectionChanged += InspectorSelectionChanged;
        
        // Define the GUI style for room nodes
        roomNodeStyle = new GUIStyle();

        roomNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);

        selectedRoomNodeStyle = new GUIStyle();
        selectedRoomNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedRoomNodeStyle.normal.textColor = Color.white;
        selectedRoomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
        selectedRoomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        
        // Load the RoomNodeTypeListSO from GameResources
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Opens the Room Node Graph Editor window when a RoomNodeGraphSO asset is double-clicked in the Unity Editor.
    /// </summary>
    /// <param name="instanceID"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    [OnOpenAsset(0)] // Priority 0 to ensure it runs first
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraphSO = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraphSO != null)
        {
            OpenWindow();
            currentRoomNodeGraph = roomNodeGraphSO;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Handles the rendering and processing of GUI events for the current frame.
    /// </summary>
    /// <remarks>This method is called automatically by the Unity engine during the GUI rendering phase. Use
    /// this method to implement custom GUI controls or handle user input through the GUI.</remarks>
    private void OnGUI()
    {
        if (currentRoomNodeGraph != null)
        {
            DrawBackgroundGrid(gridSmallSpacing, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLargeSpacing, 0.4f, Color.gray);
            
            DrawDraggingLine();
            
            ProcessEvent(Event.current);
            
            DrawRoomConnections();

            DrawRoomNodes();
        }
        if (GUI.changed)
        {
            Repaint();
        }
    }
    
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += dragOffset * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }

        Handles.color = Color.white;

    }


    private void DrawDraggingLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(
                currentRoomNodeGraph.roomNodeToDrawConnectionLineFrom.roomNodeRect.center,
                currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawConnectionLineFrom.roomNodeRect.center ,
                currentRoomNodeGraph.linePosition,
                Color.white,
                null,
                width: 3f);
        }
    }

    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(selectedRoomNodeStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }
        GUI.changed = true;
    }

    private void ProcessEvent(Event current)
    {
        dragOffset = Vector2.zero;
        
        if (currentSelectedRoomNode == null || !currentSelectedRoomNode.isLeftClickDragging)
        {
            currentSelectedRoomNode = IsMouseOverRoomNode(current);
        }
        if (currentSelectedRoomNode == null || currentRoomNodeGraph.roomNodeToDrawConnectionLineFrom != null)
        {
            // Process events for the Room Node Graph
            ProcessRoomNodeGraphEvents(current);
        }
        else
        {
            currentSelectedRoomNode.ProcessEvents(current);
        }

    }

    private RoomNodeSO IsMouseOverRoomNode(Event current)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].roomNodeRect.Contains(current.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    private void ProcessRoomNodeGraphEvents(Event current)
    {
        switch (current.type)
        {
            case EventType.MouseDown:
                ProcessEventMouseDown(current);
                break;
            case EventType.MouseDrag:
                ProcessEventMouseDrag(current);
                break;
            case EventType.MouseUp:
                ProcessEventMouseUp(current);
                break;
            default:
                break;
        }
    }
    private void ProcessEventMouseDrag(Event current)
    {
        if (current.button == 1)
        {
            ProcessRightMouseDragEvent(current);
        }
        else if (current.button == 0)
        {
            ProcessLeftMouseDragEvent(current);
        }
    }
    private void  ProcessRightMouseDragEvent(Event current)
    {
        DrawConnectingLine(current.delta);
        GUI.changed = true;
    }
    
    private void ProcessLeftMouseDragEvent(Event current)
    {
        dragOffset = current.delta;
        graphOffset = dragOffset;

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.DragNode(dragOffset);
        }
        
        GUI.changed = true;
    }
    
    private void ProcessEventMouseUp(Event current)
    {
        if (current.button == 1 && currentRoomNodeGraph.roomNodeToDrawConnectionLineFrom != null)
        {
            ProcessRightMouseUpEvent(current);
        }
    }
    
    private void  ProcessRightMouseUpEvent(Event current)
    {
        RoomNodeSO roomNodeConnect = IsMouseOverRoomNode(current);
        if (currentRoomNodeGraph.roomNodeToDrawConnectionLineFrom.AddChildRoomNodeID(roomNodeConnect.roomNodeID))
        {
            roomNodeConnect.AddParentRoomNodeID(currentRoomNodeGraph.roomNodeToDrawConnectionLineFrom.roomNodeID);
        }
        ClearLineDrag();
    }
    
    private void DrawRoomConnections()
    {
        foreach (RoomNodeSO roomNodeSo in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNodeSo.childRoomNodeID.Count > 0)
            {
                foreach (string roomChildID in roomNodeSo.childRoomNodeID)
                {
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(roomChildID))
                    {
                        DrawConnectionLine(roomNodeSo, currentRoomNodeGraph.roomNodeDictionary[roomChildID]);
                        
                        GUI.changed = true;
                    }
                }
            }
        }
    }
    
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPosition = parentRoomNode.roomNodeRect.center;
        Vector2 endPosition = childRoomNode.roomNodeRect.center;
        
        Vector2 midPosition = (startPosition + endPosition) / 2;
        Vector2 direction = (endPosition - startPosition).normalized;

        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x) * connectingArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x) * connectingArrowSize;
        Vector2 arrowHeadPoint = midPosition + direction * connectingArrowSize;
        
        // Draw arrowhead on the connection line
        Handles.DrawAAConvexPolygon(arrowTailPoint1, arrowTailPoint2, arrowHeadPoint);
        
        // Draw a line between the two nodes
        Handles.DrawBezier(
            startPosition,
            endPosition,
            startPosition,
            endPosition,
            Color.white,
            null,
            connectingLineWidth);
    }
    
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodesToDelete = new Queue<RoomNodeSO>();
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                Debug.Log("Deleting Room Node: " + roomNode.roomNodeID);
                roomNodesToDelete.Enqueue(roomNode);
                foreach (string childRoomNodeID in roomNode.childRoomNodeID)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNodeFromID(childRoomNodeID);
                    if (childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRoomNodeID(roomNode.roomNodeID);
                    }
                }
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeID)
                {
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNodeFromID(parentRoomNodeID);
                    if (parentRoomNode != null)
                    {
                        parentRoomNode.RemoveChildRoomNodeID(roomNode.roomNodeID);
                    }
                }
            }
        }

        while (roomNodesToDelete.Count > 0)
        {
            RoomNodeSO roomNodeToDelete = roomNodesToDelete.Dequeue();
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.roomNodeID);
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);
            
            DestroyImmediate(roomNodeToDelete, true);
            AssetDatabase.SaveAssets();
        }
    }
    
    private void  DrawConnectingLine(Vector2 currentDelta)
    {
        if (currentRoomNodeGraph.roomNodeToDrawConnectionLineFrom != null)
        {
            currentRoomNodeGraph.linePosition += currentDelta;
        }
    }
    
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawConnectionLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }
    
    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }
    
    private void DeleteLinkBetweenSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeID.Count > 0)
            {
                for(int i = roomNode.childRoomNodeID.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNodeFromID(roomNode.childRoomNodeID[i]);
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        childRoomNode.RemoveParentRoomNodeID(roomNode.roomNodeID);
                        roomNode.RemoveChildRoomNodeID(childRoomNode.roomNodeID);
                    }
                }
            }
        }
        
        ClearAllSelectedRoomNodes();
    }

    private void ProcessEventMouseDown(Event current)
    {
        if (current.button == 1) // Right mouse button
        {
            ShowContextMenu(current.mousePosition);
        }
        else if (current.button == 0) // Left mouse button
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);
        menu.AddItem(new GUIContent("Delete Link Between Selected Room Nodes"), false, DeleteLinkBetweenSelectedRoomNodes);
        
        
        menu.ShowAsContext();
    }

    private void CreateRoomNode(object mousePositonObject)
    {
        if(currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }
        CreateRoomNode(mousePositonObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    private void CreateRoomNode(object mousePositonObject, RoomNodeTypeSO roomNodeTypeSO)
    {
        Vector2 mousePostion = (Vector2)mousePositonObject;

        RoomNodeSO roomNodeSO = ScriptableObject.CreateInstance<RoomNodeSO>();
        currentRoomNodeGraph.roomNodeList.Add(roomNodeSO);
        roomNodeSO.Initialise(new Rect(mousePostion, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeTypeSO);

        // Add the new RoomNodeSO as a sub-asset of the current RoomNodeGraphSO
        AssetDatabase.AddObjectToAsset(roomNodeSO, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();
        
        currentRoomNodeGraph.OnValidate();
    }
    
    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }
    
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraphSO = Selection.activeObject as RoomNodeGraphSO;
        if (roomNodeGraphSO != null)
        {
            currentRoomNodeGraph = roomNodeGraphSO;
            GUI.changed = true;
        }
    }
}

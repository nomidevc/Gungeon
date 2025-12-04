using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.MPE;
using System;

public class RoomNodeGraphEditor : EditorWindow
{

    private GUIStyle roomNodeStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentSelectedRoomNode;
    private RoomNodeTypeListSO roomNodeTypeList;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    
    private const float connectingLineWidth = 3f;
    private const float connectingArrowSize = 6f;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // Define the GUI style for room nodes
        roomNodeStyle = new GUIStyle();

        roomNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);

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
            roomNode.Draw(roomNodeStyle);
        }
        GUI.changed = true;
    }

    private void ProcessEvent(Event current)
    {
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
    }
    private void  ProcessRightMouseDragEvent(Event current)
    {
        DrawConnectingLine(current.delta);
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

    private void ProcessEventMouseDown(Event current)
    {
        if (current.button == 1) // Right mouse button
        {
            ShowContextMenu(current.mousePosition);
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add Room Node"), false, CreateRoomNode, mousePosition);

        menu.ShowAsContext();
    }

    private void CreateRoomNode(object mousePositonObject)
    {
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
}

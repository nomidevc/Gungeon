using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.MPE;
using System;

public class RoomNodeGraphEditor : EditorWindow
{

    private GUIStyle roomNodeStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeTypeListSO roomNodeTypeList;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

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
        if(currentRoomNodeGraph != null)
        {
            ProcessEvent(Event.current);

            DrawRoomNodes();
        }
        if(GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(roomNodeStyle);
        }
        GUI.changed = true;
    }

    private void ProcessEvent(Event current)
    {
        ProcessRoomNodeGraphEvents(current);
    }

    private void ProcessRoomNodeGraphEvents(Event current)
    {
        switch(current.type)
        {
            case EventType.MouseDown:
                ProcessEventMouseDown(current);
                break;

            default:
                break;
        }
    }

    private void ProcessEventMouseDown(Event current)
    {
        if(current.button == 1) // Right mouse button
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
        Vector2 mousePostion = (Vector2)mousePositonObject ;

        RoomNodeSO roomNodeSO = ScriptableObject.CreateInstance<RoomNodeSO>();
        currentRoomNodeGraph.roomNodeList.Add(roomNodeSO);
        roomNodeSO.Initialise(new Rect(mousePostion, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeTypeSO);

        // Add the new RoomNodeSO as a sub-asset of the current RoomNodeGraphSO
        AssetDatabase.AddObjectToAsset(roomNodeSO, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();
    }
}

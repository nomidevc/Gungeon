using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string roomNodeID;
    [HideInInspector] public List<string> parentRoomNodeID = new List<string>();
    [HideInInspector] public List<string> childRoomNodeID = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region EDITOR_CODE

#if UNITY_EDITOR
    [HideInInspector] public Rect roomNodeRect;

    /// <summary>
    /// Initialise the Room Node Scriptable Object
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="roomNodeGraphSO"></param>
    /// <param name="roomNodeTypeSO"></param>
    public void Initialise(Rect rect, RoomNodeGraphSO roomNodeGraphSO, RoomNodeTypeSO roomNodeTypeSO)
    {
        name = "Room Node";
        roomNodeID = System.Guid.NewGuid().ToString();
        roomNodeRect = rect;
        roomNodeGraph = roomNodeGraphSO;
        roomNodeType = roomNodeTypeSO;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Draw the Room Node GUI
    /// </summary>
    /// <param name="roomNodeStyle"></param>
    public void Draw(GUIStyle roomNodeStyle)
    {
        GUILayout.BeginArea(roomNodeRect, roomNodeStyle);

        EditorGUI.BeginChangeCheck();

        // find the index of the current roomNodeType in the roomNodeTypeList
        int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());
        roomNodeType = roomNodeTypeList.list[selection];

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    private string[] GetRoomNodeTypeToDisplay()
    {
        string[] roomNameArrayToDisplay = new string[roomNodeTypeList.list.Count];
        for(int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if(roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomNameArrayToDisplay[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomNameArrayToDisplay;
    }

#endif

    #endregion EDITOR_CODE
}

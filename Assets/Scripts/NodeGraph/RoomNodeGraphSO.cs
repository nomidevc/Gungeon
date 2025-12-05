using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]

public class RoomNodeGraphSO : EditorWindow
{
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeListSO;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    void Awake()
    {
        LoadRoomNodeDictionary();
    }
    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();
        foreach (RoomNodeSO roomNodeSo in roomNodeList)
        {
            roomNodeDictionary[roomNodeSo.roomNodeID] = roomNodeSo;
        }
    }
    
    public RoomNodeSO GetRoomNodeFromID(string roomNodeID)
    {
        if (roomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }

    #region Editor Code

    #if UNITY_EDITOR
    [HideInInspector] public RoomNodeSO roomNodeToDrawConnectionLineFrom = null;
    [HideInInspector] public Vector2 linePosition;

    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO roomNodeSO, Vector2 position)
    {
        roomNodeToDrawConnectionLineFrom = roomNodeSO;
        linePosition = position;
    }
    
    #endif

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]

public class RoomNodeGraphSO : EditorWindow
{

    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeListSO;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();
}

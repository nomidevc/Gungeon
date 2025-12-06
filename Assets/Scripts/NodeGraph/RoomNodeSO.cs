using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.MPE;
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
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

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

        if (parentRoomNodeID.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // find the index of the current roomNodeType in the roomNodeTypeList
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());
            roomNodeType = roomNodeTypeList.list[selection];
            
            
            // If the room type selection has changed making child connections potentially invalid
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor 
                || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor 
                || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                // If a room node type has been changed and it already has children then delete the parent child links since we need to revalidate any
                if (childRoomNodeID.Count > 0)
                {
                    for (int i = childRoomNodeID.Count - 1; i >= 0; i--)
                    {
                        // Get child room node
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNodeFromID(childRoomNodeID[i]);

                        // If the child room node is not null
                        if (childRoomNode != null)
                        {
                            // Remove childID from parent room node
                            RemoveChildRoomNodeID(childRoomNode.roomNodeID);

                            // Remove parentID from child room node
                            childRoomNode.RemoveParentRoomNodeID(roomNodeID);
                        }
                    }
                }
            }
            
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    private string[] GetRoomNodeTypeToDisplay()
    {
        string[] roomNameArrayToDisplay = new string[roomNodeTypeList.list.Count];
        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomNameArrayToDisplay[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomNameArrayToDisplay;
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        roomNodeRect.position += delta;
        EditorUtility.SetDirty(this);
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseUpEvent();
        }
    }

    private void ProcessLeftMouseUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        Selection.activeObject = this;
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDownEvent();
        }
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDownEvent(currentEvent);
        }
    }
    private void ProcessRightMouseDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessLeftMouseDownEvent()
    {
        isSelected = !isSelected;
    }
    
    public bool AddChildRoomNodeID(string childRoomNodeIDToAdd)
    {
        if (IsChildRoomValid(childRoomNodeIDToAdd))
        {
            childRoomNodeID.Add(childRoomNodeIDToAdd);
            return true;
        }
        return false;
    }
    
    public bool RemoveChildRoomNodeID(string childRoomNodeIDToRemove)
    {
        if (childRoomNodeID.Contains(childRoomNodeIDToRemove))
        {
            childRoomNodeID.Remove(childRoomNodeIDToRemove);
            return true;
        }
        return false;
    }
    
    public bool AddParentRoomNodeID(string parentRoomNodeIDToAdd)
    {
        parentRoomNodeID.Add(parentRoomNodeIDToAdd);
        return true;
    }
    
    public bool RemoveParentRoomNodeID(string parentRoomNodeIDToRemove)
    {
        if (parentRoomNodeID.Contains(parentRoomNodeIDToRemove))
        {
            parentRoomNodeID.Remove(parentRoomNodeIDToRemove);
            return true;
        }
        return false;
    }

    private bool IsChildRoomValid(string childNodeID)
    {
        // Each graph can only have one connected Boss Room
        bool isConnectedBossAlready = false;
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeID.Count > 0)
                isConnectedBossAlready = true;
        }
        
        if (roomNodeGraph.GetRoomNodeFromID(childNodeID).roomNodeType.isBossRoom && isConnectedBossAlready)
            return false;
        
        if (roomNodeGraph.GetRoomNodeFromID(childNodeID).roomNodeType.isNone)
            return false;

        if (roomNodeGraph.GetRoomNodeFromID(childNodeID).roomNodeType.isEntrance)
            return false;

        if (this.roomNodeID == childNodeID)
            return false;
        
        if (parentRoomNodeID.Contains(childNodeID))
            return false;
        
        if (roomNodeGraph.GetRoomNodeFromID(childNodeID).parentRoomNodeID.Count > 0)
            return false;
        
        if (roomNodeType.isCorridor && roomNodeGraph.GetRoomNodeFromID(childNodeID).roomNodeType.isCorridor)
            return false;
        
        if (!roomNodeType.isCorridor && !roomNodeGraph.GetRoomNodeFromID(childNodeID).roomNodeType.isCorridor)
            return false;
        
        if (childRoomNodeID.Contains(childNodeID))
            return false;
        
        if (roomNodeGraph.GetRoomNodeFromID(childNodeID).roomNodeType.isCorridor && this.childRoomNodeID.Count >= Settings.MAX_CHILD_CORRIDOR)
            return false;
        
        if (!roomNodeGraph.GetRoomNodeFromID(childNodeID).roomNodeType.isCorridor && this.childRoomNodeID.Count > 0)
            return false;

        return true;
    }

#endif

    #endregion EDITOR_CODE
}

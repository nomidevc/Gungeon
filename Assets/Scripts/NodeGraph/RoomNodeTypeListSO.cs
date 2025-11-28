using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeListSO", menuName = "Scriptable Objects/Dungeon/Room Node Type List")]

public class RoomNodeTypeListSO : ScriptableObject
{
    #region Header ROOM NODE TYPE LIST
    [Header("ROOM NODE TYPE LIST")]
    #endregion Header ROOM NODE TYPE LIST
    #region Tooltip
    [Tooltip("This list should be populated with all the RoomNodeTypeSO in the project - it's used instead of an enum")]
    [Header("List of All Room Node Types")]
    #endregion Tooltip
    public List<RoomNodeTypeSO> list;

    #region Validation
#if UNITY_EDITOR
    /// <summary>
    /// Validate Scriptable Object data.
    /// </summary>
    public void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
#endif
    #endregion Validation
}

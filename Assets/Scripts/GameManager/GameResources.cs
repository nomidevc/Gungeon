using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;
    public static GameResources Instance
    {
        get
        {
            if(instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }

    #region Health Dungeon
    [Space(10)]
    [Header("Dungeon")]
    #endregion
    #region Tooltip
    [Tooltip("Populated with the dungeon RoomNodeTypeListSO")]
    #endregion
    public RoomNodeTypeListSO roomNodeTypeList;
}

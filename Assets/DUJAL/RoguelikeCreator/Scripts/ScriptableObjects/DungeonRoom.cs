using DUJAL.DungeonCreator.Types;
using DUJAL.Systems.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DirectionFlag
{
    public bool West;
    public bool East;
    public bool North;
    public bool South;

    public DirectionFlag(List<Direction> directions)
    {
        West = directions.Contains(Direction.West);
        East = directions.Contains(Direction.East);
        North = directions.Contains(Direction.North);
        South = directions.Contains(Direction.South);
    }
}

/// <summary>
//  Scriptable Object that contains a reference to a room instance and it's data.
/// </summary>
[CreateAssetMenu(fileName = "New Dungeon Room Asset", menuName = "DUJAL/DungeonRoom")]
public class DungeonRoom : ScriptableObject
{
    public string Name;
    public GameObject RoomPrefab;
    public DirectionFlag Directions;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
    north,
    east,
    south,
    west,
    up,
    down
}

public static class DirUtil
{
    public static Vector2Int DirToV2(Direction input)
    {
        return input switch {
            Direction.north => new Vector2Int(0, 1),
            Direction.east => new Vector2Int(1, 0),
            Direction.south => new Vector2Int(0, -1),
            Direction.west => new Vector2Int(-1, 0),
            _ => Vector2Int.zero, //should never be used
        };
    }

    public static Vector3Int DirToV3(Direction input)
    {
        return input switch {
            Direction.north => new Vector3Int(0, 0, 1),
            Direction.east => new Vector3Int(1, 0, 0),
            Direction.south => new Vector3Int(0, 0, -1),
            Direction.west => new Vector3Int(-1, 0, 0),
            Direction.up => new Vector3Int(0, 1, 0),
            Direction.down => new Vector3Int(0, -1, 0),
            _ => Vector3Int.zero, //should never be used
        };
    }

    public static Direction GetOpposite(Direction input) { return GetOpposite((int)input); }
    public static Direction GetOpposite(int input) 
    { 
        if (input < 4) { //dir is on 2D plane
            return (Direction)((input + 2) % 4);
        }
        else { //top or bottom
            return (Direction)(4 + ((input - 3) % 2));
        }
    }
}
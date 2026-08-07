using UnityEngine;

public enum SpawnSide
{
    Left,
    Right,
    Top
}

public class Room
{
    public Vector2Int GridPos;

    public bool LeftWall = true;
    public bool RightWall = true;
    public bool TopWall = true;
    public bool BottomWall = true;

    public SpawnSide SpawnSide;

    public Room(Vector2Int gridPos)
    {
        GridPos = gridPos;
    }
}
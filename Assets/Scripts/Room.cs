using System;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public RoomType Type;
    public Vector2Int GridPos;
    public int StartWave;

    public Room(Vector2Int gridPos, int wave)
    {
        GridPos = gridPos;
        StartWave = wave;
        Type = DecideRoomType(gridPos, wave / 3);
    }

    RoomType DecideRoomType(Vector2Int pos, int bottomRow)
    {
        RoomType res = RoomType.Center;

        if(pos.x < 0)
            res = RoomType.TopLeft;
        else if(pos.x > 0)
            res = RoomType.TopRight;
        
        if(pos.y > bottomRow)
            res |= RoomType.TopCenter;

        return res;
    }
}

public enum RoomType
{
    Center = 0,
    Left = 1,
    Right = 2,
    TopCenter = 4,
    TopLeft = 5,
    TopRight = 6
}

[Serializable]
public class SpawnPointData
{
    public Vector2 spawnPosition;
    public Vector2 targetPosition;
}

[Serializable]
public class RoomTypeData
{
    public RoomType roomType;
    public List<SpawnPointData> spawnPoints = new();
}
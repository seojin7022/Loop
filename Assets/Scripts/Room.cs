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

        Type = DecideRoomType(
            gridPos,
            wave / 3
        );
    }

    public void UpdateType(int bottomRow)
    {
        Type = DecideRoomType(
            GridPos,
            bottomRow
        );
    }

    private RoomType DecideRoomType(Vector2Int pos, int bottomRow)
    {
        if (pos.y > bottomRow)
        {
            if (pos.x < 0)
                return RoomType.TopLeft;

            if (pos.x > 0)
                return RoomType.TopRight;

            return RoomType.TopCenter;
        }

        if (pos.x < 0)
            return RoomType.Left;

        if (pos.x > 0)
            return RoomType.Right;

        return RoomType.Center;
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
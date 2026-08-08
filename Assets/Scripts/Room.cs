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

        // 실제 타입은 RoomManager 가 현재 BottomRow 기준으로 곧바로 다시 계산한다.
        Type = DecideRoomType(gridPos, gridPos.y);
    }

    /// <summary>
    /// 방의 가로 위치(좌·중앙·우)와, 아래 줄인지 위 줄인지로 타입을 정한다.
    /// RoomType 의 TopCenter(4) 는 '위 줄' 플래그로 쓰인다.
    /// Left(1) | TopCenter(4) = TopLeft(5), Right(2) | TopCenter(4) = TopRight(6).
    /// </summary>
    public static RoomType DecideRoomType(Vector2Int pos, int bottomRow)
    {
        RoomType res = RoomType.Center;

        if (pos.x < 0)
            res = RoomType.Left;
        else if (pos.x > 0)
            res = RoomType.Right;

        if (pos.y > bottomRow)
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
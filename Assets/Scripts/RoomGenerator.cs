using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Room Size")]
    public int roomWidth = 15;
    public int roomHeight = 11;

    Dictionary<Vector2Int, Room> rooms = new();

    public IReadOnlyDictionary<Vector2Int, Room> Rooms => rooms;

    public List<Vector3> GetSpawnPositions(Room room)
    {
        List<Vector3> result = new();

        Vector3Int origin = RoomOrigin(room.GridPos);

        switch (room.SpawnSide)
        {
            case SpawnSide.Left:
            {
                int y1 = roomHeight / 3;
                int y2 = roomHeight * 2 / 3;

                result.Add(wallTilemap.GetCellCenterWorld(
                    origin + new Vector3Int(0, y1, 0)));

                result.Add(wallTilemap.GetCellCenterWorld(
                    origin + new Vector3Int(0, y2, 0)));

                break;
            }

            case SpawnSide.Right:
            {
                int y1 = roomHeight / 3;
                int y2 = roomHeight * 2 / 3;

                result.Add(wallTilemap.GetCellCenterWorld(
                    origin + new Vector3Int(roomWidth - 1, y1, 0)));

                result.Add(wallTilemap.GetCellCenterWorld(
                    origin + new Vector3Int(roomWidth - 1, y2, 0)));

                break;
            }

            default: // Top
            {
                int x1 = roomWidth / 3;
                int x2 = roomWidth * 2 / 3;

                result.Add(wallTilemap.GetCellCenterWorld(
                    origin + new Vector3Int(x1, roomHeight - 1, 0)));

                result.Add(wallTilemap.GetCellCenterWorld(
                    origin + new Vector3Int(x2, roomHeight - 1, 0)));

                break;
            }
        }

        return result;
    }

    public Room CreateRoom(Vector2Int gridPos)
    {
        if (rooms.ContainsKey(gridPos))
            return rooms[gridPos];

        Room room = new Room(gridPos);

        room.SpawnSide = DecideSpawnSide(gridPos);

        rooms.Add(gridPos, room);

        DrawAllRooms();

        return room;
    }

    public void DeleteRow(int row)
    {
        rooms.Remove(new Vector2Int(-1, row));
        rooms.Remove(new Vector2Int(0, row));
        rooms.Remove(new Vector2Int(1, row));

        DrawAllRooms();
    }

    SpawnSide DecideSpawnSide(Vector2Int pos)
    {
        if (pos.x < 0)
            return SpawnSide.Left;

        if (pos.x > 0)
            return SpawnSide.Right;

        return SpawnSide.Top;
    }

    void DrawAllRooms()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        foreach(Room room in rooms.Values)
            DrawRoom(room);
    }

    void DrawRoom(Room room)
    {
        Vector3Int origin = RoomOrigin(room.GridPos);

        for(int x = 0; x < roomWidth; x++)
        {
            for(int y = 0; y < roomHeight; y++)
            {
                floorTilemap.SetTile(origin + new Vector3Int(x,y,0), floorTile);
            }
        }

        bool hasLeft = rooms.ContainsKey(room.GridPos + Vector2Int.left);
        bool hasRight = rooms.ContainsKey(room.GridPos + Vector2Int.right);

        bool hasUp = rooms.ContainsKey(room.GridPos + Vector2Int.up);

        if (room.GridPos.x != 0)
        {
            hasUp &= rooms.ContainsKey(new Vector2Int(0, room.GridPos.y + 1));
        }

        bool hasDown = rooms.ContainsKey(room.GridPos + Vector2Int.down);

        if (room.GridPos.x != 0)
        {
            hasDown &= rooms.ContainsKey(new Vector2Int(0, room.GridPos.y - 1));
        }

        if(!hasUp)
        {
            for(int x=0;x<roomWidth;x++)
            {
                wallTilemap.SetTile(
                    origin + new Vector3Int(x,roomHeight-1,0),
                    wallTile);
            }
        }

        if(!hasDown)
        {
            for(int x=0;x<roomWidth;x++)
            {
                wallTilemap.SetTile(
                    origin + new Vector3Int(x,0,0),
                    wallTile);
            }
        }

        if(!hasLeft)
        {
            for(int y=0;y<roomHeight;y++)
            {
                wallTilemap.SetTile(
                    origin + new Vector3Int(0,y,0),
                    wallTile);
            }
        }

        if(!hasRight)
        {
            for(int y=0;y<roomHeight;y++)
            {
                wallTilemap.SetTile(
                    origin + new Vector3Int(roomWidth-1,y,0),
                    wallTile);
            }
        }
    }

    Vector3Int RoomOrigin(Vector2Int gridPos)
    {
        return new Vector3Int(
            gridPos.x * roomWidth,
            gridPos.y * roomHeight,
            0);
    }
}
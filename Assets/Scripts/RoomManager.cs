using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    [SerializeField] RoomGenerator generator;

    int currentRow = 0;
    int currentStage = 0;
    // 0 = 가운데
    // 1 = 왼쪽
    // 2 = 오른쪽

    int bottomRow = 0;

    void Start()
    {
        CreateRoom(Vector2Int.zero, 0);
        currentStage = 1;   // 가운데는 이미 생성했으므로 다음은 왼쪽
    }

    public bool SpawnNextRoom()
    {
        Vector2Int pos;

        int currentWave = 3 * currentRow + currentStage;

        switch (currentWave)
        {
            case 0:
                pos = new Vector2Int(0, currentRow);
                break;

            case 1:
                pos = new Vector2Int(-1, currentRow);
                break;

            default:
                pos = new Vector2Int(1, currentRow);
                break;
        }

        CreateRoom(pos, currentWave);

        currentStage++;

        if (currentStage >= 3)
        {
            currentStage = 0;
            currentRow++;
        }

        // 새로운 줄의 가운데가 생성된 직후 삭제
        if (currentStage == 1 && currentRow - bottomRow > 1)
        {
            DeleteRow(bottomRow);
            bottomRow++;
        }

        return true;
    }

    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Room Size")]
    public int roomWidth = 15;
    public int roomHeight = 11;
    
    [Header("Room Data")]
    [SerializeField] List<RoomTypeData> roomTypeDatas = new();

    Dictionary<RoomType, RoomTypeData> roomDataMap;

    Dictionary<Vector2Int, Room> rooms = new();

    public IReadOnlyDictionary<Vector2Int, Room> Rooms => rooms;

    void Awake()
    {
        roomDataMap = new Dictionary<RoomType, RoomTypeData>();

        foreach (var data in roomTypeDatas)
            roomDataMap[data.roomType] = data;
    }

    public List<SpawnPointData> GetSpawnPoints(Room room)
    {
        return roomDataMap[room.Type].spawnPoints;
    }

    public Vector3 LocalToWorld(Vector2 localPos)
    {
        Vector3Int referenceCell =
            RoomOrigin(new Vector2Int(0, bottomRow))
            + new Vector3Int(roomWidth / 2, 0, 0);

        Vector3 referencePosition =
            floorTilemap.GetCellCenterWorld(referenceCell);

        return referencePosition + (Vector3)localPos;
    }

    public Room CreateRoom(Vector2Int gridPos, int currentWave)
    {
        if (rooms.ContainsKey(gridPos))
            return rooms[gridPos];

        Room room = new Room(gridPos, currentWave);

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

    Vector3 GetReferenceWorldPosition()
    {
        int minRow = int.MaxValue;

        foreach (var room in rooms.Values)
            minRow = Mathf.Min(minRow, room.GridPos.y);

        Vector3Int origin = RoomOrigin(new Vector2Int(0, minRow));

        return floorTilemap.CellToWorld(origin)
            + new Vector3(roomWidth * 0.5f, 0f, 0f);
    }

    Vector3 GetRoomOffset(Room room)
    {
        int minRow = int.MaxValue;

        foreach (var r in rooms.Values)
            minRow = Mathf.Min(minRow, r.GridPos.y);

        return new Vector3(
            room.GridPos.x * roomWidth,
            (room.GridPos.y - minRow) * roomHeight,
            0f);
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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    int currentRow = 0;

    // 한 줄 안에서 방이 붙는 순서
    // 0 = 가운데
    // 1 = 왼쪽
    // 2 = 오른쪽
    int currentStage = 0;

    /// 현재 남아 있는 방 중 가장 아래 줄. 방이 삭제되면 함께 올라간다.
    public int BottomRow
    {
        get
        {
            bool found = false;
            int min = 0;

            foreach (Vector2Int pos in rooms.Keys)
            {
                if (!found || pos.y < min)
                {
                    min = pos.y;
                    found = true;
                }
            }

            return min;
        }
    }

    void Start()
    {
        CreateRoom(Vector2Int.zero, 0);
        currentStage = 1;   // 가운데는 이미 생성했으므로 다음은 왼쪽
    }

    public bool SpawnNextRoom()
    {
        // 붙일 위치는 '몇 번째 확장인지'가 아니라 줄 안에서의 순서(currentStage)로 정해진다.
        Vector2Int pos = currentStage switch
        {
            0 => new Vector2Int(0, currentRow),
            1 => new Vector2Int(-1, currentRow),
            _ => new Vector2Int(1, currentRow),
        };

        CreateRoom(pos, 3 * currentRow + currentStage);

        currentStage++;

        if (currentStage >= 3)
        {
            currentStage = 0;
            currentRow++;
        }

        // 새로운 줄의 가운데가 생성된 직후, 두 줄 아래는 삭제한다.
        if (currentStage == 1 && currentRow - BottomRow > 1)
            DeleteRow(BottomRow);

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

    static readonly List<SpawnPointData> emptySpawnPoints = new();

    public List<SpawnPointData> GetSpawnPoints(Room room)
    {
        if (room == null || roomDataMap == null) return emptySpawnPoints;

        if (roomDataMap.TryGetValue(room.Type, out RoomTypeData data) && data.spawnPoints != null)
            return data.spawnPoints;

        Debug.LogWarning($"[RoomManager] RoomType '{room.Type}' 의 Room Data(스폰 포인트)가 없습니다.");
        return emptySpawnPoints;
    }

    public Vector3 LocalToWorld(Vector2 localPos)
    {
        Vector3Int referenceCell =
            RoomOrigin(new Vector2Int(0, BottomRow))
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

        NormalizeRows();
        DrawAllRooms();
    }

    /// <summary>
    /// 아래 줄이 사라져도 전투 공간이 항상 0번 줄에서 시작하도록 남은 방을 아래로 당긴다.
    /// 이렇게 하지 않으면 LocalToWorld 의 기준점이 한 방 높이만큼 위로 뛰면서
    /// 월드에 고정된 캐논·설치된 거울·날아가던 공이 전투 공간 밖에 남는다.
    /// </summary>
    void NormalizeRows()
    {
        int bottom = BottomRow;
        if (bottom == 0 || rooms.Count == 0) return;

        var shifted = new Dictionary<Vector2Int, Room>(rooms.Count);

        foreach (Room room in rooms.Values)
        {
            room.GridPos = new Vector2Int(room.GridPos.x, room.GridPos.y - bottom);
            shifted[room.GridPos] = room;
        }

        rooms = shifted;
        currentRow -= bottom;
    }

    void DrawAllRooms()
    {
        // 아래 줄이 삭제되면 위쪽 방이 아래 줄이 되므로, 방 타입을 매번 다시 계산한다.
        RefreshRoomTypes();

        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        foreach(Room room in rooms.Values)
            DrawRoom(room);

        EventBus.Publish("RoomsChanged");
    }

    void RefreshRoomTypes()
    {
        int bottom = BottomRow;

        foreach (Room room in rooms.Values)
            room.Type = Room.DecideRoomType(room.GridPos, bottom);
    }

    /// 현재 남아있는 방 전체를 감싸는 월드 영역 (카메라 프레이밍용)
    public Bounds RoomsBounds()
    {
        Vector2Int min = new(int.MaxValue, int.MaxValue);
        Vector2Int max = new(int.MinValue, int.MinValue);

        foreach (Vector2Int pos in rooms.Keys)
        {
            min = Vector2Int.Min(min, pos);
            max = Vector2Int.Max(max, pos);
        }

        Bounds bounds = new();
        bounds.SetMinMax(
            floorTilemap.CellToWorld(new Vector3Int(min.x * roomWidth, min.y * roomHeight, 0)),
            floorTilemap.CellToWorld(new Vector3Int((max.x + 1) * roomWidth, (max.y + 1) * roomHeight, 0)));

        return bounds;
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

        if (!hasDown)
        {
            for (int x = 0; x < roomWidth; x++)
            {
                wallTilemap.SetTile(
                    origin + new Vector3Int(x, 0, 0),
                    wallTile);
            }

            // 현재 존재하는 방 중 가장 아래 중앙 방
            // 아래쪽 벽의 중앙 타일 제거
            if (room.GridPos.x == 0 && room.GridPos.y == BottomRow)
            {
                int centerX = roomWidth / 2;

                wallTilemap.SetTile(origin + new Vector3Int(centerX, 0, 0), null);

                if(roomWidth % 2 == 0)
                    wallTilemap.SetTile(origin + new Vector3Int(centerX - 1, 0, 0), null);
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
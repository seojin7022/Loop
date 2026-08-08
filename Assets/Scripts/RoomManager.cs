using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    [Header("Room Size")]
    [SerializeField] private int roomWidth = 15;
    [SerializeField] private int roomHeight = 11;

    [Header("Room Template")]
    [SerializeField] private Tilemap templateFloorTilemap;
    [SerializeField] private Tilemap templateWallTilemap;

    [Header("Generated Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Room Data")]
    [SerializeField] private List<RoomTypeData> roomTypeDatas = new();

    private Dictionary<RoomType, RoomTypeData> roomDataMap;
    private readonly Dictionary<Vector2Int, Room> rooms = new();

    public IReadOnlyDictionary<Vector2Int, Room> Rooms => rooms;

    private int currentRow = 0;
    private int currentStage = 0;

    // 현재 존재하는 맨 아래 줄
    private int bottomRow = 0;

    public int BottomRow => bottomRow;

    //==================================================
    // 초기화
    //==================================================

    private void Awake()
    {
        roomDataMap = new Dictionary<RoomType, RoomTypeData>();

        foreach (RoomTypeData data in roomTypeDatas)
        {
            roomDataMap[data.roomType] = data;
        }
    }

    private void Start()
    {
        CreateRoom(Vector2Int.zero, 0);

        // 가운데는 이미 생성했으므로
        // 다음은 왼쪽
        currentStage = 1;
    }

    //==================================================
    // 방 생성
    //==================================================

    public Room CreateRoom(Vector2Int gridPos, int startWave)
    {
        if (rooms.TryGetValue(gridPos, out Room existingRoom))
            return existingRoom;

        Room room = new Room(gridPos, startWave);

        rooms.Add(gridPos, room);

        RedrawAllRooms();

        return room;
    }

    //==================================================
    // 다음 방 생성
    //==================================================

    public bool SpawnNextRoom()
    {
        Vector2Int pos;

        switch (currentStage)
        {
            // 가운데
            case 0:
                pos = new Vector2Int(0, currentRow);
                break;

            // 왼쪽
            case 1:
                pos = new Vector2Int(-1, currentRow);
                break;

            // 오른쪽
            default:
                pos = new Vector2Int(1, currentRow);
                break;
        }

        CreateRoom(
            pos,
            3 * currentRow + currentStage
        );

        currentStage++;

        // 한 줄 완성
        if (currentStage >= 3)
        {
            currentStage = 0;
            currentRow++;
        }

        // 새로운 줄의 가운데가 생성된 직후
        // 기존 맨 아래 줄 삭제
        if (currentStage == 1 &&
            currentRow - bottomRow > 1)
        {
            DeleteRow(bottomRow);
            bottomRow++;
        }

        return true;
    }

    //==================================================
    // 방 삭제
    //==================================================

    public void DeleteRow(int row)
    {
        rooms.Remove(new Vector2Int(-1, row));
        rooms.Remove(new Vector2Int(0, row));
        rooms.Remove(new Vector2Int(1, row));

        RedrawAllRooms();
    }

    //==================================================
    // Spawn Point
    //==================================================

    public List<SpawnPointData> GetSpawnPoints(Room room)
    {
        if (!roomDataMap.TryGetValue(room.Type, out RoomTypeData data))
        {
            Debug.LogError(
                $"RoomTypeData가 없습니다: {room.Type}"
            );

            return new List<SpawnPointData>();
        }

        return data.spawnPoints;
    }

    //==================================================
    // 좌표 변환
    //==================================================

    public Vector3 LocalToWorld(Vector2 localPos)
    {
        Vector3Int originCell = RoomOrigin(new Vector2Int(0, bottomRow));
        Vector3 origin = floorTilemap.CellToWorld(originCell);
        return origin + (Vector3)localPos;
    }

    //==================================================
    // 전체 맵 다시 그리기
    //==================================================

    private void RedrawAllRooms()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        /*
         * 먼저 현재 bottomRow 기준으로
         * 모든 RoomType을 갱신한다.
         */
        foreach (Room room in rooms.Values)
        {
            room.UpdateType(bottomRow);
        }

        /*
         * 템플릿을 복사해서 모든 방을 생성한다.
         */
        foreach (Room room in rooms.Values)
        {
            DrawRoom(room);
        }

        /*
         * 주변 방과 연결되는 벽을 제거한다.
         */
        foreach (Room room in rooms.Values)
        {
            UpdateWalls(room);
        }
    }

    //==================================================
    // 방 하나 그리기
    //==================================================

    private void DrawRoom(Room room)
    {
        Vector3Int origin = RoomOrigin(room.GridPos) - new Vector3Int(roomWidth / 2, 0);
        CopyTilemap(templateFloorTilemap, floorTilemap, origin);
        CopyTilemap(templateWallTilemap, wallTilemap, origin);
    }

    //==================================================
    // 템플릿 Tilemap 복사
    //==================================================

    private void CopyTilemap(Tilemap source, Tilemap destination, Vector3Int destinationOrigin)
    {
        if (source == null || destination == null)
        {
            Debug.LogError("Tilemap reference is missing.");
            return;
        }

        BoundsInt bounds = source.cellBounds;

        for (int y = 0; y < roomHeight; y++)
        {
            int copied = 0;
            for (int x = 0; x < roomWidth; x++)
            {
                Vector3Int sourcePos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);

                TileBase tile = source.GetTile(sourcePos);

                if (tile != null)
                    copied++;

                Vector3Int destinationPos = destinationOrigin + new Vector3Int(x, y, 0);

                destination.SetTile(destinationPos, tile);
            }
        }

    }

    //==================================================
    // 벽 연결 처리
    //==================================================

    private void UpdateWalls(Room room)
    {
        Vector3Int origin = RoomOrigin(room.GridPos);

        bool hasLeft = rooms.ContainsKey(
            room.GridPos + Vector2Int.left
        );

        bool hasRight = rooms.ContainsKey(
            room.GridPos + Vector2Int.right
        );

        bool hasUp = rooms.ContainsKey(
            room.GridPos + Vector2Int.up
        );

        bool hasDown = rooms.ContainsKey(
            room.GridPos + Vector2Int.down
        );

        if (hasLeft)
        {
            RemoveLeftWall(origin);
        }

        if (hasRight)
        {
            RemoveRightWall(origin);
        }

        if (hasUp)
        {
            RemoveTopWall(origin);
        }

        if (hasDown)
        {
            RemoveBottomWall(origin);
        }

        /*
        * 현재 존재하는 방 중
        * 가장 아래 중앙 방의 아래쪽 중앙을 출입구로 사용
        */

        if (room.GridPos.x == 0 && room.GridPos.y == bottomRow && !hasDown)
        {
            wallTilemap.SetTile(origin, null);

            if (roomWidth % 2 == 0)
            {
                wallTilemap.SetTile(origin + new Vector3Int(-1, 0, 0), null);
            }
        }

        RemoveCornerIfNeeded(room);
    }

    //==================================================
    // 벽 제거
    //==================================================

    void RemoveLeftWall(Vector3Int origin)
    {
        for (int y = 1; y < roomHeight - 1; y++)
        {
            wallTilemap.SetTile(origin + new Vector3Int(-roomWidth / 2 + 1, y, 0), null);
        }
    }

    void RemoveRightWall(Vector3Int origin)
    {
        for (int y = 1; y < roomHeight - 1; y++)
        {
            wallTilemap.SetTile(origin + new Vector3Int(roomWidth / 2, y, 0), null);
        }
    }

    void RemoveTopWall(Vector3Int origin)
    {
        for (int x = -roomWidth / 2 + 1; x < roomWidth / 2 - 1; x++)
        {
            wallTilemap.SetTile(origin + new Vector3Int(x, roomHeight - 1, 0), null);
        }
    }

    void RemoveBottomWall(Vector3Int origin)
    {
        for (int x = -roomWidth / 2 + 1; x < roomWidth / 2 - 1; x++)
        {
            wallTilemap.SetTile(origin + new Vector3Int(x, 0, 0), null);
        }
    }

    //==================================================
    // 코너 처리
    //==================================================

    void RemoveCornerIfNeeded(Room room)
    {
        Vector3Int origin = RoomOrigin(room.GridPos);

        // 오른쪽 위
        if (rooms.ContainsKey(room.GridPos + Vector2Int.right) &&
            rooms.ContainsKey(room.GridPos + Vector2Int.up) &&
            rooms.ContainsKey(room.GridPos + new Vector2Int(1, 1)))
        {
            wallTilemap.SetTile(origin + new Vector3Int(roomWidth / 2 - 1, roomHeight - 1, 0), null);
        }

        // 왼쪽 위
        if (rooms.ContainsKey(room.GridPos + Vector2Int.left) &&
            rooms.ContainsKey(room.GridPos + Vector2Int.up) &&
            rooms.ContainsKey(room.GridPos + new Vector2Int(-1, 1)))
        {
            wallTilemap.SetTile(origin + new Vector3Int(-roomWidth / 2, roomHeight - 1, 0), null);
        }

        // 오른쪽 아래
        if (rooms.ContainsKey(room.GridPos + Vector2Int.right) &&
            rooms.ContainsKey(room.GridPos + Vector2Int.down) &&
            rooms.ContainsKey(room.GridPos + new Vector2Int(1, -1)))
        {
            wallTilemap.SetTile(origin + new Vector3Int(roomWidth / 2 - 1, 0, 0), null);
        }

        // 왼쪽 아래
        if (rooms.ContainsKey(room.GridPos + Vector2Int.left) &&
            rooms.ContainsKey(room.GridPos + Vector2Int.down) &&
            rooms.ContainsKey(room.GridPos + new Vector2Int(-1, -1)))
        {
            wallTilemap.SetTile(origin + new Vector3Int(-roomWidth / 2, 0, 0), null);
        }
    }

    //==================================================
    // Room 위치
    //==================================================

    Vector3Int RoomOrigin(Vector2Int gridPos)
    {
        return new Vector3Int(
            gridPos.x * (roomWidth - 1),
            gridPos.y * (roomHeight - 1) - roomHeight / 2,
            0
        );
    }

    //==================================================
    // 외부 접근
    //==================================================

    public IEnumerable<Room> GetAllRooms()
    {
        return rooms.Values;
    }
}
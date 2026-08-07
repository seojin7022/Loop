using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private RoomGenerator generator;

    private int currentRow = 0;
    private int currentStage = 0;
    // 0 = 가운데
    // 1 = 왼쪽
    // 2 = 오른쪽

    private int bottomRow = 0;

    public IReadOnlyDictionary<Vector2Int, Room> Rooms => generator.Rooms;

    void Start()
    {
        generator.CreateRoom(Vector2Int.zero);
        currentStage = 1;   // 가운데는 이미 생성했으므로 다음은 왼쪽
    }

    public bool SpawnNextRoom()
    {
        Vector2Int pos;

        switch (currentStage)
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

        generator.CreateRoom(pos);

        currentStage++;

        if (currentStage >= 3)
        {
            currentStage = 0;
            currentRow++;
        }

        // 새로운 줄의 가운데가 생성된 직후 삭제
        if (currentStage == 1 && currentRow - bottomRow > 1)
        {
            generator.DeleteRow(bottomRow);
            bottomRow++;
        }

        return true;
    }
    
    public IEnumerable<Room> GetAllRooms()
    {
        return generator.Rooms.Values;
    }
}
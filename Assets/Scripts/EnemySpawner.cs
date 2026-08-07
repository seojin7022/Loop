using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public RoomManager roomManager;
    public GameObject enemyPrefab;
    public RoomGenerator generator;

    public void SpawnWave()
    {
        foreach(Room room in roomManager.GetAllRooms())
            SpawnInRoom(room);
    }

    void SpawnInRoom(Room room)
    {
        foreach (Vector3 pos in generator.GetSpawnPositions(room))
        {
            var a = Instantiate(enemyPrefab, pos, Quaternion.identity);
            Destroy(a, 0.5f);
        }
    }
}

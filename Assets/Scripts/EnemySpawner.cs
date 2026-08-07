using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;

public class EnemySpawner : MonoBehaviour
{
    public RoomManager roomManager;
    public GameObject enemyPrefab;

    [SerializeField]
    float initialEnemyNum, numChangePerWave;
    [SerializeField]
    float initialEnemySpeed, speedChangePerWave;
    [SerializeField]
    float initialSpawnInterval, intervalChangePerWave;

    public void SpawnWave(int wave, int wavePeriod)
    {
        foreach(Room room in roomManager.Rooms.Values)
            SpawnWaveAsync(room, wave, wavePeriod).Forget();
    }

    public async UniTaskVoid SpawnWaveAsync(Room room, int wave, int wavePeriod)
    {
        for(int i = 0; i < initialEnemyNum + numChangePerWave * wave; i++)
        {
            int actualWave = Mathf.Min(room.StartWave + wavePeriod - 1, wave);
            int num = (int)(initialEnemyNum + numChangePerWave * actualWave);

            if(i >= num) break;

            float speed = initialEnemySpeed + speedChangePerWave * actualWave;
            
            SpawnInRoom(room, speed);

            float interval = initialSpawnInterval + intervalChangePerWave * actualWave;

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }

    void SpawnInRoom(Room room, float speed)
    {
        foreach (var point in roomManager.GetSpawnPoints(room))
        {
            Vector3 spawn = roomManager.LocalToWorld(point.spawnPosition);
            Vector3 target = roomManager.LocalToWorld(point.targetPosition);

            var obj = Instantiate(enemyPrefab, spawn, Quaternion.identity);
            obj.GetComponent<Enemy>().SetEnemy(spawn, target, speed);
        }
    }
}

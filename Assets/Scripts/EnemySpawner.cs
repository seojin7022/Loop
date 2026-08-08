using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using R3;

public class EnemySpawner : MonoBehaviour
{
    public GameManager gameManager;
    public RoomManager roomManager;
    public GameObject enemyPrefab;

    [SerializeField]
    float initialEnemyNum, numChangePerWave;
    [SerializeField]
    float initialEnemyHP, hpChangePerWave;
    [SerializeField]
    float initialEnemySpeed, speedChangePerWave;
    [SerializeField]
    float initialSpawnInterval, intervalChangePerWave;

    public int SpawnWave(int wave, int wavePeriod)
    {
        int enemyNum = 0;
        foreach(Room room in roomManager.Rooms.Values)
        {
            int actualWave = Mathf.Min(room.StartWave + wavePeriod - 1, wave);
            int num = (int)(initialEnemyNum + numChangePerWave * wave);
            enemyNum += num * roomManager.GetSpawnPoints(room).Count;

            SpawnWaveAsync(room, wave, num).Forget();
        }
        return enemyNum;
    }

    public async UniTaskVoid SpawnWaveAsync(Room room, int wave, int num)
    {
        for(int i = 0; i < num; i++)
        {
            float speed = initialEnemySpeed + speedChangePerWave * wave;
            float hp = initialEnemyHP + hpChangePerWave * wave;
            
            SpawnInRoom(room, speed, hp);

            float interval = initialSpawnInterval + intervalChangePerWave * wave;

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }

    void SpawnInRoom(Room room, float speed, float hp)
    {
        foreach (var point in roomManager.GetSpawnPoints(room))
        {
            Vector3 spawn = roomManager.LocalToWorld(point.spawnPosition);
            Vector3 target = roomManager.LocalToWorld(point.targetPosition);

            var obj = Instantiate(enemyPrefab, spawn, Quaternion.identity);
            obj.GetComponent<Enemy>().SetEnemy(spawn, target, speed, hp);
        }
    }
}

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// 하나의 적 진입 라인. 방 하나의 스폰 포인트 하나에 대응한다.
public struct SpawnLane
{
    public Room Room;
    public SpawnPointData Point;
    public Vector3 SpawnWorld;
    public Vector3 TargetWorld;
}

public class EnemySpawner : MonoBehaviour
{
    public GameManager gameManager;
    public RoomManager roomManager;
    public GameObject enemyPrefab;

    [Header("Legacy 공식 (스테이지 표를 쓰지 않을 때)")]
    [SerializeField]
    float initialEnemyNum, numChangePerWave;
    [SerializeField]
    float initialEnemyHP, hpChangePerWave;
    [SerializeField]
    float initialEnemySpeed, speedChangePerWave;
    [SerializeField]
    float initialSpawnInterval, intervalChangePerWave;

    [Header("스테이지 밸런스 표")]
    [Tooltip("켜면 기획서의 스테이지 표(처치 목표/체력/속도/간격)를 사용한다.")]
    [SerializeField] bool useStageTable = true;
    [SerializeField] StageTable stageTable = new();

    public StageTable Table => stageTable;
    public bool UseStageTable => useStageTable;

    /// 현재 남아 있는 모든 방의 진입 라인 목록. 사전 동선 표시와 스폰이 같은 데이터를 쓴다.
    public List<SpawnLane> BuildLanes()
    {
        var lanes = new List<SpawnLane>();
        if (roomManager == null) return lanes;

        foreach (Room room in roomManager.Rooms.Values)
        {
            List<SpawnPointData> points = roomManager.GetSpawnPoints(room);
            if (points == null) continue;

            foreach (SpawnPointData point in points)
            {
                lanes.Add(new SpawnLane
                {
                    Room = room,
                    Point = point,
                    SpawnWorld = roomManager.LocalToWorld(point.spawnPosition),
                    TargetWorld = roomManager.LocalToWorld(point.targetPosition),
                });
            }
        }

        return lanes;
    }

    /// 이번 웨이브에 등장할 적의 총 수를 반환하고, 스폰을 시작한다.
    public int SpawnWave(int wave, int wavePeriod)
    {
        List<SpawnLane> lanes = BuildLanes();
        if (lanes.Count == 0) return 0;

        int total = GetWaveEnemyCount(wave, wavePeriod, lanes.Count);
        if (total <= 0) return 0;

        StageData stage = GetStage(wave, wavePeriod);
        float hp = useStageTable ? stage.enemyHp : initialEnemyHP + hpChangePerWave * wave;
        float speed = useStageTable ? stage.enemySpeed : initialEnemySpeed + speedChangePerWave * wave;
        float interval = useStageTable ? stage.spawnInterval : initialSpawnInterval + intervalChangePerWave * wave;
        interval = Mathf.Max(0.05f, interval);

        // 총 마릿수를 라인에 고르게 분배한다.
        var perLane = new int[lanes.Count];
        for (int i = 0; i < total; i++)
            perLane[i % lanes.Count]++;

        for (int i = 0; i < lanes.Count; i++)
        {
            if (perLane[i] <= 0) continue;
            SpawnLaneAsync(lanes[i], perLane[i], speed, hp, interval).Forget();
        }

        return total;
    }

    public StageData GetStage(int wave, int wavePeriod)
    {
        return stageTable.Get(StageTable.StageOfWave(wave, wavePeriod));
    }

    public int GetWaveEnemyCount(int wave, int wavePeriod, int laneCount)
    {
        if (useStageTable)
            return GetStage(wave, wavePeriod).killTarget;

        int perLane = Mathf.Max(0, (int)(initialEnemyNum + numChangePerWave * wave));
        return perLane * laneCount;
    }

    async UniTaskVoid SpawnLaneAsync(SpawnLane lane, int count, float speed, float hp, float interval)
    {
        var token = this.GetCancellationTokenOnDestroy();

        for (int i = 0; i < count; i++)
        {
            if (token.IsCancellationRequested) return;

            Spawn(lane, speed, hp);

            if (i < count - 1)
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
        }
    }

    void Spawn(SpawnLane lane, float speed, float hp)
    {
        var obj = Instantiate(enemyPrefab, lane.SpawnWorld, Quaternion.identity);
        obj.GetComponent<Enemy>().SetEnemy(lane.SpawnWorld, lane.TargetWorld, speed, hp);
    }
}

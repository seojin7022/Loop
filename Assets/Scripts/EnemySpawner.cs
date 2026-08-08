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

    [Header("스폰 간격 보호")]
    [Tooltip("같은 라인에서 앞 적과 유지할 최소 거리(월드 단위).")]
    [SerializeField, Min(0.1f)] float minimumEnemyGap = 0.75f;

    [Header("스테이지 밸런스 표")]
    [Tooltip("켜면 기획서의 스테이지 표(처치 목표/체력/속도/간격)를 사용한다.")]
    [SerializeField] bool useStageTable = true;
    [SerializeField] StageTable stageTable = new();

    public StageTable Table => stageTable;
    public bool UseStageTable => useStageTable;

    /// 현재 남아 있는 모든 방의 진입 라인 목록. 사전 동선 표시와 스폰이 같은 데이터를 쓴다.
public List<SpawnLane> BuildLanes(int wave = 0, int wavePeriod = 1)
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

        ApplyStageLanePattern(lanes, StageTable.StageOfWave(wave, Mathf.Max(1, wavePeriod)));
        return lanes;
    }

    // 스테이지마다 적의 진입 방향과 목적지를 바꿔 같은 거울 배치가 반복되지 않게 한다.
    void ApplyStageLanePattern(List<SpawnLane> lanes, int stage)
    {
        if (lanes.Count == 0 || roomManager == null || stage <= 1) return;

        Bounds bounds = roomManager.RoomsBounds();
        float left = bounds.min.x + 1.5f;
        float right = bounds.max.x - 1.5f;
        float bottom = bounds.min.y + 1.5f;
        float top = bounds.max.y - 1.5f;
        float centerX = bounds.center.x;
        float centerY = bounds.center.y;

        switch ((stage - 1) % 4)
        {
            case 1: // 좌우 반전
                for (int i = 0; i < lanes.Count; i++)
                {
                    SpawnLane lane = lanes[i];
                    lane.SpawnWorld.x = 2f * centerX - lane.SpawnWorld.x;
                    lane.TargetWorld.x = 2f * centerX - lane.TargetWorld.x;
                    lanes[i] = lane;
                }
                break;

            case 2: // 대각선 교차 진입
                for (int i = 0; i < lanes.Count; i++)
                {
                    bool fromUpperLeft = i % 2 == 0;
                    SpawnLane lane = lanes[i];
                    lane.SpawnWorld = new Vector3(fromUpperLeft ? left : right, fromUpperLeft ? top : bottom, 0f);
                    lane.TargetWorld = new Vector3(fromUpperLeft ? right : left, centerY, 0f);
                    lanes[i] = lane;
                }
                break;

            case 3: // 협공: 위·아래에서 중앙을 향해 진입
                for (int i = 0; i < lanes.Count; i++)
                {
                    bool fromLeft = i % 2 == 0;
                    SpawnLane lane = lanes[i];
                    lane.SpawnWorld = new Vector3(fromLeft ? left : right, fromLeft ? bottom : top, 0f);
                    lane.TargetWorld = new Vector3(centerX, fromLeft ? top : bottom, 0f);
                    lanes[i] = lane;
                }
                break;
        }
    }

    /// 이번 웨이브에 등장할 적의 총 수를 반환하고, 스폰을 시작한다.
    public int SpawnWave(int wave, int wavePeriod)
    {
        List<SpawnLane> lanes = BuildLanes(wave, wavePeriod);
        if (lanes.Count == 0) return 0;

        int total = GetWaveEnemyCount(wave, wavePeriod, lanes.Count);
        if (total <= 0) return 0;

        StageData stage = GetStage(wave, wavePeriod);
        float hp = useStageTable ? stage.enemyHp : initialEnemyHP + hpChangePerWave * wave;
        float speed = useStageTable ? stage.enemySpeed : initialEnemySpeed + speedChangePerWave * wave;
        float interval = useStageTable ? stage.spawnInterval : initialSpawnInterval + intervalChangePerWave * wave;
        // 앞 적이 최소 간격만큼 진행한 뒤 다음 적을 생성해 같은 라인의 겹침을 막는다.
        float safeInterval = minimumEnemyGap / Mathf.Max(0.01f, speed);
        interval = Mathf.Max(0.05f, interval, safeInterval);

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

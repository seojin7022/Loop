using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기획서 7. 스테이지 밸런스 표를 코드/인스펙터에서 관리하기 위한 데이터.
/// 처치 목표는 해당 스테이지의 한 웨이브에서 등장하는 적의 총 수를 뜻한다.
/// </summary>
[Serializable]
public class StageData
{
    public int stage = 1;

    [Tooltip("이 스테이지 한 웨이브에 등장하는 적의 총 수 (처치 목표)")]
    public int killTarget = 4;

    public float enemyHp = 1f;
    public float enemySpeed = 1f;

    [Tooltip("같은 진입 라인에서 적이 연달아 나오는 간격 (초)")]
    public float spawnInterval = 0.9f;

    [TextArea] public string note;
}

[Serializable]
public class StageTable
{
    [SerializeField]
    List<StageData> stages = new()
    {
        new StageData { stage = 1, killTarget = 4,  enemyHp = 1f, enemySpeed = 1.00f, spawnInterval = 0.90f, note = "상·하단 기본 라인, 튜토리얼" },
        new StageData { stage = 2, killTarget = 5,  enemyHp = 1f, enemySpeed = 1.05f, spawnInterval = 0.85f, note = "상·하단 교대 출현" },
        new StageData { stage = 3, killTarget = 6,  enemyHp = 1f, enemySpeed = 1.10f, spawnInterval = 0.80f, note = "신규 상단 진입로 소개" },
        new StageData { stage = 4, killTarget = 9,  enemyHp = 1f, enemySpeed = 1.15f, spawnInterval = 0.70f, note = "맵 상단 축소, 3개 라인 활용" },
        new StageData { stage = 5, killTarget = 12, enemyHp = 1f, enemySpeed = 1.20f, spawnInterval = 0.50f, note = "출현 간격 단축" },
        new StageData { stage = 6, killTarget = 15, enemyHp = 1f, enemySpeed = 1.25f, spawnInterval = 0.40f, note = "최종 밀도, 3개 라인 압박" },
    };

    [Header("표를 넘어선 스테이지 (7 이상) 연장 규칙")]
    [SerializeField] int killTargetGrowth = 3;
    [SerializeField] float speedGrowth = 0.05f;
    [SerializeField] float intervalGrowth = -0.02f;
    [SerializeField] float minSpawnInterval = 0.15f;

    public int Count => stages.Count;

    public IReadOnlyList<StageData> Stages => stages;

    /// 1부터 시작하는 스테이지 번호로 데이터를 얻는다. 표를 넘어가면 마지막 값을 기준으로 연장한다.
    public StageData Get(int stage)
    {
        if (stages == null || stages.Count == 0)
            return new StageData();

        stage = Mathf.Max(1, stage);

        if (stage <= stages.Count)
            return stages[stage - 1];

        StageData last = stages[^1];
        int over = stage - stages.Count;

        return new StageData
        {
            stage = stage,
            killTarget = last.killTarget + killTargetGrowth * over,
            enemyHp = last.enemyHp,
            enemySpeed = last.enemySpeed + speedGrowth * over,
            spawnInterval = Mathf.Max(minSpawnInterval, last.spawnInterval + intervalGrowth * over),
            note = "표 연장",
        };
    }

    /// 웨이브 번호(0부터)와 스테이지 길이로부터 스테이지 번호(1부터)를 계산한다.
    public static int StageOfWave(int wave, int wavePeriod)
    {
        if (wavePeriod <= 0) wavePeriod = 1;
        return Mathf.Max(0, wave) / wavePeriod + 1;
    }
}

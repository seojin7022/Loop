using UnityEngine;
using R3;
using Cysharp.Threading.Tasks;

public class WaveManager : MonoBehaviour
{
    [SerializeField]
    RoomManager roomManager;
    [SerializeField]
    EnemySpawner spawner;
    [SerializeField]
    int hp;

    public int wavePeriod;

    int currrentWave;

    int nowEnemyNum;

    public void Awake()
    {
        EventBus.OnEvent("EnemyDie")
                .Subscribe(_ => EnemyDie())
                .AddTo(this);
        
        EventBus.OnEvent("PlayerDamage")
                .Subscribe(_ => PlayerDamage())
                .AddTo(this);
    }

    public void EnemyDie()
    {
        nowEnemyNum -= 1;
        Debug.Log(nowEnemyNum);
        if(nowEnemyNum == 0)
            FinishWave();
    }

    public void PlayerDamage()
    {
        nowEnemyNum -= 1;
        Debug.Log(nowEnemyNum);
        hp -= 1;
        if(hp == 0)
            EventBus.Publish("PlayerDie");
        else if(nowEnemyNum == 0)
            FinishWave();
    }


    void Start()
    {
        StartNextWave();
    }

    public void FinishWave()
    {
        currrentWave++;

        if(currrentWave % wavePeriod == 0)
        {
            roomManager.SpawnNextRoom();
        }

        StartNextWave();
    }

    void StartNextWave()
    {
        Debug.Log($"Wave {currrentWave}");

        nowEnemyNum = spawner.SpawnWave(currrentWave, wavePeriod);

        Debug.Log(nowEnemyNum);
    }
}
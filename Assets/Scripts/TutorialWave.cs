using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

public class TutorialWave : MonoBehaviour
{
    [SerializeField] RoomManager roomManager;
    [SerializeField] EnemySpawner spawner;

    Transform[] lines;

    bool running;

    public void StartTutorial()
    {
        if (running) return;

        running = true;

        SetupTutorialMap();

        RunTutorialAsync().Forget();
    }

    void SetupTutorialMap()
    {
        roomManager.DrawAllRooms();
        spawner.BuildLanes(0, 1);

        lines = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            lines[i] = transform.GetChild(i);
    }

    async UniTaskVoid RunTutorialAsync()
    {
        await EventBus.OnEvent("Line1").FirstAsync().AsUniTask();

        Debug.Log("Line1");

        lines[0].gameObject.SetActive(true);

        await EventBus.OnEvent("Line2").FirstAsync().AsUniTask();

        lines[1].gameObject.SetActive(true);

        await EventBus.OnEvent("Line3").FirstAsync().AsUniTask();

        lines[2].gameObject.SetActive(true);
    }

    public void FinishTutorial()
    {
        if (!running) return;

        lines[0].gameObject.SetActive(false);
        lines[1].gameObject.SetActive(false);
        lines[2].gameObject.SetActive(false);

        running = false;

        // 튜토리얼 종료 처리
        Tutorial.Trigger("tutorial_finished");
    }
}

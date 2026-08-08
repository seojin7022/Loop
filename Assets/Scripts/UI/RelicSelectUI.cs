using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PulleyBun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스테이지 시작 전 특성 3택 1 선택 화면.
/// 선택지는 RelicManager.RollChoices 의 풀 규칙을 따른다.
/// 씬 배치 없이 자동 생성되며, 선택하는 동안 게임을 정지한다.
/// </summary>
public class RelicSelectUI : MonoBehaviour
{
    public static RelicSelectUI Instance { get; private set; }

    public static bool IsOpen { get; private set; }

    const int ChoiceCount = 3;

    /// Assets/Resources 기준 경로. 카드 개수·레이아웃은 이 프리팹이 결정한다.
    const string PrefabPath = "RelicSelectCanvas";

    Canvas canvas;
    Relic? picked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;

        var go = new GameObject("@RelicSelectUI");
        go.AddComponent<RelicSelectUI>();
    }

    /// 아직 생성되지 않았다면 만들어서 반환한다. (스크립트 실행 순서에 의존하지 않기 위함)
    public static RelicSelectUI Ensure()
    {
        if (Instance == null) Bootstrap();
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// 선택이 끝날 때까지 대기한다. 제시할 특성이 없으면 즉시 반환한다.
    public async UniTask ShowAndWaitAsync()
    {
        if (!RelicManager.IsEnabled) return;
        RelicManager manager = RelicManager.Instance;
        if (manager == null) return;

        List<Relic> choices = manager.RollChoices(ChoiceCount);
        if (choices.Count == 0) return;

        picked = null;
        IsOpen = true;
        UIBlocker.Push();

        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Build(choices);

        await UniTask.WaitUntil(() => picked.HasValue, PlayerLoopTiming.Update);

        manager.Choose(picked.Value);

        Teardown();
        Time.timeScale = previousTimeScale;
        UIBlocker.Pop();
        IsOpen = false;
    }

    void Build(List<Relic> choices)
    {
        GameObject prefab = Resources.Load<GameObject>(PrefabPath);
        if (prefab == null)
        {
            // 프리팹이 없으면 선택을 기다리는 쪽이 영원히 멈추므로 첫 후보로 진행한다.
            Debug.LogError($"RelicSelectUI: Resources/{PrefabPath} 프리팹을 찾지 못했다.");
            picked = choices[0];
            return;
        }

        canvas = Instantiate(prefab).GetComponent<Canvas>();
        canvas.sortingOrder = 500; // GameHud(300) 위에 그린다.
        RuntimeUI.EnsureEventSystem();

        Transform cards = canvas.transform.Find("Cards");

        for (int i = 0; i < cards.childCount; i++)
        {
            Transform card = cards.GetChild(i);

            // 프리팹의 카드 슬롯이 후보보다 많으면 남는 슬롯은 숨긴다.
            if (i >= choices.Count)
            {
                card.gameObject.SetActive(false);
                continue;
            }

            Relic relic = choices[i];
            RelicInfo info = RelicDatabase.Get(relic);

            card.Find("Name").GetComponent<TMP_Text>().text = info?.DisplayName ?? relic.ToString();
            card.Find("Description").GetComponent<TMP_Text>().text = info?.Description ?? "";

            var icon = card.Find("Icon").GetComponent<Image>();
            icon.sprite = info?.Icon;
            icon.enabled = icon.sprite != null;

            Relic captured = relic;
            card.GetComponent<Button>().onClick.AddListener(() => picked = captured);
        }
    }

    void Teardown()
    {
        if (canvas != null) Destroy(canvas.gameObject);
        canvas = null;
    }
}

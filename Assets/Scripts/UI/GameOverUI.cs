using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// HP가 0이 되었을 때(EventBus "PlayerDie") 표시되는 게임 오버 화면.
/// 씬 배치 없이 자동 생성된다.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    Canvas canvas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;

        var go = new GameObject("@GameOverUI");
        go.AddComponent<GameOverUI>();
    }

    /// 아직 생성되지 않았다면 만들어서 반환한다.
    public static GameOverUI Ensure()
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
        IsOpen = false;
    }

    public void Show(int reachedWave, int stage)
    {
        if (IsOpen) return;

        IsOpen = true;
        UIBlocker.Push();
        Time.timeScale = 0f;

        canvas = RuntimeUI.CreateCanvas("GameOverCanvas", 600);
        RuntimeUI.CreateFullScreen("Dim", canvas.transform, new Color(0f, 0f, 0f, 0.8f));

        TMP_Text title = RuntimeUI.CreateText(
            "Title", canvas.transform, "게임 오버",
            96f, Color.white, TextAlignmentOptions.Center);
        var titleRect = (RectTransform)title.transform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 160f);
        titleRect.sizeDelta = new Vector2(1000f, 130f);

        TMP_Text detail = RuntimeUI.CreateText(
            "Detail", canvas.transform,
            $"스테이지 {stage}  ·  웨이브 {reachedWave + 1} 도달",
            40f, new Color(0.85f, 0.88f, 0.95f, 1f), TextAlignmentOptions.Center);
        var detailRect = (RectTransform)detail.transform;
        detailRect.anchorMin = detailRect.anchorMax = new Vector2(0.5f, 0.5f);
        detailRect.anchoredPosition = new Vector2(0f, 60f);
        detailRect.sizeDelta = new Vector2(1000f, 60f);

        CreateAction("Retry", "다시 시작", new Vector2(-170f, -70f),
            () => Restart(SceneManager.GetActiveScene().name));

        CreateAction("Title", "타이틀로", new Vector2(170f, -70f),
            () => Restart("Title"));
    }

    void CreateAction(string name, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        Button button = RuntimeUI.CreateButton(
            name, canvas.transform,
            new Color(0.16f, 0.19f, 0.27f, 0.98f),
            new Color(0.85f, 0.92f, 1f, 1f));

        var rect = (RectTransform)button.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300f, 96f);

        button.onClick.AddListener(action);

        TMP_Text text = RuntimeUI.CreateText(
            "Label", rect, label, 38f, Color.white, TextAlignmentOptions.Center);
        RuntimeUI.Stretch((RectTransform)text.transform, 0f, 0f, 0f, 0f);
    }

    void Restart(string sceneName)
    {
        Time.timeScale = 1f;
        UIBlocker.Reset();
        IsOpen = false;

        if (canvas != null) Destroy(canvas.gameObject);
        canvas = null;

        SceneManager.LoadScene(sceneName);
    }
}

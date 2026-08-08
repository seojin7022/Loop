using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// 타이틀 씬의 시작 버튼을 인게임 씬 전환에 연결한다.
public static class SceneChanger
{
    const string TitleScene = "Title";
    const string IngameScene = "Ingame";

    /// 타이틀 씬이 로드될 때마다 호출된다. (게임 오버 후 타이틀로 돌아온 경우 포함)
    public static void Register()
    {
        if (SceneManager.GetActiveScene().name != TitleScene) return;

        Button startButton = FindStartButton();

        if (startButton == null)
        {
            Debug.LogError("[SceneChanger] 타이틀 씬에서 시작 버튼을 찾지 못했습니다.");
            return;
        }

        startButton.onClick.RemoveListener(LoadIngame);
        startButton.onClick.AddListener(LoadIngame);
    }

    static Button FindStartButton()
    {
        // 이름이 'Button' 인 오브젝트를 우선 사용하고, 없으면 씬의 첫 버튼을 사용한다.
        GameObject named = GameObject.Find("Button");
        if (named != null && named.TryGetComponent(out Button button)) return button;

        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        return buttons.Length > 0 ? buttons[0] : null;
    }

    static void LoadIngame()
    {
        Time.timeScale = 1f;
        UIBlocker.Reset();
        SceneManager.LoadScene(IngameScene);
    }
}

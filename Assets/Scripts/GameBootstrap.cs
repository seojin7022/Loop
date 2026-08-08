using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬을 새로 로드할 때마다 코드로 만드는 시스템들을 다시 세워 준다.
///
/// RuntimeInitializeOnLoadMethod 는 앱이 시작될 때 딱 한 번만 실행되고
/// SceneManager.LoadScene 으로 씬이 바뀔 때는 다시 호출되지 않는다.
/// 그래서 Title 에서 시작하면 그 시점엔 WaveManager 가 없어 HUD 가 만들어지지 않고,
/// Ingame 으로 넘어가도 부트스트랩이 다시 돌지 않아 계속 안 보이는 문제가 있었다.
/// </summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 첫 씬은 sceneLoaded 가 이미 지나갔으므로 여기서 직접 세운다.
        SetupScene();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;

        SetupScene();
    }

    static void SetupScene()
    {
        // 씬 전환 중 UI 가 떠 있었다면 정지/입력 차단 상태가 남을 수 있다.
        Time.timeScale = 1f;
        UIBlocker.Reset();
        RuntimeUI.ClearFontCache();

        // 각 Ensure 는 이미 있으면 아무것도 하지 않는다.
        Fx.Ensure();
        PulleyBun.MirrorAttachments.Ensure();
        GameHud.Ensure();

        SceneChanger.Register();
    }
}

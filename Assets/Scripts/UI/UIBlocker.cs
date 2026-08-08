using UnityEngine;

/// <summary>
/// 전체 화면 UI가 떠 있는 동안 게임플레이 입력(거울 설치·삭제)을 막기 위한 카운터.
/// UI 를 열 때 Push, 닫을 때 Pop 한다.
/// </summary>
public static class UIBlocker
{
    static int count;

    public static bool IsBlocking => count > 0;

    public static void Push() => count++;

    public static void Pop() => count = Mathf.Max(0, count - 1);

    public static void Reset() => count = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnLoad() => count = 0;
}

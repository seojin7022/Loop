using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tutorial : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        [TextArea] public string dialogue;

        [Tooltip("비워두면 아무 키/클릭으로 넘어감. 값이 있으면 Tutorial.Trigger(\"값\") 호출 시에만 넘어감")]
        public string completeOn;
        public string signal;
    }

    [SerializeField] Step[] steps;
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text dialogueLabel;
    [SerializeField] TutorialWave tutorialWave;

    static Tutorial instance;

    public static bool IsRunning => instance != null && instance.enabled;

    void Awake() => instance = this;
    void OnDestroy() { if (instance == this) instance = null; }

    public async UniTask PlayTutorial()
    {
        Debug.Log("Tutorial");
        tutorialWave.StartTutorial();

        skipCts = new CancellationTokenSource();

        try
        {
            foreach(Step step in steps)
            {
                if (dialogueLabel) dialogueLabel.text = step.dialogue;

                if(!string.IsNullOrEmpty(step.signal))
                    EventBus.Publish(step.signal);

                if(string.IsNullOrEmpty(step.completeOn))
                    await UniTask.WaitUntil(() => (Keyboard.current != null && Keyboard.current.anyKey.wasReleasedThisFrame) ||
                                                   Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame,
                                            cancellationToken: skipCts.Token);
                else
                {
                    bool done = false;
                    using IDisposable sub = trigger.Where(x => x == step.completeOn).Subscribe(_ => done = true);
                    await UniTask.WaitUntil(() => done, cancellationToken: skipCts.Token);
                }
            }
        }
        catch (OperationCanceledException) { }  // Skip 버튼

        skipCts.Dispose();
        skipCts = null;

        tutorialWave.FinishTutorial();

        Show(false);
        enabled = false;
        return;
    }

    CancellationTokenSource skipCts;

    /// <summary>Skip 버튼 OnClick 에 연결한다. 튜토리얼을 통째로 끝낸다.</summary>
    public void SkipTutorial() => skipCts?.Cancel();

    static Subject<string> trigger = new();

    public static void Trigger(string id)
    {
        trigger.OnNext(id);
    }

    void Show(bool on)
    {
        if (panel) panel.SetActive(on);
    }

    public static bool Shown()
    {
        return instance != null && instance.panel != null && instance.panel.activeSelf;
    }
}

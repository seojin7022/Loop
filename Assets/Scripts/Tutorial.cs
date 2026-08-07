using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        public string speaker;
        [TextArea] public string dialogue;

        [Tooltip("비워두면 character 이미지 유지, 넣으면 교체 (표정/캐릭터 변경)")]
        public Sprite portrait;

        [Tooltip("비워두면 아무 키/클릭으로 넘어감. 값이 있으면 Tutorial.Trigger(\"값\") 호출 시에만 넘어감")]
        public string completeOn;
    }

    [SerializeField] Step[] steps;

    [Header("Canvas")]
    [SerializeField] Image character;
    [SerializeField] GameObject panel;

    [Header("Panel")]
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] TMP_Text dialogueLabel;

    static Tutorial instance;
    int index = -1;

    public static bool IsRunning => instance != null && instance.enabled;

    void Awake() => instance = this;
    void OnDestroy() { if (instance == this) instance = null; }

    void Start() => Next();

    void Update()
    {
        if (!string.IsNullOrEmpty(steps[index].completeOn)) return;

        if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            Next();
    }

    /// 게임 코드에서 호출: Tutorial.Trigger("move"), Tutorial.Trigger("wave_cleared") 등
    public static void Trigger(string id)
    {
        if (instance != null && instance.enabled && instance.steps[instance.index].completeOn == id)
            instance.Next();
    }

    void Next()
    {
        index++;

        if (index >= steps.Length)
        {
            Show(false);
            enabled = false;
            return;
        }

        Show(true);

        Step step = steps[index];
        if (nameLabel) nameLabel.text = step.speaker;
        if (dialogueLabel) dialogueLabel.text = step.dialogue;
        if (character && step.portrait) character.sprite = step.portrait;
    }

    void Show(bool on)
    {
        if (panel) panel.SetActive(on);
        if (character) character.gameObject.SetActive(on);
    }
}

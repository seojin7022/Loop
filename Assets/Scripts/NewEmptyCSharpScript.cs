using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterTitleButton()
    {
        if (SceneManager.GetActiveScene().name != "Title")
        {
            return;
        }

        Button startButton = GameObject.Find("Button")?.GetComponent<Button>();

        if (startButton == null)
        {
            Debug.LogError("Title scene Button was not found.");
            return;
        }

        startButton.onClick.RemoveListener(LoadIngame);
        startButton.onClick.AddListener(LoadIngame);
    }

    private static void LoadIngame()
    {
        SceneManager.LoadScene("Ingame");
    }
}

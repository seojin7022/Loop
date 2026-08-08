using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class BackgroundMusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip music;
    [SerializeField, Range(0f, 1f)] float targetVolume = 0.32f;
    [SerializeField, Min(0f)] float fadeInDuration = 2.5f;

    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = 0f;
    }

    void Start()
    {
        if (music == null) return;

        audioSource.clip = music;
        audioSource.Play();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        if (fadeInDuration <= 0f)
        {
            audioSource.volume = targetVolume;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}

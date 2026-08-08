using UnityEngine;

/// <summary>외부 파일 없이 짧은 게임 효과음을 합성해 Sfx 이벤트에 연결한다.</summary>
[RequireComponent(typeof(SfxBank))]
public sealed class GameplaySfxBootstrap : MonoBehaviour
{
    const int SampleRate = 44100;

    void Awake()
    {
        SfxBank bank = GetComponent<SfxBank>();
        bank.SetEntry(Sfx.Id.RoomAdded, CreateRoomExpandClip(), 0.48f, 0.25f);
        bank.SetEntry(Sfx.Id.EnemyDie, CreateEnemyDefeatClip(), 0.38f, 0.035f);
    }

    AudioClip CreateRoomExpandClip()
    {
        const float duration = 0.62f;
        int count = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float time = i / (float)SampleRate;
            float t = time / duration;
            float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / 0.025f))
                * Mathf.Pow(1f - t, 1.35f);
            float frequency = Mathf.Lerp(290f, 880f, Mathf.SmoothStep(0f, 1f, t));
            float phase = 2f * Mathf.PI * (frequency * time + 0.65f * t * t);
            float shimmer = Mathf.Sin(phase) + 0.22f * Mathf.Sin(phase * 2.01f);
            samples[i] = shimmer * envelope * 0.42f;
        }

        AudioClip clip = AudioClip.Create("RoomExpandSynth", count, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateEnemyDefeatClip()
    {
        const float duration = 0.15f;
        int count = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float time = i / (float)SampleRate;
            float t = time / duration;
            float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / 0.004f))
                * Mathf.Pow(1f - t, 3.2f);
            float frequency = Mathf.Lerp(720f, 260f, Mathf.Sqrt(t));
            float tone = Mathf.Sin(2f * Mathf.PI * frequency * time);
            float click = Mathf.Sin(2f * Mathf.PI * 1700f * time) * Mathf.Exp(-time * 42f);
            samples[i] = (tone * 0.72f + click * 0.28f) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("EnemyDefeatSynth", count, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

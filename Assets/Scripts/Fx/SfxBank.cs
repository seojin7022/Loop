using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 효과음 재생기. 씬에 하나 올려 두고 Entries 에 id ↔ AudioClip 을 채우면
/// Sfx.EnemyHit(...) 같은 호출이 실제 소리로 바뀐다. 없으면 게임은 그냥 조용히 돌아간다.
///
/// id 는 Sfx.Id 의 상수와 같은 문자열을 쓴다. (EnemyHit, MirrorPlaced, ...)
/// 하나의 id 에 여러 클립을 넣으면 무작위로 골라 재생해 반복감을 줄인다.
/// </summary>
public class SfxBank : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        [Tooltip("Sfx.Id 의 상수와 같은 문자열 (예: EnemyHit)")]
        public string id;

        [Tooltip("여러 개를 넣으면 무작위로 하나를 재생한다.")]
        public List<AudioClip> clips = new();

        [Range(0f, 1f)] public float volume = 1f;

        [Tooltip("재생마다 음정을 이 범위 안에서 무작위로 흔든다.")]
        public Vector2 pitchRange = new(0.94f, 1.06f);

        [Tooltip("같은 소리가 이 시간 안에 겹쳐 나오지 않게 막는다. (초)")]
        public float minInterval = 0.03f;

        [NonSerialized] public float lastPlayedAt = float.NegativeInfinity;
    }

    public static SfxBank Instance { get; private set; }

    [SerializeField] List<Entry> entries = new();

    [Tooltip("전체 효과음 볼륨")]
    [Range(0f, 1f)]
    [SerializeField] float masterVolume = 1f;

    [Tooltip("비워 두면 같은 오브젝트에 AudioSource 를 자동으로 만든다.")]
    [SerializeField] AudioSource source;

    Dictionary<string, Entry> map;

    void Awake()
    {
        Instance = this;

        map = new Dictionary<string, Entry>();
        foreach (Entry entry in entries)
        {
            if (string.IsNullOrEmpty(entry.id)) continue;
            map[entry.id] = entry;
        }

        if (source == null && !TryGetComponent(out source))
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Play(string id, Vector3 position)
    {
        if (map == null || !map.TryGetValue(id, out Entry entry)) return;
        if (entry.clips == null || entry.clips.Count == 0) return;
        if (Time.unscaledTime - entry.lastPlayedAt < entry.minInterval) return;

        AudioClip clip = entry.clips[UnityEngine.Random.Range(0, entry.clips.Count)];
        if (clip == null) return;

        entry.lastPlayedAt = Time.unscaledTime;

        if (source == null) return;

        source.pitch = UnityEngine.Random.Range(entry.pitchRange.x, entry.pitchRange.y);
        source.PlayOneShot(clip, entry.volume * masterVolume);
    }
}

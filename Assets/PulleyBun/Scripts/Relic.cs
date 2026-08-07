using UnityEngine;
using System.Collections.Generic;

namespace PulleyBun
{
    public enum Relic
    {
        MoreMirror,
        BigMirror,
        Duplicate,
        DamageEnhance,
        MirrorSplash,
        MirrorTurret,
    }

    public class RelicManager : MonoBehaviour
    {
        [SerializeField] List<Relic> relics;

        public static RelicManager Instance;

        void Awake()
        {
            Instance = this;
        }

        public void AddRelic(Relic relic)
        {
            relics.Add(relic);
        }

        public void RemoveRelic(Relic relic)
        {
            relics.Remove(relic);
        }

        public bool HasRelic(Relic relic)
        {
            return relics.Contains(relic);
        }
    }
}

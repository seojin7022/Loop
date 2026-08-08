using UnityEngine;
using System;
using System.Collections.Generic;

namespace PulleyBun
{
    // 주의: enum 값 순서를 바꾸면 씬/프리팹에 직렬화된 기존 특성 값이 어긋난다.
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
        /// 활성 하수인 최대 수 (기획서: 거울 개수 ×2와 조합 시 과밀 방지)
        public const int MaxActiveMinions = 5;

        // 게임 밸런스·화면 확인용 전역 스위치. 다시 켜려면 true로 변경한다.
        public static bool IsEnabled = false;

        [SerializeField] List<Relic> relics;

        public static RelicManager Instance;

        /// 지금까지 특성을 선택한 횟수. 선택 풀 규칙(처음 2회 제한)에 사용한다.
        public int SelectionCount { get; private set; }

        public IReadOnlyList<Relic> Relics => relics;

        public event Action<Relic> RelicAdded;
        public event Action<Relic> RelicRemoved;

        void Awake()
        {
            relics ??= new List<Relic>();
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// RelicManager가 없는 씬(테스트 씬 등)에서도 안전하게 쓰기 위한 정적 조회.
        public static bool Has(Relic relic)
        {
            return IsEnabled && Instance != null && Instance.HasRelic(relic);
        }

        public void AddRelic(Relic relic)
        {
            if (relics.Contains(relic) && RelicDatabase.Get(relic)?.Unique == true)
                return;

            relics.Add(relic);
            RelicAdded?.Invoke(relic);
        }

        public void RemoveRelic(Relic relic)
        {
            if (relics.Remove(relic))
                RelicRemoved?.Invoke(relic);
        }

        public bool HasRelic(Relic relic)
        {
            return relics.Contains(relic);
        }

        /// 선택 화면에서 고른 특성을 확정한다.
        public void Choose(Relic relic)
        {
            AddRelic(relic);
            SelectionCount++;
        }

        /// <summary>
        /// 기획서의 선택 풀 규칙에 따라 제시할 특성 후보를 뽑는다.
        /// - 중복 불가 특성은 이미 보유했다면 제외
        /// - 처음 2회 선택에서는 강한 특성이 2개 이상 동시에 등장하지 않음
        /// - 상호 배타 조합(거울 개수 ×2 / 거울 하수인)은 같은 화면에 함께 등장하지 않음
        /// </summary>
        public List<Relic> RollChoices(int count = 3)
        {
            List<RelicInfo> pool = new();
            foreach (RelicInfo info in RelicDatabase.All)
            {
                if (info.Unique && HasRelic(info.Relic)) continue;
                pool.Add(info);
            }

            // Fisher-Yates 셔플
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            bool limitStrong = SelectionCount < 2;

            List<Relic> picked = new();
            bool strongPicked = false;

            foreach (RelicInfo info in pool)
            {
                if (picked.Count >= count) break;
                if (limitStrong && info.Strong && strongPicked) continue;
                if (ConflictsWithPicked(info.Relic, picked)) continue;

                picked.Add(info.Relic);
                if (info.Strong) strongPicked = true;
            }

            // 제약 때문에 개수를 못 채웠다면, 배타 조합만 지키면서 남은 자리를 채운다.
            if (picked.Count < count)
            {
                foreach (RelicInfo info in pool)
                {
                    if (picked.Count >= count) break;
                    if (picked.Contains(info.Relic)) continue;
                    if (ConflictsWithPicked(info.Relic, picked)) continue;

                    picked.Add(info.Relic);
                }
            }

            return picked;
        }

        static bool ConflictsWithPicked(Relic candidate, List<Relic> picked)
        {
            foreach ((Relic a, Relic b) in RelicDatabase.ExclusivePairs)
            {
                if (candidate == a && picked.Contains(b)) return true;
                if (candidate == b && picked.Contains(a)) return true;
            }
            return false;
        }
    }
}

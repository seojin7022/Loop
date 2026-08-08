using UnityEngine;

namespace PulleyBun
{
    /// <summary>
    /// 거울 부착 효과(오라·하수인)를 설치된 거울에 붙이고 떼는 관리자.
    /// - 특성을 새로 얻으면 이미 설치된 거울에도 소급 적용한다.
    /// - 거울을 새로 설치하면 즉시 부착한다.
    /// - 하수인은 RelicManager.MaxActiveMinions 기까지만 활성화한다.
    /// - 거울이 삭제되면 부착물은 같은 GameObject 에 붙어 있으므로 함께 사라진다.
    /// 씬에 수동으로 배치할 필요 없이 자동 생성된다.
    /// </summary>
    public class MirrorAttachments : MonoBehaviour
    {
        public static MirrorAttachments Instance { get; private set; }

        bool cachedAura;
        bool cachedMinion;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;

            var go = new GameObject("@MirrorAttachments");
            go.AddComponent<MirrorAttachments>();
        }

        /// 아직 없으면 만들어서 반환한다. (씬을 다시 로드해도 부착 효과가 살아 있도록)
        public static MirrorAttachments Ensure()
        {
            if (Instance == null) Bootstrap();
            return Instance;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LineMaker.LineCreated += OnLineCreated;
            LineMaker.LineRemoved += OnLineRemoved;
        }

        void OnDestroy()
        {
            LineMaker.LineCreated -= OnLineCreated;
            LineMaker.LineRemoved -= OnLineRemoved;
            if (Instance == this) Instance = null;
        }

        void OnLineCreated(GameObject line) => Reconcile();
        void OnLineRemoved(GameObject line) => Reconcile();

        void Update()
        {
            bool aura = RelicManager.Has(Relic.MirrorSplash);
            bool minion = RelicManager.Has(Relic.MirrorTurret);

            if (aura == cachedAura && minion == cachedMinion) return;

            cachedAura = aura;
            cachedMinion = minion;
            Reconcile();
        }

        public void Reconcile()
        {
            LineMaker maker = LineMaker.Instance;
            if (maker == null) return;

            bool wantAura = RelicManager.Has(Relic.MirrorSplash);
            bool wantMinion = RelicManager.Has(Relic.MirrorTurret);

            int minionsPlaced = 0;

            for (int i = 0; i < maker.Lines.Count; i++)
            {
                GameObject line = maker.Lines[i];
                if (line == null) continue;

                SetComponent<MirrorAura>(line, wantAura);

                bool allowMinion = wantMinion && minionsPlaced < RelicManager.MaxActiveMinions;
                SetComponent<MirrorMinion>(line, allowMinion);
                if (allowMinion) minionsPlaced++;
            }
        }

        static void SetComponent<T>(GameObject target, bool enabled) where T : Component
        {
            T existing = target.GetComponent<T>();

            if (enabled)
            {
                if (existing == null) target.AddComponent<T>();
            }
            else if (existing != null)
            {
                // 부착물이 만든 자식 시각 오브젝트는 각 컴포넌트의 OnDestroy 에서 정리한다.
                Destroy(existing);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace PulleyBun
{
    public enum RelicCategory
    {
        BulletMod,          // 탄환 변형
        PlacementResource,  // 설치 자원
        MirrorAttachment,   // 거울 부착 효과
    }

    public class RelicInfo
    {
        public Relic Relic;
        public string DisplayName;
        public string CategoryLabel;
        public string Description;
        public RelicCategory Category;

        /// Image UI에 넣을 아이콘의 Resources 경로 (확장자 없이). 예: "Relics/Duplicate"
        public string IconPath;

        /// 아이콘 스프라이트. 에셋이 없으면 null이며, 이때 UI는 아이콘 자리를 비운다.
        /// Resources.Load는 한 번 로드된 에셋을 재사용하므로 별도 캐시를 두지 않는다.
        public Sprite Icon =>
            string.IsNullOrEmpty(IconPath) ? null : Resources.Load<Sprite>(IconPath);

        /// 중복 불가: 이미 보유 중이면 선택지에 다시 나오지 않는다.
        public bool Unique = true;

        /// 강한 특성: 처음 2회 선택에서는 같은 화면에 2개 이상 등장하지 않는다.
        public bool Strong;
    }

    /// 기획서의 특성 목록을 코드에서 참조 가능한 형태로 정리한 정적 테이블.
    public static class RelicDatabase
    {
        public static readonly List<RelicInfo> All = new()
        {
            new RelicInfo
            {
                Relic = Relic.Duplicate,
                DisplayName = "번식",
                CategoryLabel = "탄환 변형 / 중복 불가",
                Description = "총알이 거울에 처음 반사될 때 두 갈래로 분열한다.\n분열된 탄환도 정상적으로 반사·피격 판정을 가진다.",
                Category = RelicCategory.BulletMod,
                IconPath = "Relics/Duplicate",
                Unique = true,
                Strong = true,
            },
            new RelicInfo
            {
                Relic = Relic.DamageEnhance,
                DisplayName = "데미지 증가",
                CategoryLabel = "탄환 변형 / 중복 불가",
                Description = "첫 반사 이후 탄환 피해가 1 증가한다.\n이후 반사 횟수와 무관하게 증가한 피해를 유지한다.",
                Category = RelicCategory.BulletMod,
                IconPath = "Relics/DamageEnhance",
                Unique = true,
                Strong = false,
            },
            new RelicInfo
            {
                Relic = Relic.MoreMirror,
                DisplayName = "거울 개수 ×2",
                CategoryLabel = "설치 자원 / 중복 불가",
                Description = "설치 가능한 거울 수가 5개에서 10개로 증가한다.",
                Category = RelicCategory.PlacementResource,
                IconPath = "Relics/MoreMirror",
                Unique = true,
                Strong = false,
            },
            new RelicInfo
            {
                Relic = Relic.BigMirror,
                DisplayName = "거울 최대 크기 ×2",
                CategoryLabel = "설치 자원 / 중복 불가",
                Description = "거울 최대 길이가 2배로 증가한다.\n멀리 있는 레인을 한 번에 덮을 수 있다.",
                Category = RelicCategory.PlacementResource,
                IconPath = "Relics/BigMirror",
                Unique = true,
                Strong = false,
            },
            new RelicInfo
            {
                Relic = Relic.MirrorSplash,
                DisplayName = "거울 오라",
                CategoryLabel = "거울 부착 효과 / 중복 불가",
                Description = "설치된 각 거울 주변 일정 범위의 적에게 초당 1 피해를 준다.\n여러 거울의 오라는 중첩되지 않는다.",
                Category = RelicCategory.MirrorAttachment,
                IconPath = "Relics/MirrorSplash",
                Unique = true,
                Strong = true,
            },
            new RelicInfo
            {
                Relic = Relic.MirrorTurret,
                DisplayName = "거울 하수인 소환",
                CategoryLabel = "거울 부착 효과 / 중복 불가",
                Description = "설치된 각 거울에 하수인 1기를 배치한다.\n하수인은 가까운 적에게 투사체를 발사한다. (최대 5기)",
                Category = RelicCategory.MirrorAttachment,
                IconPath = "Relics/MirrorTurret",
                Unique = true,
                Strong = true,
            },
        };

        /// 같은 선택 화면에 동시에 등장하면 안 되는 조합.
        public static readonly (Relic a, Relic b)[] ExclusivePairs =
        {
            (Relic.MoreMirror, Relic.MirrorTurret),
        };

        public static RelicInfo Get(Relic relic)
        {
            foreach (RelicInfo info in All)
                if (info.Relic == relic)
                    return info;
            return null;
        }

        public static string NameOf(Relic relic) => Get(relic)?.DisplayName ?? relic.ToString();
    }
}

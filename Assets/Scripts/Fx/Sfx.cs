using UnityEngine;

/// <summary>
/// 효과음 호출 지점 모음. 지금은 클립이 없어 아무 소리도 나지 않는다.
///
/// 소리를 붙이려면:
///   1. 씬 아무 오브젝트에 SfxBank 컴포넌트를 추가하고
///   2. Entries 에 아래 Id 상수와 같은 id 로 AudioClip 을 넣으면 된다.
/// 게임 코드는 손댈 필요가 없다.
/// </summary>
public static class Sfx
{
    public static class Id
    {
        public const string EnemyHit = "EnemyHit";
        public const string EnemyDie = "EnemyDie";
        public const string BallBounce = "BallBounce";
        public const string MirrorPlaced = "MirrorPlaced";
        public const string MirrorRemoved = "MirrorRemoved";
        public const string WaveStart = "WaveStart";
        public const string WaveClear = "WaveClear";
        public const string RoomAdded = "RoomAdded";
        public const string PlayerDamage = "PlayerDamage";
        public const string GameOver = "GameOver";
    }

    public static void EnemyHit(Vector3 position) => Play(Id.EnemyHit, position);
    public static void EnemyDie(Vector3 position) => Play(Id.EnemyDie, position);
    public static void BallBounce(Vector3 position) => Play(Id.BallBounce, position);
    public static void MirrorPlaced(Vector3 position) => Play(Id.MirrorPlaced, position);
    public static void MirrorRemoved(Vector3 position) => Play(Id.MirrorRemoved, position);
    public static void WaveStart() => Play(Id.WaveStart, Vector3.zero);
    public static void WaveClear() => Play(Id.WaveClear, Vector3.zero);
    public static void RoomAdded() => Play(Id.RoomAdded, Vector3.zero);
    public static void PlayerDamage(Vector3 position) => Play(Id.PlayerDamage, position);
    public static void GameOver() => Play(Id.GameOver, Vector3.zero);

    /// SfxBank 가 씬에 있으면 재생하고, 없으면 조용히 넘어간다.
    public static void Play(string id, Vector3 position)
    {
        SfxBank bank = SfxBank.Instance;
        if (bank == null) return;

        bank.Play(id, position);
    }
}

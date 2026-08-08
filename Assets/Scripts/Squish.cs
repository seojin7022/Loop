using UnityEngine;
using DG.Tweening;

public class Squish : MonoBehaviour
{
    public float squashAmount = 0.3f;
    public float duration = 0.3f;

    /// <param name="normal">로컬 공간 기준 충돌 법선</param>
    public void DoSquash(Vector2 normal)
    {
        Vector3 squashVector = new Vector3(
            1f - squashAmount,
            1f + squashAmount,
            1f
        );

        transform.DOKill();
        transform.localScale = squashVector;
        transform.DOScale(Vector3.one, duration).SetEase(Ease.OutElastic);
    }
}

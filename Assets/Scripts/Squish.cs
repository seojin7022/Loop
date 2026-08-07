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
            Mathf.Abs(normal.x) * -squashAmount + Mathf.Abs(normal.y) * squashAmount,
            Mathf.Abs(normal.y) * -squashAmount + Mathf.Abs(normal.x) * squashAmount,
            0
        );

        transform.DOKill(true);
        transform.DOPunchScale(squashVector, duration, 5, 0.5f);
    }
}

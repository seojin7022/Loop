using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class Enemy : MonoBehaviour
{
    Vector3 spawn, target;
    float speed;

    public void SetEnemy(Vector3 spawnPos, Vector3 targetPos, float speed)
    {
        spawn = spawnPos;
        target = targetPos;
        this.speed = speed;
        Debug.Log(target);
        Debug.Log(speed);
        MoveToTarget().Forget();
    }

    public async UniTask MoveToTarget()
    {
        var token = this.GetCancellationTokenOnDestroy();

        float xTime = Mathf.Abs(target.x - spawn.x) / speed;
        await transform.DOMoveX(target.x, xTime).SetEase(Ease.Linear).WithCancellation(token);
        
        float yTime = Mathf.Abs(target.y - spawn.y) / speed;
        await transform.DOMoveY(target.y, yTime).SetEase(Ease.Linear).WithCancellation(token);

        Destroy(gameObject);
    }
}

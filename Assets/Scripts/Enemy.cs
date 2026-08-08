using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class Enemy : MonoBehaviour
{
    Vector3 spawn, target;
    float speed, hp;
    bool isDestroyed;

    public void SetEnemy(Vector3 spawnPos, Vector3 targetPos, float speed, float hp)
    {
        spawn = spawnPos;
        target = targetPos;
        this.speed = speed;
        this.hp = hp;
        
        MoveToTarget().Forget();
    }

    public async UniTask MoveToTarget()
    {
        var token = this.GetCancellationTokenOnDestroy();

        float xTime = Mathf.Abs(target.x - spawn.x) / speed;
        await transform.DOMoveX(target.x, xTime).SetEase(Ease.Linear).WithCancellation(token);
        
        float yTime = Mathf.Abs(target.y - spawn.y) / speed;
        await transform.DOMoveY(target.y, yTime).SetEase(Ease.Linear).WithCancellation(token);

        if(isDestroyed) return;
        
        EventBus.Publish("PlayerDamage");
        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out PulleyBun.Ball ball))
        {
            hp -= 1;
            if(hp <= 0)
            {
                EventBus.Publish("EnemyDie");
                isDestroyed = true;
                Destroy(gameObject);
            }
        }
    }
}

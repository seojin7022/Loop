using UnityEngine;

namespace PulleyBun
{
    /// 거울 하수인이 발사하는 투사체. 콜라이더 없이 거리 판정으로 적중시킨다.
    public class MinionProjectile : MonoBehaviour
    {
        Enemy target;
        Vector3 direction;
        float speed;
        float damage;
        float lifetime;
        float hitRadius;

        public static MinionProjectile Spawn(
            Vector3 position, Enemy target, float speed, float damage,
            float lifetime, float hitRadius, float size, Color color)
        {
            var go = new GameObject("MinionProjectile");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * size;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeGfx.Circle;
            renderer.color = color;
            renderer.sortingOrder = 5;

            var projectile = go.AddComponent<MinionProjectile>();
            projectile.target = target;
            projectile.speed = speed;
            projectile.damage = damage;
            projectile.lifetime = lifetime;
            projectile.hitRadius = hitRadius;
            projectile.direction = target != null
                ? (target.transform.position - position).normalized
                : Vector3.right;

            return projectile;
        }

        void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            // 목표가 살아 있으면 가볍게 유도한다.
            if (target != null && target.IsAlive)
                direction = Vector3.Slerp(
                    direction,
                    (target.transform.position - transform.position).normalized,
                    10f * Time.deltaTime);

            transform.position += direction * (speed * Time.deltaTime);

            for (int i = Enemy.All.Count - 1; i >= 0; i--)
            {
                Enemy enemy = Enemy.All[i];
                if (enemy == null || !enemy.IsAlive) continue;

                if (Vector2.Distance(enemy.transform.position, transform.position) <= hitRadius)
                {
                    enemy.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}

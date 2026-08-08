using UnityEngine;

namespace PulleyBun
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] Vector2 velocity;
        int layerMask;
        Squish squish;
        Rigidbody2D rb;

        bool firstBounce = true;
        [SerializeField] float duplicateAngle = 10f;
        [SerializeField] GameObject splashPrefab;

        /// 번식 특성으로 갈라질 때의 각도. 예상 궤적선이 같은 값을 쓰도록 노출한다.
        public float DuplicateAngle => duplicateAngle;

        [Header("Damage")]
        [SerializeField] int baseDamage = 1;

        [Tooltip("데미지 증가 특성 보유 시 첫 반사 이후 더해지는 피해량")]
        [SerializeField] int enhancedBonus = 1;

        /// 이미 한 번이라도 반사했는지 여부. 데미지 증가 특성의 조건.
        bool hasBounced;

        /// 현재 이 탄환이 적에게 주는 피해량.
        public int Damage =>
            baseDamage + (hasBounced && RelicManager.Has(Relic.DamageEnhance) ? enhancedBonus : 0);

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            layerMask = LayerMask.GetMask("Line");
            squish = GetComponent<Squish>();
        }

        public void SetVelocity(Vector2 velocity, bool firstBounce = true, bool hasBounced = false)
        {
            this.velocity = velocity;
            transform.right = velocity.normalized;
            this.firstBounce = firstBounce;
            this.hasBounced = hasBounced;
        }

        void FixedUpdate()
        {
            var move = velocity * Time.fixedDeltaTime;
            var position = transform.position;
            var direction = transform.right;
            var hit = Physics2D.Raycast(position, direction, move.magnitude, layerMask);
            if (hit)
            {
                direction = Vector2.Reflect(direction, hit.normal);
                position = hit.point + (Vector2)direction * (Preview.RayMargin + move.magnitude - hit.distance);
                if (firstBounce)
                {
                    firstBounce = false;
                    if (RelicManager.Has(Relic.Duplicate))
                    {
                        var rotation = Quaternion.Euler(0, 0, duplicateAngle);
                        var newDirection1 = rotation * direction;
                        rotation = Quaternion.Euler(0, 0, -duplicateAngle);
                        var newDirection2 = rotation * direction;

                        direction = newDirection1;
                        var newBall = Instantiate(gameObject, position, Quaternion.identity);
                        // 분열된 탄환도 이미 한 번 반사한 것으로 취급한다 (데미지 증가 적용 대상).
                        newBall.GetComponent<Ball>().SetVelocity(newDirection2 * velocity.magnitude, false, true);
                    }
                }

                // 첫 반사가 완료된 시점부터 데미지 증가 특성이 적용된다.
                hasBounced = true;

                velocity = direction * velocity.magnitude;

                Sfx.BallBounce(hit.point);

                if (RelicManager.Has(Relic.MirrorSplash) && splashPrefab != null)
                {
                    Instantiate(splashPrefab, hit.point, Quaternion.identity);
                }

                // 거울에 끼이는 경우 방지
                var getOutMove = direction * Preview.RayMargin;
                while (hit.collider.OverlapPoint(position))
                {
                    position += getOutMove;
                }
            }
            else
            {
                position += (Vector3)move;
            }
            rb.MovePosition(position);
            transform.right = direction;
            if (hit && squish != null)
            {
                squish.DoSquash(transform.InverseTransformDirection(hit.normal));
            }
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                Destroy(gameObject);
            }
        }
    }
}

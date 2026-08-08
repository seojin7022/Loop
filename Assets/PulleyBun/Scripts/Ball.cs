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

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            layerMask = LayerMask.GetMask("Line");
            squish = GetComponent<Squish>();
        }

        public void SetVelocity(Vector2 velocity, bool firstBounce = true)
        {
            this.velocity = velocity;
            transform.right = velocity.normalized;
            this.firstBounce = firstBounce;
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
                    if (RelicManager.Instance.HasRelic(Relic.Duplicate))
                    {
                        var rotation = Quaternion.Euler(0, 0, duplicateAngle);
                        var newDirection1 = rotation * direction;
                        rotation = Quaternion.Euler(0, 0, -duplicateAngle);
                        var newDirection2 = rotation * direction;

                        direction = newDirection1;
                        var newBall = Instantiate(gameObject, position, Quaternion.identity);
                        newBall.GetComponent<Ball>().SetVelocity(newDirection2 * velocity.magnitude, false);
                    }
                }
                velocity = direction * velocity.magnitude;
                if (RelicManager.Instance.HasRelic(Relic.MirrorSplash))
                {
                    Instantiate(splashPrefab, hit.point, Quaternion.identity);
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
            if(collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                Destroy(gameObject);
            }
        }
    }
}

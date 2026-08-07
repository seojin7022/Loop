using UnityEngine;

namespace PulleyBun
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] Vector2 velocity;
        int layerMask;

        void Awake()
        {
            layerMask = LayerMask.GetMask("Line");
        }

        public void SetVelocity(Vector2 velocity)
        {
            this.velocity = velocity;
            transform.right = velocity.normalized;
        }

        void Update()
        {
            var move = velocity * Time.deltaTime;
            var position = transform.position;
            var direction = transform.right;
            var hit = Physics2D.Raycast(position, direction, move.magnitude, layerMask);
            if (hit)
            {
                direction = Vector2.Reflect(direction, hit.normal);
                position = hit.point + (Vector2)direction * (Preview.RayMargin + move.magnitude - hit.distance);
                velocity = direction * velocity.magnitude;
            }
            else
            {
                position += (Vector3)move;
            }
            transform.position = position;
            transform.right = direction;
        }
    }
}

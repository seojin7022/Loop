using UnityEngine;

namespace PulleyBun
{
    public class Preview : MonoBehaviour
    {
        public static readonly float RayMargin = 0.01f;

        [SerializeField] float predictDepth = 60f;
        [SerializeField] float predictLength = 100f;
        [SerializeField] LineRenderer lineRenderer;

        void Update()
        {
            lineRenderer.SetPosition(0, transform.position);
            var position = transform.position;
            var direction = transform.right;
            for (int i = 1; i < predictDepth; i++)
            {
                lineRenderer.positionCount = i + 1;
                var hit = Physics2D.Raycast(position, direction, predictLength);
                if (hit)
                {
                    lineRenderer.SetPosition(i, hit.point);
                    direction = Vector2.Reflect(direction, hit.normal);
                    position = hit.point + (Vector2)direction * RayMargin;
                }
                else
                {
                    lineRenderer.SetPosition(i, position + direction * predictLength);
                    break;
                }
            }
        }
    }
}

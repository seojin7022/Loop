using UnityEngine;

namespace PulleyBun
{
    public class BallMaker : MonoBehaviour
    {
        [SerializeField] Ball ballPrefab;
        [SerializeField] float speed;
        [SerializeField] float spawnInterval = 1f;
        float spawnTimer;

        void Update()
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                var ball = Instantiate(ballPrefab, transform.position, transform.rotation);
                ball.SetVelocity(transform.right * speed);
            }
        }
    }
}
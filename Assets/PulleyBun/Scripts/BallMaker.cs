using UnityEngine;

namespace PulleyBun
{
    public class BallMaker : MonoBehaviour
    {
        [SerializeField] Ball ballPrefab;

        /// 예상 궤적선이 실제 탄환 설정(분열 각도 등)을 따라가도록 노출한다.
        public Ball BallPrefab => ballPrefab;

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
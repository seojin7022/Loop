using UnityEngine;

namespace PulleyBun
{
    public class Splash : MonoBehaviour
    {
        [SerializeField] float duration = 0.5f;
        float maxDuration;
        
        void Start()
        {
            maxDuration = duration;
        }
        void Update()
        {
            duration -= Time.deltaTime;
            if (duration <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                transform.localScale = Vector3.one * duration / maxDuration;
            }
        }
    }
}

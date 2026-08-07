using UnityEngine;
using DG.Tweening;

public class Squish : MonoBehaviour
{
    Tweener tween;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        tween = transform.DOPunchScale(new Vector3(0.3f, -0.3f, 0), 0.3f, 5, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public HitGlitter hitGlitter;
    // Start is called before the first frame update
    void Awake()
    {
        if (hitGlitter != null)
            hitGlitter.Init();
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void TakeDamage(float damageValue)
    {
        if (this.hitGlitter != null)
        {
            this.hitGlitter.PlayHitGlitter(false);
        }
    }
}

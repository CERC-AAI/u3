using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody bulletRigidbody;
    public float bulletSpeed=10;
    // Start is called before the first frame update
    private void Awake()
    {
        Vector3 startScale = this.transform.localScale;
        bulletRigidbody = GetComponent<Rigidbody>();
        LeanTween.scale(this.gameObject, Vector3.zero, 0);
        LeanTween.scale(this.gameObject, startScale, 0.1f);
    }

    private void Start()
    {
        bulletRigidbody.velocity = transform.forward * bulletSpeed;
        Invoke("DestoryAfterTime", 10f);
    }



    void DestoryAfterTime()
    {
        Destroy(gameObject);
    }

}

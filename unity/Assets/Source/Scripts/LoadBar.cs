using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadBar : MonoBehaviour
{
    public SpriteRenderer loadBar;

    float maxSize;

    public void Awake()
    {
        maxSize = loadBar.transform.localScale.x;
    }

    public void SetPercent(float percent)
    {
        Vector3 localScale = loadBar.transform.localScale;

        localScale.x = maxSize * percent;
        loadBar.transform.localScale = localScale;
    }
}

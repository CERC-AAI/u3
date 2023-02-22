using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsObject : EnvironmentComponent
{
    public SpriteRenderer mSprite;

    protected override void Init()
    {
        mSprite = GetComponentInChildren<SpriteRenderer>();

        base.Init();
    }

    public void SetFacing(Vector3 facing)
    {
        if (mSprite)
        {
            if (facing != Vector3.zero)
            {
                float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

                mSprite.transform.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    public void SetTint(Color color)
    {
        if (mSprite)
        {
            mSprite.color = color;
        }
    }
}

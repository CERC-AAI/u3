using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteGraphicsObject : GraphicsObject
{
    public Transform mFacingRoot;

    SpriteRenderer[] mSprites;

    protected override void Initialize()
    {
        base.Initialize();

        mSprites = GetComponentsInChildren<SpriteRenderer>();

        Movement movement = GetComponent<Movement>();
        if (movement)
        {
            RegisterCallback<Vector3>(ref movement.OnVelocityChangedCallbacks, OnVelocityChanged);
        }
    }

    void OnVelocityChanged(Vector3 targetFacing)
    {
        SetFacing(targetFacing);
    }

    public void SetFacing(Vector3 facing)
    {
        if (facing != Vector3.zero)
        {
            float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

            if (mFacingRoot)
            {
                mFacingRoot.transform.localRotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                for (int i = 0; i < mSprites.Length; i++)
                {
                    mSprites[i].transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }
    }

    override public void SetTint(Color color)
    {
        for (int i = 0; i < mSprites.Length; i++)
        {
            mSprites[i].color = color;
        }
    }
}

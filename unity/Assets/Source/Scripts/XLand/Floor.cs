﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Floor : MonoBehaviour
{
    public int radiusX = 4;
    public int radiusY = 4;
    float _radiusX = 0;
    float _radiusY = 0;

    public float edgeSize = 0.25f;

    public Transform floor;
    public Transform top;
    public Transform bottom;
    public Transform left;
    public Transform right;

    public Material material;


    void Update()
    {
        float finalX = _radiusX;
        float finalY = _radiusY;

        if (finalX == 0)
        {
            finalX = radiusX;
        }
        if (finalY == 0)
        {
            finalY = radiusY;
        }

        floor.localScale = new Vector3(finalX / 15.0f, 1, finalY / 16.0f);
        left.localPosition = new Vector3(finalY + edgeSize/2, 0, 0);
        left.localScale = new Vector3(edgeSize, 0.02f, finalX * 2 + edgeSize * 2);
        right.localPosition = new Vector3(-finalY - edgeSize / 2, 0, 0);
        right.localScale = new Vector3(edgeSize, 0.02f, finalX * 2 + edgeSize * 2);
        top.localPosition = new Vector3(0, 0, finalX + edgeSize / 2);
        top.localScale = new Vector3(edgeSize, 0.02f, finalY * 2 + edgeSize * 2);
        bottom.localPosition = new Vector3(0, 0, -finalX - edgeSize / 2);
        bottom.localScale = new Vector3(edgeSize, 0.02f, finalY * 2 + edgeSize * 2);

        material.mainTextureScale = new Vector2(finalX / 15.0f, finalY / 16.0f);
        transform.localPosition = new Vector3(finalY - 0.5f, -0.5f, finalX - 0.5f);

    }

    public void SetSize(Vector2 size)
    {
        _radiusX = size.x / 2;
        _radiusY = size.y / 2;
    }
}

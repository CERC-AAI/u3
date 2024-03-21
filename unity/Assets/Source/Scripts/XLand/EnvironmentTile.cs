using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentTile : MonoBehaviour
{
    Dictionary<int, Color> ColorMap = new Dictionary<int, Color> {
                {0, new Color(200 / 256.0f, 200 / 256.0f, 200 / 256.0f) },
                {1, new Color(243 / 256.0f, 154 / 256.0f, 39 / 256.0f) },
                {2, new Color(3 / 256.0f, 192 / 256.0f, 60 / 256.0f) },
                {3, new Color(151 / 256.0f, 110 / 256.0f, 215 / 256.0f) },
                {4, new Color(194 / 256.0f, 59 / 256.0f, 35 / 256.0f) },
                {5, new Color(234 / 256.0f, 218 / 256.0f, 82 / 256.0f) },
                {6, new Color(87 / 256.0f, 154 / 256.0f, 190 / 256.0f) },
                {7, new Color(40 / 256.0f, 40 / 256.0f, 40 / 256.0f) },
                };


    void Start()
    {       
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            //Debug.Log(transform.position);
            //renderer.material.SetColor("Albedo", Color.blue);

            int positionY = Mathf.CeilToInt(transform.position.y);

            renderer.material.color = ColorMap[positionY % ColorMap.Count];
        }
    }
}

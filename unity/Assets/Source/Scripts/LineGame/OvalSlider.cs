using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class OvalSlider : MonoBehaviour, IPointerDownHandler
{
    public CueCard parent;
    public float width = 50.0f;
    public float height = 50.0f;
    public Image background;
    public Image handle;

    float mValue;
    bool mIsDragging = false;
    bool mIsEnabled = true;

    public void Update()
    {
        Vector2 localPosition = Input.mousePosition - background.transform.position;

        float phase = Mathf.Atan2(localPosition.y / height , localPosition.x / width) / (2 * Mathf.PI);

        if (mIsDragging && mIsEnabled && Input.GetMouseButton(0))
        {
            Vector2 phasePosition = new Vector2(Mathf.Cos(phase * 2 * Mathf.PI), Mathf.Sin(phase * 2 * Mathf.PI));

            Vector2 newPosition = phasePosition.normalized * new Vector2(width, height);
            handle.transform.localPosition = new Vector3(newPosition.x, newPosition.y, 0);
            handle.gameObject.SetActive(true);

            mValue = phase;

            parent.OnSelectSlider();
        }
        else
        {
            mIsDragging = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (mIsEnabled)
        {
            mIsDragging = true;
        }
    }

    public void init(float phase)
    {
        background.transform.localScale = new Vector3(width / 50.0f, height / 50.0f, 1.0f);
        handle.gameObject.SetActive(false);

        if (phase >= 0)
        {
            Vector2 phasePosition = new Vector2(Mathf.Cos(phase * 2 * Mathf.PI), Mathf.Sin(phase * 2 * Mathf.PI));

            Vector2 newPosition = phasePosition.normalized * new Vector2(width, height);
            handle.transform.localPosition = new Vector3(newPosition.x, newPosition.y, 0);
            handle.gameObject.SetActive(true);
        }
    }

    public float GetValue()
    {
        return mValue;
    }

    public void SetEnabled(bool enabled)
    {
        mIsEnabled = enabled;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineSlider : MonoBehaviour, IPointerDownHandler
{
    public CueCard parent;
    public float width = 50.0f;
    public Image background;
    public Image handle;
    public Transform tickMark;

    float mValue;
    bool mIsDragging = false;
    bool mIsEnabled = true;
    List<Transform> mTickMarks = new List<Transform>();

    public void Update()
    {
        Vector2 localPosition = Input.mousePosition - background.transform.position;

        float relativePosition = localPosition.x / (width * 2) + 0.5f;
        relativePosition = Mathf.Clamp01(relativePosition);

        if (mIsDragging && mIsEnabled && Input.GetMouseButton(0))
        {
            float newPosition = (relativePosition - 0.5f) * width * 2;
            handle.transform.localPosition = new Vector3(newPosition, 0, 0);
            handle.gameObject.SetActive(true);

            mValue = relativePosition; ;

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

    public void init(float value)
    {
        background.transform.localScale = new Vector3(width / 50.0f, 1.0f, 1.0f);
        handle.gameObject.SetActive(false);

        if (value >= 0)
        {
            value = Mathf.Clamp01(value);

            float newPosition = (value - 0.5f) * width * 2;
            handle.transform.localPosition = new Vector3(newPosition, 0, 0);
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

    public void SetWidth(float length)
    {
        width = length;

        background.transform.localScale = new Vector3(width / 50.0f, 1.0f, 1.0f);
    }

    public void AddTick(float tickPosition)
    {
        float newPosition = (tickPosition - 0.5f) * 100;

        Transform thisTick = Instantiate(tickMark).transform;
        mTickMarks.Add(thisTick);

        thisTick.parent = background.transform;
        thisTick.localPosition = new Vector3(newPosition, 0);
    }

    public void ClearTicks()
    {
        for (int i = 0; i < mTickMarks.Count; i++)
        {
            Destroy(mTickMarks[i].gameObject);
        }

        mTickMarks.Clear();
    }
}
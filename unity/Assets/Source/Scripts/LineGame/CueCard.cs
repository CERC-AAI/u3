using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CueCard : EnvironmentComponent
{
    // Public members
    GameObject target;
    public OvalSlider ovalSlider;
    public LineSlider lineSlider;
    public Slider slider;
    public Image handle;
    public UnityEngine.UI.Button okay;
    public UnityEngine.UI.Button skip;
    public bool useOval = false;

    float mResponse;

    bool mIsSelected = false;
    bool mUseSubmitButton = false;

    public void setTargetValue(float value)
    {
        if (useOval && ovalSlider)
        {
            ovalSlider.init(value);
            ovalSlider.SetEnabled(false);
        }
        else if (slider)
        {
            slider.value = value;
            handle.color = Color.white;
            slider.interactable = false;

            Vector3 tempPosition = slider.transform.localPosition;
            tempPosition.x = 0;
            slider.transform.localPosition = tempPosition;
        }
        else if (lineSlider)
        {
            lineSlider.init(value);
            lineSlider.SetEnabled(false);
        }

        okay.gameObject.SetActive(false);
        skip.gameObject.SetActive(false);

    }

    public void makeReponse(float offset, bool useSubmitButton = false)
    {
        if (useOval && ovalSlider)
        {
            ovalSlider.init(-1.0f);

            Vector3 tempPosition = ovalSlider.transform.localPosition;
            tempPosition.x = offset;
            ovalSlider.transform.localPosition = tempPosition;

            ovalSlider.SetEnabled(true);
        }
        else if (slider)
        {
            handle.color = Color.clear;
            slider.interactable = true;

            Vector3 tempPosition = slider.transform.localPosition;
            tempPosition.x = offset;
            slider.transform.localPosition = tempPosition;
        }
        else if (lineSlider)
        {
            lineSlider.init(-1.0f);

            Vector3 tempPosition = lineSlider.transform.localPosition;
            tempPosition.x = offset;
            lineSlider.transform.localPosition = tempPosition;

            lineSlider.SetEnabled(true);
        }

        okay.gameObject.SetActive(true);
        okay.interactable = false;

        mUseSubmitButton = useSubmitButton;
        if (!useSubmitButton)
        {
            okay.gameObject.SetActive(false);
        }

        skip.gameObject.SetActive(true);
        skip.interactable = true;

        mResponse = -1;
        mIsSelected = false;
    }

    public void disableSkip()
    {
        skip.interactable = false;
    }

    public void OnSelectSlider()
    {
        if (slider)
        {
            handle.color = Color.white;
        }

        okay.interactable = true;

        mIsSelected = true;
    }

    private void Update()
    {
        if (mIsSelected)
        {
            if (Input.GetMouseButtonUp(0) && !mUseSubmitButton)
            {
                OnClickButton();
            }
        }
    }

    public void OnClickButton()
    {
        if (useOval && ovalSlider)
        {
            mResponse = ovalSlider.GetValue();
        }
        else if (slider)
        {
            mResponse = slider.value;
        }
        else if (lineSlider)
        {
            mResponse = lineSlider.GetValue();
        }

        mEngine.AddGameEvent("Response", new JSONObject(mResponse));
    }

    public void OnClickMissed()
    {
        mEngine.AddGameEvent("Missed", null);
    }

    public float getResponse()
    {
        return mResponse;
    }

    public void SetLength(float length)
    {
        if (lineSlider)
        {
            lineSlider.SetWidth(length);
        }
    }

    public void SetTickMarks(float[] ticks)
    {
        for (int i = 0; i < ticks.Length; i++)
        {
            if (lineSlider)
            {
                lineSlider.AddTick(ticks[i]);
            }
        }
    }

    public void RemoveCard()
    {
        gameObject.SetActive(false);

        if (lineSlider)
        {
            lineSlider.ClearTicks();
        }
    }

    public void SetOval(bool isOval)
    {
        useOval = isOval;

        if (!useOval || !ovalSlider)
        {
            ovalSlider.gameObject.SetActive(false);
            if (lineSlider)
            {
                lineSlider.gameObject.SetActive(true);
            }
            if (slider)
            {
                slider.gameObject.SetActive(true);
            }
        }
        else
        {
            if (ovalSlider)
            {
                ovalSlider.gameObject.SetActive(true);
            }
            if (lineSlider)
            {
                lineSlider.gameObject.SetActive(false);
            }
            if (slider)
            {
                slider.gameObject.SetActive(false);
            }
        }
    }
}
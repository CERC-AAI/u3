using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InstructionCard : EnvironmentComponent
{
    // Public members
    public Text instructionText;
    public UnityEngine.UI.Button okay;

    Vector3 mInitialStartText;

    int mInstructionStage = 0;

    public void Start()
    {
        mInitialStartText = instructionText.transform.localPosition;
    }

    public void setRoundTimer(int roundNumber)
    {
        mInstructionStage = 0;

        if (roundNumber == 0)
        {
            instructionText.text = "Instructions";
            okay.interactable = false;
        }
        else
        {
            instructionText.text = "Get ready! ";
            instructionText.transform.localPosition = new Vector3(0, -150, 0);
            okay.interactable = false;
        }
    }

    public void setStage1(bool isLine, string overrideText = "")
    {
        instructionText.transform.localPosition = mInitialStartText;

        if (overrideText != "")
        {
            instructionText.text = overrideText;
        }
        else if (isLine)
        {
            instructionText.text = "You must remember the position of the slider on the line.\nClick anywhere to continue. (Next time this will auto-advance)\nYour cursor has been hidden, please don't use your finger.";
        }
        else
        {
            instructionText.text = "You must remember the position of the dot on the oval.\nClick anywhere to continue. (Next time this will auto-advance)\nYour cursor has been hidden, please don't use your finger.";
        }
        okay.interactable = false;

        mInstructionStage = 1;

        StartCoroutine("doStage1");
    }

    public IEnumerator doStage1()
    {
        yield return new WaitForSeconds(0.5f);
        okay.interactable = true;
    }

    public void setStage2(bool isLine, string overrideText = "")
    {
        if (overrideText != "")
        {
            instructionText.text = overrideText;
        }
        else
        {
            instructionText.text = "You will wait for a few seconds before being asked to respond.";
        }
        okay.interactable = false;

        mInstructionStage = 2;

        StartCoroutine("doStage2");
    }

    public IEnumerator doStage2()
    {
        yield return new WaitForSeconds(3.0f);

        mEngine.AddGameEvent("Stage2");
    }

    public void setStage3(bool isLine, string overrideText = "")
    {
        if (overrideText != "")
        {
            instructionText.text = overrideText;
        }
        else if (isLine)
        {
            instructionText.text = "Click or drag the slider to the original position.\nThe closer the better!\nIf you're not sure press skip for no penalty.";
        }
        else
        {
            instructionText.text = "Click or drag the dot to the original position.\nThe closer the better!\nIf you're not sure press skip for no penalty.";
        }
        okay.interactable = false;

        mInstructionStage = 3;
    }

    public void setNewBlock(float currentError)
    {
        instructionText.text = "Good job!\n Click anywhere to start another 10 trials.";
        okay.interactable = false;

        mInstructionStage = 0;

        StartCoroutine("doNewBlock");
    }

    public IEnumerator doNewBlock()
    {
        yield return new WaitForSeconds(0.5f);
        okay.interactable = true;
    }

    public void OnClick()
    {
        Debug.Log("On click");

        if (mInstructionStage == 1)
        {
            mEngine.AddGameEvent("Stage1");
        }
        else if (mInstructionStage == 0)
        {
            mEngine.AddGameEvent("Start");
            RemoveCard();
        }
    }

    public void RemoveCard()
    {
        gameObject.SetActive(false);
    }
}
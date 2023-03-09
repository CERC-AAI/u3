using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents.Sensors;
using System.Text.RegularExpressions;

/*public class LineEnvironment : BaseEnvironment
{
    //float[] mDelays = { 1.0f, 4.0f, 8.0f };
    float[] mDelays = { 0.0f, 3.0f };
    float[] mDisplayTimes = { 1.5f };
    float[] mTickMarks = { };
    float[] mMeans;

    public CueCard cueCardPrefab;
    public CueCard responseCardPrefab;
    public InstructionCard instructionCardPrefab;

    public int totalTrials = 10;
    public bool showInstructions = false;
    public float std = 0.125f;

    CueCard mCueCard;
    CueCard mReponseCard;
    InstructionCard mInstructionCard;

    float mCurrentValue = 0;
    float mOffset = 0;
    float mDisplayTime = 0;
    float mDelayTime = 0;

    int mLevelInstance = 0;
    int mRoundTimer = 0;
    int mTotalBlocks = 0;

    bool mIsOval = false;

    bool mIsFirstBlock = true;

    float mResponseAppearTime = 0;

    bool mIsLine = true;

    float mStartValue = 0.1f;
    float mEndValue = 0.9f;

    float mLength = 100.0f;

    bool mRandomOffset = false;
    bool mCusorVisible = true;
    bool mUseSubmitButton = false;
    bool mUseBlockDelays = false;

    bool mShouldStart = false;

    string mInstructionText1 = "";
    string mInstructionText2 = "";
    string mInstructionText3 = "";


    System.Random mRandom = new System.Random();

    List<int> mDelayIndices = new List<int>();
    int mCurrentDelay = 0;

    override protected void Initialize()
    {
        base.Initialize();

        mIsLine = cueCardPrefab.slider != null || cueCardPrefab.lineSlider != null;

        mMeans = new float[8];

        for (int i = 0; i < mMeans.Length; i++)
        {
            mMeans[i] = (mStartValue + (mEndValue - mStartValue) / (mMeans.Length - 1) * i) + (float)NextGaussianDouble(mRandom) * 0.03f;
        }

        if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
        {
            mIsRunning = false;
        }

        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;
    }

    public override void Update()
    {
        if (mShouldStart)
        {
            LoadEpisode();

            mShouldStart = false;
        }

        base.Update();
    }

    public override void LoadEpisode()
    {
        base.LoadEpisode();

        if (mRoundTimer > 0)
        {
            mRoundTimer = 1;
        }

        TrialInitialize(true);

        //SetParams(new JSONObject("{\"length\":\"200\"}"));
        //SetParams(new JSONObject("{\"ticks\":\"0.0,1.0\"}"));
        //SetParams(new JSONObject("{\"randomOffset\":\"false\"}"));
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = mCusorVisible;
    }

    IEnumerator RunLevel()
    {
        int thisLevel = mLevelInstance;

        if (!mRandomOffset)
        {
            Cursor.visible = false;
            mCusorVisible = false;
        }

        mInstructionCard = LoadObject(instructionCardPrefab.gameObject, Vector3.zero).GetComponent<InstructionCard>();
        mInstructionCard.gameObject.transform.localPosition = Vector3.zero;
        mInstructionCard.setRoundTimer(mRoundTimer);

        yield return new WaitForSeconds(0.75f);

        if (thisLevel == mLevelInstance)
        {
            DrawCueCard();

            if (mRoundTimer == 0)
            {
                if (!mRandomOffset)
                {
                    Cursor.visible = false;
                    mCusorVisible = false;
                }

                mInstructionCard.setStage1(mIsLine, mInstructionText1);
                mInstructionCard.transform.SetAsLastSibling();
            }
            else
            {
                mInstructionCard.RemoveCard();
            }
        }

        if (mRoundTimer > 0)
        {
            if (thisLevel == mLevelInstance)
            {
                int displayIndex = GetEngine().GetRandomRange(0, mDisplayTimes.Length);

                mDisplayTime = mDisplayTimes[displayIndex];

                if (!mUseBlockDelays || mRoundTimer == 1)
                {
                    int delayIndex = 0;

                    if (mUseBlockDelays)
                    {
                        delayIndex = mDelayIndices[mCurrentDelay % mDelayIndices.Count];
                        mCurrentDelay++;
                    }
                    else
                    {
                        delayIndex = GetEngine().GetRandomRange(0, mDelays.Length);
                    }

                    mDelayTime = mDelays[delayIndex];
                }
            }

            yield return new WaitForSeconds(mDisplayTime);

            if (thisLevel == mLevelInstance)
            {
                mCueCard.RemoveCard();
            }

            yield return new WaitForSeconds(mDelayTime);

            if (thisLevel == mLevelInstance)
            {
                DrawResponseCard();
            }
        }
    }

    void DrawCueCard()
    {
        mCueCard = LoadObject(cueCardPrefab.gameObject, Vector3.zero).GetComponent<CueCard>();
        mCueCard.SetOval(mIsOval);
        mCueCard.SetLength(mLength);
        mCueCard.SetTickMarks(mTickMarks);

        if (mRoundTimer == 0)
        {
            mCueCard.gameObject.transform.localPosition = new Vector3(0, -50, 0);
        }
        else
        {
            mCueCard.gameObject.transform.localPosition = Vector3.zero;
        }

        mCueCard.setTargetValue(mCurrentValue);
    }

    void DrawResponseCard()
    {
        mReponseCard = LoadObject(responseCardPrefab.gameObject, Vector3.zero).GetComponent<CueCard>();
        mReponseCard.SetOval(mIsOval);
        mReponseCard.SetLength(mLength);
        mReponseCard.SetTickMarks(mTickMarks);

        if (mRoundTimer == 0)
        {
            mReponseCard.gameObject.transform.localPosition = new Vector3(0, -50, 0);
        }
        else
        {
            mReponseCard.gameObject.transform.localPosition = Vector3.zero;
        }

        if (mRandomOffset)
        {
            mOffset = GetEngine().GetRandomRange(-200, 200);
        }
        else
        {
            mOffset = 0;
            Cursor.visible = true;
            mCusorVisible = true;
        }

        mReponseCard.makeReponse(mOffset, mUseSubmitButton);

        mResponseAppearTime = Time.time;
    }

    public static double NextGaussianDouble(System.Random r)
    {
        double u, v, S;

        do
        {
            u = 2.0 * r.NextDouble() - 1.0;
            v = 2.0 * r.NextDouble() - 1.0;
            S = u * u + v * v;
        }
        while (S >= 1.0);

        double fac = Math.Sqrt(-2.0 * Math.Log(S) / S);
        return u * fac;
    }

    public void TrialInitialize(bool isNewBlock)
    {
        //mCurrentValue = mMeans[UnityEngine.Random.Range(0, mMeans.Length)];
        mCurrentValue = mStartValue + GetEngine().GetRandomFloat() * (mEndValue - mStartValue);
        if (mCurrentValue < 0)
        {
            mCurrentValue += 1;
        }
        if (mCurrentValue > 1)
        {
            mCurrentValue -= 1;
        }
        //mCurrentValue = UnityEngine.Random.value;
        //mCurrentValue = Mathf.Clamp01(mCurrentValue);

        if (mRoundTimer == 0 && !showInstructions)
        {
            mRoundTimer++;
        }

        if (mRoundTimer == 0 || !isNewBlock || mIsFirstBlock)
        {
            mLevelInstance++;
            StartCoroutine("RunLevel");
        }
        else
        {
            mInstructionCard = LoadObject(instructionCardPrefab.gameObject, Vector3.zero).GetComponent<InstructionCard>();
            mInstructionCard.gameObject.transform.localPosition = Vector3.zero;
            mInstructionCard.setNewBlock(0);

            mTotalBlocks++;
            JavascriptInterface.SendEventToJavascript("OnBlockOver", mTotalBlocks.ToString());
        }
    }

    public void NextTrial(bool hasResult)
    {
        mReponseCard.RemoveCard();
        mInstructionCard.RemoveCard();

        if (hasResult)
        {
            float thisResponse = mReponseCard.getResponse();

            float responseTime = Time.time - mResponseAppearTime;

            if (mRoundTimer > 0)
            {
                AddReward(1.0f - Mathf.Abs(thisResponse - mCurrentValue));

                mEngine.AddCustomEvent("Round" + mRoundTimer + "/Offset", mOffset.ToString());
                mEngine.AddCustomEvent("Round" + mRoundTimer + "/DelayTime", mDelayTime.ToString());
                mEngine.AddCustomEvent("Round" + mRoundTimer + "/DisplayTime", mDisplayTime.ToString());
                mEngine.AddCustomEvent("Round" + mRoundTimer + "/ResponseTime", responseTime.ToString());
                mEngine.AddCustomEvent("Round" + mRoundTimer + "/ResponseValue", thisResponse.ToString());
                mEngine.AddCustomEvent("Round" + mRoundTimer + "/TargetValue", mCurrentValue.ToString());
            }

            mRoundTimer++;
        }

        if (mRoundTimer <= totalTrials)
        {
            TrialInitialize(mRoundTimer == 0);
        }
        else
        {
            mEngine.Victory();
        }
    }

    public void RandomizeDelays()
    {
        mDelayIndices.Clear();

        for (int i = 0; i < mDelays.Length; i++)
        {
            mDelayIndices.Add(i);
        }

        int n = mDelayIndices.Count;
        while (n > 1)
        {
            n--;
            int k = mRandom.Next(n + 1);
            int value = mDelayIndices[k];
            mDelayIndices[k] = mDelayIndices[n];
            mDelayIndices[n] = value;
        }
    }

    public override void Victory()
    {
        mIsFirstBlock = false;
        base.Victory();
    }

    public override bool IsValidAction(float[] vectorAction)
    {

        return true;
    }

    public override void Heuristic(float[] actionsOut)
    {
    }

    public override ObservationSpec GetObservationSpec()
    {
        return ObservationSpec.Vector(1);
    }

    public override int Write(ObservationWriter writer)
    {
        writer[0] = 0;

        return 1;
    }

    public override void OnEventSent(string eventString, EnvironmentComponent component)
    {
        switch (eventString)
        {
            case "Stage1":
                mCueCard.RemoveCard();
                mInstructionCard.setStage2(mIsLine, mInstructionText2);
                break;

            case "Stage2":
                DrawResponseCard();
                mInstructionCard.setStage3(mIsLine, mInstructionText3);
                mReponseCard.disableSkip();
                break;

            case "Response":
                NextTrial(true);
                break;

            case "Missed":
                NextTrial(false);
                break;

            case "Start":
                mLevelInstance++;
                StartCoroutine("RunLevel");
                break;
        }
    }

    public override void RunCommand(string command)
    {
        switch (command)
        {
            case "test":
                Debug.Log("Test");
                break;

            case "nointro":
                showInstructions = false;
                break;

            case "short":
                totalTrials = 1;
                break;

            case "start":
                mShouldStart = true;
                break;
        }
    }

    public override void SetParam(string key, string value)
    {
        switch (key.ToLower())
        {
            case "oval":
                bool boolValue = bool.Parse(value);

                mIsOval = boolValue;

                if (mIsOval)
                {
                    mStartValue = 0.0f;
                    mEndValue = 1.0f;
                }
                break;

            case "randomoffset":
                boolValue = bool.Parse(value);

                mRandomOffset = boolValue;
                break;

            case "length":
                int length = int.Parse(value);

                mLength = length;
                break;

            case "delays":
                string[] values = value.Split(',');

                mDelays = new float[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    float delay = float.Parse(values[i]);

                    mDelays[i] = delay;
                }

                RandomizeDelays();
                break;

            case "displays":
                values = value.Split(',');

                mDisplayTimes = new float[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    float display = float.Parse(values[i]);

                    mDisplayTimes[i] = display;
                }
                break;

            case "ticks":
                values = value.Split(',');

                mTickMarks = new float[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    float display = float.Parse(values[i]);

                    mTickMarks[i] = display;
                }
                break;

            case "instructions1":
                mInstructionText1 = value;
                break;

            case "instructions2":
                mInstructionText2 = value;
                break;

            case "instructions3":
                mInstructionText3 = value;
                break;

            case "submitbutton":
                boolValue = bool.Parse(value);

                mUseSubmitButton = boolValue;
                break;

            case "randomblocks":
                boolValue = bool.Parse(value);

                mUseBlockDelays = boolValue;
                break;

        }
    }
}*/

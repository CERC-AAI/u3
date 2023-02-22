using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using UnityEngine.Networking;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(EnvironmentEngine))]
[RequireComponent(typeof(EnvironmentAgent))]
[RequireComponent(typeof(EnvironmentSensorComponent))]
public abstract class EnvironmentBrain : Brain, ISensor
{
    EnvironmentParameters mResetParams;
    EnvironmentAgent mAgent;
    bool mAwaitingResponse = false;
    bool mQueueRestart = false;
    int mTotalLoads = 0;
    public bool forceSaveData = false;

    protected JSONObject mGameEvents = new JSONObject();
    bool mLevelIsFresh = false;
    bool mIsLevelReset = true;
    protected bool mStatic = false;
    protected int mSeedValue = -1;
    protected bool mIsTraining = false;

    bool mSizeWarning = false;
    protected bool mIsRunning = true;

    bool mIsParalyzed = false;

    string mID = "";

    Dictionary<string, string> mEnvironmentElements = new Dictionary<string, string>();


    protected EnvironmentSideChannel mSideChannel;


    override protected void Init()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;

        CheckAgent();
        CheckSideChannel();

        base.Init();
    }

    public void CheckAgent()
    {
        if (mAgent == null)
        {
            mAgent = GetComponent<EnvironmentAgent>();
        }
    }

    public void CheckSideChannel()
    {
        if (mSideChannel == null)
        {
            mSideChannel = new EnvironmentSideChannel(this);

            Unity.MLAgents.SideChannels.SideChannelsManager.RegisterSideChannel(mSideChannel);
        }
    }
    public void OnDestroy()
    {
        // De-register the Debug.Log callback
        if (Academy.IsInitialized)
        {
            Unity.MLAgents.SideChannels.SideChannelsManager.UnregisterSideChannel(mSideChannel);
        }
    }

    override public void OnUpdate(bool isEndOfTurn)
    {
        if (mQueueRestart)
        {
            mQueueRestart = false;

            LoadLevel();
        }

        if (isEndOfTurn)
        {
            if (!mAwaitingResponse)
            {
                mAwaitingResponse = true;
                mAgent.RequestDecision();

                //Debug.Log(Time.time + " RequestDecision");

                Academy.Instance.EnvironmentStep();
            }
        }

        base.OnUpdate(isEndOfTurn);
    }

    public virtual void Initialize()
    {
        mResetParams = Academy.Instance.EnvironmentParameters;
    }

    public virtual void CheckEnvironmentElement(string elementName, string elementData = "")
    {
        if (mEnvironmentElements.ContainsKey(elementName) && elementData == "")
        {
            LoadEnvironmentElement(elementName, mEnvironmentElements[elementName]);
        }
        else
        {
            LoadEnvironmentElement(elementName, elementData);
        }

        if (!mEnvironmentElements.ContainsKey(elementName) || elementData != "")
        {
            mEnvironmentElements[elementName] = elementData;
        }
    }

    public virtual void LoadEnvironmentElement(string elementName, string elementData = "")
    {
        switch (elementName)
        {
            default:
                Debug.Log("Invalid element: " + elementName + " loaded with string: " + elementData);
                break;
        }
    }

    public virtual void SetEnvironmentElement(string elementName, string elementData = "")
    {
        mEnvironmentElements[elementName.ToLower()] = elementData;
    }

    public virtual void ClearEnvironmentElements()
    {
        mEnvironmentElements.Clear();
    }

    public virtual void RunCommand(string command)
    {

    }

    public void SetID(string id)
    {
        mID = id;
    }

    public virtual void OnEnvironmentActionReceived(float[] vectorAction)
    {
        //Debug.Log(Time.time + " OnEnvironmentActionReceived: " + vectorAction[0]);

        mEngine.OnEnvironmentActionRecieved(vectorAction, !mIsParalyzed);

        mAwaitingResponse = false;
    }

    public virtual void OnTurnEnd()
    {
        mLevelIsFresh = false;

        mEngine.IncrementTime();
    }

    public virtual bool HasNonZeroInput(float[] vectorAction)
    {
        bool hasNonZero = false;
        for (int i = 0; i < vectorAction.Length; i++)
        {
            if (vectorAction[0] != 0)
            {
                hasNonZero = true;
            }
        }

        if (hasNonZero)
        {
            return true;
        }

        return false;
    }

    public virtual void HadCollision(EnvironmentObject object1, EnvironmentObject object2)
    {
    }

    public void SetReward(float reward)
    {
        mAgent.SetReward(reward);
    }

    public void AddReward(float reward)
    {
        //Debug.Log("Add reward: " + reward);

        mAgent.AddReward(reward);
    }

    public void EndEpisode()
    {
        mAgent.EndEpisode();
    }

    public abstract void Heuristic(float[] actionsOut);

    // to be implemented by the developer
    public virtual void OnEpisodeBegin()
    {
        AddGameEvent("Timed Out", "");
        SendData();

        ResetLevel();
    }

    public virtual void OnInstantiate(EnvironmentObject newObject)
    {

    }

    public virtual void OnObjectMoved(EnvironmentObject thisObject, Vector3 oldPosition, Vector3 newPosition)
    {

    }

   /* public virtual void OnObjectStopped(EnvironmentObject movedObject)
    {

    }*/

    public void GameOver()
    {
        if (!mIsLevelReset)
        {
            OnGameOver();
        }
    }

    virtual protected void OnGameOver()
    {
        EndEpisode();
    }

    virtual protected void ResetLevel()
    {
        mIsLevelReset = true;

        RestartEnvironment();
    }

    virtual public void Victory()
    {
        GameOver();
    }

    virtual public void Defeat()
    {
        GameOver();
    }

    virtual public void OnObjectLoaded(EnvironmentObject movedObject, Color loadValue)
    {
    }

    virtual public void LoadLevel()
    {
        mIsRunning = true;
        mIsLevelReset = false;
        mLevelIsFresh = true;
        mTotalLoads++;

        if (mTotalLoads % 100 == 0)
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        mGameEvents.Purge();

        if (ShouldSaveData())
        {
            AddGameEvent("InitialState", GetInitialStateInfo());
        }
    }

    protected void RestartEnvironment()
    {
        mQueueRestart = true;
    }

    public void SendData()
    {
        if (ShouldSaveData() && mGameEvents && mGameEvents.HasField("Inputs"))
        {
            if (mID == "")
            {
                mGameEvents.AddField("ID", DateTime.Now.ToString());
            }
            else
            {
                mGameEvents.AddField("ID", mID);
            }

            JavascriptInterface.SendEventToJavascript("PostData", mGameEvents.ToString());

            PostData(mGameEvents.ToString());
            mGameEvents = new JSONObject();
        }
    }

    public void PostData(string jsonString)
    {
        if (IsPython())
        {
            CheckSideChannel();

            mSideChannel.SendEventToPython("data" + jsonString);
        }

        if (!IsPython() || forceSaveData)
        {
            StartCoroutine(UploadJson(jsonString));
        }
    }

    IEnumerator UploadJson(string jsonString)
    {
        WWWForm form = new WWWForm();
        form.AddField("json", jsonString);

        using (UnityWebRequest www = UnityWebRequest.Post("json.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Complete: " + www.downloadHandler.text);
            }
        }
    }

    override protected void BuildRunStateJSON(JSONObject root)
    {
        //root["paralyzed"] = new JSONObject(mIsParalyzed);
        //root["static"] = new JSONObject(mStatic);

        base.BuildRunStateJSON(root);
    }

    override protected void LoadRunStateJSON(JSONObject root)
    {
        /*if (root.keys.Contains("paralyzed"))
        {
            mIsParalyzed = root["paralyzed"].b;
        }

        if (root.keys.Contains("static"))
        {
            mIsParalyzed = root["static"].b;
        }*/

        base.BuildRunStateJSON(root);
    }

    public void AddGameCustom(string eventName, string eventValue)
    {
        if (ShouldSaveData())
        {
            AddGameCustom(eventName, new Dictionary<string, string>() { { "value", eventValue } });
        }
    }

    public void AddGameCustom(string eventName, Dictionary<string, string> eventValues)
    {
        if (ShouldSaveData())
        {
            string[] categories = eventName.Split('/');

            if (!mGameEvents.HasField("Custom"))
            {
                mGameEvents.AddField("Custom", new JSONObject());
            }

            JSONObject mCurrentJSON = mGameEvents["Custom"];
            for (int i = 0; i < categories.Length - 1; i++)
            {
                if (!mCurrentJSON.HasField(categories[0]))
                {
                    mCurrentJSON.AddField(categories[0], new JSONObject());
                }

                mCurrentJSON = mCurrentJSON[categories[0]];
            }

            Dictionary<string, string> eventData = new Dictionary<string, string>();
            if (eventValues.Count == 1)
            {
                string key = categories[categories.Length - 1];
                foreach (KeyValuePair<string, string> pair in eventValues)
                {
                    eventData[key] = pair.Value;
                }

                mCurrentJSON.Add(new JSONObject(eventData));
            }
            else
            {
                foreach (KeyValuePair<string, string> pair in eventValues)
                {
                    if (pair.Key != "")
                    {
                        eventData[pair.Key] = pair.Value;
                    }
                }

                mCurrentJSON.Add(new JSONObject(eventData));
            }
        }
    }

    public void AddGameEvent(string eventName, string eventValue)
    {
        if (ShouldSaveData())
        {
            AddGameEvent(eventName, new Dictionary<string, string>() { { "value", eventValue } });
        }
    }

    public void AddGameEvent(string eventName, Dictionary<string, string> eventValues)
    {
        if (ShouldSaveData())
        {
            if (!mGameEvents.HasField("Events"))
            {
                mGameEvents.AddField("Events", new JSONObject());
            }

            Dictionary<string, string> eventData = new Dictionary<string, string>();
            eventData["id"] = eventName;
            eventData["time"] = Time.unscaledTime.ToString();
            foreach (KeyValuePair<string, string> pair in eventValues)
            {
                if (pair.Key != "")
                {
                    eventData[pair.Key] = pair.Value;
                }
            }

            mGameEvents["Events"].Add(new JSONObject(eventData));
        }
    }

    public void AddGameInput(string inputValue)
    {
        if (ShouldSaveData())
        {
            if (!mGameEvents.HasField("Inputs"))
            {
                mGameEvents.AddField("Inputs", new JSONObject());
            }

            if (inputValue != "")
            {
                Dictionary<string, string> eventData = new Dictionary<string, string>();
                eventData["time"] = Time.unscaledTime.ToString();
                eventData["value"] = inputValue;

                mGameEvents["Inputs"].Add(new JSONObject(eventData));
            }
        }
    }

    public bool IsPython()
    {
        return Academy.Instance.IsCommunicatorOn;
    }

    public bool IsExperiment()
    {
        return IsPython() && !mIsTraining;
    }

    public bool IsTesting()
    {
        return !IsPython() && Application.isEditor;
    }

    public virtual Dictionary<string, string> GetInitialStateInfo()
    {
        Dictionary<string, string> intialStatInfo = new Dictionary<string, string>();

        return intialStatInfo;
    }



    // Sensor implimentation

    public virtual string GetName()
    {
        return "EnvironmentSensor";
    }

    public virtual int[] GetObservationShape()
    {
        if (!mSizeWarning)
        {
            mSizeWarning = true;

            Debug.LogError("You must specify the observation size in your environment!");
        }

        return new int[1];
    }

    public virtual byte[] GetCompressedObservation()
    {
        return null;
    }

    public virtual int Write(ObservationWriter writer)
    {
        return 0;
    }

    public virtual void Update()
    {
    }

    public virtual void Reset()
    {
        //Debug.Log("Reset");
        mAwaitingResponse = false;
    }

    public virtual SensorCompressionType GetCompressionType()
    {
        return SensorCompressionType.None;
    }

    public bool ShouldSaveData()
    {
        return forceSaveData || (!IsPython() && !Application.isEditor) || (IsPython() && !mIsTraining);
    }

    public void Seed(int seedValue)
    {
        Debug.Log("Set seed:" + seedValue);

        UnityEngine.Random.InitState(seedValue);
        mSeedValue = seedValue;
    }

    public void SetStatic(bool isStatic)
    {
        Debug.Log("Set static:" + isStatic);

        mStatic = isStatic;
    }

    public void SetPythonTraining(bool isTraining)
    {
        Debug.Log("Set training:" + isTraining);

        mIsTraining = isTraining;
    }

    public virtual void OnEventSent(string eventString, EnvironmentComponent component)
    {

    }

    public bool isRunning()
    {
        return mIsRunning;
    }

    public void SetParams(JSONObject paramData)
    {
        //Debug.Log("SetParams: " + paramData);

        if (paramData.IsArray)
        {
            for (int i = 0; i < paramData.list.Count; i++)
            {
                SetParam(paramData.list[i]);
            }
        }
        else
        {
            SetParam(paramData);
        }
    }

    public void SetParam(JSONObject paramData)
    {
        //Debug.Log("SetParam: " + paramData);

        List<string> allKeys = paramData.keys;

        if (allKeys.Count != 1)
        {
            Debug.LogError("Parameters must be a key value pair, or a list of key value pairs.");
            return;
        }

        string value = "";

        paramData.GetField(ref value, allKeys[0]);

        AddGameCustom("SetParam" + allKeys[0], value);

        //Debug.Log(allKeys[0] + ": " + value);

        SetParam(allKeys[0], value);
    }

    public virtual void SetParam(string key, string value)
    {
        switch (key.ToLower())
        {
            case "paralyze":
                bool boolValue = bool.Parse(value);

                mIsParalyzed = boolValue;
                break;
        }
    }

    public void SendChannelMessage(string jsonString)
    {
        if (IsPython())
        {
            CheckSideChannel();

            mSideChannel.SendEventToPython("message" + jsonString);
        }

        JavascriptInterface.SendEventToJavascript("Message", jsonString);
    }
}

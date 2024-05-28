using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.SideChannels;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Linq;

public class EnvironmentManager : MonoBehaviour
{
    [Serializable]
    public class SceneDefinition
    {
        public string environmentID;
        public string sceneName;
        public LocalPhysicsMode physicsType = LocalPhysicsMode.Physics3D;
    }

    //public bool debugMode = true;
    public bool forceSaveData = false;
    public List<SceneDefinition> scenes = new List<SceneDefinition>();

    bool mIsTraining = false;
    Dictionary<string, string> mSceneIDs = new Dictionary<string, string>();
    Dictionary<string, LocalPhysicsMode> mPhysicsModes = new Dictionary<string, LocalPhysicsMode>();
    EnvironmentSideChannel mSideChannel;
    Dictionary<int, EnvironmentEngine> mEnvironments = new Dictionary<int, EnvironmentEngine>();
    int mNextEnvironmentID = 1;
    bool mIsInitialized = false;

    List<EnvironmentEngine> mDecisionRequests = new List<EnvironmentEngine>();
    List<EnvironmentEngine> mPhysicsRequests = new List<EnvironmentEngine>();

    Dictionary<EnvironmentEngine, JSONObject> mLoadParams = new Dictionary<EnvironmentEngine, JSONObject>();

    Dictionary<Scene, (JSONObject, bool)> mLoadingScenes = new Dictionary<Scene, (JSONObject, bool)>();

    public static EnvironmentManager Instance { get; private set; }

    public float mUpdateTime;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            if (Application.isPlaying && !Application.isEditor)
            {
                Destroy(this);
                return;
            }
        }
        else
        {
            Instance = this;

            if (Application.isPlaying)
            {
                Academy.Instance.AutomaticSteppingEnabled = false;
            }
        }

        if (IsPython())
        {
            Debug.Log("Python");
        }
        else
        {
            Debug.Log("Not python");
        }

        Physics.autoSimulation = false;
        Physics2D.simulationMode = SimulationMode2D.Script;

        Physics.autoSyncTransforms = false;
        Physics2D.autoSyncTransforms = false;

        foreach (SceneDefinition scene in scenes)
        {
            mPhysicsModes[scene.sceneName] = scene.physicsType;
            mSceneIDs[scene.environmentID] = scene.sceneName;
        }

        if (!IsPython())
        {
            GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < objects.Length; i++)
            {
                EnvironmentEngine engine = objects[i].GetComponent<EnvironmentEngine>();

                if (engine != null)
                {
                    engine.SetPhysicsEngine(SceneManager.GetActiveScene().GetPhysicsScene());
                    engine.SetPhysicsEngine2D(SceneManager.GetActiveScene().GetPhysicsScene2D());

                    AddEnvironment(engine);

                    break;
                }
            }
        }
    }

    void Start()
    {
        mIsInitialized = false;

        if (!IsPython() && mEnvironments.ContainsKey(1))
        {
            mEnvironments[1].CheckInitialized();
        }

        CheckSideChannel();
    }

    void Update()
    {
        if (!mIsInitialized)
        {
            mIsInitialized = true;

            if (!IsPython() && mEnvironments.ContainsKey(1))
            {
                mEnvironments[1].InitializeEnvironment(null);
                mEnvironments[1].StartRun();
            }
        }

        if (mDecisionRequests.Count > 0)
        {
            InputSystem.Update();

            Academy.Instance.EnvironmentStep();

            //Debug.Log(InputSystem.metrics.totalEventCount);
        }

        Scene[] keys = mLoadingScenes.Keys.ToArray();
        foreach (Scene key in keys)
        {
            if (key.isLoaded)
            {
                (JSONObject, bool) value = mLoadingScenes[key];

                mLoadingScenes.Remove(key);

                GameObject[] objects = key.GetRootGameObjects();
                for (int i = 0; i < objects.Length; i++)
                {
                    EnvironmentEngine engine = objects[i].GetComponent<EnvironmentEngine>();

                    if (engine != null)
                    {
                        engine.SetPhysicsEngine(key.GetPhysicsScene());
                        engine.SetPhysicsEngine2D(key.GetPhysicsScene2D());

                        AddEnvironment(engine);

                        engine.InitializeEnvironment(value.Item1);
                        mLoadParams[engine] = value.Item1;

                        if (!value.Item2)
                        {
                            engine.StartRun();
                        }

                        break;
                    }
                }
            }
        }

        if (!IsPython())
        {
            foreach (var pair in mEnvironments)
            {
                if (!pair.Value.IsRunning())
                {
                    pair.Value.StartRun();
                }
            }
        }

        float startTime = Time.realtimeSinceStartup;
        foreach (var pair in mEnvironments)
        {
            pair.Value.DoUpdate();
        }

        if (mPhysicsRequests.Count > 0)
        {
            Physics.SyncTransforms();
            Physics2D.SyncTransforms();

            mPhysicsRequests.Clear();
        }

        foreach (var pair in mEnvironments)
        {
            if (pair.Value.WaitingForPhysics())
            {
                pair.Value.CompleteFixedUpdate();
            }
        }
        mUpdateTime = Time.realtimeSinceStartup - startTime;
    }

    public void OnRunEnded(EnvironmentEngine environment)
    {

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

    public bool ShouldSaveData()
    {
        return forceSaveData || (!IsPython() && !Application.isEditor) || (IsPython() && !mIsTraining);
    }

    public void SetPythonTraining(bool isTraining)
    {
        Debug.Log("Set training:" + isTraining);

        mIsTraining = isTraining;
    }

    public void AddEnvironment(EnvironmentEngine environment)
    {
        if (!mEnvironments.ContainsValue(environment))
        {
            mEnvironments[mNextEnvironmentID] = environment;

            mNextEnvironmentID++;
        }
    }

    public void RemoveEnvironment(EnvironmentEngine environment)
    {
        foreach (var pair in mEnvironments)
        {
            if (pair.Value == environment)
            {
                mEnvironments.Remove(pair.Key);
                break;
            }
        }
    }

    public EnvironmentEngine InitializeEnvironment(string environmentName, JSONObject jsonParams = null, bool shouldIdle = false)
    {
        // TODO fix physics.XX errors to allow for multiple scenes at once
        EnvironmentEngine engine = null;

        GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < objects.Length; i++)
        {
            engine = objects[i].GetComponent<EnvironmentEngine>();

            if (engine != null)
            {
                engine.SetPhysicsEngine(SceneManager.GetActiveScene().GetPhysicsScene());
                engine.SetPhysicsEngine2D(SceneManager.GetActiveScene().GetPhysicsScene2D());

                AddEnvironment(engine);

                engine.InitializeEnvironment(jsonParams);
                mLoadParams[engine] = jsonParams;

                if (!shouldIdle)
                {
                    engine.StartRun();
                }

                break;
            }
        }


        /*if (mSceneIDs.ContainsKey(environmentName))
        {
            environmentName = mSceneIDs[environmentName];
        }

        LocalPhysicsMode physicsMode = LocalPhysicsMode.Physics3D;
        if (mPhysicsModes.ContainsKey(environmentName))
        {
            physicsMode = mPhysicsModes[environmentName];
        }

        try
        {
            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Additive, physicsMode);
            Scene scene = SceneManager.LoadScene(environmentName, parameters);

            mLoadingScenes[scene] = (jsonParams, shouldIdle);
        }
        catch (Exception e)
        {
            Debug.Log("Could not load scene: " + environmentName + ". Error: " + e.Message);
        }*/

        return engine;
    }

    public void OnDestroy()
    {
        // De-register the Debug.Log callback
        if (Academy.IsInitialized)
        {
            Unity.MLAgents.SideChannels.SideChannelManager.UnregisterSideChannel(mSideChannel);
        }
    }

    //Step logic

    //This is needed to make sure that physics objects are synced before calling OnLateFixedUpdate()
    public void QueueFixedUpdate(EnvironmentEngine environment)
    {
        if (!mPhysicsRequests.Contains(environment))
        {
            mPhysicsRequests.Add(environment);
        }
    }

    public void RequestedDecision(EnvironmentEngine environment)
    {
        if (!mDecisionRequests.Contains(environment))
        {
            mDecisionRequests.Add(environment);
        }
    }

    //Logging

    public void CheckSideChannel()
    {
        if (mSideChannel == null)
        {
            mSideChannel = new EnvironmentSideChannel();

            Unity.MLAgents.SideChannels.SideChannelManager.RegisterSideChannel(mSideChannel);
        }
    }
    public void OnMessageReceived(IncomingMessage msg)
    {
        string receivedString = msg.ReadString();
        JSONObject messageJSON = new JSONObject(receivedString);
        if (!messageJSON.IsNull)
        {
            OnMessageReceived(messageJSON);
        }
    }


    public void OnMessageReceived(string msg)
    {
        JSONObject messageJSON = new JSONObject(msg);
        Debug.Log(messageJSON.ToString());
    }

    public void OnMessageReceived(JSONObject messageJSON)
    {
        Debug.Log(messageJSON.ToString());
        int environmentID = 0;
        if (messageJSON["env"])
        {
            environmentID = (int)messageJSON["env"].i;
        }

        EnvironmentEngine messageEnvironment;
        if (mEnvironments.ContainsKey(environmentID))
        {
            messageEnvironment = mEnvironments[environmentID];
        }
        else
        {
            if (messageJSON["msg"].str == "init")
            {
                InitializeEnvironment(messageJSON["env"].str, messageJSON["data"]);
            }
            else
            {
                Debug.Log("Received invalid env ID: " + environmentID + ", for command: " + messageJSON.ToString());
            }

            return;
        }

        //Debug.Log(receivedString);

        switch (messageJSON["msg"].str)
        {
            case "do":
                messageEnvironment.DoAction(messageJSON["data"]);
                break;

            case "seed":
                messageEnvironment.SetSeed((int)messageJSON["data"].i);
                break;

            case "property":
                messageEnvironment.SetProperty(messageJSON["data"]);
                break;

            case "training":
                messageEnvironment.SetPythonTraining(messageJSON["data"].b);
                break;

            case "reset":
                float startTime = Time.realtimeSinceStartup;
                if (messageEnvironment.IsRunning())
                {
                    messageEnvironment.EndRun();
                }

                JSONObject loadParams = mLoadParams[messageEnvironment];

                JSONObject resetParams = messageJSON["data"];
                if (resetParams)
                {
                    foreach (string paramName in resetParams.keys)
                    {
                        if (loadParams.GetField(paramName) == null)
                        {
                            loadParams.AddField(paramName, resetParams[paramName]);
                        }
                        else
                        {
                            loadParams.SetField(paramName, resetParams[paramName]);
                        }
                    }
                }

                messageEnvironment.InitializeEnvironment(loadParams);
                messageEnvironment.StartRun();

                Debug.Log($"Restart time: {Time.realtimeSinceStartup - startTime}");
                break;

                /*case "element":
                    messageEnvironment.SetEnvironmentElement(messageJSON["data"]);
                    break;

                case "reset":
                    messageEnvironment.ClearEnvironmentElements();
                    break;*/
        }
        /*else if (receivedString.Substring(0, 8) == "getstate")
        {
            mBrain.SendChannelMessage("state" + mBrain.BuildRunStateToJSON().ToString());
        }

        else if (receivedString.Substring(0, 3) == "get")
        {
            string getString = receivedString.Substring(3);

            JSONObject tempJSON = mBrain.BuildRunStateToJSON();

            if (tempJSON.keys.Contains(getString))
            {
                mBrain.SendChannelMessage("get" + tempJSON[getString].ToString());
            }
        }
        else if (receivedString.Substring(0, 9) == "loadstate")
        {
            string loadStateString = receivedString.Substring(9);

            mBrain.LoadRunStateFromJSON(new JSONObject(loadStateString));
        }*/
    }

    public void SendEventToPython(string eventText)
    {
        mSideChannel.SendEventToPython(eventText);
    }

    [DllImport("__Internal")] private static extern void SendMessageToJavascript(string function, string arguments);

    public void SendEventToJavascript(string function, string aruguments)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SendMessageToJavascript(function, aruguments);
        }
    }

    public void SendChannelMessage(string jsonString)
    {
        if (IsPython())
        {
            CheckSideChannel();

            mSideChannel.SendEventToPython("message" + jsonString);
        }

        SendEventToJavascript("Message", jsonString);
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
}

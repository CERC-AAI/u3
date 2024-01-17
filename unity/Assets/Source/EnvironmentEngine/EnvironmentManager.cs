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

public class EnvironmentManager : MonoBehaviour
{
    [Serializable]
    public class SceneDefinition
    {
        public string environmentID;
        public string sceneName;
        public LocalPhysicsMode physicsType = LocalPhysicsMode.Physics3D;
    }

    public bool debugMode = true;
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

    public static EnvironmentManager Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            if (Application.isPlaying && !Application.isEditor)
            {
                Destroy(this);
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

        if (debugMode == true)
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

        if (debugMode && mEnvironments.ContainsKey(1))
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

            if (debugMode && mEnvironments.ContainsKey(1))
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

        if (debugMode)
        {
            foreach (var pair in mEnvironments)
            {
                if (!pair.Value.IsRunning())
                {
                    pair.Value.StartRun();
                }
            }
        }

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

    public void InitializeEnvironment(string environmentName, JSONObject jsonParams = null, bool shouldIdle = false)
    {
        if (mSceneIDs.ContainsKey(environmentName))
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

            GameObject[] objects = scene.GetRootGameObjects();
            for (int i = 0; i < objects.Length; i++)
            {
                EnvironmentEngine engine = objects[i].GetComponent<EnvironmentEngine>();

                if (engine != null)
                {
                    engine.SetPhysicsEngine(scene.GetPhysicsScene());
                    engine.SetPhysicsEngine2D(scene.GetPhysicsScene2D());

                    AddEnvironment(engine);

                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Could not load scene: " + environmentName + ". Error: " + e.Message);
        }
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
    }


    public void OnMessageReceived(string msg)
    {
        JSONObject messageJSON = new JSONObject(msg);
    }

    public void OnMessageReceived(JSONObject messageJSON)
    {
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
            Debug.Log("Recieved invalid env ID: " + environmentID + ", for command: " + messageJSON.ToString());
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

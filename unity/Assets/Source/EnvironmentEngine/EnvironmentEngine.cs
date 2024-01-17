using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Unity.MLAgents;


public class EnvironmentEngine : EnvironmentComponentHolder
{
    // Public members
    [Tooltip("Root transform for where to store deleted objects.")]
    public Transform recycleBin;
    [Tooltip("Number of fixed updates per second.")]
    [Range(1, 100)]
    public int fixedUpdatesPerSecond = 10;
    [Tooltip("Toggles whether to run the environment in training mode (Physics step every frame, and less graphics).")]
    public bool isTraining = false;

    // Private members
    protected List<EnvironmentObject> mEnvironmentObjects = new List<EnvironmentObject>();
    List<EnvironmentAgent> mActiveAgents = new List<EnvironmentAgent>();
    List<EnvironmentAgent> mDecisionRequests = new List<EnvironmentAgent>();
    List<EnvironmentAgent> mBlockingAgents = new List<EnvironmentAgent>();
    List<EnvironmentAgent> mInactiveAgents = new List<EnvironmentAgent>();

    int mNextID = 1;
    protected long mFixedTime = 0;
    protected double mEnvironmentTimer = 0;
    protected double mNextFixedUpdate = 0;
    long mLastDecisionTime = -1;
    long mLastRequestTime = -1;
    //Stores the delta time of the last update step before the physics queue occured
    float mPhysicsQueuedUpdateDelta = 0;

    bool mIsRunning = false;
    bool mIsWaitingForActions = false;

    PhysicsScene mPhysicsScene;
    PhysicsScene2D mPhysicsScene2D;

    System.Random mRandom;

    Dictionary<GameObject, List<EnvironmentObject>> mUnusedObjects = new Dictionary<GameObject, List<EnvironmentObject>>();


    protected JSONObject mGameEvents = new JSONObject();

    override protected void Initialize()
    {
        SetSeed(System.Environment.TickCount);

        mIsRunning = false;

        EnvironmentObject[] allObjects = GetComponentsInChildren<EnvironmentObject>();

        for (int i = 0; i < allObjects.Length; i++)
        {
            if (!mEnvironmentObjects.Contains(allObjects[i]))
            {
                AddObject(allObjects[i]);
                //mEnvironmentObjects.Add(allObjects[i]);
            }
        }

        base.Initialize();
    }

    virtual public void InitializeEnvironment(JSONObject loadParams)
    {

        CheckInitialized();
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            mEnvironmentObjects[i].CheckInitialized();
        }
    }

    public void StartRun()
    {
        mEnvironmentTimer = 0;
        mNextFixedUpdate = 0;
        mFixedTime = 0;
        mIsRunning = true;
        mIsWaitingForActions = true;
        mLastDecisionTime = -1;
        mLastRequestTime = -1;
        mPhysicsQueuedUpdateDelta = 0;

        for (int i = 0; i < mInactiveAgents.Count; i++)
        {
            mActiveAgents.Add(mInactiveAgents[i]);
        }
        mInactiveAgents.Clear();

        RunStarted();

        Debug.Log("Start run");
        CheckDecisions();
    }

    override public void RunStarted()
    {
        OnRunStarted();
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            mEnvironmentObjects[i].RunStarted();
        }

    }

    override public void RunEnded()
    {
        mIsRunning = false;

        OnRunEnded();
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            mEnvironmentObjects[i].RunEnded();
        }

        EnvironmentManager.Instance.OnRunEnded(this);
    }

    public void CheckDecisions()
    {
        if (mFixedTime != mLastRequestTime)
        {
            StepStarted();

            mLastRequestTime = mFixedTime;
        }

        if (mBlockingAgents.Count == 0)
        {
            mDecisionRequests.Clear();
            mBlockingAgents.Clear();

            for (int i = 0; i < mActiveAgents.Count; i++)
            {
                if (mActiveAgents[i].ShouldRequestDecision(mFixedTime))
                {
                    mActiveAgents[i].RequestDecision();

                    mDecisionRequests.Add(mActiveAgents[i]);
                }
            }
        }
        else
        {
            //Only request decisions from blocking agents
            mDecisionRequests.Clear();

            for (int i = 0; i < mBlockingAgents.Count; i++)
            {
                if (mBlockingAgents[i].ShouldRequestDecision(mFixedTime))
                {
                    mBlockingAgents[i].RequestDecision();

                    mDecisionRequests.Add(mBlockingAgents[i]);
                }
            }

            mBlockingAgents.Clear();
        }

        if (mDecisionRequests.Count == 0)
        {
            //No agents require decisions, continue the simulation
            mIsWaitingForActions = false;
        }
        else
        {
            //Hand the step off to ML Agents
            mIsWaitingForActions = true;
            EnvironmentManager.Instance.RequestedDecision(this);

            if (mFixedTime != mLastDecisionTime)
            {
                OnDecisionRequested();
            }

            mLastDecisionTime = mFixedTime;
        }
    }

    virtual public void OnDecisionRequested()
    {

    }

    public void OnAgentActionReceived(EnvironmentAgent agent, bool shouldBlock = false)
    {
        if (shouldBlock)
        {
            if (mDecisionRequests.Contains(agent))
            {
                mDecisionRequests.Remove(agent);
            }

            if (!mBlockingAgents.Contains(agent))
            {
                mBlockingAgents.Add(agent);
            }
        }
        else
        {
            if (mDecisionRequests.Contains(agent))
            {
                mDecisionRequests.Remove(agent);
            }
        }

        if (mDecisionRequests.Count == 0)
        {
            if (mBlockingAgents.Count == 0)
            {
                //No agents require decisions, continue the simulation
                StepEnded();
            }
            else
            {
                //Requery blocking agents
                CheckDecisions();
            }
        }
    }

    override public void StepEnded()
    {
        mIsWaitingForActions = false;

        base.StepEnded();
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            mEnvironmentObjects[i].StepEnded();
        }
    }

    override public void StepStarted()
    {
        mIsWaitingForActions = false;

        base.StepStarted();
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            mEnvironmentObjects[i].StepStarted();
        }
    }

    /*public void OnDecisionRequested(EnvironmentAgent agent)
    {
        if (!mDecisionRequests.Contains(agent))
        {
            mDecisionRequests.Add(agent);
        }
    }*/

    public float GetFixedDeltaTime()
    {
        return 1.0f / (float)Mathf.Max(1, fixedUpdatesPerSecond);
    }

    public void DoUpdate()
    {
        if (!WaitingForPhysics())
        {
            float deltaTime = 0;

            if (mIsRunning && !mIsWaitingForActions)
            {
                //Run a fixed update every frame.
                if (isTraining || fixedUpdatesPerSecond == 0)
                {
                    deltaTime = GetFixedDeltaTime();
                }
                else
                {
                    deltaTime = Time.deltaTime;
                }

                mEnvironmentTimer += deltaTime;
            }

            if (mEnvironmentTimer >= mNextFixedUpdate)
            {
                if (mLastDecisionTime != mFixedTime)
                {
                    CheckDecisions();
                }
            }

            if (!mIsWaitingForActions)
            {
                OnObjectUpdate(deltaTime);
                for (int i = 0; i < mEnvironmentObjects.Count; i++)
                {
                    mEnvironmentObjects[i].OnObjectUpdate(deltaTime);
                }

                if (mEnvironmentTimer >= mNextFixedUpdate)
                {

                    DoFixedUpdate(deltaTime);
                }

                if (!WaitingForPhysics())
                {
                    //Fix the code here to be local to each environment
                    //KinematicCharacterController.KinematicCharacterSystem.LateUpdate();
                    OnObjectLateUpdate(deltaTime);
                    for (int i = 0; i < mEnvironmentObjects.Count; i++)
                    {
                        mEnvironmentObjects[i].OnObjectLateUpdate(deltaTime);
                    }
                }
            }
        }

        //Debug.Log(JsonUtility.ToJson(this, true));
    }

    public float GetTime()
    {
        return (float)mEnvironmentTimer;
    }

    public void DoFixedUpdate(float deltaTime)
    {
        if (mIsRunning && !mIsWaitingForActions)
        {
            OnObjectFixedUpdate(GetFixedDeltaTime());
            for (int i = 0; i < mEnvironmentObjects.Count; i++)
            {
                mEnvironmentObjects[i].OnObjectFixedUpdate(GetFixedDeltaTime());
            }

            mPhysicsScene.Simulate(GetFixedDeltaTime());
            mPhysicsScene2D.Simulate(GetFixedDeltaTime());

            EnvironmentManager.Instance.QueueFixedUpdate(this);
            mPhysicsQueuedUpdateDelta = deltaTime;
        }
    }

    public bool WaitingForPhysics()
    {
        return mPhysicsQueuedUpdateDelta != 0;
    }

    public void CompleteFixedUpdate()
    {
        OnObjectLateFixedUpdate(GetFixedDeltaTime());
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            mEnvironmentObjects[i].OnObjectLateFixedUpdate(GetFixedDeltaTime());
        }

        OnObjectLateUpdate(mPhysicsQueuedUpdateDelta);
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            mEnvironmentObjects[i].OnObjectLateUpdate(mPhysicsQueuedUpdateDelta);
        }

        OnFinalUpdate(mPhysicsQueuedUpdateDelta);

        mPhysicsQueuedUpdateDelta = 0;

        IncrementSimulation();
    }

    virtual protected void OnFinalUpdate(float deltaTime)
    {
    }

    protected void IncrementSimulation()
    {
        mFixedTime++;
        mNextFixedUpdate += GetFixedDeltaTime();
    }

    override public void OnObjectUpdate(float deltaTime)
    {
        base.OnObjectUpdate(deltaTime);
    }

    override public void OnObjectFixedUpdate(float fixedDeltaTime)
    {
        base.OnObjectFixedUpdate(fixedDeltaTime);
    }

    override public void OnObjectLateFixedUpdate(float fixedDeltaTime)
    {
        base.OnObjectLateFixedUpdate(fixedDeltaTime);
    }

    virtual public void OnTaskEpisodeStarted(EnvironmentTask task)
    {

    }

    /*public virtual void OnEnvironmentActionRecieved(float[] vectorAction, bool shouldMove)
    {
        bool shouldEndTurn = false;

        if (shouldMove)
        {
            for (int i = 0; i < mEnvironmentObjects.Count; i++)
            {
                if (mEnvironmentObjects[i].OnObjectActionRecieved(vectorAction))
                {
                    shouldEndTurn = true;
                }
            }
        }
        else
        {
            shouldEndTurn = true;
        }

        if (shouldEndTurn)
        {
            //Debug.Log("Next turn");
            //mBrain.OnTurnEnd();
        }
    }*/

    /*new public void SendMessage(string message)
    {
        SendMessage(message, null);
    }

    new public void SendMessage(string message, object value = null, SendMessageOptions options = SendMessageOptions.DontRequireReceiver)
    {
        base.SendMessage(message, value, options);
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            mEnvironmentObjects[i].SendMessage(message, value, options);
        }
    }*/

    public void AddAgent(EnvironmentAgent newAgent)
    {
        if (!mActiveAgents.Contains(newAgent))
        {
            mActiveAgents.Add(newAgent);
        }
    }

    public void RemoveAgent(EnvironmentAgent removeAgent)
    {
        if (mActiveAgents.Contains(removeAgent))
        {
            mActiveAgents.Remove(removeAgent);

            bool hadDecisions = mDecisionRequests.Count > 0;

            if (mDecisionRequests.Contains(removeAgent))
            {
                mDecisionRequests.Remove(removeAgent);
            }

            if (hadDecisions && mDecisionRequests.Count == 0)
            {
                //No agents require decisions, continue the simulation
                StepEnded();
            }
        }

        if (mInactiveAgents.Contains(removeAgent))
        {
            mInactiveAgents.Remove(removeAgent);
        }
    }

    public void AgentEndedEpisode(EnvironmentAgent agent)
    {
        if (mActiveAgents.Contains(agent))
        {
            mActiveAgents.Remove(agent);
        }

        if (!mInactiveAgents.Contains(agent))
        {
            mInactiveAgents.Add(agent);
        }

        if (mDecisionRequests.Contains(agent))
        {
            mDecisionRequests.Remove(agent);
        }

        //No more active agents, restart run.
        if (mActiveAgents.Count == 0)
        {
            RunEnded();
        }
    }

    public void AddObject(EnvironmentObject newObject)
    {
        // Debug.Log($"Add object: {newObject} -> {mNextID}");

        if (!mEnvironmentObjects.Contains(newObject))
        {
            mEnvironmentObjects.Add(newObject);
            newObject.SetObjectID(mNextID++);
        }
    }

    public void RemoveObject(EnvironmentObject removeObject)
    {
        if (mEnvironmentObjects.Contains(removeObject))
        {
            mEnvironmentObjects.Remove(removeObject);
        }
    }

    public void HadCollision(EnvironmentObject object1, EnvironmentObject object2)
    {
        //CheckBrain();

        //mBrain.HadCollision(object1, object2);
    }


    //FIX ME: Loading and savning needs to include the environmentengine components
    public JSONObject SaveEnvironmentState()
    {
        JSONObject fullEnvironment = new JSONObject();

        JSONObject thisObjects = new JSONObject();
        for (int i = 0; i < mComponents.Length; i++)
        {
            JSONObject tempObject = mComponents[i].SaveToJSON();
            if (tempObject.Count > 0)
            {
                thisObjects.AddField(mComponents[i].GetType().Name, tempObject);
            }
        }
        if (thisObjects.Count > 0)
        {
            fullEnvironment.AddField("components", thisObjects);
        }

        JSONObject allObjects = new JSONObject();
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            allObjects.AddField(mEnvironmentObjects[i].GetObjectID().ToString(), mEnvironmentObjects[i].SaveObjectToJSON());
        }

        fullEnvironment.AddField("objects", allObjects);

        return fullEnvironment;
    }

    public void LoadEnvironmentState(JSONObject jsonObject)
    {
        List<JSONObject> allObjects = Common.getArray(jsonObject, "objects");

        for (int i = 0; i < allObjects.Count; i++)
        {
            string prefabPath = Common.getString(allObjects[i], "prefabPath");

            UnityEngine.Object tempPrefab = Resources.Load(prefabPath);

            if (tempPrefab != null)
            {
                GameObject loadedObject = (GameObject)Instantiate(tempPrefab);
                EnvironmentObject tempEnvironmentObject = loadedObject.GetComponent<EnvironmentObject>();

                tempEnvironmentObject.transform.parent = transform;
                tempEnvironmentObject.CheckInitialized();
                tempEnvironmentObject.SetPrefabPath(prefabPath);

                tempEnvironmentObject.LoadObjectFromJSON(allObjects[i]);
            }
        }
    }

    public EnvironmentObject CreateEnvironmentObject(GameObject baseObject)
    {
        return CreateEnvironmentObject(baseObject, Vector3.zero, Quaternion.identity);
    }

    public EnvironmentObject CreateEnvironmentObject(GameObject baseObject, Vector3 position)
    {
        return CreateEnvironmentObject(baseObject, position, Quaternion.identity);
    }

    public EnvironmentObject CreateEnvironmentObject(GameObject baseObject, Vector3 position, Quaternion rotation)
    {
        EnvironmentObject newObject = mEngine.GetNewObject(baseObject);

        newObject.transform.position = position;
        newObject.transform.rotation = rotation;

        newObject.transform.parent = transform;

        //CheckBrain();

        newObject.CheckInitialized();
        //OnCreated(newObject);
        newObject.OnCreated();

        //mBrain.OnInstantiate(newObject);

        return newObject;
    }

    public List<EnvironmentObject> GetAllObjects()
    {
        return mEnvironmentObjects;
    }

    public EnvironmentObject GetEnvironmentObject<T>(string name = null) where T : EnvironmentObject
    {
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            if (mEnvironmentObjects[i] is T && (name == null || mEnvironmentObjects[i].name == name))
            {
                return mEnvironmentObjects[i];
            }
        }

        return null;
    }

    public EnvironmentComponent GetEnvironmentComponent<T>(string name = null) where T : EnvironmentComponent
    {
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            if ((name == null || mEnvironmentObjects[i].name == name))
            {
                T component = mEnvironmentObjects[i].GetComponent<T>();

                if (component != null)
                {
                    return component;
                }
            }
        }

        return null;
    }

    /*public void OnObjectMoved(EnvironmentObject movedObject)
    {
        CheckBrain();
        mBrain.OnObjectMoved(movedObject);
    }

    public void OnObjectStopped(EnvironmentObject movedObject)
    {
        CheckBrain();
        mBrain.OnObjectStopped(movedObject);
    }*/

    public bool IsPython()
    {
        //CheckBrain();
        //return mBrain.IsPython();
        return false;
    }

    public bool IsRunning()
    {
        return mIsRunning;
    }

    public void OnObjectMoved(EnvironmentObject thisObject, Vector3 oldPosition, Vector3 newPosition)
    {
        //mBrain.OnObjectMoved(thisObject, oldPosition, newPosition);
    }



    public EnvironmentObject GetNewObject(GameObject baseObject)
    {
        if (!mUnusedObjects.ContainsKey(baseObject))
        {
            mUnusedObjects.Add(baseObject, new List<EnvironmentObject>());
        }

        EnvironmentObject newObject = null;
        if (mUnusedObjects[baseObject].Count > 0)
        {
            newObject = mUnusedObjects[baseObject][0];
            mUnusedObjects[baseObject].RemoveAt(0);

            newObject.WakeUp();
        }
        else
        {
            newObject = ((GameObject)GameObject.Instantiate(baseObject)).GetComponent<EnvironmentObject>();
        }

        //newObject.mBaseObject = baseObject;

        return newObject;
    }

    //External interface methods

    public virtual void DoAction(JSONObject actionObject)
    {
    }

    public virtual void SetProperty(JSONObject actionObject)
    {
    }

    public void SetPythonTraining(bool training)
    {
        Debug.Log("Set training:" + training);

        isTraining = training;
    }

    public void SetPhysicsEngine(PhysicsScene physicsScene)
    {
        mPhysicsScene = physicsScene;
    }

    public void SetPhysicsEngine2D(PhysicsScene2D physicsScene)
    {
        mPhysicsScene2D = physicsScene;
    }

    //Environment specific position and movement logic

    virtual public Vector3 ApplyEnvironmentPosition(Vector3 originalLocalPosition)
    {
        return originalLocalPosition;
    }

    virtual public Vector3 ApplyEnvironmentRotation(Vector3 originalLocalEulerAngles)
    {
        return originalLocalEulerAngles;
    }

    virtual public Vector3 ApplyEnvironmentVelocity(Vector3 originalVelocity)
    {
        return originalVelocity;
    }

    virtual public Vector3 ApplyEnvironmentAngularVelocity(Vector3 originalAngularVelocity)
    {
        return originalAngularVelocity;
    }




    //Randomization methods

    public int GetRandomInt()
    {
        return mRandom.Next();
    }

    public int GetRandomInt(int maxValue)
    {
        return mRandom.Next(maxValue);
    }

    public float GetRandomFloat()
    {
        return (float)mRandom.NextDouble();
    }

    public float GetRandomRange(float min, float max)
    {
        return min + (float)mRandom.NextDouble() * (max - min);
    }

    public int GetRandomRange(int min, int max)
    {
        return min + mRandom.Next(max - min);
    }

    public void SetSeed(int seed)
    {
        mRandom = new System.Random(seed);
    }

    public void RandomizeSeed()
    {
        mRandom = new System.Random();
    }

    //Event logging

    public void SendData()
    {
        if (EnvironmentManager.Instance.ShouldSaveData() && mGameEvents && mGameEvents.HasField("Inputs"))
        {
            mGameEvents.AddField("ID", DateTime.Now.ToString());

            EnvironmentManager.Instance.SendEventToJavascript("PostData", mGameEvents.ToString());

            EnvironmentManager.Instance.PostData(mGameEvents.ToString());
            mGameEvents = new JSONObject();
        }
    }

    public void AddGameEvent(string eventName, JSONObject eventValues = null)
    {
        if (EnvironmentManager.Instance.ShouldSaveData())
        {
            if (!mGameEvents.HasField("Events"))
            {
                mGameEvents.AddField("Events", new JSONObject());
            }

            JSONObject eventData = new JSONObject();
            eventData["id"] = new JSONObject(eventName);
            eventData["time"] = new JSONObject(Time.unscaledTime.ToString());
            eventData["values"] = eventValues;

            mGameEvents["Events"].Add(eventData);
        }
    }

    public void AddGameInput(string inputValue)
    {
        if (EnvironmentManager.Instance.ShouldSaveData())
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

    virtual public U3CharacterSystem GetKinematicCharacterSystem()
    {
        return null;
    }
}
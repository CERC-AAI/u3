using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class EnvironmentEngine : EnvironmentComponent
{
    // Public members
    public Transform recycleBin;
    public int turnTime = 0; // 0 for no turns
    public float turnSpeed = 1; // real time length of one turn
    public float gridSize = 0;
    Transform environmentRoot;

    // Private members
    List<EnvironmentObject> mEnvironmentObjects = new List<EnvironmentObject>();
    double mNextTurnTime = 0;
    bool mIsEndOfTurn = true;
    bool mHadFixedUpdate;
    int mNextID = 0;
    EnvironmentBrain mBrain;
    long mFixedTime = 0;

    // Start is called before the first frame update
    override protected void Init()
    {
        UnityEngine.Random.InitState(System.Environment.TickCount);

        if (IsPython())
        {
            Debug.Log("Python");
            turnSpeed = 0;
        }
        else
        {
            Debug.Log("Not python");
        }

        Physics.autoSimulation = turnTime == 0;
        Physics2D.autoSimulation = turnTime == 0;

        CheckBrain();

        base.Init();
    }

    void CheckBrain()
    {
        if (mBrain == null)
        {
            mBrain = GetComponent<EnvironmentBrain>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        mBrain.OnUpdate(mIsEndOfTurn);
        mEnvironmentObjects.ForEach(obj => obj.OnObjectUpdate(mIsEndOfTurn));
    }

    private void FixedUpdate()
    {
        if (mFixedTime == 0 || !mIsEndOfTurn)
        {
            //Debug.Log(mFixedTime + " before physics");

            Physics.autoSimulation = turnTime == 0;
            Physics2D.autoSimulation = turnTime == 0;

            if (!Physics.autoSimulation && !mIsEndOfTurn)
            {
                Physics.Simulate(Time.fixedDeltaTime);
                Physics2D.Simulate(Time.fixedDeltaTime);
            }

            if (!Physics.autoSimulation)
            {
                UpdatePhysics();
            }
            else
            {
                StartCoroutine("DoFixedUpdate");
            }
        }
    }

    IEnumerator DoFixedUpdate()
    {
        yield return new WaitForFixedUpdate();

        UpdatePhysics();
    }

    void UpdatePhysics()
    {
        if (turnTime > 0 && mFixedTime >= mNextTurnTime)
        {
            mIsEndOfTurn = true;
        }

        //Debug.Log(mFixedTime + " after physics " + mIsEndOfTurn);

        mBrain.OnFixedUpdate(mIsEndOfTurn);
        mEnvironmentObjects.ForEach(obj => obj.OnObjectFixedUpdate(mIsEndOfTurn));

        mFixedTime++;
    }

    public virtual void OnEnvironmentActionRecieved(float[] vectorAction, bool shouldMove)
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
            mBrain.OnTurnEnd();
        }
    }

    public Vector3 GetGlobalPosition(Vector3 environmentPosition)
    {
        if (environmentRoot)
        {
            return environmentPosition + environmentRoot.transform.position;
        }
        else
        {
            return environmentPosition;
        }
    }

    public Vector3 GetEnvironmentPosition(Vector3 globalPosition)
    {
        if (environmentRoot)
        {
            return globalPosition - environmentRoot.transform.position;
        }
        else
        {
            return globalPosition;
        }
    }

    public void AddObject(EnvironmentObject newObject)
    {
        if (!mEnvironmentObjects.Contains(newObject))
        {
            mEnvironmentObjects.Add(newObject);
            if (newObject is EnvironmentObject)
            {
                ((EnvironmentObject)newObject).SetObjectID(mNextID++);
            }
        }
    }

    public void RemoveObject(EnvironmentObject removeObject)
    {
        if (mEnvironmentObjects.Contains(removeObject))
        {
            mEnvironmentObjects.Remove(removeObject);
        }
    }

    public void IncrementTime()
    {
        if (turnTime > 0)
        {
            mNextTurnTime += turnTime;
            if (turnSpeed < 0.01)
            {
                Time.timeScale = 100;
            }
            else
            {
                Time.timeScale = 1.0f / turnSpeed;
            }
            mIsEndOfTurn = false;
        }
    }

    public void HadCollision(EnvironmentObject object1, EnvironmentObject object2)
    {
        CheckBrain();

        mBrain.HadCollision(object1, object2);
    }

    public void AddReward(float reward)
    {
        CheckBrain();

        mBrain.AddReward(reward);
    }

    public void SetReward(float reward)
    {
        CheckBrain();

        mBrain.SetReward(reward);
    }

    string SaveEnvironmentState()
    {
        JSONObject fullEnvironment = new JSONObject();

        JSONObject[] allObjects = new JSONObject[mEnvironmentObjects.Count];
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            allObjects[i] = mEnvironmentObjects[i].SaveObjectToJSON();
        }

        fullEnvironment.AddField("objects", new JSONObject(allObjects));

        return fullEnvironment.ToString();
    }

    void LoadEnvironmentState(string JSONString)
    {
        for (int i = 0; i < mEnvironmentObjects.Count; i++)
        {
            Destroy(mEnvironmentObjects[i].gameObject);
        }
        mEnvironmentObjects.Clear();

        JSONObject tempObject = new JSONObject(JSONString);

        List<JSONObject> allObjects = Common.getArray(tempObject, "objects");

        for (int i = 0; i < allObjects.Count; i++)
        {
            string prefabPath = Common.getString(allObjects[i], "prefabPath");

            UnityEngine.Object tempPrefab = Resources.Load(prefabPath);

            if (tempPrefab != null)
            {
                GameObject loadedObject = (GameObject)Instantiate(tempPrefab);
                EnvironmentObject tempEnvironmentObject = loadedObject.GetComponent<EnvironmentObject>();

                tempEnvironmentObject.transform.parent = transform;
                tempEnvironmentObject.CheckInit();
                tempEnvironmentObject.SetPrefabPath(prefabPath);

                tempEnvironmentObject.LoadObjectFromJSON(allObjects[i]);
            }
        }
    }

    public Vector3 GetDiscreteVelocity(Vector3 velocity)
    {
        if (gridSize > 0)
        {
            int framesPerTurn = Mathf.Clamp(turnTime, 1, turnTime);
            float frameSpeed = gridSize / (Time.fixedDeltaTime * framesPerTurn);

            velocity.x = Mathf.Sign(velocity.x) * Mathf.Ceil(Mathf.Abs(velocity.x) / frameSpeed) * frameSpeed;
            velocity.y = Mathf.Sign(velocity.y) * Mathf.Ceil(Mathf.Abs(velocity.y) / frameSpeed) * frameSpeed;
            velocity.z = Mathf.Sign(velocity.z) * Mathf.Ceil(Mathf.Abs(velocity.z) / frameSpeed) * frameSpeed;
        }

        return velocity;
    }

    public Vector3 GetMoveVelocity(Vector3 velocity, float speed)
    {
        if (gridSize > 0)
        {
            int framesPerTurn = Mathf.Clamp(turnTime, 1, turnTime);
            float frameSpeed = speed * gridSize / (Time.fixedDeltaTime * framesPerTurn);

            speed = Mathf.Sign(speed) * Mathf.Ceil(Mathf.Abs(speed) / frameSpeed) * frameSpeed;
        }

        return velocity * speed;
    }

    public Vector3 GetDiscretePosition(Vector3 position, bool isEndTurn)
    {
        if (gridSize > 0)
        {
            int framesPerTurn = Mathf.Clamp(turnTime, 1, turnTime);
            float fixSize = gridSize / framesPerTurn;
            if (isEndTurn)
            {
                fixSize = gridSize;
            }

            position.x = Mathf.Round(position.x / fixSize) * fixSize;
            position.y = Mathf.Round(position.y / fixSize) * fixSize;
            position.z = Mathf.Round(position.z / fixSize) * fixSize;
        }

        return position;
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
        EnvironmentObject newObject = EnvironmentObject.GetNewObject(baseObject);

        newObject.transform.position = position;
        newObject.transform.rotation = rotation;

        newObject.transform.parent = transform;

        CheckBrain();

        newObject.CheckInit();

        mBrain.OnInstantiate(newObject);

        return newObject;
    }

    public List<EnvironmentObject> GetAllObjects()
    {
        return mEnvironmentObjects;
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

    public void Victory()
    {
        CheckBrain();
        mBrain.Victory();
    }

    public void Defeat()
    {
        CheckBrain();
        mBrain.Defeat();
    }

    public void AddGameEvent(string eventName, string eventValue)
    {
        CheckBrain();
        mBrain.AddGameEvent(eventName, eventValue);
    }

    public void AddCustomEvent(string eventName, string eventValue)
    {
        CheckBrain();
        mBrain.AddGameCustom(eventName, eventValue);
    }

    public bool IsPython()
    {
        CheckBrain();
        return mBrain.IsPython();
    }

    public void SendEvent(string eventString, EnvironmentComponent component)
    {
        CheckBrain();

        mBrain.OnEventSent(eventString, component);
    }

    public void OnObjectMoved(EnvironmentObject thisObject, Vector3 oldPosition, Vector3 newPosition)
    {
        mBrain.OnObjectMoved(thisObject, oldPosition, newPosition);
    }
}
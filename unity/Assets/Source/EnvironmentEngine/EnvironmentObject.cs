using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//base class for game objects
// Defines postion in game world, collision code, etc
[ExecuteInEditMode]
public class EnvironmentObject : EnvironmentComponent
{
    // Public members

    public Vector3 Position
    {
        get { return GetPosition(); }
        set { SetPosition(value); }
    }
    public Vector3 Rotation
    {
        get { return transform.rotation.eulerAngles; }
        set
        {
            Quaternion rotation = Quaternion.Euler(value);
            transform.rotation = rotation;
        }
    }
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    // Private mebers
    Collider[] mColliders;
    Collider2D[] mColliders2D;
    Rigidbody mRigidbody;
    Rigidbody2D mRigidbody2D;
    bool mIs2D;
    int mObjectID;
    [HideInInspector]
    public string mPrefabPath;
    EnvironmentComponent[] mComponents;
    Brain mBrain;
    List<EnvironmentObject> mTurnCollisions = new List<EnvironmentObject>();
    GameObject mBaseObject;

    static Dictionary<GameObject, List<EnvironmentObject>> mUnusedObjects = new Dictionary<GameObject, List<EnvironmentObject>>();

    
    override protected void Init()
    {
        mColliders = GetComponentsInChildren<Collider>();
        mColliders2D = GetComponentsInChildren<Collider2D>();
        mRigidbody = GetComponent<Rigidbody>();
        mRigidbody2D = GetComponent<Rigidbody2D>();

        if (mRigidbody && !mRigidbody2D)
        {
            mIs2D = false;
        }
        else if (!mRigidbody && mRigidbody2D)
        {
            mIs2D = true;
        }

        CheckComponents();
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].CheckInit();
        }

        base.Init();
    }

    void CheckComponents()
    {
        if (mComponents == null)
        {
            mComponents = GetComponents<EnvironmentComponent>();
        }
    }

    void CheckBrain()
    {
        if (mBrain == null)
        {
            mBrain = GetComponent<Brain>();
        }
    }

    public void Remove()
    {
        OnRemoved();

        if (mUnusedObjects.ContainsKey(mBaseObject) && !mUnusedObjects[mBaseObject].Contains(this))
        {
            mUnusedObjects[mBaseObject].Add(this);
        }
        transform.parent = mEngine.recycleBin;
        gameObject.SetActive(false);
    }

    virtual public void OnRemoved()
    {

    }

    virtual public void OnCreated()
    {
        mColliders = null;
        mColliders2D = null;
        mRigidbody = null;
        mRigidbody2D = null;
        mComponents = null;

        mPropertiesInitialized = false;
        mIsInitialized = false;

        CheckComponents();
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnCreated();
        }
    }

    static public EnvironmentObject GetNewObject(GameObject baseObject)
    {
        if (!mUnusedObjects.ContainsKey(baseObject))
        {
            mUnusedObjects.Add(baseObject, new List<EnvironmentObject>());
        }

        EnvironmentObject newObject = null;
        if (mUnusedObjects[baseObject].Count >  0)
        {
            newObject = mUnusedObjects[baseObject][0];
            mUnusedObjects[baseObject].RemoveAt(0);
        }
        else
        {
            newObject = ((GameObject)GameObject.Instantiate(baseObject)).GetComponent<EnvironmentObject>();
        }

        newObject.RefreshObject();
        newObject.mBaseObject = baseObject;

        return newObject;
    }

    public void RefreshObject()
    {
        gameObject.SetActive(true);
        OnCreated();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEngine.Object prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);

            if (prefab != null && AssetDatabase.GetAssetPath(prefab) != null && AssetDatabase.GetAssetPath(prefab) != "")
            {
                if (mPrefabPath != AssetDatabase.GetAssetPath(prefab))
                {
                    mPrefabPath = AssetDatabase.GetAssetPath(prefab);
                    EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }

    public void OnObjectUpdate(bool isEndOfTurn)
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnUpdate(isEndOfTurn);
        }
    }

    public void OnObjectFixedUpdate(bool isEndOfTurn)
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnFixedUpdate(isEndOfTurn);
        }

        if (isEndOfTurn)
        {
            for (int i = 0; i < mTurnCollisions.Count; i++)
            {
                OnEndTurnCollision(mTurnCollisions[i]);
            }
            mTurnCollisions.Clear();
        }

        if ((!mRigidbody || mRigidbody.isKinematic || !mRigidbody.useGravity) && (!mRigidbody2D || mRigidbody2D.bodyType == RigidbodyType2D.Kinematic || mRigidbody2D.gravityScale == 0))
        {
            SetPosition(mEngine.GetDiscretePosition(GetPosition(), isEndOfTurn));

            transform.rotation = Quaternion.identity;
        }

    }

    public bool OnObjectActionRecieved(float[] vectorAction)
    {
        bool shouldEndTurn = false;

        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i].OnActionRecieved(vectorAction))
            {
                shouldEndTurn = true;
            }
        }

        return shouldEndTurn;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Application.isPlaying)
        {
            EnvironmentObject otherObject = other.GetComponentInParent<EnvironmentObject>();
            if (otherObject)
            {
                OnCollision(otherObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Application.isPlaying)
        {
            EnvironmentObject otherObject = other.GetComponentInParent<EnvironmentObject>();
            if (otherObject)
            {
                OnCollision(otherObject);
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (Application.isPlaying)
        {
            if (other.rigidbody)
            {
                EnvironmentObject otherObject = other.rigidbody.GetComponentInParent<EnvironmentObject>();
                if (otherObject)
                {
                    OnCollision(otherObject);
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (Application.isPlaying)
        {
            if (other.rigidbody)
            {
                EnvironmentObject otherObject = other.rigidbody.GetComponentInParent<EnvironmentObject>();
                if (otherObject)
                {
                    OnCollision(otherObject);
                }
            }
        }
    }


    public void SetPosition(Vector3 newEnvironmentPosition)
    {
        Vector3 worldPosition = mEngine.GetGlobalPosition(newEnvironmentPosition);
        transform.position = worldPosition;
    }

    public void SetPosition(Vector2 newEnvironmentPosition)
    {
        SetPosition(new Vector3(newEnvironmentPosition.x, newEnvironmentPosition.y, 0));
    }

    public void AddPosition(Vector3 offset)
    {
        Vector3 environmentPosition = GetPosition() + offset;
        SetPosition(environmentPosition);
    }

    public void AddPosition(Vector2 offset)
    {
        AddPosition(new Vector3(offset.x, offset.y, 0));
    }

    public Vector3 GetPosition()
    {
        return mEngine.GetEnvironmentPosition(transform.position);
    }

    // For effects that occur during collision, physics should be handled by Unity
    override public void OnCollision(EnvironmentObject otherObject)
    {
        CheckBrain();
        if (mBrain)
        {
            mBrain.OnCollision(otherObject);
        }

        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i] != this)
            {
                mComponents[i].OnCollision(otherObject);
            }
        }

        mTurnCollisions.Add(otherObject);

        base.OnCollision(otherObject);
    }

    override public void OnEndTurnCollision(EnvironmentObject otherObject)
    {
        CheckBrain();
        if (mBrain)
        {
            mBrain.OnEndTurnCollision(otherObject);
        }

        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i] != this)
            {
                mComponents[i].OnEndTurnCollision(otherObject);
            }
        }

        base.OnEndTurnCollision(otherObject);
    }

    public void SetObjectID(int id)
    {
        mObjectID = id;
    }

    public int GetObjectID()
    {
        return mObjectID;
    }

    public JSONObject SaveObjectToJSON()
    {
        JSONObject fullObject = new JSONObject();

        if (mPrefabPath == null || mPrefabPath == "")
        {
            Debug.LogError("Could not find a prefab associated with object: " + name);

            return fullObject;
        }


        JSONObject[] allObjects = new JSONObject[mComponents.Length];
        for (int i = 0; i < mComponents.Length; i++)
        {
            JSONObject tempObject = mComponents[i].SaveToJSON();

            allObjects[i] = tempObject;
        }

        string relativePrefabPath = mPrefabPath.Substring(mPrefabPath.LastIndexOf("Resources/") + 10);
        relativePrefabPath = relativePrefabPath.Substring(0, relativePrefabPath.LastIndexOf(".prefab"));
        fullObject.AddField("prefabPath", relativePrefabPath);
        fullObject.AddField("components", new JSONObject(allObjects));

        return fullObject;
    }

    public void LoadObjectFromJSON(JSONObject tempObject)
    {
        CheckComponents();

        List<JSONObject> allComponents = Common.getArray(tempObject, "components");

        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].LoadFromJSON(allComponents[i]);
        }
    }

    public void SetPrefabPath(string prefabPath)
    {
        mPrefabPath = prefabPath;
    }

    public void MakeCollidersTriggers()
    {
        for (int i = 0; i < mColliders.Length; i++)
        {
            mColliders[i].isTrigger = true;
        }
        for (int i = 0; i < mColliders2D.Length; i++)
        {
            mColliders2D[i].isTrigger = true;
        }
    }
    
    public void ClearColliders()
    {
        for (int i = 0; i < mColliders.Length; i++)
        {
            mColliders[i].enabled = false;
        }
        for (int i = 0; i < mColliders2D.Length; i++)
        {
            mColliders2D[i].enabled = false;
        }
    }

    public void RestoreColliders()
    {
        for (int i = 0; i < mColliders.Length; i++)
        {
            mColliders[i].enabled = true;
        }
        for (int i = 0; i < mColliders2D.Length; i++)
        {
            mColliders2D[i].enabled = true;
        }
    }
}
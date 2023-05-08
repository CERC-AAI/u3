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
public class EnvironmentObject : EnvironmentComponentHolder
{
    // Public members

    public Vector3 Position
    {
        get { return transform.localPosition; }
        set { transform.localPosition = mEngine.ApplyEnvironmentPosition(value);  }
    }

    public Vector3 Rotation
    {
        get { return transform.localPosition; }
        set 
        {
            Quaternion currentRotation = transform.localRotation;
            Vector3 eulerAngles = currentRotation.eulerAngles;

            eulerAngles = mEngine.ApplyEnvironmentRotation(value);

            currentRotation.eulerAngles = eulerAngles;
            transform.localRotation = currentRotation;
        }
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    // Private members
    int mObjectID;
    [HideInInspector]
    public string mPrefabPath;

    List<EnvironmentObject> mStepCollisions = new List<EnvironmentObject>();
    List<EnvironmentObject> mFixedUpdateCollisions = new List<EnvironmentObject>();
    //GameObject mBaseObject;

    Collider2D[] mColliders2D;
    Collider[] mColliders;

    override protected void Initialize()
    {
        mColliders2D = GetComponentsInChildren<Collider2D>();
        mColliders = GetComponentsInChildren<Collider>();

        CheckComponents();

        base.Initialize();
    }

    public override void OnRunStarted()
    {
        base.OnRunStarted();

        ResetDefaultProperties();
    }

    public override void ResetDefaultProperties()
    {
        base.ResetDefaultProperties();

        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i] != this)
            {
                mComponents[i].ResetDefaultProperties();
            }
        }
    }

    override public void Remove()
    {
        base.Remove();

        transform.parent = mEngine.recycleBin;
        gameObject.SetActive(false);
    }

    public override void WakeUp()
    {
        gameObject.SetActive(true);

        base.WakeUp();
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

    sealed override public void OnObjectUpdate(float deltaTime)
    {
        base.OnObjectUpdate(deltaTime);
    }

    sealed override public void OnObjectFixedUpdate(float fixedDeltaTime)
    {
        base.OnObjectFixedUpdate(fixedDeltaTime);
    }

    sealed override public void OnObjectLateFixedUpdate(float fixedDeltaTime)
    {
        base.OnObjectLateFixedUpdate(fixedDeltaTime);
    }

    public override void OnStepEnded()
    {
        for (int i = 0; i < mStepCollisions.Count; i++)
        {
            for (int j = 0; j < mComponents.Length; j++)
            {
                mComponents[j].OnEndStepCollision(mStepCollisions[i]);
            }
        }
        mStepCollisions.Clear();

        base.OnStepEnded();
    }

    public override void OnLateFixedUpdate(float fixedDeltaTime)
    {
        base.OnLateFixedUpdate(fixedDeltaTime);

        for (int i = 0; i < mFixedUpdateCollisions.Count; i++)
        {
            for (int j = 0; j < mComponents.Length; j++)
            {
                mComponents[j].OnPostCollision(mFixedUpdateCollisions[i]);
            }
        }
        mFixedUpdateCollisions.Clear();
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

    // For effects that occur during collision, physics should be handled by Unity
    override public void OnCollision(EnvironmentObject otherObject)
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i] != this)
            {
                mComponents[i].OnCollision(otherObject);
            }
        }

        if (!mStepCollisions.Contains(otherObject))
        {
            mStepCollisions.Add(otherObject);
        }
        if (!mFixedUpdateCollisions.Contains(otherObject))
        {
            mFixedUpdateCollisions.Add(otherObject);
        }

        base.OnCollision(otherObject);
    }

    override public void OnEndStepCollision(EnvironmentObject otherObject)
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i] != this)
            {
                mComponents[i].OnEndStepCollision(otherObject);
            }
        }

        base.OnEndStepCollision(otherObject);
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

        fullObject.AddField("name", name);

        if (mPrefabPath == null || mPrefabPath == "")
        {
            Debug.LogError("Could not find a prefab associated with object: " + name);
        }
        else
        {
            string relativePrefabPath = mPrefabPath.Substring(mPrefabPath.LastIndexOf("Resources/") + 10);
            relativePrefabPath = relativePrefabPath.Substring(0, relativePrefabPath.LastIndexOf(".prefab"));
            fullObject.AddField("prefabPath", relativePrefabPath);
        }

        JSONObject allObjects = new JSONObject();
        for (int i = 0; i < mComponents.Length; i++)
        {
            JSONObject tempObject = mComponents[i].SaveToJSON();
            if (tempObject.Count > 0)
            {
                allObjects.AddField(mComponents[i].GetType().Name, tempObject);
            }
        }
        if (allObjects.Count > 0)
        {
            fullObject.AddField("components", allObjects);
        }

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
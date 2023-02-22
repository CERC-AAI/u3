using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

//base class for environment objects
public class EnvironmentComponent : MonoBehaviour 
{
    // Public members

    // Private members
    int mComponentID;
    protected EnvironmentEngine mEngine;
    protected EnvironmentObject mParentObject;
    List<PropertyInfo> mSaveProperties = new List<PropertyInfo>();
    protected bool mPropertiesInitialized = false;
    protected bool mIsInitialized = false;

    // Start is called before the first frame update
    private void Start()
    {
        if (Application.isPlaying)
        {
            CheckInit();
        }
    }

    virtual public void OnCreated()
    {
        mIsInitialized = false;
        mIsInitialized = false;
    }

    virtual protected void Init() 
    {
        CheckAll();
    }

    public void CheckInit()
    {
        if (!mIsInitialized)
        {
            mIsInitialized = true;

            Init();
        }
    }

    private void Update()
    {
        CheckEngine();
        CheckObject();
    }

    public void CheckAll()
    {
        CheckEngine();
        CheckObject();
        CheckProperties();
    }

    private void OnDestroy()
    {
        if (mEngine && this is EnvironmentObject)
        {
            mEngine.RemoveObject((EnvironmentObject)this);
        }
    }

    private void CheckEngine()
    {
        if (!mEngine)
        {
            mEngine = GetComponentInParent<EnvironmentEngine>();
            if (mEngine && mEngine != this && this is EnvironmentObject)
            {
                mEngine.AddObject((EnvironmentObject)this);
            }
        }
    }

    private void CheckObject()
    {
        if (!mParentObject)
        {
            mParentObject = GetComponent<EnvironmentObject>();

            EnvironmentComponent[] allComponents = GetComponents<EnvironmentComponent>();
            for (int i = 0; i < allComponents.Length; i++)
            {
                if (allComponents[i] == this)
                {
                    mComponentID = i;
                    break;
                }
            }
        }
    }

    private void CheckProperties()
    {
        if (!mPropertiesInitialized)
        {
            mPropertiesInitialized = true;

            PropertyInfo[] propertyInfos = GetType().GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.DeclaringType.IsSubclassOf(typeof(EnvironmentComponent)))
                {
                    mSaveProperties.Add(propertyInfo);
                }
            }
        }
    }

    virtual public void OnUpdate(bool isEndOfTurn)
    {
    }

    virtual public void OnFixedUpdate(bool isEndOfTurn)
    {
    }

    virtual public bool OnActionRecieved(float[] vectorAction)
    {
        return false;
    }

    virtual public void OnLevelLoaded()
    {

    }

    //pipe out the state of the game
    virtual public void GetState(Dictionary<String, Byte[]> saveDictionary)
    {
        OnPreGetState();

        foreach (PropertyInfo propertyInfo in mSaveProperties)
        {
            saveDictionary[mParentObject.GetObjectID() + "/" + mComponentID + "/" + propertyInfo.Name] = BinarizeData.ToByteArray(propertyInfo.GetValue(this), 9999);
        }
    }

    virtual public void LoadState(Dictionary<String, Byte[]> saveDictionary)
    {
        foreach (PropertyInfo propertyInfo in mSaveProperties)
        {
            string keyName = mParentObject.GetObjectID() + "/" + mComponentID + "/" + propertyInfo.Name;

            if (saveDictionary.ContainsKey(keyName))
            {
                object tempObject = null;
                int size;

                BinarizeData.FromByteArray(propertyInfo.PropertyType, saveDictionary[keyName], 0, out size, ref tempObject);
                propertyInfo.SetValue(this, tempObject);
            }
        }

        OnPostLoadState();
    }

    virtual public JSONObject SaveToJSON()
    {
        OnPreGetState();

        JSONObject fullComponent = new JSONObject();

        foreach (PropertyInfo propertyInfo in mSaveProperties)
        {
            Common.saveJSONValue(fullComponent, propertyInfo.Name, propertyInfo.GetValue(this), propertyInfo.PropertyType);
        }

        return fullComponent;
    }

    virtual public void LoadFromJSON(JSONObject tempObject)
    {
        CheckProperties();

        foreach (PropertyInfo propertyInfo in mSaveProperties)
        {
            object tempValue = Common.getJSONValue(tempObject, propertyInfo.Name, propertyInfo.PropertyType);

            propertyInfo.SetValue(this, tempValue);
        }

        OnPostLoadState();
    }

    virtual public void OnPreGetState()
    {
    }

    virtual public void OnPostLoadState()
    {
    }

    virtual public void OnCollision(EnvironmentObject otherObject)
    {
    }

    virtual public void OnEndTurnCollision(EnvironmentObject otherObject)
    {
    }

    public void Remove()
    {
        mParentObject.Remove();
    }

    public void Move(Vector3 newPosition)
    {
        mEngine.OnObjectMoved(mParentObject, mParentObject.GetPosition(), mEngine.GetEnvironmentPosition(newPosition));

        transform.position = newPosition;
    }

    public JSONObject BuildRunStateToJSON()
    {
        OnPreGetState();

        JSONObject fullComponent = new JSONObject();
        BuildRunStateJSON(fullComponent);

        return fullComponent;
    }

    public void LoadRunStateFromJSON(JSONObject tempObject)
    {
        JSONObject lowerJSON = new JSONObject();

        foreach (string key in tempObject.keys)
        {
            lowerJSON[key.ToLower()] = tempObject[key];
        }

        LoadRunStateJSON(tempObject);

        OnPostLoadState();
    }

    virtual protected void BuildRunStateJSON(JSONObject root)
    {
    }

    virtual protected void LoadRunStateJSON(JSONObject root)
    {
    }
}

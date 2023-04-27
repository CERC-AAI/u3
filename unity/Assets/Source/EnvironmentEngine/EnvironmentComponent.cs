using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public enum CallbackScope
{
    ENVIRONMENT = 1,
    SELF = 2
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class Callback : Attribute
{
    public Type callbackType;
    public CallbackScope callbackScope;

    public Callback(Type callbackType, CallbackScope scope = CallbackScope.SELF)
    {
        this.callbackType = callbackType;
        this.callbackScope = scope;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class Action : Attribute
{
    public int actionCount = 1;

    public Action(int actionCount = 1)
    {
        this.actionCount = actionCount;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class NotSaved : Attribute
{

}

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValue : Attribute
{
    public string defaultValue = null;

    public DefaultValue(string defaultValue)
    {
        this.defaultValue = defaultValue;
    }
}

public delegate void EnvironmentCallback();
public delegate void EnvironmentCallback<T>(T value = default(T));

//base class for environment objects
public class EnvironmentComponent : MonoBehaviour 
{
    // Public members

    // Private members
    protected EnvironmentEngine mEngine;
    protected EnvironmentObject mParentObject;
    List<PropertyInfo> mSaveProperties = new List<PropertyInfo>();
    List<object> mDefaultValues = new List<object>();
    protected bool mPropertiesInitialized = false;
    protected bool mInitialized = false;

    List<Delegate> mCallbackDelegates = new List<Delegate>();
    List<Delegate> mCallbackReferences = new List<Delegate>();
    List<Type> mCallbackTypes = new List<Type>();
    bool mCallbacksInitialized = false;

    Dictionary<Type, object> mCachedComponents = new Dictionary<Type, object>();


    //Dictionary<EnvironmentCallback, EnvironmentCallback> mRegisteredCallbacks = new Dictionary<EnvironmentCallback, EnvironmentCallback>();


    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (Application.isPlaying)
        {
            CheckInitialized();
        }
    }

    public void CheckInitialized(bool force = false)
    {
        if (!mInitialized || force)
        {
            Initialize();
        }
    }

    virtual protected void Initialize() 
    {
        CheckAll();

        if (mInitialized)
        {
            Debug.LogError("Object (" + name + ") initialized twice.");
        }

        mInitialized = true;

        DoRegisterCallbacks();
    }

    public void CheckAll()
    {
        CheckEngine();
        CheckObject();
        CheckCallbacks();
        //CheckProperties();

        if (mParentObject == null && GetComponent<EnvironmentEngine>() == null)
        {
            Debug.LogError(name + "(" + GetType().ToString() + ") Each EnvironmentComponent must be attached to a GameObject with an EnvironmentObject, or a EnvironmentEngine derived class.");
        }
    }

    private void OnDestroy()
    {
        if (mEngine && this is EnvironmentObject)
        {
            mEngine.RemoveObject((EnvironmentObject)this);
        }
        if (mEngine && this is EnvironmentAgent)
        {
            mEngine.RemoveAgent((EnvironmentAgent)this);
        }

        for (int i = 0; i < mCallbackReferences.Count; i++)
        {
            mCallbackReferences[i] = Delegate.Remove(mCallbackReferences[i], mCallbackDelegates[i]);
        }
        mCallbackReferences.Clear();
        mCallbackDelegates.Clear();
        mCallbackTypes.Clear();
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
            if (mEngine && mEngine != this && this is EnvironmentAgent)
            {
                mEngine.AddAgent((EnvironmentAgent)this);
            }
        }
    }

    private void CheckObject()
    {
        if (!mParentObject)
        {
            mParentObject = GetComponent<EnvironmentObject>();

            /*EnvironmentComponent[] allComponents = GetComponents<EnvironmentComponent>();
            for (int i = 0; i < allComponents.Length; i++)
            {
                if (allComponents[i] == this)
                {
                    mComponentID = i;
                    break;
                }
            }*/
        }
    }

    public virtual void AppendActionLists(List<ActionInfo> actionsList)
    {
        Type currentType = GetType();
        do
        {
            MethodInfo[] methodInfos = currentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (MethodInfo methodInfo in methodInfos)
            {
                Action actionInfo = methodInfo.GetCustomAttribute<Action>();
                if (actionInfo != null)
                {
                    Delegate newCallback = null;

                    ParameterInfo[] paramInfos = methodInfo.GetParameters();

                    if (paramInfos.Length != 1)
                    {
                        Debug.Log("Cannot add Action" + name + "(" + methodInfo.Name + "). Function must have a single int or float parameter.");

                        return;
                    }

                    ActionInfo actionObject = null;
                    if (paramInfos[0].ParameterType == typeof(int))
                    {
                        newCallback = Delegate.CreateDelegate(typeof(ActionInfo.DiscreteAction), this, methodInfo);

                        actionObject = new ActionInfo((ActionInfo.DiscreteAction)newCallback, actionInfo.actionCount);
                    }
                    else
                    {
                        newCallback = Delegate.CreateDelegate(typeof(ActionInfo.ContinuousAction), this, methodInfo);

                        actionObject = new ActionInfo((ActionInfo.ContinuousAction)newCallback);
                    }

                    actionsList.Add(actionObject);
                }
            }

            currentType = currentType.BaseType;
        }
        while (currentType == typeof(EnvironmentComponent) || currentType.IsSubclassOf(typeof(EnvironmentComponent)));
    }

    protected virtual void DoRegisterCallbacks()
    {

    }

    private void CheckCallbacks()
    {
        if (!mCallbacksInitialized)
        {
            mCallbacksInitialized = true;

            List<string> addedMethods = new List<string>();

            Type currentType = GetType();
            do
            {
                MethodInfo[] methodInfos = currentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    Callback callbackInfo = methodInfo.GetCustomAttribute<Callback>();
                    if (callbackInfo != null && !addedMethods.Contains(methodInfo.Name))
                    {
                        addedMethods.Add(methodInfo.Name);

                        switch (callbackInfo.callbackScope)
                        {
                            case CallbackScope.ENVIRONMENT:
                                CheckRegisterCallback(mEngine, callbackInfo.callbackType, methodInfo);
                                break;

                            case CallbackScope.SELF:
                                CheckRegisterCallback(mParentObject, callbackInfo.callbackType, methodInfo);
                                break;
                        }
                    }
                }

                currentType = currentType.BaseType;
            }
            while (currentType == typeof(EnvironmentComponent) || currentType.IsSubclassOf(typeof(EnvironmentComponent)));
        }
    }

    private void CheckRegisterCallback(EnvironmentComponent thisObject, Type componentType, MethodInfo callbackMethod)
    {
        EnvironmentComponent registerComponent = (EnvironmentComponent)thisObject.GetComponent(componentType);

        if (registerComponent)
        {
            FieldInfo callbackInfo = registerComponent.GetType().GetField(callbackMethod.Name + "Callbacks", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (callbackInfo != null)
            {
                Delegate callback = (Delegate)callbackInfo.GetValue(registerComponent);
                Delegate newCallback = Delegate.CreateDelegate(callbackInfo.FieldType, this, callbackMethod);
                if (callback != null)
                {
                    callback = Delegate.Combine(callback, newCallback);
                }
                else
                {
                    callback = newCallback;
                }
                callbackInfo.SetValue(registerComponent, callback);

                Debug.Log("Added object " + name + "(" + registerComponent.GetType().Name + ") to callback of object " + thisObject.name + "(" + GetType().Name + ") for callback " + callbackMethod.Name);
            }
            else
            {
                Debug.LogError("No callback named '" + callbackMethod.Name + "Callbacks' found in object " + thisObject.name + " (" + registerComponent.GetType().Name + ").");
            }
        }
    }

    new public Component GetComponent(Type type)
    {
        if (!mCachedComponents.ContainsKey(type))
        {
            Component tempObject = base.GetComponent(type);

            mCachedComponents[type] = tempObject;
        }

        return (Component)mCachedComponents[type];
    }

    new public T GetComponent<T>()
    {
        if (!mCachedComponents.ContainsKey(typeof(T)))
        {
            T tempObject = base.GetComponent<T>();

            mCachedComponents[typeof(T)] = tempObject;
        }

        return (T)mCachedComponents[typeof(T)];
    }

    public void RegisterCallback(ref EnvironmentCallback callback, EnvironmentCallback functionToRegister)
    {
        mCallbackReferences.Add(functionToRegister);
        mCallbackDelegates.Add(callback);
        mCallbackTypes.Add(typeof(EnvironmentCallback));

        callback += functionToRegister;

        //Debug.Log("Added delegate " + functionToRegister.Target.ToString() + " to callback of object " + callback.GetInvocationList()[0].Target.ToString() + " for callback " + callback.Method.Name);
    }

    public void RegisterCallback<T>(ref EnvironmentCallback<T> callback, EnvironmentCallback<T> functionToRegister)
    {
        mCallbackReferences.Add(functionToRegister);
        mCallbackDelegates.Add(callback);
        mCallbackTypes.Add(typeof(EnvironmentCallback<T>));

        callback += functionToRegister;
    }


    public virtual void ResetDefaultProperties()
    {
        mPropertiesInitialized = false;

        CheckProperties();

        //Debug.Log(GetType().Name + ": ResetDefaultProperties");
    }

    private void CheckProperties()
    {
        if (!mPropertiesInitialized)
        {
            mPropertiesInitialized = true;

            mSaveProperties.Clear();
            mDefaultValues.Clear();

            Type currentType = GetType();
            do
            {
                PropertyInfo[] propertyInfos = currentType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    if (propertyInfo.GetCustomAttribute<NotSaved>() == null && propertyInfo.SetMethod != null && propertyInfo.GetMethod != null)
                    {
                        mSaveProperties.Add(propertyInfo);
                        mDefaultValues.Add(propertyInfo.GetValue(this));
                    }
                }

                currentType = currentType.BaseType;
            } 
            while (currentType == typeof(EnvironmentComponent) || currentType.IsSubclassOf(typeof(EnvironmentComponent)));
        }
    }

    virtual public void OnRunStarted()
    {
    }

    virtual public void OnRunEnded()
    {
    }

    virtual public void OnEpisodeStarted()
    {

    }

    virtual public void OnEpisodeEnded()
    {

    }

    virtual public void OnStepEnded()
    {
    }

    virtual public void OnStepStarted()
    {
    }

    virtual public void OnCreated()
    {
    }

    virtual public void OnUpdate(float deltaTime)
    {
    }

    virtual public void OnLateUpdate(float deltaTime)
    {
    }

    virtual public void OnFixedUpdate(float fixedDeltaTime)
    {
    }

    virtual public void OnLateFixedUpdate(float fixedDeltaTime)
    {
    }

    public EnvironmentObject GetParentObject()
    {
        return mParentObject;
    }

    public int GetObjectID()
    {
        CheckObject();

        return mParentObject.GetObjectID();
    }

    virtual public JSONObject SaveToJSON()
    {
        OnPreGetState();

        JSONObject fullComponent = new JSONObject();
        
        int i = 0;
        foreach (PropertyInfo propertyInfo in mSaveProperties)
        {
            if (name == "Player")
            {
                int  test = 1;
            }

            string defaultValue = null;
            if (propertyInfo.GetCustomAttribute<DefaultValue>() != null)
            {
                defaultValue = propertyInfo.GetCustomAttribute<DefaultValue>().defaultValue;
            }

            if (mDefaultValues[i] != null)
            {
                JSONObject componentJSON = new JSONObject();
                Common.saveJSONValue(componentJSON, propertyInfo.Name, propertyInfo.GetValue(this), propertyInfo.PropertyType, defaultValue, GetType().ToString());

                JSONObject defaultJSON = new JSONObject();
                Common.saveJSONValue(defaultJSON, propertyInfo.Name, mDefaultValues[i], propertyInfo.PropertyType, defaultValue, GetType().ToString());

                if (componentJSON.ToString() != defaultJSON.ToString())
                {
                    Common.saveJSONValue(fullComponent, propertyInfo.Name, propertyInfo.GetValue(this), propertyInfo.PropertyType, defaultValue, GetType().ToString());
                }
            }
            else
            {
                Common.saveJSONValue(fullComponent, propertyInfo.Name, propertyInfo.GetValue(this), propertyInfo.PropertyType, defaultValue, GetType().ToString());
            }
            i++;
        }

        return fullComponent;
    }

    virtual public void LoadFromJSON(JSONObject tempObject)
    {
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

    virtual public void OnRemoved()
    {

    }

    virtual public void Remove()
    {
        mParentObject.Remove();
    }

    public EnvironmentEngine GetEngine()
    {
        return mEngine;
    }

    virtual public void OnCollision(EnvironmentObject otherObject)
    {
    }

    virtual public void OnPostCollision(EnvironmentObject otherObject)
    {
    }

    virtual public void OnEndStepCollision(EnvironmentObject otherObject)
    {
    }

    public virtual void WakeUp()
    {
        mInitialized = false;
        Initialize();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents.Sensors;
using System.Text.RegularExpressions;

/*public class BaseEnvironment : EnvironmentBrain
{
    public string[] parameters;

    List<EnvironmentObject> mLoadedObjects = new List<EnvironmentObject>();

    int mTurnNumber = 0;

    override protected void Initialize()
    {
        base.Initialize();

        for (int i = 0; i < mLoadedObjects.Count; i++)
        {
            mLoadedObjects[i].CheckInitialized();
        }

        if (IsTesting())
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                SetParams(new JSONObject(parameters[i]));
            }
        }
    }

    protected EnvironmentObject LoadObject(GameObject baseObject, Vector3 position)
    {
        return LoadObject(baseObject, position, Color.white);
    }

    protected EnvironmentObject LoadObject(GameObject baseObject, Vector3 position, Color pixel)
    {
        EnvironmentObject newObject = mEngine.CreateEnvironmentObject(baseObject, position);

        mLoadedObjects.Add(newObject);

        OnObjectLoaded(newObject, pixel);

        return newObject;
    }

    public override void LoadEpisode()
    {
        AddGameEvent("LoadEpisode", "DefaultEnvironment");

        base.LoadEpisode();

        GetComponent<EnvironmentEngine>().EpisodeStarted();
    }

    public override void LoadEnvironmentElement(string elementName, string elementData = "")
    {
        switch (elementName)
        {
            default:
                base.LoadEnvironmentElement(elementName, elementData);
                break;
        }
    }

    public override void OnInstantiate(EnvironmentObject newObject)
    {
        base.OnInstantiate(newObject);
    }


    public override void OnEnvironmentActionReceived(float[] vectorAction)
    {
        if (ShouldLogInput(vectorAction))
        {
            AddGameInput(vectorAction[0].ToString());
        }
        else
        {
            AddGameInput("");
        }

        if (IsValidAction(vectorAction))
        {
            base.OnEnvironmentActionReceived(vectorAction);
        }
    }

    public virtual bool ShouldLogInput(float[] vectorAction)
    {
        return false;
    }

    public virtual bool IsValidAction(float[] vectorAction)
    {

        return true;
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
    }

    public override void Defeat()
    {
        AddGameEvent("Lose", "");

        base.Defeat();
    }

    public override void Victory()
    {
        AddGameEvent("Win", "");

        base.Victory();
    }

    protected override void OnGameOver()
    {
        SendData();

        base.OnGameOver();
    }

    protected override void ResetEpisode()
    {
        base.ResetEpisode();

        for (int i = 0; i < mLoadedObjects.Count; i++)
        {
            if (mLoadedObjects[i])
            {
                mLoadedObjects[i].Remove();
            }
        }
        mLoadedObjects.Clear();
    }

    public override void OnTurnEnd()
    {
        base.OnTurnEnd();

        mTurnNumber++;

        Dictionary<string, string> turnInfo = GetTurnInfo();

        AddEndTurnInfo(mTurnNumber, turnInfo);
    }

    public virtual Dictionary<string, string> GetTurnInfo()
    {
        Dictionary<string, string> turnInfo = new Dictionary<string, string>();

        return turnInfo;
    }

    public virtual void AddEndTurnInfo(int turnNumber, Dictionary<string, string> turnInfo)
    {
        if (ShouldSaveData())
        {
            if (!mGameEvents.HasField("Turns"))
            {
                mGameEvents.AddField("Turns", new JSONObject());
            }

            mGameEvents["Turns"].Add(new JSONObject(turnInfo));
        }
    }
}*/

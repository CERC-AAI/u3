using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class KeyGame : GridEnvironment
{
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject keyPrefab;
    public GameObject spawnPrefab;
    public GameObject healthorbPrefab;
    public GameObject exitPrefab;

    HealthBar mPlayerHP;

    List<Door> mDoors = new List<Door>();
    Dictionary<string, string> mTurnInfo = new Dictionary<string, string>();

    protected override void Init()
    {
        mPlayerHP = player.GetComponent<HealthBar>();

        base.Init();
    }

    public override void LoadLevel()
    {
        base.LoadLevel();


    }

    protected override int[] GetObjectType(EnvironmentObject checkObject)
    {
        if (!checkObject)
        {
            return new int[] { 0, 0, 0 };
        }

        if (checkObject.name.Contains("Wall"))
        {
            return new int[] { 1, 0, 0 };
        }

        Key thisKey = checkObject.GetComponent<Key>();
        if (thisKey)
        {
            return new int[] { 0, thisKey.GetLoadID(), 0 };
        }

        Door thisDoor = checkObject.GetComponent<Door>();
        if (thisDoor)
        {
            return new int[] { 1, thisDoor.GetLoadID(), 0 };
        }

        HealthOrb thisHealth = checkObject.GetComponent<HealthOrb>();
        if (thisHealth)
        {
            return new int[] { 0, 0, 1 };
        }

        return new int[] { 0, 0, 0 };
    }

    protected override void GetObjectsFromType(Color color, List<GameObject> objectList)
    {
        if (color.r * 255 == 255)
        {
            objectList.Add(wallPrefab);
        }

        if (Mathf.RoundToInt(color.g * 255) > 0)
        {
            if (Mathf.RoundToInt(color.g * 255) % 2 == 0)
            {
                objectList.Add(doorPrefab);
            }
            else
            {
                objectList.Add(keyPrefab);
            }
        }

        if (Mathf.RoundToInt(color.b * 255) > 0)
        {
            if (Mathf.RoundToInt(color.b * 255) == 50)
            {
                objectList.Add(spawnPrefab);
            }
            if (Mathf.RoundToInt(color.b * 255) == 100)
            {
                objectList.Add(exitPrefab);
            }
            if (Mathf.RoundToInt(color.b * 255) == 250)
            {
                objectList.Add(healthorbPrefab);
            }

        }
    }

    protected override void OnLoadFinsihed()
    {
        Spawn thisSpawn = GetComponentInChildren<Spawn>();

        if (thisSpawn)
        {
            Movement thisMovement = player.GetComponent<Movement>();

            if (thisMovement)
            {
                thisMovement.MoveTo(thisSpawn.transform.position);
            }
        }

        for (int i = 0; i < mDoors.Count; i++)
        {
            mDoors[i].FindKey();
        }
    }

    public override void Defeat()
    {
        Debug.Log("Oh no you lose!");

        AddReward(25.0f);

        base.Defeat();
    }

    public override void Victory()
    {
        Debug.Log("Yay you win!");

        AddReward(100.0f);

        base.Victory();
    }

    protected override void OnGameOver()
    {
        base.OnGameOver();

        mDoors.Clear();
        mPlayerHP.addHP(999999999.0f);
    }

    protected override void ResetLevel()
    {
        base.ResetLevel();

        mDoors.Clear();
        mPlayerHP.addHP(999999999.0f);
    }

    public override void OnObjectLoaded(EnvironmentObject movedObject, Color loadValue)
    {
        Door thisDoor = movedObject.GetComponent<Door>();
        if (thisDoor)
        {
            mDoors.Add(thisDoor);
        }

        LinkComponent thisLink = movedObject.GetComponent<LinkComponent>();
        if (thisLink)
        {
            thisLink.SetLoadID(Mathf.FloorToInt(loadValue.g*255/2));
        }

        base.OnObjectLoaded(movedObject, loadValue);
    }

    public override void OnEnvironmentActionReceived(float[] vectorAction)
    {
        AddReward(-0.1f);

        base.OnEnvironmentActionReceived(vectorAction);
    }

    public override void OnTurnEnd()
    {
        //AddReward(-0.1f);
        if (mPlayerHP)
        {
            mPlayerHP.subHP(0.1f);
        }

        base.OnTurnEnd();
    }

    public override Dictionary<string, string> GetTurnInfo()
    {
        mTurnInfo.Clear();

        mTurnInfo["pos"] = mCurrentPosition.ToString();
        mTurnInfo["hp"] = mPlayerHP.currentHP.ToString();

        return mTurnInfo;
    }

    
}

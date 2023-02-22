using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGame : GridEnvironment
{
    public GameObject wallPrefab;
    public GameObject buttonPrefab;
    public int totalButtons;
    public bool shouldAutoPlay;
    
    float mHealthLoss = 0.2f;

    HealthBar mPlayerHP;
    
    List<Button> mButtons = new List<Button>();
    Dictionary<string, string> mTurnInfo = new Dictionary<string, string>();
    int[] mState = null;

    Vector2Int mStartLocation = Vector2Int.zero;
    Dictionary<int, Vector2Int> mButtonsLocations = new Dictionary<int, Vector2Int>();


    int mSpawnButtons;

    protected override void Init()
    {
        mPlayerHP = player.GetComponent<HealthBar>();
        mSpawnButtons = totalButtons;

        base.Init();

        LoadLevel();
    }

    public override void LoadLevel()
    {
        base.LoadLevel();
    }

    protected override int[] GetObjectType(EnvironmentObject checkObject)
    {
        if (mState == null)
        {
            mState = new int[totalButtons + 1];
        }

        for (int i = 0; i < mState.Length; i++)
        {
            mState[i] = 0;
        }

        if (!checkObject)
        {
            return mState;
        }

        if (checkObject.name.Contains("Wall"))
        {
            mState[0] = 1;
            return mState;
        }

        Button thisButton = checkObject.GetComponent<Button>();
        if (thisButton)
        {
            mState[thisButton.GetOrder()] = 1;
            return mState;
        }

        return mState;
    }

    protected override void GetObjectsFromType(Color color, List<GameObject> objectList)
    {
        if (color.r * 255 == 255)
        {
            objectList.Add(wallPrefab);
        }
    }

    protected override void OnLoadFinsihed()
    {
        //All arenas have walls
        for (int y = 0; y < GetArenaHeight(); y++)
        {
            for (int x = 0; x < GetArenaWidth(); x++)
            {
                if (x == 0 || y == 0 || x == GetArenaWidth() - 1 || y == GetArenaHeight() - 1)
                {
                    if (!GetGridObject(new Vector2Int(x, y)))
                    {
                        LoadObject(wallPrefab, new Vector3(x, y, 0));
                    }
                }
            }
        }

        if (mStatic)
        {
            if (mSeedValue >= 0)
            {
                UnityEngine.Random.InitState(mSeedValue);
            }
            else
            {
                UnityEngine.Random.InitState(1);
            }
        }

        if (mStartLocation == Vector2Int.zero)
        {
            CheckEnvironmentElement("player", "");
        }
        else
        {
            CheckEnvironmentElement("player", mStartLocation.x + "," + mStartLocation.y);
        }

        for (int i = 0; i < mSpawnButtons; i++)
        {
            if (mButtonsLocations.ContainsKey(i) && mButtonsLocations[i] != Vector2Int.zero)
            {
                CheckEnvironmentElement("button" + (i + 1), mButtonsLocations[i].x + "," + mButtonsLocations[i].y);
            }
            else
            {
                CheckEnvironmentElement("button" + (i + 1), "");
            }
        }
    }

    public override void LoadEnvironmentElement(string elementName, string elementData = "")
    {
        switch (elementName.ToLower())
        {
            case "button1":
                if (elementData.ToLower() == "none")
                {
                    return;
                }

                Vector2Int newPosition = Vector2Int.zero;

                if (!ParseVector2DInt(elementData, out newPosition))
                {
                    newPosition = GetRandomPosition();
                }

                Button newButton = LoadObject(buttonPrefab, GetPositionFromGrid(newPosition), Color.clear).GetComponent<Button>();
                mButtons.Add(newButton);
                newButton.SetOrder(this, 1);
                break;

            case "button2":
                if (elementData.ToLower() == "none")
                {
                    return;
                }

                newPosition = Vector2Int.zero;

                if (!ParseVector2DInt(elementData, out newPosition))
                {
                    newPosition = GetRandomPosition();
                }

                newButton = LoadObject(buttonPrefab, GetPositionFromGrid(newPosition), Color.clear).GetComponent<Button>();
                mButtons.Add(newButton);
                newButton.SetOrder(this, 2);
                break;

            default:
                base.LoadEnvironmentElement(elementName, elementData);
                break;
        }
    }

    public override Dictionary<string, string> GetInitialStateInfo()
    {
        Dictionary<string, string> intialStatInfo = new Dictionary<string, string>();

        Vector2Int playerPosition;
        Vector2Int buttonPosition;
        GetGridPosition(player.transform.position, out playerPosition);
        intialStatInfo["playerPosition"] = playerPosition.ToString();

        for (int i = 0; i < mButtons.Count; i++)
        {
            GetGridPosition(mButtons[i].transform.position, out buttonPosition);
            intialStatInfo["button" + i + "Position"] = buttonPosition.ToString();
        }

        return intialStatInfo;
    }

    public override void Defeat()
    {
        //Debug.Log("Oh no you lose!");

        AddReward(25.0f);

        base.Defeat();
    }

    public override void Victory()
    {
        //Debug.Log("Yay you win!");

        AddReward(100.0f);

        base.Victory();
    }

    protected override void OnGameOver()
    {
        base.OnGameOver();
    }

    protected override void ResetLevel()
    {
        base.ResetLevel();

        mButtons.Clear();
        mPlayerHP.addHP(999999999.0f);
    }

    public override void OnObjectLoaded(EnvironmentObject movedObject, Color loadValue)
    {
        LinkComponent thisLink = movedObject.GetComponent<LinkComponent>();
        if (thisLink)
        {
            thisLink.SetLoadID(Mathf.FloorToInt(loadValue.g*255/2));
        }

        base.OnObjectLoaded(movedObject, loadValue);
    }

    public override void OnEnvironmentActionReceived(float[] vectorAction)
    {
        AddReward(-0.01f);
        if (mPlayerHP)
        {
            mPlayerHP.subHP(mHealthLoss*0.1f);
        }

        base.OnEnvironmentActionReceived(vectorAction);
    }

    public override void OnTurnEnd()
    {
        AddReward(-0.1f);
        if (mPlayerHP)
        {
            mPlayerHP.subHP(mHealthLoss);
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

    public bool CanPressButton(int order)
    {
        for (int i = 0; i < order - 1; i++)
        {
            if (!mButtons[i].isPressed())
            {
                return false;
            }
        }

        return true;
    }

    public bool IsLastButton(int order)
    {
        return order == mButtons.Count;
    }

    public override void Heuristic(float[] actionsOut)
    {
        if (!shouldAutoPlay)
        {
            base.Heuristic(actionsOut);
            return;
        }

        Vector3 target = player.transform.position;
        for (int i = 0; i < mButtons.Count; i++)
        {
            if (!mButtons[i].isPressed())
            {
                target = mButtons[i].transform.position;
                break;
            }
        }


        Vector3 moveDirection = player.transform.position - target;
        if (moveDirection.x <= -mEngine.gridSize/2)
        {
            actionsOut[0] = (int)Actions.RIGHT;
        }
        else if (moveDirection.x >= mEngine.gridSize/2)
        {
            actionsOut[0] = (int)Actions.LEFT;
        }
        else if (moveDirection.y <= -mEngine.gridSize/2)
        {
            actionsOut[0] = (int)Actions.UP;
        }
        else// if (moveDirection.y >= mEngine.gridSize/2)
        {
            actionsOut[0] = (int)Actions.DOWN;
        }
    }

    public override void RunCommand(string command)
    {
        switch (command)
        {
            case "test":
                Debug.Log("Test");
                return;

        }

        if (command.ToLower().Substring(0,10) == "movebutton")
        {
            string[] values = command.Substring(10).Split(',');

            int buttonNumber = int.Parse(values[0]);

            int x = int.Parse(values[1]);
            int y = int.Parse(values[2]);

            mButtons[buttonNumber].Move(GetPositionFromGrid(new Vector2Int(x, y)));
        }
    }

    public override void SetParam(string key, string value)
    {
        switch (key.ToLower())
        {
            case "startposition":
                string[] values = value.Split(',');

                int x = int.Parse(values[0]);
                int y = int.Parse(values[1]);

                mStartLocation = new Vector2Int(x, y);
                break;

            case "buttonposition":
                values = value.Split(',');

                int buttonNumber = int.Parse(values[0]);

                x = int.Parse(values[1]);
                y = int.Parse(values[2]);

                if (!mButtonsLocations.ContainsKey(buttonNumber))
                {
                    mButtonsLocations[buttonNumber] = Vector2Int.zero;
                }

                mButtonsLocations[buttonNumber] = new Vector2Int(x, y);
                break;

            case "buttons":
                int count = int.Parse(value);

                mSpawnButtons = count;
                break;

            case "healthloss":
                float loss = float.Parse(value);

                mHealthLoss = loss;
                break;

            case "health":
                float health = float.Parse(value);

                mPlayerHP.setHP(health);
                break;

            case "maxhealth":
                float maxhealth = float.Parse(value);

                mPlayerHP.setMaxHP(maxhealth);
                break;

            default:
                base.SetParam(key, value);
                break;

        }
    }

    override protected void BuildRunStateJSON(JSONObject root)
    {
        for (int i = 0; i < mButtons.Count; i++)
        {
            root["button" + (i+1)] = mButtons[i].BuildRunStateToJSON();
        }
        root["health"] = mPlayerHP.BuildRunStateToJSON();
        root["healthloss"] = new JSONObject(mHealthLoss);

        base.BuildRunStateJSON(root);
    }

    override protected void LoadRunStateJSON(JSONObject root)
    {
        for (int i = 0; i < mButtons.Count; i++)
        {
            if (root.keys.Contains("button" + (i+1)))
            {
                mButtons[i].LoadRunStateFromJSON(root["button" + (i + 1)]);
            }
        }
        if (root.keys.Contains("health"))
        {
            mPlayerHP.LoadRunStateFromJSON(root["health"]);
        }
        if (root.keys.Contains("healthloss"))
        {
            mHealthLoss = root["healthloss"].n;
        }

        base.LoadRunStateJSON(root);
    }
}

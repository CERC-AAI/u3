using System;
using System.Collections.Generic;
using UnityEngine;

public class GridEnvironment : EnvironmentEngine
{
    //int[,,] mGameState;
    //EnvironmentObject[,] mGameObjects;

    [Tooltip("Number of fixed updates per turn. A turn is one round of movement")]
    [Range(1, 100)]
    public int fixedUpdatesPerTurn = 10;

    public Texture2D levelImage;
    //public Texture2D gameState;
    public Player3D player;
    //public Vector2Int mCurrentPosition;
    //public int viewDistance = 5;
    public int arenaX = -1;
    public int arenaY = -1;

    public float gridSize = 1;

    int mArenaWidth = -1;
    int mArenaHeight = -1;

    int mTurnCounter = 0;
    bool mTurnOver = false;

    //int[] mObservationSize;

    //JSONObject mSaveRoot = new JSONObject();

    public enum Actions
    {
        RIGHT = 0,
        UP,
        LEFT,
        DOWN,
        NOOP,
    }

    public EnvironmentCallback OnEndTurnCallbacks;
    public EnvironmentCallback OnStartTurnCallbacks;

    public override void InitializeEnvironment(JSONObject loadParams)
    {
        Debug.Log("InitializeEnvironment");

        if (mArenaHeight == -1 || mArenaWidth == -1)
        {
            mArenaWidth = arenaX;
            mArenaHeight = arenaY;
        }

        if (levelImage)
        {
            mArenaWidth = levelImage.width;
            mArenaHeight = levelImage.height;
        }

        /*mGameState = new int[mArenaWidth, mArenaHeight, GetObjectType(null).Length];
        mGameObjects = new EnvironmentObject[mArenaWidth, mArenaHeight];

        mObservationSize = new int[3] { viewDistance * 2 + 1, viewDistance * 2 + 1, mGameState.GetLength(2) };*/

        //gameState = new Texture2D(mObservationSize[0], mObservationSize[1]);
        //gameState.filterMode = FilterMode.Point;

        if (levelImage)
        {
            /*List<GameObject> loadObjects = new List<GameObject>();

            for (int y = 0; y < levelImage.height; y++)
            {
                for (int x = 0; x < levelImage.width; x++)
                {
                    Color thisPixel = levelImage.GetPixel(x, y);

                    loadObjects.Clear();

                    GetObjectsFromType(thisPixel, loadObjects);

                    for (int i = 0; i < loadObjects.Count; i++)
                    {
                        LoadObject(loadObjects[i].gameObject, new Vector3(x, y, 0), thisPixel);
                    }
                }
            }*/
        }

        OnLoadFinsihed();

        

        /*List<EnvironmentObject> allObjects = mEngine.GetAllObjects();
        for (int i = 0; i < allObjects.Count; i++)
        {
            Vector2Int newPosition;

            if (GetGridPosition(allObjects[i].transform.position, out newPosition))
            {
                int[] type = GetObjectType(allObjects[i]);

                for (int j = 0; j < type.Length; j++)
                {
                    mGameState[newPosition.x, newPosition.y, j] = type[j];
                }
            }
        }*/


        base.InitializeEnvironment(loadParams);
    }

    public override void OnRunStarted()
    {
        mTurnCounter = fixedUpdatesPerTurn;
        mTurnOver = true;

        base.OnRunStarted();
    }

    /*public override void LoadEnvironmentElement(string elementName, string elementData = "")
    {
        switch (elementName.ToLower())
        {
            case "player":
                Vector2Int spawnPosition = Vector2Int.zero;

                if (!ParseVector2DInt(elementData, out spawnPosition))
                {
                    spawnPosition = GetRandomPosition();
                }

                Movement thisMovement = player.GetComponent<Movement>();

                thisMovement.CheckInitialized();

                if (thisMovement)
                {
                    thisMovement.MoveTo(GetPositionFromGrid(spawnPosition));
                }
                break;

            default:
                base.LoadEnvironmentElement(elementName, elementData);
                break;
        }
    }

    public override void OnObjectLoaded(EnvironmentObject loadedObject, Color loadValue)
    {
        Vector2Int position;
        GetGridPosition(loadedObject.GetPosition(), out position);

        OnObjectMoved(loadedObject, loadedObject.GetPosition(), loadedObject.GetPosition());

        base.OnObjectLoaded(loadedObject, loadValue);
    }

    public override void OnObjectMoved(EnvironmentObject thisObject, Vector3 oldPosition, Vector3 newPosition)
    {
        Vector2Int gridPosition;
        if (GetGridPosition(oldPosition, out gridPosition))
        {
            if (mGameObjects[gridPosition.x, gridPosition.y] == thisObject)
            {
                mGameObjects[gridPosition.x, gridPosition.y] = null;
            }
        }

        if (GetGridPosition(newPosition, out gridPosition))
        {
            if (gridPosition.x >= 0 && gridPosition.x < mArenaWidth && gridPosition.y >= 0 && gridPosition.y < mArenaHeight)
            {
                mGameObjects[gridPosition.x, gridPosition.y] = thisObject;
            }
        }
    }

    public EnvironmentObject GetGridObject(Vector2Int gridPosition)
    {
        if (gridPosition.x >= 0 && gridPosition.x < mArenaWidth && gridPosition.y >= 0 && gridPosition.y < mArenaHeight)
        {
            return mGameObjects[gridPosition.x, gridPosition.y];
        }

        return null;
    }

    public bool ParseVector2DInt(string data, out Vector2Int outVector)
    {
        outVector = Vector2Int.zero;

        Regex regex = new Regex(@"([\d]+),([\d]+)");
        MatchCollection matches = regex.Matches(data);

        if (matches.Count == 1)
        {
            int x = 0;
            int y = 0;

            if (int.TryParse(matches[0].Groups[1].Value, out x) && int.TryParse(matches[0].Groups[2].Value, out y))
            {
                outVector.x = x;
                outVector.y = y;

                return true;
            }
        }

        return false;
    }

    public override void OnInstantiate(EnvironmentObject newObject)
    {
        Vector2Int newPosition;

        if (GetGridPosition(player.transform.position, out newPosition))
        {
            int[] type = GetObjectType(newObject);

            for (int j = 0; j < type.Length; j++)
            {
                mGameState[newPosition.x, newPosition.y, j] = type[j];
            }
        }

        base.OnInstantiate(newObject);
    }*/

    public override void OnStepStarted()
    {
        if (mFixedTime > 0 && IsEndOfTurn() && OnEndTurnCallbacks != null)
        {
            OnEndTurnCallbacks();
        }

        base.OnStepStarted();
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        mTurnCounter--;

        if (mTurnCounter <= 0)
        {
            mTurnOver = true;
            mTurnCounter = fixedUpdatesPerTurn;
        }
        else
        {
            mTurnOver = false;
        }

        base.OnFixedUpdate(fixedDeltaTime);

        if (mTurnCounter == fixedUpdatesPerTurn)
        {
            if (OnStartTurnCallbacks != null)
            {
                OnStartTurnCallbacks();
            }
        }



        /*if (isEndOfTurn)
        {
            GetGridPosition(player.transform.position, out mCurrentPosition);
        }*/

        /*for (int x = 0; x < mGameState.GetLength(0); x++)
        {
            for (int y = 0; y < mGameState.GetLength(1); y++)
            {
                for (int j = 0; j < mGameState.GetLength(2); j++)
                {
                    mGameState[x, y, j] = 0;
                }
            }
        }*/

        /*List<EnvironmentObject> allObjects = mEngine.GetAllObjects();
        for (int i = 0; i < allObjects.Count; i++)
        {
            Vector2Int newPosition;

            if (GetGridPosition(allObjects[i].transform.position, out newPosition))
            {
                int[] type = GetObjectType(allObjects[i]);

                for (int j = 0; j < type.Length; j++)
                {
                    mGameState[newPosition.x, newPosition.y, j] = type[j];
                }
            }
        }*/

        /*if (gameState)
        {
            for (int x = gameState.width - 1; x >= 0; x--)
            {
                for (int y = 0; y < gameState.height; y++)
                {
                    int realX = mCurrentPosition.x + (x - viewDistance);
                    int realY = mCurrentPosition.y + (y - viewDistance);

                    if (realX >= 0 && realX < mGameState.GetLength(0) && realY >= 0 && realY < mGameState.GetLength(1))
                    {
                        for (int j = 0; j < mGameState.GetLength(2); j++)
                        {
                            gameState.SetPixel(x, y, new Color(mGameState[realX, realY, 0], mGameState[realX, realY, 1], mGameState[realX, realY, 2]));
                        }
                    }
                }
            }
            gameState.Apply();
        }*/
    }

    public override void OnLateFixedUpdate(float fixedDeltaTime)
    {
        base.OnLateFixedUpdate(fixedDeltaTime);

        if (mTurnOver)
        {
            //Debug.Log("OnEndTurn");
            //SendMessage("OnEndTurn");

            for (int i = 0; i < mEnvironmentObjects.Count; i++)
            {
                //This applies environment position constraints
                mEnvironmentObjects[i].Position = mEnvironmentObjects[i].Position;
            }
        }
    }

    public Vector3 GetPositionFromGrid(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * gridSize, gridPosition.y * gridSize);
    }

    public bool GetGridPosition(Vector3 globalPosition, out Vector2Int gridPosition)
    {
        /*Vector3 thisPosition = mEngine.GetEnvironmentPosition(globalPosition);
        int thisX = Mathf.RoundToInt(thisPosition.x);
        int thisY = Mathf.RoundToInt(thisPosition.y);

        gridPosition = new Vector2Int(thisX, thisY);

        if (thisX >= 0 && thisY >= 0 && thisX < mGameState.GetLength(0) && thisY < mGameState.GetLength(1))
        {
            return true;
        }*/

        gridPosition = Vector2Int.zero;

        return false;
    }

    /*public override bool IsValidAction(float[] vectorAction)
    {
        var action = Mathf.FloorToInt(vectorAction[0]);

        Vector2Int checkPosition = mCurrentPosition;

        switch ((GridEnvironment.Actions)action)
        {
            case GridEnvironment.Actions.NOOP:
                return true;

            case GridEnvironment.Actions.LEFT:
                checkPosition.x--;
                break;

            case GridEnvironment.Actions.RIGHT:
                checkPosition.x++;
                break;

            case GridEnvironment.Actions.UP:
                checkPosition.y++;
                break;

            case GridEnvironment.Actions.DOWN:
                checkPosition.y--;
                break;
        }

        //Debug.Log(mGameState[checkPosition.x][checkPosition.y]);

        return true;// mGameState[checkPosition.x][checkPosition.y] == 0;
    }

    public override bool ShouldLogInput(float[] vectorAction)
    {
        return true;
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = (int)Actions.NOOP;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut[0] = (int)Actions.RIGHT;
        }
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            actionsOut[0] = (int)Actions.UP;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut[0] = (int)Actions.LEFT;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            actionsOut[0] = (int)Actions.DOWN;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            mSaveRoot = GetComponent<EnvironmentEngine>().SaveEnvironmentState();

            string path = "JSONTest.txt";

            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(mSaveRoot.Print(true));
            writer.Close();
        }
        else if (Input.GetKey(KeyCode.Escape))
        {
            GetComponent<EnvironmentEngine>().LoadEnvironmentState(mSaveRoot);
        }
    }*/

    protected virtual int[] GetObjectType(EnvironmentObject checkObject)
    {
        return new int[]{ 1 };
    }

    protected virtual void GetObjectsFromType(Color color, List<GameObject> objectList)
    {
    }

    protected virtual void OnLoadFinsihed()
    {

    }

    

    /*protected override void OnGameOver()
    {
        SendData();

        base.OnGameOver();
    }

    public override Dictionary<string, string> GetTurnInfo()
    {
        Dictionary<string, string> turnInfo = new Dictionary<string, string>();

        return turnInfo;
    }

    public override void AddEndTurnInfo(int turnNumber, Dictionary<string, string> turnInfo)
    {
        if (ShouldSaveData())
        {
            if (!mGameEvents.HasField("Turns"))
            {
                mGameEvents.AddField("Turns", new JSONObject());
            }

            Dictionary<string, string> eventData = new Dictionary<string, string>();
            eventData["num"] = turnNumber.ToString();
            eventData["time"] = Time.unscaledTime.ToString();
            eventData["info"] = new JSONObject(turnInfo).ToString();

            mGameEvents["Turns"].Add(new JSONObject(turnInfo));
        }
    }

    public override ObservationSpec GetObservationSpec()
    {
        return ObservationSpec.Visual(mObservationSize[0], mObservationSize[1], mObservationSize[2]);
    }

    public override int Write(ObservationWriter writer)
    {
        //Debug.Log(Time.time + " Write observation");

        for (int x = mObservationSize[1] - 1; x >= 0; x--)
        {
            for (int y = 0; y < mObservationSize[0]; y++)
            {
                int realX = mCurrentPosition.x + (x - viewDistance);
                int realY = mCurrentPosition.y + (y - viewDistance);

                if (realX >= 0 && realX < mGameState.GetLength(0) && realY >= 0 && realY < mGameState.GetLength(1))
                {
                    for (int j = 0; j < mGameState.GetLength(2); j++)
                    {
                        writer[x, y, j] = mGameState[realX, realY, j];
                    }
                }
            }
        }

        return 1;
    }*/

    public int GetGridWidth()
    {
        return mArenaWidth;
    }

    public int GetGridHeight()
    {
        return mArenaHeight;
    }

    public Vector3 GetRandomPosition()
    {
        Vector3 randomPosition = Vector3.zero;

        randomPosition.x = GetEngine().GetRandomRange(1, GetGridWidth() - 1);
        randomPosition.y = GetEngine().GetRandomRange(1, GetGridHeight() - 1);

        return randomPosition;
    }

    public int GetArenaWidth()
    {
        return mArenaWidth;
    }
    public int GetArenaHeight()
    {
        return mArenaHeight;
    }

    /*public override void SetParam(string key, string value)
    {
        string[] values;
        switch (key.ToLower())
        {
            case "arenasize":
                values = value.Split(',');

                if (values.Length == 2)
                {
                    mArenaWidth = int.Parse(values[0]);
                    mArenaHeight = int.Parse(values[1]);
                }
                else
                {
                    Debug.LogError("Arena size must be 2 ints sepreated by a comma");
                }
                break;

            case "currentposition":
                values = value.Split(',');

                mCurrentPosition.x = int.Parse(values[0]);
                mCurrentPosition.y = int.Parse(values[1]);


                Movement thisMovement = player.GetComponent<Movement>();

                if (thisMovement)
                {
                    thisMovement.MoveTo(GetPositionFromGrid(mCurrentPosition));
                }
                break;

            default:
                base.SetParam(key, value);
                break;
        }
    }*/

    /*override protected void BuildRunStateJSON(JSONObject root)
    {
        root["currentposition"] = new JSONObject("\"" + mCurrentPosition.x + "," + mCurrentPosition.y + "\"");

        base.BuildRunStateJSON(root);
    }

    override protected void LoadRunStateJSON(JSONObject root)
    {
        if (root.keys.Contains("currentposition"))
        {
            SetParam("currentposition", root["currentposition"].str);
        }

        base.LoadRunStateJSON(root);
    }*/

    public float GetTurnTime()
    {
        return fixedUpdatesPerTurn * GetFixedDeltaTime();
    }

    override public Vector3 ApplyEnvironmentVelocity(Vector3 originalVelocity)
    {
        if (gridSize > 0)
        {
            float frameSpeed = gridSize / GetTurnTime();

            originalVelocity.x = Mathf.Sign(originalVelocity.x) * Mathf.Ceil(Mathf.Abs(originalVelocity.x) / frameSpeed) * frameSpeed;
            originalVelocity.y = Mathf.Sign(originalVelocity.y) * Mathf.Ceil(Mathf.Abs(originalVelocity.y) / frameSpeed) * frameSpeed;
            originalVelocity.z = Mathf.Sign(originalVelocity.z) * Mathf.Ceil(Mathf.Abs(originalVelocity.z) / frameSpeed) * frameSpeed;
        }

        return originalVelocity;
    }

    /*public Vector3 GetMoveVelocity(Vector3 velocity, float speed)
    {
        if (gridSize > 0)
        {
            float frameSpeed = speed * gridSize / (GetFixedDeltaTime() * fixedUpdatesPerTurn);

            speed = Mathf.Sign(speed) * Mathf.Ceil(Mathf.Abs(speed) / frameSpeed) * frameSpeed;
        }

        return velocity * speed;
    }*/

    /*public Vector3 GetDiscretePosition(Vector3 position, bool isEndTurn)
    {
        if (gridSize > 0)
        {
            float fixSize = gridSize / fixedUpdatesPerTurn;
            if (isEndTurn)
            {
                fixSize = gridSize;
            }

            position.x = Mathf.Round(position.x / fixSize) * fixSize;
            position.y = Mathf.Round(position.y / fixSize) * fixSize;
            position.z = Mathf.Round(position.z / fixSize) * fixSize;
        }

        return position;
    }*/

    public override Vector3 ApplyEnvironmentPosition(Vector3 originalLocalPosition)
    {
        if (gridSize > 0)
        {
            //Position can only be a factor of grid size.
            float fixSize = gridSize / fixedUpdatesPerTurn;
            if (IsEndOfTurn())
            {
                fixSize = gridSize;
            }

            originalLocalPosition.x = Mathf.Round(originalLocalPosition.x / fixSize) * fixSize;
            originalLocalPosition.y = Mathf.Round(originalLocalPosition.y / fixSize) * fixSize;
            originalLocalPosition.z = Mathf.Round(originalLocalPosition.z / fixSize) * fixSize;

            return originalLocalPosition;
        }

        return base.ApplyEnvironmentPosition(originalLocalPosition);
    }

    public bool IsEndOfTurn()
    {
        return mTurnOver;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Movement))]
public class GridPlayer : EnvironmentAgent
{
    //public members
    public float tilesPerSecondSpeed = 1;
    public bool randomizeStartPosition = true;

    GridEnvironment mGridEngine;


    //private members
    Movement mMovement;
    HealthBar mHealthBar;

    [ACTION]
    public GridEnvironment.Actions movementState;

    // Should be 3 vector3s for position, rotation, and scale
    [Sensor]
    public Vector3 position;

    // Add a dummy Camera to the agent
    // Give height and width of the camera to Sensor()
    [Sensor(width: 84, height: 84, grayscale = false, sensorName = "Camera")]
    public Camera camera;


    public float MaxSpeed
    {
        get { return tilesPerSecondSpeed; }
        set { tilesPerSecondSpeed = value; }
    }

    /*public override void AppendActionLists(List<ActionInfo> actionsList)
    {
        ActionInfo basicMovement = new ActionInfo(DoMovement, (int)GridEnvironment.Actions.NOOP);

        actionsList.Add(basicMovement);

        base.AppendActionLists(actionsList);
    }*/

    override protected void Initialize()
    {
        mMovement = GetComponent<Movement>();
        mHealthBar = GetComponent<HealthBar>();
        mGridEngine = GetComponentInParent<GridEnvironment>();
        base.Initialize();

    }

    public override void OnRunStarted()
    {
        if (randomizeStartPosition)
        {
            mParentObject.Position = mGridEngine.GetRandomPosition();
        }

        mMovement.SetVelocity(Vector3.zero);

        base.OnRunStarted();
    }

    public override bool ShouldRequestDecision(long fixedUdpateNumber)
    {
        if (mGridEngine)
        {
            return mGridEngine.IsEndOfTurn();
        }

        return base.ShouldRequestDecision(fixedUdpateNumber);
    }

    override public bool ShouldBlockDecision(ActionBuffers actions)
    {
        return movementState == GridEnvironment.Actions.NOOP;
    }

    override public void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        // TODO: think about keeping observations every step or every frame
    }

    protected override void OnDied()
    {
        base.OnDied();

        mMovement.Velocity = Vector2.zero;
    }

    void Update()
    {
        DoMovement();
        position = transform.position;

    }

    void DoMovement()
    {
        float speed = tilesPerSecondSpeed;
        if (mGridEngine)
        {
            speed = tilesPerSecondSpeed * mGridEngine.gridSize / mGridEngine.GetTurnTime();
        }

        switch ((GridEnvironment.Actions)movementState)
        {
            case GridEnvironment.Actions.NOOP:
                mMovement.Velocity = Vector2.zero * speed;
                break;

            case GridEnvironment.Actions.UP:
                mMovement.Velocity = Vector2.up * speed;
                break;

            case GridEnvironment.Actions.LEFT:
                mMovement.Velocity = Vector2.left * speed;
                break;

            case GridEnvironment.Actions.DOWN:
                mMovement.Velocity = Vector2.down * speed;
                break;

            case GridEnvironment.Actions.RIGHT:
                mMovement.Velocity = Vector2.right * speed;
                break;

            default:
                throw new ArgumentException("Invalid action value");
        }
    }

    override public void Heuristic(in ActionBuffers actionsOut)
    {
        /*test = UnityEngine.Random.Range(0, 2) == 1;
        test2 = UnityEngine.Random.onUnitSphere;*/

        movementState = GridEnvironment.Actions.NOOP;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            movementState = GridEnvironment.Actions.RIGHT;
        }
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            movementState = GridEnvironment.Actions.UP;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            movementState = GridEnvironment.Actions.LEFT;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            movementState = GridEnvironment.Actions.DOWN;
        }

        base.Heuristic(actionsOut);
    }

    /*public override void OnCollision(EnvironmentObject otherObject)
    {
        if (otherObject.name.Contains("Wall"))
        {
            mEngine.AddReward(-0.01f);
        }

        base.OnCollision(otherObject);
    }*/
}

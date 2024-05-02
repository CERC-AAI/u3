using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using static U3DPlayer;

public class SpawnManager : EnvironmentComponent
{
    public override void OnRunStarted()
    {
        Debug.Log("SpawnManager");

        List<U3DPlayer> agents = GetEngine().GetEnvironmentComponents<U3DPlayer>();
        MetatileManager metatileManager = GetEngine().GetCachedEnvironmentComponent<MetatileManager>();

        List<Vector3> spawnLocations = metatileManager.GetPermissibleSpawnLocations();

        foreach (U3DPlayer agent in agents)
        {
            Vector3 randomLocation = spawnLocations[UnityEngine.Random.Range(0, spawnLocations.Count)];

            Debug.Log($"Spawn at: {randomLocation}");

            U3DPlayer.AgentState agentState = (U3DPlayer.AgentState)agent.SaveTrialData();

            agentState.position = randomLocation;
            agentState.rotation = Quaternion.Euler(0.0f, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);
            agentState.cameraPlanarDirection = agentState.rotation * Vector3.forward;

            agent.LoadTrialData(agentState);
        }

        base.OnRunStarted();
    }
}
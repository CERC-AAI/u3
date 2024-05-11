using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using static U3DPlayer;
using NUnit;
using UnityEngine.UIElements;
using UnityEngine.Assertions.Must;
using System.Linq;

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
            int spawnRandom = UnityEngine.Random.Range(0, spawnLocations.Count);
            Vector3 randomLocation = spawnLocations[spawnRandom];
            spawnLocations.RemoveAt(spawnRandom);

            Debug.Log($"Spawn at: {randomLocation}");

            U3DPlayer.AgentState agentState = (U3DPlayer.AgentState)agent.SaveTrialData();

            agentState.position = randomLocation + new Vector3(0f, 0.5f);
            agentState.rotation = Quaternion.Euler(0.0f, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);
            agentState.cameraPlanarDirection = agentState.rotation * Vector3.forward;

            agent.LoadTrialData(agentState);
        }

        ProductionRuleManager productionRuleManager = GetEngine().GetCachedEnvironmentComponent<ProductionRuleManager>();
        for (int i = 0; i < 3; i++)
        {
            EnvironmentObject prodRuleObject = GetEngine().CreateEnvironmentObject(productionRuleManager.productionRuleObjectPrefab.gameObject);
            ProductionRuleObject prodRuleObj = prodRuleObject.GetComponent<ProductionRuleObject>();

            int shape = UnityEngine.Random.Range(0, productionRuleManager.productionRulePrefabs.Count);
            int color = UnityEngine.Random.Range(0, ProductionRuleIdentifier.colorDict.Count);

            prodRuleObj.ProductionRuleObjectInitialize(productionRuleManager.productionRulePrefabs[shape].name, ProductionRuleIdentifier.colorDict.Keys.ToArray()[color]);

            int spawnRandom = UnityEngine.Random.Range(0, spawnLocations.Count);
            Vector3 randomLocation = spawnLocations[spawnRandom];
            spawnLocations.RemoveAt(spawnRandom);

            prodRuleObj.transform.position = randomLocation + new Vector3(0f, 1.0f); 
            prodRuleObj.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0.0f, 360.0f), UnityEngine.Random.Range(0.0f, 360.0f), UnityEngine.Random.Range(0.0f, 360.0f));
        }

        base.OnRunStarted();
    }
}
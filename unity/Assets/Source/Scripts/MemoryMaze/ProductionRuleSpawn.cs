using NUnit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionRuleSpawn : EnvironmentTile
{
    static List<string> ColorMap = new List<string> {
                "red",
                "blue",
                "green",
                "orange",
                "yellow",
                };

    const int MAX_SPAWNS = 5;

    static bool mHasSpawned = false;
    public ProductionRuleObject goalPrefab;
    public ProductionRuleIdentifier identifier;

    static float MIN_DISTANCE = 8;

    protected override void Initialize()
    {
        base.Initialize();

        mHasSpawned = false;
        //Debug.Log("mHasSpawned = false");
    }

    public override void OnRunStarted()
    {
        // Only do once
        if (!mHasSpawned)
        {
            List<ProductionRuleSpawn> addedSpawns = new List<ProductionRuleSpawn>();

            mHasSpawned = true;
            Debug.Log("Spawn production rule!");

            for (float currentDistance = MIN_DISTANCE; currentDistance > 0.9f && addedSpawns.Count < MAX_SPAWNS; currentDistance *= 0.8f)
            {
                List<ProductionRuleSpawn> allSpawns = GetEngine().GetEnvironmentComponents<ProductionRuleSpawn>();

                ShuffleUtility.ShuffleList(allSpawns);

                while (allSpawns.Count > 0 && addedSpawns.Count < MAX_SPAWNS)
                {
                    Vector3 currentPosition = allSpawns[0].transform.position;
                    bool isValid = true;

                    for (int i = 0; i < addedSpawns.Count; i++)
                    {
                        if (Vector3.Distance(addedSpawns[i].transform.position, currentPosition) < currentDistance)
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid)
                    {
                        addedSpawns.Add(allSpawns[0]);
                    }

                    allSpawns.RemoveAt(0);
                }
            }

            ProductionRuleManager manager = GetEngine().GetCachedEnvironmentComponent<ProductionRuleManager>();
            List<ProductionRule> rewardRules = new List<ProductionRule>();
            for (int i = 0; i < addedSpawns.Count; i++)
            {
                // Instantiate the production rule object
                EnvironmentObject prodRuleObject = GetEngine().CreateEnvironmentObject(addedSpawns[i].goalPrefab.gameObject);
                ProductionRuleObject prodRuleObj = prodRuleObject.GetComponent<ProductionRuleObject>();
                prodRuleObj.ProductionRuleObjectInitialize(addedSpawns[i].identifier.ObjectShape, ColorMap[i % ColorMap.Count], 0.5f);
                prodRuleObj.transform.position = addedSpawns[i].transform.position;

                ProductionRuleIdentifier identifier = new ProductionRuleIdentifier(addedSpawns[i].identifier.ObjectShape, ColorMap[i % ColorMap.Count]);
                ProductionRuleCondition newCondition = new ProductionRuleCondition(ProductionRuleManager.CONDITION.AGENT_NEAR, identifier);
                ProductionRuleAction newAction = new ProductionRuleAction();
                newAction.Initialize(ProductionRuleManager.ACTION.REWARD_TOGGLE, PredicateObjects.SUBJECT, 1.0f);

                ProductionRule newRule = new ProductionRule(newCondition, newAction);
                manager.AddRule(newRule);
                newRule.ToggleActive(false);
                rewardRules.Add(newRule);
            }

            ShuffleUtility.ShuffleList(rewardRules);
            rewardRules[0].ToggleActive(true);
        }

        base.OnRunStarted();
    }

    public override void OnRunEnded()
    {
        //mHasSpawned = false;

        base.OnRunEnded();
    }
}

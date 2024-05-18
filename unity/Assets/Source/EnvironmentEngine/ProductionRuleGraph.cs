using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using static ProductionRuleManager;

[Serializable]
public class ProductionRuleGraph : EnvironmentComponent
{

    public Dictionary<CONDITION, int> necessaryObjectCountForCondition = new Dictionary<CONDITION, int>()
    {
        {CONDITION.NEAR, 2},
        {CONDITION.CONTACT, 2},
        {CONDITION.USE, 1},
        {CONDITION.DROP, 1},
        {CONDITION.PICKUP, 1},
        {CONDITION.THROW, 1},
        {CONDITION.HOLD, 1},
        {CONDITION.SEE, 1},
        {CONDITION.NONE, 0}
    };

    public Dictionary<Action, int> necessaryObjectCountForAction = new Dictionary<Action, int>()
    {
        {Action.SPAWN, 0},
        {Action.REMOVE, 2}, // don't want to end up with no objects in the environment
        {Action.REWARD, 0},
        {Action.PRINT, 0}
    };

    public Dictionary<Action, List<PredicateObjects>> PermissiblepredicateObjectsForAction = new Dictionary<Action, List<PredicateObjects>>()
    {
        {Action.SPAWN, new List<PredicateObjects>(){PredicateObjects.NONE, PredicateObjects.SUBJECT}},
        // {Action.REMOVE, new List<PredicateObjects>(){PredicateObjects.SUBJECT, PredicateObjects.BOTH}}, # TODO: fix double-predicate necessaryObjectCountForAction conflict
        {Action.REMOVE, new List<PredicateObjects>(){PredicateObjects.SUBJECT}},
        {Action.REWARD, new List<PredicateObjects>(){PredicateObjects.NONE}},
        {Action.PRINT, new List<PredicateObjects>(){PredicateObjects.NONE}}
    };

    //Node currentNode; // points to the current state of the environment

    public int minInitialObjects = 1;
    public int maxInitialObjects = 3;
    public int minRules = 1;
    public int maxRules = 3;
    public int minDeadends = 1;
    public int maxDeadends = 1;


    /*[Serializable]
    public class Node
    {
        public string nodeID;
        public List<ProductionRule> productionRules;
        public List<Node> children;
        public List<Node> parents;
        public List<ProductionRuleIdentifier> state;
    }*/

    //public List<Node> graphNodes;

    /*public List<ProductionRule> GetCurrentProductionRules()
    {
        return currentNode.productionRules;
    }*/

    /*public void ForwardWalk(ProductionRule productionRule)
    {
        int childNodeIndex = currentNode.productionRules.IndexOf(productionRule);
        if (childNodeIndex == -1)
        {
            Debug.Log("End of trial reached");
        }
        else
        {
            currentNode = currentNode.children[childNodeIndex];
        }
    }*/

    /*public void BuildProductionRuleGraph(List<ProductionRuleIdentifier> rootState, int numStates = 10, int numRules = 3)
    {
        numStates = UnityEngine.Random.Range(minRules, maxRules);
        numRules = UnityEngine.Random.Range()

        List<Node> nodes = new List<Node>();
        Node rootNode = new Node();
        rootNode.nodeID = "node0";
        rootNode.productionRules = new List<ProductionRule>();
        rootNode.children = new List<Node>();
        rootNode.parents = new List<Node>();
        rootNode.state = rootState;
        nodes.Add(rootNode);
        currentNode = rootNode;

        while (nodes.Count < numStates)
        {
            int randomNumRules = UnityEngine.Random.Range(1, numRules);
            if (nodes.Count + randomNumRules > numStates)
            {
                randomNumRules = numStates - nodes.Count;
            }

            Node node = nodes[UnityEngine.Random.Range(0, nodes.Count)];
            if (node.children.Count > 0)
            {
                node = node.children[UnityEngine.Random.Range(0, node.children.Count)];
            }
            List<ProductionRule> sampledProductionRules = SampleForwardRules(node.state, randomNumRules);
            node.productionRules.AddRange(sampledProductionRules);

            foreach (ProductionRule productionRule in sampledProductionRules)
            {
                Node childNode = new Node();
                childNode.nodeID = $"node{nodes.Count}";
                childNode.productionRules = new List<ProductionRule>();
                childNode.children = new List<Node>();
                childNode.parents = new List<Node>();
                childNode.state = GetNextState(node.state, productionRule);
                node.children.Add(childNode);
                childNode.parents.Add(node);
                nodes.Add(childNode);
            }
        }
        graphNodes = nodes;
    }*/

    public List<ProductionRuleIdentifier> GetRandomInitialState()
    {
        ProductionRuleManager productionRuleManager = GetEngine().GetCachedEnvironmentComponent<ProductionRuleManager>();

        List<string> shapeKeys = new List<string>();

        foreach (ProductionRulePrefab prefab in productionRuleManager.productionRulePrefabs)
        {
            shapeKeys.Add(prefab.name);
        }

        List<ProductionRuleIdentifier> initialState = new List<ProductionRuleIdentifier>();
        int numObjects = UnityEngine.Random.Range(minInitialObjects, maxInitialObjects + 1);
        numObjects = Mathf.Max(numObjects, 1);
        for (int i = 0; i < numObjects; i++)
        {
            List<string> colorKeys = new List<string>(ProductionRuleIdentifier.colorDict.Keys);
            string shape = shapeKeys[UnityEngine.Random.Range(0, shapeKeys.Count)];
            string color = colorKeys[UnityEngine.Random.Range(0, colorKeys.Count)];
            ProductionRuleIdentifier productionRuleIdentifier = new ProductionRuleIdentifier(shape, color);
            initialState.Add(productionRuleIdentifier);
        }
        return initialState;
    }

    public List<ProductionRule> BuildProductionRuleSet(List<ProductionRuleIdentifier> initialState)
    {
        int numRules = UnityEngine.Random.Range(minRules, maxRules + 1);
        int numDeadEnds = UnityEngine.Random.Range(minDeadends, maxDeadends + 1);

        List<ProductionRule> productionRules = new List<ProductionRule>();
        List<List<ProductionRuleIdentifier>> stateSpace = new List<List<ProductionRuleIdentifier>>();
        List<ProductionRuleIdentifier> currentState = initialState;

        // Step 1: Create a linear path of production rules
        for (int i = 0; i < numRules; i++)
        {
            ProductionRule productionRule = SampleForwardRule(currentState);

            int numTries = 0;
            while (OverlapsWithExistingProductionRules(productionRule, productionRules) && numTries < 100)
            {
                productionRule = SampleForwardRule(currentState);
                numTries++;
            }
            if (numTries == 100)
            {
                Debug.Log("Could not sample a production rule that does not overlap with existing production rules");
                break;
            }

            if (i == numRules - 1)
            {
                // Make sure there is a reward in the last production rule
                productionRule = SampleForwardRule(currentState, actionIsReward: true);
            }

            productionRules.Add(productionRule);
            List<ProductionRuleIdentifier> nextState = GetNextState(currentState, productionRule);
            stateSpace.Add(nextState);
            currentState = nextState;
        }

        // Step 2: Add "dead ends" at random points along the linear path
        for (int j = 0; j < numDeadEnds; j++)
        {
            // Randomly choose a point along the linear path to add a dead end
            int linearPathIndex = UnityEngine.Random.Range(0, numRules);
            List<ProductionRuleIdentifier> deadEndState = stateSpace[linearPathIndex];

            ProductionRule deadEndRule = SampleForwardRule(deadEndState, actionNotReward: true);

            int numTries = 0;
            while (OverlapsWithExistingProductionRules(deadEndRule, productionRules) && numTries < 100)
            {
                deadEndRule = SampleForwardRule(deadEndState, actionNotReward: true);
                numTries++;
            }
            if (numTries == 100)
            {
                Debug.Log("Could not sample a dead-end rule that does not overlap with existing production rules");
                continue;
            }

            productionRules.Add(deadEndRule);
            List<ProductionRuleIdentifier> nextState = GetNextState(deadEndState, deadEndRule);
            stateSpace.Add(nextState);
        }

        return productionRules;
    }

    /*public void UpdateProductionRuleGraph(ProductionRule productionRule)
    {
        ForwardWalk(productionRule);
    }*/

    // TODO: Make sure that .Contains() works for ProductionRuleConditions
    public bool OverlapsWithExistingProductionRules(ProductionRule productionRule, List<ProductionRule> productionRules)
    {
        bool productionRuleSubsumes = false;
        bool productionRuleIsSubsumed = false;
        // The conditions of the production rule must not subsume or be subsumed by the conditions of any existing production rules
        foreach (ProductionRule existingProductionRule in productionRules)
        {
            // ProdRule A is subsumed by ProdRule B if the list of prod rule conditions of ProdRule A is a subset of the list of prod rule conditions of prodrule B
            // ProdRule A subsumes ProdRule B if the list of prod rule conditions of ProdRule A is a superset of the list of prod rule conditions of prodrule B
            // The productionRule can neither subsume nor be subsumed by any existing production rules
            bool productionRuleSubsumesExistingProductionRule = true;
            bool existingProductionRuleSubsumesProductionRule = true;
            // First, check if the list of conditions of productionRule is a subset of the list of conditions of existingProductionRule
            foreach (ProductionRuleCondition productionRuleCondition in productionRule.conditions)
            {
                if (!existingProductionRule.conditions.Contains(productionRuleCondition))
                {
                    productionRuleSubsumesExistingProductionRule = false;
                    break;
                }
            }

            // Next, check if the list of conditions of existingProductionRule is a subset of the list of conditions of productionRule
            foreach (ProductionRuleCondition existingProductionRuleCondition in existingProductionRule.conditions)
            {
                if (!productionRule.conditions.Contains(existingProductionRuleCondition))
                {
                    existingProductionRuleSubsumesProductionRule = false;
                    break;
                }
            }

            productionRuleSubsumes = productionRuleSubsumes || productionRuleSubsumesExistingProductionRule;
            productionRuleIsSubsumed = productionRuleIsSubsumed || existingProductionRuleSubsumesProductionRule;

            if (productionRuleSubsumes || productionRuleIsSubsumed)
            {
                break;
            }
        }

        return productionRuleSubsumes || productionRuleIsSubsumed;
    }

    public List<ProductionRule> SampleForwardRules(List<ProductionRuleIdentifier> currentState, int numRules, bool actionIsReward = false, bool actionNotReward = false)
    {
        // Samples multiple forward rules from the same initial state
        List<ProductionRule> productionRules = new List<ProductionRule>();
        for (int i = 0; i < numRules; i++)
        {
            ProductionRule productionRule = SampleForwardRule(currentState, actionIsReward, actionNotReward);
            productionRules.Add(productionRule);
        }
        return productionRules;
    }

    public ProductionRule SampleForwardRule(List<ProductionRuleIdentifier> currentState, bool actionIsReward = false, bool actionNotReward = false)
    {
        List<ProductionRuleCondition> conditions = new List<ProductionRuleCondition>();
        ProductionRuleCondition productionRuleCondition = sampleForwardProductionRuleCondition(currentState);
        conditions.Add(productionRuleCondition);

        List<ProductionRuleAction> actions = new List<ProductionRuleAction>();
        ProductionRuleAction productionRuleAction = SampleForwardProductionRuleAction(currentState, actionIsReward, actionNotReward);
        actions.Add(productionRuleAction);

        ProductionRule productionRule = new ProductionRule(conditions, actions);

        return productionRule;
    }

    public ProductionRuleCondition sampleForwardProductionRuleCondition(List<ProductionRuleIdentifier> currentState)
    {
        CONDITION condition = (CONDITION)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CONDITION)).Length - 1);
        int neededObjectCount = necessaryObjectCountForCondition[condition];

        while (condition == CONDITION.NONE || neededObjectCount > currentState.Count)
        {
            if (condition == CONDITION.NONE)
            {
                Debug.Log("Sampled NONE condition, sampling again.");
            }
            else
            {
                Debug.Log($"Not enough objects for condition {condition}, sampling again.");
            }
            condition = (CONDITION)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CONDITION)).Length - 1);
            neededObjectCount = necessaryObjectCountForCondition[condition];
        }

        ProductionRuleIdentifier subjectIdentifier = null;
        ProductionRuleIdentifier objectIdentifier = null;

        if (neededObjectCount == 2)
        {
            int subjectIndex = UnityEngine.Random.Range(0, currentState.Count);
            int objectIndex = UnityEngine.Random.Range(0, currentState.Count);
            while (subjectIndex == objectIndex)
            {
                objectIndex = UnityEngine.Random.Range(0, currentState.Count);
            }

            subjectIdentifier = currentState[subjectIndex];
            objectIdentifier = currentState[objectIndex];
        }
        else if (neededObjectCount == 1)
        {
            subjectIdentifier = currentState[UnityEngine.Random.Range(0, currentState.Count)];
        }
        else if (neededObjectCount != 0)
        {
            throw new ArgumentException("neededObjectCount not recognized; check condition requirements");
        }

        ProductionRuleCondition productionRuleCondition = new ProductionRuleCondition(condition, subjectIdentifier, objectIdentifier);

        return productionRuleCondition;
    }
    public ProductionRuleAction SampleForwardProductionRuleAction(List<ProductionRuleIdentifier> currentState, bool isReward = false, bool notReward = false)
    {
        Action action = (Action)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Action)).Length);
        if (isReward && notReward)
        {
            throw new ArgumentException("isReward and notReward cannot both be true");
        }
        if (isReward)
        {
            action = Action.REWARD;
        }
        else if (notReward)
        {
            action = (Action)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Action)).Length);
            while (action == Action.REWARD)
            {
                action = (Action)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Action)).Length);
            }
        }

        while (currentState.Count - necessaryObjectCountForAction[action] < 0)
        {
            Debug.Log($"Not enough objects for action {action}, sampling again.");
            action = (Action)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Action)).Length);
        }

        float reward = UnityEngine.Random.Range(0.0f, 1.0f);

        List<PredicateObjects> permissiblePredicateObjects = PermissiblepredicateObjectsForAction[action];
        PredicateObjects predicateObjects = permissiblePredicateObjects[UnityEngine.Random.Range(0, permissiblePredicateObjects.Count)];
        ProductionRuleIdentifier identifier = currentState[UnityEngine.Random.Range(0, currentState.Count)];

        ProductionRuleAction productionRuleAction = new ProductionRuleAction(action, reward, predicateObjects, identifier);

        return productionRuleAction;
    }

    public List<ProductionRuleIdentifier> GetNextState(List<ProductionRuleIdentifier> currentState, ProductionRule forwardProductionRule)
    {
        // TODO: move to ProductionRuleAction
        Action action = forwardProductionRule.actions[0].action;
        ProductionRuleIdentifier actionTargetIdentifier = null;
        if (forwardProductionRule.actions[0].predicateObjects == PredicateObjects.SUBJECT)
        {
            actionTargetIdentifier = forwardProductionRule.conditions[0].subjectIdentifier;
        }
        else if (forwardProductionRule.actions[0].predicateObjects == PredicateObjects.OBJECT)
        {
            actionTargetIdentifier = forwardProductionRule.conditions[0].objectIdentifier;
        }
        else if (forwardProductionRule.actions[0].predicateObjects == PredicateObjects.BOTH)
        {
            actionTargetIdentifier = forwardProductionRule.conditions[0].subjectIdentifier; //TODO: change this to both
        }
        else if (forwardProductionRule.actions[0].predicateObjects == PredicateObjects.NONE)
        {
            actionTargetIdentifier = forwardProductionRule.actions[0].identifier;
        }
        else
        {
            throw new ArgumentException("PredicateObjects not recognized");
        }

        List<ProductionRuleIdentifier> nextState = new List<ProductionRuleIdentifier>();

        foreach (ProductionRuleIdentifier identifier in currentState)
        {
            nextState.Add(identifier);
        }
        if (action == Action.SPAWN)
        {
            nextState.Add(actionTargetIdentifier);
        }
        else if (action == Action.REMOVE)
        {
            nextState.Remove(actionTargetIdentifier);
        }
        else if (action == Action.REWARD)
        {
            // do nothing
        }
        else if (action == Action.PRINT)
        {
            // do nothing
        }
        else
        {
            throw new ArgumentException("Action not recognized");
        }
        return nextState;
    }

    public List<ProductionRuleIdentifier> GetPreviousState(List<ProductionRuleIdentifier> currentState, ProductionRule backwardProductionRule)
    {
        Action action = backwardProductionRule.actions[0].action;
        ProductionRuleIdentifier actionTargetIdentifier = null;
        if (backwardProductionRule.actions[0].predicateObjects == PredicateObjects.SUBJECT)
        {
            actionTargetIdentifier = backwardProductionRule.conditions[0].subjectIdentifier;
        }
        else if (backwardProductionRule.actions[0].predicateObjects == PredicateObjects.OBJECT)
        {
            actionTargetIdentifier = backwardProductionRule.conditions[0].objectIdentifier;
        }
        else if (backwardProductionRule.actions[0].predicateObjects == PredicateObjects.BOTH)
        {
            throw new ArgumentException("BOTH predicateObjects not implemented");
        }
        else if (backwardProductionRule.actions[0].predicateObjects == PredicateObjects.NONE)
        {
            actionTargetIdentifier = backwardProductionRule.actions[0].identifier;
        }
        else
        {
            throw new ArgumentException("PredicateObjects not recognized");
        }

        List<ProductionRuleIdentifier> previousState = new List<ProductionRuleIdentifier>();

        foreach (ProductionRuleIdentifier identifier in currentState)
        {
            previousState.Add(identifier);
        }
        if (action == Action.SPAWN)
        {
            previousState.Remove(actionTargetIdentifier);
        }
        else if (action == Action.REMOVE)
        {
            previousState.Add(actionTargetIdentifier);
        }
        else
        {
            throw new ArgumentException("Action not recognized");
        }
        return previousState;
    }

}
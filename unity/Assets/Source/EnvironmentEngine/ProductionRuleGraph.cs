using System.Threading;
using System.Net.Sockets;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using static ProductionRuleManager;

[Serializable]
public class ProductionRuleGraph : EnvironmentComponent
{

    public Dictionary<ProductionRuleManager.CONDITION, int> necessaryObjectCountForCondition = new Dictionary<ProductionRuleManager.CONDITION, int>()
    {
        {ProductionRuleManager.CONDITION.NEAR, 2},
        {ProductionRuleManager.CONDITION.CONTACT, 2},
        {ProductionRuleManager.CONDITION.USE, 1},
        {ProductionRuleManager.CONDITION.DROP, 1},
        {ProductionRuleManager.CONDITION.PICKUP, 1},
        {ProductionRuleManager.CONDITION.THROW, 1},
        {ProductionRuleManager.CONDITION.HOLD, 1},
        {ProductionRuleManager.CONDITION.SEE, 1},
        {ProductionRuleManager.CONDITION.NONE, 0}
    };

    public Dictionary<ProductionRuleManager.ACTION, int> necessaryObjectCountForAction = new Dictionary<ProductionRuleManager.ACTION, int>()
    {
        {ProductionRuleManager.ACTION.SPAWN, 0},
        {ProductionRuleManager.ACTION.REMOVE, 2}, // don't want to end up with no objects in the environment
        {ProductionRuleManager.ACTION.SWAP, 1},
        {ProductionRuleManager.ACTION.REWARD, 0},
        {ProductionRuleManager.ACTION.PRINT, 0}
    };

    public Dictionary<ProductionRuleManager.ACTION, List<PredicateObjects>> PermissiblepredicateObjectsForAction = new Dictionary<ProductionRuleManager.ACTION, List<PredicateObjects>>()
    {
        {ProductionRuleManager.ACTION.SPAWN, new List<PredicateObjects>(){PredicateObjects.NONE}},
        // {ProductionRuleManager.ACTION.REMOVE, new List<PredicateObjects>(){PredicateObjects.SUBJECT, PredicateObjects.BOTH}}, # TODO: fix double-predicate necessaryObjectCountForAction conflict
        {ProductionRuleManager.ACTION.SWAP, new List<PredicateObjects>(){PredicateObjects.SUBJECT, PredicateObjects.OBJECT, PredicateObjects.BOTH}},
        {ProductionRuleManager.ACTION.REMOVE, new List<PredicateObjects>(){PredicateObjects.SUBJECT, PredicateObjects.OBJECT}},
        {ProductionRuleManager.ACTION.REWARD, new List<PredicateObjects>(){PredicateObjects.NONE}},
        {ProductionRuleManager.ACTION.PRINT, new List<PredicateObjects>(){PredicateObjects.NONE}}
    };

    List<ProductionRuleManager.CONDITION> deadEndConditions = new List<ProductionRuleManager.CONDITION> {
        ProductionRuleManager.CONDITION.NEAR,
        ProductionRuleManager.CONDITION.CONTACT,
        ProductionRuleManager.CONDITION.DROP,
        ProductionRuleManager.CONDITION.PICKUP,
        ProductionRuleManager.CONDITION.THROW,
        ProductionRuleManager.CONDITION.HOLD,
    };

    List<ProductionRuleManager.ACTION> deadEndActions = new List<ProductionRuleManager.ACTION> {
        ProductionRuleManager.ACTION.REMOVE,
        ProductionRuleManager.ACTION.SWAP,
        ProductionRuleManager.ACTION.SPAWN,
    };

    List<ProductionRuleManager.CONDITION> rewardConditions = new List<ProductionRuleManager.CONDITION> {
        ProductionRuleManager.CONDITION.NEAR,
        ProductionRuleManager.CONDITION.CONTACT,
        ProductionRuleManager.CONDITION.HOLD,
    };

    List<ProductionRuleManager.ACTION> rewardActions = new List<ProductionRuleManager.ACTION> {
        ProductionRuleManager.ACTION.REWARD,
    };

    List<ProductionRuleManager.CONDITION> chainConditions = new List<ProductionRuleManager.CONDITION> {
        ProductionRuleManager.CONDITION.CONTACT,
        ProductionRuleManager.CONDITION.NEAR,
        ProductionRuleManager.CONDITION.CONTACT,
        ProductionRuleManager.CONDITION.NEAR,
        ProductionRuleManager.CONDITION.CONTACT,
        ProductionRuleManager.CONDITION.NEAR,
        ProductionRuleManager.CONDITION.PICKUP,
        ProductionRuleManager.CONDITION.THROW,
        ProductionRuleManager.CONDITION.DROP,
        ProductionRuleManager.CONDITION.HOLD,
    };

    List<ProductionRuleManager.ACTION> chainActions = new List<ProductionRuleManager.ACTION> {
        ProductionRuleManager.ACTION.SWAP,
        ProductionRuleManager.ACTION.SPAWN,
    };



    //Node currentNode; // points to the current state of the environment

    public int minInitialObjects = 1;
    public int maxInitialObjects = 3;
    public int minRules = 1;
    public int maxRules = 6;
    public int minDeadends = 0;
    public int maxDeadends = 0;


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
        numStates = GetEngine().GetRandomRange(minRules, maxRules);
        numRules = GetEngine().GetRandomRange()

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
            int randomNumRules = GetEngine().GetRandomRange(1, numRules);
            if (nodes.Count + randomNumRules > numStates)
            {
                randomNumRules = numStates - nodes.Count;
            }

            Node node = nodes[GetEngine().GetRandomRange(0, nodes.Count)];
            if (node.children.Count > 0)
            {
                node = node.children[GetEngine().GetRandomRange(0, node.children.Count)];
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
    public override void InitParameters(JSONObject jsonParameters)
    {
        if (jsonParameters)
        {
            jsonParameters.GetField(out minInitialObjects, "min_init_objs", minInitialObjects);
            jsonParameters.GetField(out maxInitialObjects, "max_init_objs", maxInitialObjects);
            jsonParameters.GetField(out minRules, "min_rules", minRules);
            jsonParameters.GetField(out maxRules, "max_rules", maxRules);
            jsonParameters.GetField(out minDeadends, "min_deadends", minDeadends);
            jsonParameters.GetField(out maxDeadends, "max_deadends", maxDeadends);
        }

        base.InitParameters(jsonParameters);
    }

    public ProductionRuleIdentifier GetRandomIdentifier()
    {
        ProductionRuleManager productionRuleManager = GetEngine().GetCachedEnvironmentComponent<ProductionRuleManager>();

        List<string> shapeKeys = new List<string>();

        foreach (ProductionRuleManager.ProductionRulePrefab prefab in productionRuleManager.productionRulePrefabs)
        {
            shapeKeys.Add(prefab.name);
        }

        List<string> colorKeys = new List<string>(ProductionRuleIdentifier.colorDict.Keys);

        string shape = shapeKeys[GetEngine().GetRandomRange(0, shapeKeys.Count)];
        string color = colorKeys[GetEngine().GetRandomRange(0, colorKeys.Count)];

        return new ProductionRuleIdentifier(shape, color);
    }

    public List<ProductionRuleIdentifier> GetRandomInitialState()
    {

        List<ProductionRuleIdentifier> initialState = new List<ProductionRuleIdentifier>();
        int numObjects = GetEngine().GetRandomRange(minInitialObjects, maxInitialObjects + 1);
        numObjects = Mathf.Max(numObjects, 1);
        for (int i = 0; i < numObjects; i++)
        {
            initialState.Add(GetRandomIdentifier());
        }

        return initialState;
    }

    public List<ProductionRule> BuildProductionRuleSet(List<ProductionRuleIdentifier> initialState)
    {
        int numRules = GetEngine().GetRandomRange(minRules, maxRules + 1);
        int numDeadEnds = GetEngine().GetRandomRange(minDeadends, maxDeadends + 1);

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
            int linearPathIndex = GetEngine().GetRandomRange(0, numRules);
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


    string SHAPE_TEXT = "s";
    string COLOR_TEXT = "c";
    string CONDITION_TEXT = "c";
    string SUBJECT_TEXT = "s";
    string OBJECT_TEXT = "o";
    string ACTION_TEXT = "a";
    string PREDICATE_OBJECTS_TEXT = "p";
    string FLOAT_TEXT = "f";
    string INITIAL_STATE_TEXT = "i";
    string RULE_SET_TEXT = "r";



    public void SavePayloads(string filename, List<ProductionRule> productionRules, List<ProductionRuleIdentifier> initialState)
    {
        // Save the initial state and production rules to a json file
        JSONObject initialStateJson = new JSONObject();
        JSONObject productionRulesJson = new JSONObject();

        // Serialize Initial State
        for (int i = 0; i < initialState.Count; i++)
        {
            JSONObject identifierJson = new JSONObject();
            identifierJson.AddField($"{SHAPE_TEXT}", initialState[i].ObjectShape);
            identifierJson.AddField($"{COLOR_TEXT}", initialState[i].ObjectColor);
            initialStateJson.Add(identifierJson);
        }

        // Serialize Production Rules
        for (int i = 0; i < productionRules.Count; i++)
        {
            JSONObject productionRuleJson = new JSONObject();
            ProductionRule productionRule = productionRules[i];

            // Serialize Conditions
            for (int j = 0; j < productionRule.conditions.Count; j++)
            {
                JSONObject conditionJson = new JSONObject();
                ProductionRuleCondition condition = productionRule.conditions[j];
                conditionJson.AddField($"{CONDITION_TEXT}", condition.condition.ToString());
                if (condition.subjectIdentifier != null)
                {
                    conditionJson.AddField($"{SUBJECT_TEXT}", SerializeIdentifier(condition.subjectIdentifier));
                }
                if (condition.objectIdentifier != null)
                {
                    conditionJson.AddField($"{OBJECT_TEXT}", SerializeIdentifier(condition.objectIdentifier));
                }
                productionRuleJson.AddField($"{CONDITION_TEXT}{j}", conditionJson);
            }

            // Serialize Actions
            for (int j = 0; j < productionRule.actions.Count; j++)
            {
                JSONObject actionJson = new JSONObject();
                ProductionRuleAction action = productionRule.actions[j];
                actionJson.AddField($"{ACTION_TEXT}", action.action.ToString());
                actionJson.AddField($"{PREDICATE_OBJECTS_TEXT}", action.predicateObjects.ToString());
                if (action.identifier != null)
                {
                    actionJson.AddField($"{OBJECT_TEXT}", SerializeIdentifier(action.identifier));
                }
                if (action.floatValue != 0)
                {
                    actionJson.AddField($"{FLOAT_TEXT}", action.floatValue);
                }
                productionRuleJson.AddField($"{ACTION_TEXT}{j}", actionJson);
            }

            productionRulesJson.Add(productionRuleJson);
        }

        // Combine both JSON objects into a single object
        JSONObject combinedJson = new JSONObject();
        combinedJson.AddField($"{INITIAL_STATE_TEXT}", initialStateJson);
        combinedJson.AddField($"{RULE_SET_TEXT}", productionRulesJson);

        // Save JSON to file
        System.IO.File.WriteAllText(filename, combinedJson.ToString());
    }

    private JSONObject SerializeIdentifier(ProductionRuleIdentifier identifier)
    {
        JSONObject identifierJson = new JSONObject();
        identifierJson.AddField($"{SHAPE_TEXT}", identifier.ObjectShape);
        identifierJson.AddField($"{COLOR_TEXT}", identifier.ObjectColor);
        return identifierJson;
    }


    public Tuple<List<ProductionRule>, List<ProductionRuleIdentifier>> LoadPayloads(string filename)
    {
        // Read the JSON file
        string jsonString = System.IO.File.ReadAllText(filename);
        JSONObject jsonObject = new JSONObject(jsonString);

        // Initialize the lists
        List<ProductionRuleIdentifier> initialState = new List<ProductionRuleIdentifier>();
        List<ProductionRule> productionRules = new List<ProductionRule>();

        // Deserialize Initial State
        JSONObject initialStateJson = jsonObject.GetField($"{INITIAL_STATE_TEXT}");
        foreach (JSONObject identifierJson in initialStateJson.list)
        {
            ProductionRuleIdentifier identifier = new ProductionRuleIdentifier(
                identifierJson.GetField($"{SHAPE_TEXT}").str,
                identifierJson.HasField($"{COLOR_TEXT}") ? identifierJson.GetField($"{COLOR_TEXT}").str : null
            );
            initialState.Add(identifier);
        }

        // Deserialize Production Rules
        JSONObject productionRulesJson = jsonObject.GetField($"{RULE_SET_TEXT}");
        foreach (JSONObject productionRuleJson in productionRulesJson.list)
        {
            List<ProductionRuleCondition> conditions = new List<ProductionRuleCondition>();
            List<ProductionRuleAction> actions = new List<ProductionRuleAction>();

            // Deserialize Conditions
            foreach (string ruleKey in productionRuleJson.keys)
            {
                if (ruleKey.StartsWith($"{CONDITION_TEXT}"))
                {
                    JSONObject conditionJson = productionRuleJson.GetField(ruleKey);
                    ProductionRuleManager.CONDITION condition = Enum.Parse<ProductionRuleManager.CONDITION>(conditionJson.GetField($"{CONDITION_TEXT}").str);
                    ProductionRuleIdentifier subjectIdentifier = DeserializeIdentifier(conditionJson.GetField($"{SUBJECT_TEXT}"));
                    ProductionRuleIdentifier objectIdentifier = conditionJson.HasField($"{OBJECT_TEXT}") ? DeserializeIdentifier(conditionJson.GetField($"{OBJECT_TEXT}")) : null;
                    ProductionRuleCondition conditionObj = new ProductionRuleCondition(condition, subjectIdentifier, objectIdentifier);
                    conditions.Add(conditionObj);
                }
            /*}

            // Deserialize Actions
            foreach (string actionKey in productionRuleJson.keys)
            {*/
                if (ruleKey.StartsWith($"{ACTION_TEXT}"))
                {
                    JSONObject actionJson = productionRuleJson.GetField(ruleKey);
                    ProductionRuleManager.ACTION action = (ProductionRuleManager.ACTION)Enum.Parse(typeof(ProductionRuleManager.ACTION), actionJson.GetField($"{ACTION_TEXT}").str);
                    PredicateObjects predicateObjects = (PredicateObjects)Enum.Parse(typeof(PredicateObjects), actionJson.GetField($"{PREDICATE_OBJECTS_TEXT}").str);
                    ProductionRuleIdentifier identifier = actionJson.HasField($"{OBJECT_TEXT}") ? DeserializeIdentifier(actionJson.GetField($"{OBJECT_TEXT}")) : null;
                    float floatValue = actionJson.HasField($"{FLOAT_TEXT}") ? actionJson.GetField($"{FLOAT_TEXT}").f : 0.0f;
                    ProductionRuleAction actionObj = ProductionRuleAction.GetProductionRuleAction(action, predicateObjects, floatValue, identifier);
                    actions.Add(actionObj);
                }
            }

            ProductionRule productionRule = new ProductionRule(conditions, actions);
            productionRules.Add(productionRule);
        }

        return new Tuple<List<ProductionRule>, List<ProductionRuleIdentifier>>(productionRules, initialState);
    }

    private ProductionRuleIdentifier DeserializeIdentifier(JSONObject identifierJson)
    {
        string shape = identifierJson.GetField($"{SHAPE_TEXT}").str;
        string color = identifierJson.HasField($"{COLOR_TEXT}") ? identifierJson.GetField($"{COLOR_TEXT}").str : null;
        return new ProductionRuleIdentifier(shape, color);
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
        ProductionRuleCondition productionRuleCondition = sampleForwardProductionRuleCondition(currentState, actionIsReward, actionNotReward);
        conditions.Add(productionRuleCondition);

        List<ProductionRuleAction> actions = new List<ProductionRuleAction>();
        ProductionRuleAction productionRuleAction = SampleForwardProductionRuleAction(currentState, actionIsReward, actionNotReward);
        actions.Add(productionRuleAction);

        ProductionRule productionRule = new ProductionRule(conditions, actions);

        return productionRule;
    }

    public ProductionRuleCondition sampleForwardProductionRuleCondition(List<ProductionRuleIdentifier> currentState, bool isReward = false, bool notReward = false)
    {
        ProductionRuleManager.CONDITION condition = ProductionRuleManager.CONDITION.NONE;
        int neededObjectCount = 0;

        do
        {
            if (isReward) // End of chain
            {
                condition = rewardConditions[GetEngine().GetRandomRange(0, rewardConditions.Count)];
            }
            else if (notReward) // Dead ends
            {
                condition = deadEndConditions[GetEngine().GetRandomRange(0, deadEndConditions.Count)];
            }
            else // Middle of chains
            {
                condition = chainConditions[GetEngine().GetRandomRange(0, chainConditions.Count)];
            }
            neededObjectCount = necessaryObjectCountForCondition[condition];
        } while (neededObjectCount > currentState.Count);

        ProductionRuleIdentifier subjectIdentifier = null;
        ProductionRuleIdentifier objectIdentifier = null;

        if (neededObjectCount == 2)
        {
            int subjectIndex = GetEngine().GetRandomRange(0, currentState.Count);
            int objectIndex = GetEngine().GetRandomRange(0, currentState.Count);
            while (subjectIndex == objectIndex)
            {
                objectIndex = GetEngine().GetRandomRange(0, currentState.Count);
            }

            subjectIdentifier = currentState[subjectIndex];
            objectIdentifier = currentState[objectIndex];
        }
        else if (neededObjectCount == 1)
        {
            subjectIdentifier = currentState[GetEngine().GetRandomRange(0, currentState.Count)];
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
        ProductionRuleManager.ACTION action = ProductionRuleManager.ACTION.NONE;
        if (isReward && notReward)
        {
            throw new ArgumentException("isReward and notReward cannot both be true");
        }

        do
        {
            if (isReward) // End of chain
            {
                action = rewardActions[GetEngine().GetRandomRange(0, rewardActions.Count)];
            }
            else if (notReward) // Dead ends
            {
                action = deadEndActions[GetEngine().GetRandomRange(0, deadEndActions.Count)];
            }
            else // Middle of chains
            {
                action = chainActions[GetEngine().GetRandomRange(0, chainActions.Count)];
            }
        }
        while (necessaryObjectCountForAction[action] > currentState.Count);

        float floatValue = 1.0f;// TODO: Figure out float values GetEngine().GetRandomRange(0.0f, 1.0f);

        List<PredicateObjects> permissiblePredicateObjects = PermissiblepredicateObjectsForAction[action];
        PredicateObjects predicateObjects = permissiblePredicateObjects[GetEngine().GetRandomRange(0, permissiblePredicateObjects.Count)];
        ProductionRuleIdentifier identifier = currentState[GetEngine().GetRandomRange(0, currentState.Count)];
        switch (action)
        {
            case ProductionRuleManager.ACTION.SWAP:
            case ProductionRuleManager.ACTION.SPAWN:
                identifier = GetRandomIdentifier();
                break;
        }

        ProductionRuleAction productionRuleAction = ProductionRuleAction.GetProductionRuleAction(action, predicateObjects, floatValue, identifier);

        return productionRuleAction;
    }

    public List<ProductionRuleIdentifier> GetNextState(List<ProductionRuleIdentifier> currentState, ProductionRule forwardProductionRule)
    {
        // TODO: move to ProductionRuleAction
        ProductionRuleManager.ACTION action = forwardProductionRule.actions[0].action;
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

        if (action == ProductionRuleManager.ACTION.SPAWN)
        {
            nextState.Add(forwardProductionRule.actions[0].identifier);
        }
        else if (action == ProductionRuleManager.ACTION.REMOVE)
        {
            nextState.Remove(actionTargetIdentifier);
            if (forwardProductionRule.actions[0].predicateObjects == PredicateObjects.BOTH)
            {
                nextState.Remove(forwardProductionRule.conditions[0].objectIdentifier);
            }
        }
        else if (action == ProductionRuleManager.ACTION.SWAP)
        {
            if (forwardProductionRule.actions[0].predicateObjects != PredicateObjects.NONE)
            {
                nextState.Remove(actionTargetIdentifier);
                if (forwardProductionRule.actions[0].predicateObjects == PredicateObjects.BOTH)
                {
                    nextState.Remove(forwardProductionRule.conditions[0].objectIdentifier);
                }
            }
            nextState.Add(forwardProductionRule.actions[0].identifier);
        }
        else if (action == ProductionRuleManager.ACTION.REWARD)
        {
            // do nothing
        }
        else if (action == ProductionRuleManager.ACTION.PRINT)
        {
            // do nothing
        }
        else
        {
            throw new ArgumentException("ACTION not recognized");
        }
        return nextState;
    }

    /*public List<ProductionRuleIdentifier> GetPreviousState(List<ProductionRuleIdentifier> currentState, ProductionRule backwardProductionRule)
    {
        ProductionRuleManager.ACTION action = backwardProductionRule.actions[0].action;
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
        if (action == ProductionRuleManager.ACTION.SPAWN)
        {
            previousState.Remove(actionTargetIdentifier);
        }
        else if (action == ProductionRuleManager.ACTION.REMOVE)
        {
            previousState.Add(actionTargetIdentifier);
        }
        else
        {
            throw new ArgumentException("ACTION not recognized");
        }
        return previousState;
    }*/

}
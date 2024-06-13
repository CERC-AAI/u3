using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

[Serializable]
public class ProductionRuleCondition
{

    public ProductionRuleManager.CONDITION condition;
    public ProductionRuleIdentifier subjectIdentifier;
    public ProductionRuleIdentifier objectIdentifier;

    public ProductionRuleCondition(ProductionRuleManager.CONDITION condition, ProductionRuleIdentifier subjectIdentifier, ProductionRuleIdentifier objectIdentifier = null)
    {
        this.condition = condition;
        this.subjectIdentifier = subjectIdentifier;
        this.objectIdentifier = objectIdentifier;
    }

    public bool IsSatisfied(ProductionRuleManager.CONDITION callbackCondition, ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        if (CheckObject(subject, obj) && CheckCondition(callbackCondition, subject, obj))
        {
            return true;
        }
        return false;
    }

    private bool CheckObject(ProductionRuleObject subject, ProductionRuleObject obj)
    {
        bool subjectIsPermissible = true;
        bool objectIsPermissible = true;

        switch (condition)
        {
            case ProductionRuleManager.CONDITION.NEAR:
            case ProductionRuleManager.CONDITION.CONTACT:
                subjectIsPermissible = subject != null && subjectIdentifier.CompareTo(subject.identifier);
                objectIsPermissible = obj != null && objectIdentifier.CompareTo(obj.identifier);
                break;

            case ProductionRuleManager.CONDITION.AGENT_NEAR:
            case ProductionRuleManager.CONDITION.HOLD:
            case ProductionRuleManager.CONDITION.DROP:
            case ProductionRuleManager.CONDITION.PICKUP:
            case ProductionRuleManager.CONDITION.SEE:
            case ProductionRuleManager.CONDITION.USE:
            case ProductionRuleManager.CONDITION.THROW:
                subjectIsPermissible = subject != null && subjectIdentifier.CompareTo(subject.identifier);
                break;

            default:
                return false;
        }

        return subjectIsPermissible && objectIsPermissible;


    }

    private bool CheckCondition(ProductionRuleManager.CONDITION callbackCondition, ProductionRuleObject subject, ProductionRuleObject obj)
    {
        // Access the state of the environment and check if the condition is satisfied
        // For example, if the condition is "near", check if the subject and object are near each other by checking their positions


        switch (condition)
        {
            case ProductionRuleManager.CONDITION.NEAR:
                return CheckNear(subject, obj);

            case ProductionRuleManager.CONDITION.AGENT_NEAR:
                return CheckNearAgent(subject);

            case ProductionRuleManager.CONDITION.HOLD:
                return CheckHold(subject);

            case ProductionRuleManager.CONDITION.SEE:
                return CheckSee(subject);

            case ProductionRuleManager.CONDITION.DROP:
            case ProductionRuleManager.CONDITION.PICKUP:
            case ProductionRuleManager.CONDITION.CONTACT:
            case ProductionRuleManager.CONDITION.THROW:
                return condition == callbackCondition;

            // case CONDITION.USE:
            //     return CheckUse(subject, obj);

            // case CONDITION.DROP:
            //     return CheckDrop(subject, obj);

            // case CONDITION.PICKUP:
            //     return CheckPickup(subject, obj);


            default:
                return false;
        }
    }

    private bool CheckNear(ProductionRuleObject subject, ProductionRuleObject obj)
    {
        if (subject == null || obj == null)
        {
            throw new Exception("CheckNear: Subject or object is null");
        }
        // throw an exception
        Vector3 subjectPosition = subject.transform.position;
        Vector3 objPosition = obj.transform.position;
        float distance = Vector3.Distance(subjectPosition, objPosition);
        return distance < subject.GetProductionRuleManager().GetNearDistance();

    }

    private bool CheckNearAgent(ProductionRuleObject subject)
    {
        if (subject == null)
        {
            throw new Exception("CheckNearAgent: Subject is null");
        }
        // throw an exception
        Vector3 subjectPosition = subject.transform.position;
        U3DPlayer player = subject.GetEngine().GetCachedEnvironmentComponent<U3DPlayer>();
        if (player == null)
        {
            throw new Exception("CheckNearAgent: Agent not found in env");
        }
        Vector3 objPosition = player.GetPosition();
        float distance = Vector3.Distance(subjectPosition, objPosition);
        return distance < 1;

    }

    private bool CheckHold(ProductionRuleObject subject)
    {
        if (subject == null)
        {
            throw new Exception("CheckHold: Subject is null");
        }

        GravityGun gravityGun = subject.GetProductionRuleManager().GetGravityGun();
        ProductionRuleObject heldObject = null;
        if (gravityGun.GetHeldObject() != null)
        {
            heldObject = gravityGun.GetHeldObject().GetComponentInParent<ProductionRuleObject>();
        }
        return heldObject != null && heldObject == subject;
    }

    public bool CheckSee(ProductionRuleObject subject)
    {
        if (subject == null)
        {
            throw new Exception("CheckSee: Subject is null");
        }
        U3DPlayer player = (U3DPlayer)subject.GetEngine().GetEnvironmentComponent<U3DPlayer>();
        Camera camera = player.Camera;
        Vector3 viewPos = camera.WorldToViewportPoint(subject.transform.position);
        if (viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1 && viewPos.z > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public string Encode()
    {
        string conditionString = "";
        if (conditionString.Length > 0)
        {
            conditionString += " and ";
        }
        if (condition == ProductionRuleManager.CONDITION.NEAR)
        {
            conditionString += $"{subjectIdentifier.Encode()} is near {objectIdentifier.Encode()}";
        }
        if (condition == ProductionRuleManager.CONDITION.AGENT_NEAR)
        {
            conditionString += $"{subjectIdentifier.Encode()} is near agent";
        }
        else if (condition == ProductionRuleManager.CONDITION.CONTACT)
        {
            conditionString += $"{subjectIdentifier.Encode()} contacts {objectIdentifier.Encode()}";
        }
        else if (condition == ProductionRuleManager.CONDITION.USE)
        {
            conditionString += $"{subjectIdentifier.Encode()} is used";
        }
        else if (condition == ProductionRuleManager.CONDITION.DROP)
        {
            conditionString += $"{subjectIdentifier.Encode()} is dropped";
        }
        else if (condition == ProductionRuleManager.CONDITION.PICKUP)
        {
            conditionString += $"{subjectIdentifier.Encode()} is picked up";
        }
        else if (condition == ProductionRuleManager.CONDITION.HOLD)
        {
            conditionString += $"{subjectIdentifier.Encode()} is held";
        }
        else if (condition == ProductionRuleManager.CONDITION.SEE)
        {
            conditionString += $"{subjectIdentifier.Encode()} is seen";
        }
        return conditionString;
    }

    virtual public void OnTrialOver()
    {
    }

    //     try
    //     {
    //         string conditionString = "";
    //         foreach (var condition in conditions)
    //         {
    //             if (conditionString.Length > 0)
    //             {
    //                 conditionString += " and ";
    //             }
    //             conditionString += $"{condition.Encode()}";
    //         }

    //         return conditionString;
    //     }
    //     catch (System.NullReferenceException)
    //     {
    //         if (conditions.Contains(CONDITION.NEAR))
    //         {
    //             return $"{subject.Encode()} is near {obj.Encode()}";
    //         }
    //         else if (conditions.Contains(CONDITION.CONTACT))
    //         {
    //             return $"{subject.Encode()} contacts {obj.Encode()}";
    //         }
    //         else if (conditions.Contains(CONDITION.USE))
    //         {
    //             return $"{subject.Encode()} is used";
    //         }
    //         else if (conditions.Contains(CONDITION.DROP))
    //         {
    //             return $"{subject.Encode()} is dropped";
    //         }
    //         else if (conditions.Contains(CONDITION.PICKUP))
    //         {
    //             return $"{subject.Encode()} is picked up";
    //         }
    //         else if (conditions.Contains(CONDITION.HOLD))
    //         {
    //             return $"{subject.Encode()} is held";
    //         }
    //         else if (conditions.Contains(CONDITION.USE))
    //         {
    //             return $"{subject.Encode()} is seen";
    //         }
    //     }

    //     return "INVALID CONDITION";
    // }
}

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
    public CONDITION condition;
    public ProductionRuleIdentifier subject;
    public ProductionRuleIdentifier obj;

    public ProductionRuleCondition(CONDITION condition, ProductionRuleIdentifier subject, ProductionRuleIdentifier obj = null)
    {
        this.condition = condition;
        this.subject = subject;
        this.obj = obj;
    }

    public bool IsSatisfied(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {

        if (!CheckCondition(condition, subject, obj))
        {
            return false;
        }
        return true;
    }

    private bool CheckCondition(CONDITION condition, ProductionRuleObject subject, ProductionRuleObject obj)
    {
        // Access the state of the environment and check if the condition is satisfied
        // For example, if the condition is "near", check if the subject and object are near each other by checking their positions
        switch (condition)
        {
            case CONDITION.NEAR:
                return CheckNear(subject, obj);

            // case CONDITION.USE:
            //     return CheckUse(subject, obj);

            // case CONDITION.DROP:
            //     return CheckDrop(subject, obj);

            // case CONDITION.PICKUP:
            //     return CheckPickup(subject, obj);

            // case CONDITION.HOLD:
            //     return CheckHold(subject, obj);

            // case CONDITION.SEE:
            //     return CheckSee(subject, obj);

            default:
                return false;
        }
    }

    private bool CheckNear(ProductionRuleObject subject, ProductionRuleObject obj)
    {
        // Logic to check if the subject and object are near each other
        // Example: return Vector3.Distance(subject.transform.position, obj.transform.position) < 1.0f;

        Vector3 subjectPosition = subject.transform.position;
        Vector3 objPosition = obj.transform.position;
        float distance = Vector3.Distance(subjectPosition, objPosition);
        return distance < subject.GetProductionRuleManager().GetNearDistance();

    }


    public string Encode()
    {
        string conditionString = "";
        if (conditionString.Length > 0)
        {
            conditionString += " and ";
        }
        if (condition == CONDITION.NEAR)
        {
            conditionString += $"{subject.Encode()} is near {obj.Encode()}";
        }
        else if (condition == CONDITION.CONTACT)
        {
            conditionString += $"{subject.Encode()} contacts {obj.Encode()}";
        }
        else if (condition == CONDITION.USE)
        {
            conditionString += $"{subject.Encode()} is used";
        }
        else if (condition == CONDITION.DROP)
        {
            conditionString += $"{subject.Encode()} is dropped";
        }
        else if (condition == CONDITION.PICKUP)
        {
            conditionString += $"{subject.Encode()} is picked up";
        }
        else if (condition == CONDITION.HOLD)
        {
            conditionString += $"{subject.Encode()} is held";
        }
        else if (condition == CONDITION.USE)
        {
            conditionString += $"{subject.Encode()} is seen";
        }
        return conditionString;
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

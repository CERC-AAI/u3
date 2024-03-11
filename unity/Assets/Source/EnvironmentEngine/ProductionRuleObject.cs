using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

public class ProductionRuleObject : EnvironmentComponent
{
    public ProductionRuleIdentifier identifier;

    public GameObject DebugCube;

    GameObject mCurrentDisplayObject;

    ProductionRuleManager mManager;

    // public ProductionRuleObject(ProductionRuleIdentifier identifier)
    // {
    //     if (identifier == null)
    //     {
    //         throw new ArgumentException("ProductionRuleObject identifier cannot be null");
    //     }
    //     if (this == null)
    //     {
    //         throw new ArgumentException("ProductionRuleObject cannot be null");
    //     }
    //     this.identifier = identifier;
    // }

    protected override void Initialize()
    {
        base.Initialize();

        DebugCube.SetActive(false);
        ProductionRuleObjectInitialize(identifier.ObjectShape, identifier.ObjectColor);

        GetProductionRuleManager().AddProdRuleObject(this);

    }

    public override void OnRemoved()
    {
        GetProductionRuleManager().RemoveProdRuleObject(this);
        base.OnRemoved();

    }

    public override void OnCollision(EnvironmentObject otherObject)
    {
        
        ProductionRuleObject productionRuleObject = otherObject.GetComponentInParent<ProductionRuleObject>();
        GetProductionRuleManager().CheckCallback(CONDITION.CONTACT, this, productionRuleObject, GetEngine());
        base.OnCollision(otherObject);
        Debug.Log("Object Collided");
    }

    // public override void OnCreated()
    // {
    //     GetProductionRuleManager().AddProdRuleObject(this);
    //     base.OnCreated();
    // }

    // Initialize the object with a shape and color, defaulting to a red sphere
    public void ProductionRuleObjectInitialize(string shape, string color)
    {
        identifier = new ProductionRuleIdentifier(shape.ToLower(), color.ToLower());
        if (shape.Trim() == "" || color.Trim() == "")
        {
            return;
        }
        if (mCurrentDisplayObject != null)
        {
            Destroy(mCurrentDisplayObject);
        }
        mCurrentDisplayObject = Instantiate(GetProductionRuleManager().getPrefabFromName(shape), this.transform);
        mCurrentDisplayObject.transform.parent = this.transform;
        mCurrentDisplayObject.transform.localPosition = Vector3.zero;
        // Ref the prefab, grab the renderer, set the color
        Debug.Log("Setting color to: " + ProductionRuleIdentifier.colorDict[color]);
        mCurrentDisplayObject.GetComponent<Renderer>().material.color = ProductionRuleIdentifier.colorDict[color];
    }


    public ProductionRuleIdentifier GetIdentifier()
    {
        return this.identifier;
    }

    public string getName()
    {
        return this.identifier.Encode();
    }

    public ProductionRuleManager GetProductionRuleManager()
    {
        if (mManager == null)
        {
            mManager = (ProductionRuleManager)GetEngine().GetEnvironmentComponent<ProductionRuleManager>();
        }
        return mManager;
        //return ProductionRuleManager.productionRuleManager;
    }

    // public override void OnCollision(CONDITION condition, EnvironmentComponent other)
    // {
    //     // Check if the other object is a ProductionRuleObject
    //     ProductionRuleObject otherProdRuleObj = other.GetComponent<ProductionRuleObject>();
    //     if (otherProdRuleObj != null)
    //     {
    //         // Check if the other object matches the identifier
    //         if (MatchesIdentifier(otherProdRuleObj))
    //         {
    //             // Check and execute any relevant production rules
    //             CheckAndExecuteProductionRules(otherProdRuleObj);
    //         }
    //     }
    // }

    // private bool MatchesIdentifier(ProductionRuleObject otherObj)
    // {
    //     // Logic to determine if the other object matches your identifier criteria
    //     // Example: return this.someIdentifier.MatchesObject(otherObj);
    // }

    // private void CheckAndExecuteProductionRules(ProductionRuleObject otherObj)
    // {
    //     // Logic to check for relevant production rules upon collision
    //     // This might involve interacting with the ProductionRuleComponent
    //     // Example: ProductionRuleComponent.Instance.EvaluateRules(this, otherObj);
    // }


}

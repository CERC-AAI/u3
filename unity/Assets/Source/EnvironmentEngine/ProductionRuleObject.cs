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

    ProductionRuleManager mManager;

    protected override void Initialize()
    {
        base.Initialize();

        DebugCube.SetActive(false);
        Initalize(identifier.ObjectShape, identifier.ObjectColor);

        GetProductionRuleManager().AddProdRuleObject(this);

    }

    // Initialize the object with a shape and color, defaulting to a red sphere
    public void Initalize(string shape = "sphere", string color = "red")
    {
        //Initialize production rule manager if needed
        mManager = GetProductionRuleManager();

        identifier = new ProductionRuleIdentifier(shape, color);

        // this.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        // this.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        // Need to set the shape and the color of the object
        // The shape is set by loading the correct prefab
        GameObject shapeObject = Instantiate(mManager.getPrefabFromName(shape), this.transform);
        shapeObject.transform.parent = this.transform;
        shapeObject.transform.localPosition = Vector3.zero;
        // Ref the prefab, grab the renderer, set the color
        shapeObject.GetComponent<Renderer>().material.color = ProductionRuleIdentifier.colorDict[color];
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

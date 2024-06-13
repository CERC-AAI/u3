using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.ProbeTouchupVolume;

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

    public void ProductionRuleObjectInitialize(string shape, string color, float alpha = 1.0f)
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
        //Debug.Log("Setting color to: " + ProductionRuleIdentifier.colorDict[color]);
        Color colorValue = ProductionRuleIdentifier.colorDict[color];
        colorValue.a = alpha;
        mCurrentDisplayObject.GetComponent<Renderer>().material.color = colorValue;
    }


    public override void OnRemoved()
    {
        GetProductionRuleManager().RemoveProdRuleObject(this);
        base.OnRemoved();

    }

    public override void OnCollision(EnvironmentObject otherObject)
    {

        ProductionRuleObject productionRuleObject = otherObject.GetComponentInParent<ProductionRuleObject>();
        GetProductionRuleManager().CheckCallback(ProductionRuleManager.CONDITION.CONTACT, this, productionRuleObject, GetEngine());
        base.OnCollision(otherObject);
        //Debug.Log("Object Collided");
    }

    // public override void OnCreated()
    // {
    //     GetProductionRuleManager().AddProdRuleObject(this);
    //     base.OnCreated();
    // }

    // Initialize the object with a shape and color, defaulting to a red sphere

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


    public class ProductionRuleObjectState : TrialManager.ObjectState
    {
        public string shape;
        public string color;
    }

    override public TrialManager.ObjectState SaveTrialData()
    {
        ProductionRuleObjectState objectState = new ProductionRuleObjectState();

        objectState.shape = GetIdentifier().ObjectShape;
        objectState.color = GetIdentifier().ObjectColor;
        objectState.position = transform.position;
        objectState.rotation = transform.rotation;
        objectState.scale = transform.localScale;

        objectState.saveObject = GetProductionRuleManager().productionRuleObjectPrefab.GetComponent<ProductionRuleObject>();

        return objectState;
    }

    override public void LoadTrialData(TrialManager.ObjectState objectState)
    {
        if (objectState is ProductionRuleObjectState)
        {
            ProductionRuleObjectState productionObjectState = (ProductionRuleObjectState)objectState;

            ProductionRuleObjectInitialize(
                productionObjectState.shape,
                productionObjectState.color);
            transform.position = productionObjectState.position;
            transform.rotation = productionObjectState.rotation;
            transform.localScale = productionObjectState.scale;
        }
    }

}

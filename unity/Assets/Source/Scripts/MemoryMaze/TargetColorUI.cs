using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class TargetColorUI : EnvironmentComponent
{
    public Texture2D texture;  // The texture to use for the cursor
    public float borderWidth = 10.0f;
    public Color color = Color.white;

    ProductionRule mCurrentRule = null;

    void OnGUI()
    {
        if (mCurrentRule == null || !mCurrentRule.isActive())
        {
            ProductionRuleManager ruleManager = GetEngine().GetCachedEnvironmentComponent<ProductionRuleManager>();
            List<ProductionRule> rules = ruleManager.GetAllProdRules();

            foreach (ProductionRule rule in rules)
            {
                if (rule.isActive())
                {
                    color = ProductionRuleIdentifier.colorDict[rule.conditions[0].subjectIdentifier.ObjectColor];
                    mCurrentRule = rule;
                    break;
                }
            }
        }

        // Draw the custom cursor texture at the current mouse position
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture, ScaleMode.StretchToFill, true, 0, color, borderWidth, 0);
    }

    public override void OnRunStarted()
    {
        base.OnRunStarted();

        mCurrentRule = null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : EnvironmentComponent
{
    public TextMesh label;
    public int amount;

    bool mPressed = false;
    int mOrder;
    ButtonGame mButtonList;

    protected override void Init()
    {
        mPressed = false;

        base.Init();
    }

    public bool isPressed()
    {
        return mPressed;
    }

    public override void OnEndTurnCollision(EnvironmentObject otherObject)
    {
        if (otherObject.tag == "Player" && mButtonList)
        {
            if (mButtonList.CanPressButton(mOrder))
            {
                HealthBar healthBar = otherObject.GetComponentInChildren<HealthBar>();

                if (healthBar)
                {
                    mEngine.AddReward(amount);
                    healthBar.addHP(amount);

                    mEngine.AddGameEvent("Button", mOrder.ToString());
                }

                mPressed = true;

                mEngine.AddGameEvent("ButtonPress", "ButtonPress");

                if (mButtonList.IsLastButton(mOrder))
                {
                    mEngine.Victory();
                }

                mParentObject.Remove();
            }
        }

        base.OnEndTurnCollision(otherObject);
    }

    public void SetOrder(ButtonGame buttonList, int order)
    {
        mButtonList = buttonList;
        mOrder = order;
        label.text = order.ToString();
    }

    public int GetOrder()
    {
        return mOrder;
    }

    protected override void BuildRunStateJSON(JSONObject root)
    {
        Vector2Int position;
        mButtonList.GetGridPosition(transform.position, out position);

        root["x"] = new JSONObject(position.x);
        root["y"] = new JSONObject(position.y);
        root["pressed"] = new JSONObject(mPressed);

        base.BuildRunStateJSON(root);
    }

    protected override void LoadRunStateJSON(JSONObject root)
    {
        if (root.keys.Contains("x") && root.keys.Contains("y"))
        {
            Move(mButtonList.GetPositionFromGrid(new Vector2Int((int)root["x"].i, (int)root["y"].i)));
        }
        if (root.keys.Contains("pressed"))
        {
            mPressed = root["pressed"].b;
        }

        base.LoadRunStateJSON(root);
    }

    public override void OnPostLoadState()
    {
        if (isPressed() && gameObject.activeSelf)
        {
            mParentObject.Remove();
        }
        else if (!isPressed() && !gameObject.activeSelf)
        {
            mParentObject.RefreshObject();
        }

        base.OnPostLoadState();
    }
}

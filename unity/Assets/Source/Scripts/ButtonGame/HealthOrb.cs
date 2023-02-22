using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthOrb : EnvironmentComponent
{
    public int amount = 5;

    public override void OnEndTurnCollision(EnvironmentObject otherObject)
    {
        Debug.Log("Hit health orb");

        if (otherObject.tag == "Player")
        {
            HealthBar healthBar = otherObject.GetComponentInChildren<HealthBar>();

            if (healthBar)
            {
                mEngine.AddReward(amount);
                healthBar.addHP(amount);

                mEngine.AddGameEvent("Orb", amount.ToString());
            }

            mParentObject.Remove();
        }

        base.OnEndTurnCollision(otherObject);
    }

}

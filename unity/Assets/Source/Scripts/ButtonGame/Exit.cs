using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : EnvironmentComponent
{

    public override void OnEndStepCollision(EnvironmentObject otherObject)
    {
        if (otherObject.tag == "Player")
        {
            //mEngine.Victory();
        }

        base.OnEndStepCollision(otherObject);
    }

}

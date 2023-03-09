using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : LinkComponent
{
    bool mIsTaken = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool isPickedUp()
    {
        return mIsTaken;
    }

    public override void OnEndStepCollision(EnvironmentObject otherObject)
    {
        /*if (otherObject.tag == "Player")
        {
            if (!mIsTaken)
            {
                mEngine.AddReward(5);
                mIsTaken = true;

                mEngine.AddGameEvent("Key", mLoadID.ToString());

                gameObject.SetActive(false);
            }
        }*/

        base.OnEndStepCollision(otherObject);
    }
}

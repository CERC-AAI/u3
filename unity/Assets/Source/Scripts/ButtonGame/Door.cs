using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : LinkComponent
{
    public Key thisKey;

    bool mLocked = true;

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        if (mLocked && thisKey && thisKey.isPickedUp())
        {
            Debug.Log("Open door");

            mLocked = false;
            mParentObject.MakeCollidersTriggers();
        }

        base.OnFixedUpdate(fixedDeltaTime);
    }

    public bool isLocked()
    {
        return mLocked;
    }

    public void FindKey()
    {
        GetLinkedObjects();

        if (mLinkedObjects.Count > 0 && mLinkedObjects[0] is Key)
        {
            thisKey = (Key)mLinkedObjects[0];
        }
    }

    public override void OnEndStepCollision(EnvironmentObject otherObject)
    {
        if (true || !mLocked)
        {
            if (otherObject.tag == "Player")
            {
                gameObject.SetActive(false);

                mEngine.AddGameEvent("Door", new JSONObject(mLoadID.ToString()));
            }
        }

        base.OnEndStepCollision(otherObject);
    }
}

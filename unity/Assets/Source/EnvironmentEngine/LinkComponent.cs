using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkComponent : EnvironmentComponent
{
    Color[] mColors = { Color.red, Color.blue, Color.green};
    //Color[] mColors = { Color.red, Color.blue, Color.green, Color.cyan, Color.yellow, Color.magenta };

    protected List<LinkComponent> mLinkedObjects = new List<LinkComponent>();
    protected int mLoadID;

    public void GetLinkedObjects()
    {
        CheckAll();

        LinkComponent[] thisLinks = mEngine.GetComponentsInChildren<LinkComponent>();

        mLinkedObjects.Clear();
        for (int i = 0; i < thisLinks.Length; i++)
        {
            if (thisLinks[i] && thisLinks[i] != this && thisLinks[i].mLoadID == mLoadID)
            {
                mLinkedObjects.Add(thisLinks[i]);
            }
        }
    }

    public void SetLoadID(int loadID)
    {
        mLoadID = loadID;

        GraphicsObject thisGraphics = GetComponent<GraphicsObject>();

        if (thisGraphics)
        {
            thisGraphics.SetTint(mColors[loadID % mColors.Length]);
        }
    }

    public int GetLoadID()
    {
        return mLoadID;
    }
}

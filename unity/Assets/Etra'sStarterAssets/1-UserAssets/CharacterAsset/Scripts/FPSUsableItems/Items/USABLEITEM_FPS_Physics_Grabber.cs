using StarterAssets;
using System.Collections;
using UnityEngine;

public class USABLEITEM_FPS_Physics_Grabber : EtraFPSUsableItemBaseClass
{
    //From @aMySour
    /*
    The MIT License (MIT)
    Copyright 2023 amysour
    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    */

    //Name of Prefab to load and required function
    private string nameOfPrefabToLoadFromAssets = "FPSPhysicsGrabberGroup";
    public override string getNameOfPrefabToLoad() { return nameOfPrefabToLoadFromAssets; }

    //References
    StarterAssetsInputs starterAssetsInputs;
    Animator physgrabberAnimator;


    public float pickUpRange = 3f;
    public float maxMass = 5f; // anything greater than this will not be picked up
    private Rigidbody pickedUpObject;
    private SpringJoint springJoint;
    public float springJointForce = 200f;
    public float springJointDamper = 10f;
    private Rigidbody otherRB; // we make a Rigidbody that will be kinematic, this is at the target position and is needed since spring joints need two rigidbodies to work
    public GameObject temporaryParent;

    private float originalDrag;
    private float originalAngularDrag;
    public float pickedUpDrag = 10f;

    public float pickedUpDistance = 2f;
    public float dropDistance = 5f; // if it goes too far away from the player, we will drop it

    private void Awake()
    {
        this.enabled = false;
    }
    public void OnEnable()
    {
        //Set references WHEN THIS SCRIPT IS ENABLED
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();

        GameObject otherRBGO = new GameObject("otherRBPHYSGRABBER");
        otherRB = otherRBGO.AddComponent<Rigidbody>();
        otherRB.isKinematic = true;
        otherRB.useGravity = false;
        otherRB.mass = 2000f; // this helps certain components understand that this is a static object

        otherRB.transform.parent = Camera.main.transform;
        otherRB.transform.localPosition = new Vector3(0, 0, pickedUpDistance);
    }

    public void Update()
    {
        if (starterAssetsInputs.aim)
        {
            starterAssetsInputs.aim = false;
            if (pickedUpObject == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickUpRange))
                {
                    if (hit.rigidbody != null)
                    {
                        if (hit.rigidbody.mass <= maxMass)
                        {
                            pickedUpObject = hit.rigidbody;

                            // remove any existing spring joints
                            if (pickedUpObject.GetComponent<SpringJoint>() != null)
                            {
                                Destroy(pickedUpObject.GetComponent<SpringJoint>());
                            }

                            springJoint = pickedUpObject.gameObject.AddComponent<SpringJoint>();
                            springJoint.connectedBody = otherRB;
                            springJoint.spring = springJointForce;
                            springJoint.damper = springJointDamper;
                            springJoint.autoConfigureConnectedAnchor = false;
                            springJoint.connectedAnchor = Vector3.zero;
                            springJoint.anchor = Vector3.zero;

                            originalAngularDrag = pickedUpObject.angularDrag;
                            originalDrag = pickedUpObject.drag;

                            pickedUpObject.angularDrag = pickedUpDrag;
                            pickedUpObject.drag = pickedUpDrag;
                        }
                    }
                }
            }
            else
            {
                Destroy(springJoint);
                pickedUpObject.angularDrag = originalAngularDrag;
                pickedUpObject.drag = originalDrag;
                pickedUpObject = null;
            }
        }

        if (starterAssetsInputs.shoot)
        {
            if (pickedUpObject != null)
            {
                Destroy(springJoint);
                pickedUpObject.angularDrag = originalAngularDrag;
                pickedUpObject.drag = originalDrag;

                pickedUpObject.AddForce(Camera.main.transform.forward * 1000f);

                pickedUpObject = null;

                starterAssetsInputs.shoot = false;
            }
            else
            {
                // raycast for rb and hit that with a force (regardless of mass)
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickUpRange))
                {
                    if (hit.rigidbody != null)
                    {
                        // closer objects have more force, lets divide by distance
                        hit.rigidbody.AddForce(Camera.main.transform.forward * 1000f / (hit.distance * 0.5f));
                    }
                }

                starterAssetsInputs.shoot = false;
            }
        }
    }

}

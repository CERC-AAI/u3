using StarterAssets;
using UnityEngine;

public class ABILITY_GrabInteract : EtraAbilityBaseClass
{
    //Abilty created by: asour

    // With this ability, we will pick up Rigidbodies within a certain mass and range.
    // We will raycast using the ABILITY_CameraMovement script
    // Then we will check the distance between player camera root and the pickUpRange, if we hit a rigidbody in that range, we will pick it up.
    // If we are picked up, the raycast will determine where the object's target position is, and we will use a spring joint to move the object to that position.

    [Header("Basics")]
    public float pickUpRange = 3f;
    public float maxMass = 5f; // anything greater than this will not be picked up
    public float pickedUpObjectDistanceFromPlayer = 2f;
    public float dropDistance = 5f; // if it goes too far away from the player, we will drop it

    //Spring Joint Settings
    private SpringJoint springJoint;
    private float springJointForce = 200f;
    private float springJointDamper = 10f;

    //Attachment and Drag settings
    private Rigidbody otherRB; // we make a Rigidbody that will be kinematic, this is at the target position and is needed since spring joints need two rigidbodies to work
    private Rigidbody pickedUpObject;
    private GameObject temporaryParent;
    private float originalDrag;
    private float originalAngularDrag;
    private float pickedUpDrag = 10f;


    //References
    StarterAssetsInputs starterAssetsInputs;
    ABILITY_CameraMovement camMoveScript;

    public override void abilityStart()
    {
        //Set references
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        camMoveScript = GameObject.Find("EtraAbilityManager").GetComponent<ABILITY_CameraMovement>();

        GameObject otherRBGO = new GameObject("otherRB");
        otherRB = otherRBGO.AddComponent<Rigidbody>();
        otherRB.isKinematic = true;
        otherRB.useGravity = false;
        otherRB.mass = 2000f; // this helps certain components understand that this is a static object

        otherRB.transform.parent = GameObject.Find("EtraPlayerCameraRoot").transform;
        otherRB.transform.localPosition = new Vector3(0, 0, pickedUpObjectDistanceFromPlayer);
    }

    bool interactHeld;
    bool secondInteractCheck = false;
    public override void abilityUpdate()
    {
        if (!enabled)
        {
            return;
        }

        if(interactHeld && !starterAssetsInputs.interact)
        {
            interactHeld = false;
        }

        if (!starterAssetsInputs.interact)
        {
            secondInteractCheck = false;
        }

        if (pickedUpObject == null)
        {
            // if we press E while looking at a rigidbody we raycast to, we will pick it up

            // check e is held
            if (starterAssetsInputs.interact && !interactHeld && !secondInteractCheck)
            {
                interactHeld = true;

                // check if object is being looked at, and is in range to be picked up
                if (pickUpRange > Vector3.Distance(camMoveScript.playerCameraRoot.transform.position, camMoveScript.pointCharacterIsLookingAt))
                {
                    GameObject objectThatIsLookedAt = camMoveScript.raycastHit.transform.gameObject;

                    // if rigid body, and not too big, pick up object
                    if (objectThatIsLookedAt.GetComponent<Rigidbody>() != null)
                    {
                        if (objectThatIsLookedAt.GetComponent<Rigidbody>().mass <= maxMass)
                        {
                            pickedUpObject = objectThatIsLookedAt.GetComponent<Rigidbody>();

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
        }
        else
        {
            // if we press E while holding an object, we will drop it
            if (starterAssetsInputs.interact && !interactHeld || Vector3.Distance(pickedUpObject.transform.position, camMoveScript.playerCameraRoot.transform.position) > dropDistance)
            {
                Destroy(springJoint);
                pickedUpObject.angularDrag = originalAngularDrag;
                pickedUpObject.drag = originalDrag;
                pickedUpObject = null;
                secondInteractCheck = true;
            }

        }

    }



}

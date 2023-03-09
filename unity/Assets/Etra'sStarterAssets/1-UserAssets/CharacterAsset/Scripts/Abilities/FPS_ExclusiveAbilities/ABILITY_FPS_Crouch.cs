using StarterAssets;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ABILITY_CharacterMovement))]

public class ABILITY_FPS_Crouch : EtraAbilityBaseClass
{
    //This script is functional and as is for now. Will rework in the future.
    //
    [HideInInspector]
    public enum crouchType
    {
        Toggle,
        Hold
    }
    [Header("Basics")]
    public float crouchedMovementSpeed = 1;
    public crouchType CrouchType;
    [Range(0.1f, 2)]
    [SerializeField] private float timeToCrouch = 0.25f;

    //Crouch and standing hitbox variables
     private float crouchHeight = 0.9f;
     private float standingHeight = 1.8f;
     private Vector3 crouchingCenter = new Vector3(0, 1.1f,0);
     private Vector3 standingCenter = new Vector3(0, 0.93f, 0);
     private float crouchGroundedOffset = -0.9f;
     private float standGroundedOffset;

    //toggleCrouch
    private bool toggleIsCrouching;
    private bool duringToggleCrouchTransition = false;

    //hold crouch
    private bool holdLowering = false;
    private bool holdRising = false;
    private float timeElapsedHold = 0;
    private float overallCrouchTimer;
    private float timeToCrouchHold;

    //References
    private StarterAssetsInputs starterAssetsInputs;
    private CharacterController characterController;
    private ABILITY_CharacterMovement movementAbility;
    private GameObject playerFollowCam;

    public override void abilityStart()
    {
        OnValidate();
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        mainController = GetComponentInParent<EtraCharacterMainController>();
        characterController = GetComponentInParent<CharacterController>();
        movementAbility = GetComponent<ABILITY_CharacterMovement>();
        standGroundedOffset = mainController.GroundedOffset;
        playerFollowCam = GameObject.Find("Etra'sStarterAssetsFollowCamera");
    }


    private bool crouchHeld = false;
    private bool crouchReleased = false;


    private void Reset()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        movementAbility = GetComponent<ABILITY_CharacterMovement>();
        movementAbility.crouchSpeed = crouchedMovementSpeed;
    }


    public override void abilityUpdate()
    {

        if (!abilityEnabled)
        {
            return;
        }

        //When Crouch Held
        if (!crouchHeld && starterAssetsInputs.crouch && mainController.Grounded)
        {
            crouchHeld = true;
            crouchReleased = false;
            switch (CrouchType)
            {
                case ABILITY_FPS_Crouch.crouchType.Toggle:
                    if (!duringToggleCrouchTransition)
                    {
                        StartCoroutine(toggleCrouch());
                    }
                    break;
                case ABILITY_FPS_Crouch.crouchType.Hold:
                        holdLowering = true;
                        holdRising = false;
                        toggleIsCrouching = false;
                        StopCoroutine(crouchRise());
                        StartCoroutine(crouchLower());
                    break;
            } 
        }
        if (!starterAssetsInputs.crouch && !crouchReleased)
        {
            crouchHeld = false;
            crouchReleased = true;

            switch (CrouchType)
            {
                case ABILITY_FPS_Crouch.crouchType.Toggle:

                    break;
                case ABILITY_FPS_Crouch.crouchType.Hold:

                        holdRising = true;
                        holdLowering = false;
                        toggleIsCrouching = true;
                        StopCoroutine(crouchLower());
                        StartCoroutine(crouchRise());
                    break;
            }

        }
    }
    private IEnumerator toggleCrouch()
    {
        movementAbility.isCrouched = true;
        duringToggleCrouchTransition = true;
        float timeElapsed = 0;
        float targetHeight = toggleIsCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = toggleIsCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;
        float targetGroundedOffset = toggleIsCrouching ? standGroundedOffset : crouchGroundedOffset ;
        float currentGroundedOffset = mainController.GroundedOffset;

        while (timeElapsed < timeToCrouch)
        {
            if (!toggleIsCrouching)
            {
                characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
                characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
                mainController.GroundedOffset = Mathf.Lerp(currentGroundedOffset, targetGroundedOffset, timeElapsed / timeToCrouch);

                timeElapsed += Time.deltaTime;
            }
            else if (toggleIsCrouching  && !Physics.Raycast(playerFollowCam.transform.position, Vector3.up, 1f)){
                characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
                characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
                mainController.GroundedOffset = Mathf.Lerp(currentGroundedOffset, targetGroundedOffset, timeElapsed / timeToCrouch);

                timeElapsed += Time.deltaTime;
            }

            yield return null;

        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;
        mainController.GroundedOffset = targetGroundedOffset;

        toggleIsCrouching = !toggleIsCrouching;

        if (!toggleIsCrouching)
        {
            movementAbility.isCrouched = false;
        }

        duringToggleCrouchTransition = false;

    }


    private IEnumerator crouchLower()
    {
        movementAbility.isCrouched = true;
        timeToCrouchHold = timeToCrouch - overallCrouchTimer;
        timeElapsedHold = 0;
        float targetHeight = crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = crouchingCenter;
        Vector3 currentCenter = characterController.center;
        float targetGroundedOffset = crouchGroundedOffset;
        float currentGroundedOffset = mainController.GroundedOffset;

        while (timeElapsedHold < timeToCrouchHold)
        {
            if (holdLowering) { 
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsedHold / timeToCrouchHold);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsedHold / timeToCrouchHold);
            mainController.GroundedOffset = Mathf.Lerp(currentGroundedOffset, targetGroundedOffset, timeElapsedHold / timeToCrouchHold);

            timeElapsedHold += Time.deltaTime;
            
             overallCrouchTimer += Time.deltaTime;
            }
            yield return null;
        }

        if (holdLowering)
        {
        characterController.height = targetHeight;
        characterController.center = targetCenter;
        mainController.GroundedOffset = targetGroundedOffset;
        }


    }


    private IEnumerator crouchRise()
    {
        timeToCrouchHold = overallCrouchTimer;
        timeElapsedHold = 0;

        float targetHeight = standingHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = standingCenter;
        Vector3 currentCenter = characterController.center;
        float targetGroundedOffset = standGroundedOffset;
        float currentGroundedOffset = mainController.GroundedOffset;

        while (timeElapsedHold < timeToCrouchHold)
        {

            if (holdRising && !Physics.Raycast(playerFollowCam.transform.position, Vector3.up, 1f))
            {
                characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsedHold / timeToCrouchHold);
                characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsedHold / timeToCrouchHold);
                mainController.GroundedOffset = Mathf.Lerp(currentGroundedOffset, targetGroundedOffset, timeElapsedHold / timeToCrouchHold);

                timeElapsedHold += Time.deltaTime;
                overallCrouchTimer -= Time.deltaTime;
            }
            yield return null;
        }

        if (holdRising)
        {
        characterController.height = targetHeight;
        characterController.center = targetCenter;
        mainController.GroundedOffset = targetGroundedOffset;
        }

        movementAbility.isCrouched = false;

    }
}

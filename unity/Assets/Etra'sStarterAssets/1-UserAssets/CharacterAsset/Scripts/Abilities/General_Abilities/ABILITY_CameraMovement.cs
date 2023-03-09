using StarterAssets;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ABILITY_CameraMovement : EtraAbilityBaseClass
{

    [Header("Basics")]
    //Camera sensitivity
    public float cameraSensitivity = 1;
    [HideInInspector] public float usedCameraSensitivity;
    [HideInInspector] public float aimSensitivity = 1;
    [SerializeField] public bool setForwardToPlayerLookDirection = true;
    private float rotateTowardsCharacterLookPointSpeed = 20f;

    //Layers to collide with
    [SerializeField] private LayerMask aimColliderMask = new LayerMask();
    private float distanceToTargetIfNoObjectIsHitByRay = 10;
    //Raycast feedback
    [HideInInspector] public bool objectHit;
    [HideInInspector] public Vector3 pointCharacterIsLookingAt;
    [HideInInspector] public RaycastHit raycastHit;

    //Debug
    [Header("Debug")]
    public bool camLookDebug;
    [SerializeField] private Transform debugTransform;

    //Cinemachine Cam Angle Limiting
    [Header("Camera Angle Limits")]
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    //Misc
    private const float _threshold = 0.01f;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;


    //X & Y Locks
    [Header("Locks")]
    public bool lockLookX = false;
    public bool lockLookY = false;
    private float camModX = 1;
    private float camModY = 1;

    //References 
    [HideInInspector]
    public GameObject playerCameraRoot;
    private StarterAssetsInputs _input;


#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";
#else
    private bool IsCurrentDeviceMouse = true;
#endif



    //Set defaults of gameplay variables 
    private void Reset()
    {
        if (GetComponentInParent<EtraCharacterMainController>().appliedGameplayType == EtraCharacterMainController.GameplayType.FirstPerson)
        {
            setForwardToPlayerLookDirection = true;
        }
        if (GetComponentInParent<EtraCharacterMainController>().appliedGameplayType == EtraCharacterMainController.GameplayType.ThirdPerson)
        {
            setForwardToPlayerLookDirection = false;
        }

        aimColliderMask = LayerMask.GetMask("Default");
        playerCameraRoot = GameObject.Find("EtraPlayerCameraRoot");
        OnValidate();
    }

    public override void abilityStart()
    {
        //Check OnValidate and x&y locks and set usedCameraSensitivity
        OnValidate();
        //Get references
        playerCameraRoot = GameObject.Find("EtraPlayerCameraRoot");
        _input = GetComponentInParent<StarterAssetsInputs>();
        playerCameraRoot.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponentInParent<PlayerInput>();
#endif
    }

    public override void abilityUpdate()
    {
        //Return if this ability is disabled
        if (!abilityEnabled)
        {
            return;
        }

        //Shoot a ray towards the center of the screen
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out raycastHit, 999f, aimColliderMask))
        {
            //If the ray hits a layer that the ray can collide with, set the hit location as the pointCharacterIsLookingAt
            objectHit = true;
            pointCharacterIsLookingAt = raycastHit.point;

        }
        else
        {
            //If the ray hits nothing, select a point 'distanceToTargetIfNoObjectIsHitByRay' away to mark as pointCharacterIsLookingAt.
            objectHit = false;
            pointCharacterIsLookingAt = ray.GetPoint(distanceToTargetIfNoObjectIsHitByRay);

        }

        //If debug is active, teleport debugTransform to pointCharacterIsLookingAt 
        if (camLookDebug)
        {
            debugTransform.position = pointCharacterIsLookingAt;
        }

        //If the boolean is checked, set the players forward to where the player is facing
        if (setForwardToPlayerLookDirection)
        {
            Vector3 worldAimTarget = pointCharacterIsLookingAt;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            transform.parent.forward = Vector3.Lerp(transform.parent.forward, aimDirection, Time.deltaTime * rotateTowardsCharacterLookPointSpeed);
        }

    }


    public override void abilityLateUpdate()
    {
        //Return if this ability is disabled
        if (!abilityEnabled)
        {
            return;
        }

        //Set camera rotation from controller or mouse input
        if (_input.look.sqrMagnitude >= _threshold)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * camModX * usedCameraSensitivity * aimSensitivity * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * camModY * usedCameraSensitivity * aimSensitivity * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        playerCameraRoot.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);

    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnValidate()
    {
        //Set the used variable to whatever the new camera sensitivity variable is.
        usedCameraSensitivity = cameraSensitivity;

        //If the locks are selected, then lock a certain direction
        if (lockLookX)
        {
            camModX = 0;
        }
        else
        {
            camModX = 1;
        }
        if (lockLookY)
        {
            camModY = 0;
        }
        else
        {
            camModY = 1;
        }
    }

}

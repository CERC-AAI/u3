using StarterAssets;
using UnityEngine;

public class ABILITY_CharacterMovement : EtraAbilityBaseClass
{
    [Header("Basics")]
	[Tooltip("Move speed of the character in m/s")]
    public float moveSpeed = 2.0f;
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;
    public bool rotateTowardMoveDirection = false;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float rotateTowardMoveDirectionSpeed = 0.12f;

    [Header("Locks")]
    public bool upUnlocked = true;
	public bool downUnlocked = true;
	public bool rightUnlocked = true;
	public bool leftUnlocked = true;


    //References
    [HideInInspector] public Vector2 passedMovementInput;
    private StarterAssetsInputs _input;
    private CharacterController _controller;
    private ABILITY_Sprint sprintSource;
    private GameObject _mainCamera;
    private Animator _animator;
    private bool _hasAnimator;
    private int _animIDSpeed;
    private int _animIDMotionSpeed;
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;

    //Speed adjustments from other ABILITY scripts
    [HideInInspector] public float sprintSpeed; //Set by Ability_Sprint if it exists.
    [HideInInspector] public bool isCrouched = false; //Set by Ability_Crouch if it exists.
    [HideInInspector] public float crouchSpeed;//Set by Ability_Crouch if it exists.


    //Set gameplay var defaults based on gameplay type
    private void Reset()
    {
        if (GetComponentInParent<EtraCharacterMainController>().appliedGameplayType == EtraCharacterMainController.GameplayType.FirstPerson){
            rotateTowardMoveDirection = false; }
        if (GetComponentInParent<EtraCharacterMainController>().appliedGameplayType == EtraCharacterMainController.GameplayType.ThirdPerson) {
            rotateTowardMoveDirection = true; }
    }

    public override void abilityStart()
    {
        //Set Refences
        if (_mainCamera == null)
		{
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
        _hasAnimator = EtrasResourceGrabbingFunctions.TryGetComponentInChildren<Animator>(EtraCharacterMainController.Instance.modelParent);
        if (_hasAnimator) { _animator = EtraCharacterMainController.Instance.modelParent.GetComponentInChildren<Animator>(); }

        _controller = GetComponentInParent<CharacterController>();
        _input = GetComponentInParent<StarterAssetsInputs>();
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        //Set Sprint speed if Sprint is an attached script
        if (this.gameObject.GetComponent<ABILITY_Sprint>() != null)
        {
            sprintSource = this.gameObject.GetComponent<ABILITY_Sprint>();
            sprintSource.sprintSpeed = sprintSpeed;
        }
        else { sprintSpeed = moveSpeed; }
    }
    public override void abilityUpdate() 
    {
        //Make variables to recieve movement input from
        float inputX = _input.move.x;
        float inputY = _input.move.y;

        //Nullify inputs if this ability is disabled
        if (!abilityEnabled)
        {
            inputX = 0;
            inputY = 0;
        }

        //If all movement is unlocked, don't worry about modifying the movement input variables.
        if (rightUnlocked && leftUnlocked && upUnlocked && downUnlocked)
        {
            //Do nothing to inputX and inputY
        }
        else
        {
            //InputX
            if (rightUnlocked == false && leftUnlocked == false)
            {
                inputX = 0;
            }
            else if (rightUnlocked == false && leftUnlocked == true)
            {
                if (inputX > 0) { inputX = 0; }
            }
            else if (leftUnlocked == false && rightUnlocked == true)
            {
                if (inputX < 0) { inputX = 0; }
            }

            //InputY
            if (upUnlocked == false && downUnlocked == false)
            {
                inputY = 0;
            }
            else if (upUnlocked == false && downUnlocked == true)
            {
                if (inputY > 0) { inputY = 0; }
            }
            else if (downUnlocked == false && upUnlocked == true)
            {
                if (inputY < 0) { inputY = 0; }
            }

        }
        passedMovementInput = new Vector2(inputX, inputY);

        //Set correct speed based off of sprint or crouch modifiers
        float targetSpeed;
        targetSpeed = _input.sprint ? sprintSpeed : moveSpeed;
        if (isCrouched)
        {
            targetSpeed = crouchSpeed; 
        }

		// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is no input, set the target speed to 0
		if (passedMovementInput == Vector2.zero) targetSpeed = 0.0f;

		// a reference to the players current horizontal velocity
		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

			// round speed to 3 decimal places
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = targetSpeed;
		}
		_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);


        // normalise input direction
        Vector3 inputDirection = new Vector3(inputX, 0.0f, inputY).normalized;




        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        if (passedMovementInput != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotateTowardMoveDirectionSpeed);
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // if there is a move input rotate player when the player is moving
            if (rotateTowardMoveDirection)
            {
                // rotate to face input direction relative to camera position
                transform.parent.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            // move the player
            EtraCharacterMainController.Instance.addConstantForceToEtraCharacter(targetDirection.normalized * _speed);

        }

        // update animator if using armature
        if (_hasAnimator)
		{
			_animator.SetFloat(_animIDSpeed, _animationBlend);
			_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
		}
        
	}
}

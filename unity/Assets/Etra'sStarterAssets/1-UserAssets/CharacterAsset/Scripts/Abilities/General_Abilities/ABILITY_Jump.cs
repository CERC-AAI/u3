using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
public class ABILITY_Jump : EtraAbilityBaseClass
{
	[Header("Basics")]
    [Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f;
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.05f;
    private float _jumpTimeoutDelta;
    private bool alteredJumpInput;

	[Header("Cam Shake")]
	public bool jumpShakeEnabled = true;
	//The variables here are (intensity, time)
	public Vector2 jumpingShake = new Vector2(0.5f, 0.1f);
    public Vector2 landingShake = new Vector2(1f, 0.1f);

	private bool landing = false;

    //References
    private Animator _animator;
	private StarterAssetsInputs _input;
	public bool _hasAnimator;
	private int _animIDJump;
    private int _animIDFreeFall;

	public override void abilityStart()
    {
		//Set jump cooldown variable
        _jumpTimeoutDelta = JumpTimeout;
		//Get references
        mainController = GetComponentInParent<EtraCharacterMainController>();
        _hasAnimator = EtrasResourceGrabbingFunctions.TryGetComponentInChildren<Animator>(EtraCharacterMainController.Instance.modelParent);
        if (_hasAnimator) { _animator = EtraCharacterMainController.Instance.modelParent.GetComponentInChildren<Animator>(); }
        _input = GetComponentInParent<StarterAssetsInputs>();
		_animIDJump = Animator.StringToHash("Jump");
		_animIDFreeFall = Animator.StringToHash("FreeFall");
	}


	public override void abilityUpdate()
    {
        alteredJumpInput = _input.jump;

		//If ability is disabled do not take jump input
		if (!abilityEnabled)
        {
			alteredJumpInput = false;
		}

		if (mainController.Grounded)
		{
            if (jumpShakeEnabled && landing) { CinemachineShake.Instance.ShakeCamera(landingShake); landing = false; }

            if (alteredJumpInput && _jumpTimeoutDelta <= 0.0f)
			{
				if (jumpShakeEnabled) { CinemachineShake.Instance.ShakeCamera(jumpingShake); }
				landing = true;
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                mainController._verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * mainController.Gravity);

				// update animator if using character
				if (_hasAnimator)
				{
					_animator.SetBool(_animIDJump, true);
				}
			}

			// jump timeout
			if (_jumpTimeoutDelta >= 0.0f)
			{
				_jumpTimeoutDelta -= Time.deltaTime;
			}
		}
		else
		{

			// reset the jump timeout timer
			_jumpTimeoutDelta = JumpTimeout;

			// fall timeout
			if (mainController._fallTimeoutDelta >= 0.0f)
			{
				mainController._fallTimeoutDelta -= Time.deltaTime;
			}
			else
			{
				// update animator if using character
				if (_hasAnimator)
				{
					_animator.SetBool(_animIDFreeFall, true);
				}
			}


			// if we are not grounded, do not jump
			alteredJumpInput = false;
			_input.jump = false;
		}

	}

}

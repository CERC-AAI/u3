using Cinemachine;
using UnityEngine;
using UnityEditor;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class EtraCharacterMainController : MonoBehaviour
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //PUBLIC AND PRIVATE VARIABLES
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //Instance (this is for easy referencing of the one Etra Character in the scene, Instance is set up in OnValidate())
        [HideInInspector] public static EtraCharacterMainController Instance;

        //************************
        //Gameplay type selector
        //************************
        //Make custom enums
        [HideInInspector]
        public enum GameplayType
        {
            FirstPerson,
            ThirdPerson
        }
        [HideInInspector]
        public enum Model
        {
            DefaultArmature,
            Capsule,
            Voxel,
            None
        }
        //These public variables let you select what gameplay type you want
        [SerializeField] private GameplayType gameplayType;
        [SerializeField] private Model characterModel;
        [HideInInspector] public GameplayType appliedGameplayType = EtraCharacterMainController.GameplayType.FirstPerson;
        [HideInInspector] public Model appliedCharacterModel = EtraCharacterMainController.Model.Capsule;
        //This is space for the "Apply Gameplay Changes" button generated in UnityEditorForEtraCharacterMainController.cs
        [Space(40)]

  
        public EtraAbilityManager abilityManager;
        public Transform modelParent;

        [Header("Gravity and Ground")]
        //************************
        //Gravity and floor collision variables
        //************************
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;
        [HideInInspector]
        public float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        [HideInInspector]
        public float _fallTimeoutDelta;
        // animation IDs for floor collision events
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;

        //************************
        //Camera Setup variables
        //************************
        GameObject etraFollowCam;
        private NoiseSettings shake;
        private NoiseSettings handheldNormal;

        //************************
        //References
        //************************
        private bool _hasAnimator;
        private Animator _animator;
        private CharacterController _controller;

        //************************
        //Externally called function variables
        //************************
        private const int APPLIED_FORCES_AMOUNT = 6;
        private Vector3[] appliedForces = new Vector3[APPLIED_FORCES_AMOUNT]; //Up to six forces can be applied to the character in a single frame. Adjust this to change.

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //SETUP FUNCTIONS
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        #region Gameplay Type Changing Functions 

        //This function is what you want to run if you wish to change gameplay settings from an outside script
        public void applyGameplayChanges(GameplayType passedGameplayType, Model passedCharacterModel)
        {
            appliedGameplayType = passedGameplayType;
            appliedCharacterModel = passedCharacterModel;
            applyGameplayChanges();

        }

        //This function runs when the inspector button is pressed
        public void applyGameplayChangesInspectorButtonPressed()
        {
            appliedGameplayType = gameplayType;
            appliedCharacterModel = characterModel;
            applyGameplayChanges();
        }

        private void applyGameplayChanges()
        {
            //Update public inspector variables
            gameplayType = appliedGameplayType;
            characterModel = appliedCharacterModel;

            //Destroy the current Cinemachine Virtual Camera
            etraFollowCam = GameObject.Find("Etra'sStarterAssetsFollowCamera");
            DestroyImmediate(etraFollowCam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>());
            //Create an editable Cinemachine Virtual Camera
            etraFollowCam.GetComponent<CinemachineVirtualCamera>().AddCinemachineComponent<Cinemachine3rdPersonFollow>();
            Cinemachine3rdPersonFollow newCamComponent = etraFollowCam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            newCamComponent.VerticalArmLength = 0;
            //Maken the new camera collide with and ignore correct layers
            newCamComponent.CameraCollisionFilter = LayerMask.GetMask("Default");
            newCamComponent.IgnoreTag = "Player";
            newCamComponent.CameraRadius = 0.15f;

            switch (appliedGameplayType)
            {
                case EtraCharacterMainController.GameplayType.FirstPerson:
                    //FPS Default Camera Settings
                    newCamComponent.CameraDistance = 0;
                    newCamComponent.CameraSide = 0.6f;
                    newCamComponent.ShoulderOffset = new Vector3(0, 0, 0);
                    newCamComponent.Damping = new Vector3(0.0f, 0.0f, 0.0f);

                    //FPS Ability Variable Default adjustment
                    if (GetComponentInChildren<ABILITY_CameraMovement>())
                    {
                        GetComponentInChildren<ABILITY_CameraMovement>().setForwardToPlayerLookDirection = true;
                    }
                    if (GetComponentInChildren<ABILITY_CharacterMovement>())
                    {
                        GetComponentInChildren<ABILITY_CharacterMovement>().rotateTowardMoveDirection = false;
                    }
                    break;

                case EtraCharacterMainController.GameplayType.ThirdPerson:
                    //TPS Default Camera Settings
                    newCamComponent.CameraDistance = 4;
                    newCamComponent.CameraSide = 1f;
                    newCamComponent.ShoulderOffset = new Vector3(0.7f, 0.25f, 0);
                    newCamComponent.Damping = new Vector3(0.1f, 0.25f, 0.3f);

                    //TPS Ability Variable Default adjustment
                    if (GetComponentInChildren<ABILITY_CameraMovement>())
                    {
                        GetComponentInChildren<ABILITY_CameraMovement>().setForwardToPlayerLookDirection = false;
                    }
                    if (GetComponentInChildren<ABILITY_CharacterMovement>())
                    {
                        GetComponentInChildren<ABILITY_CharacterMovement>().rotateTowardMoveDirection = true;
                    }
                    break;
            }

            
            //Destroy the current character model
            foreach (Transform child in modelParent)
            {
                DestroyImmediate(child.gameObject);
            }
            GameObject model;
            //Select correct character model
            switch (appliedCharacterModel)
            {

                case EtraCharacterMainController.Model.DefaultArmature:
                    model = EtrasResourceGrabbingFunctions.addPrefabFromAssetsByName("DefaultArmatureCharacterModel", modelParent, false, Vector3.zero);
                    model.transform.localPosition = EtrasResourceGrabbingFunctions.getPrefabFromAssetsByName("DefaultArmatureCharacterModel").transform.localPosition;
                    break;
                case EtraCharacterMainController.Model.Capsule:
                    model = EtrasResourceGrabbingFunctions.addPrefabFromAssetsByName("CapsuleCharacterModel", modelParent, false, Vector3.zero);
                    model.transform.localPosition = EtrasResourceGrabbingFunctions.getPrefabFromAssetsByName("CapsuleCharacterModel").transform.localPosition;
                    break;
                case EtraCharacterMainController.Model.Voxel:
                    model = EtrasResourceGrabbingFunctions.addPrefabFromAssetsByName("VoxelCharacterModel", modelParent, false, Vector3.zero);
                    model.transform.localPosition = EtrasResourceGrabbingFunctions.getPrefabFromAssetsByName("VoxelCharacterModel").transform.localPosition;
                    break;
                case EtraCharacterMainController.Model.None:
                    break;
            }
        }

        #endregion
        #region Setup, Reset, and OnValidate functions
        private void Reset()
        {
            GroundLayers = LayerMask.GetMask("Default");
        }

        private void setUpCinemachineScreenShakeNoiseProfile()
        {

            //Set up cinemachine screen shake profile
            string[] filePaths;
            filePaths = AssetDatabase.FindAssets("6DShake");
            if (filePaths[0] == null) { Debug.LogError("6DShake Cinemachine Noise Settings not found."); return; }
            shake = (NoiseSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(filePaths[0]), typeof(NoiseSettings));

            filePaths = AssetDatabase.FindAssets("HandheldNormal");
            if (filePaths[0] == null) { Debug.LogError("HandheldNormal Cinemachine Noise Settings not found."); return; }
            handheldNormal = (NoiseSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(filePaths[0]), typeof(NoiseSettings));

            etraFollowCam = GameObject.Find("Etra'sStarterAssetsFollowCamera");
            etraFollowCam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = shake;
        }

        private void AssignAnimationIDs()
        {
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
        }
        #endregion
        #region Grounded and Gravity Functions
        private void GroundedCheck()
        {

            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            //Vector3 spherePosition = new Vector3(characterController.center.x, characterController.center.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            //Ignore objects with the player tag cause layers are not usable for a package
            //To fix this we need player to manually add a player layer. 
            int maxColliders = 1;
            Collider[] hitColliders = new Collider[maxColliders];
            int numColliders = Physics.OverlapSphereNonAlloc(spherePosition, GroundedRadius, hitColliders);
            if (hitColliders.Length == 1 && hitColliders[0] != null)
            {
                if (hitColliders[0].gameObject.tag == "Player")
                {
                    Grounded = false;
                }
            }

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void ApplyGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }



        }


        private void OnDrawGizmosSelected()
        {
            _controller = GetComponent<CharacterController>();
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        #endregion

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //MAIN FUNCTIONS
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void Awake()
        {
            Physics.autoSyncTransforms = true;

            //Set up Instance so it can be easily referenced. 
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            //Run setup functions
            setUpCinemachineScreenShakeNoiseProfile();
            AssignAnimationIDs();
            //Set reference variables
            _hasAnimator = TryGetComponent(out _animator);
            _hasAnimator = EtrasResourceGrabbingFunctions.TryGetComponentInChildren<Animator>(modelParent);
            if (_hasAnimator) { _animator = modelParent.GetComponentInChildren<Animator>(); }
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            //Calculate Grounded and gravity
            GroundedCheck();
            ApplyGravity();
            //Apply vertical velocity from gravity or jump every frame
            addConstantForceToEtraCharacter(new Vector3(0.0f, _verticalVelocity, 0.0f));
            updateImpulseVariables();

        }

        private void LateUpdate()
        {
            //Apply movement and forces to the character controller
            Vector3 overallForce = Vector3.zero;
            // string log= "";
            for (int i = 0; i < appliedForces.Length; i++)
            {
                //log += appliedForces[i] + ", ";
                overallForce += appliedForces[i];
            }
            //We want to call character controller .Move() once every frame with one overall vector or it acts wonky.  
            // Debug.Log(log);
            _controller.Move(overallForce * Time.deltaTime);
            //Reset all applied forces to Vector3.zero
            appliedForces = new Vector3[APPLIED_FORCES_AMOUNT];
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //COLLISIONS
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (abilityManager.GetComponent<ABILITY_RigidbodyPush>())
            {
                abilityManager.GetComponent<ABILITY_RigidbodyPush>().PushRigidBodies(hit);
            }
        }



        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //CUSTOM FUNCTIONS TO BE CALLED FROM OTHER SCRIPTS
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public void addConstantForceToEtraCharacter(Vector3 addedForce)
        {
            int emptyIndex = -1;
            for (int i = 0; i < appliedForces.Length; i++)
            {
                if (appliedForces[i].Equals(Vector3.zero))
                {
                    emptyIndex = i;
                    i = appliedForces.Length;
                }

                if (i == appliedForces.Length - 1)
                {
                    Debug.Log("Cannot add any more forces to character controller in this singular frame. To add more forces to the character" +
                        " please increase APPLIED_FORCES_AMOUNT in EtraCharacterMainController.cs .");
                    return;
                }

            }
            appliedForces[emptyIndex] = addedForce;
        }


        Vector3 impact = Vector3.zero;
        public void updateImpulseVariables()
        {
            if (impact.magnitude > 0.2f)
            {
                _controller.Move(impact * Time.deltaTime);
            }

            impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
        }

        public void addImpulseForceToEtraCharacter(Vector3 direction, float force)
        {
            direction.Normalize();
            if (direction.y < 0) direction.y = -direction.y; // reflect down force on the ground
            impact += direction.normalized * force / 3.0f;
        }

    }

}


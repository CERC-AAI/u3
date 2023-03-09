using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool aim;
        public bool shoot;
        public bool crouch;
        public bool interact;
        public bool dash;



        [Header("Movement Settings")]
        public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;
#endif

        [Header("Usable Item Inventory")]
        public float usableItemInventoryScroll;
        public bool item0Select;
        public bool item1Select;
        public bool item2Select;
        public bool item3Select;
        public bool item4Select;
        public bool item5Select;
        public bool item6Select;
        public bool item7Select;
        public bool item8Select;
        public bool item9Select;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnAim(InputValue value)
        {
            AimInput(value.isPressed);
        }

        public void OnShoot(InputValue value)
        {
            ShootInput(value.isPressed);
        }

        public void OnCrouch(InputValue value)
        {
            CrouchInput(value.isPressed);
        }

        public void OnInteract(InputValue value)
        {
            InteractInput(value.isPressed);
        }

        public void OnDash(InputValue value)
        {
            DashInput(value.isPressed);
        }

        public void OnMouseWheelInventoryScroll(InputValue value)
        {
            InventoryScrollInput(value.Get<float>());
        }

        public void OnUsableItemScroll(InputValue value)
        {
            InventoryScrollInput(value.Get<float>());
        }

        public void OnSelectItem0(InputValue value)
        {
            Item0SelectInput(value.isPressed);
        }

        public void OnSelectItem1(InputValue value)
        {
            Item1SelectInput(value.isPressed);
        }

        public void OnSelectItem2(InputValue value)
        {
            Item2SelectInput(value.isPressed);
        }

        public void OnSelectItem3(InputValue value)
        {
            Item3SelectInput(value.isPressed);
        }

        public void OnSelectItem4(InputValue value)
        {
            Item4SelectInput(value.isPressed);
        }

        public void OnSelectItem5(InputValue value)
        {
            Item5SelectInput(value.isPressed);
        }

        public void OnSelectItem6(InputValue value)
        {
            Item6SelectInput(value.isPressed);
        }

        public void OnSelectItem7(InputValue value)
        {
            Item7SelectInput(value.isPressed);
        }
        public void OnSelectItem8(InputValue value)
        {
            Item8SelectInput(value.isPressed);
        }

        public void OnSelectItem9(InputValue value)
        {
            Item9SelectInput(value.isPressed);
        }


#else
	// old input system support

        void Update()
        {
            float horizantalMove = Input.GetAxis("Horizontal");
            float verticalMove = Input.GetAxis("Vertical");
            MoveInput(new Vector2(horizantalMove, verticalMove));

            float horizantalLook = Input.GetAxis("Mouse X"); //For controller support you need to map the right joystick x to this axis
            float verticalLook = Input.GetAxis("Mouse Y") *-1; //For controller support you need to map the right joystick y to this axis
            LookInput(new Vector2(horizantalLook, verticalLook));

            if (Input.GetButtonDown("Jump"))
            {
                JumpInput(true);
            }

            if (Input.GetButtonDown("Fire1"))
            {
                ShootInput(true);
            }

            ///****************************
            /// THESE AXIS AND BUTTONS NEED ADDED
            ///****************************
            /*
            if (Input.GetButtonDown("Dash"))
            {
                DashInput(true);
            }
            SprintInput(Input.GetButton("SprintInput"));
            AimInput(Input.GetButton("AimInput"));
            CrouchInput(Input.GetButton("CrouchInput"));
            float itemScroll = Input.GetAxis("UsableItemScroll");
            InventoryScrollInput(itemScroll);
            */

            //Inventory management
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            InventoryScrollInput(mouseScroll);
            if (Input.GetKeyDown(KeyCode.Alpha1)) {Item0SelectInput(true); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { Item1SelectInput(true); }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { Item2SelectInput(true); }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { Item3SelectInput(true); }
            if (Input.GetKeyDown(KeyCode.Alpha5)) { Item4SelectInput(true); }
            if (Input.GetKeyDown(KeyCode.Alpha6)) { Item5SelectInput(true); }
            if (Input.GetKeyDown(KeyCode.Alpha7)) { Item6SelectInput(true); }
            if (Input.GetKeyDown(KeyCode.Alpha8)) { Item7SelectInput(true); }
            if (Input.GetKeyDown(KeyCode.Alpha9)) { Item8SelectInput(true); }
            if (Input.GetKeyDown(KeyCode.Alpha0)) { Item9SelectInput(true); }

        }

#endif



        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void AimInput(bool newAimState)
        {
            aim = newAimState;
        }

        public void ShootInput(bool newShootInput)
        {
            shoot = newShootInput;
        }

        public void CrouchInput(bool newCrouchInput)
        {
            crouch = newCrouchInput;

        }

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }

        public void DashInput(bool newDashState)
        {
            dash = newDashState;
        }

        public void InventoryScrollInput(float newScrollState)
        {
            if (newScrollState > 0)
            {
                newScrollState = 1;
            }
            else if (newScrollState < 0)
            {
                newScrollState = -1;
            }

            usableItemInventoryScroll = newScrollState;
        }

        public void Item0SelectInput(bool newItem0PressedState)
        {
            item0Select = newItem0PressedState;
        }
        public void Item1SelectInput(bool newItem1PressedState)
        {
            item1Select = newItem1PressedState;
        }

        public void Item2SelectInput(bool newItem2PressedState)
        {
            item2Select = newItem2PressedState;
        }

        public void Item3SelectInput(bool newItem3PressedState)
        {
            item3Select = newItem3PressedState;
        }

        public void Item4SelectInput(bool newItem4PressedState)
        {
            item4Select = newItem4PressedState;
        }
        public void Item5SelectInput(bool newItem5PressedState)
        {
            item5Select = newItem5PressedState;
        }

        public void Item6SelectInput(bool newItem6PressedState)
        {
            item6Select = newItem6PressedState;
        }

        public void Item7SelectInput(bool newItem7PressedState)
        {
            item7Select = newItem7PressedState;
        }

        public void Item8SelectInput(bool newItem8PressedState)
        {
            item8Select = newItem8PressedState;
        }

        public void Item9SelectInput(bool newItem9PressedState)
        {
            item9Select = newItem9PressedState;
        }


#if !UNITY_IOS || !UNITY_ANDROID

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

#endif

    }

}
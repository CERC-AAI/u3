using UnityEngine;

[RequireComponent(typeof(ABILITY_CharacterMovement))]
public class ABILITY_Sprint : EtraAbilityBaseClass
{
    //This script is just a contaiiner for the sprint speed variable.
    //On Validate it will set the public var sprintSpeed to Character_Movement's sprint speed.
    public float sprintSpeed = 6;
    ABILITY_CharacterMovement movementAbility;

    public override void abilityStart()
    {
        OnValidate();
    }

    private void Reset()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        movementAbility = GetComponent<ABILITY_CharacterMovement>();
        movementAbility.sprintSpeed = sprintSpeed;
    }

}

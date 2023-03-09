using UnityEditor;
using UnityEngine;

public class EtraAbilityManager : MonoBehaviour
{
    [Header("The Abilities Will Start(), Update(), and LateUpdate() in The Following Order:")]
    public EtraAbilityBaseClass[] characterAbilityUpdateOrder;
    #region Functions to update The characterAbilityUpdateOrder Array
    EtraAbilityManager()
    {
        ObjectFactory.componentWasAdded -= HandleComponentAdded;
        ObjectFactory.componentWasAdded += HandleComponentAdded;

        EditorApplication.quitting -= OnEditorQuiting;
        EditorApplication.quitting += OnEditorQuiting;
    }
    private  void HandleComponentAdded(UnityEngine.Component obj)
    {
        updateCharacterAbilityArray();
    }

    private void updateCharacterAbilityArray()
    {
        removeNullAbilitySlots();
        //I understand this is Big O^2 however, it only runs on validate. What's more important is navigation of the final structure (an array) is as fast as possible.
        EtraAbilityBaseClass[] grabbedAbilities;

        if(this == null)
        {
            return;
        }

        if(GetComponent<EtraAbilityBaseClass>() != null)
        {
            grabbedAbilities = GetComponents<EtraAbilityBaseClass>();
        }
        else
        {
            grabbedAbilities = new EtraAbilityBaseClass[0];
        }



        foreach (EtraAbilityBaseClass ability in grabbedAbilities)
        {
            bool abilityFound = false;

            foreach (EtraAbilityBaseClass setAbility in characterAbilityUpdateOrder)
            {
                if (ability.Equals(setAbility))
                {
                    abilityFound = true;
                }
            }

            if (!abilityFound)
            {
                increaseAbilityArrayWithNewElement(ability);
            }
        }
    }

    private void removeNullAbilitySlots()
    {
        
        if (characterAbilityUpdateOrder.Length == 0)
        {
            return;
        }

        bool slotsNeedRemoved = true; 
        while (slotsNeedRemoved)
        {
            slotsNeedRemoved = false;
            int elementToPass = 0;
            for (int i = 0; i < characterAbilityUpdateOrder.Length; i++)
            {
                if (characterAbilityUpdateOrder[i] == null)
                {
                    slotsNeedRemoved = true;
                    elementToPass = i;
                    i = characterAbilityUpdateOrder.Length;
                }
            }

            if (slotsNeedRemoved)
            {
                characterAbilityUpdateOrder = removeElementFromArray(elementToPass);
            }

        }


    }

    private EtraAbilityBaseClass[] removeElementFromArray(int elementToSkip)
    {
        EtraAbilityBaseClass[] shortenedArray = new EtraAbilityBaseClass[characterAbilityUpdateOrder.Length-1];

        int iterator = 0;
        for (int i = 0; i < characterAbilityUpdateOrder.Length; i++)
        {
            if (i != elementToSkip)
            {
                shortenedArray[iterator] = characterAbilityUpdateOrder[i];
                iterator++;
            }
        }

        return shortenedArray;
    }

    private void increaseAbilityArrayWithNewElement(EtraAbilityBaseClass abilityToAdd)
    {
        EtraAbilityBaseClass[] temp = new EtraAbilityBaseClass[characterAbilityUpdateOrder.Length + 1];

        for (int i = 0; i < characterAbilityUpdateOrder.Length; i++)
        {
            temp[i] = characterAbilityUpdateOrder[i];
        }

        temp[characterAbilityUpdateOrder.Length] = abilityToAdd;


        characterAbilityUpdateOrder = temp;
    }

    private void OnEditorQuiting()
    {
        ObjectFactory.componentWasAdded -= HandleComponentAdded;
        EditorApplication.quitting -= OnEditorQuiting;
    }

    private void Reset()
    {
        updateCharacterAbilityArray();
    }

    private void OnValidate()
    {
        updateCharacterAbilityArray();
    }

    #endregion

    private void Awake()
    {
        //This is ran to remove any null array elements
        removeNullAbilitySlots();
    }

    //Run the EtraAbilityBaseClass functions in the order defined above
    private void Start()
    {
        for (int i = 0; i < characterAbilityUpdateOrder.Length; i++)
        {
            characterAbilityUpdateOrder[i].abilityStart();
        }
    }
    private void Update()
    {
        for (int i = 0; i < characterAbilityUpdateOrder.Length; i++)
        {
            characterAbilityUpdateOrder[i].abilityUpdate();
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < characterAbilityUpdateOrder.Length; i++)
        {
            characterAbilityUpdateOrder[i].abilityLateUpdate();
        }
    }

}



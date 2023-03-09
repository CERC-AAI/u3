using StarterAssets;
using UnityEngine;
using System.Collections;
using UnityEditor;
using static UnityEditor.Progress;


public class EtraFPSUsableItemManager : MonoBehaviour
{
    //Visible in Inspector
    public bool playEquipAnims = true;
    public bool playUnequipAnims = true;

    [Header("The Items Will Be Selected In This Order:")]
    public usableItemScriptAndPrefab[] usableItems;


    #region Functions to update The characterAbilityUpdateOrder Array
    EtraFPSUsableItemManager()
    {
        ObjectFactory.componentWasAdded -= HandleComponentAdded;
        ObjectFactory.componentWasAdded += HandleComponentAdded;

        EditorApplication.quitting -= OnEditorQuiting;
        EditorApplication.quitting += OnEditorQuiting;
    }
    private void HandleComponentAdded(UnityEngine.Component obj)
    {
        updateUsableItemsArray();
    }

    private void updateUsableItemsArray()
    {
        removeNullItemSlots();
        //I understand this is Big O^2 however, it only runs on validate. What's more important is navigation of the final structure (an array) is as fast as possible.
        EtraFPSUsableItemBaseClass[] grabbedUsableItems;

        if (this == null)
        {
            return;
        }

        if (GetComponent<EtraFPSUsableItemBaseClass>() != null)
        {
            grabbedUsableItems = GetComponents<EtraFPSUsableItemBaseClass>();
        }
        else
        {
            grabbedUsableItems = new EtraFPSUsableItemBaseClass[0];
        }

        foreach (EtraFPSUsableItemBaseClass item in grabbedUsableItems)
        {
            bool itemFound = false;

            foreach (usableItemScriptAndPrefab setItem in usableItems)
            {
                if (item.Equals(setItem.script))
                {
                    itemFound = true;
                }
            }

            if (!itemFound)
            {
                usableItemScriptAndPrefab newItem = new usableItemScriptAndPrefab(item);
                increaseAbilityArrayWithNewElement(newItem);
            }
        }




  
    }

    private void removeNullItemSlots()
    {

        if (usableItems == null)
        {
            return;
        }

        if (usableItems.Length == 0)
        {
            return;
        }

        bool slotsNeedRemoved = true;
        while (slotsNeedRemoved)
        {
            slotsNeedRemoved = false;
            int elementToPass = 0;
            for (int i = 0; i < usableItems.Length; i++)
            {
                if (usableItems[i].script == null)
                {
                    slotsNeedRemoved = true;
                    elementToPass = i;
                    i = usableItems.Length;
                }
            }

            if (slotsNeedRemoved)
            {
                usableItems = removeElementFromArray(elementToPass);
            }

        }


    }

    private usableItemScriptAndPrefab[] removeElementFromArray(int elementToSkip)
    {
        usableItemScriptAndPrefab[] shortenedArray = new usableItemScriptAndPrefab[usableItems.Length - 1];

        int iterator = 0;
        for (int i = 0; i < usableItems.Length; i++)
        {
            if (i != elementToSkip)
            {
                shortenedArray[iterator] = usableItems[i];
                iterator++;
            }
        }

        return shortenedArray;
    }

    private void increaseAbilityArrayWithNewElement(usableItemScriptAndPrefab abilityToAdd)
    {
        usableItemScriptAndPrefab[] temp = new usableItemScriptAndPrefab[usableItems.Length + 1];

        for (int i = 0; i < usableItems.Length; i++)
        {
            temp[i] = usableItems[i];
        }

        temp[usableItems.Length] = abilityToAdd;


        usableItems = temp;
    }

    private void OnEditorQuiting()
    {
        ObjectFactory.componentWasAdded -= HandleComponentAdded;
        EditorApplication.quitting -= OnEditorQuiting;
    }


    #endregion




    //Not in Inspector
    private int activeItemNum = 0;
    [HideInInspector] public GameObject activeItemPrefab;
    [HideInInspector] bool isEquipping = false;

    //References
    StarterAssetsInputs starterAssetsInputs;
    GameObject cameraRoot;



    [System.Serializable]
    public class usableItemScriptAndPrefab
    {
        public EtraFPSUsableItemBaseClass script;
        [HideInInspector]public GameObject prefab;

        public usableItemScriptAndPrefab(EtraFPSUsableItemBaseClass passedScript)
        {
            script = passedScript;
            prefab = EtrasResourceGrabbingFunctions.getPrefabFromAssetsByName(script.getNameOfPrefabToLoad());
        }
    }

    

    //Reset is ran by the character creator adding this component
    public void Reset()
    {
        updateUsableItemsArray();
        //Add usable FPS item camera if it does not exist
        GameObject FPSUsableItemsCamera = EtrasResourceGrabbingFunctions.addPrefabFromAssetsByName("FPSUsableItemsCamera" , GameObject.Find("EtraPlayerCameraRoot").transform, false, Vector3.zero);
        //Add usable FPS item camera script to the camera to check for the FPSUsableItem Layer
        if (FPSUsableItemsCamera !=null && FPSUsableItemsCamera.GetComponent<FPS_Item_Cam_Checks>() == null) { FPSUsableItemsCamera.AddComponent<FPS_Item_Cam_Checks>(); }
    }


    private void OnValidate() //ON COMPONENT ADD
    {
        updateUsableItemsArray();
        //go through usable items.
        //load all items in game object prefabs so searching does not have to be done.
    }


    private void Awake()
    {
        removeNullItemSlots();
        cameraRoot = GameObject.Find("EtraPlayerCameraRoot");
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();

        for (int i = 0; i < usableItems.Length; i++)
        {
            usableItems[i].script.enabled = false;
        }

    }


    private void Start()
    {
        instatiateItemAtStart();
    }


    void instatiateItemAtStart()
    {
        GameObject newItem = Instantiate(usableItems[activeItemNum].prefab);
        newItem.transform.SetParent(cameraRoot.transform);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;
        activeItemPrefab = newItem;
        usableItems[activeItemNum].script.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Some sort of check that were not moving to same item
        if (isEquipping)
        {
            return;
        }

        //Number Keys select
        #region Number Key Item Selection
        if (starterAssetsInputs.item0Select)
        {
            if (activeItemNum != 0 && usableItems.Length > 0)
            {
                StartCoroutine(equipItem(0));
            }
        }

        if (starterAssetsInputs.item1Select)
        {
            if (activeItemNum != 1 && usableItems.Length > 1)
            {
                StartCoroutine(equipItem(1));
            }
        }

        if (starterAssetsInputs.item2Select)
        {
            if (activeItemNum != 2 && usableItems.Length > 2 )
            {
                StartCoroutine(equipItem(2));
            }
        }

        if (starterAssetsInputs.item3Select)
        {
            if (activeItemNum != 3 && usableItems.Length > 3)
            {
                StartCoroutine(equipItem(3));
            }
        }

        if (starterAssetsInputs.item4Select)
        {
            if (activeItemNum != 4 && usableItems.Length > 4)
            {
                StartCoroutine(equipItem(4));
            }
        }

        if (starterAssetsInputs.item5Select)
        {
            if (activeItemNum != 5 && usableItems.Length > 5)
            {
                StartCoroutine(equipItem(5));
            }
        }

        if (starterAssetsInputs.item6Select)
        {
            if (activeItemNum != 6 && usableItems.Length > 6)
            {
                StartCoroutine(equipItem(6));
            }
        }

        if (starterAssetsInputs.item7Select)
        {
            if (activeItemNum != 7 && usableItems.Length > 7)
            {
                StartCoroutine(equipItem(7));
            }
        }

        if (starterAssetsInputs.item8Select)
        {
            if (activeItemNum != 8 && usableItems.Length > 8)
            {
                StartCoroutine(equipItem(8));
            }
        }

        if (starterAssetsInputs.item9Select)
        {
            if (activeItemNum != 9 && usableItems.Length > 9)
            {
                StartCoroutine(equipItem(9));
            }
        }
        #endregion

        if (usableItems.Length <= 1)
        {
            return;
        }

        //Mouse wheel and shoulder button scroll
        if (starterAssetsInputs.usableItemInventoryScroll == 1)
        {
            int itemToMoveTo = activeItemNum + 1;

            if (itemToMoveTo >= usableItems.Length)
            {
                itemToMoveTo = 0;
            }
            StartCoroutine(equipItem(itemToMoveTo));

        }

        if (starterAssetsInputs.usableItemInventoryScroll == -1)
        {
            int itemToMoveTo = activeItemNum - 1;

            if (itemToMoveTo < 0)
            {
                itemToMoveTo = usableItems.Length -1;
            }
            StartCoroutine(equipItem(itemToMoveTo));

        }

    }

    IEnumerator equipItem(int newItemNum)
    {

        isEquipping = true;


        if (playUnequipAnims)
        {
            usableItems[activeItemNum].script.runUnequipAnimation();
            yield return new WaitForSeconds(usableItems[activeItemNum].script.getItemUnequipSpeed());
        }

        usableItems[activeItemNum].script.enabled = false;
        Destroy(activeItemPrefab);

        activeItemNum = newItemNum;
        GameObject newItem = Instantiate(usableItems[activeItemNum].prefab);
        newItem.transform.SetParent(cameraRoot.transform);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;
        activeItemPrefab = newItem;

        if (playEquipAnims)
        {
            activeItemPrefab.transform.localRotation = Quaternion.Euler(usableItems[activeItemNum].script.getItemUnequipRotation());
            usableItems[activeItemNum].script.runEquipAnimation();
            yield return new WaitForSeconds(usableItems[activeItemNum].script.getItemEquipSpeed());
        }

        usableItems[activeItemNum].script.enabled = true;
        setInputsToDefault();
        isEquipping = false;

        
    }

    void setInputsToDefault()
    {
        starterAssetsInputs.usableItemInventoryScroll = 0;

        starterAssetsInputs.item0Select = false;
        starterAssetsInputs.item1Select = false;
        starterAssetsInputs.item2Select = false;
        starterAssetsInputs.item3Select = false;
        starterAssetsInputs.item4Select = false;
        starterAssetsInputs.item5Select = false;
        starterAssetsInputs.item6Select = false;
        starterAssetsInputs.item7Select = false;
        starterAssetsInputs.item8Select = false;
        starterAssetsInputs.item9Select = false;
    }

}

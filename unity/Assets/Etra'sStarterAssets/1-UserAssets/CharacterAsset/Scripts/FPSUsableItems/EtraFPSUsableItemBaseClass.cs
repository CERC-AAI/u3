using System.Collections;
using UnityEngine;

public abstract class EtraFPSUsableItemBaseClass : MonoBehaviour
{
    public abstract string getNameOfPrefabToLoad(); //Require the name of loaded prefab in all scripts

    #region Default Equip and Unequip Animations

    private EtraFPSUsableItemManager _manager;

    [HideInInspector] public bool isEquipping;
    public virtual float getItemEquipSpeed() { return 0.2f; }
    public virtual Vector3 getItemEquipRotation() { return new Vector3(0f, 0f, 0f); }

    [HideInInspector] public bool isUnequipping;
    public virtual float getItemUnequipSpeed() { return 0.2f; }
    public virtual Vector3 getItemUnequipRotation() { return new Vector3(35f, 0f, 0f); }

    public virtual void runEquipAnimation() //return true when Animation is complete
    {
        isEquipping = true;
        StartCoroutine(equipAnimationCoroutine());
    }

    IEnumerator equipAnimationCoroutine()
    {
        if (_manager == null) { _manager = GetComponent<EtraFPSUsableItemManager>(); }
        LeanTween.rotateLocal(_manager.activeItemPrefab, getItemEquipRotation(), getItemEquipSpeed()).setEaseInOutSine();
        yield return new WaitForSeconds(getItemEquipSpeed());
        isEquipping = false;
    }

    public virtual void runUnequipAnimation() //return true when Animation is complete
    {
        isUnequipping = true;
        StartCoroutine(unequipAnimationCoroutine());
    }
    IEnumerator unequipAnimationCoroutine()
    {
        if (_manager == null) { _manager = GetComponent<EtraFPSUsableItemManager>(); }
        LeanTween.rotateLocal(_manager.activeItemPrefab, getItemUnequipRotation(), getItemUnequipSpeed()).setEaseInOutSine();
        yield return new WaitForSeconds(getItemUnequipSpeed());
        isUnequipping = false;
    }

    #endregion
}



using StarterAssets;
using System.Collections;
using UnityEngine;

public class USABLEITEM_FPS_Flashlight : EtraFPSUsableItemBaseClass
{
    //Name of Prefab to load and required function
    private string nameOfPrefabToLoadFromAssets = "FPSFlashlightGroup";
    public override string getNameOfPrefabToLoad() { return nameOfPrefabToLoadFromAssets; }

    //Public variables
    [Header("Basics")]
    public float lightIntensity = 2f;
    public float lightRange = 20f;
    public Color lightColor = Color.white;

    //State check
    bool flashLightOn = false;
    bool isAnimating = false;

    //References
    StarterAssetsInputs starterAssetsInputs;
    Light flashlightLight;
    Animator flashlightAnimator;


    private void OnValidate()
    {
        //This code allows the flashlight light to be updated live
        setPublicFlashlightValues();
    }

    void setPublicFlashlightValues()
    {
        if (flashlightLight == null) { return; }
        flashlightLight.GetComponent<Light>().intensity = lightIntensity;
        flashlightLight.GetComponent<Light>().range = lightRange;
        flashlightLight.GetComponent<Light>().color = lightColor;
    }


    private void Awake()
    {
        this.enabled = false; 
    }

    public void OnEnable()
    {
        //Set references WHEN THIS SCRIPT IS ENABLED
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        GameObject flashLightReference = GetComponent<EtraFPSUsableItemManager>().activeItemPrefab;
        flashlightAnimator = flashLightReference.GetComponentInChildren<Animator>();
        flashlightLight = flashLightReference.GetComponentInChildren<Light>();
        setPublicFlashlightValues();

        if (flashLightOn)
        {
            StartCoroutine(turnFlashlightOn());
        }
        else
        {
            flashlightLight.enabled = flashLightOn;
        }
        
    }
    



    public void Update()
    {
        if (starterAssetsInputs.shoot && !isAnimating)
        {
            changeFlashlightState();
        }
    }

    void changeFlashlightState()
    {
        if (!flashLightOn)
        {
            StartCoroutine(turnFlashlightOn());
        }
        else
        {
            StartCoroutine(turnFlashlightOff());
        }
    }

    IEnumerator turnFlashlightOn()
    {
        isAnimating = true;
        flashlightAnimator.SetBool("FlashlightOn", true);
        yield return new WaitForSeconds(0.1f);
        flashLightOn = true;
        flashlightLight.enabled = flashLightOn;
        starterAssetsInputs.shoot = false;
        isAnimating = false;
    }

    IEnumerator turnFlashlightOff()
    {
        isAnimating = true;
        flashlightAnimator.SetBool("FlashlightOn", false);
        flashLightOn = false;
        flashlightLight.enabled = flashLightOn;
        yield return new WaitForSeconds(0.1f);
        starterAssetsInputs.shoot = false;
        isAnimating = false;
    }


}

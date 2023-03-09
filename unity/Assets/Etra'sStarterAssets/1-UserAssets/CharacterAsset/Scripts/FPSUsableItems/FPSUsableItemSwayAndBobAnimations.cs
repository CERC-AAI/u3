using StarterAssets;
using System;
using UnityEngine;

public class FPSUsableItemSwayAndBobAnimations : MonoBehaviour
{
    //From BuffaLou
    //https://www.youtube.com/watch?v=DR4fTllQnXg
    /*
    The MIT License (MIT)
    Copyright 2022 BuffaLou
    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    */

    public bool sway = true;
    public bool swayRotation = true;
    public bool bobOffset = true;
    public bool bobSway = true;

    public float controllerLookAnimationReduction = 250f;

    public Vector3 startingPositionOffset;
    public Quaternion startingRotationOffset;

    private void OnValidate()
    {
        startingPositionOffset = transform.localPosition;
        startingRotationOffset = transform.localRotation;
    }

    private GameObject characterBase;
    StarterAssetsInputs starterAssetsInputs;
    EtraCharacterMainController etraCharacterMainController;
    CharacterController charController;
    private void Start()
    {
        characterBase = GameObject.Find("EtraCharacterAssetBase");
        etraCharacterMainController = characterBase.GetComponent<EtraCharacterMainController>();
        starterAssetsInputs = characterBase.GetComponent<StarterAssetsInputs>();
        charController= characterBase.GetComponent<CharacterController>();

        startingPositionOffset = transform.localPosition;
        startingRotationOffset = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Sway();
        SwayRotation();
        BobOffset();
        BobRotation();

        CompositePositionRotation();
    }


    [Header("Basics")]
    float smooth = 10f;
    float smoothRot = 12f;

    void CompositePositionRotation()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition ,
            swayPos + bobPosition + startingPositionOffset,
            Time.deltaTime * smooth);

        transform.localRotation =
        Quaternion.Slerp(transform.localRotation,
        Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation) * startingRotationOffset,
        Time.deltaTime * smoothRot);
    }


    Vector2 procedAnimWalkInput;
    Vector2 procedAnimLookInput;
    void GetInput()
    {
        procedAnimWalkInput.x = starterAssetsInputs.move.x;
        procedAnimWalkInput.y = starterAssetsInputs.move.y;
        procedAnimWalkInput = procedAnimWalkInput.normalized; // MAYBE?

        procedAnimLookInput.x = starterAssetsInputs.look.x;
        procedAnimLookInput.y = starterAssetsInputs.look.y;

        //Reduce starterAssetsInputs.look if value is coming from controller. Controller ouputs average values around 0-300 where mouse outputs average values around 0-2
        if (Math.Abs(starterAssetsInputs.look.x) > 10 || Math.Abs(starterAssetsInputs.look.y) > 10)
        {
            procedAnimLookInput.x = starterAssetsInputs.look.x/ controllerLookAnimationReduction;
            procedAnimLookInput.y = starterAssetsInputs.look.y/ controllerLookAnimationReduction;
        }


    }

    [Header("Sway")]
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;
    public Vector3 swayPos;
    void Sway()
    {
        if(sway == false){ swayPos = Vector3.zero; return; }
        Vector3 invertLook = procedAnimLookInput * -step;
        invertLook.x *= Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y *= Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

        swayPos= invertLook;
    }

    [Header("Sway Rotation")]
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    Vector3 swayEulerRot;

    void SwayRotation()
    {
        if (swayRotation == false) { swayEulerRot = Vector3.zero; return; }

        Vector2 invertLook = procedAnimLookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);

        swayEulerRot= new Vector3(invertLook.y, invertLook.x, invertLook.x);   
    }


    [Header("Bobbing")]
    public float speedCurve;

    float curveSin { get => Mathf.Sin(speedCurve); }
    float curveCos { get => Mathf.Sin(speedCurve); }

    public Vector3 travelLimit = Vector3.one * 0.025f;
    public Vector3 bobLimit = Vector3.one * 0.01f;

    Vector3 bobPosition;
    void BobOffset()
    {
        speedCurve += Time.deltaTime * (etraCharacterMainController.Grounded ? charController.velocity.magnitude : 1f) + 0.01f;

        if (bobOffset == false) { bobPosition = Vector3.zero; return; }

        bobPosition.x =
            (curveCos * bobLimit.x * (etraCharacterMainController.Grounded ? 1 : 0))
            - (procedAnimWalkInput.x * travelLimit.x);

        bobPosition.y =
        (curveSin * bobLimit.y)
        - (charController.velocity.y * travelLimit.y);

        bobPosition.z =
        - (procedAnimWalkInput.y * travelLimit.z);

    }

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    Vector3 bobEulerRotation;
    void BobRotation()
    {
        if (bobSway == false) { bobEulerRotation = Vector3.zero; return; }

        bobEulerRotation.x = (procedAnimWalkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) :
                                multiplier.x * (Mathf.Sin(2 * speedCurve) / 2));

        bobEulerRotation.y = (procedAnimWalkInput != Vector2.zero ? multiplier.y * curveCos : 0);
        bobEulerRotation.z = (procedAnimWalkInput != Vector2.zero ? multiplier.z * curveCos * procedAnimWalkInput.x : 0);
    }



}

using StarterAssets;
using UnityEngine;
public class Treadmill_Interactable : MonoBehaviour
{
    //From JustKris#0001
    /*
    The MIT License (MIT)
    Copyright 2023 Krissy
    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    */

    //Variables
    public float treadmillForce = 1f;
    public float textureScrollSpeed = 0.5f;

    //Every frame update the treadmill texture animation
    void Update()
    {
        float offsetY = Time.time * textureScrollSpeed;
        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0, offsetY);
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.tag == "Player")
        {
            // Apply force to character
            EtraCharacterMainController.Instance.GetComponent<CharacterController>().Move(transform.forward * treadmillForce * Time.deltaTime);
        }

        if (other.GetComponent<Rigidbody>() != null)
        {   
            //Apply force to anything with a rigidbody
            other.GetComponent<Rigidbody>().position += (transform.forward * treadmillForce * Time.deltaTime);
        }
    }

}

using UnityEngine;

public class Interactable_WeightedButton : MonoBehaviour
{
    //From JustKris#0001
    /*
    The MIT License (MIT)
    Copyright 2023 Krissy
    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    */

    //References
    public SciFiDoor door;
    private Transform buttonMain;

    //Variables
    Vector3 buttonStartPos = new Vector3(0, -0.94f, 0);
    Vector3 buttonEndPos = new Vector3(0, -1.32f, 0);

    //Set buttonStartPos
    private void Start()
    {
        buttonMain = transform.GetChild(0);
        Vector3 buttonStartPos = buttonMain.transform.position;
    }

    //Check for player or object collision
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            buttonPressed();
        }
        else if (other.GetComponent<Rigidbody>() != null)
        {
            buttonPressed();
        }

    }

    private void OnTriggerExit(Collider other)
    {
            buttonReleased();
    }


    private void buttonPressed()
    {
         door.SetOpened(true);
        //Use LeanTween to play basic animations
        LeanTween.moveLocal(buttonMain.gameObject, buttonEndPos, 0.15f);
    }

    private void buttonReleased()
    {
        door.SetOpened(false);
        //Use LeanTween to play basic animations
        LeanTween.moveLocal(buttonMain.gameObject, buttonStartPos, 0.15f);
    }
}

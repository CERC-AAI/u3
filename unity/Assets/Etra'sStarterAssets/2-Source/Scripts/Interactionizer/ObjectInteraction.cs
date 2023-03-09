using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ObjectInteraction : MonoBehaviour
{
    //From @aMySour
    /*
    The MIT License (MIT)
    Copyright 2023 amysour
    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    */

    public bool isInteractable = true; // Whether the object is interactable
    public UnityEvent onPress; // The event that is called when a full successful interaction is made
    public UnityEvent onInteract; // The event that is called when the object is interacted with
    public UnityEvent onEndInteract; // The event that is called when the interaction is released
    public UnityEvent onHover; // The event that is called when the object is hovered over
    public UnityEvent onEndHover; // The event that is called when the object is no longer hovered over

    public void Interact()
    {
        // Call the event
        onInteract.Invoke();
    }

    public void SetInteractable(bool interactable)
    {
        // Set the interactable state
        isInteractable = interactable;
    }
}
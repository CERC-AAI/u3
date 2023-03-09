using UnityEngine;

public class MovingPlatformAttach : MonoBehaviour
{
    Transform savedParent;
    void OnTriggerEnter(Collider other)
    {
        // If the player enters the platform's trigger collider, make them a child of the platform
        if (other.CompareTag("Player"))
        {
            savedParent = other.transform.parent;
            other.transform.SetParent(transform);
        }

    }

    void OnTriggerExit(Collider other)
    {
        // If the player exits the platform's trigger collider, remove them from the platform's children
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(savedParent);
        }

    }


}

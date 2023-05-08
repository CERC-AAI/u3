using UnityEngine;

public class HighlightNearestObject : MonoBehaviour
{
    public LayerMask objectLayerMask;  // Only consider objects on this layer
    public float highlightDistance = 100.0f;  // Distance at which the object can be highlighted
    public Color highlightColor = Color.yellow;  // Color to use when highlighting the object
    public float lineThickness = 0.05f;  // Thickness of the glowing line
    public float lineIntensity = 1.5f;  // Intensity of the glowing line

    private Rigidbody highlightedObject;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Cast a ray from the mouse position in the direction of the camera
        Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(mouseRay, out hitInfo, highlightDistance, objectLayerMask))
        {
            // Check if the hit object has a Rigidbody component
            Rigidbody hitRigidbody = hitInfo.collider.GetComponent<Rigidbody>();
            if (hitRigidbody != null)
            {
                // If there is a highlighted object, unhighlight it
                if (highlightedObject != null && highlightedObject != hitRigidbody)
                {
                    UnhighlightObject(highlightedObject);
                }

                // Highlight the hit object
                HighlightObject(hitRigidbody);
            }
            else
            {
                // If there is a highlighted object, unhighlight it
                if (highlightedObject != null)
                {
                    UnhighlightObject(highlightedObject);
                }
            }
        }
        else
        {
            // If there is a highlighted object, unhighlight it
            if (highlightedObject != null)
            {
                UnhighlightObject(highlightedObject);
            }
        }
    }

    void HighlightObject(Rigidbody rb)
    {
        // Add a glow material to the object to highlight it
        highlightedObject = rb;
        Transform objectTransform = rb.transform;
        Renderer[] renderers = objectTransform.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            Material[] newMaterials = new Material[materials.Length + 1];
            for (int i = 0; i < materials.Length; i++)
            {
                newMaterials[i] = materials[i];
            }
            Material glowMaterial = new Material(Shader.Find("Custom/Glow"));
            glowMaterial.color = highlightColor;
            glowMaterial.SetColor("_EmissionColor", highlightColor * lineIntensity);
            newMaterials[materials.Length] = glowMaterial;
            renderer.materials = newMaterials;
        }
    }

    void UnhighlightObject(Rigidbody rb)
    {
        // Remove the glow material from the object to unhighlight it
        highlightedObject = null;
        Transform objectTransform = rb.transform;
        Renderer[] renderers = objectTransform.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            Material[] newMaterials = new Material[materials.Length - 1];
            for (int i = 0; i < materials.Length - 1; i++)
            {
                newMaterials[i] = materials[i];
            }
            renderer.materials = newMaterials;
        }
    }
}
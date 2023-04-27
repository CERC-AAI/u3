using UnityEngine;

public class MakeCube : MonoBehaviour
{
    void Reset()
    {
        // Create a new Mesh object
        Mesh cubeMesh = new Mesh();

        // Define the vertices of the cube
        Vector3[] vertices = new Vector3[8];
        vertices[0] = new Vector3(-0.5f, -0.5f, -0.5f);
        vertices[1] = new Vector3(-0.5f, 0.5f, -0.5f);
        vertices[2] = new Vector3(0.5f, 0.5f, -0.5f);
        vertices[3] = new Vector3(0.5f, -0.5f, -0.5f);
        vertices[4] = new Vector3(-0.5f, -0.5f, 0.5f);
        vertices[5] = new Vector3(-0.5f, 0.5f, 0.5f);
        vertices[6] = new Vector3(0.5f, 0.5f, 0.5f);
        vertices[7] = new Vector3(0.5f, -0.5f, 0.5f);

        // Define the triangles of the cube
        int[] triangles = new int[36];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;
        triangles[6] = 3;
        triangles[7] = 2;
        triangles[8] = 6;
        triangles[9] = 3;
        triangles[10] = 6;
        triangles[11] = 7;
        triangles[12] = 7;
        triangles[13] = 6;
        triangles[14] = 5;
        triangles[15] = 7;
        triangles[16] = 5;
        triangles[17] = 4;
        triangles[18] = 4;
        triangles[19] = 5;
        triangles[20] = 1;
        triangles[21] = 4;
        triangles[22] = 1;
        triangles[23] = 0;
        /*triangles[24] = 1;
        triangles[25] = 5;
        triangles[26] = 6;
        triangles[27] = 1;
        triangles[28] = 6;
        triangles[29] = 2;
        triangles[30] = 4;
        triangles[31] = 0;
        triangles[32] = 3;
        triangles[33] = 4;
        triangles[34] = 3;
        triangles[35] = 7;*/

        // Assign the vertices and triangles to the Mesh object
        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;

        // Assign the Mesh to a new MeshFilter component on this GameObject
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = cubeMesh;

        // Add a MeshRenderer component to this GameObject
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

        // Set the material of the MeshRenderer to the default Unity material
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }
}

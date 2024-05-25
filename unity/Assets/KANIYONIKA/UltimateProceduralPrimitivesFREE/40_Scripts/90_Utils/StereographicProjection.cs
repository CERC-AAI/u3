namespace UltimateProceduralPrimitivesFREE
{

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  public class StereographicProjection
  {
    public void ExecuteStereographicProjection(List<Vector3> ref_verts, List<Vector3> ref_origin_verts, float radius, Vector3 transform)
    {
      var error_margin = new Vector3(0.001f, 0.001f, 0.001f);  // mandated by the delaunay transform
      var origin = new Vector3(0, 0, -radius);
      var sp = Vector3.zero;

      for (int i = 0; i < ref_verts.Count; i++)
      {
        var coord = ref_origin_verts[i];
        if ((coord.x - origin.x <= error_margin.x) && (coord.y - origin.y <= error_margin.y) && (coord.z - origin.z <= error_margin.z))
        {
          var corrected_coords = new Vector3(1.0f, ((float)Random.Range(0, 1000) / 1000.0f) * 2.0f - 1.0f, -radius + error_margin.z); // introduce randomness in case there are more than one problematic points
          sp = new Vector3(Project(radius, corrected_coords.x, corrected_coords.z), Project(radius, corrected_coords.y, corrected_coords.z), -1.0f);
        }
        else
        {
          sp = new Vector3(Project(radius, coord.x, coord.z), Project(radius, coord.y, coord.z), -1.0f);
        }
        var newVertX = sp.x * (1.0f - transform.x) + transform.x * coord.x;
        var newVertY = sp.y * (1.0f - transform.y) + transform.y * coord.y;
        var newVertZ = sp.z * (1.0f - transform.z) + transform.z * coord.z;
        var newVert = new Vector3(newVertX, newVertY, newVertZ);
        ref_verts.RemoveAt(i);
        ref_verts.Insert(i, newVert);
      }
    }

    float Project(float radius, float ordinates, float z)
    {
      // given two points (0,-radius), (ordinates, z) find the line that goes trough them and get the ordinate of it's intersection with the horizontal axis
      return radius * ordinates / (z + radius);
    }
  }

}
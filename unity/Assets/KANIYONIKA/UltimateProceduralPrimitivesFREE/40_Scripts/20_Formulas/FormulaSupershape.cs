using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  public class FormulaSupershape : AbstractFormula
  {
    public MyMeshInfo CalculateVertexesAndUVs(SupershapeParameters parameter)
    {

      // j is　for latitude shape.  (Shape when cut vertically.)
      for (int j = 0; j < parameter.Segments + 1; j++)
      {
        // Debug.Log($"------- j: {j} loop ---------");
        float lat = Mathf.Lerp(-1.0f * Mathf.PI / 2.0f, Mathf.PI / 2.0f, (float)j / (float)parameter.Segments);
        float latRadian = AdditionalFormula(lat, parameter.LatShape_N1, parameter.LatShape_N2, parameter.LatShape_N3, parameter.LatShape_M, parameter.LatShape_A, parameter.LatShape_B);

        // j is　for longitude shape.  (Shape when cut horizontally.)
        myGlobeVtx.Add(new List<Vector3>());
        myGlobeUVs.Add(new List<Vector2>());
        for (int i = 0; i < parameter.Segments + 1; i++)
        {
          // Debug.Log($"------- i: {i} loop ---------");
          float lon = Mathf.Lerp(-1.0f * Mathf.PI, Mathf.PI, (float)i / (float)parameter.Segments);
          float lonRadian = AdditionalFormula(lon, parameter.LonShape_N1, parameter.LonShape_N2, parameter.LonShape_N3, parameter.LonShape_M, parameter.LonShape_A, parameter.LonShape_B);

          // Escape infinity
          if (Mathf.Abs(lonRadian) == Mathf.Infinity) { Debug.Log($"r1 has been replaced by 0 because that is {lonRadian}. => j:{j}, i:{i}"); lonRadian = 0; };
          if (Mathf.Abs(latRadian) == Mathf.Infinity) { Debug.Log($"r2 has been replaced by 0 because that is {latRadian}. => j:{j}, i:{i}"); latRadian = 0; };

          // Create Vertex
          float x = parameter.Radius * lonRadian * Mathf.Cos(lon) * latRadian * Mathf.Cos(lat);
          float y = parameter.Radius * latRadian * Mathf.Sin(lat);
          float z = parameter.Radius * lonRadian * Mathf.Sin(lon) * latRadian * Mathf.Cos(lat);

          // Create UV
          float xUV = Mathf.Lerp(0.25f, 1.25f, (float)i / (float)parameter.Segments);
          float yUV = Mathf.Lerp(0.0f, 1.0f, (float)j / (float)parameter.Segments);

          // Set the Vertex and UV
          myGlobeVtx[j].Add(new Vector3(x, y, z));
          myGlobeUVs[j].Add(new Vector2(xUV, yUV));

          // Debug.Log(
          //   $@"j: {j}, i: {i}, size_radius:{parameter.Radius.ToString("000.000")}, r1:{lonRadian.ToString("000.000")}, r2:{latRadian.ToString("000.000")}, lat:{lat.ToString("000.0000000")}, lon:{lon.ToString("000.0000000")}
          //   {globeVertex[j, i].ToString("F8")}"
          // );
        }
      }

      CreateMyVtxUVsIdx_FlexPlane_Supershape_SuperEllipsoid_TearDrop(myVtx, myUVs, myGlobeVtx, myGlobeUVs, myIdx);
      FlipNormals(parameter.FlipNormals, myVtx, myUVs, myIdx);
      DirectionCollection(parameter.Direction, myVtx);
      SetPivotPosition(parameter.PivotPosition, myVtx);
      return CreateMyMeshInfoStruct(myVtx, myUVs, myIdx);
    }


    // Additional Formula for the Shape
    float AdditionalFormula(float theta, float n1, float n2, float n3, float m, float a, float b)
    {
      float t1;
      t1 = Mathf.Abs((1.0f / a) * Mathf.Cos(m * theta / 4.0f));
      t1 = Mathf.Pow(t1, n2);

      float t2;
      t2 = Mathf.Abs((1.0f / b) * Mathf.Sin(m * theta / 4.0f));
      t2 = Mathf.Pow(t2, n3);

      float t3;
      t3 = t1 + t2;

      float _r;
      _r = Mathf.Pow(t3, -1.0f / n1);

      return _r;
    }
  }
}
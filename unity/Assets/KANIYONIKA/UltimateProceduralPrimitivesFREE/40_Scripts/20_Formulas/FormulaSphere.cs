/*
 * The following code was studied and constructed based on https://github.com/keijiro/Metamesh
 */

using System.Collections.Generic;
using UnityEngine;


namespace UltimateProceduralPrimitivesFREE
{
  public class FormulaSphere : AbstractFormula
  {
    public MyMeshInfo CalculateVertexesAndUVs(SphereParameters parameter)
    {

      float Radius = parameter.Radius;
      int Columns = parameter.Columns;
      int Rows = parameter.Rows;
      Direction Axis = parameter.Direction;

      // Parameter
      var res = new Vector2((int)parameter.Columns, (int)parameter.Rows);
      res.x = Mathf.Max(res.x, 3);
      res.y = Mathf.Max(res.y, 2);

      // Axis
      var va = Vector3.right;
      var vx = Vector3.up;
      var vy = Vector3.forward;

      // Vertex
      var vtx = new List<Vector3>();
      var nrm = new List<Vector3>();
      var uv0 = new List<Vector3>();

      for (var iy = 0; iy < res.y + 1; iy++)
      {
        for (var ix = 0; ix < res.x + 1; ix++)
        {
          var u = (float)ix / res.x;
          var v = (float)iy / res.y;

          var theta = u * Mathf.PI * 2;
          var phi = (v - 0.5f) * Mathf.PI;

          var rx = Utils.AxisAngle(-vx, theta);
          var ry = Utils.AxisAngle(vy, phi);
          var p = Utils.Mul(rx, Utils.Mul(ry, va));

          vtx.Add(p * Radius);
          nrm.Add(p);
          uv0.Add(new Vector2(u, v));
        }
      }

      // Index
      var idx = new List<int>();
      var i = 0;

      for (var iy = 0; iy < res.y; iy++, i++)
      {
        for (var ix = 0; ix < res.x; ix++, i++)
        {
          if (iy > 0)
          {
            idx.Add(i);
            idx.Add(i + (int)res.x + 1);
            idx.Add(i + 1);
          }

          if (iy < res.y - 1)
          {
            idx.Add(i + 1);
            idx.Add(i + (int)res.x + 1);
            idx.Add(i + (int)res.x + 2);
          }
        }
      }


      // Create myVtx
      foreach (var item in vtx)
        myVtx.Add(item);
      // Create myUVs
      foreach (var item in uv0)
        myUVs.Add(new Vector2(item.x - 0.25f, item.y));
      // Create myIdx
      foreach (var item in idx)
        myIdx.Add(item);


      FlipNormals(parameter.FlipNormals, myVtx, myUVs, myIdx);  // Collect FlipNormals here
      DirectionCollection(parameter.Direction, myVtx);  // Collect Direction here
      SetPivotPosition(parameter.PivotPosition, myVtx);  // Collect Pivot here
      return CreateMyMeshInfoStruct(myVtx, myUVs, myIdx);
    }
  }
}
/*
 * The following code was studied and constructed based on https://github.com/keijiro/Metamesh
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UltimateProceduralPrimitivesFREE
{
  public class FormulaCone : AbstractFormula
  {
    public MyMeshInfo CalculateVertexesAndUVs(ConeParameters parameter)
    {
      // // Parameter sanitization
      var res = new Vector2((int)parameter.Columns, (int)parameter.Rows);
      res.x = Mathf.Max(res.x, 3);
      res.y = Mathf.Max(res.y, 1);

      // Axis selection
      var va = Vector3.up;
      var vx = Vector3.forward;

      // Normal vector for the first vertex
      var edge = (parameter.TopRadius - parameter.BottomRadius) * vx + parameter.Height * va;
      var n0 = Vector3.Normalize(Vector3.Cross(Vector3.Cross(va, vx), edge));

      // Vertex array
      var vtx = new List<Vector3>();
      var uv0 = new List<Vector2>();

      // (Body vertices)
      for (var iy = 0; iy < res.y + 1; iy++)
      {
        for (var ix = 0; ix < res.x + 1; ix++)
        {
          var u = (float)ix / res.x;
          var v = (float)iy / res.y;

          var r = Mathf.Lerp(parameter.BottomRadius, parameter.TopRadius, v);
          var rot = Utils.AxisAngle(va, u * Mathf.PI * -2.0f);
          var n = Utils.Mul(rot, n0);
          var p = Utils.Mul(rot, vx) * r + va * (v - 0.5f) * parameter.Height;

          vtx.Add((Vector3)(p));
          uv0.Add(new Vector2(u, v));
        }
      }

      // (End cap vertices)
      if (parameter.Caps)
      {
        vtx.Add(va * parameter.Height / -2.0f);
        vtx.Add(va * parameter.Height / +2.0f);

        uv0.Add(new Vector2(0.5f, 0.5f));
        uv0.Add(new Vector2(0.5f, 0.5f));

        for (var ix = 0; ix < res.x; ix++)
        {
          var u = (float)ix / res.x * Mathf.PI * 2.0f;

          var rot = Utils.AxisAngle(va, -u);
          var p = Utils.Mul(rot, vx);

          vtx.Add((Vector3)(p * parameter.BottomRadius + va * parameter.Height / -2.0f));
          vtx.Add((Vector3)(p * parameter.TopRadius + va * parameter.Height / +2.0f));

          uv0.Add(new Vector2(Mathf.Cos(-u) / 2.0f + 0.5f, Mathf.Sin(-u) / 2.0f + 0.5f));
          uv0.Add(new Vector2(Mathf.Cos(+u) / 2.0f + 0.5f, Mathf.Sin(+u) / 2.0f + 0.5f));
        }
      }

      // Index array
      var idx = new List<int>();
      var i = 0;

      // (Body indices)
      for (var iy = 0; iy < res.y; iy++, i++)
      {
        for (var ix = 0; ix < res.x; ix++, i++)
        {
          idx.Add(i);
          idx.Add(i + (int)res.x + 1);
          idx.Add(i + 1);

          idx.Add(i + 1);
          idx.Add(i + (int)res.x + 1);
          idx.Add(i + (int)res.x + 2);
        }
      }

      // (End cap indices)
      if (parameter.Caps)
      {
        i += (int)res.x + 1;

        for (var ix = 0; ix < (res.x - 1) * 2; ix += 2)
        {
          idx.Add(i);
          idx.Add(i + ix + 2);
          idx.Add(i + ix + 4);

          idx.Add(i + 1);
          idx.Add(i + ix + 5);
          idx.Add(i + ix + 3);
        }

        idx.Add(i);
        idx.Add(i + (int)res.x * 2);
        idx.Add(i + 2);

        idx.Add(i + 1);
        idx.Add(i + 3);
        idx.Add(i + 1 + (int)res.x * 2);
      }


      // Create myVtx
      foreach (var item in vtx)
        myVtx.Add(item);
      // Create myUVs
      foreach (var item in uv0)
        myUVs.Add(new Vector2(item.x, item.y));
      // Create myIdx
      foreach (var item in idx)
        myIdx.Add(item);

      // Debug.Log($"vertexes.Count: {vertexes.Count}");
      // Debug.Log($"uvs.Count: {uvs.Count}");
      // Debug.Log($"idx.Count: {idx.Count}");


      FlipNormals(parameter.FlipNormals, myVtx, myUVs, myIdx);
      DirectionCollection(parameter.Direction, myVtx);
      SetPivotPosition(parameter.PivotPosition, myVtx);
      return CreateMyMeshInfoStruct(myVtx, myUVs, myIdx);
    }
  }
}